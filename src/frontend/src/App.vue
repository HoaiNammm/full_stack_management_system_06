<template>
  <!-- Wait for router initial navigation to avoid layout flash on reload -->
  <template v-if="ready">
    <!-- Layout: Login page (no sidebar/topbar) -->
    <RouterView v-if="isPublicRoute" />

    <!-- Layout: App with sidebar + topbar -->
    <div v-else class="flex h-screen overflow-hidden bg-background text-on-background">
      <SideBar />
      <div class="md:ml-[260px] flex flex-col flex-1 h-screen overflow-hidden">
        <TopBar />
        <main class="flex-1 overflow-y-auto">
          <RouterView />
        </main>
      </div>
    </div>
  </template>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SideBar from './components/SideBar.vue'
import TopBar  from './components/TopBar.vue'

const route  = useRoute()
const router = useRouter()
const ready  = ref(false)

router.isReady().then(() => { ready.value = true })

const isPublicRoute = computed(() => route.meta?.public === true)
</script>
