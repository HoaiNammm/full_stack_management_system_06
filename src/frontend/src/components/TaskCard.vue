<template>
  <article
    class="relative overflow-hidden rounded-2xl border bg-surface-container-lowest p-3 shadow-sm transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg"
    :class="task.done ? 'border-outline-variant/50 opacity-80' : 'border-outline-variant hover:border-primary/50'"
  >
    <div v-if="task.thumbnail" class="mb-3 h-28 overflow-hidden rounded-xl bg-surface-container">
      <img :src="task.thumbnail" class="h-full w-full object-cover transition-transform duration-300 hover:scale-105" alt="" />
    </div>

    <div class="flex items-start justify-between gap-2">
      <div class="flex flex-wrap gap-1">
        <span
          v-for="tag in task.tags"
          :key="tag.text"
          class="soft-badge"
          :class="tag.color"
        >
          {{ tag.text }}
        </span>
      </div>
      <span class="material-symbols-outlined text-[18px]" :class="priorityClass" :title="priorityTitle">
        {{ priorityIcon }}
      </span>
    </div>

    <h4
      class="mt-3 font-headline-sm text-headline-sm leading-snug line-clamp-2"
      :class="task.done ? 'text-on-surface-variant line-through' : 'text-on-surface'"
    >
      {{ task.title }}
    </h4>
    <p class="mt-1 font-label-md text-label-md text-outline">#{{ task.shortId }}</p>

    <p v-if="task.description" class="mt-3 font-body-sm text-body-sm text-on-surface-variant line-clamp-2">
      {{ task.description }}
    </p>

    <div v-if="task.progress !== undefined" class="mt-3 rounded-xl bg-surface-container-low p-2">
      <div class="mb-1 flex items-center justify-between">
        <span class="font-label-sm text-label-sm text-on-surface-variant">Progress</span>
        <span class="font-label-sm text-label-sm text-on-surface">{{ task.progress }}%</span>
      </div>
      <div class="h-1.5 overflow-hidden rounded-full bg-surface-container">
        <div class="h-full rounded-full bg-primary" :style="{ width: `${task.progress}%` }"></div>
      </div>
    </div>

    <div class="mt-3 flex items-center justify-between gap-2">
      <div class="flex flex-wrap items-center gap-2 text-outline">
        <span v-if="task.deadline" class="inline-flex items-center gap-1 font-label-sm text-label-sm" :class="task.deadlineColor">
          <span class="material-symbols-outlined text-[14px]">calendar_today</span>
          {{ task.deadline }}
        </span>
        <span class="inline-flex items-center gap-1 font-label-sm text-label-sm">
          <span class="material-symbols-outlined text-[14px]">chat_bubble_outline</span>
          {{ task.comments || 0 }}
        </span>
        <span class="inline-flex items-center gap-1 font-label-sm text-label-sm">
          <span class="material-symbols-outlined text-[14px]">attach_file</span>
          {{ task.attachments || 0 }}
        </span>
      </div>

      <div class="flex -space-x-2">
        <img
          v-if="task.avatar"
          :src="task.avatar"
          class="w-7 h-7 rounded-full border-2 border-surface-container-lowest object-cover"
        />
        <div
          v-if="task.avatarText"
          class="w-7 h-7 rounded-full border-2 border-surface-container-lowest text-white flex items-center justify-center font-label-sm text-[10px]"
          :style="{ backgroundColor: task.avatarColor || '#3525cd' }"
          :title="task.assignedName || ''"
        >
          {{ task.avatarText }}
        </div>
        <div
          v-if="task.unassigned"
          class="w-7 h-7 rounded-full border-2 border-surface-container-lowest bg-surface-container flex items-center justify-center text-outline"
        >
          <span class="material-symbols-outlined text-[14px]">person</span>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps(['task'])

const priorityIcon = computed(() => {
  if (props.task.done) return 'check_circle'
  if (props.task.priority === 'high') return 'keyboard_double_arrow_up'
  if (props.task.priority === 'low') return 'keyboard_arrow_down'
  return 'remove'
})

const priorityClass = computed(() => {
  if (props.task.done) return 'text-secondary'
  if (props.task.priority === 'high') return 'text-error'
  if (props.task.priority === 'low') return 'text-outline'
  return 'text-tertiary'
})

const priorityTitle = computed(() => {
  if (props.task.priority === 'high') return 'Ưu tiên cao'
  if (props.task.priority === 'low') return 'Ưu tiên thấp'
  return 'Ưu tiên trung bình'
})
</script>
