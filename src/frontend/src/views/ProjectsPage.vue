<template>
  <div class="page-wrap">
    <section class="page-hero">
      <div class="relative z-10 flex flex-col gap-lg lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="page-eyebrow">Project portfolio</p>
          <h2 class="font-headline-lg text-headline-lg text-on-surface">Dự án</h2>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-1 max-w-2xl">
            Theo dõi danh sách dự án, sprint đang chạy, thành viên và deadline trên dữ liệu ProjectService.
          </p>
        </div>
        <button @click="showCreate = true" class="app-button-primary">
          <span class="material-symbols-outlined text-[18px]">add</span>
          Tạo dự án mới
        </button>
      </div>
      <div class="relative z-10 mt-lg grid grid-cols-1 md:grid-cols-3 gap-md">
        <div v-for="item in summaryCards" :key="item.label" class="rounded-xl border border-outline-variant bg-surface-container-low p-md">
          <div class="flex items-center justify-between">
            <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ item.label }}</span>
            <span class="material-symbols-outlined text-primary text-[20px]">{{ item.icon }}</span>
          </div>
          <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ item.value }}</p>
        </div>
      </div>
    </section>

    <div class="toolbar-panel">
      <div class="flex items-center app-input rounded-full px-3 py-1.5 w-full lg:w-96">
        <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
        <input v-model="search"
          class="bg-transparent border-none outline-none ml-2 font-body-md text-body-md text-on-surface w-full placeholder:text-outline"
          placeholder="Tìm kiếm dự án..." />
      </div>
      <div class="flex flex-wrap gap-2">
        <button v-for="f in filters" :key="f.id" @click="activeFilter = f.id"
          class="px-3 py-1.5 rounded-full font-label-md text-label-md border transition-colors"
          :class="activeFilter === f.id
            ? 'bg-primary text-on-primary border-primary'
            : 'border-outline-variant text-on-surface-variant hover:bg-surface-container-high'">
          {{ f.label }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center py-xl text-on-surface-variant gap-2">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>
      Đang tải dự án...
    </div>
    <div v-else-if="error" class="bg-error-container/20 border border-error/30 rounded-xl p-md text-error font-label-md flex items-center gap-2">
      <span class="material-symbols-outlined">warning</span>{{ error }}
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-lg">
      <article v-for="p in filteredProjects" :key="p.id"
        @click="$router.push(`/projects/${p.id}`)"
        class="app-card interactive-card group overflow-hidden">
        <div class="h-2" :style="{ backgroundColor: p.color || '#3525cd' }"></div>
        <div class="p-md flex flex-col gap-md">
          <div class="flex items-start justify-between gap-sm">
            <div class="flex items-center gap-sm min-w-0">
              <div class="w-12 h-12 rounded-xl flex items-center justify-center font-bold text-white flex-shrink-0"
                :style="{ backgroundColor: p.color || '#3525cd' }">
                {{ initials(p.name) }}
              </div>
              <div class="min-w-0">
                <h3 class="font-headline-sm text-headline-sm text-on-surface group-hover:text-primary transition-colors truncate">{{ p.name }}</h3>
                <p class="font-body-md text-body-md text-on-surface-variant line-clamp-2">{{ p.description || 'Chưa có mô tả' }}</p>
              </div>
            </div>
            <button @click.stop="deleteProject(p)"
              class="p-1.5 rounded-lg text-on-surface-variant hover:text-error hover:bg-error-container/20 transition-all"
              title="Xóa dự án">
              <span class="material-symbols-outlined text-[18px]">delete</span>
            </button>
          </div>

          <div class="flex flex-wrap gap-xs">
            <span class="soft-badge" :class="p.status === 1 ? 'bg-secondary-container/30 text-secondary' : 'bg-surface-container text-on-surface-variant'">
              {{ p.status === 1 ? 'Active' : 'Draft' }}
            </span>
            <span v-if="p.activeSprint" class="soft-badge bg-primary/10 text-primary">{{ p.activeSprint.name }}</span>
            <span v-else class="soft-badge bg-surface-container text-on-surface-variant">Chưa có sprint</span>
          </div>

          <div>
            <div class="flex items-center justify-between mb-1">
              <span class="font-label-sm text-label-sm text-on-surface-variant">Tiến độ</span>
              <span class="font-label-sm text-label-sm text-primary">{{ projectProgress(p) }}%</span>
            </div>
            <div class="h-2.5 rounded-full bg-surface-container overflow-hidden">
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
              <p class="font-label-lg text-label-lg text-on-surface truncate">{{ p.endDate ? formatDate(p.endDate) : 'Chưa đặt' }}</p>
            </div>
          </div>
        </div>
      </article>

      <button @click="showCreate = true"
        class="rounded-2xl border-2 border-dashed border-outline-variant hover:border-primary hover:bg-primary/5 transition-all cursor-pointer flex flex-col items-center justify-center gap-2 p-xl min-h-[260px] group">
        <div class="w-12 h-12 rounded-xl bg-surface-container-high group-hover:bg-primary/10 flex items-center justify-center transition-colors">
          <span class="material-symbols-outlined text-on-surface-variant group-hover:text-primary transition-colors">add</span>
        </div>
        <span class="font-label-lg text-label-lg text-on-surface-variant group-hover:text-primary transition-colors">Tạo dự án mới</span>
      </button>
    </div>
  </div>

  <CreateProjectModal v-if="showCreate" @close="showCreate = false" @created="handleCreated" />
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import CreateProjectModal from '../components/CreateProjectModal.vue'
import { projectService } from '../services/api'

const router = useRouter()
const route = useRoute()
const showCreate = ref(false)
const search = ref(route.query.search || '')
const activeFilter = ref('all')
const loading = ref(true)
const error = ref('')
const projects = ref([])

watch(() => route.query.search, q => { if (q) search.value = q })

const filters = [
  { id: 'all', label: 'Tất cả' },
  { id: 'active', label: 'Active' },
  { id: 'draft', label: 'Draft' },
]

const summaryCards = computed(() => [
  { label: 'Tổng dự án', value: projects.value.length, icon: 'folder_shared' },
  { label: 'Đang chạy', value: projects.value.filter(p => p.status === 1).length, icon: 'rocket_launch' },
  { label: 'Có sprint', value: projects.value.filter(p => p.activeSprint).length, icon: 'sprint' },
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
      return { ...p, status: activeSprint ? 1 : p.status, activeSprint }
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
