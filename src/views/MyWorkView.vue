<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useTaskStore } from '../stores/taskStore'
import { format, isToday, isPast, startOfDay } from 'date-fns'
import { parseUtc } from '../utils/date'
import { AlertCircle, CalendarIcon, CheckCircle2, RefreshCw, User } from 'lucide-vue-next'
import { StatusBadge, PriorityBadge, BaseButton, EmptyState } from '@/components/base'

const router    = useRouter()
const taskStore = useTaskStore()
const navError  = ref('')

onMounted(() => taskStore.fetchMyTasks())

const tasks   = computed(() => taskStore.myTasks)
const loading = computed(() => taskStore.myTasksLoading)

const assignedToMe = computed(() =>
  tasks.value.filter(t => t.status !== 'Done')
)

const dueToday = computed(() =>
  tasks.value.filter(t => t.dueDate && isToday(new Date(t.dueDate)) && t.status !== 'Done')
)

const overdue = computed(() =>
  tasks.value.filter(t =>
    t.dueDate &&
    isPast(startOfDay(new Date(t.dueDate))) &&
    !isToday(new Date(t.dueDate)) &&
    t.status !== 'Done'
  )
)

const recentlyUpdated = computed(() =>
  [...tasks.value]
    .filter(t => t.updatedAt)
    .sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
    .slice(0, 10)
)

const completedToday = computed(() =>
  tasks.value.filter(t => t.status === 'Done' && t.updatedAt && isToday(new Date(t.updatedAt)))
)

function goToTask(task) {
  const pid = task.projectId
  if (!pid) {
    navError.value = 'Task is not linked to a project and cannot be opened.'
    setTimeout(() => { navError.value = '' }, 3000)
    return
  }
  router.push(`/taskDetails?projectId=${pid}&taskId=${task.id}`)
}

const LABEL_COLORS = [
  'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
  'bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300',
  'bg-teal-100 text-teal-700 dark:bg-teal-900 dark:text-teal-300',
  'bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300',
  'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
  'bg-pink-100 text-pink-700 dark:bg-pink-900 dark:text-pink-300',
]
function labelColor(str) {
  let h = 0; for (const c of str) h = c.charCodeAt(0) + ((h << 5) - h)
  return LABEL_COLORS[Math.abs(h) % LABEL_COLORS.length]
}

function fmtDate(d) {
  try { return format(new Date(d), 'dd MMM') } catch { return '' }
}
function fmtDateTime(d) {
  try { return format(parseUtc(d), 'dd MMM, HH:mm') } catch { return '' }
}
</script>

<template>
  <div class="mx-auto max-w-5xl space-y-8 text-zinc-900 dark:text-zinc-100">

    <!-- Nav error alert -->
    <div
      v-if="navError"
      role="alert"
      aria-live="polite"
      class="flex items-center gap-2 rounded-lg border border-amber-300 bg-amber-50 px-4 py-2.5 text-sm text-amber-800 dark:border-amber-700 dark:bg-amber-900/30 dark:text-amber-300"
    >
      <AlertCircle class="size-4 flex-shrink-0" aria-hidden="true" /> {{ navError }}
    </div>

    <!-- Header -->
    <div class="page-header-banner flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight text-white sm:text-2xl">My Work</h1>
        <p class="mt-0.5 text-sm text-white/80">Tasks assigned to you across all projects</p>
      </div>
      <BaseButton
        variant="secondary"
        size="sm"
        :disabled="loading"
        @click="taskStore.fetchMyTasks()"
        aria-label="Refresh tasks"
      >
        <RefreshCw :class="['size-3.5', loading ? 'animate-spin' : '']" aria-hidden="true" /> Refresh
      </BaseButton>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="py-16 text-center text-sm text-zinc-400 dark:text-zinc-500">
      Loading your tasks…
    </div>

    <div v-else class="space-y-8">

      <!-- Summary cards -->
      <div class="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div
          v-for="card in [
            { label: 'Assigned',   value: assignedToMe.length,   color: 'text-blue-600 dark:text-blue-400',       border: 'border-blue-200 dark:border-blue-900' },
            { label: 'Due Today',  value: dueToday.length,       color: 'text-amber-600 dark:text-amber-400',     border: 'border-amber-200 dark:border-amber-900' },
            { label: 'Overdue',    value: overdue.length,        color: 'text-red-600 dark:text-red-400',         border: 'border-red-200 dark:border-red-900' },
            { label: 'Done Today', value: completedToday.length, color: 'text-emerald-600 dark:text-emerald-400', border: 'border-emerald-200 dark:border-emerald-900' },
          ]"
          :key="card.label"
          :class="['rounded-lg border bg-white p-4 dark:bg-zinc-900', card.border]"
        >
          <div :class="['text-2xl font-bold tabular-nums', card.color]">{{ card.value }}</div>
          <div class="mt-1 text-xs text-zinc-500 dark:text-zinc-400">{{ card.label }}</div>
        </div>
      </div>

      <!-- Overdue -->
      <section v-if="overdue.length" aria-label="Overdue tasks">
        <h2 class="mb-3 flex items-center gap-2 text-sm font-semibold text-red-600 dark:text-red-400">
          <AlertCircle class="size-4" aria-hidden="true" /> Overdue ({{ overdue.length }})
        </h2>
        <div class="space-y-1.5" role="list">
          <div
            v-for="task in overdue"
            :key="task.id"
            role="listitem"
            tabindex="0"
            @click="goToTask(task)"
            @keydown.enter="goToTask(task)"
            class="flex cursor-pointer items-center gap-3 rounded-lg border border-red-200 bg-red-50/80 px-4 py-2.5 transition-colors hover:bg-red-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-red-400 dark:border-red-900/60 dark:bg-red-950/20 dark:hover:bg-red-950/30"
          >
            <PriorityBadge v-if="task.priority" :priority="task.priority" variant="dot" />
            <div class="min-w-0 flex-1">
              <p class="truncate text-sm font-medium">{{ task.title }}</p>
            </div>
            <StatusBadge :status="task.status" size="xs" class="flex-shrink-0" />
            <div class="flex flex-shrink-0 items-center gap-1 text-xs text-red-500 dark:text-red-400">
              <CalendarIcon class="size-3" aria-hidden="true" /> {{ fmtDate(task.dueDate) }}
            </div>
            <div class="flex max-w-[120px] flex-wrap gap-1">
              <span v-for="lbl in (task.labels || []).slice(0, 2)" :key="lbl" :class="['rounded px-1.5 py-0.5 text-[10px]', labelColor(lbl)]">{{ lbl }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Due Today -->
      <section v-if="dueToday.length" aria-label="Tasks due today">
        <h2 class="mb-3 flex items-center gap-2 text-sm font-semibold text-amber-600 dark:text-amber-400">
          <CalendarIcon class="size-4" aria-hidden="true" /> Due Today ({{ dueToday.length }})
        </h2>
        <div class="space-y-1.5" role="list">
          <div
            v-for="task in dueToday"
            :key="task.id"
            role="listitem"
            tabindex="0"
            @click="goToTask(task)"
            @keydown.enter="goToTask(task)"
            class="flex cursor-pointer items-center gap-3 rounded-lg border border-amber-200 bg-amber-50/80 px-4 py-2.5 transition-colors hover:bg-amber-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-amber-400 dark:border-amber-900/60 dark:bg-amber-950/20 dark:hover:bg-amber-950/30"
          >
            <PriorityBadge v-if="task.priority" :priority="task.priority" variant="dot" />
            <div class="min-w-0 flex-1">
              <p class="truncate text-sm font-medium">{{ task.title }}</p>
            </div>
            <StatusBadge :status="task.status" size="xs" class="flex-shrink-0" />
            <div class="flex max-w-[120px] flex-wrap gap-1">
              <span v-for="lbl in (task.labels || []).slice(0, 2)" :key="lbl" :class="['rounded px-1.5 py-0.5 text-[10px]', labelColor(lbl)]">{{ lbl }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Assigned To Me -->
      <section v-if="assignedToMe.length" aria-label="Tasks assigned to me">
        <h2 class="mb-3 flex items-center gap-2 text-sm font-semibold text-blue-600 dark:text-blue-400">
          <User class="size-4" aria-hidden="true" /> Assigned to Me ({{ assignedToMe.length }})
        </h2>
        <div class="space-y-1.5" role="list">
          <div
            v-for="task in assignedToMe.slice(0, 20)"
            :key="task.id"
            role="listitem"
            tabindex="0"
            @click="goToTask(task)"
            @keydown.enter="goToTask(task)"
            class="flex cursor-pointer items-center gap-3 rounded-lg border border-zinc-200 bg-white px-4 py-2.5 transition-colors hover:border-zinc-300 hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700 dark:hover:bg-zinc-800/60"
          >
            <PriorityBadge v-if="task.priority" :priority="task.priority" variant="dot" />
            <div class="min-w-0 flex-1">
              <p class="truncate text-sm font-medium">{{ task.title }}</p>
            </div>
            <StatusBadge :status="task.status" size="xs" class="flex-shrink-0" />
            <div v-if="task.dueDate" class="flex flex-shrink-0 items-center gap-1 text-xs text-zinc-400 dark:text-zinc-500">
              <CalendarIcon class="size-3" aria-hidden="true" /> {{ fmtDate(task.dueDate) }}
            </div>
            <div class="flex max-w-[120px] flex-wrap gap-1">
              <span v-for="lbl in (task.labels || []).slice(0, 2)" :key="lbl" :class="['rounded px-1.5 py-0.5 text-[10px]', labelColor(lbl)]">{{ lbl }}</span>
            </div>
          </div>
          <p v-if="assignedToMe.length > 20" class="mt-2 pl-1 text-xs text-zinc-400 dark:text-zinc-500">
            +{{ assignedToMe.length - 20 }} more tasks
          </p>
        </div>
      </section>

      <!-- Recently Updated -->
      <section v-if="recentlyUpdated.length" aria-label="Recently updated tasks">
        <h2 class="mb-3 flex items-center gap-2 text-sm font-semibold text-zinc-500 dark:text-zinc-400">
          <RefreshCw class="size-4" aria-hidden="true" /> Recently Updated
        </h2>
        <div class="space-y-1.5" role="list">
          <div
            v-for="task in recentlyUpdated"
            :key="task.id"
            role="listitem"
            tabindex="0"
            @click="goToTask(task)"
            @keydown.enter="goToTask(task)"
            class="flex cursor-pointer items-center gap-3 rounded-lg border border-zinc-200 bg-white px-4 py-2.5 transition-colors hover:border-zinc-300 hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700 dark:hover:bg-zinc-800/60"
          >
            <PriorityBadge v-if="task.priority" :priority="task.priority" variant="dot" />
            <div class="min-w-0 flex-1">
              <p class="truncate text-sm font-medium">{{ task.title }}</p>
            </div>
            <StatusBadge :status="task.status" size="xs" class="flex-shrink-0" />
            <span v-if="task.updatedAt" class="flex-shrink-0 text-xs text-zinc-400 dark:text-zinc-500">
              {{ fmtDateTime(task.updatedAt) }}
            </span>
          </div>
        </div>
      </section>

      <!-- Empty state -->
      <EmptyState
        v-if="!tasks.length"
        :icon="CheckCircle2"
        title="You're all caught up!"
        description="No tasks are currently assigned to you."
        size="lg"
      />
    </div>
  </div>
</template>
