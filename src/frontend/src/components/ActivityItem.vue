<template>
  <div class="flex gap-sm relative">
    <div v-if="showLine" class="absolute left-4 top-8 bottom-[-16px] w-px bg-outline-variant/50"></div>
    <img v-if="activity.avatar" :src="activity.avatar"
      class="w-8 h-8 rounded-full z-10 outline outline-2 outline-surface-container-lowest flex-shrink-0 object-cover" />
    <div v-else class="w-8 h-8 rounded-full flex items-center justify-center z-10 outline outline-2 outline-surface-container-lowest flex-shrink-0 font-label-sm"
      :class="activity.iconBg">
      <span class="material-symbols-outlined text-[16px]">{{ activity.icon }}</span>
    </div>
    <div class="flex-1 min-w-0">
      <p class="font-body-md text-body-md text-on-surface">
        <span class="font-label-md font-bold">{{ activity.name }}</span>
        {{ activity.action }}
        <span class="text-primary cursor-pointer hover:underline"> {{ activity.link }}</span>
        <span v-if="activity.badge"
          class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-bold ml-1 uppercase"
          :class="activity.badgeColor">{{ activity.badge }}</span>
      </p>
      <div v-if="activity.comment"
        class="mt-2 p-2 bg-surface-container-low rounded border border-outline-variant/30 font-body-md text-[13px] text-on-surface-variant italic">
        {{ activity.comment }}
      </div>
      <p class="font-label-sm text-label-sm text-on-surface-variant mt-1">{{ activity.time }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps(['activity', 'showLine'])
</script>
