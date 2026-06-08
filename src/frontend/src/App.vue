<template>
  <div class="flex h-screen overflow-hidden bg-background text-on-background">
    <SideBar />
    <div class="md:ml-[260px] flex flex-col flex-1 h-screen overflow-hidden">
      <TopBar />
      <main class="flex-1 overflow-y-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup>
import SideBar from './components/SideBar.vue'
import TopBar from './components/TopBar.vue'
</script>
