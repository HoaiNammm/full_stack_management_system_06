<template>
  <div class="relative">
    <button @click="toggleOpen"
      class="px-3 py-1.5 rounded-md border font-label-md text-label-md flex items-center gap-1.5 hover:bg-surface-container-high transition-colors whitespace-nowrap"
      :class="isActive
        ? 'border-primary bg-primary/10 text-primary'
        : 'border-outline-variant bg-surface text-on-surface-variant'">
      <span class="material-symbols-outlined text-[16px]">{{ icon }}</span>
      {{ label }}
      <span v-if="isActive"
        class="min-w-[18px] h-[18px] bg-primary text-on-primary text-[11px] font-bold rounded-full flex items-center justify-center px-1">
        {{ modelValue.length }}
      </span>
      <span class="material-symbols-outlined text-[16px] transition-transform" :class="{ 'rotate-180': open }">
        arrow_drop_down
      </span>
    </button>

    <!-- Dropdown -->
    <div v-if="open && options?.length"
      class="absolute top-full left-0 mt-1 min-w-[180px] bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg z-50 overflow-hidden py-1">
      <label v-for="opt in options" :key="opt.value"
        class="flex items-center gap-2 px-3 py-1.5 hover:bg-surface-container-low cursor-pointer select-none">
        <input type="checkbox" :value="opt.value" v-model="localValue" class="accent-primary w-4 h-4 rounded" />
        <span v-if="opt.icon" class="material-symbols-outlined text-[15px] text-on-surface-variant">{{ opt.icon }}</span>
        <span class="font-label-md text-label-md text-on-surface">{{ opt.label }}</span>
      </label>
      <div v-if="isActive" class="border-t border-outline-variant/50 mt-1 px-3 py-1.5">
        <button @click="clear" class="font-label-sm text-label-sm text-error hover:underline">Xóa bộ lọc</button>
      </div>
    </div>

    <!-- Backdrop -->
    <div v-if="open" class="fixed inset-0 z-40" @click="open = false"></div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  icon:       String,
  label:      String,
  options:    Array,
  modelValue: { type: Array, default: () => [] },
})
const emit = defineEmits(['update:modelValue'])

const open     = ref(false)
const isActive = computed(() => props.modelValue?.length > 0)

const localValue = computed({
  get: () => props.modelValue || [],
  set: (val) => emit('update:modelValue', val),
})

function toggleOpen() {
  if (!props.options?.length) return
  open.value = !open.value
}

function clear() {
  emit('update:modelValue', [])
  open.value = false
}
</script>
