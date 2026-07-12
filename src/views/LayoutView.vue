<script setup>
import { ref, onMounted } from 'vue'
import { useThemeStore } from '../stores/themeStore'
import Navbar from '../components/Navbar.vue'
import Sidebar from '../components/Sidebar.vue'

const themeStore = useThemeStore()
const isSidebarOpen = ref(false)

onMounted(() => {
  themeStore.loadTheme()
})
</script>

<template>
  <div class="relative flex h-screen overflow-hidden" style="background: var(--bg-main); color: var(--text-primary);">
    <Sidebar :isSidebarOpen="isSidebarOpen" @update:isSidebarOpen="isSidebarOpen = $event" />
    <div class="flex min-h-0 flex-1 flex-col overflow-hidden">
      <Navbar :isSidebarOpen="isSidebarOpen" @update:isSidebarOpen="isSidebarOpen = $event" />
      <main class="min-h-0 flex-1 overflow-y-auto p-6">
        <router-view />
      </main>
    </div>
  </div>
</template>
