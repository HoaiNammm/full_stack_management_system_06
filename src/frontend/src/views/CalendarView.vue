<template>
  <div class="page-wrap">
    <!-- Header -->
    <section class="page-hero">
      <div>
        <h2 class="font-headline-lg text-headline-lg text-on-surface">Lịch</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Xem deadline task, sprint và milestone của tất cả dự án.</p>
      </div>

      <div class="relative z-10 mt-lg flex items-center gap-3 flex-wrap">
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
          class="app-input rounded-lg px-3 py-1.5 font-label-md text-label-md">
          <option value="">Tất cả dự án</option>
          <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>

        <!-- Sprint filter (only when a project is selected and sprints exist) -->
        <select v-if="selectedProjectId && sprints.length > 0"
          v-model="selectedSprintId" @change="applyFilters"
          class="app-input rounded-lg px-3 py-1.5 font-label-md text-label-md">
          <option value="">Tất cả sprint</option>
          <option v-for="s in sprints" :key="s.id" :value="s.id">
            {{ s.name }}{{ s.status === 1 ? ' ●' : '' }}
          </option>
        </select>
      </div>
    </section>

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
    <div v-else class="app-panel flex-1 min-h-[600px]">
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
          allDay: true,
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
          classNames: ['calendar-event-sprint'],
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
          classNames: ['calendar-event-milestone'],
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
          classNames: ['calendar-event-task'],
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
  --fc-border-color: rgb(var(--color-outline-variant));
  --fc-page-bg-color: transparent;
  --fc-neutral-bg-color: rgb(var(--color-surface-container-low));
  --fc-list-event-hover-bg-color: rgb(var(--color-surface-container-low));
  --fc-today-bg-color: rgb(var(--color-primary) / 0.07);
  --fc-event-border-color: transparent;
  color: rgb(var(--color-on-surface));
}
.fc .fc-scrollgrid,
.fc-theme-standard td,
.fc-theme-standard th {
  border-color: rgb(var(--color-outline-variant)) !important;
}
.fc .fc-col-header-cell {
  background: rgb(var(--color-surface-container-low)) !important;
}
.fc .fc-col-header-cell-cushion,
.fc .fc-daygrid-day-number {
  color: rgb(var(--color-on-surface-variant)) !important;
}
.fc .fc-daygrid-day {
  background: rgb(var(--color-surface-container-lowest) / 0.58);
}
.fc .fc-daygrid-day-frame {
  transition: background-color 160ms ease;
}
.fc .fc-daygrid-day:hover .fc-daygrid-day-frame {
  background: rgb(var(--color-primary) / 0.035);
}
.fc-toolbar-title {
  font-size: 1.1rem !important;
  font-weight: 700 !important;
  color: rgb(var(--color-on-surface));
}
.fc-button-primary {
  background-color: rgb(var(--color-primary)) !important;
  border-color: rgb(var(--color-primary)) !important;
  color: rgb(var(--color-on-primary)) !important;
  box-shadow: none !important;
}
.fc-button-primary:not(.fc-button-active):hover {
  opacity: 0.9;
}
.fc-event {
  cursor: pointer;
  font-size: 0.78rem !important;
  border-radius: 7px !important;
  padding: 2px 6px !important;
  box-shadow: none !important;
}
.fc-event.calendar-event-task {
  background-color: #b3261e !important;
  border-color: #b3261e !important;
}
.fc-event.calendar-event-sprint {
  background-color: #146cae !important;
  border-color: #146cae !important;
}
.fc-event.calendar-event-milestone {
  background-color: #c55a11 !important;
  border-color: #c55a11 !important;
}

.dark .fc {
  --fc-border-color: rgb(var(--color-outline-variant) / 0.62);
  --fc-neutral-bg-color: rgb(var(--color-surface-container));
  --fc-today-bg-color: rgb(var(--color-primary) / 0.11);
}
.dark .fc .fc-scrollgrid {
  background: rgb(var(--color-surface-container-lowest)) !important;
}
.dark .fc .fc-col-header-cell {
  background: rgb(var(--color-surface-container-high)) !important;
}
.dark .fc .fc-col-header-cell-cushion {
  color: rgb(var(--color-on-surface) / 0.86) !important;
}
.dark .fc .fc-daygrid-day {
  background: rgb(var(--color-surface-container-lowest)) !important;
}
.dark .fc .fc-day-other {
  background: rgb(var(--color-surface-container) / 0.55) !important;
}
.dark .fc .fc-daygrid-day-number {
  color: rgb(var(--color-on-surface-variant)) !important;
}
.dark .fc .fc-day-today .fc-daygrid-day-frame {
  background: rgb(var(--color-primary) / 0.12) !important;
  box-shadow: inset 0 0 0 1px rgb(var(--color-primary) / 0.22);
}
.dark .fc .fc-daygrid-day:hover .fc-daygrid-day-frame {
  background: rgb(var(--color-primary) / 0.08);
}
.dark .fc-event {
  color: #f8fafc !important;
  opacity: 0.92;
}
.dark .fc-event.calendar-event-task {
  background-color: #74252a !important;
  border-color: #74252a !important;
}
.dark .fc-event.calendar-event-sprint {
  background-color: #174c72 !important;
  border-color: #174c72 !important;
}
.dark .fc-event.calendar-event-milestone {
  background-color: #7c3f16 !important;
  border-color: #7c3f16 !important;
}
.dark .fc-list,
.dark .fc-list-day-cushion,
.dark .fc-list-table td {
  background: rgb(var(--color-surface-container-lowest)) !important;
  color: rgb(var(--color-on-surface)) !important;
  border-color: rgb(var(--color-outline-variant)) !important;
}
</style>
