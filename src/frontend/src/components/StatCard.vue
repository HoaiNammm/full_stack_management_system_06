<template>
  <div class="app-card p-md flex flex-col gap-xs interactive-card h-full group"
    :class="borderClass || 'border-outline-variant'">
    <div class="flex justify-between items-center">
      <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ label }}</span>
      <div class="w-9 h-9 rounded-full flex items-center justify-center group-hover:scale-105 transition-transform" :class="iconBg">
        <span class="material-symbols-outlined text-[18px]">{{ icon }}</span>
      </div>
    </div>
    <div class="flex items-baseline gap-sm">
      <span class="font-headline-lg text-headline-lg" :class="valueColor || 'text-on-surface'">{{ value }}</span>
      <span class="font-label-sm text-label-sm px-2 py-0.5 rounded-full" :class="badgeColor">{{ badge }}</span>
    </div>
  </div>
</template>

<script setup>
defineProps(['icon', 'label', 'value', 'badge', 'badgeColor', 'iconBg', 'valueColor', 'borderClass'])
</script>
