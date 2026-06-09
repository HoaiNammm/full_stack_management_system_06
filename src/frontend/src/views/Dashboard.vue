<template>
  <div class="flex-grow pt-24 px-lg pb-xl max-w-7xl mx-auto w-full flex flex-col gap-lg">
    <!-- Page Title -->
    <div class="flex justify-between items-end mb-sm">
      <div>
        <h2 class="font-headline-lg text-headline-lg text-on-surface">Tổng quan hệ thống</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Theo dõi tiến độ và hiệu suất các dự án hiện tại.</p>
      </div>
      <div class="flex items-center gap-sm text-on-surface-variant">
        <span class="material-symbols-outlined">calendar_today</span>
        <span class="font-label-md text-label-md">{{ today }}</span>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-md">
      <StatCard icon="folder_copy" label="Tổng số dự án"
        :value="loading ? '...' : stats.totalProjects"
        :badge="loading ? '...' : `${stats.activeProjects} đang hoạt động`" badgeColor="text-secondary bg-secondary-container/30"
        iconBg="bg-primary/10 text-primary" />
      <StatCard icon="engineering" label="Task chưa hoàn thành"
        :value="loading ? '...' : stats.activeTasks"
        badge="Trên tất cả dự án" badgeColor="text-on-surface-variant"
        iconBg="bg-secondary/10 text-secondary" />
      <StatCard icon="warning" label="Task quá hạn"
        :value="loading ? '...' : stats.overdueTasks"
        badge="Xem chi tiết" badgeColor="text-on-surface-variant"
        iconBg="bg-error-container text-error" valueColor="text-error" borderClass="border-error/20" />
      <StatCard icon="mail" label="Thông báo mới"
        :value="loading ? '...' : stats.unreadNotifs"
        badge="Chưa đọc" badgeColor="text-on-surface-variant"
        iconBg="bg-tertiary/10 text-tertiary" />
    </div>

    <!-- Sprint Hero -->
    <div v-if="activeSprint" class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
      <div class="p-md border-b border-outline-variant bg-surface-container-low/50 flex justify-between items-center">
        <div class="flex items-center gap-sm">
          <span class="material-symbols-outlined text-primary" style="font-variation-settings: 'FILL' 1">sprint</span>
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Sprint Hiện Tại: {{ activeSprint.name }}</h3>
        </div>
        <div class="bg-surface-container-highest px-sm py-1 rounded-full flex items-center gap-xs">
          <span class="material-symbols-outlined text-[16px] text-on-surface-variant">schedule</span>
          <span class="font-label-md text-label-md text-on-surface-variant">{{ sprintDaysLeft }}</span>
        </div>
      </div>
      <div class="p-md flex flex-col gap-md">
        <div class="flex justify-between items-end">
          <div class="w-2/3">
            <p class="font-label-lg text-label-lg text-on-surface mb-1">Mục tiêu Sprint</p>
            <p class="font-body-md text-body-md text-on-surface-variant">{{ activeSprint.goal || activeSprint.description || 'Không có mục tiêu cụ thể.' }}</p>
          </div>
          <div class="text-right">
            <span class="font-headline-md text-headline-md text-primary">{{ sprintProject?.name }}</span>
            <p class="font-label-sm text-label-sm text-on-surface-variant">{{ formatDate(activeSprint.startDate) }} → {{ formatDate(activeSprint.endDate) }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Error -->
    <div v-if="error" class="bg-error-container/20 border border-error/30 rounded-xl p-md text-error font-label-md text-label-md flex items-center gap-2">
      <span class="material-symbols-outlined">warning</span>
      {{ error }}
    </div>

    <!-- Bottom Row -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-lg flex-grow">
      <!-- Projects List -->
      <div class="lg:col-span-2 flex flex-col bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden">
        <div class="p-md border-b border-outline-variant flex justify-between items-center">
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Dự án hoạt động gần đây</h3>
          <button @click="$router.push('/projects')" class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</button>
        </div>
        <div v-if="loading" class="p-lg flex items-center justify-center text-on-surface-variant">
          <span class="material-symbols-outlined animate-spin mr-2">progress_activity</span>Đang tải...
        </div>
        <div v-else-if="projects.length === 0" class="p-lg text-center text-on-surface-variant font-body-md text-body-md">
          Chưa có dự án nào. <button @click="$router.push('/projects')" class="text-primary hover:underline">Tạo dự án mới</button>
        </div>
        <div v-else class="flex flex-col">
          <ProjectRow v-for="p in projects.slice(0, 5)" :key="p.id" :project="mapProject(p)" />
        </div>
      </div>

      <!-- Notifications Feed -->
      <div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
        <div class="p-md border-b border-outline-variant flex justify-between items-center bg-surface-container-low/30">
          <h3 class="font-headline-sm text-headline-sm text-on-surface flex items-center gap-2">
            <span class="material-symbols-outlined text-[20px] text-on-surface-variant">notifications</span>
            Thông báo gần đây
          </h3>
        </div>
        <div v-if="notifLoading" class="p-lg flex items-center justify-center text-on-surface-variant">
          <span class="material-symbols-outlined animate-spin mr-2">progress_activity</span>
        </div>
        <div v-else-if="notifications.length === 0" class="p-lg text-center font-body-md text-body-md text-on-surface-variant">
          Không có thông báo mới
        </div>
        <div v-else class="p-md flex flex-col gap-md overflow-y-auto max-h-[400px]">
          <div v-for="n in notifications.slice(0, 10)" :key="n.id"
            class="flex items-start gap-3 pb-3 border-b border-outline-variant/40 last:border-0">
            <div class="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0 mt-0.5">
              <span class="material-symbols-outlined text-primary text-[16px]">{{ notifIcon(n.type) }}</span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-body-sm text-body-sm text-on-surface leading-snug">{{ n.message }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant mt-0.5">{{ relativeTime(n.createdAt) }}</p>
            </div>
            <div v-if="!n.isRead" class="w-2 h-2 bg-primary rounded-full flex-shrink-0 mt-2"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import StatCard   from '../components/StatCard.vue'
import ProjectRow from '../components/ProjectRow.vue'
import { projectService, notifyService, taskService } from '../services/api'

const today = new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' })

const loading      = ref(true)
const notifLoading = ref(true)
const error        = ref('')
const projects     = ref([])
const notifications = ref([])
const stats = ref({ totalProjects: 0, activeProjects: 0, activeTasks: 0, overdueTasks: 0, unreadNotifs: 0 })
const activeSprint    = ref(null)
const sprintProject   = ref(null)

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('vi-VN')
}

const sprintDaysLeft = computed(() => {
  if (!activeSprint.value?.endDate) return ''
  const diff = Math.ceil((new Date(activeSprint.value.endDate) - Date.now()) / 86400000)
  return diff > 0 ? `Còn ${diff} ngày` : diff === 0 ? 'Hết hôm nay' : `Quá hạn ${-diff} ngày`
})

function mapProject(p) {
  const initials = (p.name || '').split(' ').map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'PR'
  return {
    id: p.id,
    initials,
    color: p.color ? `bg-[${p.color}]/10 border-[${p.color}]/20 text-[${p.color}]` : 'bg-primary/10 border-primary/20 text-primary',
    name: p.name,
    desc: p.description || '',
    tasks: p.taskCount ?? 0,
    statusText: p.status === 1 ? 'Active' : 'Draft',
    statusColor: p.status === 1 ? 'text-secondary' : 'text-on-surface-variant',
    avatars: [],
    extraCount: 0,
  }
}

function notifIcon(type) {
  const map = {
    task_assigned: 'person_add',
    task_column_changed: 'swap_horiz',
    member_added: 'group_add',
    comment_mention: 'alternate_email',
  }
  return map[type] || 'notifications'
}

function relativeTime(dateStr) {
  if (!dateStr) return ''
  const diff = Date.now() - new Date(dateStr)
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'Vừa xong'
  if (mins < 60) return `${mins} phút trước`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs} giờ trước`
  return `${Math.floor(hrs / 24)} ngày trước`
}

onMounted(async () => {
  // Load projects
  try {
    const data = await projectService.getAll()
    const list = data || []

    // Fetch sprints for all projects in parallel, derive effective status
    const sprintResults = await Promise.allSettled(list.map(p => projectService.getSprints(p.id)))
    projects.value = list.map((p, i) => {
      const r = sprintResults[i]
      const allSprints = r.status === 'fulfilled' ? (r.value || []) : []
      const activeSprintItem = allSprints.find(s => s.status === 1) || null
      return { ...p, status: activeSprintItem ? 1 : p.status, _activeSprint: activeSprintItem }
    })

    stats.value.totalProjects  = projects.value.length
    stats.value.activeProjects = projects.value.filter(p => p.status === 1).length

    // Find first active sprint across projects
    const firstWithSprint = projects.value.find(p => p._activeSprint)
    if (firstWithSprint) {
      activeSprint.value  = firstWithSprint._activeSprint
      sprintProject.value = firstWithSprint
    }
  } catch (e) {
    error.value = 'Không thể tải dự án. Kiểm tra ProjectService đang chạy.'
  } finally {
    loading.value = false
  }

  // Load tasks scoped to user's projects (avoid global getAll which returns all DB tasks)
  try {
    const projectIds = projects.value.map(p => p.id)
    if (projectIds.length > 0) {
      const [taskResults, colResults] = await Promise.all([
        Promise.allSettled(projectIds.map(pid => taskService.getAll({ projectId: pid }))),
        Promise.allSettled(projectIds.map(pid => taskService.getColumns(pid))),
      ])

      // Collect IDs of "done" columns to exclude from "pending" count
      const doneColumnIds = new Set()
      for (const r of colResults)
        if (r.status === 'fulfilled')
          for (const col of r.value || [])
            if (col.type === 'done') doneColumnIds.add(col.id)

      const allTasks  = taskResults.flatMap(r => r.status === 'fulfilled' ? (r.value || []) : [])
      const liveTasks = allTasks.filter(t => !t.deletedAt)
      const now       = Date.now()

      // Count tasks not yet done (backlog + todo + in-progress)
      stats.value.activeTasks  = liveTasks.filter(t => !doneColumnIds.has(t.columnId)).length
      stats.value.overdueTasks = liveTasks.filter(t =>
        t.dueDate && new Date(t.dueDate).getTime() < now
      ).length

      // Per-project count: use active sprint scope to match Kanban default view
      const countMap = {}
      for (const t of liveTasks) {
        if (!t.projectId) continue
        const proj        = projects.value.find(p => p.id === t.projectId)
        const activeSpId  = proj?._activeSprint?.id
        // If project has active sprint, only count tasks in that sprint
        if (activeSpId && t.sprintId !== activeSpId) continue
        countMap[t.projectId] = (countMap[t.projectId] || 0) + 1
      }
      projects.value = projects.value.map(p => ({ ...p, taskCount: countMap[p.id] ?? 0 }))
    }
  } catch { /* TaskService may be offline */ }

  // Load notifications
  try {
    const [notifs, count] = await Promise.all([
      notifyService.getAll(),
      notifyService.unreadCount(),
    ])
    notifications.value      = notifs || []
    stats.value.unreadNotifs = count
  } catch { /* NotifyService may be offline */ }
  finally {
    notifLoading.value = false
  }
})
</script>
