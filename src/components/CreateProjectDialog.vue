<script setup>
import { ref } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { Layers, X } from 'lucide-vue-next'
import ProjectTemplateDialog from './ProjectTemplateDialog.vue'
import { BaseButton } from '@/components/base'

const props = defineProps({ isOpen: Boolean })
const emit  = defineEmits(['close'])

const workspaceStore     = useWorkspaceStore()
const showTemplateDialog = ref(false)

const formData = ref({
  name: '', description: '', color: '', start_date: '', end_date: '',
})

const isSubmitting = ref(false)

function applyTemplate(tplForm) {
  formData.value.name        = tplForm.name        || formData.value.name
  formData.value.description = tplForm.description || formData.value.description
  formData.value.color       = tplForm.color       || formData.value.color
}

async function handleSubmit() {
  if (!workspaceStore.currentWorkspace) return
  isSubmitting.value = true
  try {
    await workspaceStore.createProject(workspaceStore.currentWorkspaceId, {
      name:        formData.value.name,
      description: formData.value.description || null,
      color:       formData.value.color || null,
      startDate:   formData.value.start_date || null,
      endDate:     formData.value.end_date || null,
    })
    emit('close')
  } catch (err) {
    console.error('Failed to create project:', err)
  } finally {
    isSubmitting.value = false
  }
}

function close() {
  formData.value = { name: '', description: '', color: '', start_date: '', end_date: '' }
  emit('close')
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
    @click.self="close"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="create-project-heading"
      class="relative w-full max-w-lg rounded-xl border border-zinc-200 bg-white text-zinc-900 shadow-xl dark:border-zinc-800 dark:bg-zinc-950 dark:text-zinc-200"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-zinc-200 px-6 py-4 dark:border-zinc-800">
        <h2 id="create-project-heading" class="text-base font-semibold">Create New Project</h2>
        <div class="flex items-center gap-2">
          <BaseButton variant="secondary" size="xs" @click="showTemplateDialog = true">
            <Layers class="size-3.5" aria-hidden="true" /> Templates
          </BaseButton>
          <button
            @click="close"
            aria-label="Close dialog"
            class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <X class="size-4" aria-hidden="true" />
          </button>
        </div>
      </div>

      <!-- Body -->
      <div class="px-6 py-5">
        <p v-if="workspaceStore.currentWorkspace" class="mb-4 text-sm text-zinc-500 dark:text-zinc-400">
          In workspace: <span class="font-medium text-blue-600 dark:text-blue-400">{{ workspaceStore.currentWorkspace.name }}</span>
        </p>

        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Project Name *</label>
            <input v-model="formData.name" type="text" placeholder="Enter project name" required :class="inputCls" />
          </div>
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Description</label>
            <textarea v-model="formData.description" placeholder="Describe your project" :class="inputCls + ' h-20 resize-none'" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">Start Date</label>
              <input v-model="formData.start_date" type="date" :class="inputCls" />
            </div>
            <div>
              <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">End Date</label>
              <input v-model="formData.end_date" type="date" :min="formData.start_date" :class="inputCls" />
            </div>
          </div>
          <div>
            <label class="mb-1 block text-xs font-medium text-zinc-600 dark:text-zinc-400">
              Color <span class="font-normal text-zinc-400">(optional)</span>
            </label>
            <input v-model="formData.color" type="text" placeholder="#3b82f6 or blue" :class="inputCls" />
          </div>

          <div class="flex justify-end gap-2 pt-1">
            <BaseButton type="button" variant="secondary" size="sm" @click="close">Cancel</BaseButton>
            <BaseButton
              type="submit"
              variant="primary"
              size="sm"
              :loading="isSubmitting"
              :disabled="isSubmitting || !workspaceStore.currentWorkspace"
            >
              Create Project
            </BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>

  <ProjectTemplateDialog :isOpen="showTemplateDialog" @close="showTemplateDialog = false" @apply="applyTemplate" />
</template>
