<script setup>
import { computed, onMounted, ref } from 'vue'
import { useTaskStore } from '../stores/taskStore'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useToast } from 'vue-toastification'
import { isPast, isToday } from 'date-fns'
import { parseUtc } from '../utils/date'
import { CalendarCheck2, Check } from 'lucide-vue-next'
import { PriorityBadge, EmptyState } from '@/components/base'

const taskStore      = useTaskStore()
const workspaceStore = useWorkspaceStore()
const toast          = useToast()

const completing = ref(null)

onMounted(() => { taskStore.fetchMyTasks() })

function projectName(projectId) {
  return workspaceStore.projects.find(p => p.id === projectId)?.name || ''
}

// Ranked so the most urgent work is on top: overdue, then due today, then everything else assigned to me.
const items = computed(() =>
  taskStore.myTasks
    .filter(t => t.status !== 'Done')
    .map(t => {
      const due = t.dueDate ? parseUtc(t.dueDate) : null
      return {
        ...t,
        overdue:  !!due && isPast(due) && !isToday(due),
        dueToday: !!due && isToday(due),
      }
    })
    .sort((a, b) => {
      const rank = (x) => (x.overdue ? 0 : x.dueToday ? 1 : x.dueDate ? 2 : 3)
      return rank(a) - rank(b)
    })
    .slice(0, 8)
)

const overdueCount  = computed(() => items.value.filter(t => t.overdue).length)
const dueTodayCount = computed(() => items.value.filter(t => t.dueToday).length)

async function markDone(task) {
  completing.value = task.id
  try {
    await taskStore.changeStatus(task.projectId, task.id, 'Done')
    await taskStore.fetchMyTasks()
    toast.success('Task marked as done')
  } catch {
    toast.error('Failed to update task')
  } finally {
    completing.value = null
  }
}
</script>

<template>
  <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white transition-colors hover:border-zinc-300 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700">

    <!-- Card header -->
    <div class="flex items-center justify-between border-b border-zinc-200 px-4 py-3 dark:border-zinc-800">
      <h2 class="text-sm font-medium text-zinc-800 dark:text-zinc-200">My Day</h2>
      <div v-if="overdueCount || dueTodayCount" class="flex items-center gap-2 text-xs">
        <span v-if="overdueCount" class="font-medium text-red-500">{{ overdueCount }} overdue</span>
        <span v-if="dueTodayCount" class="font-medium text-amber-500">{{ dueTodayCount }} due today</span>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="items.length === 0" class="px-4">
      <EmptyState
        :icon="CalendarCheck2"
        title="You're all caught up"
        description="Nothing assigned to you needs attention right now."
        size="sm"
      />
    </div>

    <!-- Task list -->
    <div v-else class="divide-y divide-zinc-100 dark:divide-zinc-800">
      <div
        v-for="task in items"
        :key="task.id"
        class="flex items-start gap-3 px-4 py-3 transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-800/50"
      >
        <!-- Mark done -->
        <button
          type="button"
          @click="markDone(task)"
          :disabled="completing === task.id"
          :aria-label="`Mark '${task.title}' as done`"
          class="group mt-0.5 flex size-5 flex-shrink-0 items-center justify-center rounded-full border transition-colors hover:border-emerald-500 hover:bg-emerald-50 disabled:opacity-50 dark:hover:bg-emerald-900/30"
          :style="{ borderColor: 'var(--border)' }"
        >
          <Check class="size-3 text-emerald-500 opacity-0 transition-opacity group-hover:opacity-100" aria-hidden="true" />
        </button>

        <!-- Content -->
        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <h4 class="truncate text-sm text-zinc-800 dark:text-zinc-200">{{ task.title }}</h4>
            <PriorityBadge :priority="task.priority" class="flex-shrink-0" />
          </div>
          <div class="mt-1 flex items-center gap-2 text-xs text-zinc-400 dark:text-zinc-500">
            <span v-if="projectName(task.projectId)" class="truncate">{{ projectName(task.projectId) }}</span>
            <span
              v-if="task.overdue"
              class="ml-auto flex-shrink-0 font-medium text-red-500"
            >
              Overdue
            </span>
            <span
              v-else-if="task.dueToday"
              class="ml-auto flex-shrink-0 font-medium text-amber-500"
            >
              Due today
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
