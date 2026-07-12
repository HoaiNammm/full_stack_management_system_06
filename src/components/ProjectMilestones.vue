<script setup>
import { ref, computed, onMounted } from 'vue'
import { format } from 'date-fns'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { milestoneApi } from '../api/projects'
import { Flag, Plus, Pencil, Trash2, Check, X } from 'lucide-vue-next'

const props = defineProps({ project: Object })
const workspaceStore = useWorkspaceStore()
const wid = computed(() => workspaceStore.currentWorkspaceId)
const pid = computed(() => props.project?.id)

const milestones = ref([])
const loading    = ref(false)
const error      = ref('')
const showCreate = ref(false)
const submitting = ref(false)
const editingId  = ref(null)

const blank = () => ({ name: '', description: '', dueDate: '', status: 'NotStarted' })
const form  = ref(blank())
const editForm = ref(blank())
const todayStr = new Date().toISOString().split('T')[0]

const statusMeta = {
  NotStarted: { label: 'Not Started', cls: 'bg-zinc-200 text-zinc-700 dark:bg-zinc-700 dark:text-zinc-300' },
  InProgress: { label: 'In Progress', cls: 'bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300' },
  Completed:  { label: 'Completed',   cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300' },
  Overdue:    { label: 'Overdue',     cls: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300' },
}

onMounted(load)

async function load() {
  if (!wid.value || !pid.value) return
  loading.value = true
  error.value   = ''
  try {
    milestones.value = await milestoneApi.getAll(wid.value, pid.value) || []
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to load milestones'
  } finally {
    loading.value = false
  }
}

async function handleCreate() {
  if (!form.value.name.trim()) return
  if (form.value.dueDate && form.value.dueDate < todayStr) {
    error.value = 'Milestone due date cannot be in the past.'
    return
  }
  submitting.value = true
  error.value      = ''
  try {
    const m = await milestoneApi.create(wid.value, pid.value, {
      name:        form.value.name,
      description: form.value.description || null,
      dueDate:     form.value.dueDate || null,
      status:      form.value.status,
    })
    milestones.value.push(m)
    form.value  = blank()
    showCreate.value = false
  } catch (e) {
    error.value = e?.response?.data?.error || e?.response?.data?.message || 'Failed to create milestone'
  } finally {
    submitting.value = false
  }
}

function startEdit(m) {
  editingId.value = m.id
  editForm.value  = {
    name:        m.name,
    description: m.description || '',
    dueDate:     m.dueDate ? m.dueDate.split('T')[0] : '',
    status:      m.status || 'NotStarted',
  }
}

function cancelEdit() {
  editingId.value = null
}

async function handleUpdate(m) {
  submitting.value = true
  error.value      = ''
  try {
    const updated = await milestoneApi.update(wid.value, pid.value, m.id, {
      name:        editForm.value.name,
      description: editForm.value.description || null,
      dueDate:     editForm.value.dueDate || null,
      status:      editForm.value.status,
    })
    const idx = milestones.value.findIndex(x => x.id === m.id)
    if (idx !== -1) milestones.value[idx] = updated
    editingId.value = null
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to update milestone'
  } finally {
    submitting.value = false
  }
}

async function handleDelete(m) {
  if (!confirm(`Delete milestone "${m.name}"?`)) return
  try {
    await milestoneApi.delete(wid.value, pid.value, m.id)
    milestones.value = milestones.value.filter(x => x.id !== m.id)
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to delete milestone'
  }
}

const inputCls = 'w-full px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200 focus:outline-none focus:ring-1 focus:ring-blue-500'
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h2 class="text-base font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
        <Flag class="size-4" /> Milestones ({{ milestones.length }})
      </h2>
      <button @click="showCreate = !showCreate"
        class="flex items-center gap-2 px-4 py-1.5 text-sm rounded bg-gradient-to-br from-blue-500 to-blue-600 text-white">
        <Plus class="size-4" /> New Milestone
      </button>
    </div>

    <p v-if="error" class="text-sm text-red-500">{{ error }}</p>

    <!-- Create form -->
    <div v-if="showCreate" class="border border-zinc-300 dark:border-zinc-700 rounded-lg p-4 space-y-3 bg-white dark:bg-zinc-900">
      <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-200">New Milestone</h3>
      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">Name *</label>
          <input v-model="form.name" placeholder="v1.0 Release" :class="inputCls" class="mt-1" />
        </div>
        <div>
          <label class="text-xs text-zinc-500 dark:text-zinc-400">Due Date</label>
          <input v-model="form.dueDate" type="date" :min="todayStr" :class="inputCls" class="mt-1" />
        </div>
      </div>
      <div>
        <label class="text-xs text-zinc-500 dark:text-zinc-400">Description</label>
        <input v-model="form.description" placeholder="What does this milestone represent?" :class="inputCls" class="mt-1" />
      </div>
      <div>
        <label class="text-xs text-zinc-500 dark:text-zinc-400">Status</label>
        <select v-model="form.status" :class="inputCls" class="mt-1">
          <option value="NotStarted">Not Started</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
        </select>
      </div>
      <div class="flex gap-2 justify-end">
        <button @click="showCreate = false; form = blank()"
          class="px-4 py-1.5 text-sm border border-zinc-300 dark:border-zinc-700 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">Cancel</button>
        <button @click="handleCreate" :disabled="submitting || !form.name.trim()"
          class="px-4 py-1.5 text-sm bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600">
          {{ submitting ? 'Creating...' : 'Create Milestone' }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-sm text-zinc-500 dark:text-zinc-400 py-4">Loading milestones...</div>

    <div v-else-if="milestones.length === 0 && !showCreate" class="text-sm text-zinc-500 dark:text-zinc-400 py-8 text-center">
      No milestones yet. Create one to track key project goals.
    </div>

    <div v-else class="space-y-3">
      <div v-for="m in milestones" :key="m.id"
        class="border border-zinc-300 dark:border-zinc-700 rounded-lg p-4 bg-white dark:bg-zinc-900">

        <!-- View mode -->
        <template v-if="editingId !== m.id">
          <div class="flex items-start justify-between gap-4">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <h3 class="text-sm font-medium text-zinc-900 dark:text-zinc-100">{{ m.name }}</h3>
                <span :class="['px-2 py-0.5 text-xs rounded', (statusMeta[m.status] || statusMeta.NotStarted).cls]">
                  {{ (statusMeta[m.status] || statusMeta.NotStarted).label }}
                </span>
              </div>
              <p v-if="m.description" class="text-xs text-zinc-500 dark:text-zinc-400 mt-1">{{ m.description }}</p>
              <p v-if="m.dueDate" class="text-xs text-zinc-500 dark:text-zinc-400 mt-1">
                Due: {{ format(new Date(m.dueDate), 'dd MMM yyyy') }}
              </p>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button @click="startEdit(m)" class="p-1 text-zinc-400 hover:text-blue-500 transition-colors">
                <Pencil class="size-3.5" />
              </button>
              <button @click="handleDelete(m)" class="p-1 text-zinc-400 hover:text-red-500 transition-colors">
                <Trash2 class="size-3.5" />
              </button>
            </div>
          </div>
        </template>

        <!-- Edit mode -->
        <template v-else>
          <div class="space-y-3">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">Name *</label>
                <input v-model="editForm.name" :class="inputCls" class="mt-1" />
              </div>
              <div>
                <label class="text-xs text-zinc-500 dark:text-zinc-400">Due Date</label>
                <input v-model="editForm.dueDate" type="date" :class="inputCls" class="mt-1" />
              </div>
            </div>
            <div>
              <label class="text-xs text-zinc-500 dark:text-zinc-400">Description</label>
              <input v-model="editForm.description" :class="inputCls" class="mt-1" />
            </div>
            <div>
              <label class="text-xs text-zinc-500 dark:text-zinc-400">Status</label>
              <select v-model="editForm.status" :class="inputCls" class="mt-1">
                <option value="NotStarted">Not Started</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
            <div class="flex gap-2 justify-end">
              <button @click="cancelEdit"
                class="flex items-center gap-1 px-3 py-1 text-xs border border-zinc-300 dark:border-zinc-700 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">
                <X class="size-3" /> Cancel
              </button>
              <button @click="handleUpdate(m)" :disabled="submitting || !editForm.name.trim()"
                class="flex items-center gap-1 px-3 py-1 text-xs bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600">
                <Check class="size-3" /> Save
              </button>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
