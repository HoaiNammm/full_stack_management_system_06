<template>
  <div class="page-wrap">
    <section class="dashboard-shell">
      <header class="dashboard-header">
        <div>
          <p class="dashboard-eyebrow">Bảng điều khiển</p>
          <h1>Quản lý dự án & công việc</h1>
          <span>Theo dõi tiến độ, trạng thái task và deadline từ dữ liệu API thật.</span>
        </div>
        <div class="dashboard-actions">
          <RouterLink to="/projects">
            <span class="material-symbols-outlined">add</span>
            Tạo dự án
          </RouterLink>
          <RouterLink to="/kanban">
            <span class="material-symbols-outlined">view_kanban</span>
            Mở Kanban
          </RouterLink>
        </div>
      </header>

      <div v-if="serviceWarnings.length" class="service-warning-grid">
        <div v-for="warning in serviceWarnings" :key="warning" class="service-warning">
          <span class="material-symbols-outlined">warning</span>
          {{ warning }}
        </div>
      </div>

      <div class="kpi-grid">
        <RouterLink v-for="metric in metricCards" :key="metric.label" :to="metric.to" class="kpi-card">
          <div class="kpi-head">
            <span>{{ metric.label }}</span>
            <div class="kpi-icon" :class="metric.tone">
              <span class="material-symbols-outlined">{{ metric.icon }}</span>
            </div>
          </div>
          <strong>{{ loading ? '...' : metric.value }}</strong>
          <p>{{ metric.caption }}</p>
          <div class="kpi-line">
            <i :style="{ width: `${metric.percent}%` }"></i>
          </div>
        </RouterLink>
      </div>

      <div class="dashboard-main-grid">
        <section class="panel chart-panel">
          <div class="panel-head">
            <div>
              <h2>Phân bố công việc</h2>
              <p>Biểu đồ task theo trạng thái Kanban.</p>
            </div>
            <RouterLink to="/tasks">Xem task</RouterLink>
          </div>

          <div class="chart-area">
            <div v-for="item in statusChart" :key="item.label" class="chart-column">
              <div class="chart-bar-wrap">
                <div class="chart-bar" :style="{ height: `${Math.max(10, item.percent)}%` }"></div>
              </div>
              <span>{{ translateColumn(item.label) }}</span>
              <b>{{ item.value }}</b>
            </div>
          </div>
        </section>

        <section class="panel progress-panel">
          <div class="panel-head">
            <div>
              <h2>Tiến độ tổng quan</h2>
              <p>Tính theo task ở cột hoàn tất.</p>
            </div>
            <span class="material-symbols-outlined panel-symbol">donut_large</span>
          </div>

          <div class="progress-compact">
            <div class="progress-donut" :style="{ '--p': `${overallCompletion * 3.6}deg` }">
              <div>
                <strong>{{ overallCompletion }}</strong>
                <span>%</span>
              </div>
            </div>

            <div class="progress-summary">
              <div>
                <p>Đã hoàn thành</p>
                <strong>{{ stats.doneTasks }}</strong>
              </div>
              <div>
                <p>Đang xử lý</p>
                <strong>{{ stats.inProgressTasks }}</strong>
              </div>
              <div>
                <p>Tổng task</p>
                <strong>{{ stats.totalTasks }}</strong>
              </div>
              <div>
                <p>Sprint đang chạy</p>
                <strong>{{ activeSprintCount }}</strong>
              </div>
            </div>
          </div>
        </section>
      </div>

      <div class="dashboard-secondary-grid">
        <section class="panel deadline-panel">
          <div class="panel-head">
            <div>
              <h2>Deadline sắp tới</h2>
              <p>Task gần hạn hoặc quá hạn.</p>
            </div>
            <RouterLink to="/calendar">Mở lịch</RouterLink>
          </div>
          <div class="stack-list">
            <button v-for="task in riskTasks" :key="task.id"
              @click="$router.push({ path: '/kanban', query: { projectId: task.projectId } })"
              class="deadline-row">
              <span class="material-symbols-outlined" :class="task.overdue ? 'is-danger' : 'is-warning'">event_upcoming</span>
              <div>
                <strong>{{ displayText(task.title) }}</strong>
                <small>
                  <b>{{ task.overdue ? 'Quá hạn' : 'Gần hạn' }}</b>
                  <i></i>
                  {{ formatDate(task.dueDate) }}
                </small>
              </div>
            </button>
            <div v-if="riskTasks.length === 0" class="empty-state">Không có deadline rủi ro.</div>
          </div>
        </section>

        <section class="panel">
          <div class="panel-head">
            <div>
              <h2>Thành viên đang bận</h2>
              <p>Workload theo người được giao.</p>
            </div>
            <RouterLink to="/members">Xem đội ngũ</RouterLink>
          </div>
          <div class="stack-list">
            <RouterLink v-for="member in memberWorkload" :key="member.id" to="/members" class="member-row">
              <div class="avatar" :style="{ backgroundColor: member.color }">{{ member.initials }}</div>
              <div class="row-main">
                <div class="row-title">
                  <strong>{{ member.name }}</strong>
                  <span>{{ member.count }} task</span>
                </div>
                <div class="line-progress">
                  <i :style="{ width: `${member.percent}%` }"></i>
                </div>
              </div>
            </RouterLink>
            <div v-if="memberWorkload.length === 0" class="empty-state">Chưa có task được giao.</div>
          </div>
        </section>

        <section class="panel">
          <div class="panel-head">
            <div>
              <h2>Dự án gần đây</h2>
              <p>Các dự án có dữ liệu task và tiến độ.</p>
            </div>
            <RouterLink to="/projects">Xem dự án</RouterLink>
          </div>
          <div class="stack-list">
            <RouterLink v-for="project in recentProjects" :key="project.id" :to="`/projects/${project.id}`" class="project-row">
              <div class="row-icon" :style="{ backgroundColor: project.color }">
                <span class="material-symbols-outlined">folder</span>
              </div>
              <div class="row-main">
                <div class="row-title">
                  <strong>{{ displayText(project.name) }}</strong>
                  <span>{{ project.percent }}%</span>
                </div>
                <div class="line-progress">
                  <i :style="{ width: `${project.percent}%` }"></i>
                </div>
                <small>{{ project.done }} / {{ project.total }} task hoàn thành</small>
              </div>
            </RouterLink>
            <div v-if="recentProjects.length === 0" class="empty-state">Chưa có dự án.</div>
          </div>
        </section>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { projectService, taskService, userService } from '../services/api'

const loading = ref(true)
const serviceWarnings = ref([])
const projects = ref([])
const users = ref([])
const tasks = ref([])
const columnsByProject = ref({})
const sprintsByProject = ref({})

const colors = ['#2563eb', '#059669', '#b45309', '#dc2626', '#7c3aed', '#0891b2', '#16a34a', '#ea580c']

const doneColumns = computed(() => {
  const ids = new Set()
  for (const cols of Object.values(columnsByProject.value)) {
    for (const col of cols || []) {
      if (col.type === 'done' || /done|hoàn thành/i.test(col.name || '')) ids.add(col.id)
    }
  }
  return ids
})

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
  }
})

const overallCompletion = computed(() => {
  const total = stats.value.totalTasks
  return total ? Math.round((stats.value.doneTasks / total) * 100) : 0
})

const activeSprintCount = computed(() => {
  return Object.values(sprintsByProject.value)
    .flatMap(list => list || [])
    .filter(sprint => sprint.status === 1).length
})

const metricCards = computed(() => [
  {
    label: 'Tổng dự án',
    value: stats.value.totalProjects,
    caption: `${stats.value.activeProjects} dự án đang chạy`,
    icon: 'folder_shared',
    tone: 'tone-primary',
    percent: stats.value.totalProjects ? 100 : 0,
    to: '/projects',
  },
  {
    label: 'Tổng task',
    value: stats.value.totalTasks,
    caption: 'Toàn bộ công việc',
    icon: 'task_alt',
    tone: 'tone-info',
    percent: stats.value.totalTasks ? 100 : 0,
    to: '/tasks',
  },
  {
    label: 'Đang thực hiện',
    value: stats.value.inProgressTasks,
    caption: 'Task chưa hoàn tất',
    icon: 'sync',
    tone: 'tone-warning',
    percent: stats.value.totalTasks ? Math.round((stats.value.inProgressTasks / stats.value.totalTasks) * 100) : 0,
    to: { path: '/tasks', query: { status: 'active' } },
  },
  {
    label: 'Hoàn thành',
    value: stats.value.doneTasks,
    caption: 'Task đã xong',
    icon: 'check_circle',
    tone: 'tone-success',
    percent: overallCompletion.value,
    to: { path: '/tasks', query: { status: 'done' } },
  },
])

const projectProgress = computed(() => {
  return projects.value.map((p, index) => {
    const projectTasks = tasks.value.filter(t => t.projectId === p.id && !t.deletedAt)
    const done = projectTasks.filter(t => doneColumns.value.has(t.columnId)).length
    const total = projectTasks.length
    return {
      id: p.id,
      name: p.name,
      color: p.color || colors[index % colors.length],
      done,
      total,
      percent: total ? Math.round((done / total) * 100) : 0,
    }
  }).sort((a, b) => b.percent - a.percent)
})

const recentProjects = computed(() => projectProgress.value.slice(0, 4))

const statusChart = computed(() => {
  const buckets = [
    { label: 'Backlog', match: col => col.type === 'backlog' || /backlog/i.test(col.name), value: 0 },
    { label: 'To Do', match: col => /to do|todo/i.test(col.name), value: 0 },
    { label: 'In Progress', match: col => /progress|doing/i.test(col.name), value: 0 },
    { label: 'Review', match: col => /review|test/i.test(col.name), value: 0 },
    { label: 'Done', match: col => col.type === 'done' || /done|hoàn thành/i.test(col.name), value: 0 },
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
    .slice(0, 5)
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
    .slice(0, 5)
})

function initials(name) {
  return (name || '').split(' ').filter(Boolean).map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'ND'
}

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('vi-VN')
}

function translateColumn(label) {
  const map = {
    Backlog: 'Tồn đọng',
    'To Do': 'Cần làm',
    'In Progress': 'Đang làm',
    Review: 'Đánh giá',
    Done: 'Hoàn tất',
  }
  return map[label] || label
}

function displayText(value) {
  const text = String(value || '')
  const normalized = text
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()

  const replacements = [
    ['demo thuyet trinh', 'DEMO THUYẾT TRÌNH'],
    ['he thong quan ly du an va phan cong cong viec', 'Hệ thống quản lý dự án và phân công công việc'],
    ['phan tich yeu cau va ve luong nghiep vu', 'Phân tích yêu cầu và vẽ luồng nghiệp vụ'],
    ['xay dung api project va member', 'Xây dựng API Project và Member'],
    ['thiet ke giao dien dashboard', 'Thiết kế giao diện Dashboard'],
    ['thiet ke giao dien dash', 'Thiết kế giao diện Dashboard'],
    ['cau hinh dark mode', 'Cấu hình dark mode'],
    ['tran thi dev', 'Trần Thị Dev'],
    ['le van tester', 'Lê Văn Tester'],
    ['nguyen van pm', 'Nguyễn Văn PM'],
  ]

  let result = text
  for (const [plain, pretty] of replacements) {
    result = result.replace(new RegExp(plain, 'ig'), pretty)
  }
  return result
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
}

onMounted(loadDashboard)
</script>

<style scoped>
.dashboard-shell {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
}

.dashboard-eyebrow {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: rgb(var(--color-primary));
}

.dashboard-header h1 {
  margin-top: 4px;
  font-size: clamp(28px, 3vw, 40px);
  line-height: 1.08;
  font-weight: 950;
  color: rgb(var(--color-on-surface));
}

.dashboard-header span {
  margin-top: 6px;
  display: block;
  max-width: 720px;
  color: rgb(var(--color-on-surface-variant));
}

.dashboard-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
}

.dashboard-actions a {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 40px;
  border-radius: 12px;
  background: rgb(var(--color-primary));
  padding: 0 14px;
  font-size: 13px;
  font-weight: 900;
  color: rgb(var(--color-on-primary));
  box-shadow: 0 12px 28px rgb(var(--color-primary) / .18);
}

.dashboard-actions a:last-child {
  background: rgb(var(--color-surface-container-lowest));
  color: rgb(var(--color-on-surface));
  box-shadow: inset 0 0 0 1px rgb(var(--color-outline-variant));
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 10px;
}

.kpi-card,
.panel {
  border: 1px solid rgb(var(--color-outline-variant));
  border-radius: 16px;
  background: rgb(var(--color-surface-container-lowest));
  box-shadow: 0 10px 24px rgb(var(--color-outline) / .07);
}

.kpi-card {
  display: grid;
  grid-template-columns: 1fr auto;
  grid-template-areas:
    "label icon"
    "value icon"
    "caption caption"
    "line line";
  align-items: center;
  min-height: 68px;
  padding: 9px 11px 8px;
  transition: transform .18s ease, box-shadow .18s ease;
}

.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 42px rgb(var(--color-outline) / .12);
}

.kpi-head {
  display: contents;
}

.kpi-head > span {
  grid-area: label;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 10px;
  font-weight: 900;
  letter-spacing: .05em;
  text-transform: uppercase;
  color: rgb(var(--color-on-surface-variant));
}

.kpi-icon {
  grid-area: icon;
  display: flex;
  width: 30px;
  height: 30px;
  flex: none;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
}

.kpi-icon .material-symbols-outlined {
  font-size: 16px;
}

.kpi-card strong {
  grid-area: value;
  display: block;
  margin-top: 3px;
  font-size: 24px;
  line-height: 1;
  font-weight: 950;
  color: rgb(var(--color-on-surface));
}

.kpi-card p {
  grid-area: caption;
  min-width: 0;
  overflow: hidden;
  margin-top: 1px;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 11px;
  color: rgb(var(--color-on-surface-variant));
}

.kpi-line {
  grid-area: line;
  height: 3px;
  overflow: hidden;
  margin-top: 7px;
  border-radius: 999px;
  background: rgb(var(--color-surface-container));
}

.kpi-line i {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgb(var(--color-primary)), rgb(var(--color-secondary)));
}

.tone-primary { color: rgb(var(--color-primary)); background: rgb(var(--color-primary) / .12); }
.tone-info { color: #0284c7; background: rgba(2, 132, 199, .12); }
.tone-warning { color: #b45309; background: rgba(180, 83, 9, .12); }
.tone-success { color: rgb(var(--color-secondary)); background: rgb(var(--color-secondary) / .12); }

.dashboard-main-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
  gap: 12px;
}

.dashboard-secondary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.panel {
  padding: 14px;
}

.panel-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.panel-head h2 {
  font-size: 15px;
  font-weight: 950;
  color: rgb(var(--color-on-surface));
}

.panel-head p,
.panel-head a {
  margin-top: 3px;
  font-size: 12px;
  color: rgb(var(--color-on-surface-variant));
}

.panel-head a {
  font-weight: 800;
  color: rgb(var(--color-primary));
}

.panel-symbol {
  color: rgb(var(--color-primary));
}

.chart-area {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 12px;
  min-height: 205px;
  margin-top: 12px;
  padding: 10px;
  border-radius: 14px;
  background: rgb(var(--color-surface-container-low));
}

.chart-column {
  display: grid;
  grid-template-rows: 1fr auto auto;
  align-items: end;
  gap: 6px;
  text-align: center;
}

.chart-bar-wrap {
  display: flex;
  align-items: end;
  justify-content: center;
  height: 135px;
}

.chart-bar {
  width: min(54px, 72%);
  min-height: 12px;
  border-radius: 12px 12px 4px 4px;
  background: linear-gradient(180deg, rgb(var(--color-primary)), rgb(var(--color-secondary)));
  box-shadow: 0 12px 24px rgb(var(--color-primary) / .20);
}

.chart-column span {
  font-size: 12px;
  font-weight: 800;
  color: rgb(var(--color-on-surface-variant));
}

.chart-column b {
  font-size: 13px;
  color: rgb(var(--color-on-surface));
}

.progress-panel {
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.progress-compact {
  display: grid;
  grid-template-columns: 1fr;
  justify-items: center;
  gap: 12px;
  margin-top: 14px;
}

.progress-donut {
  display: grid;
  width: 124px;
  height: 124px;
  place-items: center;
  border-radius: 999px;
  background: conic-gradient(rgb(var(--color-primary)) var(--p), rgb(var(--color-surface-container)) 0);
}

.progress-donut > div {
  display: grid;
  width: 82px;
  height: 82px;
  place-items: center;
  border-radius: inherit;
  background: rgb(var(--color-surface-container-lowest));
  box-shadow: inset 0 0 0 1px rgb(var(--color-outline-variant));
}

.progress-donut strong {
  font-size: 28px;
  line-height: .9;
  font-weight: 950;
}

.progress-donut span {
  font-size: 12px;
  color: rgb(var(--color-on-surface-variant));
}

.progress-summary {
  display: grid;
  width: 100%;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.progress-summary div {
  border-radius: 10px;
  background: rgb(var(--color-surface-container-low));
  padding: 8px 10px;
}

.progress-summary p {
  font-size: 10px;
  font-weight: 800;
  color: rgb(var(--color-on-surface-variant));
}

.progress-summary strong {
  display: block;
  margin-top: 2px;
  font-size: 17px;
  line-height: 1;
  font-weight: 950;
}

.stack-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 12px;
}

.project-row,
.deadline-row,
.member-row {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  min-height: 52px;
  border-radius: 12px;
  background: rgb(var(--color-surface-container-low));
  padding: 9px;
  text-align: left;
  transition: background .18s ease, transform .18s ease;
}

.project-row:hover,
.deadline-row:hover,
.member-row:hover {
  transform: translateY(-1px);
  background: rgb(var(--color-surface-container-high));
}

.row-icon {
  display: grid;
  width: 42px;
  height: 42px;
  flex: none;
  place-items: center;
  border-radius: 14px;
  color: white;
}

.avatar {
  display: grid;
  width: 36px;
  height: 36px;
  flex: none;
  place-items: center;
  border-radius: 999px;
  color: white;
  font-size: 11px;
  font-weight: 950;
}

.row-main {
  min-width: 0;
  flex: 1;
}

.row-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.row-title strong,
.deadline-row strong {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 13px;
  color: rgb(var(--color-on-surface));
}

.row-title span,
.project-row small,
.deadline-row small {
  font-size: 11px;
  color: rgb(var(--color-on-surface-variant));
}

.deadline-row small {
  display: flex;
  align-items: center;
  gap: 7px;
  margin-top: 3px;
}

.deadline-row small b {
  font-weight: 900;
  color: rgb(var(--color-error));
}

.deadline-row small i {
  display: block;
  width: 4px;
  height: 4px;
  border-radius: 999px;
  background: rgb(var(--color-outline));
}

.line-progress {
  height: 6px;
  overflow: hidden;
  margin-top: 7px;
  border-radius: 999px;
  background: rgb(var(--color-surface-container));
}

.line-progress i {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgb(var(--color-primary)), rgb(var(--color-secondary)));
}

.deadline-row .material-symbols-outlined {
  display: grid;
  width: 36px;
  height: 36px;
  flex: none;
  place-items: center;
  border-radius: 12px;
  background: rgb(var(--color-primary) / .10);
  color: rgb(var(--color-primary));
  font-size: 20px;
}

.deadline-row .is-danger {
  background: rgb(var(--color-error-container) / .45);
  color: rgb(var(--color-error));
}

.deadline-row .is-warning {
  background: rgb(var(--color-tertiary) / .12);
  color: rgb(var(--color-tertiary));
}

.empty-state {
  border-radius: 14px;
  border: 1px dashed rgb(var(--color-outline-variant));
  padding: 14px;
  font-size: 13px;
  color: rgb(var(--color-on-surface-variant));
}

.service-warning-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}

.service-warning {
  display: flex;
  align-items: center;
  gap: 8px;
  border-radius: 14px;
  background: rgb(var(--color-error-container) / .28);
  padding: 10px 12px;
  font-size: 12px;
  font-weight: 800;
  color: rgb(var(--color-error));
}

@media (max-width: 980px) {
  .dashboard-main-grid {
    grid-template-columns: 1fr;
  }

  .kpi-grid,
  .dashboard-secondary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .dashboard-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .kpi-grid,
  .dashboard-secondary-grid,
  .service-warning-grid {
    grid-template-columns: 1fr;
  }

  .chart-area {
    overflow-x: auto;
    grid-template-columns: repeat(5, 120px);
  }

  .progress-compact {
    grid-template-columns: 1fr;
  }
}
</style>
