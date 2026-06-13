<template>
  <div class="page-wrap">
    <section class="gradient-panel text-on-surface shadow-xl">
      <div class="relative grid grid-cols-1 gap-lg p-lg lg:grid-cols-[1.35fr_0.65fr] lg:p-xl">
        <div>
          <p class="font-label-md text-label-md uppercase tracking-wider text-primary">Workspace overview</p>
          <h2 class="mt-2 text-[34px] font-black leading-[42px] md:text-[44px] md:leading-[52px]">
            Quản lý dự án & phân công công việc
          </h2>
          <p class="mt-sm max-w-2xl font-body-lg text-body-lg text-on-surface-variant">
            Dashboard tổng hợp dữ liệu thật từ ProjectService, TaskService và NotifyService để demo luồng nghiệp vụ hoàn chỉnh.
          </p>
          <div class="mt-lg flex flex-wrap gap-sm">
            <RouterLink to="/projects" class="app-button-primary">
              <span class="material-symbols-outlined text-[18px]">add</span>
              Tạo dự án
            </RouterLink>
            <RouterLink to="/kanban" class="inline-flex items-center justify-center gap-xs rounded-lg border border-outline-variant px-md py-sm font-label-lg text-label-lg text-on-surface transition-all hover:bg-surface-container-high active:scale-95">
              <span class="material-symbols-outlined text-[18px]">view_kanban</span>
              Mở Kanban
            </RouterLink>
          </div>
        </div>

        <div class="glass-card rounded-2xl p-md">
          <div class="flex items-center justify-between">
            <span class="font-label-md text-label-md text-on-surface-variant">Tiến độ workspace</span>
            <span class="font-headline-sm text-headline-sm text-primary">{{ overallCompletion }}%</span>
          </div>
          <div class="mt-sm h-3 overflow-hidden rounded-full bg-surface-container">
            <div class="h-full rounded-full bg-primary transition-all" :style="{ width: `${overallCompletion}%` }"></div>
          </div>
          <div class="mt-lg grid grid-cols-3 gap-sm">
            <div class="metric-tile">
              <p class="font-label-sm text-label-sm text-on-surface-variant">Projects</p>
              <p class="font-headline-sm text-headline-sm">{{ stats.totalProjects }}</p>
            </div>
            <div class="metric-tile">
              <p class="font-label-sm text-label-sm text-on-surface-variant">Tasks</p>
              <p class="font-headline-sm text-headline-sm">{{ stats.totalTasks }}</p>
            </div>
            <div class="metric-tile">
              <p class="font-label-sm text-label-sm text-on-surface-variant">Done</p>
              <p class="font-headline-sm text-headline-sm">{{ stats.doneTasks }}</p>
            </div>
          </div>
          <div class="mt-md flex items-center gap-xs text-on-surface-variant">
            <span class="material-symbols-outlined text-[18px]">calendar_today</span>
            <span class="font-label-md text-label-md">{{ today }}</span>
          </div>
        </div>
      </div>
    </section>

    <div v-if="serviceWarnings.length" class="grid grid-cols-1 md:grid-cols-3 gap-sm">
      <div v-for="warning in serviceWarnings" :key="warning"
        class="rounded-xl border border-error/20 bg-error-container/20 px-md py-sm text-error font-label-md text-label-md flex items-center gap-2">
        <span class="material-symbols-outlined text-[18px]">warning</span>
        {{ warning }}
      </div>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-6 gap-md">
      <RouterLink to="/projects" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="folder_copy" label="Tổng dự án" :value="loading ? '...' : stats.totalProjects"
          :badge="`${stats.activeProjects} active`" badgeColor="text-secondary bg-secondary-container/30"
          iconBg="bg-primary/10 text-primary" />
      </RouterLink>
      <RouterLink to="/members" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="group" label="Thành viên" :value="loading ? '...' : stats.totalMembers"
          badge="Members" badgeColor="text-secondary bg-secondary-container/30"
          iconBg="bg-secondary/10 text-secondary" />
      </RouterLink>
      <RouterLink to="/tasks" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="task_alt" label="Tổng task" :value="loading ? '...' : stats.totalTasks"
          badge="All" badgeColor="text-primary bg-primary/10"
          iconBg="bg-primary/10 text-primary" />
      </RouterLink>
      <RouterLink :to="{ path: '/tasks', query: { status: 'done' } }" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="check_circle" label="Hoàn thành" :value="loading ? '...' : stats.doneTasks"
          badge="Done" badgeColor="text-secondary bg-secondary-container/30"
          iconBg="bg-secondary/10 text-secondary" />
      </RouterLink>
      <RouterLink :to="{ path: '/tasks', query: { status: 'active' } }" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="sync" label="Đang thực hiện" :value="loading ? '...' : stats.inProgressTasks"
          badge="Active" badgeColor="text-primary bg-primary/10"
          iconBg="bg-primary/10 text-primary" />
      </RouterLink>
      <RouterLink to="/notifications" class="block rounded-xl focus:outline-none focus:ring-2 focus:ring-primary">
        <StatCard icon="notifications" label="Thông báo mới" :value="notifLoading ? '...' : stats.unreadNotifs"
          badge="Unread" badgeColor="text-tertiary bg-tertiary/10"
          iconBg="bg-tertiary/10 text-tertiary" />
      </RouterLink>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-lg">
      <section class="workspace-card lg:col-span-2">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="font-headline-sm text-headline-sm text-on-surface">Project health</h3>
            <p class="font-body-sm text-body-sm text-on-surface-variant">Tổng quan sức khỏe dự án theo tiến độ và task rủi ro.</p>
          </div>
          <RouterLink to="/projects" class="font-label-md text-label-md text-primary hover:underline">Portfolio</RouterLink>
        </div>
        <div class="mt-md grid grid-cols-1 md:grid-cols-3 gap-sm">
          <div v-for="item in healthCards" :key="item.label" class="metric-tile">
            <div class="flex items-center justify-between">
              <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ item.label }}</span>
              <span class="material-symbols-outlined text-[20px]" :class="item.color">{{ item.icon }}</span>
            </div>
            <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ item.value }}</p>
            <p class="font-label-sm text-label-sm text-on-surface-variant">{{ item.caption }}</p>
          </div>
        </div>
      </section>

      <section class="workspace-card">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="font-headline-sm text-headline-sm text-on-surface">Sprint progress</h3>
            <p class="font-body-sm text-body-sm text-on-surface-variant">Sprint đang hoạt động.</p>
          </div>
          <span class="material-symbols-outlined text-primary">sprint</span>
        </div>
        <div class="mt-md flex flex-col gap-sm">
          <div class="rounded-xl bg-surface-container-low p-sm">
            <div class="flex items-center justify-between">
              <span class="font-label-md text-label-md text-on-surface-variant">Active sprint</span>
              <span class="font-headline-sm text-headline-sm text-primary">{{ activeSprintCount }}</span>
            </div>
            <div class="mt-2 h-2 rounded-full bg-surface-container overflow-hidden">
              <div class="h-full rounded-full bg-primary" :style="{ width: `${sprintCoverage}%` }"></div>
            </div>
          </div>
          <p class="font-label-sm text-label-sm text-on-surface-variant">
            {{ sprintCoverage }}% dự án có sprint đang chạy.
          </p>
        </div>
      </section>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-[1.35fr_0.65fr] gap-lg">
      <section class="app-panel">
        <div class="flex items-center justify-between border-b border-outline-variant px-md py-sm">
          <div>
            <h3 class="font-headline-sm text-headline-sm text-on-surface">Recent projects</h3>
            <p class="font-body-sm text-body-sm text-on-surface-variant">Các dự án đang có task và tiến độ.</p>
          </div>
          <RouterLink to="/projects" class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</RouterLink>
        </div>
        <div v-if="loading" class="p-lg text-on-surface-variant flex items-center gap-2">
          <span class="material-symbols-outlined animate-spin">progress_activity</span>
          Đang tải...
        </div>
        <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-md p-md">
          <RouterLink v-for="item in recentProjects" :key="item.id" :to="`/projects/${item.id}`"
            class="rounded-xl border border-outline-variant bg-surface-container-low p-md transition-all hover:-translate-y-0.5 hover:border-primary hover:shadow-md">
            <div class="flex items-start gap-sm">
              <div class="w-11 h-11 rounded-xl text-white font-bold flex items-center justify-center" :style="{ backgroundColor: item.color }">
                {{ item.initials }}
              </div>
              <div class="min-w-0 flex-1">
                <p class="font-label-lg text-label-lg text-on-surface truncate">{{ item.name }}</p>
                <p class="font-label-sm text-label-sm text-on-surface-variant">{{ item.done }} / {{ item.total }} task hoàn thành</p>
              </div>
              <span class="font-label-md text-label-md text-primary">{{ item.percent }}%</span>
            </div>
            <div class="mt-md h-2 overflow-hidden rounded-full bg-surface-container">
              <div class="h-full rounded-full bg-primary" :style="{ width: `${item.percent}%` }"></div>
            </div>
          </RouterLink>
        </div>
      </section>

      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm">
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Task distribution</h3>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Theo trạng thái Kanban.</p>
        </div>
        <div class="p-md flex flex-col gap-md">
          <div v-for="slice in statusChart" :key="slice.label" class="rounded-lg bg-surface-container-low p-sm">
            <div class="flex items-center justify-between">
              <span class="font-label-md text-label-md text-on-surface flex items-center gap-2">
                <span class="w-2.5 h-2.5 rounded-full" :class="slice.dot"></span>
                {{ slice.label }}
              </span>
              <span class="font-label-md text-label-md text-on-surface-variant">{{ slice.value }}</span>
            </div>
            <div class="mt-2 h-2 rounded-full bg-surface-container overflow-hidden">
              <div class="h-full rounded-full" :class="slice.bar" :style="{ width: `${slice.percent}%` }"></div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-lg">
      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm">
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Workload</h3>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Task theo thành viên.</p>
        </div>
        <div class="p-md flex flex-col gap-sm">
          <div v-if="memberWorkload.length === 0" class="text-on-surface-variant font-body-md text-body-md">
            Chưa có task được giao.
          </div>
          <RouterLink v-for="member in memberWorkload" :key="member.id" to="/members"
            class="flex items-center gap-3 rounded-lg p-sm hover:bg-surface-container-low transition-colors">
            <div class="w-9 h-9 rounded-full text-white font-bold text-[12px] flex items-center justify-center"
              :style="{ backgroundColor: member.color }">{{ member.initials }}</div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between">
                <span class="font-label-lg text-label-lg text-on-surface truncate">{{ member.name }}</span>
                <span class="font-label-md text-label-md text-on-surface-variant">{{ member.count }}</span>
              </div>
              <div class="h-1.5 rounded-full bg-surface-container overflow-hidden mt-1">
                <div class="h-full rounded-full bg-secondary" :style="{ width: `${member.percent}%` }"></div>
              </div>
            </div>
          </RouterLink>
        </div>
      </section>

      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm">
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Upcoming deadlines</h3>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Task gần hạn hoặc quá hạn.</p>
        </div>
        <div class="p-md flex flex-col gap-sm">
          <div v-if="riskTasks.length === 0" class="font-body-md text-body-md text-on-surface-variant">
            Không có task rủi ro.
          </div>
          <button v-for="task in riskTasks" :key="task.id"
            @click="$router.push({ path: '/kanban', query: { projectId: task.projectId } })"
            class="text-left rounded-xl border border-outline-variant bg-surface-container-low p-sm hover:border-primary hover:shadow-sm transition-all">
            <p class="font-label-lg text-label-lg text-on-surface line-clamp-1">{{ task.title }}</p>
            <p class="font-label-sm text-label-sm" :class="task.overdue ? 'text-error' : 'text-tertiary'">
              {{ task.overdue ? 'Quá hạn' : 'Gần hạn' }} · {{ formatDate(task.dueDate) }}
            </p>
          </button>
        </div>
      </section>

      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm flex items-center justify-between">
          <div>
            <h3 class="font-headline-sm text-headline-sm text-on-surface">Notifications</h3>
            <p class="font-body-sm text-body-sm text-on-surface-variant">Thông báo mới nhất.</p>
          </div>
          <RouterLink to="/notifications" class="font-label-md text-label-md text-primary hover:underline">Mở</RouterLink>
        </div>
        <div class="p-md flex flex-col gap-sm">
          <div v-if="notifications.length === 0" class="font-body-md text-body-md text-on-surface-variant">
            Không có thông báo mới.
          </div>
          <RouterLink v-for="n in notifications.slice(0, 5)" :key="n.id" to="/notifications"
            class="rounded-xl border border-outline-variant bg-surface-container-low p-sm flex items-start gap-2 hover:border-primary transition-colors">
            <span class="material-symbols-outlined text-primary text-[18px] mt-0.5">{{ notifIcon(n.type) }}</span>
            <div class="min-w-0">
              <p class="font-label-lg text-label-lg text-on-surface line-clamp-1">{{ n.title || n.message || n.content }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant">{{ relativeTime(n.createdAt) }}</p>
            </div>
          </RouterLink>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import StatCard from '../components/StatCard.vue'
import { notifyService, projectService, taskService, userService } from '../services/api'

const today = new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' })
const loading = ref(true)
const notifLoading = ref(true)
const serviceWarnings = ref([])
const projects = ref([])
const users = ref([])
const tasks = ref([])
const columnsByProject = ref({})
const sprintsByProject = ref({})
const notifications = ref([])
const unreadCount = ref(0)

const colors = ['#3525cd', '#006a61', '#684000', '#ba1a1a', '#0f5e9c', '#6a0dad', '#2e7d32', '#e65100']

const stats = computed(() => {
  const doneColumnIds = doneColumns.value
  const liveTasks = tasks.value.filter(t => !t.deletedAt)
  return {
    totalProjects: projects.value.length,
    activeProjects: projects.value.filter(p => p.status === 1).length,
    totalMembers: users.value.length,
    totalTasks: liveTasks.length,
    doneTasks: liveTasks.filter(t => doneColumnIds.has(t.columnId)).length,
    inProgressTasks: liveTasks.filter(t => !doneColumnIds.has(t.columnId)).length,
    unreadNotifs: unreadCount.value,
  }
})

const doneColumns = computed(() => {
  const ids = new Set()
  for (const cols of Object.values(columnsByProject.value)) {
    for (const col of cols || []) {
      if (col.type === 'done' || /done|hoàn thành/i.test(col.name || '')) ids.add(col.id)
    }
  }
  return ids
})

const projectProgress = computed(() => {
  return projects.value.map((p, index) => {
    const projectTasks = tasks.value.filter(t => t.projectId === p.id && !t.deletedAt)
    const done = projectTasks.filter(t => doneColumns.value.has(t.columnId)).length
    const total = projectTasks.length
    return {
      id: p.id,
      name: p.name,
      color: p.color || colors[index % colors.length],
      initials: initials(p.name),
      done,
      total,
      percent: total ? Math.round((done / total) * 100) : 0,
    }
  }).sort((a, b) => b.percent - a.percent)
})

const recentProjects = computed(() => projectProgress.value.slice(0, 4))

const overallCompletion = computed(() => {
  const total = stats.value.totalTasks
  return total ? Math.round((stats.value.doneTasks / total) * 100) : 0
})

const activeSprintCount = computed(() => {
  return Object.values(sprintsByProject.value)
    .flatMap(list => list || [])
    .filter(sprint => sprint.status === 1).length
})

const sprintCoverage = computed(() => {
  const total = projects.value.length
  if (!total) return 0
  const withActive = projects.value.filter(project =>
    (sprintsByProject.value[project.id] || []).some(sprint => sprint.status === 1)
  ).length
  return Math.round((withActive / total) * 100)
})

const healthCards = computed(() => [
  {
    label: 'Healthy',
    value: projectProgress.value.filter(project => project.percent >= 60).length,
    caption: 'Dự án tiến độ tốt',
    icon: 'verified',
    color: 'text-secondary',
  },
  {
    label: 'Watch',
    value: projectProgress.value.filter(project => project.percent > 0 && project.percent < 60).length,
    caption: 'Cần theo dõi',
    icon: 'visibility',
    color: 'text-tertiary',
  },
  {
    label: 'Risk',
    value: riskTasks.value.length,
    caption: 'Task gần hạn/quá hạn',
    icon: 'warning',
    color: 'text-error',
  },
])

const statusChart = computed(() => {
  const buckets = [
    { label: 'Backlog', match: col => col.type === 'backlog' || /backlog/i.test(col.name), value: 0, dot: 'bg-outline', bar: 'bg-outline' },
    { label: 'To Do', match: col => /to do|todo/i.test(col.name), value: 0, dot: 'bg-primary', bar: 'bg-primary' },
    { label: 'In Progress', match: col => /progress|doing/i.test(col.name), value: 0, dot: 'bg-tertiary', bar: 'bg-tertiary' },
    { label: 'Review / Testing', match: col => /review|test/i.test(col.name), value: 0, dot: 'bg-secondary', bar: 'bg-secondary' },
    { label: 'Done', match: col => col.type === 'done' || /done|hoàn thành/i.test(col.name), value: 0, dot: 'bg-secondary', bar: 'bg-secondary' },
  ]
  const colMap = new Map()
  for (const cols of Object.values(columnsByProject.value)) {
    for (const col of cols || []) colMap.set(col.id, col)
  }
  for (const task of tasks.value.filter(t => !t.deletedAt)) {
    const col = colMap.get(task.columnId)
    const bucket = buckets.find(b => col && b.match(col)) || buckets[1]
    bucket.value += 1
  }
  const max = Math.max(1, ...buckets.map(b => b.value))
  return buckets.map(b => ({ ...b, percent: Math.round((b.value / max) * 100) }))
})

const memberWorkload = computed(() => {
  const counts = new Map()
  for (const task of tasks.value.filter(t => !t.deletedAt && t.assignedTo)) {
    counts.set(task.assignedTo, (counts.get(task.assignedTo) || 0) + 1)
  }
  const max = Math.max(1, ...counts.values())
  return [...counts.entries()]
    .map(([id, count], index) => {
      const user = users.value.find(u => u.id === id)
      const name = user?.fullName || user?.email || id
      return {
        id,
        count,
        name,
        initials: initials(name).slice(0, 2),
        color: colors[index % colors.length],
        percent: Math.round((count / max) * 100),
      }
    })
    .sort((a, b) => b.count - a.count)
    .slice(0, 6)
})

const riskTasks = computed(() => {
  const now = Date.now()
  const threeDays = 3 * 86400000
  return tasks.value
    .filter(t => t.dueDate && !doneColumns.value.has(t.columnId))
    .map(t => {
      const due = new Date(t.dueDate).getTime()
      return { ...t, overdue: due < now, soon: due >= now && due - now <= threeDays }
    })
    .filter(t => t.overdue || t.soon)
    .sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
    .slice(0, 6)
})

function initials(name) {
  return (name || '').split(' ').filter(Boolean).map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'PR'
}

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('vi-VN')
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

function notifIcon(type) {
  const map = {
    task_assigned: 'person_add',
    task_column_changed: 'swap_horiz',
    member_added: 'group_add',
    comment_mention: 'alternate_email',
  }
  return map[type] || 'notifications'
}

async function loadDashboard() {
  loading.value = true
  serviceWarnings.value = []
  try {
    projects.value = await projectService.getAll() || []
  } catch {
    serviceWarnings.value.push('ProjectService chưa sẵn sàng')
  }

  try {
    users.value = await userService.getAll() || []
  } catch {
    serviceWarnings.value.push('NotifyService users chưa sẵn sàng')
  }

  try {
    const projectIds = projects.value.map(p => p.id)
    const [taskResults, columnResults, sprintResults] = await Promise.all([
      Promise.allSettled(projectIds.map(id => taskService.getAll({ projectId: id }))),
      Promise.allSettled(projectIds.map(id => taskService.getColumns(id))),
      Promise.allSettled(projectIds.map(id => projectService.getSprints(id))),
    ])
    tasks.value = taskResults.flatMap(r => r.status === 'fulfilled' ? (r.value || []) : [])
    columnsByProject.value = Object.fromEntries(projectIds.map((id, index) => [
      id,
      columnResults[index]?.status === 'fulfilled' ? (columnResults[index].value || []) : [],
    ]))
    sprintsByProject.value = Object.fromEntries(projectIds.map((id, index) => [
      id,
      sprintResults[index]?.status === 'fulfilled' ? (sprintResults[index].value || []) : [],
    ]))
  } catch {
    serviceWarnings.value.push('TaskService chưa sẵn sàng')
  } finally {
    loading.value = false
  }

  notifLoading.value = true
  try {
    const [notifs, count] = await Promise.all([
      notifyService.getAll().catch(() => []),
      notifyService.unreadCount().catch(() => 0),
    ])
    notifications.value = notifs || []
    unreadCount.value = count || 0
  } catch {
    serviceWarnings.value.push('Notification API chưa sẵn sàng')
  } finally {
    notifLoading.value = false
  }
}

onMounted(loadDashboard)
</script>
