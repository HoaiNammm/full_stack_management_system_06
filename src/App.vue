<script setup>
import { onMounted, watch } from 'vue'
import { useAuthStore } from './stores/authStore'
import { useWorkspaceStore } from './stores/workspaceStore'

const authStore = useAuthStore()
const workspaceStore = useWorkspaceStore()

onMounted(async () => {
  if (localStorage.getItem('access_token')) {
    await authStore.fetchMe()
  }
})

watch(() => authStore.user, async (user) => {
  if (user && workspaceStore.workspaces.length === 0) await workspaceStore.fetchWorkspaces()
})
</script>

<template>
  <router-view />
</template>
