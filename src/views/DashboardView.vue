<script setup>
import { ref, computed } from 'vue'
import { useAuthStore } from '../stores/authStore'
import { Plus, Sparkles } from 'lucide-vue-next'
import StatsGrid from '../components/StatsGrid.vue'
import ProjectOverview from '../components/ProjectOverview.vue'
import MyDay from '../components/MyDay.vue'
import SprintProgress from '../components/SprintProgress.vue'
import CreateProjectDialog from '../components/CreateProjectDialog.vue'
import AIProjectDialog from '../components/AIProjectDialog.vue'
import { BaseButton } from '@/components/base'

const authStore = useAuthStore()
const isDialogOpen   = ref(false)
const isAIDialogOpen = ref(false)

const displayName = computed(() =>
  authStore.user?.fullName ||
  authStore.user?.name ||
  authStore.user?.email?.split('@')[0] ||
  'User'
)
</script>

<template>
  <div class="mx-auto max-w-6xl space-y-8">

    <!-- Page header -->
    <div class="page-header-banner flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-xl font-semibold sm:text-2xl" style="color: var(--text-primary);">
          Welcome back, {{ displayName }} 👋
        </h1>
        <p class="mt-0.5 text-sm" style="color: var(--text-secondary);">
          Here's what's happening with your projects today
        </p>
      </div>

      <div class="flex flex-shrink-0 items-center gap-2">
        <BaseButton variant="secondary" size="sm" @click="isAIDialogOpen = true">
          <Sparkles class="size-3.5" aria-hidden="true" />
          AI Create
        </BaseButton>
        <BaseButton variant="primary" size="sm" @click="isDialogOpen = true">
          <Plus class="size-4" aria-hidden="true" />
          New Project
        </BaseButton>
      </div>
    </div>

    <StatsGrid />

    <!-- Main content: single column, project slider on top -->
    <div class="space-y-8">
      <ProjectOverview />
      <MyDay />
      <SprintProgress />
    </div>

  </div>

  <CreateProjectDialog :isOpen="isDialogOpen" @close="isDialogOpen = false" />
  <AIProjectDialog :isOpen="isAIDialogOpen" @close="isAIDialogOpen = false" />
</template>
