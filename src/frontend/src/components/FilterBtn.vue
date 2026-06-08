<template>
  <button class="px-3 py-1.5 rounded-md border border-outline-variant bg-surface text-on-surface-variant font-label-md text-label-md flex items-center gap-1.5 hover:bg-surface-container-high transition-colors whitespace-nowrap">
    <span class="material-symbols-outlined text-[16px]">{{ icon }}</span>
    {{ label }}
    <span class="material-symbols-outlined text-[16px]">arrow_drop_down</span>
  </button>
</template>

<script setup>
defineProps(['icon', 'label'])
</script>
