<template>
  <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-2 w-80 pointer-events-none">
    <TransitionGroup name="toast">
      <div v-for="ev in events" :key="ev.id"
        class="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-xl p-4 flex gap-3 items-start pointer-events-auto">
        <div class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0" :class="ev.iconBg">
          <span class="material-symbols-outlined text-[20px]" style="font-variation-settings: 'FILL' 1">{{ ev.icon }}</span>
        </div>
        <div class="flex-1 min-w-0">
          <span class="text-[9px] font-bold uppercase tracking-widest text-on-surface-variant bg-surface-container px-1.5 py-0.5 rounded">
            EVENT PUBLISHED
          </span>
          <p class="font-mono font-label-lg text-label-lg text-primary mt-1">{{ ev.type }}</p>
          <p class="font-body-md text-body-md text-on-surface-variant mt-1 text-xs leading-relaxed">{{ ev.summary }}</p>
          <p class="font-label-sm text-label-sm text-outline mt-1">{{ ev.time }}</p>
        </div>
        <button @click="$emit('dismiss', ev.id)"
          class="text-on-surface-variant hover:text-on-surface flex-shrink-0 transition-colors">
          <span class="material-symbols-outlined text-[18px]">close</span>
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup>
defineProps(['events'])
defineEmits(['dismiss'])
</script>

<style scoped>
.toast-enter-active { transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); }
.toast-leave-active { transition: all 0.25s ease-in; }
.toast-enter-from { opacity: 0; transform: translateX(80px) scale(0.9); }
.toast-leave-to { opacity: 0; transform: translateX(80px); }
</style>
