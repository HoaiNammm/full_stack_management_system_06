<template>
  <div class="flex-grow px-lg py-lg max-w-7xl mx-auto w-full flex flex-col gap-lg">
    <!-- Header -->
    <div class="flex flex-wrap justify-between items-center gap-3">
      <div>
        <h2 class="font-headline-lg text-headline-lg text-on-surface">Lịch</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Xem deadline task, sprint và milestone của tất cả dự án.</p>
      </div>

      <div class="flex items-center gap-3 flex-wrap">
        <!-- Legend -->
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-[#ba1a1a]"></div>
            <span class="font-label-sm text-label-sm text-on-surface-variant">Task deadline</span>
          </div>
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-[#0f5e9c]"></div>
            <span class="font-label-sm text-label-sm text-on-surface-variant">Sprint</span>
          </div>
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-[#e65100]"></div>
            <span class="font-label-sm text-label-sm text-on-surface-variant">Milestone</span>
          </div>
        </div>

        <!-- Project filter -->
        <select v-model="selectedProjectId" @change="onProjectChange"
          class="bg-surface-container-low border border-outline-variant rounded-lg px-3 py-1.5 font-label-md text-label-md text-on-surface outline-none focus:border-primary transition-all">
          <option value="">Tất cả dự án</option>
          <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>

        <!-- Sprint filter (only when a project is selected and sprints exist) -->
        <select v-if="selectedProjectId && sprints.length > 0"
          v-model="selectedSprintId" @change="applyFilters"
          class="bg-surface-container-low border border-outline-variant rounded-lg px-3 py-1.5 font-label-md text-label-md text-on-surface outline-none focus:border-primary transition-all">
          <option value="">Tất cả sprint</option>
          <option v-for="s in sprints" :key="s.id" :value="s.id">
            {{ s.name }}{{ s.status === 1 ? ' ●' : '' }}
          </option>
        </select>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-xl text-on-surface-variant gap-2">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>
      Đang tải lịch...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="bg-error-container/20 border border-error/30 rounded-xl p-md text-error font-label-md flex items-center gap-2">
      <span class="material-symbols-outlined">warning</span>{{ error }}
    </div>

    <!-- Calendar -->
    <div v-else class="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-sm overflow-hidden flex-1 min-h-[600px]">
      <FullCalendar
        ref="calRef"
        :options="calendarOptions"
        class="h-full"
      />
    </div>
  </div>

  <CalendarEventPopup v-if="selectedEvent" :event="selectedEvent" @close="selectedEvent = null" />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin     from '@fullcalendar/daygrid'
import listPlugin        from '@fullcalendar/list'
import interactionPlugin from '@fullcalendar/interaction'
import CalendarEventPopup from '../components/calendar/CalendarEventPopup.vue'
import { projectService, taskService } from '../services/api'

const loading           = ref(true)
const error             = ref('')
const projects          = ref([])
const sprints           = ref([])
const allEvents         = ref([])
const filteredEvents    = ref([])
const selectedProjectId = ref('')
const selectedSprintId  = ref('')
const selectedEvent     = ref(null)
const calRef            = ref(null)

// ── Calendar config ──────────────────────────────────────────────
const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, listPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: 'vi',
  firstDay: 1,
  headerToolbar: {
    left:   'prev,next today',
    center: 'title',
    right:  'dayGridMonth,listMonth',
  },
  buttonText: {
    today:     'Hôm nay',
    month:     'Tháng',
    listMonth: 'Danh sách',
  },
  events: filteredEvents.value,
  eventClick: handleEventClick,
  eventTimeFormat: { hour: '2-digit', minute: '2-digit', hour12: false },
  height: 'auto',
  aspectRatio: 1.8,
  eventDisplay: 'block',
  dayMaxEvents: 4,
}))

function handleEventClick(info) {
  selectedEvent.value = info.event
}

// ── Filtering ────────────────────────────────────────────────────
function applyFilters() {
  let evts = allEvents.value

  if (selectedProjectId.value) {
    evts = evts.filter(e => e.extendedProps?.projectId === selectedProjectId.value)
  }

  if (selectedSprintId.value) {
    evts = evts.filter(e => {
      // Always keep sprint & milestone events for that project
      if (e.extendedProps?.type !== 'task') return true
      // Only keep task events that belong to the selected sprint
      return e.extendedProps?.sprintId === selectedSprintId.value
    })
  }

  filteredEvents.value = evts
}

async function onProjectChange() {
  selectedSprintId.value = ''
  sprints.value = []

  if (selectedProjectId.value) {
    sprints.value = await projectService.getSprints(selectedProjectId.value).catch(() => [])
    // Auto-select active sprint if one exists
    const active = sprints.value.find(s => s.status === 1)
    if (active) selectedSprintId.value = active.id
  }

  applyFilters()
}

// ── Data loading ─────────────────────────────────────────────────
async function loadCalendarData() {
  loading.value = true
  error.value   = ''
  const events  = []

  try {
    projects.value = await projectService.getAll() || []
  } catch {
    error.value = 'Không thể tải dự án. Kiểm tra ProjectService.'
    loading.value = false
    return
  }

  await Promise.all(projects.value.map(async (p) => {
    const pId    = p.id
    const pName  = p.name

    // Sprints
    try {
      const pSprints = await projectService.getSprints(pId) || []
      for (const s of pSprints) {
        if (!s.startDate) continue
        events.push({
          id:    `sprint-${s.id}`,
          title: `🏃 ${s.name}`,
          start: s.startDate,
          end:   s.endDate || s.startDate,
          backgroundColor: '#0f5e9c',
          borderColor:     '#0f5e9c',
          textColor:       '#ffffff',
          extendedProps: {
            type:        'sprint',
            projectId:   pId,
            projectName: pName,
            sprintId:    s.id,
            description: s.goal || s.description,
          },
        })
      }
    } catch { /* ignore */ }

    // Milestones
    try {
      const milestones = await projectService.getMilestones(pId) || []
      for (const m of milestones) {
        if (!m.targetDate) continue
        events.push({
          id:    `ms-${m.id}`,
          title: `🏁 ${m.name}`,
          start: m.targetDate,
          allDay: true,
          backgroundColor: '#e65100',
          borderColor:     '#e65100',
          textColor:       '#ffffff',
          extendedProps: {
            type:        'milestone',
            projectId:   pId,
            projectName: pName,
            description: m.description,
          },
        })
      }
    } catch { /* ignore */ }

    // Task deadlines
    try {
      const tasks = await taskService.getAll({ projectId: pId }) || []
      for (const t of tasks) {
        const dl = t.dueDate || t.deadline
        if (!dl || t.deletedAt) continue
        events.push({
          id:    `task-${t.id}`,
          title: `📌 ${t.title}`,
          start: dl,
          allDay: true,
          backgroundColor: '#ba1a1a',
          borderColor:     '#ba1a1a',
          textColor:       '#ffffff',
          extendedProps: {
            type:        'task',
            projectId:   pId,
            projectName: pName,
            sprintId:    t.sprintId || null,
            description: t.description,
          },
        })
      }
    } catch { /* TaskService may be offline */ }
  }))

  allEvents.value      = events
  filteredEvents.value = events
  loading.value        = false
}

onMounted(loadCalendarData)
</script>

<style>
/* FullCalendar custom theming */
.fc {
  --fc-border-color: rgba(0,0,0,0.08);
  --fc-today-bg-color: rgba(53,37,205,0.06);
  --fc-event-border-color: transparent;
}
.fc-toolbar-title {
  font-size: 1.1rem !important;
  font-weight: 700 !important;
}
.fc-button-primary {
  background-color: #3525cd !important;
  border-color: #3525cd !important;
}
.fc-button-primary:not(.fc-button-active):hover {
  background-color: #2a1db0 !important;
}
.fc-event {
  cursor: pointer;
  font-size: 0.78rem !important;
  border-radius: 4px !important;
  padding: 1px 4px !important;
}
</style>
