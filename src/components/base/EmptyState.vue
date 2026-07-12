<script setup>
/**
 * EmptyState — Impeccable harden.md compliant empty state.
 *
 * Pattern: icon → title → description → CTA (all optional except title).
 * Replaces the inconsistent empty states scattered across the app
 * (some have only text, some have icon+text, none have all 4 layers).
 *
 * Sizes:
 *   sm  — for inline sections (column in Kanban, empty card body)
 *   md  — for page sections (tab content, list area)
 *   lg  — for full-page states (Project not found, no projects yet)
 *
 * Usage:
 *   <EmptyState
 *     :icon="FolderOpen"
 *     title="No projects yet"
 *     description="Create your first project to start organizing your team's work."
 *     action-label="New Project"
 *     @action="isDialogOpen = true"
 *   />
 *
 *   <!-- Without CTA (viewer role) -->
 *   <EmptyState
 *     :icon="Bell"
 *     title="All caught up"
 *     description="No notifications right now."
 *   />
 *
 *   <!-- Small variant (Kanban column) -->
 *   <EmptyState size="sm" title="Drop tasks here" />
 */
defineProps({
  icon:        Object,   // Lucide component — passed via :icon="FolderOpen"
  title:       { type: String, required: true },
  description: String,
  actionLabel: String,
  size:        { type: String, default: 'md' },  // sm | md | lg
})

defineEmits(['action'])

const containerCls = {
  sm: 'py-8',
  md: 'py-14',
  lg: 'py-20',
}

const iconWrapCls = {
  sm: 'w-10 h-10 mb-3',
  md: 'w-14 h-14 mb-4',
  lg: 'w-16 h-16 mb-5',
}

const iconSizeCls = {
  sm: 'size-4',
  md: 'size-6',
  lg: 'size-7',
}

const titleCls = {
  sm: 'text-sm',
  md: 'text-base',
  lg: 'text-lg',
}

const descCls = {
  sm: 'text-xs max-w-xs',
  md: 'text-sm max-w-sm',
  lg: 'text-sm max-w-md',
}
</script>

<template>
  <div
    :class="[
      'flex flex-col items-center text-center select-none',
      containerCls[size],
    ]"
  >
    <!-- Icon in a neutral pill -->
    <div
      v-if="icon"
      :class="[
        'rounded-full bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center shrink-0',
        iconWrapCls[size],
      ]"
      aria-hidden="true"
    >
      <component
        :is="icon"
        :class="['text-zinc-400 dark:text-zinc-500', iconSizeCls[size]]"
      />
    </div>

    <!-- Title -->
    <h3
      :class="[
        'font-medium text-zinc-800 dark:text-zinc-200',
        description || actionLabel ? 'mb-1' : '',
        titleCls[size],
      ]"
    >
      {{ title }}
    </h3>

    <!-- Description -->
    <p
      v-if="description"
      :class="[
        'text-zinc-500 dark:text-zinc-400 leading-relaxed',
        actionLabel ? 'mb-4' : '',
        descCls[size],
      ]"
    >
      {{ description }}
    </p>

    <!-- CTA — only rendered when caller provides both label and @action listener -->
    <button
      v-if="actionLabel"
      @click="$emit('action')"
      class="inline-flex items-center gap-2 px-4 py-2 text-sm rounded font-medium
             bg-gradient-to-br from-blue-500 to-blue-600 text-white
             hover:opacity-90 transition
             focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-1 dark:focus-visible:ring-offset-zinc-900"
    >
      {{ actionLabel }}
    </button>
  </div>
</template>
