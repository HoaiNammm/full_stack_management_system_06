<script setup>
import { ref, computed, watch } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useTaskStore } from '../stores/taskStore'
import { sprintApi } from '../api/projects'
import { X } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

const props = defineProps({ show: Boolean, projectId: String, defaultSprintId: { type: String, default: null } })
const emit  = defineEmits(['close'])

const workspaceStore = useWorkspaceStore()
const taskStore      = useTaskStore()
const project        = computed(() => workspaceStore.projects.find(p => p.id === props.projectId))
const teamMembers    = computed(() => project.value?.members || [])

const isSubmitting = ref(false)
const sprints      = ref([])
const labelInput   = ref('')

const formData = ref({
  title: '', description: '', status: 'ToDo', priority: 'Medium',
  assignedTo: '', dueDate: '', estimatedHours: '', sprintId: '', labels: [],
})

function addLabel() {
  const val = labelInput.value.trim()
  if (val && !formData.value.labels.includes(val)) formData.value.labels.push(val)
  labelInput.value = ''
}

function removeLabel(idx) {
  formData.value.labels.splice(idx, 1)
}

watch(() => props.show, async (open) => {
  if (!open || !props.projectId || !workspaceStore.currentWorkspaceId) return
  try {
    const all = await sprintApi.getAll(workspaceStore.currentWorkspaceId, props.projectId)
    sprints.value = (all || []).filter(s => s.status !== 'Completed')
  } catch {
    sprints.value = []
  }
  formData.value.sprintId = props.defaultSprintId || ''
}, { immediate: true })

async function handleSubmit() {
  if (!formData.value.title.trim()) return
  isSubmitting.value = true
  try {
    await taskStore.createTask(props.projectId, {
      title:          formData.value.title,
      description:    formData.value.description || null,
      status:         formData.value.status,
      priority:       formData.value.priority,
      assignedTo:     formData.value.assignedTo    || null,
      dueDate:        formData.value.dueDate        || null,
      estimatedHours: formData.value.estimatedHours ? Number(formData.value.estimatedHours) : null,
      sprintId:       formData.value.sprintId       || null,
      labels:         formData.value.labels,
    })
    formData.value = { title: '', description: '', status: 'ToDo', priority: 'Medium', assignedTo: '', dueDate: '', estimatedHours: '', sprintId: '', labels: [] }
    labelInput.value = ''
    emit('close')
  } catch (err) {
    console.error('Failed to create task:', err)
  } finally {
    isSubmitting.value = false
  }
}

const today = new Date().toISOString().split('T')[0]

const fieldCls = [
  'w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200',
].join(' ')
</script>

<template>
  <div
    v-if="show"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/25 p-4 backdrop-blur-sm dark:bg-black/60"
    @click.self="emit('close')"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="create-task-heading"
      class="flex max-h-[90vh] w-full max-w-lg flex-col overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-xl dark:border-zinc-800 dark:bg-zinc-950"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-zinc-200 px-6 py-4 dark:border-zinc-800">
        <h2 id="create-task-heading" class="text-base font-semibold text-zinc-900 dark:text-zinc-100">Create New Task</h2>
        <button
          @click="emit('close')"
          aria-label="Close dialog"
          class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
        >
          <X class="size-4" aria-hidden="true" />
        </button>
      </div>

      <!-- Body (scrollable) -->
      <div class="flex-1 overflow-y-auto px-6 py-5">
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <!-- Title -->
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Title *</label>
            <input v-model="formData.title" placeholder="Task title" required :class="fieldCls" />
          </div>

          <!-- Description -->
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Description</label>
            <textarea v-model="formData.description" placeholder="Describe the task" :class="fieldCls + ' h-20 resize-none'" />
          </div>

          <!-- Priority + Status -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Priority</label>
              <select v-model="formData.priority" :class="fieldCls + ' cursor-pointer'">
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Status</label>
              <select v-model="formData.status" :class="fieldCls + ' cursor-pointer'">
                <option value="Backlog">Backlog</option>
                <option value="ToDo">To Do</option>
                <option value="InProgress">In Progress</option>
                <option value="Review">Review</option>
                <option value="Done">Done</option>
              </select>
            </div>
          </div>

          <!-- Assignee + Due Date -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Assignee</label>
              <select v-model="formData.assignedTo" :class="fieldCls + ' cursor-pointer'">
                <option value="">Unassigned</option>
                <option v-for="member in teamMembers" :key="member.userId" :value="member.userId">
                  {{ member.user?.name || member.user?.email || member.userId }}
                </option>
              </select>
            </div>
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Due Date</label>
              <input v-model="formData.dueDate" type="date" :min="today" :class="fieldCls" />
            </div>
          </div>

          <!-- Sprint + Estimated Hours -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Sprint</label>
              <select v-model="formData.sprintId" :class="fieldCls + ' cursor-pointer'">
                <option value="">Backlog (no sprint)</option>
                <option v-for="sprint in sprints" :key="sprint.id" :value="sprint.id">
                  {{ sprint.name }}{{ sprint.status === 'Active' ? ' ★' : '' }}
                </option>
              </select>
            </div>
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Est. Hours</label>
              <input v-model="formData.estimatedHours" type="number" min="0" step="0.5" placeholder="e.g. 4" :class="fieldCls + ' tabular-nums'" />
            </div>
          </div>

          <!-- Labels -->
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Labels</label>
            <div v-if="formData.labels.length" class="mb-1.5 flex flex-wrap gap-1">
              <span
                v-for="(lbl, idx) in formData.labels"
                :key="lbl"
                class="inline-flex items-center gap-1 rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-700 dark:bg-blue-900/40 dark:text-blue-300"
              >
                {{ lbl }}
                <button type="button" @click="removeLabel(idx)" class="leading-none transition hover:text-red-500" :aria-label="`Remove label ${lbl}`">&times;</button>
              </span>
            </div>
            <div class="flex gap-2">
              <input
                v-model="labelInput"
                @keydown.enter.prevent="addLabel"
                placeholder="Type label and press Enter"
                :class="fieldCls + ' flex-1'"
              />
              <BaseButton type="button" variant="secondary" size="sm" @click="addLabel">+</BaseButton>
            </div>
          </div>
        </form>
      </div>

      <!-- Footer -->
      <div class="flex justify-end gap-2 border-t border-zinc-200 px-6 py-4 dark:border-zinc-800">
        <BaseButton type="button" variant="secondary" size="sm" @click="emit('close')">Cancel</BaseButton>
        <BaseButton
          type="button"
          variant="primary"
          size="sm"
          :loading="isSubmitting"
          :disabled="isSubmitting || !formData.title.trim()"
          @click="handleSubmit"
        >
          Create Task
        </BaseButton>
      </div>
    </div>
  </div>
</template>
