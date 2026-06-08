<template>
  <div class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
    <div class="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant w-full max-w-lg flex flex-col max-h-[90vh]">
      <!-- Header -->
      <div class="flex items-center justify-between p-md border-b border-outline-variant flex-shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
            <span class="material-symbols-outlined text-primary text-[20px]">create_new_folder</span>
          </div>
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Tạo dự án mới</h2>
        </div>
        <button @click="$emit('close')" class="text-on-surface-variant hover:text-on-surface p-1 rounded-full hover:bg-surface-container-high transition-colors">
          <span class="material-symbols-outlined">close</span>
        </button>
      </div>

      <!-- Body -->
      <div class="p-md flex flex-col gap-md overflow-y-auto">
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

          <!-- Preview -->
          <div class="flex items-center gap-3 mt-2 p-3 bg-surface-container-low rounded-xl border border-outline-variant/50">
            <div class="w-10 h-10 rounded-lg flex items-center justify-center font-bold text-white font-label-lg flex-shrink-0"
              :style="{ backgroundColor: form.color }">
              {{ previewInitials }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-label-lg text-label-lg text-on-surface truncate">{{ form.name || 'Tên dự án' }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant">Xem trước thẻ dự án</p>
            </div>
            <div class="h-5 w-1 rounded-full flex-shrink-0" :style="{ backgroundColor: form.color }"></div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="p-md border-t border-outline-variant flex justify-end gap-3 flex-shrink-0">
        <button @click="$emit('close')"
          class="px-md py-sm rounded-lg border border-outline-variant font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
          Hủy
        </button>
        <button @click="handleSubmit"
          class="flex items-center gap-xs px-md py-sm rounded-lg bg-primary text-on-primary font-label-lg text-label-lg hover:opacity-90 transition-opacity shadow-sm">
          <span class="material-symbols-outlined text-[18px]">add</span>
          Tạo dự án
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const emit = defineEmits(['close', 'created'])

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

const form = ref({ name: '', description: '', startDate: '', endDate: '', color: '#3525cd' })
const errors = ref({})

const previewInitials = computed(() => {
  if (!form.value.name) return 'PM'
  return form.value.name.split(' ').map(w => w[0]).slice(0, 3).join('').toUpperCase()
})

function validate() {
  errors.value = {}
  if (!form.value.name.trim()) errors.value.name = 'Vui lòng nhập tên dự án'
  if (!form.value.startDate) errors.value.startDate = 'Vui lòng chọn ngày bắt đầu'
  return Object.keys(errors.value).length === 0
}

function handleSubmit() {
  if (!validate()) return
  emit('created', {
    id: Date.now(),
    initials: previewInitials.value.slice(0, 3),
    color: form.value.color,
    name: form.value.name,
    desc: form.value.description || 'Dự án mới',
    memberCount: 1,
    sprint: null,
    status: 0,
    startDate: form.value.startDate
      ? new Date(form.value.startDate).toLocaleDateString('vi-VN') : '',
    endDate: form.value.endDate
      ? new Date(form.value.endDate).toLocaleDateString('vi-VN') : '',
  })
}
</script>
