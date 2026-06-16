<template>
  <div class="page-wrap">
    <section class="project-overview-shell">
      <div class="relative z-10 flex flex-col gap-lg lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="project-eyebrow">Danh mục dự án</p>
          <h2 class="font-headline-lg text-headline-lg text-on-surface">Dự án</h2>
          <p class="mt-1 max-w-2xl font-body-lg text-body-lg text-on-surface-variant">
            Theo dõi danh sách dự án, sprint đang chạy, thành viên và deadline từ ProjectService.
          </p>
        </div>
        <button @click="showCreate = true" class="app-button-primary">
          <span class="material-symbols-outlined text-[18px]">add</span>
          Tạo dự án mới
        </button>
      </div>

      <div class="relative z-10 mt-lg grid grid-cols-1 gap-md md:grid-cols-3">
        <RouterLink v-for="item in summaryCards" :key="item.label" :to="item.to" class="overview-widget group">
          <div class="overview-widget-icon" :class="item.iconTone">
            <span class="material-symbols-outlined">{{ item.icon }}</span>
          </div>
          <div class="min-w-0 flex-1">
            <p>{{ item.label }}</p>
            <strong>{{ item.value }}</strong>
            <span>{{ item.caption }}</span>
            <div class="overview-progress">
              <i :style="{ width: `${item.percent}%` }"></i>
            </div>
          </div>
          <span class="material-symbols-outlined text-on-surface-variant transition group-hover:translate-x-1 group-hover:text-primary">arrow_forward</span>
        </RouterLink>
      </div>
    </section>

    <div class="toolbar-panel">
      <div class="flex w-full items-center rounded-full px-3 py-1.5 app-input lg:w-96">
        <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
        <input v-model="search"
          class="ml-2 w-full border-none bg-transparent font-body-md text-body-md text-on-surface outline-none placeholder:text-outline"
          placeholder="Tìm kiếm dự án..." />
      </div>
      <div class="flex flex-wrap gap-2">
        <button v-for="f in filters" :key="f.id" @click="activeFilter = f.id"
          class="rounded-full border px-3 py-1.5 font-label-md text-label-md transition-colors"
          :class="activeFilter === f.id
            ? 'border-primary bg-primary text-on-primary'
            : 'border-outline-variant text-on-surface-variant hover:bg-surface-container-high'">
          {{ f.label }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center gap-2 py-xl text-on-surface-variant">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>
      Đang tải dự án...
    </div>
    <div v-else-if="error" class="flex items-center gap-2 rounded-xl border border-error/30 bg-error-container/20 p-md font-label-md text-error">
      <span class="material-symbols-outlined">warning</span>{{ error }}
    </div>

    <div v-else class="grid grid-cols-1 gap-lg md:grid-cols-2 xl:grid-cols-3">
      <article v-for="p in filteredProjects" :key="p.id"
        @click="$router.push(`/projects/${p.id}`)"
        class="app-card interactive-card group overflow-hidden">
        <div class="relative h-40 overflow-hidden">
          <img :src="p.coverImage" class="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" alt="" />
          <div class="absolute inset-0 bg-gradient-to-t from-black/60 via-black/15 to-transparent"></div>
          <div class="absolute bottom-3 left-3 flex items-center gap-2">
            <div class="flex h-12 w-12 items-center justify-center rounded-xl font-bold text-white shadow-lg ring-2 ring-white/30"
              :style="{ backgroundColor: p.color || '#3525cd' }">
              {{ initials(p.name) }}
            </div>
            <span class="rounded-full bg-white/90 px-2.5 py-1 text-[11px] font-bold text-slate-900 backdrop-blur">
              {{ p.activeSprint ? 'Sprint đang chạy' : 'Workspace' }}
            </span>
          </div>
        </div>

        <div class="flex flex-col gap-md p-md">
          <div class="flex items-start justify-between gap-sm">
            <div class="min-w-0">
              <h3 class="truncate font-headline-sm text-headline-sm text-on-surface transition-colors group-hover:text-primary">{{ p.name }}</h3>
              <p class="line-clamp-2 font-body-md text-body-md text-on-surface-variant">{{ p.description || 'Chưa có mô tả' }}</p>
            </div>
            <button @click.stop="deleteProject(p)"
              class="rounded-lg p-1.5 text-on-surface-variant transition-all hover:bg-error-container/20 hover:text-error"
              title="Xóa dự án">
              <span class="material-symbols-outlined text-[18px]">delete</span>
            </button>
          </div>

          <div class="flex flex-wrap gap-xs">
            <span class="soft-badge" :class="p.status === 1 ? 'bg-secondary-container/30 text-secondary' : 'bg-surface-container text-on-surface-variant'">
              {{ p.status === 1 ? 'Đang chạy' : 'Bản nháp' }}
            </span>
            <span v-if="p.activeSprint" class="soft-badge bg-primary/10 text-primary">{{ p.activeSprint.name }}</span>
            <span v-else class="soft-badge bg-surface-container text-on-surface-variant">Chưa có sprint</span>
          </div>

          <div>
            <div class="mb-1 flex items-center justify-between">
              <span class="font-label-sm text-label-sm text-on-surface-variant">Tiến độ</span>
              <span class="font-label-sm text-label-sm text-primary">{{ projectProgress(p) }}%</span>
            </div>
            <div class="h-2.5 overflow-hidden rounded-full bg-surface-container">
              <div class="h-full rounded-full bg-primary transition-all" :style="{ width: `${projectProgress(p)}%` }"></div>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-sm border-t border-outline-variant/50 pt-sm">
            <div class="rounded-lg bg-surface-container-low p-sm">
              <p class="font-label-sm text-label-sm text-on-surface-variant">Thành viên</p>
              <p class="font-label-lg text-label-lg text-on-surface">{{ p.memberCount ?? 0 }}</p>
            </div>
            <div class="rounded-lg bg-surface-container-low p-sm">
              <p class="font-label-sm text-label-sm text-on-surface-variant">Deadline</p>
              <p class="truncate font-label-lg text-label-lg text-on-surface">{{ p.endDate ? formatDate(p.endDate) : 'Chưa đặt' }}</p>
            </div>
          </div>
        </div>
      </article>

      <button @click="showCreate = true"
        class="group flex min-h-[260px] cursor-pointer flex-col items-center justify-center gap-2 rounded-2xl border-2 border-dashed border-outline-variant p-xl transition-all hover:border-primary hover:bg-primary/5">
        <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-surface-container-high transition-colors group-hover:bg-primary/10">
          <span class="material-symbols-outlined text-on-surface-variant transition-colors group-hover:text-primary">add</span>
        </div>
        <span class="font-label-lg text-label-lg text-on-surface-variant transition-colors group-hover:text-primary">Tạo dự án mới</span>
      </button>
    </div>
  </div>

  <CreateProjectModal v-if="showCreate" @close="showCreate = false" @created="handleCreated" />
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import CreateProjectModal from '../components/CreateProjectModal.vue'
import { projectService } from '../services/api'
import { getProjectCover } from '../services/visualAssets'

const router = useRouter()
const route = useRoute()
const showCreate = ref(false)
const search = ref(route.query.search || '')
const activeFilter = ref(route.query.filter || 'all')
const loading = ref(true)
const error = ref('')
const projects = ref([])

watch(() => route.query.search, q => { if (q) search.value = q })
watch(() => route.query.filter, q => { activeFilter.value = q || 'all' })

const filters = [
  { id: 'all', label: 'Tất cả' },
  { id: 'active', label: 'Đang chạy' },
  { id: 'draft', label: 'Bản nháp' },
]

const activeCount = computed(() => projects.value.filter(p => p.status === 1).length)
const sprintCount = computed(() => projects.value.filter(p => p.activeSprint).length)

const summaryCards = computed(() => [
  {
    label: 'Tổng dự án',
    value: projects.value.length,
    icon: 'folder_shared',
    caption: 'Tất cả không gian làm việc',
    iconTone: 'is-primary',
    percent: projects.value.length ? 100 : 8,
    to: '/projects',
  },
  {
    label: 'Đang chạy',
    value: activeCount.value,
    icon: 'rocket_launch',
    caption: 'Dự án active',
    iconTone: 'is-secondary',
    percent: projects.value.length ? Math.round((activeCount.value / projects.value.length) * 100) : 8,
    to: { path: '/projects', query: { filter: 'active' } },
  },
  {
    label: 'Có sprint',
    value: sprintCount.value,
    icon: 'sprint',
    caption: 'Sprint đang quản lý',
    iconTone: 'is-tertiary',
    percent: projects.value.length ? Math.round((sprintCount.value / projects.value.length) * 100) : 8,
    to: '/calendar',
  },
])

const filteredProjects = computed(() => {
  let list = projects.value
  if (activeFilter.value === 'active') list = list.filter(p => p.status === 1)
  if (activeFilter.value === 'draft') list = list.filter(p => p.status === 0 || p.status === null)
  if (search.value) {
    const keyword = search.value.toLowerCase()
    list = list.filter(p => [p.name, p.description].some(v => (v || '').toLowerCase().includes(keyword)))
  }
  return list
})

function initials(name) {
  return (name || '').split(' ').filter(Boolean).map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'PR'
}

function formatDate(d) {
  return new Date(d).toLocaleDateString('vi-VN')
}

function projectProgress(project) {
  const raw = project.progressPercent ?? project.progress ?? project.completionPercent ?? 0
  const value = Number(raw)
  if (!Number.isFinite(value)) return 0
  return Math.max(0, Math.min(100, Math.round(value)))
}

async function loadProjects() {
  loading.value = true
  error.value = ''
  try {
    const list = await projectService.getAll() || []
    const sprintResults = await Promise.allSettled(list.map(p => projectService.getSprints(p.id)))
    projects.value = list.map((p, i) => {
      const allSprints = sprintResults[i]?.status === 'fulfilled' ? (sprintResults[i].value || []) : []
      const activeSprint = allSprints.find(s => s.status === 1) || null
      return { ...p, status: activeSprint ? 1 : p.status, activeSprint, coverImage: getProjectCover(p, i) }
    })
  } catch {
    error.value = 'Không thể tải dự án. Kiểm tra ProjectService đang chạy.'
  } finally {
    loading.value = false
  }
}

async function deleteProject(p) {
  if (!confirm(`Xóa dự án "${p.name}"?`)) return
  try {
    await projectService.delete(p.id)
    projects.value = projects.value.filter(x => x.id !== p.id)
  } catch (e) {
    alert('Không thể xóa: ' + (e.response?.data?.message || e.message))
  }
}

async function handleCreated(newProject) {
  showCreate.value = false
  await loadProjects()
  if (newProject?.id) router.push(`/projects/${newProject.id}`)
}

onMounted(loadProjects)
</script>

<style scoped>
.project-overview-shell {
  position: relative;
  overflow: hidden;
  border-radius: 26px;
  background:
    radial-gradient(circle at 12% 0%, rgb(var(--color-primary) / 0.13), transparent 28rem),
    radial-gradient(circle at 100% 20%, rgb(var(--color-secondary) / 0.11), transparent 22rem),
    rgb(var(--color-surface-container-lowest));
  padding: clamp(20px, 2.4vw, 32px);
  box-shadow: 0 18px 50px rgb(var(--color-outline) / 0.12);
}

.project-eyebrow {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: rgb(var(--color-primary));
}

.overview-widget {
  display: flex;
  align-items: center;
  gap: 14px;
  border-radius: 20px;
  background: rgb(var(--color-surface-container-low) / 0.86);
  padding: 16px;
  box-shadow: inset 0 1px 0 rgb(var(--color-surface-bright) / 0.55);
  transition: transform 180ms ease, background 180ms ease, box-shadow 180ms ease;
}

.overview-widget:hover {
  transform: translateY(-2px);
  background: rgb(var(--color-surface-container-high));
  box-shadow: 0 18px 42px rgb(var(--color-outline) / 0.14);
}

.overview-widget-icon {
  display: grid;
  width: 52px;
  height: 52px;
  flex: none;
  place-items: center;
  border-radius: 16px;
}

.overview-widget-icon .material-symbols-outlined {
  font-size: 24px;
}

.overview-widget-icon.is-primary {
  background: rgb(var(--color-primary) / 0.12);
  color: rgb(var(--color-primary));
}

.overview-widget-icon.is-secondary {
  background: rgb(var(--color-secondary) / 0.12);
  color: rgb(var(--color-secondary));
}

.overview-widget-icon.is-tertiary {
  background: rgb(var(--color-tertiary) / 0.12);
  color: rgb(var(--color-tertiary));
}

.overview-widget p {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .06em;
  text-transform: uppercase;
  color: rgb(var(--color-on-surface-variant));
}

.overview-widget strong {
  display: block;
  margin-top: 4px;
  font-size: 34px;
  line-height: 1;
  font-weight: 950;
  color: rgb(var(--color-on-surface));
}

.overview-widget span:not(.material-symbols-outlined) {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  color: rgb(var(--color-on-surface-variant));
}

.overview-progress {
  margin-top: 10px;
  height: 6px;
  overflow: hidden;
  border-radius: 999px;
  background: rgb(var(--color-surface-container));
}

.overview-progress i {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgb(var(--color-primary)), rgb(var(--color-secondary)));
}
</style>
