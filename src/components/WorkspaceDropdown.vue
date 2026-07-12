<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { ChevronDown, Check, Plus } from 'lucide-vue-next'

const workspaceStore = useWorkspaceStore()
const router = useRouter()

const isOpen      = ref(false)
const creating    = ref(false)
const newName     = ref('')
const dropdownRef = ref(null)

const currentWorkspace = computed(() =>
  workspaceStore.workspaces.find(w => w.id === workspaceStore.currentWorkspaceId) || null
)

const WORKSPACE_PALETTE = [
  ['#8b5cf6', '#7c3aed'], // purple
  ['#ef4444', '#dc2626'], // red
  ['#64748b', '#475569'], // grey
  ['#22c55e', '#16a34a'], // green
  ['#06b6d4', '#0891b2'], // cyan
  ['#f97316', '#ea580c'], // orange
  ['#ec4899', '#db2777'], // pink
  ['#eab308', '#ca8a04'], // yellow
  ['#14b8a6', '#0d9488'], // teal
  ['#6366f1', '#4f46e5'], // indigo
]
function workspaceGradient(id = '') {
  let h = 0
  for (const c of String(id)) h = c.charCodeAt(0) + h * 31
  const [from, to] = WORKSPACE_PALETTE[Math.abs(h) % WORKSPACE_PALETTE.length]
  return `linear-gradient(135deg, ${from}, ${to})`
}

function onSelectWorkspace(id) {
  workspaceStore.setCurrentWorkspace(id)
  isOpen.value = false
  router.push('/')
}

async function handleCreate(e) {
  e.preventDefault()
  if (!newName.value.trim()) return
  creating.value = true
  await workspaceStore.createWorkspace({ name: newName.value.trim() })
  newName.value = ''
  creating.value = false
  isOpen.value = false
  router.push('/')
}

function handleClickOutside(event) {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target)) isOpen.value = false
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside))
</script>

<template>
  <div class="relative" ref="dropdownRef">
    <!-- Brand header -->
    <button
      @click="isOpen = !isOpen"
      :aria-expanded="isOpen"
      aria-haspopup="listbox"
      class="flex w-full items-center gap-3 px-4 py-4 focus-visible:outline-none"
      style="color: var(--sidebar-text-active);"
    >
      <!-- Logo mark: colored square like Dash Optima -->
      <div
        class="flex size-8 flex-shrink-0 items-center justify-center rounded-lg text-sm font-bold text-white"
        :style="{ background: workspaceGradient(currentWorkspace?.id) }"
      >
        {{ currentWorkspace?.name?.[0]?.toUpperCase() || 'W' }}
      </div>
      <div class="min-w-0 flex-1 text-left">
        <p class="truncate text-sm font-semibold leading-tight" style="color: var(--sidebar-text-active);">
          {{ currentWorkspace?.name || 'Workspace' }}
        </p>
        <p class="text-[11px] truncate" style="color: var(--sidebar-section-hd);">
          {{ workspaceStore.workspaces.length }} workspace{{ workspaceStore.workspaces.length !== 1 ? 's' : '' }}
        </p>
      </div>
      <ChevronDown
        :class="['size-4 flex-shrink-0 transition-transform', isOpen ? 'rotate-180' : '']"
        style="color: var(--sidebar-section-hd);"
        aria-hidden="true"
      />
    </button>

    <!-- Dropdown — floats over content, uses white surface -->
    <div
      v-if="isOpen"
      class="absolute left-3 right-3 top-full z-50 mt-1 overflow-hidden rounded-xl"
      style="
        background: #ffffff;
        box-shadow: 0 8px 24px rgba(0,0,0,0.15), 0 0 0 1px rgba(0,0,0,0.06);
      "
      role="listbox"
      aria-label="Workspaces"
    >
      <div class="p-2">
        <p class="mb-1.5 px-2 text-[10px] font-semibold uppercase tracking-wider text-slate-400">
          Workspaces
        </p>
        <button
          v-for="ws in workspaceStore.workspaces"
          :key="ws.id"
          @click="onSelectWorkspace(ws.id)"
          class="flex w-full items-center gap-3 rounded-lg p-2 text-left transition-colors hover:bg-slate-50 focus-visible:bg-slate-50 focus-visible:outline-none"
          role="option"
          :aria-selected="workspaceStore.currentWorkspaceId === ws.id"
        >
          <div class="flex size-7 flex-shrink-0 items-center justify-center rounded-lg text-xs font-bold text-white"
            :style="{ background: workspaceGradient(ws.id) }"
          >
            {{ ws.name?.[0]?.toUpperCase() }}
          </div>
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-medium text-slate-800">{{ ws.name }}</p>
            <p class="truncate text-xs text-slate-400">{{ ws.memberCount || 0 }} members</p>
          </div>
          <Check
            v-if="workspaceStore.currentWorkspaceId === ws.id"
            class="size-4 flex-shrink-0 text-blue-500"
            aria-hidden="true"
          />
        </button>
      </div>

      <div class="mx-2 h-px bg-slate-100" />

      <div class="p-2">
        <form @submit.prevent="handleCreate" class="flex gap-2">
          <input
            v-model="newName"
            placeholder="New workspace…"
            class="flex-1 rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-xs text-slate-800 placeholder:text-slate-400 focus-visible:border-blue-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-400/20"
          />
          <button
            type="submit"
            :disabled="creating || !newName.trim()"
            aria-label="Create workspace"
            class="flex items-center justify-center rounded-lg bg-blue-500 px-2.5 py-1.5 text-white transition-colors hover:bg-blue-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-400 disabled:opacity-40"
          >
            <Plus class="size-3.5" aria-hidden="true" />
          </button>
        </form>
      </div>
    </div>
  </div>
</template>
