<script setup>
import { ref } from 'vue'
import { X, Layers, GitBranch, Code2, Megaphone, Rocket, Check } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

defineProps({ isOpen: Boolean })
const emit = defineEmits(['close', 'apply'])

const TEMPLATES = [
  {
    id: 'scrum',
    icon: GitBranch,
    name: 'Scrum / Agile',
    description: 'Sprint-based development with backlog, reviews and retrospectives.',
    color: '#3b82f6',
    tags: ['Software', 'Dev', 'Sprints'],
    form: { name: 'New Scrum Project', description: 'Agile sprint-based project following Scrum ceremonies.', color: '#3b82f6' },
  },
  {
    id: 'kanban',
    icon: Layers,
    name: 'Kanban Flow',
    description: 'Continuous delivery board — pull tasks as capacity allows.',
    color: '#8b5cf6',
    tags: ['Operations', 'Support', 'Flow'],
    form: { name: 'New Kanban Project', description: 'Continuous flow project with WIP limits and visual board.', color: '#8b5cf6' },
  },
  {
    id: 'software',
    icon: Code2,
    name: 'Software Development',
    description: 'Full product lifecycle: design → build → test → release.',
    color: '#06b6d4',
    tags: ['Software', 'Product', 'Engineering'],
    form: { name: 'New Software Project', description: 'End-to-end software development project with design, build, test and release phases.', color: '#06b6d4' },
  },
  {
    id: 'marketing',
    icon: Megaphone,
    name: 'Marketing Campaign',
    description: 'Plan, create, publish and analyse marketing initiatives.',
    color: '#f59e0b',
    tags: ['Marketing', 'Content', 'Brand'],
    form: { name: 'New Campaign', description: 'Marketing campaign — strategy, creative production and performance tracking.', color: '#f59e0b' },
  },
  {
    id: 'launch',
    icon: Rocket,
    name: 'Product Launch',
    description: 'Go-to-market checklist from MVP to public release.',
    color: '#10b981',
    tags: ['Product', 'Launch', 'GTM'],
    form: { name: 'Product Launch', description: 'Coordinated product launch plan covering development, marketing and sales readiness.', color: '#10b981' },
  },
]

const selected = ref(null)

function choose(tpl) {
  selected.value = tpl
}

function handleApply() {
  if (!selected.value) return
  emit('apply', { ...selected.value.form })
  selected.value = null
  emit('close')
}

function handleClose() {
  selected.value = null
  emit('close')
}
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[60] flex items-center justify-center bg-black/30 p-4 backdrop-blur-sm dark:bg-black/60"
    @click.self="handleClose"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="template-dialog-heading"
      class="flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-xl dark:border-zinc-800 dark:bg-zinc-950"
    >
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-zinc-200 px-5 py-4 dark:border-zinc-800">
        <div>
          <h2 id="template-dialog-heading" class="text-base font-semibold text-zinc-900 dark:text-zinc-100">Choose a Template</h2>
          <p class="mt-0.5 text-xs text-zinc-500 dark:text-zinc-400">Pre-fill your project with a proven structure</p>
        </div>
        <button
          @click="handleClose"
          aria-label="Close dialog"
          class="rounded-lg p-1.5 text-zinc-400 transition-colors hover:bg-zinc-100 hover:text-zinc-700 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
        >
          <X class="size-4" aria-hidden="true" />
        </button>
      </div>

      <!-- Template grid (scrollable) -->
      <div class="flex-1 overflow-y-auto p-5">
        <div class="grid gap-3 sm:grid-cols-2">
          <button
            v-for="tpl in TEMPLATES"
            :key="tpl.id"
            @click="choose(tpl)"
            :class="[
              'relative rounded-xl border p-4 text-left transition-all focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
              selected?.id === tpl.id
                ? 'border-blue-500 bg-blue-50 ring-1 ring-blue-400 dark:bg-blue-950/30'
                : 'border-zinc-200 bg-white hover:border-zinc-300 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:border-zinc-700',
            ]"
            :aria-pressed="selected?.id === tpl.id"
          >
            <!-- Selected check -->
            <div
              v-if="selected?.id === tpl.id"
              class="absolute right-3 top-3 flex size-5 items-center justify-center rounded-full bg-blue-500"
              aria-hidden="true"
            >
              <Check class="size-3 text-white" />
            </div>

            <!-- Icon -->
            <div
              class="mb-3 flex size-10 items-center justify-center rounded-lg"
              :style="{ backgroundColor: tpl.color + '20' }"
              aria-hidden="true"
            >
              <component :is="tpl.icon" class="size-5" :style="{ color: tpl.color }" />
            </div>

            <h3 class="mb-1 text-sm font-semibold text-zinc-900 dark:text-zinc-100">{{ tpl.name }}</h3>
            <p class="mb-3 text-xs leading-relaxed text-zinc-500 dark:text-zinc-400">{{ tpl.description }}</p>

            <div class="flex flex-wrap gap-1">
              <span
                v-for="tag in tpl.tags"
                :key="tag"
                class="rounded-full bg-zinc-100 px-2 py-0.5 text-xs text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400"
              >
                {{ tag }}
              </span>
            </div>
          </button>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex justify-end gap-2 border-t border-zinc-200 px-5 py-4 dark:border-zinc-800">
        <BaseButton variant="secondary" size="sm" @click="handleClose">Cancel</BaseButton>
        <BaseButton variant="primary" size="sm" :disabled="!selected" @click="handleApply">
          <Check class="size-3.5" aria-hidden="true" /> Use Template
        </BaseButton>
      </div>
    </div>
  </div>
</template>
