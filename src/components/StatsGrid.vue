<script setup>
import { computed } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { useAuthStore } from '../stores/authStore'
import { FolderOpen, CheckCircle, Users, AlertTriangle } from 'lucide-vue-next'

const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()
const authStore      = useAuthStore()

const stats = computed(() => {
  const projects = workspaceStore.projects || []
  const tasks    = taskStore.allTasks
  const now      = new Date()
  const userId   = authStore.user?.id
  return {
    totalProjects:     projects.length,
    completedProjects: projects.filter(p => p.status === 'Completed').length,
    myTasks:           userId ? tasks.filter(t => t.assigneeId === userId || t.assignee?.id === userId).length : 0,
    overdueIssues:     tasks.filter(t => t.dueDate && new Date(t.dueDate) < now && t.status !== 'Done').length,
  }
})

const statCards = computed(() => [
  {
    icon:      FolderOpen,
    title:     'Total Projects',
    value:     stats.value.totalProjects,
    subtitle:  `in ${workspaceStore.currentWorkspace?.name || 'workspace'}`,
    accent:    '#6366f1',
    accentBg:  'rgba(99,102,241,0.1)',
  },
  {
    icon:      CheckCircle,
    title:     'Completed',
    value:     stats.value.completedProjects,
    subtitle:  `of ${stats.value.totalProjects} total`,
    accent:    '#10b981',
    accentBg:  'rgba(16,185,129,0.1)',
  },
  {
    icon:      Users,
    title:     'My Tasks',
    value:     stats.value.myTasks,
    subtitle:  'assigned to me',
    accent:    '#a855f7',
    accentBg:  'rgba(168,85,247,0.1)',
  },
  {
    icon:      AlertTriangle,
    title:     'Overdue',
    value:     stats.value.overdueIssues,
    subtitle:  'need attention',
    accent:    '#ef4444',
    accentBg:  'rgba(239,68,68,0.1)',
  },
])
</script>

<template>
  <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
    <div
      v-for="(card, i) in statCards"
      :key="i"
      class="card p-5"
    >
      <div class="flex items-start justify-between gap-4">
        <div class="min-w-0 flex-1">
          <p class="text-sm" style="color: var(--text-secondary);">{{ card.title }}</p>
          <p class="mt-1 text-3xl font-bold" style="color: var(--text-primary);">{{ card.value }}</p>
          <p v-if="card.subtitle" class="mt-0.5 truncate text-xs" style="color: var(--text-muted);">{{ card.subtitle }}</p>
        </div>
        <div
          class="flex h-11 w-11 flex-shrink-0 items-center justify-center rounded-xl"
          :style="{ background: card.accentBg }"
        >
          <component :is="card.icon" class="size-5" :style="{ color: card.accent }" aria-hidden="true" />
        </div>
      </div>
    </div>
  </div>
</template>
