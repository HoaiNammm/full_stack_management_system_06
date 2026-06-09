<template>
  <div class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
    <div class="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant w-full max-w-lg flex flex-col max-h-[90vh]">

      <!-- Header -->
      <div class="flex items-center justify-between p-md border-b border-outline-variant flex-shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
            <span class="material-symbols-outlined text-primary text-[20px]">create_new_folder</span>
          </div>
          <div>
            <h2 class="font-headline-sm text-headline-sm text-on-surface">Tạo dự án mới</h2>
            <p class="font-label-sm text-label-sm text-on-surface-variant">Bước {{ step }} / 2</p>
          </div>
        </div>
        <button @click="$emit('close')" class="text-on-surface-variant hover:text-on-surface p-1 rounded-full hover:bg-surface-container-high transition-colors">
          <span class="material-symbols-outlined">close</span>
        </button>
      </div>

      <!-- Step indicator -->
      <div class="flex px-md pt-md gap-2">
        <div class="flex-1 h-1 rounded-full transition-colors" :class="step >= 1 ? 'bg-primary' : 'bg-surface-container-high'"></div>
        <div class="flex-1 h-1 rounded-full transition-colors" :class="step >= 2 ? 'bg-primary' : 'bg-surface-container-high'"></div>
      </div>

      <!-- Body -->
      <div class="p-md flex flex-col gap-md overflow-y-auto flex-1">

        <!-- STEP 1: Template selection -->
        <template v-if="step === 1">
          <p class="font-label-lg text-label-lg text-on-surface">Chọn template dự án</p>

          <!-- Loading -->
          <div v-if="loadingTemplates" class="flex items-center justify-center py-8 gap-3 text-on-surface-variant">
            <span class="material-symbols-outlined animate-spin text-[20px]">progress_activity</span>
            <span class="font-body-md text-body-md">Đang tải template...</span>
          </div>

          <!-- Template cards -->
          <div v-else class="grid grid-cols-1 gap-3">
            <button
              v-for="tpl in templates"
              :key="tpl.id"
              @click="form.templateId = tpl.id"
              class="flex items-start gap-3 p-4 rounded-xl border-2 transition-all text-left"
              :class="form.templateId === tpl.id
                ? 'border-primary bg-primary/5'
                : 'border-outline-variant hover:border-outline hover:bg-surface-container-low'"
            >
              <!-- Icon -->
              <div class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0" :class="tpl.iconBg">
                <span class="material-symbols-outlined text-[20px]" :class="tpl.iconColor"
                  style="font-variation-settings: 'FILL' 1">{{ tpl.icon }}</span>
              </div>

              <!-- Info -->
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2">
                  <p class="font-label-lg text-label-lg text-on-surface">{{ tpl.name }}</p>
                  <span v-if="tpl.recommended"
                    class="text-[10px] bg-secondary-container/40 text-secondary px-1.5 py-0.5 rounded-full font-bold">Phổ biến</span>
                </div>
                <p class="font-body-sm text-body-sm text-on-surface-variant mt-0.5">{{ tpl.description }}</p>

                <!-- Preview chips: columns -->
                <div v-if="form.templateId === tpl.id" class="mt-2 flex flex-col gap-1.5">
                  <!-- Columns flow -->
                  <div class="flex items-center gap-1 flex-wrap">
                    <template v-for="(col, idx) in tpl.columns" :key="col">
                      <span class="text-[10px] px-1.5 py-0.5 rounded bg-surface-container text-on-surface-variant">{{ col }}</span>
                      <span v-if="idx < tpl.columns.length - 1" class="text-on-surface-variant/40 text-[10px]">›</span>
                    </template>
                  </div>
                  <!-- Sprint + milestone badges -->
                  <div class="flex gap-2 flex-wrap">
                    <span v-if="tpl.sprintCount > 0"
                      class="flex items-center gap-1 text-[10px] px-1.5 py-0.5 rounded bg-primary/10 text-primary">
                      <span class="material-symbols-outlined text-[12px]">sprint</span>
                      {{ tpl.sprintCount }} sprint × 14 ngày
                    </span>
                    <span v-if="tpl.milestoneNames?.length > 0"
                      class="flex items-center gap-1 text-[10px] px-1.5 py-0.5 rounded bg-secondary/10 text-secondary">
                      <span class="material-symbols-outlined text-[12px]">flag</span>
                      {{ tpl.milestoneNames.length }} milestone
                    </span>
                  </div>
                  <!-- Milestone names -->
                  <div v-if="tpl.milestoneNames?.length > 0" class="flex gap-1 flex-wrap">
                    <span v-for="ms in tpl.milestoneNames" :key="ms"
                      class="text-[10px] px-1.5 py-0.5 rounded-full border border-outline-variant/60 text-on-surface-variant">
                      {{ ms }}
                    </span>
                  </div>
                </div>

                <!-- Tags when not selected -->
                <div v-else class="flex gap-1 mt-2 flex-wrap">
                  <span v-for="tag in tpl.tags" :key="tag"
                    class="text-[10px] px-1.5 py-0.5 rounded bg-surface-container text-on-surface-variant">{{ tag }}</span>
                </div>
              </div>

              <!-- Radio -->
              <div class="w-5 h-5 rounded-full border-2 flex items-center justify-center flex-shrink-0 mt-0.5 transition-colors"
                :class="form.templateId === tpl.id ? 'border-primary bg-primary' : 'border-outline-variant'">
                <span v-if="form.templateId === tpl.id" class="material-symbols-outlined text-on-primary text-[12px]">check</span>
              </div>
            </button>
          </div>
        </template>

        <!-- STEP 2: Project details -->
        <template v-else>
          <!-- Name -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Tên dự án <span class="text-error">*</span></label>
            <input v-model="form.name"
              :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
                errors.name ? 'border-error focus:ring-1 focus:ring-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
              placeholder="VD: Payment Gateway v2" />
            <p v-if="errors.name" class="font-label-sm text-label-sm text-error flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px]">error</span>{{ errors.name }}
            </p>
          </div>

          <!-- Description -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Mô tả</label>
            <textarea v-model="form.description" rows="3"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-low outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all resize-none"
              placeholder="Mô tả ngắn về mục tiêu dự án..."></textarea>
          </div>

          <!-- Dates -->
          <div class="grid grid-cols-2 gap-md">
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Ngày bắt đầu <span class="text-error">*</span></label>
              <input type="date" v-model="form.startDate"
                :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
                  errors.startDate ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']" />
              <p v-if="errors.startDate" class="font-label-sm text-label-sm text-error">{{ errors.startDate }}</p>
            </div>
            <div class="flex flex-col gap-xs">
              <label class="font-label-lg text-label-lg text-on-surface">Ngày kết thúc</label>
              <input type="date" v-model="form.endDate"
                class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-low outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all" />
            </div>
          </div>

          <!-- Color Picker -->
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Màu nhận diện</label>
            <div class="flex gap-2 flex-wrap">
              <button v-for="c in colors" :key="c.hex" @click="form.color = c.hex"
                class="w-8 h-8 rounded-full border-2 transition-all hover:scale-110 flex items-center justify-center"
                :style="{ backgroundColor: c.hex }"
                :class="form.color === c.hex ? 'border-on-surface scale-110 ring-2 ring-offset-2 ring-on-surface/30' : 'border-white'"
                :title="c.name">
                <span v-if="form.color === c.hex" class="material-symbols-outlined text-white text-[14px]">check</span>
              </button>
            </div>

            <!-- Preview card -->
            <div class="flex items-center gap-3 mt-2 p-3 bg-surface-container-low rounded-xl border border-outline-variant/50">
              <div class="w-10 h-10 rounded-lg flex items-center justify-center font-bold text-white font-label-lg flex-shrink-0"
                :style="{ backgroundColor: form.color }">
                {{ previewInitials }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="font-label-lg text-label-lg text-on-surface truncate">{{ form.name || 'Tên dự án' }}</p>
                <p class="font-label-sm text-label-sm text-on-surface-variant">{{ selectedTemplate?.name }}</p>
              </div>
              <!-- Template summary badges -->
              <div class="flex flex-col items-end gap-1 flex-shrink-0">
                <span v-if="selectedTemplate?.sprintCount > 0"
                  class="text-[10px] bg-primary/10 text-primary px-1.5 py-0.5 rounded">
                  {{ selectedTemplate.sprintCount }} sprints
                </span>
                <span v-if="selectedTemplate?.milestoneNames?.length > 0"
                  class="text-[10px] bg-secondary/10 text-secondary px-1.5 py-0.5 rounded">
                  {{ selectedTemplate.milestoneNames.length }} milestones
                </span>
              </div>
            </div>
          </div>

          <!-- API Error -->
          <p v-if="apiError" class="font-label-sm text-label-sm text-error flex items-center gap-1 bg-error-container/20 rounded-lg px-3 py-2">
            <span class="material-symbols-outlined text-[14px]">error</span>{{ apiError }}
          </p>
        </template>
      </div>

      <!-- Footer -->
      <div class="p-md border-t border-outline-variant flex justify-between gap-3 flex-shrink-0">
        <button v-if="step === 2" @click="step = 1"
          class="px-md py-sm rounded-lg border border-outline-variant font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors flex items-center gap-1">
          <span class="material-symbols-outlined text-[16px]">arrow_back</span>
          Quay lại
        </button>
        <div v-else></div>

        <div class="flex gap-3">
          <button @click="$emit('close')"
            class="px-md py-sm rounded-lg border border-outline-variant font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
            Hủy
          </button>
          <button v-if="step === 1" @click="step = 2" :disabled="loadingTemplates"
            class="flex items-center gap-xs px-md py-sm rounded-lg bg-primary text-on-primary font-label-lg text-label-lg hover:opacity-90 transition-opacity shadow-sm disabled:opacity-50">
            Tiếp theo
            <span class="material-symbols-outlined text-[18px]">arrow_forward</span>
          </button>
          <button v-else @click="handleSubmit" :disabled="submitting"
            class="flex items-center gap-xs px-md py-sm rounded-lg bg-primary text-on-primary font-label-lg text-label-lg hover:opacity-90 transition-opacity shadow-sm disabled:opacity-60">
            <span v-if="submitting" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
            <span v-else class="material-symbols-outlined text-[18px]">add</span>
            {{ submitting ? 'Đang tạo...' : 'Tạo dự án' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { projectService } from '../services/api'

const emit = defineEmits(['close', 'created'])

const step            = ref(1)
const submitting      = ref(false)
const apiError        = ref('')
const loadingTemplates = ref(false)
const templates       = ref([])

const colors = [
  { hex: '#3525cd', name: 'Indigo' },
  { hex: '#006a61', name: 'Teal' },
  { hex: '#684000', name: 'Nâu' },
  { hex: '#ba1a1a', name: 'Đỏ' },
  { hex: '#0f5e9c', name: 'Xanh dương' },
  { hex: '#6a0dad', name: 'Tím' },
  { hex: '#2e7d32', name: 'Xanh lá' },
  { hex: '#e65100', name: 'Cam' },
]

const form   = ref({ templateId: '', name: '', description: '', startDate: '', endDate: '', color: '#3525cd' })
const errors = ref({})

const selectedTemplate = computed(() => templates.value.find(t => t.id === form.value.templateId))

const previewInitials = computed(() => {
  if (!form.value.name) return 'PM'
  return form.value.name.split(' ').map(w => w[0]).slice(0, 3).join('').toUpperCase()
})

onMounted(async () => {
  loadingTemplates.value = true
  try {
    templates.value = await projectService.getTemplates()
    if (templates.value.length > 0) {
      form.value.templateId = templates.value[0].id
    }
  } catch {
    // Fallback: minimal blank template so modal still works
    templates.value = [{
      id: 'blank', name: 'Trống', description: 'Board cơ bản',
      icon: 'add_box', iconBg: 'bg-surface-container-high', iconColor: 'text-on-surface-variant',
      recommended: false, tags: ['Linh hoạt'],
      columns: ['Backlog', 'To Do', 'In Progress', 'Done'],
      sprintCount: 0, milestoneNames: []
    }]
    form.value.templateId = 'blank'
  } finally {
    loadingTemplates.value = false
  }
})

function validate() {
  errors.value = {}
  if (!form.value.name.trim()) errors.value.name = 'Vui lòng nhập tên dự án'
  if (!form.value.startDate)   errors.value.startDate = 'Vui lòng chọn ngày bắt đầu'
  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  submitting.value = true
  apiError.value   = ''
  try {
    const project = await projectService.create({
      name:        form.value.name.trim(),
      description: form.value.description.trim() || null,
      color:       form.value.color,
      startDate:   form.value.startDate ? new Date(form.value.startDate).toISOString() : null,
      endDate:     form.value.endDate   ? new Date(form.value.endDate).toISOString()   : null,
      templateId:  form.value.templateId,
    })
    emit('created', project)
  } catch (e) {
    apiError.value = e.response?.data?.message
      || e.response?.data?.error?.code
      || 'Tạo dự án thất bại. Kiểm tra ProjectService.'
  } finally {
    submitting.value = false
  }
}
</script>
