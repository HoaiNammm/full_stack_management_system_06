<template>
  <nav class="w-[260px] h-screen fixed left-0 top-0 bg-surface-container-low border-r border-outline-variant shadow-sm flex flex-col py-lg px-md z-40">
    <!-- Brand -->
    <div class="flex items-center gap-sm mb-xl px-xs">
      <div class="w-10 h-10 rounded-lg bg-primary flex items-center justify-center text-on-primary font-bold font-headline-sm">
        PM
      </div>
      <div>
        <h1 class="font-headline-sm text-headline-sm font-bold text-primary">Project Manager</h1>
        <p class="font-label-sm text-label-sm text-on-surface-variant">Microservices Architecture</p>
      </div>
    </div>

    <!-- Links -->
    <div class="flex flex-col gap-xs flex-grow">
      <RouterLink
        to="/dashboard"
        class="flex items-center gap-sm px-sm py-sm rounded-lg transition-all active:scale-95"
        :class="$route.path === '/dashboard'
          ? 'text-primary font-bold border-r-4 border-primary bg-primary/10'
          : 'text-on-surface-variant hover:text-primary hover:bg-surface-container-high'"
      >
        <span class="material-symbols-outlined" :style="{ fontVariationSettings: $route.path === '/dashboard' ? `'FILL' 1` : `'FILL' 0` }">dashboard</span>
        <span class="font-label-lg text-label-lg">Tổng quan</span>
      </RouterLink>

      <RouterLink
        to="/projects"
        class="flex items-center gap-sm px-sm py-sm rounded-lg transition-all active:scale-95"
        :class="$route.path.startsWith('/projects')
          ? 'text-primary font-bold border-r-4 border-primary bg-primary/10'
          : 'text-on-surface-variant hover:text-primary hover:bg-surface-container-high'"
      >
        <span class="material-symbols-outlined"
          :style="{ fontVariationSettings: $route.path.startsWith('/projects') ? `'FILL' 1` : `'FILL' 0` }">folder_shared</span>
        <span class="font-label-lg text-label-lg">Dự án</span>
      </RouterLink>

      <RouterLink
        to="/kanban"
        class="flex items-center gap-sm px-sm py-sm rounded-lg transition-all active:scale-95"
        :class="$route.path === '/kanban'
          ? 'text-primary font-bold border-r-4 border-primary bg-primary/10'
          : 'text-on-surface-variant hover:text-primary hover:bg-surface-container-high'"
      >
        <span class="material-symbols-outlined" :style="{ fontVariationSettings: $route.path === '/kanban' ? `'FILL' 1` : `'FILL' 0` }">view_kanban</span>
        <span class="font-label-lg text-label-lg">Bảng Kanban</span>
      </RouterLink>

      <a href="#" class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95">
        <span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0">notifications</span>
        <span class="font-label-lg text-label-lg">Thông báo</span>
      </a>

      <a href="#" class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95">
        <span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0">settings</span>
        <span class="font-label-lg text-label-lg">Cài đặt</span>
      </a>
    </div>

    <!-- CTA -->
    <button @click="$router.push('/projects')"
      class="mt-auto flex items-center justify-center gap-xs w-full py-sm bg-primary text-on-primary rounded-lg font-label-lg text-label-lg shadow-sm hover:opacity-90 transition-opacity">
      <span class="material-symbols-outlined text-[18px]">add</span>
      Tạo dự án mới
    </button>
  </nav>
</template>

<script setup>
import { useRoute } from 'vue-router'
const $route = useRoute()
</script>
