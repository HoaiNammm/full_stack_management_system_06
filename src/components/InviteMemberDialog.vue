<script setup>
import { ref } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { workspaceApi } from '../api/workspaces'
import { Mail, UserPlus, X } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

const props = defineProps({ isOpen: Boolean })
const emit  = defineEmits(['close'])

const workspaceStore = useWorkspaceStore()
const isSubmitting   = ref(false)
const error          = ref('')
const success        = ref(false)
const formData       = ref({ email: '', role: 'Member' })

async function handleSubmit() {
  if (!workspaceStore.currentWorkspaceId) return
  isSubmitting.value = true
  error.value = ''
  try {
    await workspaceApi.addMember(workspaceStore.currentWorkspaceId, {
      email: formData.value.email,
      role:  formData.value.role,
    })
    success.value = true
    formData.value = { email: '', role: 2 }
    setTimeout(() => { success.value = false; emit('close') }, 1500)
  } catch (e) {
    error.value = e?.response?.data?.message || e?.response?.data?.error || 'Failed to add member. Check if the email is registered.'
  } finally {
    isSubmitting.value = false
  }
}

const inputCls = [
  'w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200',
].join(' ')
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/25 p-4 backdrop-blur-sm dark:bg-black/60"
    @click.self="emit('close')"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="invite-member-heading"
      class="w-full max-w-md rounded-xl border border-zinc-200 bg-white shadow-xl dark:border-zinc-800 dark:bg-zinc-950"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-zinc-200 px-6 py-4 dark:border-zinc-800">
        <h2 id="invite-member-heading" class="flex items-center gap-2 text-base font-semibold text-zinc-900 dark:text-zinc-100">
          <UserPlus class="size-4 text-blue-500" aria-hidden="true" /> Invite Team Member
        </h2>
        <button
          @click="emit('close')"
          aria-label="Close dialog"
          class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
        >
          <X class="size-4" aria-hidden="true" />
        </button>
      </div>

      <!-- Body -->
      <div class="px-6 py-5">
        <p v-if="workspaceStore.currentWorkspace" class="mb-4 text-sm text-zinc-500 dark:text-zinc-400">
          Inviting to workspace:
          <span class="font-medium text-blue-600 dark:text-blue-400">{{ workspaceStore.currentWorkspace.name }}</span>
        </p>

        <!-- Success -->
        <div v-if="success" class="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400">
          Member added successfully!
        </div>

        <!-- Error -->
        <div v-if="error" role="alert" class="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-800 dark:bg-red-950/30 dark:text-red-400">
          {{ error }}
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Email Address</label>
            <div class="relative">
              <Mail class="pointer-events-none absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-zinc-400" aria-hidden="true" />
              <input
                v-model="formData.email"
                type="email"
                required
                placeholder="Enter email address"
                autocomplete="email"
                :class="inputCls + ' pl-9'"
              />
            </div>
          </div>

          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Role</label>
            <select v-model="formData.role" :class="inputCls + ' cursor-pointer'">
              <option value="Member">Member — có thể tạo task, comment, log time</option>
              <option value="Owner">Owner — toàn quyền quản lý workspace</option>
              <option value="Viewer">Viewer — chỉ xem, không chỉnh sửa</option>
            </select>
          </div>

          <div class="flex justify-end gap-2 pt-1">
            <BaseButton type="button" variant="secondary" size="sm" @click="emit('close')">Cancel</BaseButton>
            <BaseButton
              type="submit"
              variant="primary"
              size="sm"
              :loading="isSubmitting"
              :disabled="isSubmitting || !workspaceStore.currentWorkspace"
            >
              Add Member
            </BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
