<script setup>
import { ref, computed, watch } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { sprintApi } from '../api/projects'
import { differenceInCalendarDays } from 'date-fns'
import { parseUtc } from '../utils/date'
import { Zap } from 'lucide-vue-next'
import { EmptyState } from '@/components/base'

const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()

const activeSprints = ref([])   // [{ sprint, project }]
const loading       = ref(true)

async function load() {
  loading.value = true
  const wid = workspaceStore.currentWorkspaceId
  const projects = workspaceStore.projects
  const found = []

  await Promise.all(projects.map(async (project) => {
    try {
      const sprints = await sprintApi.getAll(wid, project.id)
      for (const sprint of sprints || []) {
        if (sprint.status === 'Active') found.push({ sprint, project })
      }
    } catch { /* skip projects the sprint fetch fails for */ }
  }))

  await Promise.all(found.map(({ project }) =>
    taskStore.getProjectTasks(project.id).length ? null : taskStore.fetchTasks(project.id)
  ))

  activeSprints.value = found
  loading.value = false
}

watch(() => workspaceStore.projects, load, { immediate: true })

function sprintStats(sprintId, projectId) {
  const tasks = taskStore.getProjectTasks(projectId).filter(t => t.sprintId === sprintId)
  const done  = tasks.filter(t => t.status === 'Done').length
  const total = tasks.length
  return { done, total, percent: total ? Math.round((done / total) * 100) : 0 }
}

function daysLeftLabel(endDate) {
  const d = parseUtc(endDate)
  if (!d) return ''
  const days = differenceInCalendarDays(d, new Date())
  if (days < 0) return 'Ended'
  if (days === 0) return 'Ends today'
  return `${days} day${days === 1 ? '' : 's'} left`
}
</script>

<template>
  <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white transition-colors hover:border-zinc-300 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700">

    <!-- Card header -->
    <div class="border-b border-zinc-200 px-4 py-3 dark:border-zinc-800">
      <h2 class="text-sm font-medium text-zinc-800 dark:text-zinc-200">Sprint Progress</h2>
    </div>

    <!-- Empty state -->
    <div v-if="!loading && activeSprints.length === 0" class="px-4">
      <EmptyState
        :icon="Zap"
        title="No active sprint"
        description="Start a sprint on one of your projects to track its progress here."
        size="sm"
      />
    </div>

    <!-- Active sprints -->
    <div v-else class="divide-y divide-zinc-100 dark:divide-zinc-800">
      <div
        v-for="{ sprint, project } in activeSprints"
        :key="sprint.id"
        class="px-4 py-3"
      >
        <div class="mb-1.5 flex items-start justify-between gap-2">
          <div class="min-w-0">
            <h4 class="truncate text-sm font-medium text-zinc-800 dark:text-zinc-200">{{ sprint.name }}</h4>
            <p class="truncate text-xs text-zinc-400 dark:text-zinc-500">{{ project.name }}</p>
          </div>
          <span class="flex-shrink-0 text-xs text-zinc-400 dark:text-zinc-500">{{ daysLeftLabel(sprint.endDate) }}</span>
        </div>

        <div class="h-1.5 w-full overflow-hidden rounded-full bg-zinc-100 dark:bg-zinc-800">
          <div
            role="progressbar"
            :aria-valuenow="sprintStats(sprint.id, project.id).percent"
            aria-valuemin="0"
            aria-valuemax="100"
            :aria-label="`${sprint.name} progress`"
            class="h-full rounded-full bg-black transition-[width] dark:bg-white"
            :style="{ width: `${sprintStats(sprint.id, project.id).percent}%` }"
          />
        </div>
        <p class="mt-1 text-right text-xs text-zinc-400 dark:text-zinc-500">
          {{ sprintStats(sprint.id, project.id).done }}/{{ sprintStats(sprint.id, project.id).total }} tasks · {{ sprintStats(sprint.id, project.id).percent }}%
        </p>
      </div>
    </div>
  </div>
</template>
