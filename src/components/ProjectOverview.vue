<script setup>
import { ref, computed } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { ArrowRight, ChevronLeft, ChevronRight, FolderOpen } from 'lucide-vue-next'
import { EmptyState } from '@/components/base'
import ProjectCard from './ProjectCard.vue'
import CreateProjectDialog from './CreateProjectDialog.vue'

const workspaceStore = useWorkspaceStore()
const isDialogOpen   = ref(false)
const sliderRef      = ref(null)

const projects         = computed(() => workspaceStore.projects)
const currentWorkspace = computed(() => workspaceStore.currentWorkspace)

function scrollSlider(direction) {
  sliderRef.value?.scrollBy({ left: direction * 300, behavior: 'smooth' })
}
</script>

<template>
  <div v-if="currentWorkspace">
    <!-- Header -->
    <div class="mb-3 flex items-center justify-between">
      <h2 class="text-sm font-medium text-zinc-800 dark:text-zinc-200">Project Overview</h2>
      <div class="flex items-center gap-3">
        <router-link
          to="/projects"
          class="flex items-center gap-1 text-xs text-zinc-500 transition-colors hover:text-zinc-800 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:text-zinc-400 dark:hover:text-zinc-200"
        >
          View all <ArrowRight class="size-3" aria-hidden="true" />
        </router-link>
        <div v-if="projects.length > 0" class="flex items-center gap-1.5">
          <button
            type="button"
            @click="scrollSlider(-1)"
            aria-label="Scroll projects left"
            class="flex size-7 items-center justify-center rounded-lg border border-zinc-200 text-zinc-500 transition-colors hover:bg-zinc-100 hover:text-zinc-800 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-800 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <ChevronLeft class="size-4" aria-hidden="true" />
          </button>
          <button
            type="button"
            @click="scrollSlider(1)"
            aria-label="Scroll projects right"
            class="flex size-7 items-center justify-center rounded-lg border border-zinc-200 text-zinc-500 transition-colors hover:bg-zinc-100 hover:text-zinc-800 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-800 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <ChevronRight class="size-4" aria-hidden="true" />
          </button>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div
      v-if="projects.length === 0"
      class="rounded-lg border border-zinc-200 bg-white px-4 dark:border-zinc-800 dark:bg-zinc-900"
    >
      <EmptyState
        :icon="FolderOpen"
        title="No projects yet"
        description="Create your first project to start organizing your team's work."
        action-label="New Project"
        @action="isDialogOpen = true"
      />
      <CreateProjectDialog :isOpen="isDialogOpen" @close="isDialogOpen = false" />
    </div>

    <!-- Project slider -->
    <div
      v-else
      ref="sliderRef"
      class="no-scrollbar -mx-1 flex snap-x snap-mandatory gap-4 overflow-x-auto px-1 pb-2"
    >
      <div
        v-for="project in projects"
        :key="project.id"
        class="w-72 flex-shrink-0 snap-start"
      >
        <ProjectCard :project="project" />
      </div>
    </div>
  </div>
</template>
