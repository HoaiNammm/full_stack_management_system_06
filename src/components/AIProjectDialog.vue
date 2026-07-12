<script setup>
import { ref } from 'vue'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { aiGenerateApi } from '../api/projects'
import { Sparkles, RefreshCw, Check, X, Loader2, MessageCircle } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'created'])

const workspaceStore    = useWorkspaceStore()
const step              = ref('prompt')   // prompt | preview | confirming | done
const prompt            = ref('')
const refinement        = ref('')
const preview           = ref(null)
const error             = ref('')
const loading           = ref(false)
const showRefineInput   = ref(false)

async function handleGenerate() {
  if (!prompt.value.trim()) return
  loading.value = true
  error.value   = ''
  try {
    preview.value = await aiGenerateApi.preview(workspaceStore.currentWorkspaceId, prompt.value)
    step.value    = 'preview'
  } catch (e) {
    error.value = e?.response?.data?.error || e?.response?.data?.message || 'Cannot generate project plan. Check if AI service is configured.'
  } finally {
    loading.value = false
  }
}

async function handleRefine() {
  if (!refinement.value.trim() || !preview.value) return
  loading.value = true
  error.value   = ''
  try {
    preview.value         = await aiGenerateApi.refine(workspaceStore.currentWorkspaceId, preview.value, refinement.value)
    refinement.value      = ''
    showRefineInput.value = false
  } catch (e) {
    error.value = e?.response?.data?.error || 'Could not refine plan.'
  } finally {
    loading.value = false
  }
}

async function handleConfirm() {
  if (!preview.value) return
  loading.value = true
  error.value   = ''
  step.value    = 'confirming'
  try {
    const result = await aiGenerateApi.confirm(workspaceStore.currentWorkspaceId, preview.value)
    step.value   = 'done'
    await workspaceStore.fetchProjects(workspaceStore.currentWorkspaceId)
    emit('created', result.projectId)
  } catch (e) {
    error.value = e?.response?.data?.error || 'Could not create project.'
    step.value  = 'preview'
  } finally {
    loading.value = false
  }
}

function handleReset() {
  step.value            = 'prompt'
  preview.value         = null
  prompt.value          = ''
  refinement.value      = ''
  error.value           = ''
  showRefineInput.value = false
}

function handleClose() {
  handleReset()
  emit('close')
}

const priorityLabel = (p) => ['Low', 'Medium', 'High', 'Urgent'][p] || 'Medium'

const textareaCls = [
  'w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'placeholder-zinc-400 dark:placeholder-zinc-500 resize-none',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-200',
].join(' ')
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/30 p-4 backdrop-blur-sm dark:bg-black/60"
    @click.self="handleClose"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="ai-project-heading"
      class="flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-xl dark:border-zinc-800 dark:bg-zinc-950"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
        <h2 id="ai-project-heading" class="flex items-center gap-2 text-base font-semibold text-zinc-900 dark:text-zinc-100">
          <Sparkles class="size-4.5 text-blue-500" aria-hidden="true" /> Create Project with AI
        </h2>
        <button
          @click="handleClose"
          aria-label="Close dialog"
          class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
        >
          <X class="size-4" aria-hidden="true" />
        </button>
      </div>

      <!-- Body (scrollable) -->
      <div class="flex-1 space-y-4 overflow-y-auto p-5">
        <!-- Error -->
        <div
          v-if="error"
          role="alert"
          class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-800 dark:bg-red-950/30 dark:text-red-400"
        >
          {{ error }}
        </div>

        <!-- Step: prompt -->
        <div v-if="step === 'prompt'" class="space-y-4">
          <p class="text-sm text-zinc-600 dark:text-zinc-400">
            Describe your project in natural language. The AI will generate a full project plan with sprints, milestones, and tasks.
          </p>
          <textarea
            v-model="prompt"
            placeholder="E.g.: Build an e-commerce website for selling clothes. 3 months, 2 developers. Need inventory management, payment integration, and admin dashboard."
            rows="5"
            :class="textareaCls"
          />
        </div>

        <!-- Step: preview -->
        <div v-else-if="step === 'preview' && preview" class="space-y-4">
          <!-- Project info -->
          <div class="rounded-lg border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-950/30">
            <h3 class="text-base font-semibold text-zinc-900 dark:text-zinc-100">{{ preview.name }}</h3>
            <p class="mt-1 text-sm text-zinc-600 dark:text-zinc-400">{{ preview.description }}</p>
            <div class="mt-2 flex gap-4 text-xs text-zinc-500 dark:text-zinc-400">
              <span>Start: {{ preview.startDate }}</span>
              <span v-if="preview.endDate">End: {{ preview.endDate }}</span>
            </div>
          </div>

          <!-- Milestones -->
          <div v-if="preview.milestones?.length > 0">
            <h4 class="mb-2 text-sm font-medium text-zinc-700 dark:text-zinc-300">Milestones ({{ preview.milestones.length }})</h4>
            <div class="space-y-1">
              <div
                v-for="(m, i) in preview.milestones"
                :key="i"
                class="flex items-center gap-2 rounded-lg bg-purple-50 p-2 text-sm dark:bg-purple-950/20"
              >
                <span class="flex-1 font-medium text-zinc-800 dark:text-zinc-200">{{ m.name }}</span>
                <span v-if="m.dueDate" class="text-xs text-zinc-400">due {{ m.dueDate }}</span>
              </div>
            </div>
          </div>

          <!-- Sprints -->
          <div v-if="preview.sprints?.length > 0">
            <h4 class="mb-2 text-sm font-medium text-zinc-700 dark:text-zinc-300">Sprints ({{ preview.sprints.length }})</h4>
            <div class="space-y-1">
              <div
                v-for="(s, i) in preview.sprints"
                :key="i"
                class="rounded-lg bg-zinc-50 p-2 text-sm dark:bg-zinc-900"
              >
                <div class="flex items-center gap-2">
                  <span class="w-16 flex-shrink-0 text-xs text-zinc-500 dark:text-zinc-400">Sprint {{ i + 1 }}</span>
                  <span class="flex-1 font-medium text-zinc-800 dark:text-zinc-200">{{ s.name }}</span>
                  <span class="text-xs text-zinc-400">{{ s.startDate }} → {{ s.endDate }}</span>
                </div>
                <p v-if="s.goal" class="mt-1 pl-[calc(4rem+0.5rem)] text-xs text-zinc-500 dark:text-zinc-400">{{ s.goal }}</p>
              </div>
            </div>
          </div>

          <!-- Tasks -->
          <div v-if="preview.tasks?.length > 0">
            <h4 class="mb-2 text-sm font-medium text-zinc-700 dark:text-zinc-300">Tasks ({{ preview.tasks.length }})</h4>
            <div class="max-h-48 space-y-1 overflow-y-auto">
              <div
                v-for="(t, i) in preview.tasks"
                :key="i"
                class="flex items-start gap-2 rounded-lg bg-zinc-50 p-2 text-sm dark:bg-zinc-900"
              >
                <span :class="['mt-0.5 flex-shrink-0 rounded px-1.5 py-0.5 text-xs',
                  t.priority >= 2 ? 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300'
                    : t.priority === 1 ? 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300'
                    : 'bg-zinc-200 text-zinc-600 dark:bg-zinc-700 dark:text-zinc-400']">
                  {{ priorityLabel(t.priority) }}
                </span>
                <div class="min-w-0 flex-1">
                  <p class="text-zinc-800 dark:text-zinc-200">{{ t.title }}</p>
                  <p class="mt-0.5 flex flex-wrap gap-x-2 text-xs text-zinc-400">
                    <span v-if="t.subtasks?.length > 0">{{ t.subtasks.length }} subtask{{ t.subtasks.length > 1 ? 's' : '' }}</span>
                    <span v-if="t.estimatedHours">{{ t.estimatedHours }}h</span>
                    <span v-if="t.assigneeName">→ {{ t.assigneeName }}</span>
                  </p>
                </div>
              </div>
            </div>
          </div>

          <!-- Refinement -->
          <div class="border-t border-zinc-200 pt-3 dark:border-zinc-800">
            <button
              @click="showRefineInput = !showRefineInput"
              class="flex items-center gap-1.5 text-sm text-blue-600 transition hover:text-blue-700 focus-visible:outline-none focus-visible:underline dark:text-blue-400 dark:hover:text-blue-300"
            >
              <MessageCircle class="size-3.5" aria-hidden="true" /> Request changes to this plan
            </button>
            <div v-if="showRefineInput" class="mt-2 space-y-2">
              <textarea
                v-model="refinement"
                placeholder="E.g.: Add more tasks for testing phase, reduce sprint count to 2..."
                rows="2"
                :class="textareaCls"
              />
              <div class="flex gap-2">
                <BaseButton
                  variant="primary"
                  size="sm"
                  :loading="loading"
                  :disabled="loading || !refinement.trim()"
                  @click="handleRefine"
                >
                  <RefreshCw v-if="!loading" class="size-3.5" aria-hidden="true" /> Regenerate
                </BaseButton>
                <button
                  @click="showRefineInput = false"
                  class="px-3 py-1.5 text-sm text-zinc-500 transition hover:text-zinc-800 focus-visible:outline-none focus-visible:underline dark:text-zinc-400 dark:hover:text-zinc-200"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Step: done -->
        <div v-else-if="step === 'done'" class="py-8 text-center">
          <div class="mx-auto mb-4 flex size-16 items-center justify-center rounded-full bg-emerald-100 dark:bg-emerald-900/30">
            <Check class="size-8 text-emerald-600 dark:text-emerald-400" aria-hidden="true" />
          </div>
          <h3 class="mb-1 text-lg font-semibold text-zinc-900 dark:text-zinc-100">Project Created!</h3>
          <p class="text-sm text-zinc-600 dark:text-zinc-400">Your project has been created with all sprints and milestones.</p>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex items-center justify-between gap-3 border-t border-zinc-200 px-5 py-4 dark:border-zinc-800">
        <BaseButton
          v-if="step === 'preview'"
          variant="ghost"
          size="sm"
          @click="handleReset"
        >
          <RefreshCw class="size-3.5" aria-hidden="true" /> Start over
        </BaseButton>
        <div v-else />

        <div class="flex gap-2">
          <BaseButton variant="secondary" size="sm" @click="handleClose">
            {{ step === 'done' ? 'Close' : 'Cancel' }}
          </BaseButton>
          <BaseButton
            v-if="step === 'prompt'"
            variant="primary"
            size="sm"
            :loading="loading"
            :disabled="loading || !prompt.trim()"
            @click="handleGenerate"
          >
            <Sparkles v-if="!loading" class="size-3.5" aria-hidden="true" /> Generate Plan
          </BaseButton>
          <BaseButton
            v-if="step === 'preview'"
            variant="primary"
            size="sm"
            :loading="loading"
            :disabled="loading"
            @click="handleConfirm"
          >
            <Check v-if="!loading" class="size-3.5" aria-hidden="true" /> Create Project
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>
