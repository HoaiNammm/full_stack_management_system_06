<script setup>
import { ref, watch, computed } from 'vue'
import { format } from 'date-fns'
import { Plus, Save, Trash2 } from 'lucide-vue-next'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { projectApi } from '../api/projects'
import AddProjectMember from './AddProjectMember.vue'

const props = defineProps({ project: Object })
const emit  = defineEmits(['updated'])

const workspaceStore = useWorkspaceStore()
const wid = computed(() => workspaceStore.currentWorkspaceId)

// ── Project details form ──────────────────────────────────────────────────────
const formData = ref({ name: '', description: '', status: 'Planning', color: '', startDate: '', endDate: '' })
const isSubmitting = ref(false)
const saveError    = ref('')

watch(() => props.project, (p) => {
  if (p) formData.value = {
    name:        p.name        || '',
    description: p.description || '',
    status:      p.status      || 'Planning',
    color:       p.color       || '',
    startDate:   p.startDate   ? formatDate(p.startDate) : '',
    endDate:     p.endDate     ? formatDate(p.endDate)   : '',
  }
}, { immediate: true })

async function handleSubmit() {
  if (!props.project) return
  isSubmitting.value = true
  saveError.value    = ''
  try {
    const updated = await projectApi.update(wid.value, props.project.id, {
      name:        formData.value.name,
      description: formData.value.description || null,
      status:      formData.value.status,
      color:       formData.value.color || null,
      startDate:   formData.value.startDate || null,
      endDate:     formData.value.endDate   || null,
    })
    const idx = workspaceStore.projects.findIndex(p => p.id === props.project.id)
    if (idx !== -1) workspaceStore.projects[idx] = { ...workspaceStore.projects[idx], ...updated }
    emit('updated', updated)
  } catch (err) {
    saveError.value = err?.response?.data?.message || err?.response?.data?.error || 'Failed to save changes'
  } finally {
    isSubmitting.value = false
  }
}

// ── Member role update + remove ───────────────────────────────────────────────
const memberErrors   = ref({})
const updatingMember = ref(null)
const removingMember = ref(null)

async function handleUpdateRole(member, newRole) {
  updatingMember.value = member.id
  memberErrors.value[member.id] = ''
  try {
    await projectApi.updateMember(wid.value, props.project.id, member.id, { role: newRole })
    const proj = workspaceStore.projects.find(p => p.id === props.project.id)
    if (proj?.members) {
      const m = proj.members.find(x => x.id === member.id)
      if (m) m.role = newRole
    }
  } catch (err) {
    memberErrors.value[member.id] = err?.response?.data?.message || err?.response?.data?.error || 'Failed to update role'
  } finally {
    updatingMember.value = null
  }
}

async function handleRemoveMember(member) {
  const label = member.user?.email || member.userId || 'this member'
  if (!confirm(`Remove ${label} from project?`)) return
  removingMember.value = member.id
  memberErrors.value[member.id] = ''
  try {
    await projectApi.removeMember(wid.value, props.project.id, member.id)
    const proj = workspaceStore.projects.find(p => p.id === props.project.id)
    if (proj?.members) proj.members = proj.members.filter(m => m.id !== member.id)
  } catch (err) {
    memberErrors.value[member.id] = err?.response?.data?.message || err?.response?.data?.error || 'Failed to remove member'
  } finally {
    removingMember.value = null
  }
}

const isDialogOpen = ref(false)

async function onMemberAdded() {
  if (props.project?.id) await workspaceStore.fetchProject(props.project.id)
  emit('updated', workspaceStore.projects.find(p => p.id === props.project?.id) || props.project)
}

function formatDate(date) {
  if (!date) return ''
  try { return format(new Date(date), 'yyyy-MM-dd') } catch { return '' }
}

const inputCls = 'w-full px-3 py-2 rounded mt-2 border text-sm dark:bg-zinc-900 border-zinc-300 dark:border-zinc-700 text-zinc-900 dark:text-zinc-300'
const cardCls  = 'rounded-lg border p-6 bg-white dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50 border-zinc-300 dark:border-zinc-800'
const lblCls   = 'text-sm text-zinc-600 dark:text-zinc-400'
</script>

<template>
  <div class="grid lg:grid-cols-2 gap-8">
    <!-- Project Details -->
    <div :class="cardCls">
      <h2 class="text-lg font-medium text-zinc-900 dark:text-zinc-300 mb-4">Project Details</h2>
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-2">
          <label :class="lblCls">Project Name</label>
          <input v-model="formData.name" :class="inputCls" required />
        </div>
        <div class="space-y-2">
          <label :class="lblCls">Description</label>
          <textarea v-model="formData.description" :class="inputCls + ' h-24'" />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <label :class="lblCls">Status</label>
            <select v-model="formData.status" :class="inputCls">
              <option value="Planning">Planning</option>
              <option value="Active">Active</option>
              <option value="OnHold">On Hold</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          <div class="space-y-2">
            <label :class="lblCls">Color</label>
            <input type="text" v-model="formData.color" placeholder="#3b82f6" :class="inputCls" />
          </div>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <label :class="lblCls">Start Date</label>
            <input type="date" v-model="formData.startDate" :class="inputCls" />
          </div>
          <div class="space-y-2">
            <label :class="lblCls">End Date</label>
            <input type="date" v-model="formData.endDate" :min="formData.startDate" :class="inputCls" />
          </div>
        </div>
        <p v-if="saveError" class="text-sm text-red-500">{{ saveError }}</p>
        <button type="submit" :disabled="isSubmitting"
          class="ml-auto flex items-center text-sm justify-center gap-2 bg-gradient-to-br from-blue-500 to-blue-600 text-white px-4 py-2 rounded disabled:opacity-60">
          <Save class="size-4" /> {{ isSubmitting ? 'Saving...' : 'Save Changes' }}
        </button>
      </form>
    </div>

    <!-- Team Members -->
    <div :class="cardCls">
      <div class="flex items-center justify-between gap-4 mb-4">
        <h2 class="text-lg font-medium text-zinc-900 dark:text-zinc-300">
          Team Members <span class="text-sm text-zinc-600 dark:text-zinc-400">({{ project?.members?.length || 0 }})</span>
        </h2>
        <button type="button" @click="isDialogOpen = true"
          class="flex items-center gap-1 px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 hover:bg-zinc-100 dark:hover:bg-zinc-800">
          <Plus class="size-3.5" /> Add
        </button>
        <AddProjectMember :isOpen="isDialogOpen" @close="isDialogOpen = false" @added="onMemberAdded" />
      </div>

      <div v-if="project?.members?.length > 0" class="space-y-2 max-h-80 overflow-y-auto pr-1">
        <div v-for="member in project.members" :key="member.id"
          class="group rounded dark:bg-zinc-800/60 border border-zinc-200 dark:border-zinc-700 px-3 py-2">
          <div class="flex items-center justify-between gap-2">
            <span class="text-sm text-zinc-900 dark:text-zinc-200 truncate">
              {{ member.user?.name || member.user?.email || member.userId || 'Unknown' }}
            </span>
            <div class="flex items-center gap-2 flex-shrink-0">
              <select
                :value="member.role"
                :disabled="member.role === 'Owner' || updatingMember === member.id"
                @change="handleUpdateRole(member, $event.target.value)"
                class="text-xs rounded border border-zinc-300 dark:border-zinc-600 dark:bg-zinc-900 px-2 py-0.5 disabled:opacity-60">
                <option value="Owner">Owner</option>
                <option value="Manager">Manager</option>
                <option value="Member">Member</option>
                <option value="Viewer">Viewer</option>
              </select>
              <button
                v-if="member.role !== 'Owner'"
                @click="handleRemoveMember(member)"
                :disabled="removingMember === member.id"
                class="opacity-0 group-hover:opacity-100 p-0.5 text-zinc-400 hover:text-red-500 transition disabled:opacity-30">
                <Trash2 class="size-3.5" />
              </button>
            </div>
          </div>
          <p v-if="memberErrors[member.id]" class="text-xs text-red-500 mt-1">{{ memberErrors[member.id] }}</p>
        </div>
      </div>
      <p v-else class="text-sm text-zinc-500 dark:text-zinc-400 py-4 text-center">
        No members yet. Click Add to invite team members.
      </p>
    </div>
  </div>
</template>
