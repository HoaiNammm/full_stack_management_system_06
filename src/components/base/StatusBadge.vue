<script setup>
/**
 * StatusBadge — single source of truth for ALL status colors in the app.
 *
 * Covers both task statuses (Backlog/ToDo/InProgress/Review/Done)
 * and project statuses (Planning/Active/OnHold/Completed/Cancelled).
 *
 * Why one component: prevents the current situation where 6+ files each define
 * their own statusColors map with slight differences.
 *
 * Usage:
 *   <StatusBadge status="InProgress" />
 *   <StatusBadge status="Active" />
 *   <StatusBadge status="Done" size="xs" />
 */
defineProps({
  status: { type: String, required: true },
  size:   { type: String, default: 'sm' },  // xs | sm
  dot:    { type: Boolean, default: false }, // pill with a colored status dot
})

// Task statuses (from backend enum: Backlog/ToDo/InProgress/Review/Done)
// Note: "Blocked" is NOT in the backend enum per COMPONENT_GUIDE.md TODO — excluded.
const taskCls = {
  Backlog:    'bg-zinc-200 text-zinc-800 dark:bg-zinc-700 dark:text-zinc-300',
  ToDo:       'bg-blue-100 text-blue-800 dark:bg-blue-900/60 dark:text-blue-300',
  InProgress: 'bg-amber-100 text-amber-800 dark:bg-amber-900/60 dark:text-amber-300',
  Review:     'bg-purple-100 text-purple-800 dark:bg-purple-900/60 dark:text-purple-300',
  Done:       'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/60 dark:text-emerald-300',
}

// Project statuses
const projectCls = {
  Planning:  'bg-zinc-200 text-zinc-900 dark:bg-zinc-700 dark:text-zinc-200',
  Active:    'bg-emerald-200 text-emerald-900 dark:bg-emerald-900/60 dark:text-emerald-300',
  OnHold:    'bg-amber-200 text-amber-900 dark:bg-amber-900/60 dark:text-amber-300',
  Completed: 'bg-blue-200 text-blue-900 dark:bg-blue-900/60 dark:text-blue-300',
  Cancelled: 'bg-purple-200 text-purple-900 dark:bg-purple-900/60 dark:text-purple-300',
}

// Human-readable labels (handles camelCase → "In Progress", "On Hold")
const label = {
  Backlog:    'Backlog',
  ToDo:       'To Do',
  InProgress: 'In Progress',
  Review:     'Review',
  Done:       'Done',
  Planning:   'Planning',
  Active:     'Active',
  OnHold:     'On Hold',
  Completed:  'Completed',
  Cancelled:  'Cancelled',
}

const allCls = { ...taskCls, ...projectCls }

const fallbackCls = 'bg-zinc-100 text-zinc-700 dark:bg-zinc-700 dark:text-zinc-300'

// Dot variant: lighter pill background + a solid dot, e.g. "● Ready" style
const dotPillCls = {
  Backlog:    'bg-zinc-50 text-zinc-700 dark:bg-zinc-800/60 dark:text-zinc-300',
  ToDo:       'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
  InProgress: 'bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300',
  Review:     'bg-purple-50 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300',
  Done:       'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300',
  Planning:   'bg-zinc-50 text-zinc-700 dark:bg-zinc-800/60 dark:text-zinc-300',
  Active:     'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
  OnHold:     'bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300',
  Completed:  'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300',
  Cancelled:  'bg-rose-50 text-rose-700 dark:bg-rose-900/30 dark:text-rose-300',
}
const dotColorCls = {
  Backlog: 'bg-zinc-400', ToDo: 'bg-blue-500', InProgress: 'bg-amber-500',
  Review: 'bg-purple-500', Done: 'bg-emerald-500',
  Planning: 'bg-zinc-400', Active: 'bg-blue-500', OnHold: 'bg-amber-500',
  Completed: 'bg-emerald-500', Cancelled: 'bg-rose-500',
}
const fallbackDotPillCls = 'bg-zinc-50 text-zinc-700 dark:bg-zinc-800/60 dark:text-zinc-300'
const fallbackDotColorCls = 'bg-zinc-400'
</script>

<template>
  <span
    v-if="dot"
    :class="[
      'inline-flex items-center gap-1.5 rounded-full font-medium whitespace-nowrap',
      dotPillCls[status] ?? fallbackDotPillCls,
      size === 'xs' ? 'px-2 py-0.5 text-[10px]' : 'px-2.5 py-1 text-xs',
    ]"
  >
    <span :class="['size-1.5 rounded-full flex-shrink-0', dotColorCls[status] ?? fallbackDotColorCls]" aria-hidden="true" />
    {{ label[status] ?? status }}
  </span>
  <span
    v-else
    :class="[
      'inline-flex items-center rounded font-medium whitespace-nowrap',
      allCls[status] ?? fallbackCls,
      size === 'xs' ? 'px-1.5 py-0.5 text-[10px]' : 'px-2 py-0.5 text-xs',
    ]"
  >
    {{ label[status] ?? status }}
  </span>
</template>
