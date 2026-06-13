<template>
  <div class="w-[310px] flex-shrink-0 flex flex-col max-h-full rounded-xl shadow-sm transition-all"
    :class="[
      'bg-surface-container-low/80 border border-outline-variant backdrop-blur-sm',
      dragOver ? 'ring-2 ring-primary ring-offset-1 ring-offset-background' : ''
    ]"
    @dragover.prevent="dragOver = true"
    @dragleave="dragOver = false"
    @drop="onDrop">

    <!-- Header -->
    <div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-container-lowest rounded-t-xl">
      <div class="flex items-center gap-2">
        <div class="w-2 h-2 rounded-full" :class="column.dotColor"></div>
        <h3 class="font-label-lg text-label-lg text-on-surface">{{ column.title }}</h3>
        <span class="px-2 py-0.5 rounded-full font-label-sm text-label-sm" :class="column.countBg">{{ column.tasks.length }}</span>
      </div>
    </div>

    <!-- Add Task button / inline form -->
    <div class="px-2 pt-2">
      <div v-if="canEdit && addingTask" class="flex flex-col gap-1">
        <input v-model="newTitle" ref="inputRef"
          @keyup.enter="submitTask" @keyup.esc="cancelAdd"
          class="app-input w-full px-2 py-1.5 rounded-lg font-body-sm text-body-sm"
          placeholder="Tiêu đề task..." />
        <div class="flex gap-1">
          <button @click="submitTask" :disabled="!newTitle.trim() || saving"
            class="flex-1 py-1 text-[11px] bg-primary text-on-primary rounded-lg font-label-md disabled:opacity-50 flex items-center justify-center gap-1">
            <span v-if="saving" class="material-symbols-outlined animate-spin text-[12px]">progress_activity</span>
            Thêm
          </button>
          <button @click="cancelAdd" class="px-2 py-1 text-[11px] border border-outline-variant text-on-surface-variant rounded">Hủy</button>
        </div>
      </div>
      <button v-else-if="canEdit" @click="startAdding"
        class="w-full py-1.5 border border-dashed border-outline-variant rounded-lg text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
        <span class="material-symbols-outlined text-[16px]">add</span> Thêm task
      </button>
    </div>

    <!-- Task cards -->
    <div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
      <div v-for="task in column.tasks" :key="task.id"
        data-testid="kanban-task-card"
        :draggable="canEdit"
        @dragstart="onDragStart($event, task.id)"
        @dragend="dragOver = false"
        @click="emit('open-task', task.id)"
        :class="canEdit ? 'cursor-pointer active:cursor-grabbing' : 'cursor-pointer'">
        <TaskCard :task="task" />
      </div>
      <div v-if="dragOver && column.tasks.length === 0"
        class="h-16 rounded border-2 border-dashed border-primary/40 bg-primary/5 flex items-center justify-center">
        <span class="font-label-sm text-label-sm text-primary/60">Thả vào đây</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import TaskCard from './TaskCard.vue'

const props = defineProps({
  column: { type: Object, required: true },
  canEdit: { type: Boolean, default: true },
})
const emit  = defineEmits(['add-task', 'task-moved', 'open-task'])

const addingTask = ref(false)
const newTitle   = ref('')
const saving     = ref(false)
const dragOver   = ref(false)
const inputRef   = ref(null)

function startAdding() {
  if (!props.canEdit) return
  addingTask.value = true
  newTitle.value   = ''
  nextTick(() => inputRef.value?.focus())
}

function cancelAdd() {
  addingTask.value = false
  newTitle.value   = ''
}

async function submitTask() {
  if (!props.canEdit) return
  if (!newTitle.value.trim() || saving.value) return
  saving.value = true
  try {
    await emit('add-task', { columnId: props.column.id, title: newTitle.value.trim() })
    cancelAdd()
  } finally {
    saving.value = false
  }
}

function onDragStart(e, taskId) {
  if (!props.canEdit) {
    e.preventDefault()
    return
  }
  e.dataTransfer.setData('taskId', taskId)
  e.dataTransfer.effectAllowed = 'move'
}

function onDrop(e) {
  dragOver.value = false
  if (!props.canEdit) return
  const taskId = e.dataTransfer.getData('taskId')
  if (taskId) emit('task-moved', { taskId, columnId: props.column.id })
}
</script>
