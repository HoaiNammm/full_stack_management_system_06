<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { ChevronRight, ChevronDown, Kanban, BarChart2, Calendar, Settings, ArrowRight } from 'lucide-vue-next'

const workspaceStore = useWorkspaceStore()
const route = useRoute()

const projects = computed(() => workspaceStore.projects)
const expandedProjects = ref(new Set())
const sectionOpen = ref(true)

function getProjectSubItems(projectId) {
  return [
    { title: 'Tasks',     icon: Kanban,   url: `/projectsDetail?id=${projectId}&tab=tasks` },
    { title: 'Analytics', icon: BarChart2, url: `/projectsDetail?id=${projectId}&tab=analytics` },
    { title: 'Calendar',  icon: Calendar, url: `/projectsDetail?id=${projectId}&tab=calendar` },
    { title: 'Settings',  icon: Settings, url: `/projectsDetail?id=${projectId}&tab=settings` },
  ]
}

function toggleProject(id) {
  const s = new Set(expandedProjects.value)
  s.has(id) ? s.delete(id) : s.add(id)
  expandedProjects.value = s
}

function isSubItemActive(projectId, title) {
  return route.path === '/projectsDetail' &&
    route.query.id === projectId &&
    route.query.tab === title.toLowerCase()
}
</script>

<template>
  <div class="px-3">
    <!-- Section header -->
    <button
      @click="sectionOpen = !sectionOpen"
      class="flex w-full items-center justify-between px-2 py-1.5 mb-1 focus-visible:outline-none"
      style="color: var(--sidebar-section-hd);"
    >
      <span class="text-[11px] font-semibold uppercase tracking-widest">Projects</span>
      <div class="flex items-center gap-2">
        <router-link
          to="/projects"
          @click.stop
          class="flex size-5 items-center justify-center rounded transition-colors focus-visible:outline-none"
          style="color: var(--sidebar-section-hd);"
          aria-label="View all projects"
        >
          <ArrowRight class="size-3" aria-hidden="true" />
        </router-link>
        <ChevronDown :class="['size-3 transition-transform', sectionOpen ? '' : '-rotate-90']" aria-hidden="true" />
      </div>
    </button>

    <div v-show="sectionOpen" class="space-y-0.5">
      <div v-for="project in projects" :key="project.id">
        <!-- Project row -->
        <button
          @click="toggleProject(project.id)"
          class="sidebar-nav-item"
          :aria-expanded="expandedProjects.has(project.id)"
        >
          <span
            class="size-2.5 flex-shrink-0 rounded-sm"
            :style="{ background: project.color || '#3b82f6' }"
            aria-hidden="true"
          />
          <span class="flex-1 truncate text-sm">{{ project.name }}</span>
          <ChevronRight
            :class="['size-3 flex-shrink-0 opacity-50 transition-transform duration-150', expandedProjects.has(project.id) ? 'rotate-90' : '']"
            aria-hidden="true"
          />
        </button>

        <!-- Sub-items -->
        <div v-if="expandedProjects.has(project.id)" class="ml-5 mt-0.5 space-y-0.5">
          <router-link
            v-for="subItem in getProjectSubItems(project.id)"
            :key="subItem.title"
            :to="subItem.url"
            class="sidebar-nav-item text-xs py-1.5"
            :class="{ active: isSubItemActive(project.id, subItem.title) }"
          >
            <component :is="subItem.icon" class="size-3 flex-shrink-0" aria-hidden="true" />
            {{ subItem.title }}
          </router-link>
        </div>
      </div>

      <p
        v-if="projects.length === 0"
        class="px-3 py-2 text-xs"
        style="color: var(--sidebar-text);"
      >
        No projects yet
      </p>
    </div>
  </div>
</template>
