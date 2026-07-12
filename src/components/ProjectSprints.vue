<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { format } from 'date-fns'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { sprintApi } from '../api/projects'
import {
  Plus, Play, CheckCircle, Trash2, GitBranch, Pencil, Check, X,
  ChevronDown, ChevronRight, Circle, Clock, AlertCircle, CheckCircle2,
} from 'lucide-vue-next'
import CreateTaskDialog from './CreateTaskDialog.vue'

const props = defineProps({ project: Object })
const router         = useRouter()
const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()

const sprints      = ref([])
const loading      = ref(false)
const error        = ref('')
const showCreate   = ref(false)
const submitting   = ref(false)
const expandedIds  = ref(new Set())          // which sprints are expanded
const addingTask   = ref(null)               // sprintId currently showing create dialog

const newSprint    = ref({ name: '', goal: '', startDate: '', endDate: '' })
const workspaceId  = computed(() => workspaceStore.currentWorkspaceId)
const todayStr     = new Date().toISOString().split('T')[0]

// ── Edit state ────────────────────────────────────────────────────────────────
const editingId   = ref(null)
const editData    = ref({})
const editSaving  = ref(false)
const editError   = ref('')

const statusColors = {
  Planning:  'bg-zinc-100 text-zinc-700 dark:bg-zinc-700 dark:text-zinc-300',
  Active:    'bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300',
  Completed: 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
}

const taskStatusIcon = {
  Backlog:    Circle,
  ToDo:       Circle,
  InProgress: Clock,
  Review:     AlertCircle,
  Done:       CheckCircle2,
  Blocked:    AlertCircle,
}
const taskStatusCls = {
  Backlog:    'text-zinc-400',
  ToDo:       'text-blue-400',
  InProgress: 'text-amber-400',
  Review:     'text-purple-400',
  Done:       'text-emerald-500',
  Blocked:    'text-red-400',
}

onMounted(loadSprints)

async function loadSprints() {
  if (!props.project) return
  loading.value = true
  error.value   = ''
  try {
    const data = await sprintApi.getAll(workspaceId.value, props.project.id)
    sprints.value = Array.isArray(data) ? data : []
    // Auto-expand active sprint
    const active = sprints.value.find(s => s.status === 'Active')
    if (active) expandedIds.value.add(active.id)
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to load sprints'
  } finally {
    loading.value = false
  }
}

function sprintTasks(sprintId) {
  return (taskStore.getProjectTasks(props.project?.id) || []).filter(t => t.sprintId === sprintId)
}

function toggleExpand(sprintId) {
  if (expandedIds.value.has(sprintId)) expandedIds.value.delete(sprintId)
  else expandedIds.value.add(sprintId)
  // trigger reactivity
  expandedIds.value = new Set(expandedIds.value)
}

function isExpanded(sprintId) {
  return expandedIds.value.has(sprintId)
}

// ── Create sprint ─────────────────────────────────────────────────────────────
async function handleCreate() {
  if (!newSprint.value.name.trim() || !newSprint.value.startDate) return
  if (newSprint.value.startDate < todayStr) {
    error.value = 'Sprint start date cannot be in the past.'
    return
  }
  submitting.value = true
  error.value      = ''
  try {
    const sprint = await sprintApi.create(workspaceId.value, props.project.id, {
      name:      newSprint.value.name,
      goal:      newSprint.value.goal || null,
      startDate: newSprint.value.startDate,
      endDate:   newSprint.value.endDate || null,
    })
    sprints.value.push(sprint)
    newSprint.value = { name: '', goal: '', startDate: '', endDate: '' }
    showCreate.value = false
    expandedIds.value = new Set([...expandedIds.value, sprint.id])
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to create sprint'
  } finally {
    submitting.value = false
  }
}

// ── Start / Complete / Delete ─────────────────────────────────────────────────
async function handleStart(sprint) {
  error.value = ''
  try {
    const updated = await sprintApi.start(workspaceId.value, props.project.id, sprint.id)
    const idx = sprints.value.findIndex(s => s.id === sprint.id)
    if (idx !== -1) sprints.value[idx] = { ...sprints.value[idx], ...updated }
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to start sprint'
  }
}

async function handleComplete(sprint) {
  error.value = ''
  try {
    const updated = await sprintApi.complete(workspaceId.value, props.project.id, sprint.id)
    const idx = sprints.value.findIndex(s => s.id === sprint.id)
    if (idx !== -1) sprints.value[idx] = { ...sprints.value[idx], ...updated }
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to complete sprint'
  }
}

async function handleDelete(sprint) {
  if (!confirm(`Delete sprint "${sprint.name}"?`)) return
  error.value = ''
  try {
    await sprintApi.delete(workspaceId.value, props.project.id, sprint.id)
    sprints.value = sprints.value.filter(s => s.id !== sprint.id)
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to delete sprint'
  }
}

// ── Inline Sprint Edit ────────────────────────────────────────────────────────
function startEdit(sprint) {
  editingId.value = sprint.id
  editError.value = ''
  editData.value  = {
    name:      sprint.name,
    goal:      sprint.goal || '',
    startDate: sprint.startDate ? sprint.startDate.split('T')[0] : '',
    endDate:   sprint.endDate && !sprint.endDate.startsWith('0001')
                 ? sprint.endDate.split('T')[0] : '',
  }
}

function cancelEdit() {
  editingId.value = null
  editError.value = ''
}

async function handleSaveEdit(sprint) {
  if (!editData.value.name.trim() || !editData.value.startDate) return
  editSaving.value = true
  editError.value  = ''
  try {
    const updated = await sprintApi.update(workspaceId.value, props.project.id, sprint.id, {
      name:      editData.value.name.trim(),
      goal:      editData.value.goal || null,
      startDate: editData.value.startDate,
      endDate:   editData.value.endDate || null,
    })
    const idx = sprints.value.findIndex(s => s.id === sprint.id)
    if (idx !== -1) sprints.value[idx] = { ...sprints.value[idx], ...updated }
    editingId.value = null
  } catch (e) {
    editError.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to save sprint'
  } finally {
    editSaving.value = false
  }
}

// ── Task creation from sprint ─────────────────────────────────────────────────
function openAddTask(sprintId) {
  addingTask.value = sprintId
}

function onTaskCreated() {
  addingTask.value = null
  // tasks are updated inside CreateTaskDialog via taskStore
}

// ── Utils ─────────────────────────────────────────────────────────────────────
function fmtDate(d) {
  if (!d || d.startsWith('0001')) return '—'
  try { return format(new Date(d), 'dd MMM yyyy') } catch { return '—' }
}

function goToTask(task) {
  router.push(`/taskDetails?projectId=${task.projectId || props.project?.id}&taskId=${task.id}`)
}

function doneCount(tasks) { return tasks.filter(t => t.status === 'Done').length }

const inputCls = 'w-full px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200 focus:outline-none focus:ring-1 focus:ring-blue-500'
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h2 class="text-base font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
        <GitBranch class="size-4" /> Sprints ({{ sprints.length }})
      </h2>
      <button @click="showCreate = !showCreate"
        class="flex items-center gap-2 px-4 py-1.5 text-sm rounded bg-gradient-to-br from-blue-500 to-blue-600 text-white">
        <Plus class="size-4" /> New Sprint
      </button>
    </div>

    <p v-if="error" class="text-sm text-red-500">{{ error }}</p>

    <!-- Create form -->
    <div v-if="showCreate" class="border border-zinc-300 dark:border-zinc-700 rounded-lg p-4 space-y-3 bg-white dark:bg-zinc-900">
      <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-200">New Sprint</h3>
      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">Sprint Name *</label>
          <input v-model="newSprint.name" placeholder="Sprint 1" :class="'mt-1 ' + inputCls" />
        </div>
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">Start Date *</label>
          <input v-model="newSprint.startDate" type="date" :min="todayStr" :class="'mt-1 ' + inputCls" />
        </div>
      </div>
      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">End Date</label>
          <input v-model="newSprint.endDate" type="date" :min="newSprint.startDate || todayStr" :class="'mt-1 ' + inputCls" />
        </div>
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">Sprint Goal</label>
          <input v-model="newSprint.goal" placeholder="What will be achieved?" :class="'mt-1 ' + inputCls" />
        </div>
      </div>
      <div class="flex gap-2 justify-end">
        <button @click="showCreate = false" class="px-4 py-1.5 text-sm border border-zinc-300 dark:border-zinc-700 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">Cancel</button>
        <button @click="handleCreate" :disabled="submitting || !newSprint.name.trim() || !newSprint.startDate"
          class="px-4 py-1.5 text-sm bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600">
          {{ submitting ? 'Creating...' : 'Create Sprint' }}
        </button>
      </div>
    </div>

    <!-- Sprint list -->
    <div v-if="loading" class="text-sm text-zinc-500 dark:text-zinc-400 py-4">Loading sprints...</div>
    <div v-else-if="sprints.length === 0 && !showCreate" class="text-sm text-zinc-500 dark:text-zinc-400 py-8 text-center">
      No sprints yet. Create your first sprint to get started.
    </div>

    <div v-else class="space-y-3">
      <div v-for="sprint in sprints" :key="sprint.id"
        class="border border-zinc-300 dark:border-zinc-700 rounded-lg bg-white dark:bg-zinc-900 overflow-hidden">

        <!-- ── View mode header ─────────────────────────────────────────────── -->
        <template v-if="editingId !== sprint.id">
          <!-- Clickable header row -->
          <div
            @click="toggleExpand(sprint.id)"
            class="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-zinc-50 dark:hover:bg-zinc-800/60 transition-colors select-none"
          >
            <!-- Chevron -->
            <component :is="isExpanded(sprint.id) ? ChevronDown : ChevronRight"
              class="size-4 text-zinc-400 dark:text-zinc-500 flex-shrink-0" />

            <!-- Sprint info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-sm font-medium text-zinc-900 dark:text-zinc-100">{{ sprint.name }}</span>
                <span :class="['px-2 py-0.5 text-xs rounded', statusColors[sprint.status] || statusColors.Planning]">
                  {{ sprint.status || 'Planning' }}
                </span>
                <!-- Task progress pill -->
                <span v-if="sprintTasks(sprint.id).length > 0"
                  class="text-xs text-zinc-500 dark:text-zinc-400 bg-zinc-100 dark:bg-zinc-800 px-2 py-0.5 rounded">
                  {{ doneCount(sprintTasks(sprint.id)) }}/{{ sprintTasks(sprint.id).length }} done
                </span>
              </div>
              <div class="flex items-center gap-3 mt-0.5">
                <span v-if="sprint.goal" class="text-xs text-zinc-500 dark:text-zinc-400 truncate max-w-xs">
                  {{ sprint.goal }}
                </span>
                <span class="text-xs text-zinc-400 dark:text-zinc-500">
                  {{ fmtDate(sprint.startDate) }} → {{ fmtDate(sprint.endDate) }}
                </span>
              </div>
            </div>

            <!-- Action buttons (stop propagation so they don't toggle expand) -->
            <div class="flex items-center gap-1.5 flex-shrink-0" @click.stop>
              <button @click="startEdit(sprint)"
                class="p-1.5 text-zinc-400 hover:text-blue-500 transition-colors" title="Edit sprint">
                <Pencil class="size-3.5" />
              </button>
              <button v-if="!sprint.status || sprint.status === 'Planning'" @click="handleStart(sprint)"
                class="flex items-center gap-1 px-3 py-1 text-xs bg-emerald-500 text-white rounded hover:bg-emerald-600">
                <Play class="size-3" /> Start
              </button>
              <button v-if="sprint.status === 'Active'" @click="handleComplete(sprint)"
                class="flex items-center gap-1 px-3 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600">
                <CheckCircle class="size-3" /> Complete
              </button>
              <button v-if="sprint.status !== 'Active'" @click="handleDelete(sprint)"
                class="p-1 text-zinc-400 hover:text-red-500 transition-colors">
                <Trash2 class="size-3.5" />
              </button>
            </div>
          </div>

          <!-- Progress bar (always visible) -->
          <div v-if="sprintTasks(sprint.id).length > 0"
            class="h-0.5 bg-zinc-100 dark:bg-zinc-800 mx-4">
            <div class="h-full bg-emerald-500 transition-all rounded"
              :style="{ width: `${Math.round(doneCount(sprintTasks(sprint.id)) / sprintTasks(sprint.id).length * 100)}%` }" />
          </div>

          <!-- ── Expanded task list ──────────────────────────────────────────── -->
          <div v-if="isExpanded(sprint.id)" class="border-t border-zinc-100 dark:border-zinc-800">
            <!-- Task rows -->
            <div v-if="sprintTasks(sprint.id).length > 0" class="divide-y divide-zinc-100 dark:divide-zinc-800">
              <div
                v-for="task in sprintTasks(sprint.id)" :key="task.id"
                @click="goToTask(task)"
                class="flex items-center gap-3 px-4 py-2.5 hover:bg-zinc-50 dark:hover:bg-zinc-800/60 cursor-pointer group transition-colors"
              >
                <!-- Status icon -->
                <component :is="taskStatusIcon[task.status] || Circle"
                  :class="['size-3.5 flex-shrink-0', taskStatusCls[task.status] || 'text-zinc-400']" />
                <!-- Title -->
                <span class="flex-1 text-sm text-zinc-900 dark:text-zinc-200 truncate group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                  {{ task.title }}
                </span>
                <!-- Priority -->
                <span v-if="task.priority" :class="['text-xs px-1.5 py-0.5 rounded flex-shrink-0', {
                  'bg-red-100 text-red-600 dark:bg-red-900/40 dark:text-red-300':    task.priority === 'High',
                  'bg-amber-100 text-amber-600 dark:bg-amber-900/40 dark:text-amber-300': task.priority === 'Medium',
                  'bg-zinc-100 text-zinc-500 dark:bg-zinc-700 dark:text-zinc-400':   task.priority === 'Low',
                }]">{{ task.priority }}</span>
                <!-- Assignee -->
                <img v-if="task.assignee?.avatarUrl" :src="task.assignee.avatarUrl"
                  class="size-5 rounded-full flex-shrink-0" alt="avatar" />
              </div>
            </div>
            <p v-else class="px-4 py-3 text-xs text-zinc-400 dark:text-zinc-500 italic">
              No tasks in this sprint yet.
            </p>

            <!-- Add task to sprint button -->
            <div class="px-4 py-2.5 border-t border-zinc-100 dark:border-zinc-800">
              <button
                @click="openAddTask(sprint.id)"
                class="flex items-center gap-1.5 text-xs text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300 font-medium transition-colors"
              >
                <Plus class="size-3.5" /> Add task to this sprint
              </button>
            </div>
          </div>
        </template>

        <!-- ── Edit mode ──────────────────────────────────────────────────────── -->
        <template v-else>
          <div class="p-4 space-y-3">
            <div class="flex items-center justify-between gap-2">
              <p class="text-xs font-medium text-zinc-500 dark:text-zinc-400 uppercase tracking-wide">Editing sprint</p>
              <button @click="cancelEdit" class="text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200">
                <X class="size-4" />
              </button>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">Name *</label>
                <input v-model="editData.name" :class="'mt-1 ' + inputCls" @keydown.enter="handleSaveEdit(sprint)" />
              </div>
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">Goal</label>
                <input v-model="editData.goal" :class="'mt-1 ' + inputCls" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">Start Date *</label>
                <input v-model="editData.startDate" type="date" :class="'mt-1 ' + inputCls" />
              </div>
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">End Date</label>
                <input v-model="editData.endDate" type="date" :min="editData.startDate" :class="'mt-1 ' + inputCls" />
              </div>
            </div>
            <p v-if="editError" class="text-xs text-red-500">{{ editError }}</p>
            <div class="flex gap-2 justify-end">
              <button @click="cancelEdit" class="px-3 py-1.5 text-sm border border-zinc-300 dark:border-zinc-700 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">
                Cancel
              </button>
              <button @click="handleSaveEdit(sprint)" :disabled="editSaving || !editData.name.trim() || !editData.startDate"
                class="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600">
                <Check class="size-3.5" /> {{ editSaving ? 'Saving…' : 'Save' }}
              </button>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- CreateTask dialog with sprint pre-selected -->
    <CreateTaskDialog
      v-if="addingTask"
      :show="!!addingTask"
      :projectId="project?.id"
      :defaultSprintId="addingTask"
      @close="onTaskCreated"
    />
  </div>
</template>
