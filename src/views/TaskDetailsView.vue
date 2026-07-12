<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTaskStore } from '../stores/taskStore'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useAuthStore } from '../stores/authStore'
import { sprintApi } from '../api/projects'
import { format } from 'date-fns'
import { parseUtc } from '../utils/date'
import { ArrowLeft, CalendarIcon, Check, CheckSquare, Clock, MessageCircle, Pencil, Plus, Square, Trash2, X } from 'lucide-vue-next'
import { StatusBadge, PriorityBadge, BaseButton } from '@/components/base'

const route = useRoute()
const router = useRouter()
const taskStore      = useTaskStore()
const workspaceStore = useWorkspaceStore()
const authStore      = useAuthStore()

const projectId = computed(() => route.query.projectId)
const taskId    = computed(() => route.query.taskId)

const task     = computed(() => taskStore.currentTask)
const comments = computed(() => taskStore.comments)
const subtasks = computed(() => taskStore.subtasks)
const timelogs = computed(() => taskStore.timelogs)

const project     = computed(() => workspaceStore.projects.find(p => p.id === projectId.value))
const teamMembers = computed(() => project.value?.members || [])

const newComment        = ref('')
const newSubtask        = ref('')
const submittingComment = ref(false)
const submittingSubtask = ref(false)
const commentError      = ref('')

// ── @mention ─────────────────────────────────────────────────────────────────
const mentionedUserIds  = ref([])
const mentionQuery      = ref(null)
const mentionStart      = ref(-1)
const commentRef        = ref(null)

const mentionSuggestions = computed(() => {
  if (mentionQuery.value === null) return []
  const q = mentionQuery.value.toLowerCase()
  return (teamMembers.value || []).filter(m => {
    const email = (m.user?.email || '').toLowerCase()
    const name  = (m.user?.name  || '').toLowerCase()
    return email.includes(q) || name.includes(q)
  }).slice(0, 6)
})

function onCommentInput(e) {
  newComment.value = e.target.value
  const val    = e.target.value
  const cursor = e.target.selectionStart
  const before = val.slice(0, cursor)
  const atIdx  = before.lastIndexOf('@')
  if (atIdx !== -1) {
    const fragment = before.slice(atIdx + 1)
    if (!fragment.includes(' ') && !fragment.includes('\n')) {
      mentionQuery.value = fragment
      mentionStart.value = atIdx
      return
    }
  }
  mentionQuery.value = null
}

function selectMention(member) {
  const userId      = member.user?.id || member.userId
  const displayName = member.user?.name || member.user?.email || userId

  const before = newComment.value.slice(0, mentionStart.value)
  const after  = newComment.value.slice(mentionStart.value + 1 + (mentionQuery.value?.length || 0))
  newComment.value = `${before}@${displayName} ${after}`

  if (userId && !mentionedUserIds.value.includes(userId))
    mentionedUserIds.value.push(userId)

  mentionQuery.value = null
  commentRef.value?.focus()
}

const showTimeLog = ref(false)
const timeLogData = ref({ hoursLogged: 1, loggedAt: new Date().toISOString().split('T')[0], description: '' })

// ── Comment edit / delete ─────────────────────────────────────────────────────
const editingCommentId   = ref(null)
const editCommentContent = ref('')
const savingComment      = ref(false)

function startEditComment(c) {
  editingCommentId.value   = c.id
  editCommentContent.value = c.content
}
function cancelEditComment() {
  editingCommentId.value   = null
  editCommentContent.value = ''
}
async function handleUpdateComment(c) {
  if (!editCommentContent.value.trim()) return
  savingComment.value = true
  try {
    await taskStore.updateComment(taskId.value, c.id, editCommentContent.value.trim())
    editingCommentId.value = null
  } catch (err) {
    console.error('Failed to update comment:', err)
  } finally {
    savingComment.value = false
  }
}
async function handleDeleteComment(commentId) {
  if (!confirm('Delete this comment?')) return
  try { await taskStore.deleteComment(taskId.value, commentId) }
  catch (err) { console.error('Failed to delete comment:', err) }
}

// ── Inline edit task ──────────────────────────────────────────────────────────
const isEditing      = ref(false)
const editSaving     = ref(false)
const editError      = ref('')
const sprints        = ref([])
const editData       = ref({})
const labelEditInput = ref('')

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

function addEditLabel() {
  const val = labelEditInput.value.trim()
  if (val && !(editData.value.labels || []).includes(val)) {
    if (!editData.value.labels) editData.value.labels = []
    editData.value.labels.push(val)
  }
  labelEditInput.value = ''
}

function removeEditLabel(idx) {
  editData.value.labels?.splice(idx, 1)
}

async function startEdit() {
  const t = task.value
  editData.value = {
    title:          t.title,
    description:    t.description || '',
    priority:       t.priority,
    assignedTo:     t.assignedTo || t.assignee?.id || '',
    dueDate:        t.dueDate ? t.dueDate.split('T')[0] : '',
    estimatedHours: t.estimatedHours ?? '',
    sprintId:       t.sprintId || '',
    labels:         t.labels ? [...t.labels] : [],
  }
  if (!sprints.value.length && workspaceStore.currentWorkspaceId && projectId.value) {
    try {
      sprints.value = await sprintApi.getAll(workspaceStore.currentWorkspaceId, projectId.value)
    } catch { sprints.value = [] }
  }
  isEditing.value = true
  editError.value = ''
}

function cancelEdit() {
  isEditing.value = false
  editError.value = ''
}

async function handleSaveEdit() {
  editSaving.value = true
  editError.value  = ''
  try {
    await taskStore.updateTask(projectId.value, taskId.value, {
      title:          editData.value.title,
      description:    editData.value.description || null,
      priority:       editData.value.priority,
      assignedTo:     editData.value.assignedTo  || null,
      dueDate:        editData.value.dueDate      || null,
      estimatedHours: editData.value.estimatedHours ? Number(editData.value.estimatedHours) : null,
      sprintId:       editData.value.sprintId    || null,
      clearSprint:    !editData.value.sprintId,
      labels:         editData.value.labels || [],
    })
    isEditing.value = false
  } catch (err) {
    editError.value = err?.response?.data?.error || err?.response?.data?.message || 'Failed to save changes'
  } finally {
    editSaving.value = false
  }
}

// ── On mount ─────────────────────────────────────────────────────────────────
onMounted(async () => {
  if (projectId.value && taskId.value) {
    await Promise.all([
      taskStore.fetchTask(projectId.value, taskId.value),
      taskStore.fetchComments(taskId.value),
      taskStore.fetchSubtasks(taskId.value),
      taskStore.fetchTimelogs(taskId.value),
    ])
  }
})

onUnmounted(() => taskStore.clearCurrentTask())

// ── Comments ──────────────────────────────────────────────────────────────────
async function handleAddComment() {
  if (!newComment.value.trim()) return
  submittingComment.value = true
  commentError.value      = ''
  try {
    await taskStore.addComment(taskId.value, newComment.value.trim(), mentionedUserIds.value)
    newComment.value       = ''
    mentionedUserIds.value = []
    mentionQuery.value     = null
  } catch (err) {
    commentError.value = err?.response?.data?.message || err?.response?.data?.error || 'Failed to post comment. Please try again.'
  } finally {
    submittingComment.value = false
  }
}

// ── Subtasks ──────────────────────────────────────────────────────────────────
async function handleAddSubtask() {
  if (!newSubtask.value.trim()) return
  submittingSubtask.value = true
  try {
    await taskStore.createSubtask(taskId.value, newSubtask.value.trim())
    newSubtask.value = ''
  } catch (err) {
    console.error('Failed to add subtask:', err)
  } finally {
    submittingSubtask.value = false
  }
}

async function handleToggleSubtask(sub) {
  try { await taskStore.toggleSubtask(taskId.value, sub.id, !sub.isCompleted) }
  catch (err) { console.error('Failed to toggle subtask:', err) }
}

async function handleDeleteSubtask(id) {
  try { await taskStore.deleteSubtask(taskId.value, id) }
  catch (err) { console.error('Failed to delete subtask:', err) }
}

// ── Time log ──────────────────────────────────────────────────────────────────
async function handleLogTime() {
  try {
    await taskStore.logTime(taskId.value, {
      hoursLogged: Number(timeLogData.value.hoursLogged),
      loggedAt:    timeLogData.value.loggedAt,
      description: timeLogData.value.description || null,
    })
    showTimeLog.value = false
    timeLogData.value = { hoursLogged: 1, loggedAt: new Date().toISOString().split('T')[0], description: '' }
  } catch (err) {
    console.error('Failed to log time:', err)
  }
}

const totalHours = computed(() => timelogs.value.reduce((sum, l) => sum + (l.hoursLogged ?? l.hours ?? 0), 0))

const inputCls = [
  'w-full mt-1 rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200',
].join(' ')
</script>

<template>
  <!-- Loading -->
  <div v-if="taskStore.taskLoading" class="py-12 text-center text-sm text-zinc-400 dark:text-zinc-500">
    Loading task details…
  </div>

  <!-- Not found -->
  <div v-else-if="!task" class="flex min-h-[60vh] flex-col items-center justify-center gap-3 text-center">
    <p class="text-sm text-zinc-500 dark:text-zinc-400">Task not found.</p>
    <BaseButton variant="secondary" size="sm" @click="router.back()">
      <ArrowLeft class="size-4" aria-hidden="true" /> Go back
    </BaseButton>
  </div>

  <!-- Main content -->
  <div v-else class="mx-auto max-w-6xl text-zinc-900 dark:text-zinc-100">

    <!-- Back -->
    <button
      @click="router.back()"
      class="-ml-1.5 mb-6 flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-zinc-500 transition-colors hover:bg-zinc-100 hover:text-zinc-900 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
    >
      <ArrowLeft class="size-4" aria-hidden="true" /> Back
    </button>

    <div class="flex flex-col gap-6 lg:flex-row">

      <!-- ── Left: Discussion + Subtasks ─────────────────────────────────── -->
      <div class="min-w-0 flex-[2] space-y-6">

        <!-- Discussion -->
        <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900">
          <div class="border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
            <h2 class="flex items-center gap-2 text-sm font-semibold text-zinc-900 dark:text-zinc-100">
              <MessageCircle class="size-4" aria-hidden="true" />
              Discussion
              <span class="font-normal text-zinc-400 dark:text-zinc-500">({{ comments.length }})</span>
            </h2>
          </div>

          <div class="p-5">
            <!-- Comment list -->
            <div class="mb-4 max-h-72 space-y-3 overflow-y-auto pr-1">
              <p v-if="comments.length === 0" class="text-sm text-zinc-400 dark:text-zinc-500">
                No comments yet. Be the first!
              </p>
              <div
                v-for="c in comments"
                :key="c.id"
                class="group rounded-lg border border-zinc-200 p-3 dark:border-zinc-700 dark:bg-zinc-800/50"
              >
                <!-- Author row -->
                <div class="mb-2 flex items-center gap-2 text-sm">
                  <img v-if="c.author?.avatarUrl" :src="c.author.avatarUrl" alt="avatar" class="size-5 rounded-full flex-shrink-0" />
                  <div
                    v-else
                    class="flex size-5 flex-shrink-0 items-center justify-center rounded-full bg-zinc-200 text-xs font-medium text-zinc-600 dark:bg-zinc-700 dark:text-zinc-400"
                  >
                    {{ c.author?.name?.[0] || '?' }}
                  </div>
                  <span class="font-medium text-zinc-900 dark:text-zinc-100">{{ c.author?.name || 'Unknown' }}</span>
                  <span class="text-xs text-zinc-400 dark:text-zinc-500">
                    · {{ format(parseUtc(c.createdAt), 'dd MMM yyyy, HH:mm') }}
                  </span>
                  <!-- Edit/Delete (own comments) -->
                  <div
                    v-if="(c.authorId || c.author?.id) === authStore.user?.id"
                    class="ml-auto flex items-center gap-0.5 opacity-0 transition-opacity group-hover:opacity-100"
                  >
                    <button
                      @click="startEditComment(c)"
                      aria-label="Edit comment"
                      class="rounded p-1 text-zinc-400 transition-colors hover:text-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500"
                    >
                      <Pencil class="size-3.5" aria-hidden="true" />
                    </button>
                    <button
                      @click="handleDeleteComment(c.id)"
                      aria-label="Delete comment"
                      class="rounded p-1 text-zinc-400 transition-colors hover:text-red-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-red-500"
                    >
                      <Trash2 class="size-3.5" aria-hidden="true" />
                    </button>
                  </div>
                </div>

                <!-- Content / inline edit -->
                <p v-if="editingCommentId !== c.id" class="text-sm text-zinc-800 dark:text-zinc-200">{{ c.content }}</p>
                <div v-else class="mt-1 space-y-2">
                  <textarea
                    v-model="editCommentContent"
                    rows="2"
                    class="w-full resize-none rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-sm text-zinc-900 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200"
                  />
                  <div class="flex justify-end gap-2">
                    <BaseButton variant="secondary" size="sm" @click="cancelEditComment">Cancel</BaseButton>
                    <BaseButton
                      variant="primary"
                      size="sm"
                      :disabled="savingComment || !editCommentContent.trim()"
                      :loading="savingComment"
                      @click="handleUpdateComment(c)"
                    >Save</BaseButton>
                  </div>
                </div>
              </div>
            </div>

            <!-- New comment textarea -->
            <div class="relative">
              <textarea
                ref="commentRef"
                :value="newComment"
                @input="onCommentInput"
                @keydown.escape="mentionQuery = null"
                placeholder="Write a comment… type @ to mention someone"
                rows="3"
                class="w-full resize-none rounded-lg border border-zinc-300 bg-white p-3 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200 dark:placeholder:text-zinc-500"
              />
              <!-- @mention dropdown -->
              <div
                v-if="mentionQuery !== null && mentionSuggestions.length > 0"
                class="absolute bottom-full left-0 z-20 mb-1 w-60 overflow-hidden rounded-lg border border-zinc-200 bg-white shadow-lg dark:border-zinc-700 dark:bg-zinc-900"
              >
                <button
                  v-for="m in mentionSuggestions"
                  :key="m.user?.id || m.userId"
                  @mousedown.prevent="selectMention(m)"
                  class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-800"
                >
                  <div class="flex size-6 flex-shrink-0 items-center justify-center rounded-full bg-blue-100 text-xs font-medium text-blue-700 dark:bg-blue-900 dark:text-blue-300">
                    {{ (m.user?.name || m.user?.email || '?')[0].toUpperCase() }}
                  </div>
                  <div class="min-w-0">
                    <p class="truncate font-medium text-zinc-900 dark:text-zinc-100">{{ m.user?.name || m.user?.email }}</p>
                    <p class="truncate text-xs text-zinc-400">{{ m.user?.email }}</p>
                  </div>
                </button>
              </div>
            </div>

            <p v-if="commentError" role="alert" class="mt-2 text-xs text-red-500">{{ commentError }}</p>

            <div class="mt-3 flex items-center justify-between gap-3">
              <span v-if="mentionedUserIds.length > 0" class="text-xs text-blue-500">
                {{ mentionedUserIds.length }} mentioned
              </span>
              <div v-else />
              <BaseButton
                variant="primary"
                size="sm"
                :disabled="submittingComment || !newComment.trim()"
                :loading="submittingComment"
                @click="handleAddComment"
              >
                Post
              </BaseButton>
            </div>
          </div>
        </div>

        <!-- Subtasks -->
        <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900">
          <div class="border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
            <h2 class="flex items-center gap-2 text-sm font-semibold text-zinc-900 dark:text-zinc-100">
              <CheckSquare class="size-4" aria-hidden="true" />
              Subtasks
              <span class="font-normal text-zinc-400 dark:text-zinc-500">
                {{ subtasks.filter(s => s.isCompleted).length }}/{{ subtasks.length }}
              </span>
            </h2>
          </div>

          <div class="p-5">
            <div class="mb-4 space-y-1">
              <p v-if="subtasks.length === 0" class="text-sm text-zinc-400 dark:text-zinc-500">No subtasks yet.</p>
              <div
                v-for="sub in subtasks"
                :key="sub.id"
                class="group flex items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-zinc-50 dark:hover:bg-zinc-800/50"
              >
                <button
                  @click="handleToggleSubtask(sub)"
                  :aria-label="sub.isCompleted ? 'Mark incomplete' : 'Mark complete'"
                  class="flex-shrink-0 text-zinc-400 transition-colors hover:text-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500"
                >
                  <CheckSquare v-if="sub.isCompleted" class="size-4 text-emerald-500" aria-hidden="true" />
                  <Square v-else class="size-4" aria-hidden="true" />
                </button>
                <span :class="['flex-1 text-sm', sub.isCompleted ? 'text-zinc-400 line-through dark:text-zinc-500' : 'text-zinc-900 dark:text-zinc-200']">
                  {{ sub.title }}
                </span>
                <button
                  @click="handleDeleteSubtask(sub.id)"
                  aria-label="Delete subtask"
                  class="flex-shrink-0 rounded p-0.5 text-zinc-300 opacity-0 transition hover:text-red-500 focus-visible:opacity-100 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-red-500 group-hover:opacity-100 dark:text-zinc-600"
                >
                  <Trash2 class="size-3.5" aria-hidden="true" />
                </button>
              </div>
            </div>

            <div class="flex gap-2">
              <input
                v-model="newSubtask"
                @keydown.enter="handleAddSubtask"
                placeholder="Add a subtask…"
                class="min-w-0 flex-1 rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200"
              />
              <BaseButton
                variant="primary"
                size="sm"
                :disabled="!newSubtask.trim() || submittingSubtask"
                :loading="submittingSubtask"
                @click="handleAddSubtask"
                aria-label="Add subtask"
              >
                <Plus class="size-4" aria-hidden="true" />
              </BaseButton>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Right: Task Info + Time Tracking ────────────────────────────── -->
      <div class="flex flex-col gap-6 lg:w-80 lg:flex-shrink-0">

        <!-- Task Info card -->
        <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900">

          <!-- VIEW MODE -->
          <template v-if="!isEditing">
            <div class="flex items-start gap-2 border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
              <h1 class="flex-1 text-base font-semibold leading-snug text-zinc-900 dark:text-zinc-100">{{ task.title }}</h1>
              <button
                @click="startEdit"
                aria-label="Edit task"
                class="flex-shrink-0 rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800"
              >
                <Pencil class="size-3.5" aria-hidden="true" />
              </button>
            </div>

            <div class="space-y-4 p-5">
              <!-- Status + Priority -->
              <div class="flex flex-wrap items-center gap-2">
                <StatusBadge :status="task.status" />
                <PriorityBadge v-if="task.priority" :priority="task.priority" variant="badge" />
              </div>

              <!-- Labels -->
              <div v-if="(task.labels || []).length" class="flex flex-wrap gap-1.5">
                <span
                  v-for="lbl in task.labels"
                  :key="lbl"
                  :class="['rounded px-2 py-0.5 text-xs', labelColor(lbl)]"
                >{{ lbl }}</span>
              </div>

              <!-- Description -->
              <p v-if="task.description" class="text-sm leading-relaxed text-zinc-600 dark:text-zinc-400">
                {{ task.description }}
              </p>

              <div class="border-t border-zinc-100 dark:border-zinc-800" />

              <!-- Meta -->
              <dl class="space-y-2.5 text-sm">
                <div class="flex items-center gap-2 text-zinc-600 dark:text-zinc-400">
                  <img v-if="task.assignee?.avatarUrl" :src="task.assignee.avatarUrl" class="size-5 flex-shrink-0 rounded-full" alt="" />
                  <div v-else class="size-5 flex-shrink-0 rounded-full bg-zinc-200 dark:bg-zinc-700" aria-hidden="true" />
                  <span>{{ task.assignee?.name || task.assignee?.email || 'Unassigned' }}</span>
                </div>
                <div v-if="task.dueDate" class="flex items-center gap-2 text-zinc-500 dark:text-zinc-400">
                  <CalendarIcon class="size-4 flex-shrink-0" aria-hidden="true" />
                  <span>Due {{ format(new Date(task.dueDate), 'dd MMM yyyy') }}</span>
                </div>
                <div v-if="task.estimatedHours" class="flex items-center gap-2 text-zinc-500 dark:text-zinc-400">
                  <Clock class="size-4 flex-shrink-0" aria-hidden="true" />
                  <span>Est. {{ task.estimatedHours }}h</span>
                </div>
                <div v-if="task.subTaskCount > 0" class="flex items-center gap-2 text-zinc-500 dark:text-zinc-400">
                  <CheckSquare class="size-4 flex-shrink-0" aria-hidden="true" />
                  <span>{{ task.completedSubTaskCount }}/{{ task.subTaskCount }} subtasks</span>
                </div>
              </dl>
            </div>
          </template>

          <!-- EDIT MODE -->
          <template v-else>
            <div class="flex items-center justify-between border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
              <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Edit Task</h2>
              <button
                @click="cancelEdit"
                aria-label="Cancel edit"
                class="rounded-lg p-1 text-zinc-400 transition-colors hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-zinc-400 dark:hover:text-zinc-200"
              >
                <X class="size-4" aria-hidden="true" />
              </button>
            </div>

            <div class="space-y-3 p-5">
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Title *</label>
                <input v-model="editData.title" required :class="inputCls" />
              </div>
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Description</label>
                <textarea v-model="editData.description" rows="3" :class="inputCls + ' resize-none'" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Priority</label>
                  <select v-model="editData.priority" :class="inputCls">
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Assignee</label>
                  <select v-model="editData.assignedTo" :class="inputCls">
                    <option value="">Unassigned</option>
                    <option v-for="m in teamMembers" :key="m.userId" :value="m.userId">
                      {{ m.user?.name || m.user?.email || m.userId }}
                    </option>
                  </select>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Due Date</label>
                  <input v-model="editData.dueDate" type="date" :class="inputCls" />
                </div>
                <div>
                  <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Est. Hours</label>
                  <input v-model="editData.estimatedHours" type="number" min="0" step="0.5" :class="inputCls" />
                </div>
              </div>
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Sprint</label>
                <select v-model="editData.sprintId" :class="inputCls">
                  <option value="">Backlog (no sprint)</option>
                  <option v-for="s in sprints" :key="s.id" :value="s.id">
                    {{ s.name }}{{ s.status === 'Active' ? ' ★' : '' }}
                  </option>
                </select>
              </div>

              <!-- Labels -->
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Labels</label>
                <div v-if="(editData.labels || []).length" class="mb-1.5 mt-1 flex flex-wrap gap-1">
                  <span
                    v-for="(lbl, idx) in editData.labels"
                    :key="lbl"
                    :class="['inline-flex items-center gap-1 rounded px-2 py-0.5 text-xs', labelColor(lbl)]"
                  >
                    {{ lbl }}
                    <button type="button" @click="removeEditLabel(idx)" class="leading-none transition-colors hover:text-red-500">&times;</button>
                  </span>
                </div>
                <div class="mt-1 flex gap-2">
                  <input
                    v-model="labelEditInput"
                    @keydown.enter.prevent="addEditLabel"
                    placeholder="Add label…"
                    :class="inputCls + ' flex-1'"
                  />
                  <button
                    type="button"
                    @click="addEditLabel"
                    class="rounded-lg border border-zinc-300 px-3 py-1.5 text-sm transition hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:hover:bg-zinc-800"
                  >+</button>
                </div>
              </div>

              <p v-if="editError" role="alert" class="text-xs text-red-500">{{ editError }}</p>

              <div class="flex justify-end gap-2 pt-1">
                <BaseButton variant="secondary" size="sm" @click="cancelEdit">
                  <X class="size-3.5" aria-hidden="true" /> Cancel
                </BaseButton>
                <BaseButton
                  variant="primary"
                  size="sm"
                  :disabled="editSaving || !editData.title?.trim()"
                  :loading="editSaving"
                  @click="handleSaveEdit"
                >
                  <Check class="size-3.5" aria-hidden="true" /> Save
                </BaseButton>
              </div>
            </div>
          </template>
        </div>

        <!-- Time Tracking card -->
        <div class="overflow-hidden rounded-lg border border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900">
          <div class="flex items-center justify-between border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
            <h2 class="flex items-center gap-2 text-sm font-semibold text-zinc-900 dark:text-zinc-100">
              <Clock class="size-4" aria-hidden="true" /> Time Logged
            </h2>
            <BaseButton variant="ghost" size="sm" @click="showTimeLog = !showTimeLog">
              <Plus class="size-3" aria-hidden="true" /> Log time
            </BaseButton>
          </div>

          <div class="p-5">
            <p class="mb-4 text-2xl font-bold tabular-nums text-zinc-900 dark:text-zinc-100">
              {{ totalHours.toFixed(1) }}h
            </p>

            <!-- Log time form -->
            <div v-if="showTimeLog" class="mb-4 space-y-3 rounded-lg border border-zinc-200 p-4 dark:border-zinc-800">
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Hours</label>
                <input v-model.number="timeLogData.hoursLogged" type="number" min="0.25" step="0.25" :class="inputCls" />
              </div>
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Date</label>
                <input v-model="timeLogData.loggedAt" type="date" :class="inputCls" />
              </div>
              <div>
                <label class="block text-xs font-medium text-zinc-500 dark:text-zinc-400">Note (optional)</label>
                <input v-model="timeLogData.description" placeholder="What did you work on?" :class="inputCls" />
              </div>
              <div class="flex gap-2">
                <BaseButton variant="primary" size="sm" class="flex-1" @click="handleLogTime">Save</BaseButton>
                <BaseButton variant="secondary" size="sm" @click="showTimeLog = false">Cancel</BaseButton>
              </div>
            </div>

            <!-- Log entries -->
            <div v-if="timelogs.length > 0" class="space-y-2">
              <div
                v-for="log in timelogs.slice(0, 5)"
                :key="log.id"
                class="flex items-center justify-between text-xs text-zinc-500 dark:text-zinc-400"
              >
                <span class="truncate">{{ log.description || 'Work logged' }}</span>
                <span class="ml-3 flex-shrink-0 font-medium tabular-nums text-zinc-700 dark:text-zinc-300">
                  {{ (log.hoursLogged ?? log.hours ?? 0) }}h
                </span>
              </div>
            </div>
            <p v-else-if="!showTimeLog" class="text-xs text-zinc-400 dark:text-zinc-500">No time logged yet.</p>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>
