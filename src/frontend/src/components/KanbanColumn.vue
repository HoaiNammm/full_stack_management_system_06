<template>
  <div class="w-[300px] flex-shrink-0 flex flex-col max-h-full rounded-lg shadow-sm transition-all"
    :class="[
      column.dotColor === 'bg-outline' ? 'bg-surface-container-lowest border border-outline-variant' : 'bg-surface-container-lowest border border-outline-variant',
      dragOver ? 'ring-2 ring-primary ring-offset-1' : ''
    ]"
    @dragover.prevent="dragOver = true"
    @dragleave="dragOver = false"
    @drop="onDrop">

    <!-- Header -->
    <div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
      <div class="flex items-center gap-2">
        <div class="w-2 h-2 rounded-full" :class="column.dotColor"></div>
        <h3 class="font-label-lg text-label-lg text-on-surface">{{ column.title }}</h3>
        <span class="px-2 py-0.5 rounded-full font-label-sm text-label-sm" :class="column.countBg">{{ column.tasks.length }}</span>
      </div>
    </div>

    <!-- Add Task button / inline form -->
    <div class="px-2 pt-2">
      <div v-if="addingTask" class="flex flex-col gap-1">
        <input v-model="newTitle" ref="inputRef"
          @keyup.enter="submitTask" @keyup.esc="cancelAdd"
          class="w-full px-2 py-1.5 rounded border border-primary font-body-sm text-body-sm bg-surface-container-lowest outline-none"
          placeholder="Tiêu đề task..." />
        <div class="flex gap-1">
          <button @click="submitTask" :disabled="!newTitle.trim() || saving"
            class="flex-1 py-1 text-[11px] bg-primary text-on-primary rounded font-label-md disabled:opacity-50 flex items-center justify-center gap-1">
            <span v-if="saving" class="material-symbols-outlined animate-spin text-[12px]">progress_activity</span>
            Thêm
          </button>
          <button @click="cancelAdd" class="px-2 py-1 text-[11px] border border-outline-variant text-on-surface-variant rounded">Hủy</button>
        </div>
      </div>
      <button v-else @click="startAdding"
        class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
        <span class="material-symbols-outlined text-[16px]">add</span> Thêm task
      </button>
    </div>

    <!-- Task cards -->
    <div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
      <div v-for="task in column.tasks" :key="task.id"
        draggable="true"
        @dragstart="onDragStart($event, task.id)"
        @dragend="dragOver = false"
        @click="emit('open-task', task.id)"
        class="cursor-pointer active:cursor-grabbing">
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

const props = defineProps(['column'])
const emit  = defineEmits(['add-task', 'task-moved', 'open-task'])

const addingTask = ref(false)
const newTitle   = ref('')
const saving     = ref(false)
const dragOver   = ref(false)
const inputRef   = ref(null)

function startAdding() {
  addingTask.value = true
  newTitle.value   = ''
  nextTick(() => inputRef.value?.focus())
}

function cancelAdd() {
  addingTask.value = false
  newTitle.value   = ''
}

async function submitTask() {
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
  e.dataTransfer.setData('taskId', taskId)
  e.dataTransfer.effectAllowed = 'move'
}

function onDrop(e) {
  dragOver.value = false
  const taskId = e.dataTransfer.getData('taskId')
  if (taskId) emit('task-moved', { taskId, columnId: props.column.id })
}
</script>
