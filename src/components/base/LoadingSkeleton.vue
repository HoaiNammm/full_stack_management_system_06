<script setup>
/**
 * LoadingSkeleton — Impeccable harden.md compliant: skeleton > spinner for page loads.
 *
 * Types match the actual data shapes in the app:
 *   stat   — 4-column stat card grid (Dashboard StatsGrid)
 *   card   — project card grid (ProjectsView)
 *   list   — task / notification row list (ProjectTasks, MyWork, Notifications)
 *   text   — generic lines of text (description areas, activity feed)
 *
 * The shimmer animation is the standard pulse. prefers-reduced-motion in index.css
 * automatically kills the animation for users who need it — no extra code required.
 *
 * Usage:
 *   <LoadingSkeleton type="stat" />               — 4 stat cards
 *   <LoadingSkeleton type="card" :count="6" />    — 6 project card skeletons
 *   <LoadingSkeleton type="list" :count="8" />    — 8 task row skeletons
 *   <LoadingSkeleton type="text" :count="3" />    — 3 lines of text
 */
defineProps({
  type:  { type: String, default: 'list' },  // stat | card | list | text
  count: { type: Number, default: 4 },
})
</script>

<template>
  <!-- ── Stat cards (Dashboard) ─────────────────────────────────────────── -->
  <div
    v-if="type === 'stat'"
    class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6"
    aria-busy="true"
    aria-label="Loading statistics"
  >
    <div
      v-for="i in count"
      :key="i"
      class="bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 rounded-lg p-5 animate-pulse"
    >
      <div class="flex items-start justify-between gap-4">
        <div class="flex-1 space-y-2">
          <div class="h-3 w-28 bg-zinc-200 dark:bg-zinc-700 rounded" />
          <div class="h-8 w-16 bg-zinc-200 dark:bg-zinc-700 rounded" />
          <div class="h-2.5 w-20 bg-zinc-100 dark:bg-zinc-800 rounded" />
        </div>
        <div class="w-10 h-10 shrink-0 bg-zinc-200 dark:bg-zinc-700 rounded-xl" />
      </div>
    </div>
  </div>

  <!-- ── Project cards ──────────────────────────────────────────────────── -->
  <div
    v-else-if="type === 'card'"
    class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5"
    aria-busy="true"
    aria-label="Loading projects"
  >
    <div
      v-for="i in count"
      :key="i"
      class="bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 rounded-lg p-5 animate-pulse"
    >
      <div class="space-y-3">
        <div class="h-4 w-3/4 bg-zinc-200 dark:bg-zinc-700 rounded" />
        <div class="h-3 w-full bg-zinc-100 dark:bg-zinc-800 rounded" />
        <div class="h-3 w-2/3 bg-zinc-100 dark:bg-zinc-800 rounded" />
        <div class="flex items-center justify-between pt-1">
          <div class="h-5 w-16 bg-zinc-200 dark:bg-zinc-700 rounded-full" />
          <div class="h-4 w-10 bg-zinc-100 dark:bg-zinc-800 rounded" />
        </div>
        <div class="pt-1 space-y-1.5">
          <div class="flex justify-between">
            <div class="h-2.5 w-12 bg-zinc-100 dark:bg-zinc-800 rounded" />
            <div class="h-2.5 w-8 bg-zinc-100 dark:bg-zinc-800 rounded" />
          </div>
          <div class="h-1.5 w-full bg-zinc-100 dark:bg-zinc-800 rounded-full" />
        </div>
        <div class="flex gap-3 pt-1">
          <div class="h-3 w-16 bg-zinc-100 dark:bg-zinc-800 rounded" />
          <div class="h-3 w-14 bg-zinc-100 dark:bg-zinc-800 rounded" />
        </div>
      </div>
    </div>
  </div>

  <!-- ── List rows (tasks / notifications / activity) ───────────────────── -->
  <div
    v-else-if="type === 'list'"
    class="space-y-2"
    aria-busy="true"
    aria-label="Loading items"
  >
    <div
      v-for="i in count"
      :key="i"
      class="flex items-center gap-3 px-4 py-3 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 animate-pulse"
    >
      <!-- Status indicator -->
      <div class="size-4 rounded bg-zinc-200 dark:bg-zinc-700 shrink-0" />
      <!-- Title + meta -->
      <div class="flex-1 space-y-1.5 min-w-0">
        <div class="h-3.5 bg-zinc-200 dark:bg-zinc-700 rounded" :style="{ width: i % 2 === 0 ? '65%' : '80%' }" />
        <div class="h-2.5 bg-zinc-100 dark:bg-zinc-800 rounded w-1/3" />
      </div>
      <!-- Badge -->
      <div class="h-5 w-14 bg-zinc-100 dark:bg-zinc-800 rounded shrink-0" />
    </div>
  </div>

  <!-- ── Generic text lines ─────────────────────────────────────────────── -->
  <div
    v-else
    class="space-y-2 animate-pulse"
    aria-busy="true"
    aria-label="Loading"
  >
    <div
      v-for="i in count"
      :key="i"
      class="h-3 bg-zinc-200 dark:bg-zinc-700 rounded"
      :style="{ width: i === count ? '55%' : i % 3 === 0 ? '85%' : '100%' }"
    />
  </div>
</template>
