<script setup>
defineProps({
  modelValue: [String, Number],
  label:      String,
  error:      String,
  hint:       String,
  required:   { type: Boolean, default: false },
  disabled:   { type: Boolean, default: false },
  id:         String,
})

defineEmits(['update:modelValue'])
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label
      v-if="label"
      :for="id"
      class="text-sm font-medium text-slate-700 dark:text-slate-300 select-none"
    >
      {{ label }}<span v-if="required" class="text-red-500 ml-0.5" aria-hidden="true">*</span>
    </label>

    <select
      :id="id"
      :value="modelValue"
      @change="$emit('update:modelValue', $event.target.value)"
      :disabled="disabled"
      :required="required"
      :class="[
        'w-full rounded-xl px-3 py-2 text-sm',
        'bg-white dark:bg-slate-800',
        'text-slate-900 dark:text-slate-100',
        'transition-shadow duration-150',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/30 focus-visible:border-blue-400',
        'disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer',
        error
          ? 'border border-red-400 dark:border-red-500'
          : 'border border-slate-200 dark:border-slate-700',
      ]"
    >
      <slot />
    </select>

    <p v-if="error" role="alert" class="text-xs text-red-500">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-slate-400 dark:text-slate-500">{{ hint }}</p>
  </div>
</template>
