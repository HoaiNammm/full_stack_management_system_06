<template>
  <div class="fixed inset-0 bg-black/45 backdrop-blur-sm z-50 flex justify-end" @click.self="$emit('close')">
    <div class="w-full max-w-4xl h-full bg-surface-container-lowest border-l border-outline-variant shadow-2xl flex flex-col overflow-hidden animate-drawer">

      <!-- Loading -->
      <div v-if="loading" class="p-xl flex items-center justify-center gap-2 text-on-surface-variant">
        <span class="material-symbols-outlined animate-spin">progress_activity</span> Đang tải...
      </div>

      <template v-else-if="task">
        <!-- Header -->
        <div class="relative overflow-hidden flex items-center justify-between px-lg pt-lg pb-md border-b border-outline-variant flex-shrink-0 bg-surface-container-low">
          <div class="absolute inset-0 bg-gradient-to-br from-primary/10 via-transparent to-secondary/10 pointer-events-none"></div>
          <div class="flex items-center gap-3">
            <div class="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center shadow-sm">
              <span class="material-symbols-outlined text-primary text-[20px]" style="font-variation-settings: 'FILL' 1">task_alt</span>
            </div>
            <div>
              <p class="page-eyebrow">Task detail</p>
              <p class="font-label-md text-label-md text-outline">#{{ task.id.substring(0, 8).toUpperCase() }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <button v-if="canEdit" @click="confirmDelete" :disabled="deleting"
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
        <div class="p-lg overflow-y-auto flex-1">
          <div class="grid grid-cols-1 lg:grid-cols-[1fr_260px] gap-lg">
            <div class="flex flex-col gap-md">

          <!-- Title -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Tiêu đề <span class="text-error">*</span></label>
            <input v-model="form.title" :disabled="!canEdit"
              class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md"
              placeholder="Tiêu đề task..." />
          </div>

          <!-- Description -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Mô tả</label>
            <textarea v-model="form.description" rows="3" :disabled="!canEdit"
              class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md resize-none"
              placeholder="Mô tả chi tiết..."></textarea>
          </div>

          <!-- Priority + Column row -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Độ ưu tiên</label>
              <select v-model="form.priority" :disabled="!canEdit"
                class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md">
                <option :value="1">Thấp</option>
                <option :value="2">Trung bình</option>
                <option :value="3">Cao</option>
              </select>
            </div>
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Cột hiện tại</label>
              <select v-model="form.columnId" :disabled="!canEdit"
                class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md">
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
            <select v-model="form.sprintId" :disabled="!canEdit"
              class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md">
              <option value="">— Không thuộc sprint nào —</option>
              <option v-for="s in sprints" :key="s.id" :value="s.id">
                {{ s.name }}{{ s.status === 1 ? ' (Active)' : s.status === 2 ? ' (Done)' : '' }}
              </option>
            </select>
          </div>

          <!-- Due date + Estimated hours row -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Hạn hoàn thành</label>
              <input v-model="form.dueDate" type="date" :disabled="!canEdit"
                class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md" />
            </div>
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Thời gian ước tính (h)</label>
              <input v-model.number="form.estimatedHours" type="number" min="0" step="0.5" :disabled="!canEdit"
                class="app-input w-full px-3 py-2 rounded-lg font-body-md text-body-md"
                placeholder="0" />
            </div>
          </div>

          <!-- Created info -->
          <div class="flex items-center gap-2 text-on-surface-variant">
            <span class="material-symbols-outlined text-[16px]">schedule</span>
            <span class="font-label-sm text-label-sm">Tạo lúc: {{ formatDate(task.createdAt) }}</span>
          </div>
            </div>

            <aside class="flex flex-col gap-md">
              <div class="workspace-card">
                <p class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Status</p>
                <div class="mt-3 space-y-3">
                  <div class="flex items-center justify-between gap-3">
                    <span class="font-label-md text-label-md text-on-surface-variant">Priority</span>
                    <span class="soft-badge" :class="priorityMeta.class">{{ priorityMeta.label }}</span>
                  </div>
                  <div class="flex items-center justify-between gap-3">
                    <span class="font-label-md text-label-md text-on-surface-variant">Column</span>
                    <span class="font-label-md text-label-md text-on-surface text-right">{{ currentColumnName }}</span>
                  </div>
                  <div class="flex items-center justify-between gap-3">
                    <span class="font-label-md text-label-md text-on-surface-variant">Sprint</span>
                    <span class="font-label-md text-label-md text-on-surface text-right">{{ currentSprintName }}</span>
                  </div>
                </div>
              </div>

              <div class="workspace-card">
                <p class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Delivery</p>
                <div class="mt-3 h-2 rounded-full bg-surface-container-high overflow-hidden">
                  <div class="h-full rounded-full bg-primary" :style="{ width: progressPercent + '%' }"></div>
                </div>
                <div class="mt-3 grid grid-cols-1 gap-sm">
                  <div class="metric-tile !p-sm flex items-center justify-between">
                    <span>Estimate</span>
                    <strong class="!text-title-md">{{ form.estimatedHours || 0 }}h</strong>
                  </div>
                  <div class="metric-tile !p-sm flex items-center justify-between">
                    <span>Due</span>
                    <strong class="!text-title-md">{{ form.dueDate || '--' }}</strong>
                  </div>
                </div>
              </div>

              <div class="workspace-card">
                <div class="flex items-center gap-2 text-on-surface-variant">
                  <span class="material-symbols-outlined text-[18px]">forum</span>
                  <span class="font-label-md text-label-md">Task comments dùng NotifyService.</span>
                </div>
              </div>
            </aside>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-lg py-md border-t border-outline-variant bg-surface-container-low flex justify-end gap-2 flex-shrink-0">
          <button @click="$emit('close')"
            class="app-button-secondary">
            Hủy
          </button>
          <button v-if="canEdit" @click="save" :disabled="!form.title.trim() || saving"
            class="app-button-primary disabled:opacity-60 flex items-center gap-2">
            <span v-if="saving" class="material-symbols-outlined text-[16px] animate-spin">progress_activity</span>
            Lưu thay đổi
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { taskService, projectService } from '../services/api'

const props = defineProps({
  taskId: { type: String, required: true },
  columns: { type: Array, default: () => [] },
  canEdit: { type: Boolean, default: true },
})
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

const currentColumnName = computed(() => props.columns?.find(c => c.id === form.value.columnId)?.title || 'Chưa chọn')
const currentSprintName = computed(() => sprints.value.find(s => s.id === form.value.sprintId)?.name || 'Không thuộc sprint')
const priorityMeta = computed(() => {
  const map = {
    1: { label: 'Low', class: 'bg-secondary/10 text-secondary' },
    2: { label: 'Medium', class: 'bg-primary/10 text-primary' },
    3: { label: 'High', class: 'bg-error-container/40 text-error' },
  }
  return map[form.value.priority] || map[2]
})
const progressPercent = computed(() => {
  if (form.value.priority === 3) return 35
  if (form.value.priority === 1) return 70
  return 55
})

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
  if (!props.canEdit) return
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
  if (!props.canEdit) return
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

<style scoped>
.animate-drawer {
  animation: drawer-in 180ms ease-out;
}

@keyframes drawer-in {
  from { transform: translateX(24px); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}
</style>
