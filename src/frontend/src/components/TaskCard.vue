<template>
  <div class="p-3 rounded border cursor-grab hover:shadow-md transition-shadow group relative overflow-hidden"
    :class="task.done
      ? 'bg-surface-bright border-outline-variant/50'
      : task.accentBar
        ? 'bg-surface-container-lowest border-primary/30 shadow-[0_4px_6px_-1px_rgba(0,0,0,0.1)]'
        : 'bg-surface-container-lowest border-outline-variant shadow-[0_1px_2px_rgba(0,0,0,0.05)]'">
    <!-- Accent bar -->
    <div v-if="task.accentBar" class="absolute left-0 top-0 bottom-0 w-1 bg-secondary rounded-l"></div>
    <div :class="{ 'pl-2': task.accentBar }">
      <!-- Tags + Priority -->
      <div class="flex justify-between items-start mb-2">
        <div class="flex flex-wrap gap-1">
          <span v-for="tag in task.tags" :key="tag.text"
            class="px-1.5 py-0.5 rounded font-label-sm text-label-sm" :class="tag.color"
            :style="{ opacity: task.done ? 0.7 : 1 }">
            {{ tag.text }}
          </span>
        </div>
        <!-- Priority icon -->
        <span v-if="task.done" class="material-symbols-outlined text-[16px] text-secondary" style="font-variation-settings: 'FILL' 1">check_circle</span>
        <span v-else-if="task.priority === 'high'" class="material-symbols-outlined text-[16px] text-error" title="Ủu tiên Cao">keyboard_double_arrow_up</span>
        <span v-else-if="task.priority === 'medium'" class="material-symbols-outlined text-[16px] text-tertiary" title="Ủu tiên Trung bình">remove</span>
        <span v-else-if="task.priority === 'low'" class="material-symbols-outlined text-[16px] text-outline" title="Ủu tiên Thấp">keyboard_arrow_down</span>
      </div>

      <!-- Title -->
      <h4 class="font-headline-sm text-headline-sm mb-1 leading-snug line-clamp-2"
        :class="task.done ? 'text-on-surface-variant line-through' : 'text-on-surface'">
        {{ task.title }}
      </h4>
      <p class="font-label-md text-label-md text-outline mb-3">#{{ task.shortId }}</p>

      <!-- Progress -->
      <div v-if="task.progress !== undefined" class="flex items-center gap-2 mb-3">
        <div class="flex-1 h-1.5 bg-surface-container rounded-full overflow-hidden">
          <div class="h-full bg-tertiary" :style="`width:${task.progress}%`"></div>
        </div>
        <span class="font-label-sm text-label-sm text-outline">{{ task.progress }}%</span>
      </div>

      <!-- Footer -->
      <div class="flex justify-between items-end mt-auto" :class="{ 'opacity-70': task.done }">
        <div class="flex items-center gap-2">
          <!-- In progress indicator -->
          <div v-if="task.inProgress" class="flex items-center gap-1 text-secondary font-medium">
            <span class="material-symbols-outlined text-[14px] animate-spin" style="font-variation-settings: 'FILL' 1">sync</span>
            <span class="font-label-sm text-label-sm">Đang code</span>
          </div>
          <!-- Deadline -->
          <div v-else-if="task.deadline" class="flex items-center gap-1" :class="task.deadlineColor">
            <span class="material-symbols-outlined text-[14px]">
              {{ task.deadline === 'Hôm nay' ? 'schedule' : task.unassigned ? 'calendar_today' : 'calendar_today' }}
            </span>
            <span class="font-label-sm text-label-sm">{{ task.deadline }}</span>
          </div>
          <!-- Done date -->
          <span v-else-if="task.doneDate" class="font-label-sm text-label-sm text-outline">{{ task.doneDate }}</span>
          <!-- Comments -->
          <div v-if="task.comments" class="flex items-center gap-1 text-outline">
            <span class="material-symbols-outlined text-[14px]">chat_bubble_outline</span>
            <span class="font-label-sm text-label-sm">{{ task.comments }}</span>
          </div>
        </div>
        <!-- Avatar -->
        <div class="flex -space-x-2">
          <img v-if="task.avatar" :src="task.avatar"
            class="w-6 h-6 rounded-full border-2 border-surface-container-lowest object-cover" />
          <div v-if="task.avatarText"
            class="w-6 h-6 rounded-full border-2 border-surface-container-lowest text-white flex items-center justify-center font-label-sm text-[10px]"
            :style="{ backgroundColor: task.avatarColor || '#3525cd' }"
            :title="task.assignedName || ''">
            {{ task.avatarText }}
          </div>
          <div v-if="task.unassigned"
            class="w-6 h-6 rounded-full border-2 border-surface-container-lowest bg-surface-container flex items-center justify-center text-outline">
            <span class="material-symbols-outlined text-[14px]">person</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps(['task'])
</script>
