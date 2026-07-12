<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { format } from 'date-fns'
import { useTaskStore } from '../stores/taskStore'
import { Bug, CalendarIcon, GitCommit, MessageSquare, Square, Trash, X, Zap } from 'lucide-vue-next'
import { PriorityBadge, StatusBadge, BaseButton } from '@/components/base'

const props = defineProps({ tasks: Array, projectId: String, onTasksChange: Function })
const router = useRouter()
const toast  = useToast()
const taskStore = useTaskStore()

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

const typeIcons = {
  Bug:         { icon: Bug,           color: 'text-red-600 dark:text-red-400' },
  Feature:     { icon: Zap,           color: 'text-blue-600 dark:text-blue-400' },
  Task:        { icon: Square,        color: 'text-emerald-600 dark:text-emerald-400' },
  Improvement: { icon: GitCommit,     color: 'text-purple-600 dark:text-purple-400' },
  Other:       { icon: MessageSquare, color: 'text-amber-600 dark:text-amber-400' },
}

const selectedTasks = ref([])
const filters = ref({ status: '', type: '', priority: '', assignee: '' })

const assigneeList = computed(() =>
  [...new Set(props.tasks.map(t => t.assignee?.name).filter(Boolean))]
)

const filteredTasks = computed(() =>
  props.tasks.filter(task => {
    const { status, type, priority, assignee } = filters.value
    return (
      (!status   || task.status        === status)   &&
      (!type     || task.type          === type)     &&
      (!priority || task.priority      === priority) &&
      (!assignee || task.assignee?.name === assignee)
    )
  })
)

const hasFilters = computed(() => Object.values(filters.value).some(Boolean))

function toggleAll() {
  if (selectedTasks.value.length === props.tasks.length) selectedTasks.value = []
  else selectedTasks.value = props.tasks.map(t => t.id)
}

function toggleTask(id) {
  if (selectedTasks.value.includes(id)) selectedTasks.value = selectedTasks.value.filter(i => i !== id)
  else selectedTasks.value.push(id)
}

function resetFilters() {
  filters.value = { status: '', type: '', priority: '', assignee: '' }
}

async function handleStatusChange(taskId, newStatus) {
  const id = toast.info('Updating status...')
  try {
    await taskStore.changeStatus(props.projectId, taskId, newStatus)
    toast.dismiss(id)
    toast.success('Task status updated')
    if (props.onTasksChange) props.onTasksChange()
  } catch (error) {
    toast.dismiss(id)
    toast.error(error?.response?.data?.message || error?.response?.data?.error || error.message)
  }
}

async function handleDelete() {
  if (!confirm('Are you sure you want to delete the selected tasks?')) return
  const id = toast.info('Deleting tasks...')
  try {
    await Promise.all(selectedTasks.value.map(tid => taskStore.deleteTask(props.projectId, tid)))
    selectedTasks.value = []
    toast.dismiss(id)
    toast.success('Tasks deleted')
    if (props.onTasksChange) props.onTasksChange()
  } catch (error) {
    toast.dismiss(id)
    toast.error(error?.response?.data?.error || error.message)
  }
}

const filterSelectCls = [
  'cursor-pointer rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200',
].join(' ')
</script>

<template>
  <div>
    <!-- ── Filters ─────────────────────────────────────────────────────────── -->
    <div class="mb-4 flex flex-wrap items-center gap-2">
      <select v-model="filters.status"   :class="filterSelectCls" aria-label="Filter by status">
        <option value="">All Statuses</option>
        <option value="Backlog">Backlog</option>
        <option value="ToDo">To Do</option>
        <option value="InProgress">In Progress</option>
        <option value="Review">Review</option>
        <option value="Done">Done</option>
      </select>

      <select v-model="filters.type"     :class="filterSelectCls" aria-label="Filter by type">
        <option value="">All Types</option>
        <option value="Task">Task</option>
        <option value="Bug">Bug</option>
        <option value="Feature">Feature</option>
        <option value="Improvement">Improvement</option>
        <option value="Other">Other</option>
      </select>

      <select v-model="filters.priority" :class="filterSelectCls" aria-label="Filter by priority">
        <option value="">All Priorities</option>
        <option value="Low">Low</option>
        <option value="Medium">Medium</option>
        <option value="High">High</option>
      </select>

      <select v-model="filters.assignee" :class="filterSelectCls" aria-label="Filter by assignee">
        <option value="">All Assignees</option>
        <option v-for="name in assigneeList" :key="name" :value="name">{{ name }}</option>
      </select>

      <BaseButton v-if="hasFilters" variant="secondary" size="sm" @click="resetFilters">
        <X class="size-3" aria-hidden="true" /> Reset
      </BaseButton>

      <BaseButton v-if="selectedTasks.length > 0" variant="danger" size="sm" @click="handleDelete">
        <Trash class="size-3" aria-hidden="true" />
        Delete {{ selectedTasks.length > 1 ? `(${selectedTasks.length})` : '' }}
      </BaseButton>
    </div>

    <!-- ── Table + mobile cards container ─────────────────────────────────── -->
    <div class="overflow-auto rounded-lg lg:border lg:border-zinc-200 dark:lg:border-zinc-800">

      <!-- Desktop Table -->
      <div class="hidden lg:block overflow-x-auto">
        <table class="min-w-full text-left text-sm">
          <thead class="bg-zinc-50 text-xs uppercase text-zinc-500 dark:bg-zinc-800/70 dark:text-zinc-400">
            <tr>
              <th class="pl-3 pr-1 py-3">
                <input
                  type="checkbox"
                  :checked="selectedTasks.length === tasks.length && tasks.length > 0"
                  @change="toggleAll"
                  class="size-3"
                  aria-label="Select all tasks"
                />
              </th>
              <th class="px-4 pl-0 py-3">Title</th>
              <th class="px-4 py-3">Type</th>
              <th class="px-4 py-3">Priority</th>
              <th class="px-4 py-3">Status</th>
              <th class="px-4 py-3">Assignee</th>
              <th class="px-4 py-3">Due Date</th>
              <th class="px-4 py-3">Labels</th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-zinc-900">
            <template v-if="filteredTasks.length > 0">
              <tr
                v-for="task in filteredTasks"
                :key="task.id"
                @click="router.push(`/taskDetails?projectId=${task.projectId}&taskId=${task.id}`)"
                class="cursor-pointer border-t border-zinc-200 text-zinc-900 transition-colors hover:bg-zinc-50 dark:border-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-800/50"
              >
                <td @click.stop class="pl-3 pr-1 py-2">
                  <input
                    type="checkbox"
                    :checked="selectedTasks.includes(task.id)"
                    @change="toggleTask(task.id)"
                    class="size-3"
                    :aria-label="`Select ${task.title}`"
                  />
                </td>
                <td class="px-4 pl-0 py-2 font-medium">{{ task.title }}</td>
                <td class="px-4 py-2">
                  <div class="flex items-center gap-1.5">
                    <component
                      v-if="typeIcons[task.type]"
                      :is="typeIcons[task.type].icon"
                      :class="['size-3.5', typeIcons[task.type].color]"
                      aria-hidden="true"
                    />
                    <span :class="['text-xs uppercase', typeIcons[task.type]?.color]">{{ task.type }}</span>
                  </div>
                </td>
                <td class="px-4 py-2">
                  <PriorityBadge v-if="task.priority" :priority="task.priority" variant="badge" />
                </td>
                <td @click.stop class="px-4 py-2">
                  <select
                    :value="task.status"
                    @change="e => handleStatusChange(task.id, e.target.value)"
                    class="cursor-pointer rounded border border-zinc-200 bg-white px-2 py-1 text-sm text-zinc-900 transition focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200"
                  >
                    <option value="Backlog">Backlog</option>
                    <option value="ToDo">To Do</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Review">Review</option>
                    <option value="Done">Done</option>
                  </select>
                </td>
                <td class="px-4 py-2">
                  <div class="flex items-center gap-2">
                    <img v-if="task.assignee?.avatarUrl" :src="task.assignee.avatarUrl" class="size-5 rounded-full" alt="avatar" />
                    <div v-else class="size-5 rounded-full bg-zinc-200 dark:bg-zinc-700" aria-hidden="true" />
                    <span class="text-zinc-600 dark:text-zinc-400">{{ task.assignee?.name || '—' }}</span>
                  </div>
                </td>
                <td class="px-4 py-2">
                  <div class="flex items-center gap-1 text-zinc-500 dark:text-zinc-400">
                    <CalendarIcon class="size-3.5" aria-hidden="true" />
                    {{ task.dueDate ? format(new Date(task.dueDate), 'dd MMM') : '—' }}
                  </div>
                </td>
                <td class="px-4 py-2">
                  <div class="flex flex-wrap gap-1">
                    <span
                      v-for="lbl in (task.labels || []).slice(0, 2)"
                      :key="lbl"
                      :class="['rounded px-1.5 py-0.5 text-[10px]', labelColor(lbl)]"
                    >{{ lbl }}</span>
                    <span v-if="(task.labels || []).length > 2" class="text-xs text-zinc-400">
                      +{{ task.labels.length - 2 }}
                    </span>
                  </div>
                </td>
              </tr>
            </template>
            <tr v-else>
              <td colspan="8" class="py-8 text-center text-sm text-zinc-400 dark:text-zinc-500">
                No tasks match the selected filters.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile Cards -->
      <div class="flex flex-col gap-3 lg:hidden">
        <template v-if="filteredTasks.length > 0">
          <div
            v-for="task in filteredTasks"
            :key="task.id"
            @click="router.push(`/taskDetails?projectId=${task.projectId}&taskId=${task.id}`)"
            class="cursor-pointer rounded-lg border border-zinc-200 bg-white p-4 transition-colors hover:border-zinc-300 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700"
          >
            <div class="mb-2 flex items-start justify-between gap-2">
              <h3 class="text-sm font-semibold text-zinc-900 dark:text-zinc-200">{{ task.title }}</h3>
              <input
                type="checkbox"
                :checked="selectedTasks.includes(task.id)"
                @change.stop="toggleTask(task.id)"
                class="mt-0.5 size-4 flex-shrink-0"
                :aria-label="`Select ${task.title}`"
              />
            </div>

            <div class="mb-2 flex items-center gap-1.5 text-xs">
              <component v-if="typeIcons[task.type]" :is="typeIcons[task.type].icon" :class="['size-3.5', typeIcons[task.type].color]" aria-hidden="true" />
              <span :class="['uppercase', typeIcons[task.type]?.color]">{{ task.type }}</span>
            </div>

            <div class="mb-2 flex items-center gap-2">
              <PriorityBadge v-if="task.priority" :priority="task.priority" variant="badge" />
            </div>

            <div class="mb-2" @click.stop>
              <label class="mb-1 block text-xs text-zinc-500 dark:text-zinc-400">Status</label>
              <select
                :value="task.status"
                @change="e => handleStatusChange(task.id, e.target.value)"
                class="w-full cursor-pointer rounded border border-zinc-200 bg-zinc-50 px-2 py-1 text-sm text-zinc-900 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200"
              >
                <option value="Backlog">Backlog</option>
                <option value="ToDo">To Do</option>
                <option value="InProgress">In Progress</option>
                <option value="Review">Review</option>
                <option value="Done">Done</option>
              </select>
            </div>

            <div class="mb-1.5 flex items-center gap-2 text-sm text-zinc-600 dark:text-zinc-400">
              <img v-if="task.assignee?.avatarUrl" :src="task.assignee.avatarUrl" class="size-5 rounded-full" alt="avatar" />
              <div v-else class="size-5 rounded-full bg-zinc-200 dark:bg-zinc-700" aria-hidden="true" />
              {{ task.assignee?.name || '—' }}
            </div>

            <div class="mb-2 flex items-center gap-1.5 text-sm text-zinc-500 dark:text-zinc-400">
              <CalendarIcon class="size-4" aria-hidden="true" />
              {{ task.dueDate ? format(new Date(task.dueDate), 'dd MMM yyyy') : '—' }}
            </div>

            <div v-if="(task.labels || []).length" class="flex flex-wrap gap-1">
              <span v-for="lbl in task.labels.slice(0, 3)" :key="lbl"
                :class="['rounded px-1.5 py-0.5 text-xs', labelColor(lbl)]">{{ lbl }}</span>
            </div>
          </div>
        </template>
        <p v-else class="py-6 text-center text-sm text-zinc-400 dark:text-zinc-500">
          No tasks match the selected filters.
        </p>
      </div>

    </div>
  </div>
</template>
