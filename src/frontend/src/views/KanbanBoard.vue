<template>
  <div class="flex-1 flex flex-col h-full overflow-hidden">
    <!-- Toolbar -->
    <div class="w-full px-md md:px-lg py-sm bg-surface-container-lowest/85 border-b border-outline-variant flex flex-wrap items-center justify-between gap-4 z-20 backdrop-blur-xl">
      <div class="flex items-center gap-3 flex-wrap">
        <div class="hidden lg:block mr-sm">
          <p class="page-eyebrow">Board</p>
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Kanban Workspace</h2>
        </div>
        <!-- Project selector -->
        <div class="flex items-center gap-2">
          <span class="material-symbols-outlined text-[18px] text-on-surface-variant">folder_shared</span>
          <select v-model="selectedProjectId" @change="onProjectChange"
            class="app-input rounded-lg px-3 py-1.5 font-label-md text-label-md">
            <option value="">-- Chọn dự án --</option>
            <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
          </select>
        </div>
        <!-- Sprint filter -->
        <div v-if="sprints.length > 0" class="flex items-center gap-2">
          <span class="material-symbols-outlined text-[18px] text-on-surface-variant">sprint</span>
          <select v-model="selectedSprintId" @change="loadColumns"
            class="app-input rounded-lg px-3 py-1.5 font-label-md text-label-md">
            <option value="">Tất cả sprint</option>
            <option v-for="s in sprints" :key="s.id" :value="s.id">
              {{ s.name }}{{ s.status === 1 ? ' ●' : '' }}
            </option>
          </select>
        </div>
        <FilterBtn icon="flag"  label="Độ ưu tiên" :options="PRIORITY_OPTIONS" v-model="selectedPriorities" />
        <FilterBtn icon="label" label="Nhãn"        :options="availableTagOptions"  v-model="selectedTags" />
      </div>
      <div class="flex items-center gap-2">
        <button class="p-1.5 text-primary bg-primary/10 rounded transition-colors" title="Bảng">
          <span class="material-symbols-outlined text-[20px]" style="font-variation-settings: 'FILL' 1">grid_view</span>
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex-1 flex items-center justify-center text-on-surface-variant gap-2">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>Đang tải bảng Kanban...
    </div>

    <!-- No project selected -->
    <div v-else-if="!selectedProjectId" class="flex-1 flex flex-col items-center justify-center gap-4 text-on-surface-variant">
      <span class="material-symbols-outlined text-[64px] opacity-30">view_kanban</span>
      <p class="font-body-lg text-body-lg">Chọn dự án để xem bảng Kanban</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="bg-error-container/20 border border-error/30 rounded-xl p-md text-error font-label-md flex items-center gap-2">
        <span class="material-symbols-outlined">warning</span>{{ error }}
      </div>
    </div>

    <!-- Kanban Board -->
    <div v-else class="flex-1 overflow-x-auto overflow-y-hidden kanban-scroll p-md md:p-lg flex gap-md sm:gap-lg items-start">
      <KanbanColumn v-for="col in filteredColumns" :key="col.id" :column="col"
        :can-edit="canEditTasks"
        @task-moved="handleTaskMoved" @add-task="handleAddTask"
        @open-task="openTaskId = $event" />

      <!-- Empty state -->
      <div v-if="columns.length === 0" class="flex-1 flex items-center justify-center text-on-surface-variant">
        <p class="font-body-lg text-body-lg">Dự án này chưa có cột Kanban nào.</p>
      </div>
    </div>
  </div>

  <TaskDetailModal
    v-if="openTaskId"
    :taskId="openTaskId"
    :columns="columns"
    :can-edit="canEditTasks"
    @close="openTaskId = null"
    @updated="loadColumns"
    @deleted="loadColumns" />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import KanbanColumn     from '../components/KanbanColumn.vue'
import FilterBtn        from '../components/FilterBtn.vue'
import TaskDetailModal  from '../components/TaskDetailModal.vue'
import { useAuth } from '../composables/useAuth'
import { projectService, taskService, userService } from '../services/api'

const route = useRoute()
const { user } = useAuth()

const projects          = ref([])
const selectedProjectId = ref(route.query.projectId || '')
const sprints           = ref([])
const selectedSprintId  = ref('')
const projectMembers    = ref([])
const columns           = ref([])
const loading           = ref(false)
const error             = ref('')
const openTaskId        = ref(null)

const selectedPriorities = ref([])
const selectedTags       = ref([])

const PRIORITY_OPTIONS = [
  { value: 'high',   label: 'Cao',        icon: 'keyboard_double_arrow_up' },
  { value: 'medium', label: 'Trung bình', icon: 'drag_handle' },
  { value: 'low',    label: 'Thấp',       icon: 'keyboard_double_arrow_down' },
  { value: 'none',   label: 'Không có',   icon: 'remove' },
]

const availableTagOptions = computed(() => {
  const tags = new Set()
  for (const col of columns.value)
    for (const t of col.tasks)
      for (const tag of t.tags) tags.add(tag.text)
  return [...tags].map(t => ({ value: t, label: t }))
})

const filteredColumns = computed(() => {
  const pf = selectedPriorities.value
  const tf = selectedTags.value
  if (!pf.length && !tf.length) return columns.value
  return columns.value.map(col => {
    const tasks = col.tasks.filter(t => {
      const matchPriority = !pf.length || pf.includes(t.priority)
      const matchTag      = !tf.length || t.tags.some(tag => tf.includes(tag.text))
      return matchPriority && matchTag
    })
    return { ...col, tasks, count: tasks.length }
  })
})
const currentMember = computed(() =>
  projectMembers.value.find(member => String(member.userId).toLowerCase() === String(user.value?.id || '').toLowerCase())
)
const currentRole = computed(() => {
  const names = ['Owner', 'Project Manager', 'Developer', 'Tester', 'Viewer']
  return names[currentMember.value?.role] || ''
})
const canEditTasks = computed(() => ['Owner', 'Project Manager', 'Developer', 'Tester'].includes(currentRole.value))

const PRIORITY_MAP  = { 1: 'low', 2: 'medium', 3: 'high', 0: 'none' }
const AVATAR_COLORS = ['#3525cd','#006a61','#684000','#ba1a1a','#0f5e9c','#6a0dad','#2e7d32','#e65100']

function formatDeadline(d) {
  if (!d) return { text: 'Không có', color: 'text-outline' }
  const date = new Date(d)
  const now  = new Date()
  const diff = Math.ceil((date - now) / 86400000)
  if (diff < 0)   return { text: 'Quá hạn',      color: 'text-error' }
  if (diff === 0)  return { text: 'Hôm nay',      color: 'text-error' }
  if (diff <= 3)   return { text: `${diff} ngày`, color: 'text-tertiary' }
  return { text: date.toLocaleDateString('vi-VN'), color: 'text-on-surface-variant' }
}

function mapTask(t, userMap = {}) {
  const dl   = formatDeadline(t.dueDate || t.deadline)
  const user = t.assignedTo ? userMap[t.assignedTo] : null
  const initials = user?.fullName
    ? user.fullName.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
    : null
  // Deterministic color from userId chars
  const colorIdx = t.assignedTo
    ? t.assignedTo.charCodeAt(0) % AVATAR_COLORS.length
    : 0

  return {
    id:            t.id,
    shortId:       t.id.slice(0, 8).toUpperCase(),
    title:         t.title,
    description:   t.description || '',
    tags:          (t.tags?.length ? t.tags : [PRIORITY_MAP[t.priority] === 'high' ? 'Critical' : 'Project']).map(tag => ({ text: tag, color: tag === 'Critical' ? 'bg-error-container/30 text-error' : 'bg-primary/10 text-primary' })),
    priority:      PRIORITY_MAP[t.priority] || 'medium',
    deadline:      dl.text,
    deadlineColor: dl.color,
    avatar:        null,
    avatarText:    initials,
    avatarColor:   initials ? AVATAR_COLORS[colorIdx] : null,
    assignedName:  user?.fullName || null,
    unassigned:    !t.assignedTo,
    inProgress:    false,
    done:          false,
    progress:      t.progress ?? (PRIORITY_MAP[t.priority] === 'high' ? 35 : 60),
    comments:      t.commentCount ?? t.commentsCount ?? 0,
    attachments:   t.attachmentCount ?? 0,
  }
}

async function loadColumns() {
  if (!selectedProjectId.value) { columns.value = []; projectMembers.value = []; return }

  loading.value = true
  error.value   = ''
  try {
    const taskParams = { projectId: selectedProjectId.value }
    if (selectedSprintId.value) taskParams.sprintId = selectedSprintId.value

    const [cols, tasks, allUsers, members] = await Promise.all([
      taskService.getColumns(selectedProjectId.value),
      taskService.getAll(taskParams),
      userService.getAll().catch(() => []),
      projectService.getMembers(selectedProjectId.value).catch(() => []),
    ])
    projectMembers.value = members || []
    const userMap = Object.fromEntries((allUsers || []).map(u => [u.id, u]))

    const tasksByColumn = {}
    for (const t of (tasks || [])) {
      const cid = t.columnId
      if (!tasksByColumn[cid]) tasksByColumn[cid] = []
      tasksByColumn[cid].push(mapTask(t, userMap))
    }

    const DOT_COLORS = {
      backlog: 'bg-outline',
      active:  'bg-secondary',
      done:    'bg-secondary',
      custom:  'bg-primary-container',
    }

    columns.value = (cols || []).map(c => ({
      id:       c.id,
      title:    c.name,
      dotColor: DOT_COLORS[c.type] || 'bg-outline',
      countBg:  c.type === 'done' ? 'bg-surface-container text-on-surface-variant' : 'bg-primary/10 text-primary',
      count:    (tasksByColumn[c.id] || []).length,
      tasks:    tasksByColumn[c.id] || [],
    }))
  } catch (e) {
    error.value = 'Không thể tải dữ liệu Kanban. Kiểm tra TaskService đang chạy.'
  } finally {
    loading.value = false
  }
}

async function onProjectChange() {
  selectedSprintId.value = ''
  sprints.value = []
  if (selectedProjectId.value) {
    sprints.value = await projectService.getSprints(selectedProjectId.value).catch(() => [])
    // Auto-select active sprint if any
    const active = sprints.value.find(s => s.status === 1)
    if (active) selectedSprintId.value = active.id
  }
  await loadColumns()
}

async function handleTaskMoved({ taskId, columnId }) {
  if (!canEditTasks.value) return
  try {
    await taskService.moveColumn(taskId, columnId)
    await loadColumns()
  } catch { /* ignore */ }
}

async function handleAddTask({ columnId, title }) {
  if (!canEditTasks.value) return
  if (!selectedProjectId.value) return
  try {
    await taskService.create({
      projectId: selectedProjectId.value,
      columnId,
      title,
      priority: 2,
      sprintId: selectedSprintId.value || null,
    })
    await loadColumns()
  } catch { /* ignore */ }
}

onMounted(async () => {
  try {
    projects.value = await projectService.getAll() || []
    if (!selectedProjectId.value && projects.value.length > 0) {
      selectedProjectId.value = projects.value[0].id
    }
    // Load sprints for the initial project
    if (selectedProjectId.value) {
      sprints.value = await projectService.getSprints(selectedProjectId.value).catch(() => [])
      const active = sprints.value.find(s => s.status === 1)
      if (active) selectedSprintId.value = active.id
    }
    await loadColumns()
  } catch { /* ProjectService may be offline */ }
})
</script>
