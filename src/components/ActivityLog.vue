<script setup>
import { ref, onMounted } from 'vue'
import { format } from 'date-fns'
import { parseUtc } from '../utils/date'
import { activityLogApi } from '../api/tasks'
import { Activity, User } from 'lucide-vue-next'

const props = defineProps({ projectId: String })

const logs    = ref([])
const loading = ref(false)
const error   = ref('')

onMounted(load)

async function load() {
  if (!props.projectId) return
  loading.value = true
  error.value   = ''
  try {
    logs.value = await activityLogApi.getProject(props.projectId) || []
  } catch (e) {
    error.value = e?.response?.data?.error || 'Failed to load activity log'
  } finally {
    loading.value = false
  }
}

// Map common action types to readable labels
function actionLabel(action) {
  const map = {
    TaskCreated:     'created task',
    TaskUpdated:     'updated task',
    TaskDeleted:     'deleted task',
    TaskStatusChanged: 'changed status',
    CommentAdded:    'added a comment',
    MemberAdded:     'added a member',
    MemberRemoved:   'removed a member',
    SprintStarted:   'started sprint',
    SprintCompleted: 'completed sprint',
    ProjectUpdated:  'updated project',
  }
  return map[action] || action?.replace(/([A-Z])/g, ' $1').trim().toLowerCase()
}
</script>

<template>
  <div class="space-y-4">
    <h2 class="text-base font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
      <Activity class="size-4" /> Activity Log
    </h2>

    <p v-if="error" class="text-sm text-red-500">{{ error }}</p>

    <div v-if="loading" class="text-sm text-zinc-500 dark:text-zinc-400 py-4">Loading activity...</div>

    <div v-else-if="logs.length === 0" class="text-sm text-zinc-500 dark:text-zinc-400 py-8 text-center">
      No activity recorded yet.
    </div>

    <div v-else class="relative">
      <!-- Timeline line -->
      <div class="absolute left-5 top-0 bottom-0 w-px bg-zinc-200 dark:bg-zinc-700" />

      <div class="space-y-4">
        <div v-for="log in logs" :key="log.id" class="flex gap-4 relative">
          <!-- Avatar dot -->
          <div class="relative z-10 flex-shrink-0 size-10 rounded-full bg-zinc-100 dark:bg-zinc-800 border border-zinc-300 dark:border-zinc-700 flex items-center justify-center">
            <img v-if="log.actor?.avatarUrl" :src="log.actor.avatarUrl" :alt="log.actor.name" class="size-10 rounded-full object-cover" />
            <User v-else class="size-4 text-zinc-500 dark:text-zinc-400" />
          </div>

          <div class="flex-1 min-w-0 pb-4">
            <div class="flex flex-wrap items-baseline gap-1 text-sm">
              <span class="font-medium text-zinc-900 dark:text-zinc-100">
                {{ log.actor?.name || log.actor?.email || 'System' }}
              </span>
              <span class="text-zinc-500 dark:text-zinc-400">{{ actionLabel(log.action || log.type) }}</span>
              <span v-if="log.entityName" class="font-medium text-zinc-900 dark:text-zinc-100 truncate max-w-xs">
                "{{ log.entityName }}"
              </span>
            </div>

            <p v-if="log.description" class="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">{{ log.description }}</p>

            <span class="text-xs text-zinc-400 dark:text-zinc-500 mt-1 block">
              {{ log.createdAt ? format(parseUtc(log.createdAt), 'dd MMM yyyy, HH:mm') : '' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
