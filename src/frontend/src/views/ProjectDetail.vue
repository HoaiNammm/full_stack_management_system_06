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
      <div class="px-lg pt-6 pb-0 border-b border-outline-variant bg-surface-container-lowest/85 flex-shrink-0 backdrop-blur-xl">
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
          <div class="gradient-panel p-lg mb-4">
          <div class="relative z-10 flex items-start gap-md">
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
                <button v-if="canDeleteProject" @click="confirmDelete" :disabled="deleting"
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
          </div>

          <!-- Tabs -->
          <div class="flex gap-2 -mb-px overflow-x-auto">
            <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
              class="px-md py-sm font-label-lg text-label-lg rounded-t-xl transition-colors flex items-center gap-1.5 whitespace-nowrap border border-transparent"
              :class="activeTab === tab.id
                ? 'bg-surface-container-lowest border-outline-variant text-primary shadow-sm'
                : 'text-on-surface-variant hover:text-on-surface hover:bg-surface-container-low'">
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
          <div v-if="activeTab === 'overview'" class="grid grid-cols-1 xl:grid-cols-[1fr_340px] gap-lg">
            <div class="flex flex-col gap-md">
              <!-- Stats -->
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-md">
                <div v-for="stat in overviewStats" :key="stat.label"
                  class="app-card interactive-card p-md flex flex-col gap-xs">
                  <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ stat.label }}</span>
                  <div class="flex items-center gap-2">
                    <span class="material-symbols-outlined text-[20px]" :class="stat.iconColor">{{ stat.icon }}</span>
                    <span class="font-headline-sm text-headline-sm" :class="stat.color">{{ stat.value }}</span>
                  </div>
                </div>
              </div>
              <!-- Description -->
              <div class="app-panel p-md">
                <h3 class="font-headline-sm text-headline-sm text-on-surface mb-3">Mô tả dự án</h3>
                <p class="font-body-md text-body-md text-on-surface-variant">{{ project.description || 'Chưa có mô tả.' }}</p>
              </div>

              <div class="grid grid-cols-1 lg:grid-cols-2 gap-md">
                <div class="app-panel p-md">
                  <div class="flex items-start justify-between gap-3">
                    <div>
                      <p class="page-eyebrow">Project health</p>
                      <h3 class="font-headline-sm text-headline-sm text-on-surface">Tình trạng dự án</h3>
                    </div>
                    <span class="soft-badge" :class="projectHealth.badge">{{ projectHealth.label }}</span>
                  </div>
                  <div class="mt-md h-2 rounded-full bg-surface-container-high overflow-hidden">
                    <div class="h-full rounded-full bg-primary" :style="{ width: projectProgress + '%' }"></div>
                  </div>
                  <div class="mt-md grid grid-cols-3 gap-sm">
                    <div class="metric-tile !p-sm">
                      <span>Progress</span>
                      <strong class="!text-title-md">{{ projectProgress }}%</strong>
                    </div>
                    <div class="metric-tile !p-sm">
                      <span>Risk</span>
                      <strong class="!text-title-md">{{ riskyTasks.length }}</strong>
                    </div>
                    <div class="metric-tile !p-sm">
                      <span>Done</span>
                      <strong class="!text-title-md">{{ doneTaskCount }}</strong>
                    </div>
                  </div>
                </div>

                <div class="app-panel p-md">
                  <div class="flex items-start justify-between gap-3">
                    <div>
                      <p class="page-eyebrow">Active sprint</p>
                      <h3 class="font-headline-sm text-headline-sm text-on-surface">{{ activeSprint?.name || 'Chưa có sprint đang chạy' }}</h3>
                    </div>
                    <span class="material-symbols-outlined text-primary">rocket_launch</span>
                  </div>
                  <p class="font-body-md text-body-md text-on-surface-variant mt-2">
                    {{ activeSprint?.goal || 'Bắt đầu sprint để theo dõi nhịp phát triển và milestone.' }}
                  </p>
                  <button @click="activeTab = 'sprints'" class="app-button-secondary mt-md">Xem sprint</button>
                </div>
              </div>

              <div class="app-panel">
                <div class="px-md py-sm border-b border-outline-variant flex items-center justify-between">
                  <div>
                    <h3 class="font-headline-sm text-headline-sm text-on-surface">Upcoming deadlines</h3>
                    <p class="font-body-md text-body-md text-on-surface-variant">Task sắp tới và task cần chú ý.</p>
                  </div>
                  <button @click="activeTab = 'kanban'" class="font-label-md text-label-md text-primary">Mở Kanban</button>
                </div>
                <div class="p-md grid grid-cols-1 md:grid-cols-2 gap-sm">
                  <div v-for="task in upcomingTasks" :key="task.id" class="data-row">
                    <p class="font-label-lg text-label-lg text-on-surface truncate">{{ task.title }}</p>
                    <p class="font-label-sm text-label-sm text-on-surface-variant mt-1">{{ formatDate(task.dueDate) }}</p>
                  </div>
                  <div v-if="!upcomingTasks.length" class="text-on-surface-variant font-body-md text-body-md">
                    Chưa có deadline gần.
                  </div>
                </div>
              </div>
            </div>

            <!-- Members sidebar -->
            <div class="flex flex-col gap-md">
            <div class="app-panel p-md">
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

            <div class="app-panel">
              <div class="px-md py-sm border-b border-outline-variant">
                <h3 class="font-headline-sm text-headline-sm text-on-surface">Activity snapshot</h3>
              </div>
              <div class="p-md flex flex-col gap-sm">
                <div v-for="log in activities.slice(0, 4)" :key="log.id" class="data-row flex items-start gap-3">
                  <div class="w-8 h-8 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                    <span class="material-symbols-outlined text-[17px]">history</span>
                  </div>
                  <div class="min-w-0">
                    <p class="font-label-md text-label-md text-on-surface truncate">{{ log.description || log.action }}</p>
                    <p class="font-label-sm text-label-sm text-on-surface-variant">{{ formatDateTime(log.createdAt) }}</p>
                  </div>
                </div>
                <div v-if="!activities.length" class="text-on-surface-variant font-body-md text-body-md">
                  Chưa có activity log.
                </div>
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

          <!-- Timeline Tab -->
          <div v-if="activeTab === 'timeline'" class="flex flex-col gap-md">
            <div class="gradient-panel p-lg overflow-hidden">
              <div class="relative z-10 flex items-center justify-between gap-3 mb-6">
                <div>
                  <p class="page-eyebrow">Project timeline</p>
                  <h3 class="font-headline-md text-headline-md text-on-surface">Timeline & Gantt</h3>
                  <p class="font-body-md text-body-md text-on-surface-variant">
                    Tổng hợp ngày bắt đầu, ngày kết thúc, sprint, milestone và deadline task.
                  </p>
                </div>
                <button @click="$router.push({ path: '/calendar', query: { projectId: project.id } })"
                  class="app-button-secondary">
                  Calendar
                </button>
              </div>
              <div class="relative z-10 flex flex-col gap-3">
                <div v-for="item in timelineItems" :key="item.id" class="grid grid-cols-[92px_1fr_auto] md:grid-cols-[140px_1fr_auto] gap-3 items-center">
                  <span class="font-label-md text-label-md text-on-surface-variant">{{ formatDate(item.date) }}</span>
                  <div class="h-12 rounded-2xl border border-outline-variant bg-surface-container-low flex items-center overflow-hidden shadow-sm">
                    <div class="h-full w-1.5" :class="item.color"></div>
                    <div class="px-3 min-w-0">
                      <p class="font-label-lg text-label-lg text-on-surface truncate">{{ item.title }}</p>
                      <p class="font-label-sm text-label-sm text-on-surface-variant">{{ item.kind }}</p>
                    </div>
                  </div>
                  <span class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="item.badgeClass">{{ item.status }}</span>
                </div>
                <div v-if="timelineItems.length === 0" class="text-center py-xl text-on-surface-variant">
                  Chưa có sprint, milestone hoặc task deadline để hiển thị.
                </div>
              </div>
            </div>
          </div>

          <!-- Kanban Tab -->
          <div v-if="activeTab === 'kanban'" class="app-panel p-lg flex flex-col gap-lg">
            <div class="flex items-center justify-between gap-3">
              <div>
                <p class="page-eyebrow">Execution board</p>
                <h3 class="font-headline-md text-headline-md text-on-surface">Kanban preview</h3>
                <p class="font-body-md text-body-md text-on-surface-variant">
                  Mở board đầy đủ để kéo thả task, gán deadline và ưu tiên.
                </p>
              </div>
              <button @click="$router.push({ path: '/kanban', query: { projectId: project.id } })"
                class="app-button-primary flex items-center gap-1">
                <span class="material-symbols-outlined text-[18px]">view_kanban</span>
                Mở Kanban
              </button>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-md">
              <div v-for="summary in kanbanSummary" :key="summary.label"
                class="workspace-card">
                <p class="font-label-md text-label-md uppercase tracking-wider text-on-surface-variant">{{ summary.label }}</p>
                <div class="mt-3 flex items-end justify-between gap-3">
                  <p class="font-headline-lg text-headline-lg text-on-surface">{{ summary.value }}</p>
                  <div class="h-2 flex-1 rounded-full bg-surface-container-high overflow-hidden max-w-[120px]">
                    <div class="h-full rounded-full bg-primary" :style="{ width: Math.min(100, summary.value * 18 + 8) + '%' }"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Activity Tab -->
          <div v-if="activeTab === 'activity'" class="app-panel">
            <div class="px-md py-sm border-b border-outline-variant">
              <h3 class="font-headline-sm text-headline-sm text-on-surface">Activity Log</h3>
              <p class="font-body-md text-body-md text-on-surface-variant">Hoạt động được ghi nhận từ NotifyService.</p>
            </div>
            <div v-if="activityLoading" class="p-lg flex items-center gap-2 text-on-surface-variant">
              <span class="material-symbols-outlined animate-spin">progress_activity</span>
              Đang tải activity...
            </div>
            <div v-else-if="activities.length === 0" class="p-lg text-center text-on-surface-variant">
              Chưa có activity log cho dự án này.
            </div>
            <div v-else class="divide-y divide-outline-variant/50">
              <div v-for="log in activities" :key="log.id" class="px-md py-sm flex items-start gap-3">
                <div class="w-8 h-8 rounded-full bg-primary/10 text-primary flex items-center justify-center flex-shrink-0">
                  <span class="material-symbols-outlined text-[18px]">history</span>
                </div>
                <div class="min-w-0">
                  <p class="font-label-lg text-label-lg text-on-surface">{{ log.description || log.action }}</p>
                  <p class="font-label-sm text-label-sm text-on-surface-variant">{{ formatDateTime(log.createdAt) }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Comments Tab -->
          <div v-if="activeTab === 'comments'" class="gradient-panel p-lg overflow-hidden">
            <div class="relative z-10 grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-lg items-center">
              <div>
                <p class="page-eyebrow">Collaboration</p>
                <h3 class="font-headline-md text-headline-md text-on-surface">Project comments</h3>
                <p class="font-body-lg text-body-lg text-on-surface-variant mt-2">
                  NotifyService hiện đã hỗ trợ comment theo task. Bình luận cấp dự án là phần backend cần bổ sung để hoàn thiện luồng cộng tác ở cấp workspace.
                </p>
                <div class="mt-md flex flex-wrap gap-sm">
                  <code class="soft-badge">GET /api/projects/{projectId}/comments</code>
                  <code class="soft-badge">POST /api/projects/{projectId}/comments</code>
                </div>
              </div>
              <div class="glass-card p-md">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-2xl bg-primary/10 text-primary flex items-center justify-center">
                    <span class="material-symbols-outlined">forum</span>
                  </div>
                  <div>
                    <p class="font-label-lg text-label-lg text-on-surface">Ready for API</p>
                    <p class="font-label-sm text-label-sm text-on-surface-variant">UI đã có chỗ để nối endpoint thật.</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

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
import { useAuth } from '../composables/useAuth'
import { notifyService, projectService, taskService, userService } from '../services/api'

const router    = useRouter()
const route     = useRoute()
const { user }  = useAuth()
const activeTab = ref('overview')
const loading   = ref(true)
const error     = ref('')
const deleting  = ref(false)

const project = ref(null)
const members = ref([])
const sprints = ref([])
const milestones = ref([])
const projectTasks = ref([])
const projectColumns = ref([])
const activities = ref([])
const activityLoading = ref(false)

const tabs = [
  { id: 'overview', label: 'Tổng quan', icon: 'dashboard' },
  { id: 'members',  label: 'Thành viên', icon: 'group' },
  { id: 'sprints',  label: 'Sprint',     icon: 'sprint' },
  { id: 'timeline', label: 'Timeline',   icon: 'timeline' },
  { id: 'kanban',   label: 'Kanban',     icon: 'view_kanban' },
  { id: 'activity', label: 'Activity',   icon: 'history' },
  { id: 'comments', label: 'Bình luận',  icon: 'forum' },
]

const AVATAR_COLORS = ['#3525cd','#006a61','#684000','#ba1a1a','#0f5e9c','#6a0dad','#2e7d32','#e65100']

const ROLE_NAMES  = ['Owner', 'Project Manager', 'Developer', 'Tester', 'Viewer']
const ROLE_STYLES = [
  'bg-primary/10 text-primary',
  'bg-secondary/10 text-secondary',
  'bg-surface-container text-on-surface-variant',
  'bg-tertiary/10 text-tertiary',
  'bg-surface-container text-outline',
]
function roleName(role)  { return ROLE_NAMES[role]  ?? 'Unknown' }
function roleStyle(role) { return ROLE_STYLES[role] ?? '' }

const currentMember = computed(() =>
  members.value.find(member => String(member.userId).toLowerCase() === String(user.value?.id || '').toLowerCase())
)
const currentProjectRole = computed(() => roleName(currentMember.value?.role))
const canDeleteProject = computed(() => currentProjectRole.value === 'Owner')

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
  { label: 'Quá hạn',   value: riskyTasks.value.length, color: 'text-secondary', icon: 'warning', iconColor: 'text-secondary' },
])

const doneTaskCount = computed(() => projectTasks.value.filter(t => t.columnType === 'done').length)
const projectProgress = computed(() => {
  if (!projectTasks.value.length) return 0
  return Math.round((doneTaskCount.value / projectTasks.value.length) * 100)
})
const riskyTasks = computed(() =>
  projectTasks.value.filter(t => t.dueDate && new Date(t.dueDate) < new Date() && t.columnType !== 'done')
)
const upcomingTasks = computed(() =>
  projectTasks.value
    .filter(t => t.dueDate && t.columnType !== 'done')
    .sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
    .slice(0, 6)
)
const activeSprint = computed(() => sprints.value.find(s => s.status === 1))
const projectHealth = computed(() => {
  if (riskyTasks.value.length > 0) return { label: 'At risk', badge: 'bg-error-container/40 text-error' }
  if (projectProgress.value >= 70) return { label: 'Healthy', badge: 'bg-secondary/10 text-secondary' }
  return { label: 'Watch', badge: 'bg-tertiary/10 text-tertiary' }
})

function formatDateTime(d) {
  if (!d) return ''
  return new Date(d).toLocaleString('vi-VN')
}

const projectWithMembers = computed(() => ({
  ...(project.value || {}),
  members: members.value.map(m => ({
    ...m,
    name:    m.displayName,
    email:   m.displayEmail,
    role:    roleName(m.role),
    joinedAt: m.joinedAt ? formatDate(m.joinedAt) : '',
  }))
}))

const projectWithSprints = computed(() => ({
  ...(project.value || {}),
  sprints: sprints.value
}))

const timelineItems = computed(() => {
  const items = []
  if (project.value?.startDate) {
    items.push({ id: 'project-start', title: 'Bắt đầu dự án', kind: 'Project', date: project.value.startDate, status: 'Start', color: 'bg-primary', badgeClass: 'bg-primary/10 text-primary' })
  }
  for (const sprint of sprints.value) {
    items.push({
      id: `sprint-${sprint.id}`,
      title: sprint.name,
      kind: 'Sprint',
      date: sprint.startDate,
      status: sprint.status === 1 ? 'Active' : sprint.status === 2 ? 'Done' : 'Planned',
      color: sprint.status === 1 ? 'bg-primary' : sprint.status === 2 ? 'bg-secondary' : 'bg-outline',
      badgeClass: sprint.status === 1 ? 'bg-primary/10 text-primary' : sprint.status === 2 ? 'bg-secondary/10 text-secondary' : 'bg-surface-container text-on-surface-variant',
    })
  }
  for (const milestone of milestones.value) {
    items.push({
      id: `milestone-${milestone.id}`,
      title: milestone.name,
      kind: 'Milestone',
      date: milestone.targetDate,
      status: milestone.status === 2 ? 'Done' : milestone.status === 1 ? 'Doing' : 'Open',
      color: 'bg-tertiary',
      badgeClass: 'bg-tertiary/10 text-tertiary',
    })
  }
  for (const task of projectTasks.value.filter(t => t.dueDate).slice(0, 20)) {
    items.push({ id: `task-${task.id}`, title: task.title, kind: 'Task deadline', date: task.dueDate, status: 'Due', color: 'bg-error', badgeClass: 'bg-error-container/40 text-error' })
  }
  if (project.value?.endDate) {
    items.push({ id: 'project-end', title: 'Kết thúc dự án', kind: 'Project', date: project.value.endDate, status: 'End', color: 'bg-secondary', badgeClass: 'bg-secondary/10 text-secondary' })
  }
  return items.sort((a, b) => new Date(a.date) - new Date(b.date))
})

const kanbanSummary = computed(() => {
  if (projectColumns.value.length) {
    return projectColumns.value.map(column => ({
      label: column.name,
      value: projectTasks.value.filter(task => task.columnId === column.id).length,
    }))
  }

  return [
    { label: 'Backlog', value: 0 },
    { label: 'To Do', value: 0 },
    { label: 'In Progress', value: 0 },
    { label: 'Review', value: 0 },
    { label: 'Testing', value: 0 },
    { label: 'Done', value: 0 },
  ]

  const done = projectTasks.value.filter(t => t.columnType === 'done').length
  const overdue = projectTasks.value.filter(t => t.dueDate && new Date(t.dueDate) < new Date() && t.columnType !== 'done').length
  return [
    { label: 'Tổng task', value: projectTasks.value.length },
    { label: 'Hoàn thành', value: done },
    { label: 'Quá hạn', value: overdue },
  ]
})

async function loadDetail() {
  const id = route.params.id
  if (!id) { error.value = 'Không tìm thấy ID dự án'; loading.value = false; return }

  loading.value = true
  error.value   = ''
  try {
    const [p, m, s, ms, allUsers] = await Promise.all([
      projectService.getById(id),
      projectService.getMembers(id).catch(() => []),
      projectService.getSprints(id).catch(() => []),
      projectService.getMilestones(id).catch(() => []),
      userService.getAll().catch(() => []),
    ])

    // Build a lookup map: userId → user info
    const userMap = Object.fromEntries((allUsers || []).map(u => [u.id, u]))

    project.value = p
    sprints.value = s || []
    milestones.value = ms || []
    members.value = (m || []).map((mem, i) => {
      const u = userMap[mem.userId]
      return {
        ...mem,
        displayName:  u?.fullName  || mem.userId,
        displayEmail: u?.email     || '',
        avatarColor:  AVATAR_COLORS[i % AVATAR_COLORS.length],
      }
    })
    await loadProjectTaskSummary(id)
    await loadActivity(id)
  } catch (e) {
    if (e.response?.status === 404) error.value = 'Dự án không tồn tại.'
    else error.value = 'Không thể tải thông tin dự án. Kiểm tra ProjectService đang chạy.'
  } finally {
    loading.value = false
  }
}

async function loadProjectTaskSummary(projectId) {
  try {
    const [tasks, columns] = await Promise.all([
      taskService.getAll({ projectId }).catch(() => []),
      taskService.getColumns(projectId).catch(() => []),
    ])
    const columnMap = Object.fromEntries((columns || []).map(c => [c.id, c]))
    projectColumns.value = columns || []
    projectTasks.value = (tasks || []).map(task => ({
      ...task,
      columnType: columnMap[task.columnId]?.type || 'active',
    }))
  } catch {
    projectColumns.value = []
    projectTasks.value = []
  }
}

async function loadActivity(projectId) {
  activityLoading.value = true
  try {
    activities.value = await notifyService.getProjectActivity(projectId) || []
  } catch {
    activities.value = []
  } finally {
    activityLoading.value = false
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
  if (!canDeleteProject.value) return
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
