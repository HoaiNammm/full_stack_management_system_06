<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { Calendar, Users, ListChecks, Activity, MoreVertical, Pencil, Trash2 } from 'lucide-vue-next'
import { format } from 'date-fns'
import { parseUtc } from '../utils/date'
import { StatusBadge, PriorityBadge } from '@/components/base'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { activityLogApi } from '../api/tasks'
import { projectApi } from '../api/projects'

const props = defineProps({ project: Object })

const router = useRouter()
const toast  = useToast()
const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()

const menuOpen      = ref(false)
const menuRef       = ref(null)
const deleting      = ref(false)
const activityCount = ref(null)
const members       = ref(props.project.members || null)

const taskCount = computed(() => taskStore.getProjectTasks(props.project.id).length)
const visibleMembers = computed(() => (members.value || []).slice(0, 4))
const extraMemberCount = computed(() => Math.max((members.value?.length ?? props.project.memberCount ?? 0) - 4, 0))

const AVATAR_PALETTE = ['#ef4444','#f97316','#eab308','#22c55e','#06b6d4','#6366f1','#a855f7','#ec4899','#14b8a6','#f43f5e']
function avatarColor(name = '') {
  let h = 0
  for (const c of name) h = c.charCodeAt(0) + h * 31
  return AVATAR_PALETTE[Math.abs(h) % AVATAR_PALETTE.length]
}
function memberLabel(m) {
  return m.user?.name || m.user?.email || '?'
}

const deadlineLabel = computed(() => {
  const d = parseUtc(props.project.endDate)
  return d ? format(d, 'MMM d, yyyy') : 'No deadline'
})
const startDateLabel = computed(() => {
  const d = parseUtc(props.project.startDate ?? props.project.createdAt)
  return d ? format(d, 'MMM d, yyyy') : '—'
})

function handleClickOutside(e) {
  if (menuRef.value && !menuRef.value.contains(e.target)) menuOpen.value = false
}

onMounted(async () => {
  document.addEventListener('mousedown', handleClickOutside)
  if (!taskStore.getProjectTasks(props.project.id).length) taskStore.fetchTasks(props.project.id)
  try {
    const logs = await activityLogApi.getProject(props.project.id)
    activityCount.value = (logs || []).length
  } catch {
    activityCount.value = null
  }
  if (!members.value) {
    try {
      members.value = await projectApi.getMembers(workspaceStore.currentWorkspaceId, props.project.id) || []
    } catch {
      members.value = []
    }
  }
})
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside))

function goToEdit() {
  menuOpen.value = false
  router.push(`/projectsDetail?id=${props.project.id}&tab=settings`)
}

async function handleDelete() {
  menuOpen.value = false
  if (!confirm(`Delete project "${props.project.name}"? This cannot be undone.`)) return
  deleting.value = true
  try {
    await workspaceStore.deleteProject(workspaceStore.currentWorkspaceId, props.project.id)
    toast.success('Project deleted')
  } catch (err) {
    toast.error(err?.response?.data?.message || err?.response?.data?.error || 'Failed to delete project')
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <div class="card relative p-4">
    <!-- Color strip -->
    <div
      class="absolute inset-x-0 top-0 h-1.5 rounded-t-xl"
      :style="{ background: project.color || '#3b82f6' }"
    />

    <!-- More menu -->
    <div ref="menuRef" class="absolute right-3 top-3 z-10">
      <button
        type="button"
        @click.stop.prevent="menuOpen = !menuOpen"
        aria-label="Project actions"
        :aria-expanded="menuOpen"
        class="flex size-7 items-center justify-center rounded-lg transition-colors hover:bg-[var(--bg-subtle)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-black"
        style="color: var(--text-muted);"
      >
        <MoreVertical class="size-4" aria-hidden="true" />
      </button>
      <div
        v-if="menuOpen"
        class="absolute right-0 top-full mt-1 w-36 overflow-hidden rounded-lg"
        style="background: var(--bg-surface); border: 1px solid var(--border-light); box-shadow: 0 8px 24px rgba(0,0,0,0.1), 0 0 0 1px rgba(0,0,0,0.04);"
        role="menu"
      >
        <button
          type="button"
          @click.stop.prevent="goToEdit"
          class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm transition-colors hover:bg-[var(--bg-subtle)]"
          style="color: var(--text-primary);"
          role="menuitem"
        >
          <Pencil class="size-3.5 flex-shrink-0" aria-hidden="true" /> Edit
        </button>
        <button
          type="button"
          :disabled="deleting"
          @click.stop.prevent="handleDelete"
          class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-red-600 transition-colors hover:bg-red-50 disabled:opacity-50"
          role="menuitem"
        >
          <Trash2 class="size-3.5 flex-shrink-0" aria-hidden="true" /> Delete
        </button>
      </div>
    </div>

    <router-link
      :to="`/projectsDetail?id=${project.id}&tab=tasks`"
      class="block focus-visible:outline-none"
      style="text-decoration: none;"
    >
      <!-- Block: Name + status + description -->
      <div class="mb-3 pr-7">
        <div class="mb-2 flex items-start justify-between gap-2">
          <h3 class="flex min-w-0 items-center gap-1.5 truncate font-semibold" style="color: var(--text-primary);">
            <span class="size-2 flex-shrink-0 rounded-full" :style="{ background: project.color || '#3b82f6' }" />
            <span class="truncate">{{ project.name }}</span>
          </h3>
          <StatusBadge :status="project.status" dot size="xs" class="flex-shrink-0" />
        </div>
        <p class="line-clamp-2 min-h-10 text-sm" style="color: var(--text-secondary);">
          {{ project.description || 'No description' }}
        </p>
      </div>

      <!-- Block: Deadline + Progress -->
      <div class="mb-3 space-y-1.5 pt-3" style="border-top: 1px solid var(--border-light);">
        <div class="flex items-center gap-1.5 text-xs" style="color: var(--text-muted);">
          <Calendar class="size-3.5 flex-shrink-0" aria-hidden="true" />
          <span>Deadline: {{ deadlineLabel }}</span>
        </div>
        <div class="h-1.5 w-full overflow-hidden rounded-full" style="background: var(--bg-subtle);">
          <div
            role="progressbar"
            :aria-valuenow="project.progress || 0"
            aria-valuemin="0"
            aria-valuemax="100"
            :aria-label="`${project.name} progress: ${project.progress || 0}%`"
            class="h-full rounded-full bg-black transition-[width]"
            :style="{ width: `${project.progress || 0}%` }"
          />
        </div>
        <p class="text-right text-xs font-medium" style="color: var(--text-secondary);">{{ project.progress || 0 }}%</p>
      </div>

      <!-- Block: Members / Tasks / Activities -->
      <div class="mb-3 flex flex-wrap items-center gap-3 pt-3 text-xs" style="border-top: 1px solid var(--border-light); color: var(--text-muted);">
        <div v-if="visibleMembers.length" class="flex items-center -space-x-1.5">
          <template v-for="m in visibleMembers" :key="m.id">
            <img
              v-if="m.user?.avatarUrl"
              :src="m.user.avatarUrl"
              :alt="memberLabel(m)"
              :title="memberLabel(m)"
              class="size-6 rounded-full object-cover"
              style="border: 2px solid var(--bg-surface);"
            />
            <div
              v-else
              :title="memberLabel(m)"
              class="flex size-6 items-center justify-center rounded-full text-[9px] font-semibold text-white"
              :style="{ background: avatarColor(memberLabel(m)), border: '2px solid var(--bg-surface)' }"
            >
              {{ memberLabel(m)?.[0]?.toUpperCase() }}
            </div>
          </template>
          <div
            v-if="extraMemberCount > 0"
            :title="`${extraMemberCount} more member${extraMemberCount === 1 ? '' : 's'}`"
            class="flex size-6 items-center justify-center rounded-full text-[10px] font-semibold"
            style="background: var(--bg-subtle); color: var(--text-muted); border: 2px solid var(--bg-surface);"
          >
            …
          </div>
        </div>
        <span v-else class="flex items-center gap-1">
          <Users class="size-3.5 flex-shrink-0" aria-hidden="true" />
          {{ project.memberCount ?? 0 }} member{{ project.memberCount === 1 ? '' : 's' }}
        </span>
        <span class="flex items-center gap-1">
          <ListChecks class="size-3.5 flex-shrink-0" aria-hidden="true" />
          {{ taskCount }} task{{ taskCount === 1 ? '' : 's' }}
        </span>
        <span v-if="activityCount !== null" class="flex items-center gap-1">
          <Activity class="size-3.5 flex-shrink-0" aria-hidden="true" />
          {{ activityCount }} activit{{ activityCount === 1 ? 'y' : 'ies' }}
        </span>
      </div>

      <!-- Block: Start date / Priority -->
      <div class="grid grid-cols-2 gap-2 pt-3" style="border-top: 1px solid var(--border-light);">
        <div>
          <p class="text-[11px]" style="color: var(--text-muted);">Start Date</p>
          <p class="text-xs font-medium" style="color: var(--text-primary);">{{ startDateLabel }}</p>
        </div>
        <div>
          <p class="mb-0.5 text-[11px]" style="color: var(--text-muted);">Priority</p>
          <PriorityBadge :priority="project.priority" />
        </div>
      </div>
    </router-link>
  </div>
</template>
