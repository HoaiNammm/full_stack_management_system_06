<template>
  <div class="page-wrap">
    <section class="task-overview-shell">
      <div class="relative z-10 flex flex-col gap-lg lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="task-eyebrow">Trung tâm công việc</p>
          <h2 class="font-headline-lg text-headline-lg text-on-surface">Danh sách công việc</h2>
          <p class="mt-1 max-w-2xl font-body-lg text-body-lg text-on-surface-variant">
            Tổng hợp task từ các dự án hiện có, dữ liệu lấy trực tiếp từ TaskService.
          </p>
        </div>
        <button @click="$router.push('/kanban')" class="app-button-primary">
          <span class="material-symbols-outlined text-[18px]">view_kanban</span>
          Mở Kanban
        </button>
      </div>

      <div v-if="error" class="relative z-10 mt-md rounded-lg border border-error/20 bg-error-container/20 px-md py-sm font-label-md text-error">
        {{ error }}
      </div>

      <div class="relative z-10 mt-lg grid grid-cols-1 gap-md md:grid-cols-3">
        <RouterLink
          v-for="item in summaryCards"
          :key="item.label"
          :to="{ path: '/tasks', query: item.value === 'all' ? {} : { status: item.value } }"
          class="task-widget group"
          :class="[item.tone, activeStatus === item.value ? 'is-active' : '']"
        >
          <div class="task-widget-icon">
            <span class="material-symbols-outlined">{{ item.icon }}</span>
          </div>
          <div class="min-w-0 flex-1">
            <p>{{ item.label }}</p>
            <strong>{{ item.count }}</strong>
            <span>{{ item.caption }}</span>
            <div class="task-widget-progress">
              <i :style="{ width: `${item.percent}%` }"></i>
            </div>
          </div>
        </RouterLink>
      </div>
    </section>

    <div class="app-panel">
      <div class="flex flex-col gap-sm border-b border-outline-variant px-md py-sm lg:flex-row lg:items-center lg:justify-between">
        <div class="flex flex-wrap gap-xs">
          <RouterLink
            v-for="item in filters"
            :key="item.value"
            :to="{ path: '/tasks', query: item.value === 'all' ? {} : { status: item.value } }"
            class="rounded-full border px-3 py-1.5 font-label-md text-label-md transition-colors"
            :class="activeStatus === item.value
              ? 'border-primary bg-primary text-on-primary'
              : 'border-outline-variant bg-surface-container-low text-on-surface-variant hover:border-primary hover:text-primary'"
          >
            {{ item.label }}
          </RouterLink>
        </div>

        <div class="flex items-center gap-xs rounded-full px-3 py-1.5 app-input">
          <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
          <input
            v-model="search"
            class="w-56 border-none bg-transparent font-body-md text-body-md text-on-surface outline-none placeholder:text-outline"
            placeholder="Tìm task, dự án, người phụ trách..."
          />
        </div>
      </div>

      <div v-if="loading" class="flex items-center gap-2 p-lg text-on-surface-variant">
        <span class="material-symbols-outlined animate-spin">progress_activity</span>
        Đang tải task...
      </div>

      <div v-else-if="filteredTasks.length === 0" class="p-xl text-center text-on-surface-variant">
        <span class="material-symbols-outlined mb-sm block text-[48px]">task_alt</span>
        Không có task phù hợp.
      </div>

      <div v-else class="grid grid-cols-1 gap-md p-md xl:grid-cols-2">
        <button
          v-for="task in filteredTasks"
          :key="task.id"
          @click="$router.push({ path: '/kanban', query: { projectId: task.projectId } })"
          class="workspace-card group overflow-hidden p-0 text-left"
        >
          <img :src="task.thumbnail" class="h-32 w-full object-cover transition-transform duration-500 group-hover:scale-105" alt="" />
          <div class="flex flex-col gap-2 p-md">
            <div class="min-w-0">
              <div class="flex flex-wrap items-center gap-2">
                <span class="rounded-full px-2 py-0.5 font-label-sm text-label-sm" :class="statusClass(task)">
                  {{ task.columnName || 'Chưa phân cột' }}
                </span>
                <span class="rounded-full px-2 py-0.5 font-label-sm text-label-sm" :class="priorityClass(task.priority)">
                  {{ priorityText(task.priority) }}
                </span>
              </div>
              <h3 class="mt-2 line-clamp-1 font-headline-sm text-headline-sm text-on-surface">{{ task.title }}</h3>
              <p class="line-clamp-2 font-body-sm text-body-sm text-on-surface-variant">{{ task.description || 'Chưa có mô tả' }}</p>
            </div>
            <div class="flex flex-wrap items-center gap-sm font-label-md text-label-md text-on-surface-variant">
              <span class="inline-flex items-center gap-1">
                <span class="material-symbols-outlined text-[16px]">folder</span>
                {{ task.projectName }}
              </span>
              <span class="inline-flex items-center gap-1">
                <span class="material-symbols-outlined text-[16px]">person</span>
                {{ task.assigneeName || 'Chưa giao' }}
              </span>
              <span v-if="task.dueDate" class="inline-flex items-center gap-1">
                <span class="material-symbols-outlined text-[16px]">event</span>
                {{ formatDate(task.dueDate) }}
              </span>
            </div>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { projectService, taskService, userService } from '../services/api'
import { getTaskThumbnail } from '../services/visualAssets'

const route = useRoute()
const loading = ref(true)
const error = ref('')
const search = ref('')
const projects = ref([])
const users = ref([])
const tasks = ref([])
const columnsByProject = ref({})

const activeStatus = computed(() => {
  const status = route.query.status
  return ['done', 'active'].includes(status) ? status : 'all'
})

const doneColumnIds = computed(() => {
  const ids = new Set()
  for (const cols of Object.values(columnsByProject.value)) {
    for (const col of cols || []) {
      if (col.type === 'done' || /done|hoàn thành/i.test(col.name || '')) ids.add(col.id)
    }
  }
  return ids
})

const enrichedTasks = computed(() => {
  const projectMap = new Map(projects.value.map(project => [project.id, project]))
  const userMap = new Map(users.value.map(user => [user.id, user]))
  const columnMap = new Map()

  for (const cols of Object.values(columnsByProject.value)) {
    for (const col of cols || []) columnMap.set(col.id, col)
  }

  return tasks.value
    .filter(task => !task.deletedAt)
    .map((task, index) => {
      const column = columnMap.get(task.columnId)
      const assignee = userMap.get(task.assignedTo)
      return {
        ...task,
        projectName: projectMap.get(task.projectId)?.name || 'Không rõ dự án',
        columnName: column?.name || '',
        columnType: column?.type || '',
        isDone: doneColumnIds.value.has(task.columnId),
        assigneeName: assignee?.fullName || assignee?.email || '',
        thumbnail: getTaskThumbnail(task, index),
      }
    })
})

const totalCount = computed(() => enrichedTasks.value.length)
const activeCount = computed(() => enrichedTasks.value.filter(task => !task.isDone).length)
const doneCount = computed(() => enrichedTasks.value.filter(task => task.isDone).length)

const filteredTasks = computed(() => {
  const keyword = search.value.trim().toLowerCase()
  return enrichedTasks.value
    .filter(task => activeStatus.value === 'done' ? task.isDone : activeStatus.value === 'active' ? !task.isDone : true)
    .filter(task => {
      if (!keyword) return true
      return [task.title, task.description, task.projectName, task.assigneeName, task.columnName]
        .some(value => (value || '').toLowerCase().includes(keyword))
    })
})

const filters = computed(() => [
  { value: 'all', label: `Tất cả (${totalCount.value})` },
  { value: 'active', label: `Đang thực hiện (${activeCount.value})` },
  { value: 'done', label: `Hoàn thành (${doneCount.value})` },
])

const summaryCards = computed(() => [
  {
    value: 'all',
    label: 'Tổng task',
    count: totalCount.value,
    icon: 'task_alt',
    caption: 'Toàn bộ công việc',
    tone: 'is-primary',
    percent: totalCount.value ? 100 : 8,
  },
  {
    value: 'active',
    label: 'Đang thực hiện',
    count: activeCount.value,
    icon: 'sync',
    caption: 'Cần xử lý',
    tone: 'is-warning',
    percent: totalCount.value ? Math.round((activeCount.value / totalCount.value) * 100) : 8,
  },
  {
    value: 'done',
    label: 'Hoàn thành',
    count: doneCount.value,
    icon: 'check_circle',
    caption: 'Đã xong',
    tone: 'is-success',
    percent: totalCount.value ? Math.round((doneCount.value / totalCount.value) * 100) : 8,
  },
])

function priorityText(priority) {
  const map = { high: 'Cao', medium: 'Trung bình', low: 'Thấp' }
  return map[String(priority || '').toLowerCase()] || 'Bình thường'
}

function priorityClass(priority) {
  const value = String(priority || '').toLowerCase()
  if (value === 'high') return 'bg-error-container/30 text-error'
  if (value === 'medium') return 'bg-tertiary/10 text-tertiary'
  if (value === 'low') return 'bg-surface-container text-on-surface-variant'
  return 'bg-primary/10 text-primary'
}

function statusClass(task) {
  if (task.isDone) return 'bg-secondary-container/30 text-secondary'
  if (/progress|doing/i.test(task.columnName || '')) return 'bg-primary/10 text-primary'
  return 'bg-surface-container text-on-surface-variant'
}

function formatDate(date) {
  return new Date(date).toLocaleDateString('vi-VN')
}

async function loadTasks() {
  loading.value = true
  error.value = ''
  try {
    const [projectList, userList] = await Promise.all([
      projectService.getAll().catch(() => []),
      userService.getAll().catch(() => []),
    ])
    projects.value = projectList || []
    users.value = userList || []

    const projectIds = projects.value.map(project => project.id)
    const [taskResults, columnResults] = await Promise.all([
      Promise.allSettled(projectIds.map(id => taskService.getAll({ projectId: id }))),
      Promise.allSettled(projectIds.map(id => taskService.getColumns(id))),
    ])

    tasks.value = taskResults.flatMap(result => result.status === 'fulfilled' ? (result.value || []) : [])
    columnsByProject.value = Object.fromEntries(projectIds.map((id, index) => [
      id,
      columnResults[index]?.status === 'fulfilled' ? (columnResults[index].value || []) : [],
    ]))
  } catch {
    error.value = 'Không tải được danh sách task. Kiểm tra TaskService và ProjectService.'
  } finally {
    loading.value = false
  }
}

onMounted(loadTasks)
</script>

<style scoped>
.task-overview-shell {
  position: relative;
  overflow: hidden;
  border-radius: 26px;
  background:
    radial-gradient(circle at 12% 0%, rgb(var(--color-primary) / 0.13), transparent 28rem),
    radial-gradient(circle at 100% 20%, rgb(var(--color-tertiary) / 0.10), transparent 22rem),
    rgb(var(--color-surface-container-lowest));
  padding: clamp(20px, 2.4vw, 32px);
  box-shadow: 0 18px 50px rgb(var(--color-outline) / 0.12);
}

.task-eyebrow {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: rgb(var(--color-primary));
}

.task-widget {
  display: flex;
  align-items: center;
  gap: 14px;
  border-radius: 20px;
  background: rgb(var(--color-surface-container-low) / 0.86);
  padding: 16px;
  box-shadow: inset 0 1px 0 rgb(var(--color-surface-bright) / 0.55);
  transition: transform 180ms ease, background 180ms ease, box-shadow 180ms ease;
}

.task-widget:hover,
.task-widget.is-active {
  transform: translateY(-2px);
  background: rgb(var(--color-surface-container-high));
  box-shadow: 0 18px 42px rgb(var(--color-outline) / 0.14);
}

.task-widget-icon {
  display: grid;
  width: 52px;
  height: 52px;
  flex: none;
  place-items: center;
  border-radius: 16px;
}

.task-widget-icon .material-symbols-outlined {
  font-size: 24px;
}

.task-widget.is-primary .task-widget-icon {
  background: rgb(var(--color-primary) / 0.12);
  color: rgb(var(--color-primary));
}

.task-widget.is-warning .task-widget-icon {
  background: rgb(var(--color-tertiary) / 0.12);
  color: rgb(var(--color-tertiary));
}

.task-widget.is-success .task-widget-icon {
  background: rgb(var(--color-secondary) / 0.12);
  color: rgb(var(--color-secondary));
}

.task-widget p {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .06em;
  text-transform: uppercase;
  color: rgb(var(--color-on-surface-variant));
}

.task-widget strong {
  display: block;
  margin-top: 4px;
  font-size: 34px;
  line-height: 1;
  font-weight: 950;
  color: rgb(var(--color-on-surface));
}

.task-widget span:not(.material-symbols-outlined) {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  color: rgb(var(--color-on-surface-variant));
}

.task-widget-progress {
  margin-top: 10px;
  height: 6px;
  overflow: hidden;
  border-radius: 999px;
  background: rgb(var(--color-surface-container));
}

.task-widget-progress i {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgb(var(--color-primary)), rgb(var(--color-secondary)));
}
</style>
