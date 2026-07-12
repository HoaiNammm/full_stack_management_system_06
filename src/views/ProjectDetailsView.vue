<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { sprintApi } from '../api/projects'
import {
  ArrowLeft, Plus, Settings, BarChart3, Calendar, FileStack, Zap, GitBranch,
  Sparkles, Flag, Activity, Kanban, ListTodo, User,
} from 'lucide-vue-next'
import { format } from 'date-fns'
import { parseUtc } from '../utils/date'
import { StatusBadge, PriorityBadge, BaseButton } from '@/components/base'
import ProjectAnalytics  from '../components/ProjectAnalytics.vue'
import ProjectSettings   from '../components/ProjectSettings.vue'
import CreateTaskDialog  from '../components/CreateTaskDialog.vue'
import ProjectCalendar   from '../components/ProjectCalendar.vue'
import ProjectTasks      from '../components/ProjectTasks.vue'
import KanbanBoard       from '../components/KanbanBoard.vue'
import ProjectSprints    from '../components/ProjectSprints.vue'
import ProjectMilestones from '../components/ProjectMilestones.vue'
import ActivityLog       from '../components/ActivityLog.vue'
import AIReportPanel     from '../components/AIReportPanel.vue'

const router         = useRouter()
const route          = useRoute()
const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()

const activeTab      = ref(route.query.tab || 'tasks')
const showCreateTask = ref(false)

const projectId = computed(() => route.query.id)
const project   = computed(() => workspaceStore.projects.find(p => p.id === projectId.value))
const tasks     = computed(() => taskStore.getProjectTasks(projectId.value))

const backlogTasks = computed(() => tasks.value.filter(t => !t.sprintId))

const progress = computed(() => {
  if (!tasks.value.length) return 0
  return Math.round((tasks.value.filter(t => t.status === 'Done').length / tasks.value.length) * 100)
})

const teamLead = computed(() => {
  const m = (project.value?.members || []).find(m => m.role === 'Owner')
  if (!m) return null
  return m.user?.name || m.user?.email || null
})

// ── Backlog sprint planning ───────────────────────────────────────────────────
const sprintsForBacklog = ref([])

watch(activeTab, async (tab) => {
  if (tab === 'backlog' && workspaceStore.currentWorkspaceId && projectId.value) {
    try {
      const all = await sprintApi.getAll(workspaceStore.currentWorkspaceId, projectId.value)
      sprintsForBacklog.value = (all || []).filter(s => s.status !== 'Completed')
    } catch { sprintsForBacklog.value = [] }
  }
})

async function assignToSprint(task, sprintId) {
  await taskStore.updateTask(projectId.value, task.id, {
    sprintId:    sprintId || null,
    clearSprint: !sprintId,
  })
  await refreshTasks()
}

async function refreshTasks() {
  if (projectId.value) await taskStore.fetchTasks(projectId.value)
}

onMounted(async () => {
  if (projectId.value) {
    await workspaceStore.fetchProject(projectId.value)
    await taskStore.fetchTasks(projectId.value)
  }
})

watch(projectId, async (id) => {
  if (id) {
    await workspaceStore.fetchProject(id)
    await taskStore.fetchTasks(id)
  }
}, { immediate: false })

watch(() => route.query.tab, (tab) => { if (tab) activeTab.value = tab })

function setTab(key) {
  activeTab.value = key
  router.replace({ query: { id: route.query.id, tab: key } })
}

const tabs = [
  { key: 'tasks',      label: 'Tasks',      icon: FileStack },
  { key: 'board',      label: 'Board',      icon: Kanban },
  { key: 'backlog',    label: 'Backlog',    icon: ListTodo },
  { key: 'sprints',    label: 'Sprints',    icon: GitBranch },
  { key: 'milestones', label: 'Milestones', icon: Flag },
  { key: 'calendar',   label: 'Calendar',   icon: Calendar },
  { key: 'analytics',  label: 'Analytics',  icon: BarChart3 },
  { key: 'activity',   label: 'Activity',   icon: Activity },
  { key: 'ai-report',  label: 'AI Report',  icon: Sparkles },
  { key: 'settings',   label: 'Settings',   icon: Settings },
]
</script>

<template>
  <!-- Not found -->
  <div v-if="!project" class="flex min-h-[60vh] flex-col items-center justify-center gap-3 text-center">
    <p class="text-2xl font-semibold text-zinc-900 dark:text-zinc-100">Project not found</p>
    <p class="text-sm text-zinc-500 dark:text-zinc-400">This project doesn't exist or you don't have access.</p>
    <BaseButton variant="secondary" size="sm" @click="router.push('/projects')">
      <ArrowLeft class="size-4" aria-hidden="true" />
      Back to Projects
    </BaseButton>
  </div>

  <div v-else class="mx-auto max-w-6xl space-y-5 text-zinc-900 dark:text-zinc-100">

    <!-- ── Header ─────────────────────────────────────────────────────────── -->
    <div class="page-header-banner flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-center gap-3">
        <button
          @click="router.push('/projects')"
          aria-label="Back to projects"
          class="rounded-lg p-1.5 text-white/80 transition-colors hover:bg-white/15 hover:text-white focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-white"
        >
          <ArrowLeft class="size-4" aria-hidden="true" />
        </button>
        <div class="flex items-center gap-2.5">
          <h1 class="text-xl font-semibold text-white">{{ project.name }}</h1>
          <StatusBadge :status="project.status" />
        </div>
      </div>
      <BaseButton variant="primary" size="sm" @click="showCreateTask = true">
        <Plus class="size-4" aria-hidden="true" />
        New Task
      </BaseButton>
    </div>

    <!-- ── Progress + meta bar ────────────────────────────────────────────── -->
    <div class="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-zinc-500 dark:text-zinc-400">
      <div class="flex min-w-48 items-center gap-2">
        <span class="flex-shrink-0 font-medium text-zinc-700 dark:text-zinc-300">Progress</span>
        <div class="h-1.5 flex-1 overflow-hidden rounded-full bg-zinc-200 dark:bg-zinc-700">
          <div
            role="progressbar"
            :aria-valuenow="progress"
            aria-valuemin="0"
            aria-valuemax="100"
            class="h-full rounded-full bg-emerald-500 transition-all"
            :style="{ width: progress + '%' }"
          />
        </div>
        <span class="flex-shrink-0 text-xs font-semibold text-emerald-600 dark:text-emerald-400">{{ progress }}%</span>
      </div>
      <div v-if="project.sprintCount != null" class="flex items-center gap-1.5">
        <GitBranch class="size-3.5" aria-hidden="true" />
        <span>{{ project.sprintCount }} sprint{{ project.sprintCount !== 1 ? 's' : '' }}</span>
      </div>
      <div v-if="teamLead" class="flex items-center gap-1.5">
        <User class="size-3.5" aria-hidden="true" />
        <span>{{ teamLead }}</span>
      </div>
      <div v-if="project.updatedAt">
        <span class="text-xs text-zinc-400 dark:text-zinc-500">
          Updated {{ format(parseUtc(project.updatedAt), 'dd MMM yyyy') }}
        </span>
      </div>
    </div>

    <!-- ── Stat mini-cards ────────────────────────────────────────────────── -->
    <div class="grid grid-cols-2 gap-3 sm:flex sm:flex-wrap">
      <div
        v-for="(card, idx) in [
          { label: 'Total Tasks',  value: tasks.length,                                       color: 'text-zinc-900 dark:text-zinc-100' },
          { label: 'Completed',    value: tasks.filter(t => t.status === 'Done').length,       color: 'text-emerald-700 dark:text-emerald-400' },
          { label: 'In Progress',  value: tasks.filter(t => t.status === 'InProgress').length, color: 'text-amber-700 dark:text-amber-400' },
          { label: 'Backlog',      value: backlogTasks.length,                                 color: 'text-purple-700 dark:text-purple-400' },
        ]"
        :key="idx"
        class="flex items-center justify-between rounded-lg border border-zinc-200 bg-white px-4 py-2.5 dark:border-zinc-800 dark:bg-zinc-900 sm:min-w-52"
      >
        <div>
          <div class="text-xs text-zinc-500 dark:text-zinc-400">{{ card.label }}</div>
          <div :class="['text-2xl font-bold', card.color]">{{ card.value }}</div>
        </div>
        <Zap :class="['size-4 opacity-60', card.color]" aria-hidden="true" />
      </div>
    </div>

    <!-- ── Tab navigation ─────────────────────────────────────────────────── -->
    <div>
      <div
        role="tablist"
        aria-label="Project sections"
        class="inline-flex flex-wrap gap-1 rounded-lg border border-zinc-200 p-1 dark:border-zinc-800 max-sm:grid max-sm:grid-cols-5"
      >
        <button
          v-for="tab in tabs"
          :key="tab.key"
          role="tab"
          :aria-selected="activeTab === tab.key"
          @click="setTab(tab.key)"
          :class="[
            'flex items-center gap-2 rounded-md px-3 py-1.5 text-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
            activeTab === tab.key
              ? 'bg-zinc-100 font-medium text-zinc-900 dark:bg-zinc-800 dark:text-zinc-100'
              : 'text-zinc-600 hover:bg-zinc-50 hover:text-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800/60 dark:hover:text-zinc-100',
          ]"
        >
          <component :is="tab.icon" class="size-3.5" aria-hidden="true" />
          {{ tab.label }}
        </button>
      </div>

      <!-- ── Tab panels ──────────────────────────────────────────────────── -->
      <div class="mt-6">
        <ProjectTasks
          v-if="activeTab === 'tasks'"
          :tasks="tasks"
          :projectId="projectId"
          :onTasksChange="refreshTasks"
        />
        <KanbanBoard
          v-else-if="activeTab === 'board'"
          :tasks="tasks"
          :projectId="projectId"
          :onTasksChange="refreshTasks"
        />

        <!-- Backlog -->
        <div v-else-if="activeTab === 'backlog'" class="space-y-3">
          <div class="flex items-center justify-between">
            <h2 class="flex items-center gap-2 text-base font-medium text-zinc-900 dark:text-zinc-100">
              <ListTodo class="size-4" aria-hidden="true" />
              Backlog
              <span class="text-xs font-normal text-zinc-500 dark:text-zinc-400">
                ({{ backlogTasks.length }} tasks without sprint)
              </span>
            </h2>
            <BaseButton variant="secondary" size="sm" @click="showCreateTask = true">
              <Plus class="size-3.5" aria-hidden="true" />
              Add to Backlog
            </BaseButton>
          </div>

          <p
            v-if="backlogTasks.length === 0"
            class="py-10 text-center text-sm text-zinc-400 dark:text-zinc-500"
          >
            No unassigned tasks — all tasks are in a sprint.
          </p>

          <div v-else class="space-y-2">
            <div
              v-for="task in backlogTasks"
              :key="task.id"
              @click="$router.push(`/taskDetails?projectId=${task.projectId || projectId}&taskId=${task.id}`)"
              class="flex cursor-pointer items-center gap-3 rounded-lg border border-zinc-200 bg-white px-4 py-3 transition-colors hover:border-zinc-300 hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700 dark:hover:bg-zinc-800/50"
            >
              <div class="min-w-0 flex-1">
                <p class="truncate text-sm font-medium text-zinc-900 dark:text-zinc-100">{{ task.title }}</p>
                <p v-if="task.description" class="mt-0.5 truncate text-xs text-zinc-500 dark:text-zinc-400">{{ task.description }}</p>
              </div>
              <StatusBadge :status="task.status" size="xs" class="flex-shrink-0" />
              <PriorityBadge v-if="task.priority" :priority="task.priority" variant="badge" class="flex-shrink-0" />
              <select
                v-if="sprintsForBacklog.length"
                @click.stop
                @change.stop="assignToSprint(task, $event.target.value)"
                class="flex-shrink-0 cursor-pointer rounded border border-zinc-300 bg-white px-1.5 py-0.5 text-xs text-zinc-800 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200"
              >
                <option value="">Move to Sprint…</option>
                <option v-for="s in sprintsForBacklog" :key="s.id" :value="s.id">
                  {{ s.name }}{{ s.status === 'Active' ? ' ★' : '' }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <ProjectSprints    v-else-if="activeTab === 'sprints'"    :project="project" />
        <ProjectMilestones v-else-if="activeTab === 'milestones'" :project="project" />
        <ProjectCalendar   v-else-if="activeTab === 'calendar'"   :tasks="tasks" />
        <ProjectAnalytics  v-else-if="activeTab === 'analytics'"  :tasks="tasks" :project="project" />
        <ActivityLog       v-else-if="activeTab === 'activity'"   :projectId="projectId" />
        <AIReportPanel     v-else-if="activeTab === 'ai-report'"  :projectId="projectId" />
        <ProjectSettings
          v-else-if="activeTab === 'settings'"
          :project="project"
          @updated="p => {
            const idx = workspaceStore.projects.findIndex(x => x.id === p.id)
            if (idx !== -1) workspaceStore.projects[idx] = { ...workspaceStore.projects[idx], ...p }
          }"
        />
      </div>
    </div>

  </div>

  <CreateTaskDialog
    v-if="showCreateTask"
    :show="showCreateTask"
    @close="showCreateTask = false; refreshTasks()"
    :projectId="projectId"
  />
</template>
