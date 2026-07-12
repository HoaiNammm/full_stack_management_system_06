<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import MyTasksSidebar from './MyTasksSidebar.vue'
import ProjectsSidebar from './ProjectsSidebar.vue'
import WorkspaceDropdown from './WorkspaceDropdown.vue'
import {
  LayoutDashboard, Briefcase, MessageSquare, Users, Bell,
  Settings, ChevronDown
} from 'lucide-vue-next'
import { useNotificationStore } from '../stores/notificationStore'

defineProps({ isSidebarOpen: Boolean })
const emit = defineEmits(['update:isSidebarOpen'])

const sidebarRef = ref(null)
const menuOpen   = ref(true)
const notificationStore = useNotificationStore()

const menuItems = [
  { name: 'Dashboard',     href: '/',              icon: LayoutDashboard },
  { name: 'My Work',       href: '/my-work',       icon: Briefcase },
  { name: 'Notifications', href: '/notifications', icon: Bell, badge: true },
  { name: 'Team',          href: '/team',           icon: Users },
  { name: 'Settings',      href: '/settings',      icon: Settings },
]

function handleClickOutside(event) {
  if (sidebarRef.value && !sidebarRef.value.contains(event.target)) {
    emit('update:isSidebarOpen', false)
  }
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside))
</script>

<template>
  <aside
    ref="sidebarRef"
    :class="[
      'z-20 flex w-60 flex-shrink-0 flex-col',
      'max-sm:absolute max-sm:inset-y-0 max-sm:transition-[left]',
      isSidebarOpen ? 'max-sm:left-0' : 'max-sm:-left-full',
    ]"
    style="
      background: var(--bg-sidebar);
      border-right: 1px solid var(--border-light);
    "
  >
    <!-- App logo -->
    <router-link to="/welcome" class="flex items-center gap-2.5 px-4 pt-4 pb-3">
      <div class="flex size-8 flex-shrink-0 items-center justify-center overflow-hidden rounded-lg shadow-sm" style="background: var(--bg-surface); border: 1px solid var(--border-light);">
        <img src="/logo.png" alt="Task logo" class="h-full w-full object-contain p-1" />
      </div>
      <span class="text-sm font-bold tracking-tight" style="color: var(--brand);">TASK<span style="opacity:0.7;">.</span></span>
    </router-link>

    <!-- Workspace selector -->
    <WorkspaceDropdown />

    <!-- Divider -->
    <div style="height: 1px; background: var(--sidebar-divider); margin: 0 16px;" />

    <!-- Scrollable nav -->
    <nav class="flex flex-1 flex-col overflow-y-auto no-scrollbar py-3" aria-label="Main navigation">

      <!-- Menu section -->
      <div class="px-3">
        <button
          @click="menuOpen = !menuOpen"
          class="flex w-full items-center justify-between px-2 py-1.5 mb-1 focus-visible:outline-none"
          style="color: var(--sidebar-section-hd);"
        >
          <span class="text-[11px] font-semibold uppercase tracking-widest">Menu</span>
          <ChevronDown :class="['size-3 transition-transform', menuOpen ? '' : '-rotate-90']" aria-hidden="true" />
        </button>

        <div v-show="menuOpen" class="space-y-0.5">
          <router-link
            v-for="item in menuItems"
            :key="item.name"
            :to="item.href"
            class="sidebar-nav-item"
          >
            <component :is="item.icon" class="size-4 shrink-0" aria-hidden="true" />
            <span class="flex-1 truncate">{{ item.name }}</span>
            <span
              v-if="item.badge && notificationStore.unreadCount > 0"
              class="flex h-5 min-w-[20px] items-center justify-center rounded-full bg-blue-500 px-1.5 text-[10px] font-semibold text-white"
            >
              {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
            </span>
          </router-link>
        </div>
      </div>

      <!-- Divider -->
      <div style="height: 1px; background: var(--sidebar-divider); margin: 12px 16px;" />

      <!-- Projects section -->
      <ProjectsSidebar />

      <!-- Divider -->
      <div style="height: 1px; background: var(--sidebar-divider); margin: 12px 16px;" />

      <!-- My Tasks section -->
      <MyTasksSidebar />
    </nav>
  </aside>
</template>
