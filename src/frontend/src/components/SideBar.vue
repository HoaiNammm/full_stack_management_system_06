<template>
  <nav class="hidden md:flex w-[280px] h-screen fixed left-0 top-0 bg-surface-container-lowest text-on-surface border-r border-outline-variant shadow-xl flex-col py-lg px-md z-40">
    <RouterLink to="/dashboard" class="flex items-center gap-sm mb-xl px-xs group">
      <div class="w-11 h-11 rounded-xl bg-primary text-on-primary flex items-center justify-center font-black shadow-sm group-hover:scale-105 transition-transform">
        PM
      </div>
      <div class="min-w-0">
        <h1 class="font-headline-sm text-headline-sm font-bold text-on-surface">Project Manager</h1>
        <p class="font-label-sm text-label-sm text-on-surface-variant truncate">Team workspace</p>
      </div>
    </RouterLink>

    <div class="flex flex-col gap-1 flex-grow">
      <p class="px-sm pb-xs font-label-sm text-label-sm text-outline uppercase tracking-wider">Main menu</p>
      <RouterLink
        v-for="item in mainItems"
        :key="item.to"
        :to="item.to"
        class="relative flex items-center gap-sm px-sm py-sm rounded-lg transition-all active:scale-[0.98]"
        :class="isActive(item)
          ? 'text-primary font-bold bg-primary/10 shadow-sm'
          : 'text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high'"
      >
        <span v-if="isActive(item)" class="absolute left-0 top-2 bottom-2 w-1 rounded-full bg-primary"></span>
        <span class="material-symbols-outlined text-[21px]"
          :style="{ fontVariationSettings: isActive(item) ? `'FILL' 1` : `'FILL' 0` }">{{ item.icon }}</span>
        <span class="font-label-lg text-label-lg">{{ item.label }}</span>
      </RouterLink>

      <p class="px-sm pt-lg pb-xs font-label-sm text-label-sm text-outline uppercase tracking-wider">Workspace</p>
      <RouterLink
        v-for="item in workspaceItems"
        :key="item.to"
        :to="item.to"
        class="relative flex items-center gap-sm px-sm py-sm rounded-lg transition-all active:scale-[0.98]"
        :class="isActive(item)
          ? 'text-primary font-bold bg-primary/10 shadow-sm'
          : 'text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high'"
      >
        <span v-if="isActive(item)" class="absolute left-0 top-2 bottom-2 w-1 rounded-full bg-primary"></span>
        <span class="material-symbols-outlined text-[21px]"
          :style="{ fontVariationSettings: isActive(item) ? `'FILL' 1` : `'FILL' 0` }">{{ item.icon }}</span>
        <span class="font-label-lg text-label-lg">{{ item.label }}</span>
      </RouterLink>
    </div>

    <div class="rounded-2xl border border-outline-variant bg-surface-container-low p-sm mb-sm">
      <p class="font-label-sm text-label-sm text-on-surface-variant">Demo workspace</p>
      <p class="font-label-lg text-label-lg text-on-surface">FE + 3 services</p>
    </div>

    <button @click="$router.push('/projects')"
      class="app-button-primary w-full">
      <span class="material-symbols-outlined text-[18px]">add</span>
      Tạo dự án
    </button>
  </nav>
</template>

<script setup>
import { useRoute } from 'vue-router'

const route = useRoute()

const mainItems = [
  { to: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
  { to: '/projects', label: 'Projects', icon: 'folder_shared', match: path => path.startsWith('/projects') },
  { to: '/tasks', label: 'Tasks', icon: 'task_alt' },
  { to: '/kanban', label: 'Kanban', icon: 'view_kanban' },
]

const workspaceItems = [
  { to: '/calendar', label: 'Timeline', icon: 'calendar_month' },
  { to: '/members', label: 'Members', icon: 'groups' },
  { to: '/notifications', label: 'Notifications', icon: 'notifications' },
  { to: '/settings', label: 'Profile', icon: 'account_circle' },
  { to: '/system-status', label: 'System Status', icon: 'monitor_heart' },
]

function isActive(item) {
  if (item.match) return item.match(route.path)
  return route.path === item.to
}
</script>
