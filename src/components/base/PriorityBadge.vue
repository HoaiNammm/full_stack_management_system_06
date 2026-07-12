<script setup>
/**
 * PriorityBadge — single source of truth for priority colors.
 *
 * IMPORTANT — semantic correctness:
 *   High   → red   (danger, urgent)
 *   Medium → amber (caution)
 *   Low    → zinc  (neutral, low urgency)
 *
 * This fixes the inversion bug in ProjectAnalytics.vue (Low=red, High=green)
 * and ProjectOverview.vue (High=green border).
 *
 * Variants:
 *   badge (default) — colored pill with text label
 *   dot             — small circle, used in compact lists (title attribute for a11y)
 *
 * Usage:
 *   <PriorityBadge priority="High" />
 *   <PriorityBadge priority="Medium" variant="dot" />
 */
defineProps({
  priority: { type: String, required: true },
  variant:  { type: String, default: 'badge' },  // badge | dot
})

const badgeCls = {
  High:   'bg-red-100 text-red-700 dark:bg-red-900/50 dark:text-red-300',
  Medium: 'bg-amber-100 text-amber-700 dark:bg-amber-900/50 dark:text-amber-300',
  Low:    'bg-zinc-100 text-zinc-500 dark:bg-zinc-700 dark:text-zinc-400',
}

const dotCls = {
  High:   'bg-red-500',
  Medium: 'bg-amber-500',
  Low:    'bg-zinc-400 dark:bg-zinc-500',
}

const fallbackBadge = 'bg-zinc-100 text-zinc-500 dark:bg-zinc-700 dark:text-zinc-400'
const fallbackDot   = 'bg-zinc-400'
</script>

<template>
  <!-- Dot variant: compact, used inside task cards / list rows -->
  <span
    v-if="variant === 'dot'"
    :title="priority"
    :aria-label="`Priority: ${priority}`"
    :class="['inline-block size-2 rounded-full shrink-0', dotCls[priority] ?? fallbackDot]"
  />

  <!-- Badge variant: full label with color background -->
  <span
    v-else
    :class="[
      'inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium whitespace-nowrap',
      badgeCls[priority] ?? fallbackBadge,
    ]"
  >
    {{ priority }}
  </span>
</template>
