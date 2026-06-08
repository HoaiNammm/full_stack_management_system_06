<template>
  <div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm"
    :class="{ 'opacity-80 hover:opacity-100 transition-opacity': column.dimmed }">
    <!-- Header -->
    <div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
      <div class="flex items-center gap-2">
        <div class="w-2 h-2 rounded-full" :class="column.dotColor"></div>
        <h3 class="font-label-lg text-label-lg text-on-surface">{{ column.title }}</h3>
        <span class="px-2 py-0.5 rounded-full font-label-sm text-label-sm" :class="column.countBg">{{ column.count }}</span>
      </div>
      <button class="text-on-surface-variant hover:text-primary transition-colors">
        <span class="material-symbols-outlined text-[20px]">more_horiz</span>
      </button>
    </div>

    <!-- Add Task -->
    <div class="px-2 pt-2">
      <button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
        <span class="material-symbols-outlined text-[16px]">add</span> Thêm task
      </button>
    </div>

    <!-- Cards -->
    <div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
      <TaskCard v-for="task in column.tasks" :key="task.id" :task="task" />
    </div>
  </div>
</template>

<script setup>
import TaskCard from './TaskCard.vue'
defineProps(['column'])
</script>
