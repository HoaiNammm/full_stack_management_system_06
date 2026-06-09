<template>
  <div class="flex flex-col h-full overflow-hidden">
    <!-- Loading -->
    <div v-if="loading" class="flex-1 flex items-center justify-center text-on-surface-variant gap-2">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>Đang tải dự án...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="bg-error-container/20 border border-error/30 rounded-xl p-lg text-error font-label-md flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-[48px]">warning</span>
        <p>{{ error }}</p>
        <button @click="$router.push('/projects')" class="text-primary hover:underline font-label-md">← Quay lại danh sách</button>
      </div>
    </div>

    <template v-else-if="project">
      <!-- Page Header -->
      <div class="px-lg pt-6 pb-0 border-b border-outline-variant bg-surface-container-lowest flex-shrink-0">
        <div class="max-w-7xl mx-auto">
          <!-- Breadcrumb -->
          <div class="flex items-center gap-2 mb-4">
            <button @click="$router.push('/projects')"
              class="text-on-surface-variant hover:text-primary flex items-center gap-1 font-label-md text-label-md transition-colors">
              <span class="material-symbols-outlined text-[16px]">arrow_back</span>
              Dự án
            </button>
            <span class="text-outline font-label-md text-label-md">/</span>
            <span class="font-label-md text-label-md text-on-surface truncate max-w-[200px]">{{ project.name }}</span>
          </div>

          <!-- Project info -->
          <div class="flex items-start gap-md mb-4">
            <div class="w-12 h-12 rounded-xl flex items-center justify-center font-bold text-white font-headline-sm flex-shrink-0"
              :style="{ backgroundColor: project.color || '#3525cd' }">{{ initials(project.name) }}</div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-3 flex-wrap">
                <h1 class="font-headline-md text-headline-md text-on-surface">{{ project.name }}</h1>
                <span class="text-[11px] font-bold px-2 py-0.5 rounded-full"
                  :class="project.status === 1 ? 'bg-secondary-container/30 text-secondary' : 'bg-surface-container text-on-surface-variant'">
                  {{ project.status === 1 ? 'Active' : 'Draft' }}
                </span>
                <!-- Delete button -->
                <button @click="confirmDelete" :disabled="deleting"
                  class="ml-auto flex items-center gap-1 px-3 py-1 rounded-lg border border-error/40 text-error hover:bg-error-container/20 font-label-md text-label-md transition-colors disabled:opacity-60">
                  <span class="material-symbols-outlined text-[16px]" :class="{ 'animate-spin': deleting }">
                    {{ deleting ? 'progress_activity' : 'delete' }}
                  </span>
                  Xóa dự án
                </button>
              </div>
              <p class="font-body-md text-body-md text-on-surface-variant mt-1">{{ project.description }}</p>
              <div class="flex items-center gap-4 mt-2 flex-wrap">
                <div class="flex items-center gap-1 text-on-surface-variant">
                  <span class="material-symbols-outlined text-[14px]">calendar_today</span>
                  <span class="font-label-sm text-label-sm">
                    {{ formatDate(project.startDate) }} → {{ formatDate(project.endDate) }}
                  </span>
                </div>
                <div class="flex items-center gap-1 text-on-surface-variant">
                  <span class="material-symbols-outlined text-[14px]">group</span>
                  <span class="font-label-sm text-label-sm">{{ members.length }} thành viên</span>
                </div>
                <div class="flex items-center gap-1 text-on-surface-variant">
                  <span class="material-symbols-outlined text-[14px]">sprint</span>
                  <span class="font-label-sm text-label-sm">{{ sprints.length }} sprint</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Tabs -->
          <div class="flex gap-0 -mb-px">
            <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
              class="px-md py-sm font-label-lg text-label-lg border-b-2 transition-colors flex items-center gap-1.5 whitespace-nowrap"
              :class="activeTab === tab.id
                ? 'border-primary text-primary'
                : 'border-transparent text-on-surface-variant hover:text-on-surface hover:border-outline-variant'">
              <span class="material-symbols-outlined text-[16px]"
                :style="activeTab === tab.id ? `font-variation-settings: 'FILL' 1` : `font-variation-settings: 'FILL' 0`">
                {{ tab.icon }}
              </span>
              {{ tab.label }}
            </button>
          </div>
        </div>
      </div>

      <!-- Tab Content -->
      <div class="flex-1 overflow-y-auto">
        <div class="max-w-7xl mx-auto px-lg py-lg">

          <!-- Overview Tab -->
          <div v-if="activeTab === 'overview'" class="grid grid-cols-1 md:grid-cols-3 gap-md">
            <div class="md:col-span-2 flex flex-col gap-md">
              <!-- Stats -->
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-md">
                <div v-for="stat in overviewStats" :key="stat.label"
                  class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md flex flex-col gap-xs hover:shadow-sm transition-shadow">
                  <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ stat.label }}</span>
                  <div class="flex items-center gap-2">
                    <span class="material-symbols-outlined text-[20px]" :class="stat.iconColor">{{ stat.icon }}</span>
                    <span class="font-headline-sm text-headline-sm" :class="stat.color">{{ stat.value }}</span>
                  </div>
                </div>
              </div>
              <!-- Description -->
              <div class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md">
                <h3 class="font-headline-sm text-headline-sm text-on-surface mb-3">Mô tả dự án</h3>
                <p class="font-body-md text-body-md text-on-surface-variant">{{ project.description || 'Chưa có mô tả.' }}</p>
              </div>
            </div>

            <!-- Members sidebar -->
            <div class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md">
              <div class="flex justify-between items-center mb-3">
                <h3 class="font-headline-sm text-headline-sm text-on-surface">Thành viên</h3>
                <button @click="activeTab = 'members'" class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</button>
              </div>
              <div class="flex flex-col gap-2">
                <div v-for="m in members.slice(0, 6)" :key="m.id" class="flex items-center gap-2">
                  <div class="w-7 h-7 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
                    :style="{ backgroundColor: m.avatarColor || '#3525cd' }">
                    {{ m.displayName.charAt(0).toUpperCase() }}
                  </div>
                  <span class="font-label-lg text-label-lg text-on-surface flex-1 truncate">{{ m.displayName }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="roleStyle(m.role)">{{ roleName(m.role) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Members Tab -->
          <MembersSection v-if="activeTab === 'members'"
            :project="projectWithMembers" @event="fireEvent" @reload="loadDetail" />

          <!-- Sprints Tab -->
          <SprintsSection v-if="activeTab === 'sprints'"
            :project="projectWithSprints" @event="fireEvent" @reload="loadDetail" />

        </div>
      </div>
    </template>
  </div>

  <!-- Event Toast -->
  <EventToast :events="toasts" @dismiss="id => toasts = toasts.filter(e => e.id !== id)" />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MembersSection from '../components/MembersSection.vue'
import SprintsSection from '../components/SprintsSection.vue'
import EventToast     from '../components/EventToast.vue'
import { projectService, userService } from '../services/api'

const router    = useRouter()
const route     = useRoute()
const activeTab = ref('overview')
const loading   = ref(true)
const error     = ref('')
const deleting  = ref(false)

const project = ref(null)
const members = ref([])
const sprints = ref([])

const tabs = [
  { id: 'overview', label: 'Tổng quan', icon: 'dashboard' },
  { id: 'members',  label: 'Thành viên', icon: 'group' },
  { id: 'sprints',  label: 'Sprint',     icon: 'sprint' },
]

const AVATAR_COLORS = ['#3525cd','#006a61','#684000','#ba1a1a','#0f5e9c','#6a0dad','#2e7d32','#e65100']

const ROLE_NAMES  = ['Owner', 'Manager', 'Member', 'Viewer']
const ROLE_STYLES = [
  'bg-primary/10 text-primary',
  'bg-secondary/10 text-secondary',
  'bg-surface-container text-on-surface-variant',
  'bg-surface-container text-outline',
]
function roleName(role)  { return ROLE_NAMES[role]  ?? 'Unknown' }
function roleStyle(role) { return ROLE_STYLES[role] ?? '' }

function initials(name) {
  return (name || '').split(' ').map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'PR'
}

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('vi-VN')
}

const overviewStats = computed(() => [
  { label: 'Trạng thái', value: project.value?.status === 1 ? 'Active' : 'Draft', color: 'text-secondary', icon: 'check_circle', iconColor: 'text-secondary' },
  { label: 'Thành viên', value: members.value.length, color: 'text-on-surface', icon: 'group', iconColor: 'text-primary' },
  { label: 'Sprint',     value: sprints.value.length, color: 'text-on-surface', icon: 'sprint', iconColor: 'text-tertiary' },
  { label: 'Quá hạn',   value: 0, color: 'text-secondary', icon: 'warning', iconColor: 'text-secondary' },
])

const projectWithMembers = computed(() => ({
  ...(project.value || {}),
  members: members.value.map(m => ({
    ...m,
    name:    m.displayName,
    email:   m.displayEmail,
    role:    roleName(m.role),   // MembersSection expects string "Owner"/"Manager"/...
    joinedAt: m.joinedAt ? formatDate(m.joinedAt) : '',
  }))
}))

const projectWithSprints = computed(() => ({
  ...(project.value || {}),
  sprints: sprints.value
}))

async function loadDetail() {
  const id = route.params.id
  if (!id) { error.value = 'Không tìm thấy ID dự án'; loading.value = false; return }

  loading.value = true
  error.value   = ''
  try {
    const [p, m, s, allUsers] = await Promise.all([
      projectService.getById(id),
      projectService.getMembers(id).catch(() => []),
      projectService.getSprints(id).catch(() => []),
      userService.getAll().catch(() => []),
    ])

    // Build a lookup map: userId → user info
    const userMap = Object.fromEntries((allUsers || []).map(u => [u.id, u]))

    project.value = p
    sprints.value = s || []
    members.value = (m || []).map((mem, i) => {
      const u = userMap[mem.userId]
      return {
        ...mem,
        displayName:  u?.fullName  || mem.userId,
        displayEmail: u?.email     || '',
        avatarColor:  AVATAR_COLORS[i % AVATAR_COLORS.length],
      }
    })
  } catch (e) {
    if (e.response?.status === 404) error.value = 'Dự án không tồn tại.'
    else error.value = 'Không thể tải thông tin dự án. Kiểm tra ProjectService đang chạy.'
  } finally {
    loading.value = false
  }
}

// Toast system
const toasts       = ref([])
let toastCounter   = 0

function fireEvent(event) {
  const id = ++toastCounter
  toasts.value.push({ ...event, id })
  setTimeout(() => { toasts.value = toasts.value.filter(e => e.id !== id) }, 5000)
}

async function confirmDelete() {
  if (!confirm(`Xóa dự án "${project.value?.name}"?\nThao tác này không thể hoàn tác.`)) return
  deleting.value = true
  try {
    await projectService.delete(route.params.id)
    router.push('/projects')
  } catch (e) {
    alert('Không thể xóa dự án: ' + (e.response?.data?.message || e.message))
  } finally {
    deleting.value = false
  }
}

onMounted(loadDetail)
</script>
