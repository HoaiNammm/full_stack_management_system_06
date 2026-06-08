<template>
  <div class="flex items-center justify-between p-md border-b border-outline-variant/50 hover:bg-surface-container-low transition-colors group cursor-pointer">
    <div class="flex items-center gap-md">
      <div class="w-10 h-10 rounded-lg border flex items-center justify-center font-bold font-label-lg" :class="project.color">
        {{ project.initials }}
      </div>
      <div>
        <h4 class="font-label-lg text-label-lg text-on-surface group-hover:text-primary transition-colors">{{ project.name }}</h4>
        <p class="font-body-md text-body-md text-on-surface-variant">{{ project.desc }}</p>
      </div>
    </div>
    <div class="flex items-center gap-xl">
      <div class="hidden sm:block text-right">
        <div class="font-label-md text-label-md text-on-surface">{{ project.tasks }} Tasks</div>
        <div class="font-label-sm text-label-sm" :class="project.statusColor">{{ project.statusText }}</div>
      </div>
      <div class="flex -space-x-2">
        <img v-for="(av, i) in project.avatars" :key="i" :src="av"
          class="w-8 h-8 rounded-full border-2 border-surface-container-lowest object-cover" />
        <div v-if="project.extraCount" class="w-8 h-8 rounded-full border-2 border-surface-container-lowest bg-surface-container-high flex items-center justify-center font-label-sm text-label-sm text-on-surface-variant z-10">
          +{{ project.extraCount }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps(['project'])
</script>
