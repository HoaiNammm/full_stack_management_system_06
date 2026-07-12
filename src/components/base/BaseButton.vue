<script setup>
defineProps({
  variant: {
    type: String,
    default: 'secondary',
    validator: v => ['primary', 'secondary', 'danger', 'ghost', 'icon'].includes(v),
  },
  size: {
    type: String,
    default: 'md',
    validator: v => ['xs', 'sm', 'md', 'lg'].includes(v),
  },
  loading:   { type: Boolean, default: false },
  disabled:  { type: Boolean, default: false },
  type:      { type: String, default: 'button' },
  fullWidth: { type: Boolean, default: false },
})

const variantCls = {
  primary:   'bg-black text-white hover:bg-neutral-800 shadow-sm hover:shadow-md',
  secondary: 'border bg-white text-black hover:bg-slate-50 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700',
  danger:    'border border-transparent text-red-500 hover:bg-red-50 dark:hover:bg-slate-800',
  ghost:     'border-transparent text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800',
  icon:      'size-9 justify-center border bg-white text-slate-600 hover:bg-slate-50 hover:text-slate-900 dark:bg-slate-800 dark:text-slate-400 dark:hover:bg-slate-700',
}

const borderCls = {
  primary:   '',
  secondary: 'border-slate-200 dark:border-slate-700',
  danger:    '',
  ghost:     '',
  icon:      'border-slate-200 dark:border-slate-700',
}

const sizeCls = {
  xs: 'px-2.5 py-1 text-xs',
  sm: 'px-4 py-1.5 text-xs',
  md: 'px-5 py-2 text-sm',
  lg: 'px-6 py-2.5 text-sm',
}
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center gap-2 rounded-xl font-medium',
      'transition-all duration-150',
      'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-black focus-visible:ring-offset-1',
      'disabled:opacity-50 disabled:cursor-not-allowed',
      variantCls[variant],
      borderCls[variant],
      variant !== 'icon' ? sizeCls[size] : '',
      fullWidth ? 'w-full justify-center' : '',
    ]"
  >
    <slot />
    <svg
      v-if="loading"
      class="animate-spin size-3.5 shrink-0"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
    >
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
    </svg>
  </button>
</template>
