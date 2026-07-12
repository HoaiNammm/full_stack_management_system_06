<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { format } from 'date-fns'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { workspaceApi } from '../api/workspaces'
import { parseUtc } from '../utils/date'
import { UsersIcon, Search, UserPlus, Shield, Activity, MoreVertical, UserCog, Trash2, ChevronLeft, Check } from 'lucide-vue-next'
import InviteMemberDialog from '../components/InviteMemberDialog.vue'
import { BaseButton, EmptyState } from '@/components/base'

const ROLES = ['Owner', 'Member', 'Viewer']

const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()
const isDialogOpen   = ref(false)
const searchTerm     = ref('')
const members        = ref([])
const loadingMembers = ref(false)

async function fetchMembers() {
  if (!workspaceStore.currentWorkspaceId) return
  loadingMembers.value = true
  try {
    members.value = await workspaceApi.getMembers(workspaceStore.currentWorkspaceId)
  } catch {
    members.value = []
  } finally {
    loadingMembers.value = false
  }
}

onMounted(fetchMembers)
watch(() => workspaceStore.currentWorkspaceId, fetchMembers)

function userName(m) {
  const u = m?.user
  if (!u) return 'Unknown User'
  return u.name || u.email || 'Unknown User'
}

function memberKey(m) { return m.id || m.userId }

function formatJoined(m) {
  const d = m.joinedAt || m.createdAt
  if (!d) return '—'
  try { return format(parseUtc(d), 'dd MMM yyyy') } catch { return '—' }
}

// ── Row action menu (edit role / remove) ──────────────────────────────────
const openMenuKey   = ref(null)
const menuStage     = ref('root') // 'root' | 'role'
const updatingMember = ref(null)
const removingMember = ref(null)
const memberErrors   = ref({})

function toggleMenu(key) {
  openMenuKey.value = openMenuKey.value === key ? null : key
  menuStage.value = 'root'
}

function closeMenu() {
  openMenuKey.value = null
  menuStage.value = 'root'
}

function handleClickOutside(e) {
  if (!e.target.closest('[data-member-menu]')) closeMenu()
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside))

async function handleUpdateRole(member, newRole) {
  const key = memberKey(member)
  if (newRole === member.role) { closeMenu(); return }
  updatingMember.value = key
  memberErrors.value[key] = ''
  try {
    await workspaceApi.updateMember(workspaceStore.currentWorkspaceId, key, { role: newRole })
    member.role = newRole
    closeMenu()
  } catch (err) {
    memberErrors.value[key] = err?.response?.data?.message || err?.response?.data?.error || 'Failed to update role'
  } finally {
    updatingMember.value = null
  }
}

async function handleRemoveMember(member) {
  const key = memberKey(member)
  const label = userName(member)
  if (!confirm(`Remove ${label} from the workspace?`)) return
  removingMember.value = key
  memberErrors.value[key] = ''
  try {
    await workspaceApi.removeMember(workspaceStore.currentWorkspaceId, key)
    members.value = members.value.filter(m => memberKey(m) !== key)
    closeMenu()
  } catch (err) {
    memberErrors.value[key] = err?.response?.data?.message || err?.response?.data?.error || 'Failed to remove member'
  } finally {
    removingMember.value = null
  }
}

const filteredMembers = computed(() =>
  members.value.filter(m => {
    const n = userName(m).toLowerCase()
    const e = m?.user?.email?.toLowerCase() || ''
    const q = searchTerm.value.toLowerCase()
    return n.includes(q) || e.includes(q)
  })
)

const projects      = computed(() => workspaceStore.projects)
const activeProjects = computed(() =>
  projects.value.filter(p => p.status !== 'Cancelled' && p.status !== 'Completed').length
)
const totalTasks = computed(() => taskStore.allTasks.length)
</script>

<template>
  <div class="mx-auto max-w-6xl space-y-6 text-zinc-900 dark:text-zinc-100">

    <!-- Header -->
    <div class="page-header-banner flex flex-col items-start justify-between gap-4 lg:flex-row lg:items-center">
      <div>
        <h1 class="text-xl font-semibold tracking-tight text-white sm:text-2xl">Team</h1>
        <p class="mt-0.5 text-sm text-white/80">Manage team members and their contributions</p>
      </div>
      <BaseButton variant="primary" size="sm" @click="isDialogOpen = true">
        <UserPlus class="size-4" aria-hidden="true" /> Invite Member
      </BaseButton>
    </div>

    <!-- Stat cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div
        v-for="card in [
          { label: 'Total Members',   value: members.length,  icon: UsersIcon, iconBg: 'bg-blue-100 dark:bg-blue-500/10',    iconColor: 'text-blue-500 dark:text-blue-300' },
          { label: 'Active Projects', value: activeProjects,  icon: Activity,  iconBg: 'bg-emerald-100 dark:bg-emerald-500/10', iconColor: 'text-emerald-500 dark:text-emerald-300' },
          { label: 'Total Tasks',     value: totalTasks,      icon: Shield,    iconBg: 'bg-purple-100 dark:bg-purple-500/10',   iconColor: 'text-purple-500 dark:text-purple-300' },
        ]"
        :key="card.label"
        class="rounded-lg border border-zinc-200 bg-white p-5 dark:border-zinc-800 dark:bg-zinc-900"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-zinc-500 dark:text-zinc-400">{{ card.label }}</p>
            <p class="mt-0.5 text-xl font-bold tabular-nums text-zinc-900 dark:text-zinc-100">{{ card.value }}</p>
          </div>
          <div :class="['rounded-xl p-3', card.iconBg]">
            <component :is="card.icon" :class="['size-4', card.iconColor]" aria-hidden="true" />
          </div>
        </div>
      </div>
    </div>

    <!-- Search -->
    <div class="relative max-w-xs">
      <Search class="pointer-events-none absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-zinc-400" aria-hidden="true" />
      <input
        v-model="searchTerm"
        type="text"
        placeholder="Search team members…"
        aria-label="Search team members"
        class="w-full rounded-lg border border-zinc-300 bg-white py-2 pl-9 pr-4 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-100 dark:placeholder:text-zinc-500"
      />
    </div>

    <!-- Loading -->
    <div v-if="loadingMembers" class="py-8 text-center text-sm text-zinc-400 dark:text-zinc-500">
      Loading members…
    </div>

    <!-- Empty state -->
    <EmptyState
      v-else-if="filteredMembers.length === 0"
      :icon="UsersIcon"
      :title="members.length === 0 ? 'No team members yet' : 'No members match your search'"
      :description="members.length === 0 ? 'Invite team members to start collaborating.' : 'Try adjusting your search term.'"
      :action-label="members.length === 0 ? 'Invite Member' : undefined"
      size="lg"
      @action="isDialogOpen = true"
    />

    <!-- Member list -->
    <div v-else class="max-w-5xl">

      <!-- Desktop table -->
      <div class="hidden overflow-visible rounded-lg border border-zinc-200 dark:border-zinc-800 sm:block">
        <table class="min-w-full text-left text-sm">
          <thead class="bg-zinc-50 text-xs uppercase text-zinc-500 dark:bg-zinc-800/70 dark:text-zinc-400">
            <tr>
              <th class="rounded-tl-lg px-5 py-3 font-medium">Name</th>
              <th class="px-5 py-3 font-medium">Email</th>
              <th class="px-5 py-3 font-medium">Role</th>
              <th class="px-5 py-3 font-medium">Status</th>
              <th class="px-5 py-3 font-medium">Joined</th>
              <th class="rounded-tr-lg px-5 py-3 font-medium"><span class="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-100 bg-white dark:divide-zinc-800 dark:bg-zinc-900">
            <tr
              v-for="m in filteredMembers"
              :key="memberKey(m)"
              class="transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-800/50"
            >
              <td class="px-5 py-3">
                <div class="flex items-center gap-3">
                  <img
                    v-if="m.user?.avatarUrl"
                    :src="m.user.avatarUrl"
                    :alt="userName(m)"
                    class="size-7 rounded-full object-cover"
                  />
                  <div
                    v-else
                    class="flex size-7 flex-shrink-0 items-center justify-center rounded-full bg-blue-500 text-xs font-semibold text-white"
                    aria-hidden="true"
                  >
                    {{ userName(m)?.[0]?.toUpperCase() }}
                  </div>
                  <span class="truncate font-medium text-zinc-800 dark:text-zinc-200">{{ userName(m) }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-zinc-500 dark:text-zinc-400">{{ m.user?.email }}</td>
              <td class="px-5 py-3">
                <span :class="[
                  'rounded px-2 py-1 text-xs font-medium',
                  m.role?.toLowerCase() === 'owner' || m.role?.toLowerCase() === 'manager'
                    ? 'bg-purple-100 text-purple-700 dark:bg-purple-500/20 dark:text-purple-400'
                    : 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400',
                ]">
                  {{ m.role || 'Member' }}
                </span>
              </td>
              <td class="px-5 py-3">
                <span class="inline-flex items-center gap-1.5 rounded px-2 py-1 text-xs font-medium bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-400">
                  <span class="size-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
                  Active
                </span>
              </td>
              <td class="px-5 py-3 whitespace-nowrap text-zinc-500 dark:text-zinc-400">{{ formatJoined(m) }}</td>
              <td class="relative px-5 py-3 text-right" data-member-menu>
                <button
                  type="button"
                  @click="toggleMenu(memberKey(m))"
                  :aria-expanded="openMenuKey === memberKey(m)"
                  aria-label="Member actions"
                  class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
                >
                  <MoreVertical class="size-4" aria-hidden="true" />
                </button>

                <!-- Action menu -->
                <div
                  v-if="openMenuKey === memberKey(m)"
                  class="absolute right-5 top-full z-20 mt-1 w-48 overflow-hidden rounded-lg border border-zinc-200 bg-white text-left shadow-lg dark:border-zinc-800 dark:bg-zinc-900"
                  role="menu"
                >
                  <template v-if="menuStage === 'root'">
                    <button
                      type="button"
                      @click="menuStage = 'role'"
                      :disabled="m.role === 'Owner'"
                      role="menuitem"
                      class="flex w-full items-center gap-2.5 px-3 py-2 text-sm text-zinc-700 transition-colors hover:bg-zinc-50 disabled:cursor-not-allowed disabled:opacity-40 dark:text-zinc-300 dark:hover:bg-zinc-800"
                    >
                      <UserCog class="size-4" aria-hidden="true" /> Edit role
                    </button>
                    <button
                      type="button"
                      @click="handleRemoveMember(m)"
                      :disabled="m.role === 'Owner' || removingMember === memberKey(m)"
                      role="menuitem"
                      class="flex w-full items-center gap-2.5 px-3 py-2 text-sm text-red-500 transition-colors hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40 dark:hover:bg-red-950/30"
                    >
                      <Trash2 class="size-4" aria-hidden="true" /> Remove member
                    </button>
                  </template>
                  <template v-else>
                    <button
                      type="button"
                      @click="menuStage = 'root'"
                      class="flex w-full items-center gap-1.5 px-3 py-2 text-xs font-medium text-zinc-400 transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-800"
                    >
                      <ChevronLeft class="size-3.5" aria-hidden="true" /> Back
                    </button>
                    <div class="h-px bg-zinc-100 dark:bg-zinc-800" />
                    <button
                      v-for="role in ROLES"
                      :key="role"
                      type="button"
                      @click="handleUpdateRole(m, role)"
                      :disabled="updatingMember === memberKey(m)"
                      role="menuitem"
                      class="flex w-full items-center justify-between px-3 py-2 text-sm text-zinc-700 transition-colors hover:bg-zinc-50 disabled:opacity-50 dark:text-zinc-300 dark:hover:bg-zinc-800"
                    >
                      {{ role }}
                      <Check v-if="m.role === role" class="size-3.5 text-blue-500" aria-hidden="true" />
                    </button>
                  </template>
                  <p v-if="memberErrors[memberKey(m)]" class="px-3 pb-2 pt-1 text-xs text-red-500">
                    {{ memberErrors[memberKey(m)] }}
                  </p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile cards -->
      <div class="flex flex-col gap-3 sm:hidden">
        <div
          v-for="m in filteredMembers"
          :key="memberKey(m)"
          class="relative rounded-lg border border-zinc-200 bg-white p-4 dark:border-zinc-800 dark:bg-zinc-900"
          data-member-menu
        >
          <div class="mb-3 flex items-start gap-3">
            <img
              v-if="m.user?.avatarUrl"
              :src="m.user.avatarUrl"
              :alt="userName(m)"
              class="size-9 rounded-full object-cover"
            />
            <div
              v-else
              class="flex size-9 flex-shrink-0 items-center justify-center rounded-full bg-blue-500 text-sm font-semibold text-white"
              aria-hidden="true"
            >
              {{ userName(m)?.[0]?.toUpperCase() }}
            </div>
            <div class="min-w-0 flex-1">
              <p class="truncate font-medium text-zinc-900 dark:text-zinc-100">{{ userName(m) }}</p>
              <p class="truncate text-sm text-zinc-500 dark:text-zinc-400">{{ m.user?.email }}</p>
            </div>
            <button
              type="button"
              @click="toggleMenu(memberKey(m))"
              :aria-expanded="openMenuKey === memberKey(m)"
              aria-label="Member actions"
              class="flex-shrink-0 rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
            >
              <MoreVertical class="size-4" aria-hidden="true" />
            </button>
          </div>

          <div class="flex flex-wrap items-center gap-2">
            <span :class="[
              'rounded px-2 py-1 text-xs font-medium',
              m.role?.toLowerCase() === 'owner' || m.role?.toLowerCase() === 'manager'
                ? 'bg-purple-100 text-purple-700 dark:bg-purple-500/20 dark:text-purple-400'
                : 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400',
            ]">
              {{ m.role || 'Member' }}
            </span>
            <span class="inline-flex items-center gap-1.5 rounded px-2 py-1 text-xs font-medium bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-400">
              <span class="size-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
              Active
            </span>
            <span class="text-xs text-zinc-400 dark:text-zinc-500">Joined {{ formatJoined(m) }}</span>
          </div>

          <!-- Action menu -->
          <div
            v-if="openMenuKey === memberKey(m)"
            class="absolute right-4 top-14 z-20 w-48 overflow-hidden rounded-lg border border-zinc-200 bg-white text-left shadow-lg dark:border-zinc-800 dark:bg-zinc-900"
            role="menu"
          >
            <template v-if="menuStage === 'root'">
              <button
                type="button"
                @click="menuStage = 'role'"
                :disabled="m.role === 'Owner'"
                role="menuitem"
                class="flex w-full items-center gap-2.5 px-3 py-2 text-sm text-zinc-700 transition-colors hover:bg-zinc-50 disabled:cursor-not-allowed disabled:opacity-40 dark:text-zinc-300 dark:hover:bg-zinc-800"
              >
                <UserCog class="size-4" aria-hidden="true" /> Edit role
              </button>
              <button
                type="button"
                @click="handleRemoveMember(m)"
                :disabled="m.role === 'Owner' || removingMember === memberKey(m)"
                role="menuitem"
                class="flex w-full items-center gap-2.5 px-3 py-2 text-sm text-red-500 transition-colors hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40 dark:hover:bg-red-950/30"
              >
                <Trash2 class="size-4" aria-hidden="true" /> Remove member
              </button>
            </template>
            <template v-else>
              <button
                type="button"
                @click="menuStage = 'root'"
                class="flex w-full items-center gap-1.5 px-3 py-2 text-xs font-medium text-zinc-400 transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-800"
              >
                <ChevronLeft class="size-3.5" aria-hidden="true" /> Back
              </button>
              <div class="h-px bg-zinc-100 dark:bg-zinc-800" />
              <button
                v-for="role in ROLES"
                :key="role"
                type="button"
                @click="handleUpdateRole(m, role)"
                :disabled="updatingMember === memberKey(m)"
                role="menuitem"
                class="flex w-full items-center justify-between px-3 py-2 text-sm text-zinc-700 transition-colors hover:bg-zinc-50 disabled:opacity-50 dark:text-zinc-300 dark:hover:bg-zinc-800"
              >
                {{ role }}
                <Check v-if="m.role === role" class="size-3.5 text-blue-500" aria-hidden="true" />
              </button>
            </template>
            <p v-if="memberErrors[memberKey(m)]" class="px-3 pb-2 pt-1 text-xs text-red-500">
              {{ memberErrors[memberKey(m)] }}
            </p>
          </div>
        </div>
      </div>
    </div>

  </div>

  <InviteMemberDialog :isOpen="isDialogOpen" @close="isDialogOpen = false; fetchMembers()" />
</template>
