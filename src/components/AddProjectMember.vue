<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { projectApi } from '../api/projects'
import { workspaceApi } from '../api/workspaces'
import { Mail, UserPlus } from 'lucide-vue-next'

const props = defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'added'])

const route = useRoute()
const workspaceStore = useWorkspaceStore()

const project = computed(() => workspaceStore.projects.find(p => p.id === route.query.id))
const projectMemberIds = computed(() => project.value?.members?.map(m => m.userId) || [])

const wsMembers  = ref([])
const loadingWs  = ref(false)

watch(() => props.isOpen, async (open) => {
  if (!open || !workspaceStore.currentWorkspaceId) return
  loadingWs.value = true
  try {
    wsMembers.value = await workspaceApi.getMembers(workspaceStore.currentWorkspaceId)
  } catch { wsMembers.value = [] }
  finally { loadingWs.value = false }
}, { immediate: true })

const nonProjectMembers = computed(() =>
  wsMembers.value.filter(m => !projectMemberIds.value.includes(m.userId || m.user?.id))
)

const selectedUserId = ref('')
const role = ref('Member')
const isAdding = ref(false)
const error = ref('')
const success = ref(false)

async function handleSubmit() {
  if (!selectedUserId.value || !project.value) return
  isAdding.value = true
  error.value = ''
  try {
    await projectApi.addMember(workspaceStore.currentWorkspaceId, project.value.id, {
      userId: selectedUserId.value,
      role:   role.value,
    })
    success.value = true
    selectedUserId.value = ''
    role.value = 'Member'
    setTimeout(() => {
      success.value = false
      emit('added')
      emit('close')
    }, 1200)
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to add member.'
  } finally {
    isAdding.value = false
  }
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/20 dark:bg-black/50 backdrop-blur flex items-center justify-center z-50">
    <div class="bg-white dark:bg-zinc-950 border border-zinc-300 dark:border-zinc-800 rounded-xl p-6 w-full max-w-md text-zinc-900 dark:text-zinc-200">
      <div class="mb-4">
        <h2 class="text-xl font-bold flex items-center gap-2">
          <UserPlus class="size-5" /> Add Member to Project
        </h2>
        <p v-if="project" class="text-sm text-zinc-600 dark:text-zinc-400">
          Adding to: <span class="text-blue-600 dark:text-blue-400">{{ project.name }}</span>
        </p>
      </div>

      <div v-if="success" class="mb-4 p-3 rounded bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 text-sm">
        Member added successfully!
      </div>
      <p v-if="error" class="mb-3 text-sm text-red-500">{{ error }}</p>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-2">
          <label class="text-sm font-medium">Select Member</label>
          <div class="relative">
            <Mail class="absolute left-3 top-1/2 -translate-y-1/2 text-zinc-500 dark:text-zinc-400 w-4 h-4" />
            <select v-model="selectedUserId" required :disabled="loadingWs"
              class="pl-10 mt-1 w-full rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-900 text-zinc-900 dark:text-zinc-200 text-sm py-2 focus:outline-none focus:border-blue-500 disabled:opacity-60">
              <option value="">{{ loadingWs ? 'Loading members…' : '— Select workspace member —' }}</option>
              <option v-for="m in nonProjectMembers" :key="m.user?.id || m.userId" :value="m.user?.id || m.userId">
                {{ m.user?.name || m.user?.email || m.userId }}
              </option>
            </select>
          </div>
          <p v-if="!loadingWs && nonProjectMembers.length === 0 && wsMembers.length > 0" class="text-xs text-zinc-500 dark:text-zinc-400">
            All workspace members are already in this project.
          </p>
        </div>

        <div class="space-y-2">
          <label class="text-sm font-medium">Role</label>
          <select v-model="role" class="w-full rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-900 text-zinc-900 dark:text-zinc-200 py-2 px-3 mt-1 focus:outline-none focus:border-blue-500 text-sm">
            <option value="Member">Member</option>
            <option value="Manager">Manager</option>
            <option value="Viewer">Viewer</option>
          </select>
        </div>

        <div class="flex justify-end gap-3 pt-2">
          <button type="button" @click="emit('close')" class="px-5 py-2 text-sm rounded border border-zinc-300 dark:border-zinc-700 text-zinc-900 dark:text-zinc-200 hover:bg-zinc-200 dark:hover:bg-zinc-800 transition">
            Cancel
          </button>
          <button type="submit" :disabled="isAdding || !selectedUserId || !project" class="px-5 py-2 text-sm rounded bg-gradient-to-br from-blue-500 to-blue-600 hover:opacity-90 text-white disabled:opacity-50 transition">
            {{ isAdding ? 'Adding...' : 'Add Member' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
