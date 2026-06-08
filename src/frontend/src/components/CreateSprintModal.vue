<template>
  <div class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
    <div class="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant w-full max-w-md flex flex-col">
      <!-- Header -->
      <div class="flex items-center justify-between p-md border-b border-outline-variant">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
            <span class="material-symbols-outlined text-primary text-[20px]" style="font-variation-settings: 'FILL' 1">sprint</span>
          </div>
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Tạo Sprint mới</h2>
        </div>
        <button @click="$emit('close')" class="text-on-surface-variant hover:text-on-surface p-1 rounded-full hover:bg-surface-container-high transition-colors">
          <span class="material-symbols-outlined">close</span>
        </button>
      </div>

      <!-- Body -->
      <div class="p-md flex flex-col gap-md">
        <!-- Name -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Tên sprint <span class="text-error">*</span></label>
          <input v-model="form.name"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
              errors.name ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
            placeholder="VD: Sprint 4" />
          <p v-if="errors.name" class="font-label-sm text-label-sm text-error flex items-center gap-1">
            <span class="material-symbols-outlined text-[14px]">error</span>{{ errors.name }}
          </p>
        </div>

        <!-- Goal -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Mục tiêu sprint <span class="text-error">*</span></label>
          <textarea v-model="form.goal" rows="3"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all resize-none',
              errors.goal ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
            placeholder="Mô tả mục tiêu chính của sprint này..."></textarea>
          <p v-if="errors.goal" class="font-label-sm text-label-sm text-error flex items-center gap-1">
            <span class="material-symbols-outlined text-[14px]">error</span>{{ errors.goal }}
          </p>
        </div>

        <!-- Start date -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Ngày bắt đầu <span class="text-error">*</span></label>
          <input type="date" v-model="form.startDate"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
              errors.startDate ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']" />
          <p v-if="errors.startDate" class="font-label-sm text-label-sm text-error flex items-center gap-1">
            <span class="material-symbols-outlined text-[14px]">error</span>{{ errors.startDate }}
          </p>
        </div>

        <!-- Auto end date info -->
        <div class="flex items-start gap-3 p-3 rounded-xl border transition-all"
          :class="form.startDate ? 'bg-primary/5 border-primary/20' : 'bg-surface-container-low border-outline-variant/50'">
          <span class="material-symbols-outlined text-[20px] flex-shrink-0 mt-0.5"
            :class="form.startDate ? 'text-primary' : 'text-on-surface-variant'"
            style="font-variation-settings: 'FILL' 1">info</span>
          <div>
            <p class="font-label-lg text-label-lg text-on-surface">Ngày kết thúc (tự động)</p>
            <p class="font-headline-sm text-headline-sm mt-0.5"
              :class="form.startDate ? 'text-primary' : 'text-on-surface-variant'">
              {{ endDateDisplay }}
            </p>
            <p class="font-label-sm text-label-sm text-on-surface-variant mt-0.5">
              Thời gian sprint cố định: <strong>2 tuần (14 ngày)</strong>
            </p>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="p-md border-t border-outline-variant flex justify-end gap-3">
        <button @click="$emit('close')"
          class="px-md py-sm rounded-lg border border-outline-variant font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
          Hủy
        </button>
        <button @click="handleSubmit"
          class="flex items-center gap-xs px-md py-sm rounded-lg bg-primary text-on-primary font-label-lg text-label-lg hover:opacity-90 transition-opacity shadow-sm">
          <span class="material-symbols-outlined text-[18px]">add</span>
          Tạo sprint
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const emit = defineEmits(['close', 'created'])
const form = ref({ name: '', goal: '', startDate: '' })
const errors = ref({})

const endDateDisplay = computed(() => {
  if (!form.value.startDate) return 'Chọn ngày bắt đầu để tính'
  const d = new Date(form.value.startDate)
  d.setDate(d.getDate() + 14)
  return d.toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
})

const endDateIso = computed(() => {
  if (!form.value.startDate) return ''
  const d = new Date(form.value.startDate)
  d.setDate(d.getDate() + 14)
  return d.toISOString().split('T')[0]
})

function validate() {
  errors.value = {}
  if (!form.value.name.trim()) errors.value.name = 'Vui lòng nhập tên sprint'
  if (!form.value.goal.trim()) errors.value.goal = 'Vui lòng nhập mục tiêu sprint'
  if (!form.value.startDate) errors.value.startDate = 'Vui lòng chọn ngày bắt đầu'
  return Object.keys(errors.value).length === 0
}

function handleSubmit() {
  if (!validate()) return
  emit('created', {
    id: Date.now(),
    name: form.value.name,
    goal: form.value.goal,
    startDate: form.value.startDate,
    endDate: endDateIso.value,
    status: 0,
  })
}
</script>
