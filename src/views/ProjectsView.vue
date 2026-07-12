<script setup>
import { ref, computed } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { Plus, Search, FolderOpen, Sparkles } from 'lucide-vue-next'
import ProjectCard from '../components/ProjectCard.vue'
import CreateProjectDialog from '../components/CreateProjectDialog.vue'
import AIProjectDialog from '../components/AIProjectDialog.vue'
import { BaseButton, LoadingSkeleton, EmptyState } from '@/components/base'

const workspaceStore = useWorkspaceStore()
const isDialogOpen   = ref(false)
const isAIDialogOpen = ref(false)
const searchTerm     = ref('')
const filters        = ref({ status: 'ALL', priority: 'ALL' })

const filteredProjects = computed(() => {
  let list = workspaceStore.projects
  if (searchTerm.value) {
    const q = searchTerm.value.toLowerCase()
    list = list.filter(p => p.name.toLowerCase().includes(q) || p.description?.toLowerCase().includes(q))
  }
  if (filters.value.status !== 'ALL')   list = list.filter(p => p.status === filters.value.status)
  if (filters.value.priority !== 'ALL') list = list.filter(p => p.priority === filters.value.priority)
  return list
})

const isFiltered = computed(() =>
  !!searchTerm.value || filters.value.status !== 'ALL' || filters.value.priority !== 'ALL'
)

const selectCls = [
  'cursor-pointer rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100',
].join(' ')
</script>

<template>
  <div class="mx-auto max-w-6xl space-y-6">

    <!-- Page header -->
    <div class="page-header-banner flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight text-white sm:text-2xl">Projects</h1>
        <p class="mt-0.5 text-sm text-white/80">Manage and track your projects</p>
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

    <!-- Filter bar -->
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
      <!-- Search -->
      <div class="relative min-w-0 flex-1 max-w-xs">
        <Search class="pointer-events-none absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-zinc-400" aria-hidden="true" />
        <input
          v-model="searchTerm"
          type="text"
          placeholder="Search projects…"
          aria-label="Search projects"
          class="w-full rounded-lg border border-zinc-300 bg-white py-2 pl-9 pr-4 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-100 dark:placeholder:text-zinc-500"
        />
      </div>

      <!-- Status filter -->
      <select v-model="filters.status" :class="selectCls" aria-label="Filter by status">
        <option value="ALL">All Status</option>
        <option value="Planning">Planning</option>
        <option value="Active">Active</option>
        <option value="OnHold">On Hold</option>
        <option value="Completed">Completed</option>
        <option value="Cancelled">Cancelled</option>
      </select>

      <!-- Priority filter -->
      <select v-model="filters.priority" :class="selectCls" aria-label="Filter by priority">
        <option value="ALL">All Priority</option>
        <option value="High">High</option>
        <option value="Medium">Medium</option>
        <option value="Low">Low</option>
      </select>
    </div>

    <!-- Loading -->
    <LoadingSkeleton v-if="workspaceStore.loading" type="card" :count="6" />

    <!-- Empty state -->
    <EmptyState
      v-else-if="filteredProjects.length === 0"
      :icon="FolderOpen"
      :title="isFiltered ? 'No projects found' : 'No projects yet'"
      :description="isFiltered
        ? 'Try adjusting your search or filters.'
        : 'Create your first project to start organizing your team.'"
      :action-label="isFiltered ? undefined : 'New Project'"
      size="lg"
      @action="isDialogOpen = true"
    />

    <!-- Project grid -->
    <div v-else class="grid grid-cols-1 gap-5 md:grid-cols-2 lg:grid-cols-3">
      <ProjectCard v-for="project in filteredProjects" :key="project.id" :project="project" />
    </div>

  </div>

  <CreateProjectDialog :isOpen="isDialogOpen" @close="isDialogOpen = false" />
  <AIProjectDialog :isOpen="isAIDialogOpen" @close="isAIDialogOpen = false" />
</template>
