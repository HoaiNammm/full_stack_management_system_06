<template>
  <div class="flex flex-col gap-lg">
    <!-- If a sprint is selected for detail view -->
    <template v-if="detailSprint">
      <SprintDetail
        :sprint="detailSprint"
        :project="project"
        @back="detailSprint = null"
        @start="startSprint"
        @complete="completeSprint"
        @reload="$emit('reload')" />
    </template>

    <!-- Sprint list view -->
    <template v-else>
      <!-- Header -->
      <div class="gradient-panel p-lg overflow-hidden">
        <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p class="page-eyebrow">Delivery rhythm</p>
            <h3 class="font-headline-md text-headline-md text-on-surface">Sprint planning</h3>
            <p class="font-body-md text-body-md text-on-surface-variant mt-1">
              Theo dõi chu kỳ phát triển, mục tiêu sprint và trạng thái release.
            </p>
          </div>
          <button v-if="canManageSprints" @click="showModal = true"
            class="app-button-primary flex items-center gap-xs">
            <span class="material-symbols-outlined text-[18px]">add</span>
            Tạo sprint mới
          </button>
        </div>
        <div class="relative z-10 grid grid-cols-2 md:grid-cols-4 gap-sm mt-lg">
          <div class="metric-tile">
            <span>Total sprint</span>
            <strong>{{ sprints.length }}</strong>
          </div>
          <div class="metric-tile">
            <span>Planned</span>
            <strong>{{ sprints.filter(s => s.status === 0).length }}</strong>
          </div>
          <div class="metric-tile">
            <span>Active</span>
            <strong>{{ sprints.filter(s => s.status === 1).length }}</strong>
          </div>
          <div class="metric-tile">
            <span>Done</span>
            <strong>{{ sprints.filter(s => s.status === 2).length }}</strong>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-if="sprints.length === 0"
        class="flex flex-col items-center justify-center gap-3 p-xl app-panel border-2 border-dashed border-outline-variant text-center">
        <span class="material-symbols-outlined text-[48px] text-on-surface-variant/40">sprint</span>
        <p class="font-headline-sm text-headline-sm text-on-surface-variant">Chưa có sprint nào</p>
        <p class="font-body-md text-body-md text-outline">Tạo sprint đầu tiên để bắt đầu quản lý tiến độ dự án</p>
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-2 gap-md">
        <div v-for="sprint in sprints" :key="sprint.id"
          class="workspace-card overflow-hidden cursor-pointer interactive-card"
          :class="sprint.status === 1 ? 'border-primary/30' : 'border-outline-variant'"
          @click="openDetail(sprint)">
          <!-- Active bar -->
          <div class="h-1 w-full bg-surface-container-high">
            <div class="h-full rounded-r-full transition-all"
              :class="sprint.status === 2 ? 'bg-secondary' : sprint.status === 1 ? 'bg-primary' : 'bg-outline'"
              :style="{ width: progressForSprint(sprint) + '%' }"></div>
          </div>

          <div class="pt-md">
            <div class="flex items-start justify-between gap-3">
              <div class="flex items-center gap-3 flex-1 min-w-0">
                <div class="w-12 h-12 rounded-2xl flex items-center justify-center flex-shrink-0"
                  :class="sprint.status === 2 ? 'bg-secondary/10 text-secondary' : sprint.status === 1 ? 'bg-primary/10 text-primary' : 'bg-surface-container-high text-on-surface-variant'">
                  <span class="material-symbols-outlined text-[22px]"
                    :style="sprint.status === 1 ? `font-variation-settings: 'FILL' 1` : `font-variation-settings: 'FILL' 0`">sprint</span>
                </div>
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 flex-wrap">
                    <h4 class="font-headline-sm text-headline-sm text-on-surface">{{ sprint.name }}</h4>
                    <span class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="statusStyle(sprint.status)">
                      {{ statusLabel[sprint.status] }}
                    </span>
                  </div>
                  <div class="flex items-center gap-1 mt-1">
                    <span class="material-symbols-outlined text-[13px] text-on-surface-variant">calendar_today</span>
                    <span class="font-label-sm text-label-sm text-on-surface-variant">
                      {{ fmt(sprint.startDate) }} → {{ fmt(sprint.endDate) }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- Actions (stop click propagation so card click doesn't also fire) -->
              <div class="flex-shrink-0" @click.stop>
                <button v-if="canManageSprints && sprint.status === 0" @click="startSprint(sprint)" :disabled="saving === sprint.id"
                  class="flex items-center gap-1 px-3 py-1.5 border border-primary text-primary rounded-xl font-label-md text-label-md hover:bg-primary hover:text-on-primary transition-colors disabled:opacity-60">
                  <span class="material-symbols-outlined text-[16px]" :class="saving === sprint.id ? 'animate-spin' : ''">
                    {{ saving === sprint.id ? 'progress_activity' : 'play_arrow' }}
                  </span>
                  Bắt đầu
                </button>
                <button v-else-if="canManageSprints && sprint.status === 1" @click="completeSprint(sprint)" :disabled="saving === sprint.id"
                  class="flex items-center gap-1 px-3 py-1.5 border border-secondary text-secondary rounded-xl font-label-md text-label-md hover:bg-secondary hover:text-on-secondary transition-colors disabled:opacity-60">
                  <span class="material-symbols-outlined text-[16px]" :class="saving === sprint.id ? 'animate-spin' : ''">
                    {{ saving === sprint.id ? 'progress_activity' : 'check_circle' }}
                  </span>
                  Hoàn thành
                </button>
                <div v-else class="flex items-center gap-1 text-secondary">
                  <span class="material-symbols-outlined text-[16px]" style="font-variation-settings: 'FILL' 1">check_circle</span>
                  <span class="font-label-md text-label-md">Đã hoàn thành</span>
                </div>
              </div>
            </div>

            <div class="mt-md grid grid-cols-3 gap-sm">
              <div class="metric-tile">
                <span>Progress</span>
                <strong>{{ progressForSprint(sprint) }}%</strong>
              </div>
              <div class="metric-tile">
                <span>Start</span>
                <strong class="!text-label-lg">{{ fmtShort(sprint.startDate) }}</strong>
              </div>
              <div class="metric-tile">
                <span>End</span>
                <strong class="!text-label-lg">{{ fmtShort(sprint.endDate) }}</strong>
              </div>
            </div>

            <!-- Goal -->
            <div class="mt-3 p-3 bg-surface-container-low rounded-2xl border border-outline-variant/50">
              <p class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider mb-1">Mục tiêu Sprint</p>
              <p class="font-body-md text-body-md text-on-surface">{{ sprint.goal }}</p>
            </div>

            <!-- Click hint -->
            <p class="mt-3 font-label-sm text-label-sm text-outline flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px]">open_in_full</span>
              Click để xem chi tiết
            </p>
          </div>
        </div>
      </div>
    </template>
  </div>

  <CreateSprintModal v-if="showModal && canManageSprints" @close="showModal = false" @created="handleCreated" />
</template>

<script setup>
import { computed, ref } from 'vue'
import CreateSprintModal from './CreateSprintModal.vue'
import SprintDetail      from './SprintDetail.vue'
import { useAuth } from '../composables/useAuth'
import { projectService } from '../services/api'

const props = defineProps(['project'])
const emit  = defineEmits(['event', 'reload'])
const { user } = useAuth()

const sprints     = ref(props.project.sprints.map(s => ({ ...s })))
const showModal   = ref(false)
const saving      = ref(null)
const detailSprint = ref(null)
const currentMember = computed(() =>
  (props.project.members || []).find(member => String(member.userId).toLowerCase() === String(user.value?.id || '').toLowerCase())
)
const canManageSprints = computed(() => ['Owner', 'Project Manager'].includes(currentMember.value?.role))

const statusLabel = ['Planned', 'Active', 'Completed']
const statusStyle = (s) => [
  'bg-surface-container text-on-surface-variant',
  'bg-primary/10 text-primary',
  'bg-secondary-container/30 text-secondary',
][s]

function fmt(d) {
  return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function fmtShort(d) {
  if (!d) return '--'
  return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
}

function progressForSprint(sprint) {
  if (sprint.status === 2) return 100
  if (sprint.status === 0) return 12
  const start = new Date(sprint.startDate).getTime()
  const end = new Date(sprint.endDate || Date.now()).getTime()
  const now = Date.now()
  if (!start || !end || end <= start) return 45
  return Math.max(15, Math.min(92, Math.round(((now - start) / (end - start)) * 100)))
}

function openDetail(sprint) {
  detailSprint.value = sprint
}

async function handleCreated(sprint) {
  if (!canManageSprints.value) return
  try {
    const created = await projectService.createSprint(props.project.id, {
      name:        sprint.name,
      description: sprint.description || '',
      goal:        sprint.goal || '',
      startDate:   sprint.startDate,
    })
    sprints.value.push(created)
    showModal.value = false
    emit('reload')
  } catch (e) {
    alert('Không thể tạo sprint: ' + (e.response?.data?.message || e.message))
  }
}

async function startSprint(sprint) {
  if (!canManageSprints.value) return
  if (sprints.value.some(s => s.status === 1)) {
    alert('Đang có sprint Active. Hãy hoàn thành sprint hiện tại trước khi bắt đầu sprint mới.')
    return
  }
  saving.value = sprint.id
  try {
    await projectService.updateSprint(props.project.id, sprint.id, {
      name: sprint.name, description: sprint.description || '',
      goal: sprint.goal || '', startDate: sprint.startDate,
      endDate: sprint.endDate, status: 1,
    })
    if (props.project.status !== 1) {
      await projectService.update(props.project.id, {
        name: props.project.name, description: props.project.description || '',
        status: 1, color: props.project.color,
        startDate: props.project.startDate, endDate: props.project.endDate,
      })
    }
    sprint.status = 1
    if (detailSprint.value?.id === sprint.id) detailSprint.value = { ...sprint }
    emit('event', {
      type: 'sprint.started', icon: 'sprint', iconBg: 'bg-primary/10 text-primary',
      summary: `${sprint.name} đã bắt đầu`,
      time: new Date().toLocaleTimeString('vi-VN'),
    })
    emit('reload')
  } catch { /* ignore */ }
  finally { saving.value = null }
}

async function completeSprint(sprint) {
  if (!canManageSprints.value) return
  saving.value = sprint.id
  try {
    await projectService.updateSprint(props.project.id, sprint.id, {
      name: sprint.name, description: sprint.description || '',
      goal: sprint.goal || '', startDate: sprint.startDate,
      endDate: sprint.endDate, status: 2,
    })
    sprint.status = 2
    if (detailSprint.value?.id === sprint.id) detailSprint.value = { ...sprint }
    emit('reload')
  } catch { /* ignore */ }
  finally { saving.value = null }
}
</script>
