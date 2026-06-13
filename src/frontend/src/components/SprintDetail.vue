<template>
  <div class="flex flex-col gap-md">
    <!-- Back nav + header -->
    <div class="flex items-center gap-3">
      <button @click="$emit('back')"
        class="flex items-center gap-1 text-on-surface-variant hover:text-primary font-label-md text-label-md transition-colors">
        <span class="material-symbols-outlined text-[18px]">arrow_back</span>
        Sprints
      </button>
      <span class="text-outline">/</span>
      <div class="flex items-center gap-2">
        <span class="font-headline-sm text-headline-sm text-on-surface">{{ sprint.name }}</span>
        <span class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="statusStyle(sprint.status)">
          {{ statusLabel[sprint.status] }}
        </span>
      </div>
      <div class="ml-auto flex items-center gap-2">
        <button v-if="canManageSprints && sprint.status === 0" @click="$emit('start', sprint)"
          class="flex items-center gap-1 px-3 py-1.5 border border-primary text-primary rounded-lg font-label-md text-label-md hover:bg-primary hover:text-on-primary transition-colors">
          <span class="material-symbols-outlined text-[16px]">play_arrow</span>Bắt đầu
        </button>
        <button v-else-if="canManageSprints && sprint.status === 1" @click="$emit('complete', sprint)"
          class="flex items-center gap-1 px-3 py-1.5 border border-secondary text-secondary rounded-lg font-label-md text-label-md hover:bg-secondary hover:text-on-secondary transition-colors">
          <span class="material-symbols-outlined text-[16px]">check_circle</span>Hoàn thành
        </button>
      </div>
    </div>

    <!-- Sprint meta -->
    <div class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md flex flex-wrap gap-md">
      <div class="flex items-center gap-2 text-on-surface-variant">
        <span class="material-symbols-outlined text-[16px]">calendar_today</span>
        <span class="font-label-md text-label-md">{{ fmt(sprint.startDate) }} → {{ fmt(sprint.endDate) }}</span>
      </div>
      <div class="flex items-center gap-2 text-on-surface-variant">
        <span class="material-symbols-outlined text-[16px]">flag</span>
        <span class="font-label-md text-label-md">{{ sprint.goal || 'Chưa có mục tiêu' }}</span>
      </div>
    </div>

    <!-- Progress + Stats -->
    <div v-if="!loading" class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md flex flex-col gap-3">
      <div class="flex items-center justify-between">
        <h4 class="font-headline-sm text-headline-sm text-on-surface">Tiến độ</h4>
        <span class="font-label-lg text-label-lg text-on-surface-variant">
          {{ doneTasks }} / {{ tasks.length }} task hoàn thành
        </span>
      </div>
      <!-- Progress bar -->
      <div class="h-3 bg-surface-container rounded-full overflow-hidden">
        <div class="h-full bg-secondary rounded-full transition-all duration-500"
          :style="`width: ${progressPct}%`"></div>
      </div>
      <div class="grid grid-cols-3 gap-md text-center">
        <div class="bg-surface-container rounded-lg p-3">
          <p class="font-headline-sm text-headline-sm text-on-surface">{{ tasks.length }}</p>
          <p class="font-label-sm text-label-sm text-on-surface-variant">Tổng task</p>
        </div>
        <div class="bg-surface-container rounded-lg p-3">
          <p class="font-headline-sm text-headline-sm text-secondary">{{ doneTasks }}</p>
          <p class="font-label-sm text-label-sm text-on-surface-variant">Hoàn thành</p>
        </div>
        <div class="bg-surface-container rounded-lg p-3">
          <p class="font-headline-sm text-headline-sm text-primary">{{ tasks.length - doneTasks }}</p>
          <p class="font-label-sm text-label-sm text-on-surface-variant">Còn lại</p>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-md">
      <!-- Task list -->
      <div class="lg:col-span-2 bg-surface-container-lowest rounded-xl border border-outline-variant overflow-hidden">
        <div class="p-md border-b border-outline-variant">
          <h4 class="font-headline-sm text-headline-sm text-on-surface">Danh sách task</h4>
        </div>

        <div v-if="loading" class="p-lg flex items-center justify-center text-on-surface-variant gap-2">
          <span class="material-symbols-outlined animate-spin">progress_activity</span>Đang tải...
        </div>
        <div v-else-if="tasks.length === 0" class="p-lg text-center text-on-surface-variant font-body-md text-body-md">
          Sprint này chưa có task nào. Gán task vào sprint từ bảng Kanban.
        </div>
        <div v-else class="divide-y divide-outline-variant/50">
          <div v-for="t in tasks" :key="t.id"
            class="flex items-center gap-3 px-md py-3 hover:bg-surface-container-low transition-colors">
            <!-- Status dot (by column type) -->
            <span class="w-2 h-2 rounded-full flex-shrink-0"
              :class="t._done ? 'bg-secondary' : t._active ? 'bg-primary' : 'bg-outline'"></span>
            <!-- Title -->
            <div class="flex-1 min-w-0">
              <p class="font-body-md text-body-md text-on-surface truncate" :class="{ 'line-through text-on-surface-variant': t._done }">
                {{ t.title }}
              </p>
              <p class="font-label-sm text-label-sm text-outline">#{{ t.id.slice(0, 8).toUpperCase() }}</p>
            </div>
            <!-- Priority badge -->
            <span class="text-[11px] font-bold px-1.5 py-0.5 rounded flex-shrink-0"
              :class="priorityStyle(t.priority)">{{ priorityLabel(t.priority) }}</span>
            <!-- Assignee avatar -->
            <div v-if="t.assignedTo && userMap[t.assignedTo]" class="flex-shrink-0"
              :title="userMap[t.assignedTo]?.fullName">
              <div class="w-6 h-6 rounded-full text-white flex items-center justify-center font-label-sm text-[10px]"
                :style="{ backgroundColor: avatarColor(t.assignedTo) }">
                {{ userMap[t.assignedTo]?.fullName?.charAt(0).toUpperCase() }}
              </div>
            </div>
            <div v-else class="w-6 h-6 rounded-full bg-surface-container flex items-center justify-center text-outline flex-shrink-0">
              <span class="material-symbols-outlined text-[14px]">person</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Members panel -->
      <div class="bg-surface-container-lowest rounded-xl border border-outline-variant overflow-hidden">
        <div class="p-md border-b border-outline-variant">
          <h4 class="font-headline-sm text-headline-sm text-on-surface">Thành viên trong sprint</h4>
          <p class="font-label-sm text-label-sm text-on-surface-variant mt-0.5">{{ activeMembers.length }} người có task</p>
        </div>
        <div v-if="loading" class="p-md flex items-center justify-center text-on-surface-variant">
          <span class="material-symbols-outlined animate-spin">progress_activity</span>
        </div>
        <div v-else-if="activeMembers.length === 0" class="p-md text-center text-on-surface-variant font-body-sm text-body-sm">
          Chưa có task nào được gán
        </div>
        <div v-else class="p-md flex flex-col gap-3">
          <div v-for="m in activeMembers" :key="m.userId" class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-full text-white flex items-center justify-center font-label-md flex-shrink-0"
              :style="{ backgroundColor: avatarColor(m.userId) }">
              {{ m.name.charAt(0).toUpperCase() }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-label-lg text-label-lg text-on-surface truncate">{{ m.name }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant">{{ m.taskCount }} task</p>
            </div>
            <!-- Mini progress per member -->
            <div class="text-right">
              <p class="font-label-sm text-label-sm text-secondary">{{ m.doneCount }}/{{ m.taskCount }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '../composables/useAuth'
import { taskService, userService } from '../services/api'

const props = defineProps(['sprint', 'project'])
defineEmits(['back', 'start', 'complete', 'reload'])
const { user } = useAuth()

const tasks   = ref([])
const userMap = ref({})
const currentMember = computed(() =>
  (props.project.members || []).find(member => String(member.userId).toLowerCase() === String(user.value?.id || '').toLowerCase())
)
const canManageSprints = computed(() => ['Owner', 'Project Manager'].includes(currentMember.value?.role))
const columns = ref([])
const loading = ref(true)

const statusLabel = ['Planned', 'Active', 'Completed']
const statusStyle = (s) => [
  'bg-surface-container text-on-surface-variant',
  'bg-primary/10 text-primary',
  'bg-secondary-container/30 text-secondary',
][s]

const AVATAR_COLORS = ['#3525cd','#006a61','#684000','#ba1a1a','#0f5e9c','#6a0dad','#2e7d32','#e65100']
function avatarColor(uid) {
  return AVATAR_COLORS[(uid?.charCodeAt(0) ?? 0) % AVATAR_COLORS.length]
}

function priorityLabel(p) { return { 1: 'Thấp', 2: 'TB', 3: 'Cao' }[p] ?? '-' }
function priorityStyle(p) {
  return {
    1: 'bg-surface-container text-outline',
    2: 'bg-tertiary/10 text-tertiary',
    3: 'bg-error-container/30 text-error',
  }[p] ?? ''
}

function fmt(d) {
  return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const doneTasks = computed(() => tasks.value.filter(t => t._done).length)
const progressPct = computed(() => {
  if (!tasks.value.length) return 0
  return Math.round((doneTasks.value / tasks.value.length) * 100)
})

const activeMembers = computed(() => {
  const map = {}
  for (const t of tasks.value) {
    if (!t.assignedTo) continue
    const uid = t.assignedTo
    if (!map[uid]) {
      map[uid] = {
        userId:    uid,
        name:      userMap.value[uid]?.fullName || uid.slice(0, 8),
        taskCount: 0,
        doneCount: 0,
      }
    }
    map[uid].taskCount++
    if (t._done) map[uid].doneCount++
  }
  return Object.values(map).sort((a, b) => b.taskCount - a.taskCount)
})

onMounted(async () => {
  try {
    const [rawTasks, cols, allUsers] = await Promise.all([
      taskService.getAll({ projectId: props.project.id, sprintId: props.sprint.id }),
      taskService.getColumns(props.project.id),
      userService.getAll().catch(() => []),
    ])

    columns.value = cols || []
    userMap.value = Object.fromEntries((allUsers || []).map(u => [u.id, u]))

    // Build a set of "done" column IDs (type === 'done') and "active" (type === 'active')
    const doneColIds   = new Set(columns.value.filter(c => c.type === 'done').map(c => c.id))
    const activeColIds = new Set(columns.value.filter(c => c.type === 'active').map(c => c.id))

    tasks.value = (rawTasks || []).map(t => ({
      ...t,
      _done:   doneColIds.has(t.columnId),
      _active: activeColIds.has(t.columnId),
    }))
  } catch { /* TaskService may be offline */ }
  finally { loading.value = false }
})
</script>
