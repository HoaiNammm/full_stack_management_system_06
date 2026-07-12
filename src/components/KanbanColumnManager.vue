<script setup>
import { ref, computed, onMounted } from 'vue'
import { useTaskStore } from '../stores/taskStore'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { kanbanApi } from '../api/tasks'
import { Columns, Plus, Pencil, Trash2, Check, X } from 'lucide-vue-next'

const props = defineProps({ project: Object })
const taskStore      = useTaskStore()
const workspaceStore = useWorkspaceStore()

const pid      = computed(() => props.project?.id)
const columns  = ref([])
const loading  = ref(false)
const error    = ref('')

const showCreate  = ref(false)
const creating    = ref(false)
const newColName  = ref('')

const editingId   = ref(null)
const editName    = ref('')
const saving      = ref(false)

const deletingId  = ref(null)

// Status int labels (for display only)
const STATUS_STR = { 0: 'Backlog', 1: 'To Do', 2: 'In Progress', 3: 'Review', 4: 'Done', 5: 'Blocked' }

onMounted(load)

async function load() {
  if (!pid.value) return
  loading.value = true
  error.value   = ''
  try {
    const cols = await kanbanApi.getColumns(pid.value) || []
    columns.value = [...cols].sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
    // keep store in sync
    taskStore.columnsByProject[pid.value] = columns.value
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to load columns'
  } finally {
    loading.value = false
  }
}

async function handleCreate() {
  if (!newColName.value.trim()) return
  creating.value = true
  error.value    = ''
  try {
    const col = await kanbanApi.createColumn(pid.value, {
      name:  newColName.value.trim(),
      order: columns.value.length,
    })
    columns.value.push(col)
    taskStore.columnsByProject[pid.value] = [...columns.value]
    newColName.value = ''
    showCreate.value = false
  } catch (e) {
    error.value = e?.response?.data?.error || e?.response?.data?.message || 'Failed to create column'
  } finally {
    creating.value = false
  }
}

function startEdit(col) {
  editingId.value = col.id
  editName.value  = col.name
}

async function handleUpdate(col) {
  if (!editName.value.trim()) return
  saving.value = true
  error.value  = ''
  try {
    const updated = await kanbanApi.updateColumn(col.id, { name: editName.value.trim() })
    const idx = columns.value.findIndex(c => c.id === col.id)
    if (idx !== -1) columns.value[idx] = { ...columns.value[idx], ...updated }
    taskStore.columnsByProject[pid.value] = [...columns.value]
    editingId.value = null
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to update column'
  } finally {
    saving.value = false
  }
}

async function handleDelete(col) {
  if (!confirm(`Delete column "${col.name}"? Tasks in this column will not be deleted but may lose their column assignment.`)) return
  deletingId.value = col.id
  error.value      = ''
  try {
    await kanbanApi.deleteColumn(col.id)
    columns.value = columns.value.filter(c => c.id !== col.id)
    taskStore.columnsByProject[pid.value] = [...columns.value]
  } catch (e) {
    error.value = e?.response?.data?.error || e?.response?.data?.message || 'Failed to delete column'
  } finally {
    deletingId.value = null
  }
}

const inputCls = 'flex-1 px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200 focus:outline-none focus:ring-1 focus:ring-blue-500'
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h2 class="text-base font-medium text-zinc-900 dark:text-zinc-300 flex items-center gap-2">
        <Columns class="size-4" /> Kanban Columns ({{ columns.length }})
      </h2>
      <button @click="showCreate = true"
        class="flex items-center gap-1.5 px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 hover:bg-zinc-100 dark:hover:bg-zinc-800">
        <Plus class="size-3.5" /> Add Column
      </button>
    </div>

    <p v-if="error" class="text-sm text-red-500">{{ error }}</p>

    <!-- Create form -->
    <div v-if="showCreate" class="flex items-center gap-2">
      <input v-model="newColName" placeholder="Column name (e.g. Testing)" @keydown.enter="handleCreate" :class="inputCls" />
      <button @click="handleCreate" :disabled="creating || !newColName.trim()"
        class="px-3 py-1.5 text-sm bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600 flex items-center gap-1">
        <Check class="size-3.5" /> {{ creating ? '…' : 'Add' }}
      </button>
      <button @click="showCreate = false; newColName = ''"
        class="px-3 py-1.5 text-sm border border-zinc-300 dark:border-zinc-700 rounded hover:bg-zinc-100 dark:hover:bg-zinc-800">
        <X class="size-3.5" />
      </button>
    </div>

    <div v-if="loading" class="text-sm text-zinc-500 dark:text-zinc-400">Loading columns...</div>

    <div v-else-if="columns.length === 0" class="text-sm text-zinc-500 dark:text-zinc-400">
      No columns found. Columns are auto-created when a project is created.
    </div>

    <div v-else class="space-y-2">
      <div v-for="col in columns" :key="col.id"
        class="flex items-center gap-3 px-3 py-2 rounded border border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-900 group">

        <!-- Order indicator -->
        <span class="text-xs text-zinc-400 dark:text-zinc-500 w-5 text-center">{{ col.order ?? '' }}</span>

        <!-- View / Edit -->
        <template v-if="editingId === col.id">
          <input v-model="editName" @keydown.enter="handleUpdate(col)" @keydown.escape="editingId = null" :class="inputCls" />
          <button @click="handleUpdate(col)" :disabled="saving || !editName.trim()"
            class="p-1.5 text-blue-500 hover:text-blue-700 disabled:opacity-40">
            <Check class="size-4" />
          </button>
          <button @click="editingId = null" class="p-1.5 text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200">
            <X class="size-4" />
          </button>
        </template>

        <template v-else>
          <span class="flex-1 text-sm text-zinc-900 dark:text-zinc-100">{{ col.name }}</span>
          <span v-if="col.defaultStatus !== undefined"
            class="text-xs text-zinc-400 dark:text-zinc-500 px-2 py-0.5 bg-zinc-100 dark:bg-zinc-800 rounded">
            {{ STATUS_STR[col.defaultStatus] ?? 'Custom' }}
          </span>
          <button @click="startEdit(col)"
            class="opacity-0 group-hover:opacity-100 p-1.5 text-zinc-400 hover:text-blue-500 transition">
            <Pencil class="size-3.5" />
          </button>
          <button @click="handleDelete(col)" :disabled="deletingId === col.id"
            class="opacity-0 group-hover:opacity-100 p-1.5 text-zinc-400 hover:text-red-500 transition disabled:opacity-30">
            <Trash2 class="size-3.5" />
          </button>
        </template>
      </div>
    </div>

    <p class="text-xs text-zinc-400 dark:text-zinc-500">
      Columns with a default status are auto-seeded; they map tasks by status. Custom columns have no default status.
    </p>
  </div>
</template>
