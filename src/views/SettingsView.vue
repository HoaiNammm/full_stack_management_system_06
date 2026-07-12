<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { useThemeStore } from '../stores/themeStore'
import { useAuthStore } from '../stores/authStore'
import { AlertTriangle, CheckCircle, Moon, Save, Sun, Trash2 } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

const workspaceStore = useWorkspaceStore()
const themeStore     = useThemeStore()
const authStore      = useAuthStore()
const router         = useRouter()

const workspace = computed(() =>
  workspaceStore.workspaces.find(w => w.id === workspaceStore.currentWorkspaceId) || null
)

// ── Workspace form ────────────────────────────────────────────────────────────
const wsForm   = ref({ name: '', description: '' })
const wsSaving = ref(false)
const wsMsg    = ref('')
const wsErr    = ref('')

onMounted(() => {
  if (workspace.value) {
    wsForm.value.name        = workspace.value.name        || ''
    wsForm.value.description = workspace.value.description || ''
  }
})

async function saveWorkspace() {
  if (!wsForm.value.name.trim()) { wsErr.value = 'Workspace name is required'; return }
  wsSaving.value = true
  wsMsg.value    = ''
  wsErr.value    = ''
  try {
    await workspaceStore.updateWorkspace(workspaceStore.currentWorkspaceId, {
      name:        wsForm.value.name.trim(),
      description: wsForm.value.description.trim() || null,
    })
    wsMsg.value = 'Workspace updated successfully.'
  } catch (e) {
    wsErr.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to save.'
  } finally {
    wsSaving.value = false
  }
}

// ── Delete workspace ──────────────────────────────────────────────────────────
const deleteConfirm = ref('')
const deleteErr     = ref('')
const deleteLoading = ref(false)

async function handleDelete() {
  if (deleteConfirm.value !== workspace.value?.name) {
    deleteErr.value = 'Workspace name does not match.'
    return
  }
  deleteLoading.value = true
  deleteErr.value     = ''
  try {
    await workspaceStore.deleteWorkspace(workspaceStore.currentWorkspaceId)
    router.push('/')
  } catch (e) {
    deleteErr.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to delete workspace.'
  } finally {
    deleteLoading.value = false
  }
}

const inputCls = [
  'w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200',
].join(' ')

const cardCls = 'rounded-lg border border-zinc-200 bg-white p-6 space-y-4 dark:border-zinc-800 dark:bg-zinc-900'
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-8 text-zinc-900 dark:text-zinc-100">
    <div class="page-header-banner">
      <h1 class="text-xl font-semibold text-white">Settings</h1>
    </div>

    <!-- Workspace Settings -->
    <div :class="cardCls">
      <div>
        <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Workspace Settings</h2>
        <p class="mt-0.5 text-xs text-zinc-500 dark:text-zinc-400">Manage your workspace name and description.</p>
      </div>

      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">Workspace Name</label>
        <input v-model="wsForm.name" :class="inputCls" placeholder="My workspace" />
      </div>
      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">
          Description <span class="font-normal text-zinc-400">(optional)</span>
        </label>
        <textarea v-model="wsForm.description" :class="inputCls + ' h-20 resize-none'" placeholder="What is this workspace for?" />
      </div>

      <div v-if="wsMsg" class="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
        <CheckCircle class="size-4" aria-hidden="true" /> {{ wsMsg }}
      </div>
      <div v-if="wsErr" role="alert" class="text-sm text-red-500">{{ wsErr }}</div>

      <div class="flex justify-end">
        <BaseButton variant="primary" size="sm" :loading="wsSaving" @click="saveWorkspace">
          <Save class="size-3.5" aria-hidden="true" /> Save Changes
        </BaseButton>
      </div>
    </div>

    <!-- Appearance -->
    <div :class="cardCls">
      <div>
        <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Appearance</h2>
        <p class="mt-0.5 text-xs text-zinc-500 dark:text-zinc-400">Choose how the interface looks.</p>
      </div>

      <div class="flex gap-3" role="group" aria-label="Color theme">
        <button
          :aria-pressed="themeStore.theme === 'light'"
          @click="themeStore.theme !== 'light' && themeStore.toggleTheme()"
          :class="[
            'flex flex-1 flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
            themeStore.theme === 'light'
              ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
              : 'border-zinc-200 hover:border-zinc-300 dark:border-zinc-700 dark:hover:border-zinc-600',
          ]"
        >
          <Sun class="size-5 text-amber-500" aria-hidden="true" />
          <span class="text-xs font-medium text-zinc-900 dark:text-zinc-200">Light</span>
        </button>
        <button
          :aria-pressed="themeStore.theme === 'dark'"
          @click="themeStore.theme !== 'dark' && themeStore.toggleTheme()"
          :class="[
            'flex flex-1 flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
            themeStore.theme === 'dark'
              ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
              : 'border-zinc-200 hover:border-zinc-300 dark:border-zinc-700 dark:hover:border-zinc-600',
          ]"
        >
          <Moon class="size-5 text-blue-400" aria-hidden="true" />
          <span class="text-xs font-medium text-zinc-900 dark:text-zinc-200">Dark</span>
        </button>
      </div>
    </div>

    <!-- Account Info -->
    <div :class="cardCls">
      <div>
        <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Account</h2>
        <p class="mt-0.5 text-xs text-zinc-500 dark:text-zinc-400">
          Logged in as
          <span class="font-medium text-zinc-700 dark:text-zinc-300">{{ authStore.user?.email }}</span>.
          To update your profile or change password, go to
          <router-link
            to="/profile"
            class="text-blue-500 underline-offset-2 hover:underline focus-visible:rounded focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500"
          >Profile Settings</router-link>.
        </p>
      </div>
    </div>

    <!-- Danger Zone -->
    <div class="space-y-4 rounded-lg border border-red-200 bg-white p-6 dark:border-red-900/50 dark:bg-zinc-900">
      <div class="flex items-center gap-2">
        <AlertTriangle class="size-4 text-red-500" aria-hidden="true" />
        <h2 class="text-sm font-semibold text-red-600 dark:text-red-400">Danger Zone</h2>
      </div>
      <p class="text-xs text-zinc-500 dark:text-zinc-400">
        Deleting the workspace is permanent and cannot be undone. All projects and tasks inside will be lost.
      </p>

      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">
          Type <span class="font-mono font-semibold text-zinc-800 dark:text-zinc-200">{{ workspace?.name }}</span> to confirm
        </label>
        <input
          v-model="deleteConfirm"
          :class="inputCls + ' border-red-300 focus-visible:border-red-400 focus-visible:ring-red-400 dark:border-red-800'"
          placeholder="Workspace name"
        />
      </div>

      <div v-if="deleteErr" role="alert" class="text-sm text-red-500">{{ deleteErr }}</div>

      <BaseButton
        variant="danger"
        size="sm"
        :loading="deleteLoading"
        :disabled="deleteLoading || deleteConfirm !== workspace?.name"
        @click="handleDelete"
      >
        <Trash2 class="size-4" aria-hidden="true" /> Delete Workspace
      </BaseButton>
    </div>
  </div>
</template>
