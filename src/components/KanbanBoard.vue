<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTaskStore } from '../stores/taskStore'
import { format } from 'date-fns'
import { CalendarIcon, GripVertical, User } from 'lucide-vue-next'

const props = defineProps({
  tasks:          Array,
  projectId:      String,
  onTasksChange:  Function,
})

const router    = useRouter()
const taskStore = useTaskStore()

const draggedId    = ref(null)
const dragOverCol  = ref(null)
const updatingId   = ref(null)
const error        = ref('')

// STATUS_INT → string (same as taskStore)
const STATUS_STR = { 0: 'Backlog', 1: 'ToDo', 2: 'InProgress', 3: 'Review', 4: 'Done', 5: 'Blocked' }

// Columns from cache (populated by fetchColumns in taskStore)
const columns = computed(() => {
  const cols = taskStore.columnsByProject[props.projectId]
  if (cols && cols.length) return [...cols].sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
  // Fallback before columns load
  return [
    { id: '__Backlog',    name: 'Backlog',     defaultStatus: 0 },
    { id: '__ToDo',       name: 'To Do',       defaultStatus: 1 },
    { id: '__InProgress', name: 'In Progress', defaultStatus: 2 },
    { id: '__Review',     name: 'Review',      defaultStatus: 3 },
    { id: '__Done',       name: 'Done',        defaultStatus: 4 },
  ]
})

function colTasks(col) {
  const statusStr = STATUS_STR[col.defaultStatus]
  return (props.tasks || []).filter(t => t.status === statusStr)
}

// ── Drag & Drop ───────────────────────────────────────────────────────────────
function onDragStart(e, task) {
  draggedId.value = task.id
  e.dataTransfer.effectAllowed = 'move'
}

function onDragOver(e, colId) {
  e.preventDefault()
  e.dataTransfer.dropEffect = 'move'
  dragOverCol.value = colId
}

function onDragLeave(colId) {
  if (dragOverCol.value === colId) dragOverCol.value = null
}

async function onDrop(col) {
  const taskId = draggedId.value
  draggedId.value  = null
  dragOverCol.value = null
  if (!taskId) return

  const targetStatus = STATUS_STR[col.defaultStatus]
  const task = (props.tasks || []).find(t => t.id === taskId)
  if (!task || task.status === targetStatus) return

  error.value    = ''
  updatingId.value = taskId
  try {
    await taskStore.changeStatus(props.projectId, taskId, targetStatus)
    if (props.onTasksChange) props.onTasksChange()
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to move task'
  } finally {
    updatingId.value = null
  }
}

function onDragEnd() {
  draggedId.value  = null
  dragOverCol.value = null
}

onMounted(() => {
  taskStore.fetchColumns(props.projectId)
})

// ── Column header accents ────────────────────────────────────────────────────
const colAccent = {
  0: 'border-t-2 border-zinc-400',
  1: 'border-t-2 border-blue-400',
  2: 'border-t-2 border-amber-400',
  3: 'border-t-2 border-purple-400',
  4: 'border-t-2 border-emerald-500',
  5: 'border-t-2 border-red-400',
}

// ── Priority badge ────────────────────────────────────────────────────────────
const priorityCls = {
  High:   'bg-red-100 text-red-700 dark:bg-red-900/60 dark:text-red-300',
  Medium: 'bg-amber-100 text-amber-700 dark:bg-amber-900/60 dark:text-amber-300',
  Low:    'bg-zinc-100 text-zinc-500 dark:bg-zinc-700 dark:text-zinc-400',
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
</script>

<template>
  <div>
    <p v-if="error" role="alert" class="mb-3 text-sm text-red-500">{{ error }}</p>

    <!-- Horizontal scroll wrapper -->
    <div class="overflow-x-auto pb-2">
      <div class="flex gap-3 min-w-max">
        <div
          v-for="col in columns"
          :key="col.id"
          class="flex flex-col w-60 rounded-lg bg-zinc-50 dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800"
          :class="[colAccent[col.defaultStatus], dragOverCol === col.id ? 'ring-2 ring-blue-400' : '']"
          @dragover="onDragOver($event, col.id)"
          @dragleave="onDragLeave(col.id)"
          @drop="onDrop(col)"
        >
          <!-- Column header -->
          <div class="flex items-center justify-between px-3 py-2.5">
            <span class="text-xs font-semibold uppercase tracking-wider text-zinc-500 dark:text-zinc-400">{{ col.name }}</span>
            <span class="text-xs text-zinc-400 dark:text-zinc-500 bg-zinc-200 dark:bg-zinc-700 rounded px-1.5 py-0.5">
              {{ colTasks(col).length }}
            </span>
          </div>

          <!-- Task cards -->
          <div class="flex flex-col gap-2 px-2 pb-3 min-h-20 flex-1">
            <div
              v-for="task in colTasks(col)"
              :key="task.id"
              draggable="true"
              tabindex="0"
              @dragstart="onDragStart($event, task)"
              @dragend="onDragEnd"
              @click="router.push(`/taskDetails?projectId=${task.projectId}&taskId=${task.id}`)"
              @keydown.enter="router.push(`/taskDetails?projectId=${task.projectId}&taskId=${task.id}`)"
              :aria-label="task.title"
              :class="[
                'group relative bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-lg p-3 cursor-grab active:cursor-grabbing hover:shadow-md transition-shadow select-none',
                updatingId === task.id ? 'opacity-40 pointer-events-none' : '',
                draggedId === task.id  ? 'opacity-50 ring-2 ring-blue-400' : '',
              ]"
            >
              <!-- Drag handle hint -->
              <GripVertical class="absolute right-2 top-3 size-3.5 text-zinc-300 dark:text-zinc-600 opacity-0 group-hover:opacity-100 transition-opacity" />

              <!-- Title -->
              <p class="text-sm text-zinc-900 dark:text-zinc-100 font-medium leading-snug pr-5 mb-2">{{ task.title }}</p>

              <!-- Priority + Labels -->
              <div class="flex items-center gap-2 flex-wrap">
                <span v-if="task.priority" :class="['text-xs px-1.5 py-0.5 rounded', priorityCls[task.priority] || priorityCls.Low]">
                  {{ task.priority }}
                </span>
                <span v-for="lbl in (task.labels || []).slice(0, 2)" :key="lbl"
                  :class="['text-[10px] px-1.5 py-0.5 rounded', labelColor(lbl)]">{{ lbl }}</span>
                <span v-if="(task.labels || []).length > 2" class="text-[10px] text-zinc-400">
                  +{{ task.labels.length - 2 }}
                </span>
              </div>

              <!-- Footer: assignee + due date -->
              <div class="flex items-center justify-between mt-2 gap-1">
                <div class="flex items-center gap-1">
                  <img v-if="task.assignee?.avatarUrl" :src="task.assignee.avatarUrl" class="size-5 rounded-full" alt="avatar" />
                  <div v-else-if="task.assignedTo" class="size-5 rounded-full bg-blue-200 dark:bg-blue-800 flex items-center justify-center">
                    <User class="size-3 text-blue-600 dark:text-blue-300" />
                  </div>
                  <span v-if="task.assignee?.name" class="text-xs text-zinc-400 dark:text-zinc-500 max-w-[80px] truncate">
                    {{ task.assignee.name }}
                  </span>
                </div>
                <div v-if="task.dueDate" class="flex items-center gap-0.5 text-xs text-zinc-400 dark:text-zinc-500">
                  <CalendarIcon class="size-3" />
                  {{ format(new Date(task.dueDate), 'dd MMM') }}
                </div>
              </div>

              <!-- Subtask progress bar -->
              <div v-if="task.subTaskCount > 0" class="mt-2">
                <div class="flex justify-between text-xs text-zinc-400 mb-0.5">
                  <span>{{ task.completedSubTaskCount }}/{{ task.subTaskCount }} subtasks</span>
                </div>
                <div class="h-1 bg-zinc-200 dark:bg-zinc-700 rounded-full overflow-hidden">
                  <div
                    class="h-full bg-emerald-500 rounded-full transition-all"
                    :style="{ width: `${Math.round((task.completedSubTaskCount / task.subTaskCount) * 100)}%` }"
                  />
                </div>
              </div>
            </div>

            <!-- Drop zone hint when empty -->
            <div
              v-if="colTasks(col).length === 0"
              class="flex-1 flex items-center justify-center min-h-16 border-2 border-dashed border-zinc-200 dark:border-zinc-700 rounded-lg"
              :class="dragOverCol === col.id ? 'border-blue-400 bg-blue-50 dark:bg-blue-950/20' : ''"
            >
              <span class="text-xs text-zinc-300 dark:text-zinc-600">Drop here</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
