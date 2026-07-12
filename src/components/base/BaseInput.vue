<script setup>
defineProps({
  modelValue:   [String, Number],
  label:        String,
  error:        String,
  hint:         String,
  required:     { type: Boolean, default: false },
  disabled:     { type: Boolean, default: false },
  placeholder:  String,
  type:         { type: String, default: 'text' },
  autocomplete: String,
  rows:         { type: Number, default: 3 },
  min:          [String, Number],
  max:          [String, Number],
  step:         [String, Number],
  id:           String,
})

defineEmits(['update:modelValue'])

const base = [
  'w-full rounded-xl px-3 py-2 text-sm',
  'bg-white dark:bg-slate-800',
  'text-slate-900 dark:text-slate-100',
  'placeholder:text-slate-400 dark:placeholder:text-slate-500',
  'transition-shadow duration-150',
  'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/30 focus-visible:border-blue-400',
  'disabled:opacity-50 disabled:cursor-not-allowed',
].join(' ')

const normalBorder = 'border border-slate-200 dark:border-slate-700'
const errorBorder  = 'border border-red-400 focus-visible:ring-red-400/30 dark:border-red-500'
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

    <textarea
      v-if="type === 'textarea'"
      :id="id"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
      :rows="rows"
      :placeholder="placeholder"
      :disabled="disabled"
      :required="required"
      :class="[base, 'resize-none', error ? errorBorder : normalBorder]"
    />

    <input
      v-else
      :id="id"
      :type="type"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
      :placeholder="placeholder"
      :disabled="disabled"
      :required="required"
      :autocomplete="autocomplete"
      :min="min"
      :max="max"
      :step="step"
      :class="[base, error ? errorBorder : normalBorder]"
    />

    <p v-if="error" role="alert" class="text-xs text-red-500">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-slate-400 dark:text-slate-500">{{ hint }}</p>
  </div>
</template>
