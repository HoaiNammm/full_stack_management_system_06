<template>
  <div class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
    <div class="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant w-full max-w-lg flex flex-col max-h-[90vh]">

      <!-- Loading -->
      <div v-if="loading" class="p-xl flex items-center justify-center gap-2 text-on-surface-variant">
        <span class="material-symbols-outlined animate-spin">progress_activity</span> Đang tải...
      </div>

      <template v-else-if="task">
        <!-- Header -->
        <div class="flex items-center justify-between px-md pt-md pb-3 border-b border-outline-variant flex-shrink-0">
          <div class="flex items-center gap-3">
            <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
              <span class="material-symbols-outlined text-primary text-[20px]" style="font-variation-settings: 'FILL' 1">task_alt</span>
            </div>
            <div>
              <p class="font-label-sm text-label-sm text-on-surface-variant">Task</p>
              <p class="font-label-md text-label-md text-outline">{{ task.id.substring(0, 8).toUpperCase() }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <button @click="confirmDelete" :disabled="deleting"
              class="flex items-center gap-1 px-3 py-1 rounded-lg border border-error/40 text-error hover:bg-error-container/20 font-label-md text-label-md transition-colors disabled:opacity-60">
              <span class="material-symbols-outlined text-[16px]" :class="{ 'animate-spin': deleting }">
                {{ deleting ? 'progress_activity' : 'delete' }}
              </span>
              Xóa
            </button>
            <button @click="$emit('close')" class="p-1 rounded-full text-on-surface-variant hover:bg-surface-container-high transition-colors">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
        </div>

        <!-- Body -->
        <div class="p-md flex flex-col gap-md overflow-y-auto flex-1">

          <!-- Title -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Tiêu đề <span class="text-error">*</span></label>
            <input v-model="form.title"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all"
              placeholder="Tiêu đề task..." />
          </div>

          <!-- Description -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Mô tả</label>
            <textarea v-model="form.description" rows="3"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary font-body-md text-body-md bg-surface-container-low outline-none resize-none transition-all"
              placeholder="Mô tả chi tiết..."></textarea>
          </div>

          <!-- Priority + Column row -->
          <div class="grid grid-cols-2 gap-3">
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Độ ưu tiên</label>
              <select v-model="form.priority"
                class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all">
                <option :value="1">Thấp</option>
                <option :value="2">Trung bình</option>
                <option :value="3">Cao</option>
              </select>
            </div>
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Cột hiện tại</label>
              <select v-model="form.columnId"
                class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all">
                <option v-for="col in columns" :key="col.id" :value="col.id">{{ col.title }}</option>
              </select>
            </div>
          </div>

          <!-- Sprint -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface flex items-center gap-1">
              <span class="material-symbols-outlined text-[16px] text-on-surface-variant">sprint</span>
              Sprint
            </label>
            <select v-model="form.sprintId"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all">
              <option value="">— Không thuộc sprint nào —</option>
              <option v-for="s in sprints" :key="s.id" :value="s.id">
                {{ s.name }}{{ s.status === 1 ? ' (Active)' : s.status === 2 ? ' (Done)' : '' }}
              </option>
            </select>
          </div>

          <!-- Due date + Estimated hours row -->
          <div class="grid grid-cols-2 gap-3">
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Hạn hoàn thành</label>
              <input v-model="form.dueDate" type="date"
                class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all" />
            </div>
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Thời gian ước tính (h)</label>
              <input v-model.number="form.estimatedHours" type="number" min="0" step="0.5"
                class="w-full px-3 py-2 rounded-lg border border-outline-variant focus:border-primary font-body-md text-body-md bg-surface-container-low outline-none transition-all"
                placeholder="0" />
            </div>
          </div>

          <!-- Created info -->
          <div class="flex items-center gap-2 text-on-surface-variant">
            <span class="material-symbols-outlined text-[16px]">schedule</span>
            <span class="font-label-sm text-label-sm">Tạo lúc: {{ formatDate(task.createdAt) }}</span>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-md py-3 border-t border-outline-variant flex justify-end gap-2 flex-shrink-0">
          <button @click="$emit('close')"
            class="px-md py-sm border border-outline-variant rounded-lg font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
            Hủy
          </button>
          <button @click="save" :disabled="!form.title.trim() || saving"
            class="px-md py-sm bg-primary text-on-primary rounded-lg font-label-lg text-label-lg hover:opacity-90 transition-opacity disabled:opacity-60 flex items-center gap-2">
            <span v-if="saving" class="material-symbols-outlined text-[16px] animate-spin">progress_activity</span>
            Lưu thay đổi
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { taskService, projectService } from '../services/api'

const props = defineProps(['taskId', 'columns'])
const emit  = defineEmits(['close', 'updated', 'deleted'])

const task     = ref(null)
const loading  = ref(true)
const saving   = ref(false)
const deleting = ref(false)
const sprints  = ref([])

const form = ref({
  title: '',
  description: '',
  priority: 2,
  dueDate: '',
  estimatedHours: null,
  columnId: '',
  sprintId: '',
})

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('vi-VN')
}

function toDateInput(d) {
  if (!d) return ''
  return new Date(d).toISOString().split('T')[0]
}

onMounted(async () => {
  try {
    const t = await taskService.getById(props.taskId)
    task.value = t
    form.value = {
      title:          t.title || '',
      description:    t.description || '',
      priority:       t.priority ?? 2,
      dueDate:        toDateInput(t.dueDate),
      estimatedHours: t.estimatedHours ?? null,
      columnId:       t.columnId,
      sprintId:       t.sprintId || '',
    }
    // Load sprints for sprint selector
    if (t.projectId) {
      sprints.value = await projectService.getSprints(t.projectId).catch(() => [])
    }
  } catch {
    emit('close')
  } finally {
    loading.value = false
  }
})

async function save() {
  if (!form.value.title.trim() || saving.value) return
  saving.value = true
  try {
    const columnChanged = form.value.columnId !== task.value.columnId

    const sprintChanged = form.value.sprintId !== (task.value.sprintId || '')
    await taskService.update(props.taskId, {
      title:          form.value.title.trim(),
      description:    form.value.description || null,
      priority:       form.value.priority,
      assignedTo:     task.value.assignedTo || null,
      dueDate:        form.value.dueDate ? new Date(form.value.dueDate).toISOString() : null,
      estimatedHours: form.value.estimatedHours || null,
      sprintId:       form.value.sprintId || null,
      clearSprint:    sprintChanged && !form.value.sprintId,
    })

    if (columnChanged) {
      await taskService.moveColumn(props.taskId, form.value.columnId)
    }

    emit('updated')
    emit('close')
  } catch (e) {
    alert('Không thể lưu: ' + (e.response?.data?.message || e.message))
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!confirm('Xóa task này?')) return
  deleting.value = true
  try {
    await taskService.delete(props.taskId)
    emit('deleted')
    emit('close')
  } catch (e) {
    alert('Không thể xóa: ' + (e.response?.data?.message || e.message))
  } finally {
    deleting.value = false
  }
}
</script>
