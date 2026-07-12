<script setup>
import { ref, computed } from 'vue'
import { format, isSameDay, isBefore, startOfMonth, endOfMonth, eachDayOfInterval, addMonths, subMonths, startOfDay } from 'date-fns'
import { CalendarIcon, Clock, User, ChevronLeft, ChevronRight } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const props = defineProps({ tasks: Array, projectId: String })
const router = useRouter()

const selectedDate  = ref(new Date())
const currentMonth  = ref(new Date())
const today         = startOfDay(new Date())

const priorityBorder = { Low: 'border-zinc-300 dark:border-zinc-600', Medium: 'border-amber-400 dark:border-amber-500', High: 'border-red-400 dark:border-red-500', Critical: 'border-purple-400 dark:border-purple-500' }
const statusColor    = { Backlog: 'bg-zinc-100 text-zinc-600 dark:bg-zinc-700 dark:text-zinc-300', ToDo: 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300', InProgress: 'bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300', Review: 'bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300', Done: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300', Blocked: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300' }

// Tasks that have a dueDate (camelCase from backend)
const dueTasks = computed(() => (props.tasks || []).filter(t => t.dueDate))

const daysInMonth = computed(() =>
  eachDayOfInterval({ start: startOfMonth(currentMonth.value), end: endOfMonth(currentMonth.value) })
)

// Leading blank cells so grid starts on correct weekday (0=Sun)
const leadingBlanks = computed(() => startOfMonth(currentMonth.value).getDay())

const upcomingTasks = computed(() =>
  dueTasks.value
    .filter(t => !isBefore(startOfDay(new Date(t.dueDate)), today) && t.status !== 'Done')
    .sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
    .slice(0, 5)
)

const overdueTasks = computed(() =>
  dueTasks.value.filter(t => isBefore(startOfDay(new Date(t.dueDate)), today) && t.status !== 'Done')
)

function getTasksForDate(date) {
  return dueTasks.value.filter(t => isSameDay(new Date(t.dueDate), date))
}

const selectedDateTasks = computed(() => getTasksForDate(selectedDate.value))

function changeMonth(dir) {
  currentMonth.value = dir === 'next' ? addMonths(currentMonth.value, 1) : subMonths(currentMonth.value, 1)
}

function goToTask(task) {
  if (task.projectId || props.projectId)
    router.push(`/taskDetails?projectId=${task.projectId || props.projectId}&taskId=${task.id}`)
}
</script>

<template>
  <div class="grid lg:grid-cols-3 gap-6">
    <!-- Calendar grid -->
    <div class="lg:col-span-2 space-y-4">
      <div class="bg-white dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 rounded-lg p-4">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-sm font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <CalendarIcon class="size-4" /> Task Calendar
          </h2>
          <div class="flex items-center gap-2">
            <button @click="changeMonth('prev')" class="p-1 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">
              <ChevronLeft class="size-4 text-zinc-500 dark:text-zinc-400" />
            </button>
            <span class="text-sm font-medium text-zinc-900 dark:text-zinc-100 min-w-32 text-center">
              {{ format(currentMonth, 'MMMM yyyy') }}
            </span>
            <button @click="changeMonth('next')" class="p-1 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">
              <ChevronRight class="size-4 text-zinc-500 dark:text-zinc-400" />
            </button>
          </div>
        </div>

        <!-- Weekday headers -->
        <div class="grid grid-cols-7 mb-1">
          <div v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']" :key="d"
            class="text-center text-xs text-zinc-400 dark:text-zinc-500 py-1">{{ d }}</div>
        </div>

        <!-- Day cells -->
        <div class="grid grid-cols-7 gap-1">
          <!-- Leading blank cells -->
          <div v-for="n in leadingBlanks" :key="'blank-' + n" />

          <button v-for="day in daysInMonth" :key="day.toISOString()" @click="selectedDate = day"
            :class="[
              'min-h-12 rounded-md p-1 flex flex-col items-center transition-colors',
              isSameDay(day, selectedDate)
                ? 'bg-blue-500 text-white'
                : isSameDay(day, today)
                ? 'bg-zinc-100 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-100 ring-1 ring-blue-400'
                : 'hover:bg-zinc-50 dark:hover:bg-zinc-800/60 text-zinc-700 dark:text-zinc-300',
              getTasksForDate(day).some(t => t.status !== 'Done' && isBefore(startOfDay(new Date(t.dueDate)), today))
                ? 'ring-1 ring-red-400 dark:ring-red-500' : ''
            ]">
            <span class="text-xs font-medium">{{ format(day, 'd') }}</span>
            <template v-if="getTasksForDate(day).length">
              <span :class="['text-[9px] mt-0.5 px-1 rounded-full font-medium',
                isSameDay(day, selectedDate) ? 'bg-white/30 text-white' : 'bg-blue-100 dark:bg-blue-900/50 text-blue-700 dark:text-blue-300']">
                {{ getTasksForDate(day).length }}
              </span>
            </template>
          </button>
        </div>
      </div>

      <!-- Tasks for selected day -->
      <div class="bg-white dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 rounded-lg p-4">
        <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-100 mb-3">
          {{ format(selectedDate, 'EEEE, MMM d, yyyy') }}
          <span class="text-zinc-400 dark:text-zinc-500 font-normal ml-1">({{ selectedDateTasks.length }} tasks)</span>
        </h3>

        <div v-if="selectedDateTasks.length === 0" class="text-sm text-zinc-400 dark:text-zinc-500 py-4 text-center">
          No tasks due on this date
        </div>
        <div v-else class="space-y-2">
          <div v-for="task in selectedDateTasks" :key="task.id"
            @click="goToTask(task)"
            :class="['border-l-4 px-3 py-2.5 rounded-r cursor-pointer hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors', priorityBorder[task.priority] || 'border-zinc-300']">
            <div class="flex items-center justify-between gap-2">
              <span class="text-sm font-medium text-zinc-900 dark:text-zinc-100 truncate">{{ task.title }}</span>
              <span :class="['text-xs px-1.5 py-0.5 rounded flex-shrink-0', statusColor[task.status] || 'bg-zinc-100 text-zinc-500']">
                {{ task.status }}
              </span>
            </div>
            <div class="flex items-center gap-3 mt-1 text-xs text-zinc-500 dark:text-zinc-400">
              <span v-if="task.assignee?.name" class="flex items-center gap-1">
                <User class="size-3" /> {{ task.assignee.name }}
              </span>
              <span>{{ task.priority }} priority</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Sidebar -->
    <div class="space-y-4">
      <!-- Upcoming -->
      <div class="bg-white dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 rounded-lg p-4">
        <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2 mb-3">
          <Clock class="size-4 text-blue-500" /> Upcoming
        </h3>
        <p v-if="upcomingTasks.length === 0" class="text-xs text-zinc-400 dark:text-zinc-500 text-center py-2">
          No upcoming tasks
        </p>
        <div v-else class="space-y-2">
          <div v-for="task in upcomingTasks" :key="task.id"
            @click="goToTask(task)"
            class="p-2.5 rounded cursor-pointer hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors border border-zinc-100 dark:border-zinc-800">
            <p class="text-xs font-medium text-zinc-900 dark:text-zinc-100 truncate">{{ task.title }}</p>
            <p class="text-xs text-blue-600 dark:text-blue-400 mt-0.5">{{ format(new Date(task.dueDate), 'MMM d') }}</p>
          </div>
        </div>
      </div>

      <!-- Overdue -->
      <div v-if="overdueTasks.length > 0" class="bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <h3 class="text-sm font-medium text-red-700 dark:text-red-400 flex items-center gap-2 mb-3">
          <Clock class="size-4" /> Overdue ({{ overdueTasks.length }})
        </h3>
        <div class="space-y-2">
          <div v-for="task in overdueTasks.slice(0, 5)" :key="task.id"
            @click="goToTask(task)"
            class="p-2.5 rounded cursor-pointer hover:bg-red-100 dark:hover:bg-red-900/20 transition-colors border border-red-100 dark:border-red-900">
            <p class="text-xs font-medium text-zinc-900 dark:text-zinc-100 truncate">{{ task.title }}</p>
            <p class="text-xs text-red-600 dark:text-red-400 mt-0.5">Due {{ format(new Date(task.dueDate), 'MMM d') }}</p>
          </div>
          <p v-if="overdueTasks.length > 5" class="text-xs text-zinc-400 text-center">+{{ overdueTasks.length - 5 }} more</p>
        </div>
      </div>

      <!-- Stats -->
      <div class="bg-white dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 rounded-lg p-4">
        <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-100 mb-3">This month</h3>
        <div class="space-y-2 text-xs text-zinc-600 dark:text-zinc-400">
          <div class="flex justify-between">
            <span>Total with due date</span>
            <span class="font-medium text-zinc-900 dark:text-zinc-100">{{ dueTasks.length }}</span>
          </div>
          <div class="flex justify-between">
            <span>Upcoming</span>
            <span class="font-medium text-blue-600 dark:text-blue-400">{{ upcomingTasks.length }}</span>
          </div>
          <div class="flex justify-between">
            <span>Overdue</span>
            <span class="font-medium text-red-600 dark:text-red-400">{{ overdueTasks.length }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
