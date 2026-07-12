<script setup>
import { computed } from 'vue'
import { Bar, Pie } from 'vue-chartjs'
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend } from 'chart.js'
import { CheckCircle, Clock, AlertTriangle, Users } from 'lucide-vue-next'
import { PriorityBadge } from '@/components/base'

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend)

const props = defineProps({ project: Object, tasks: Array })

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']

const priorityBarCls = {
  High:   'bg-red-500',
  Medium: 'bg-amber-500',
  Low:    'bg-zinc-400',
}

const analytics = computed(() => {
  const tasks = props.tasks || []
  const now = new Date()
  const total = tasks.length

  const statusMap   = { ToDo: 0, InProgress: 0, Review: 0, Done: 0, Backlog: 0 }
  const typeMap     = { Task: 0, Bug: 0, Feature: 0, Improvement: 0, Other: 0 }
  const priorityMap = { Low: 0, Medium: 0, High: 0 }

  let completed = 0, inProgress = 0, overdue = 0

  tasks.forEach(t => {
    if (t.status === 'Done')       completed++
    if (t.status === 'InProgress') inProgress++
    if (t.dueDate && new Date(t.dueDate) < now && t.status !== 'Done') overdue++
    if (statusMap[t.status]      !== undefined) statusMap[t.status]++
    if (typeMap[t.type]          !== undefined) typeMap[t.type]++
    if (priorityMap[t.priority]  !== undefined) priorityMap[t.priority]++
  })

  return {
    stats: { total, completed, inProgress, overdue },
    completionRate: total ? Math.round((completed / total) * 100) : 0,
    barData: {
      labels: Object.keys(statusMap),
      datasets: [{ data: Object.values(statusMap), backgroundColor: '#3b82f6', borderRadius: 4 }]
    },
    pieData: {
      labels: Object.entries(typeMap).filter(([, v]) => v > 0).map(([k]) => k),
      datasets: [{ data: Object.entries(typeMap).filter(([, v]) => v > 0).map(([, v]) => v), backgroundColor: COLORS }]
    },
    priorityData: Object.entries(priorityMap).map(([k, v]) => ({
      name: k, value: v, percentage: total > 0 ? Math.round((v / total) * 100) : 0
    }))
  }
})

const chartOptions = { responsive: true, plugins: { legend: { display: false } } }
</script>

<template>
  <div class="space-y-6">
    <!-- Metrics -->
    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
      <div
        v-for="(m, i) in [
          { label: 'Completion Rate', value: `${analytics.completionRate}%`, color: 'text-emerald-600 dark:text-emerald-400', icon: CheckCircle, bg: 'bg-emerald-100 dark:bg-emerald-500/10' },
          { label: 'Active Tasks',    value: analytics.stats.inProgress,     color: 'text-blue-600 dark:text-blue-400',        icon: Clock,         bg: 'bg-blue-100 dark:bg-blue-500/10' },
          { label: 'Overdue Tasks',   value: analytics.stats.overdue,         color: 'text-red-600 dark:text-red-400',          icon: AlertTriangle, bg: 'bg-red-100 dark:bg-red-500/10' },
          { label: 'Team Size',       value: project?.memberCount || 0,        color: 'text-purple-600 dark:text-purple-400',    icon: Users,         bg: 'bg-purple-100 dark:bg-purple-500/10' },
        ]"
        :key="i"
        class="rounded-lg border border-zinc-200 bg-white p-6 dark:border-zinc-800 dark:bg-zinc-900"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-zinc-500 dark:text-zinc-400">{{ m.label }}</p>
            <p :class="['text-xl font-bold', m.color]">{{ m.value }}</p>
          </div>
          <div :class="['rounded-md p-2', m.bg]">
            <component :is="m.icon" :class="['size-5', m.color]" aria-hidden="true" />
          </div>
        </div>
      </div>
    </div>

    <!-- Charts -->
    <div class="grid gap-6 lg:grid-cols-2">
      <div class="rounded-lg border border-zinc-200 bg-white p-6 dark:border-zinc-800 dark:bg-zinc-900">
        <h2 class="mb-4 font-medium text-zinc-900 dark:text-zinc-100">Tasks by Status</h2>
        <Bar :data="analytics.barData" :options="chartOptions" />
      </div>
      <div class="rounded-lg border border-zinc-200 bg-white p-6 dark:border-zinc-800 dark:bg-zinc-900">
        <h2 class="mb-4 font-medium text-zinc-900 dark:text-zinc-100">Tasks by Type</h2>
        <div v-if="analytics.pieData.labels.length > 0">
          <Pie :data="analytics.pieData" :options="{ responsive: true }" />
        </div>
        <p v-else class="py-8 text-center text-sm text-zinc-400 dark:text-zinc-500">No tasks to display</p>
      </div>
    </div>

    <!-- Priority Breakdown -->
    <div class="rounded-lg border border-zinc-200 bg-white p-6 dark:border-zinc-800 dark:bg-zinc-900">
      <h2 class="mb-4 font-medium text-zinc-900 dark:text-zinc-100">Tasks by Priority</h2>
      <div class="space-y-4">
        <div v-for="p in analytics.priorityData" :key="p.name" class="space-y-2">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <PriorityBadge :priority="p.name" variant="badge" />
            </div>
            <div class="flex items-center gap-2">
              <span class="text-sm text-zinc-500 dark:text-zinc-400">{{ p.value }} tasks</span>
              <span class="rounded border border-zinc-300 px-2 py-0.5 text-xs text-zinc-500 dark:border-zinc-700 dark:text-zinc-400">
                {{ p.percentage }}%
              </span>
            </div>
          </div>
          <div class="h-1.5 w-full overflow-hidden rounded-full bg-zinc-200 dark:bg-zinc-800">
            <div
              role="progressbar"
              :aria-valuenow="p.percentage"
              aria-valuemin="0"
              aria-valuemax="100"
              :aria-label="`${p.name} priority: ${p.percentage}%`"
              :class="['h-full rounded-full transition-all', priorityBarCls[p.name] || 'bg-zinc-400']"
              :style="{ width: `${p.percentage}%` }"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
