<script setup>
import { ref, computed } from 'vue'
import { useTaskStore } from '../stores/taskStore'
import { useAuthStore } from '../stores/authStore'
import { CheckSquare, ChevronDown } from 'lucide-vue-next'

const taskStore   = useTaskStore()
const authStore   = useAuthStore()
const sectionOpen = ref(false)

const myTasks = computed(() => {
  const uid = authStore.user?.id
  if (!uid) return []
  return taskStore.allTasks.filter(t => t.assigneeId === uid || t.assignee?.id === uid)
})

const STATUS_COLOR = {
  Done:       '#10b981',
  InProgress: '#f59e0b',
  Review:     '#a855f7',
  ToDo:       '#6366f1',
  Backlog:    '#94a3b8',
}

function statusColor(s) { return STATUS_COLOR[s] || '#94a3b8' }
function formatStatus(s) { return s?.replace(/([A-Z])/g, ' $1').trim() || s }
</script>

<template>
  <div class="px-3">
    <!-- Section header -->
    <button
      @click="sectionOpen = !sectionOpen"
      :aria-expanded="sectionOpen"
      class="flex w-full items-center justify-between px-2 py-1.5 mb-1 focus-visible:outline-none"
      style="color: var(--sidebar-section-hd);"
    >
      <div class="flex items-center gap-2">
        <span class="text-[11px] font-semibold uppercase tracking-widest">My Tasks</span>
        <span
          class="flex h-4 min-w-[18px] items-center justify-center rounded-full px-1 text-[10px] font-semibold"
          style="background: rgba(99,102,241,0.25); color: #818cf8;"
        >
          {{ myTasks.length }}
        </span>
      </div>
      <ChevronDown :class="['size-3 transition-transform', sectionOpen ? '' : '-rotate-90']" aria-hidden="true" />
    </button>

    <div v-show="sectionOpen" class="space-y-0.5">
      <p
        v-if="myTasks.length === 0"
        class="px-3 py-2 text-center text-xs"
        style="color: var(--sidebar-text);"
      >
        No tasks assigned
      </p>

      <router-link
        v-for="task in myTasks"
        :key="task.id"
        :to="`/taskDetails?projectId=${task.projectId}&taskId=${task.id}`"
        class="sidebar-nav-item"
      >
        <span
          class="size-2 flex-shrink-0 rounded-full"
          :style="{ background: statusColor(task.status) }"
          aria-hidden="true"
        />
        <div class="min-w-0 flex-1">
          <p class="truncate text-xs font-medium" style="color: var(--sidebar-text-hover);">{{ task.title }}</p>
          <p class="text-[10px]" style="color: var(--sidebar-section-hd);">{{ formatStatus(task.status) }}</p>
        </div>
      </router-link>
    </div>
  </div>
</template>
