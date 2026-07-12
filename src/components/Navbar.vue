<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { Search, PanelLeft, Moon, Sun, Bell, X, LogOut, User } from 'lucide-vue-next'
import { useThemeStore } from '../stores/themeStore'
import { useNotificationStore } from '../stores/notificationStore'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { useAuthStore } from '../stores/authStore'

defineProps({ isSidebarOpen: Boolean })
const emit = defineEmits(['update:isSidebarOpen'])

const themeStore        = useThemeStore()
const notificationStore = useNotificationStore()
const workspaceStore    = useWorkspaceStore()
const taskStore         = useTaskStore()
const authStore         = useAuthStore()
const router            = useRouter()

const searchQuery  = ref('')
const showResults  = ref(false)
const searchRef    = ref(null)
const showUserMenu = ref(false)
const userMenuRef  = ref(null)

const filteredResults = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return { projects: [], tasks: [] }
  return {
    projects: workspaceStore.projects.filter(p =>
      p.name?.toLowerCase().includes(q) || p.description?.toLowerCase().includes(q)
    ).slice(0, 5),
    tasks: taskStore.allTasks.filter(t =>
      t.title?.toLowerCase().includes(q) || t.description?.toLowerCase().includes(q)
    ).slice(0, 5),
  }
})

const hasResults = computed(() =>
  filteredResults.value.projects.length + filteredResults.value.tasks.length > 0
)

function onInput() { showResults.value = !!searchQuery.value.trim() }
function clearSearch() { searchQuery.value = ''; showResults.value = false }
function goToProject(p) { clearSearch(); router.push(`/projectsDetail?id=${p.id}`) }
function goToTask(t) { clearSearch(); router.push(`/taskDetails?projectId=${t.projectId}&taskId=${t.id}`) }

function handleClickOutside(e) {
  if (searchRef.value && !searchRef.value.contains(e.target)) showResults.value = false
  if (userMenuRef.value && !userMenuRef.value.contains(e.target)) showUserMenu.value = false
}

async function handleLogout() {
  showUserMenu.value = false
  await authStore.logout()
  router.push('/login')
}

const user = computed(() => authStore.user)

const STATUS_COLORS = {
  Backlog: '#94a3b8', ToDo: '#6366f1', InProgress: '#f59e0b',
  Review: '#a855f7', Done: '#10b981', Blocked: '#ef4444',
}

let _timer = null
onMounted(() => {
  notificationStore.fetchUnreadCount()
  _timer = setInterval(() => notificationStore.fetchUnreadCount(), 60000)
  document.addEventListener('mousedown', handleClickOutside)
})
onBeforeUnmount(() => {
  clearInterval(_timer)
  document.removeEventListener('mousedown', handleClickOutside)
})
</script>

<template>
  <header
    class="flex-shrink-0 px-5 py-2.5"
    style="
      background: var(--bg-surface);
      border-bottom: 1px solid var(--border-light);
      position: relative; z-index: 10;
    "
  >
    <div class="flex items-center gap-4">

      <!-- Mobile sidebar toggle -->
      <button
        @click="emit('update:isSidebarOpen', !isSidebarOpen)"
        :aria-label="isSidebarOpen ? 'Close sidebar' : 'Open sidebar'"
        class="flex-shrink-0 rounded-lg p-2 transition-colors hover:bg-[var(--bg-sidebar-hover)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--brand)] sm:hidden"
        style="color: var(--brand);"
      >
        <PanelLeft class="size-5" aria-hidden="true" />
      </button>

      <!-- Search -->
      <div ref="searchRef" class="relative flex-1 max-w-xs">
        <Search
          class="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2"
          style="color: var(--brand);"
          aria-hidden="true"
        />
        <input
          v-model="searchQuery"
          @input="onInput"
          @focus="showResults = !!searchQuery.trim()"
          type="text"
          placeholder="Search…"
          aria-label="Search projects and tasks"
          class="w-full rounded-xl py-2 pl-9 pr-8 text-sm transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--brand)]"
          style="
            background: var(--bg-subtle);
            border: 1px solid var(--border);
            color: #000000;
          "
        />
        <button
          v-if="searchQuery"
          type="button"
          @click="clearSearch"
          aria-label="Clear search"
          class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded p-0.5 transition-colors focus-visible:outline-none"
          style="color: var(--text-muted);"
        >
          <X class="size-3.5" aria-hidden="true" />
        </button>

        <!-- Search results -->
        <div
          v-if="showResults"
          class="absolute left-0 top-full z-50 mt-2 max-h-72 w-72 overflow-y-auto rounded-xl"
          style="
            background: var(--bg-surface);
            border: 1px solid var(--border-light);
            box-shadow: 0 8px 24px rgba(0,0,0,0.1), 0 0 0 1px rgba(0,0,0,0.04);
          "
          role="listbox"
          aria-label="Search results"
        >
          <template v-if="hasResults">
            <div v-if="filteredResults.projects.length">
              <p class="px-3 pt-3 pb-1 text-[10px] font-semibold uppercase tracking-wider" style="color: var(--text-muted);">
                Projects
              </p>
              <button
                v-for="p in filteredResults.projects"
                :key="p.id"
                @click="goToProject(p)"
                class="flex w-full items-center gap-2.5 px-3 py-2 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 focus-visible:outline-none"
                role="option"
              >
                <span class="flex size-6 flex-shrink-0 items-center justify-center rounded-lg bg-blue-500/10 text-xs font-bold text-blue-500">
                  {{ p.name?.[0]?.toUpperCase() }}
                </span>
                <span class="truncate text-sm" style="color: var(--text-primary);">{{ p.name }}</span>
                <span v-if="p.status" class="ml-auto flex-shrink-0 text-[10px]" style="color: var(--text-muted);">{{ p.status }}</span>
              </button>
            </div>
            <div v-if="filteredResults.tasks.length">
              <p class="px-3 pt-3 pb-1 text-[10px] font-semibold uppercase tracking-wider" style="color: var(--text-muted);">
                Tasks
              </p>
              <button
                v-for="t in filteredResults.tasks"
                :key="t.id"
                @click="goToTask(t)"
                class="flex w-full items-center gap-2.5 px-3 py-2 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 focus-visible:outline-none"
                role="option"
              >
                <span
                  class="size-2 flex-shrink-0 rounded-full"
                  :style="{ background: STATUS_COLORS[t.status] || '#94a3b8' }"
                />
                <span class="truncate text-sm" style="color: var(--text-primary);">{{ t.title }}</span>
                <span class="ml-auto flex-shrink-0 text-[10px]" style="color: var(--text-muted);">{{ t.status }}</span>
              </button>
            </div>
          </template>
          <p v-else class="px-3 py-4 text-center text-sm" style="color: var(--text-muted);">
            No results for "{{ searchQuery }}"
          </p>
        </div>
      </div>

      <!-- Spacer -->
      <div class="flex-1" />

      <!-- Right actions -->
      <div class="flex flex-shrink-0 items-center gap-1.5">

        <!-- Theme toggle -->
        <button
          @click="themeStore.toggleTheme()"
          :aria-label="themeStore.theme === 'light' ? 'Switch to dark mode' : 'Switch to light mode'"
          class="flex size-9 items-center justify-center rounded-xl transition-colors hover:bg-[var(--bg-sidebar-hover)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--brand)]"
          style="color: var(--brand);"
        >
          <Moon v-if="themeStore.theme === 'light'" class="size-4" aria-hidden="true" />
          <Sun v-else class="size-4 text-amber-500" aria-hidden="true" />
        </button>

        <!-- Notifications -->
        <button
          @click="router.push('/notifications')"
          aria-label="View notifications"
          class="relative flex size-9 items-center justify-center rounded-xl transition-colors hover:bg-[var(--bg-sidebar-hover)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--brand)]"
          style="color: var(--brand);"
        >
          <Bell class="size-4" aria-hidden="true" />
          <span
            v-if="notificationStore.unreadCount > 0"
            class="absolute -right-0.5 -top-0.5 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-red-500 px-1 text-[9px] font-bold text-white"
          >
            {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
          </span>
        </button>

        <!-- Divider -->
        <div class="mx-1 h-6 w-px" style="background: var(--border);" />

        <!-- User avatar + menu -->
        <div ref="userMenuRef" class="relative">
          <button
            @click="showUserMenu = !showUserMenu"
            :aria-label="`User menu — ${user?.name || 'Account'}`"
            :aria-expanded="showUserMenu"
            class="flex items-center gap-2.5 rounded-xl px-2 py-1.5 transition-colors hover:bg-[var(--bg-sidebar-hover)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--brand)]"
          >
            <img v-if="user?.avatarUrl" :src="user.avatarUrl" alt="User avatar" class="size-7 rounded-full object-cover" />
            <div v-else class="flex size-7 items-center justify-center rounded-full text-xs font-bold text-white" style="background: var(--brand);">
              {{ user?.name?.[0]?.toUpperCase() || '?' }}
            </div>
            <div class="hidden sm:block text-left">
              <p class="text-sm font-medium leading-tight" style="color: var(--brand);">{{ user?.name || 'User' }}</p>
            </div>
          </button>

          <!-- Dropdown -->
          <div
            v-if="showUserMenu"
            class="absolute right-0 top-full z-50 mt-2 w-52 overflow-hidden rounded-xl"
            style="
              background: var(--bg-surface);
              border: 1px solid var(--border-light);
              box-shadow: 0 8px 24px rgba(0,0,0,0.1), 0 0 0 1px rgba(0,0,0,0.04);
            "
            role="menu"
          >
            <div class="px-4 py-3" style="border-bottom: 1px solid var(--border-light);">
              <p class="truncate text-sm font-semibold" style="color: var(--text-primary);">{{ user?.name || 'User' }}</p>
              <p class="truncate text-xs" style="color: var(--text-muted);">{{ user?.email || '' }}</p>
            </div>
            <div class="p-1.5 space-y-0.5">
              <button
                @click="showUserMenu = false; router.push('/profile')"
                class="flex w-full items-center gap-2.5 rounded-lg px-3 py-2.5 text-sm transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 focus-visible:outline-none"
                style="color: var(--text-primary);"
                role="menuitem"
              >
                <User class="size-4 flex-shrink-0" aria-hidden="true" />
                Profile
              </button>
              <button
                @click="handleLogout"
                class="flex w-full items-center gap-2.5 rounded-lg px-3 py-2.5 text-sm text-red-500 transition-colors hover:bg-red-50 dark:hover:bg-slate-800 focus-visible:outline-none"
                role="menuitem"
              >
                <LogOut class="size-4 flex-shrink-0" aria-hidden="true" />
                Sign out
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>
