<template>
  <div class="page-wrap">
    <section class="page-hero">
      <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="page-eyebrow">Task hub</p>
          <h2 class="font-headline-lg text-headline-lg text-on-surface">Danh sách task</h2>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-1">
            Tổng hợp task từ các dự án hiện có, dữ liệu lấy trực tiếp từ TaskService.
          </p>
        </div>
      <button
        @click="$router.push('/kanban')"
        class="app-button-primary"
      >
        <span class="material-symbols-outlined text-[18px]">view_kanban</span>
        Mở Kanban
      </button>
      </div>
    </section>

    <div v-if="error" class="bg-error-container/20 border border-error/20 rounded-lg px-md py-sm text-error font-label-md text-label-md">
      {{ error }}
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-md">
      <RouterLink
        v-for="item in summaryCards"
        :key="item.label"
        :to="{ path: '/tasks', query: item.value === 'all' ? {} : { status: item.value } }"
        class="app-card interactive-card p-md"
        :class="activeStatus === item.value ? 'ring-2 ring-primary border-primary' : ''"
      >
        <div class="flex items-center justify-between">
          <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ item.label }}</span>
          <span class="material-symbols-outlined text-[20px]" :class="item.iconClass">{{ item.icon }}</span>
        </div>
        <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ item.count }}</p>
      </RouterLink>
    </div>

    <div class="app-panel">
      <div class="px-md py-sm border-b border-outline-variant flex flex-col gap-sm lg:flex-row lg:items-center lg:justify-between">
        <div class="flex flex-wrap gap-xs">
          <RouterLink
            v-for="item in filters"
            :key="item.value"
            :to="{ path: '/tasks', query: item.value === 'all' ? {} : { status: item.value } }"
            class="px-3 py-1.5 rounded-full border font-label-md text-label-md transition-colors"
            :class="activeStatus === item.value
              ? 'bg-primary text-on-primary border-primary'
              : 'bg-surface-container-low text-on-surface-variant border-outline-variant hover:border-primary hover:text-primary'"
          >
            {{ item.label }}
          </RouterLink>
        </div>

        <div class="flex items-center gap-xs app-input rounded-full px-3 py-1.5">
          <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
          <input
            v-model="search"
            class="bg-transparent border-none outline-none text-on-surface font-body-md text-body-md placeholder:text-outline w-56"
            placeholder="Tìm task, dự án, người phụ trách..."
          />
        </div>
      </div>

      <div v-if="loading" class="p-lg text-on-surface-variant flex items-center gap-2">
        <span class="material-symbols-outlined animate-spin">progress_activity</span>
        Đang tải task...
      </div>

      <div v-else-if="filteredTasks.length === 0" class="p-xl text-center text-on-surface-variant">
        <span class="material-symbols-outlined text-[48px] block mb-sm">task_alt</span>
        Không có task phù hợp.
      </div>

      <div v-else class="divide-y divide-outline-variant/50">
        <button
          v-for="task in filteredTasks"
          :key="task.id"
          @click="$router.push({ path: '/kanban', query: { projectId: task.projectId } })"
          class="w-full text-left px-md py-sm hover:bg-surface-container-low transition-colors flex flex-col gap-2"
        >
          <div class="flex flex-col gap-2 lg:flex-row lg:items-start lg:justify-between">
            <div class="min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="font-label-sm text-label-sm px-2 py-0.5 rounded-full" :class="statusClass(task)">
                  {{ task.columnName || 'Chưa phân cột' }}
                </span>
                <span class="font-label-sm text-label-sm px-2 py-0.5 rounded-full" :class="priorityClass(task.priority)">
                  {{ priorityText(task.priority) }}
                </span>
              </div>
              <h3 class="font-headline-sm text-headline-sm text-on-surface mt-2 line-clamp-1">{{ task.title }}</h3>
              <p class="font-body-sm text-body-sm text-on-surface-variant line-clamp-2">{{ task.description || 'Chưa có mô tả' }}</p>
            </div>
            <div class="flex flex-wrap items-center gap-sm text-on-surface-variant font-label-md text-label-md lg:justify-end">
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
    .map(task => {
      const column = columnMap.get(task.columnId)
      const assignee = userMap.get(task.assignedTo)
      return {
        ...task,
        projectName: projectMap.get(task.projectId)?.name || 'Không rõ dự án',
        columnName: column?.name || '',
        columnType: column?.type || '',
        isDone: doneColumnIds.value.has(task.columnId),
        assigneeName: assignee?.fullName || assignee?.email || '',
      }
    })
})

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
  { value: 'all', label: `Tất cả (${enrichedTasks.value.length})` },
  { value: 'active', label: `Đang thực hiện (${enrichedTasks.value.filter(task => !task.isDone).length})` },
  { value: 'done', label: `Hoàn thành (${enrichedTasks.value.filter(task => task.isDone).length})` },
])

const summaryCards = computed(() => [
  { value: 'all', label: 'Tổng task', count: enrichedTasks.value.length, icon: 'task_alt', iconClass: 'text-primary' },
  { value: 'active', label: 'Đang thực hiện', count: enrichedTasks.value.filter(task => !task.isDone).length, icon: 'sync', iconClass: 'text-primary' },
  { value: 'done', label: 'Hoàn thành', count: enrichedTasks.value.filter(task => task.isDone).length, icon: 'check_circle', iconClass: 'text-secondary' },
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
