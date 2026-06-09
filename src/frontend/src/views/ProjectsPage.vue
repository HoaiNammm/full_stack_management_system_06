<template>
  <div class="flex-grow px-lg py-lg max-w-7xl mx-auto w-full flex flex-col gap-lg">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h2 class="font-headline-lg text-headline-lg text-on-surface">Dự án</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Quản lý tất cả dự án của bạn.</p>
      </div>
      <button @click="showCreate = true"
        class="flex items-center gap-xs bg-primary text-on-primary px-md py-sm rounded-lg font-label-lg text-label-lg shadow-sm hover:opacity-90 transition-opacity">
        <span class="material-symbols-outlined text-[18px]">add</span>
        Tạo dự án mới
      </button>
    </div>

    <!-- Search + Filter -->
    <div class="flex items-center gap-3">
      <div class="flex items-center bg-surface-container-low rounded-full px-3 py-1.5 border border-outline-variant w-72 focus-within:border-primary focus-within:ring-1 focus-within:ring-primary transition-all">
        <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
        <input v-model="search"
          class="bg-transparent border-none outline-none ml-2 font-body-md text-body-md text-on-surface w-full placeholder:text-outline"
          placeholder="Tìm kiếm dự án..." />
      </div>
      <div class="flex gap-2">
        <button v-for="f in filters" :key="f.id" @click="activeFilter = f.id"
          class="px-3 py-1.5 rounded-full font-label-md text-label-md border transition-colors"
          :class="activeFilter === f.id
            ? 'bg-primary text-on-primary border-primary'
            : 'border-outline-variant text-on-surface-variant hover:bg-surface-container-high'">
          {{ f.label }}
        </button>
      </div>
    </div>

    <!-- Loading / Error -->
    <div v-if="loading" class="flex items-center justify-center py-xl text-on-surface-variant gap-2">
      <span class="material-symbols-outlined animate-spin">progress_activity</span>
      Đang tải dự án...
    </div>
    <div v-else-if="error" class="bg-error-container/20 border border-error/30 rounded-xl p-md text-error font-label-md flex items-center gap-2">
      <span class="material-symbols-outlined">warning</span>{{ error }}
    </div>

    <!-- Project Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-md">
      <div v-for="p in filteredProjects" :key="p.id"
        class="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-sm hover:shadow-md transition-all cursor-pointer group overflow-hidden relative">
        <!-- Color bar -->
        <div class="h-1.5 w-full" :style="{ backgroundColor: p.color || '#3525cd' }"></div>
        <!-- Delete button (top-right, shown on hover) -->
        <button @click.stop="deleteProject(p)"
          class="absolute top-3 right-3 opacity-0 group-hover:opacity-100 p-1 rounded-lg text-on-surface-variant hover:text-error hover:bg-error-container/20 transition-all z-10"
          title="Xóa dự án">
          <span class="material-symbols-outlined text-[18px]">delete</span>
        </button>
        <div class="p-md" @click="$router.push(`/projects/${p.id}`)">
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-lg flex items-center justify-center font-bold font-label-lg text-white flex-shrink-0"
                :style="{ backgroundColor: p.color || '#3525cd' }">{{ initials(p.name) }}</div>
              <div class="min-w-0">
                <h3 class="font-headline-sm text-headline-sm text-on-surface group-hover:text-primary transition-colors truncate">{{ p.name }}</h3>
                <p class="font-body-md text-body-md text-on-surface-variant truncate">{{ p.description || 'Không có mô tả' }}</p>
              </div>
            </div>
            <span class="text-[11px] font-bold px-2 py-0.5 rounded-full flex-shrink-0 ml-2"
              :class="p.status === 1 ? 'bg-secondary-container/30 text-secondary' : 'bg-surface-container text-on-surface-variant'">
              {{ p.status === 1 ? 'Active' : 'Draft' }}
            </span>
          </div>

          <div class="flex items-center gap-1 text-on-surface-variant mb-3">
            <span class="material-symbols-outlined text-[14px]">calendar_today</span>
            <span class="font-label-sm text-label-sm">
              {{ p.startDate ? formatDate(p.startDate) : '—' }} → {{ p.endDate ? formatDate(p.endDate) : '—' }}
            </span>
          </div>

          <div class="flex justify-between items-center pt-3 border-t border-outline-variant/50">
            <div class="flex items-center gap-1 text-on-surface-variant">
              <span class="material-symbols-outlined text-[16px]">group</span>
              <span class="font-label-md text-label-md">{{ p.memberCount ?? 0 }} thành viên</span>
            </div>
            <div v-if="p.activeSprint" class="flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px] text-secondary">sprint</span>
              <span class="font-label-sm text-label-sm text-secondary">{{ p.activeSprint.name }}</span>
            </div>
            <span v-else class="font-label-sm text-label-sm text-outline">Chưa có sprint</span>
          </div>
        </div>
      </div>

      <!-- Add card -->
      <div @click="showCreate = true"
        class="rounded-xl border-2 border-dashed border-outline-variant hover:border-primary hover:bg-primary/5 transition-all cursor-pointer flex flex-col items-center justify-center gap-2 p-xl min-h-[180px] group">
        <div class="w-10 h-10 rounded-full bg-surface-container-high group-hover:bg-primary/10 flex items-center justify-center transition-colors">
          <span class="material-symbols-outlined text-on-surface-variant group-hover:text-primary transition-colors">add</span>
        </div>
        <span class="font-label-lg text-label-lg text-on-surface-variant group-hover:text-primary transition-colors">Tạo dự án mới</span>
      </div>
    </div>
  </div>

  <CreateProjectModal v-if="showCreate" @close="showCreate = false" @created="handleCreated" />
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import CreateProjectModal from '../components/CreateProjectModal.vue'
import { projectService } from '../services/api'

const router      = useRouter()
const route       = useRoute()
const showCreate  = ref(false)
const search      = ref(route.query.search || '')
const activeFilter = ref('all')

watch(() => route.query.search, q => { if (q) search.value = q })
const loading     = ref(true)
const error       = ref('')
const projects    = ref([])

const filters = [
  { id: 'all',    label: 'Tất cả' },
  { id: 'active', label: 'Active' },
  { id: 'draft',  label: 'Draft' },
]

function initials(name) {
  return (name || '').split(' ').map(w => w[0]).slice(0, 3).join('').toUpperCase() || 'PR'
}

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('vi-VN')
}

const filteredProjects = computed(() => {
  let list = projects.value
  if (activeFilter.value === 'active') list = list.filter(p => p.status === 1)
  if (activeFilter.value === 'draft')  list = list.filter(p => p.status === 0 || p.status === null)
  if (search.value) list = list.filter(p => p.name.toLowerCase().includes(search.value.toLowerCase()))
  return list
})

async function loadProjects() {
  loading.value = true
  error.value   = ''
  try {
    const list = await projectService.getAll() || []
    // Fetch sprints for each project in parallel
    const sprintResults = await Promise.allSettled(
      list.map(p => projectService.getSprints(p.id))
    )
    const mapped = list.map((p, i) => {
      const r = sprintResults[i]
      const allSprints = r.status === 'fulfilled' ? (r.value || []) : []
      const activeSprint = allSprints.find(s => s.status === 1) || null
      // Derive effective status: if has active sprint → Active, regardless of DB value
      const effectiveStatus = activeSprint ? 1 : p.status
      return { ...p, status: effectiveStatus, activeSprint }
    })
    projects.value = mapped

    // Silently fix stale Draft status in DB for projects that actually have an active sprint
    mapped.forEach(p => {
      if (p.activeSprint && p.status === 1) {
        const orig = list.find(x => x.id === p.id)
        if (orig && orig.status !== 1) {
          projectService.update(p.id, {
            name: p.name, description: p.description || '',
            status: 1, color: p.color,
            startDate: p.startDate, endDate: p.endDate,
          }).catch(() => {})
        }
      }
    })
  } catch (e) {
    error.value = 'Không thể tải dự án. Kiểm tra ProjectService đang chạy trên cổng 5131.'
  } finally {
    loading.value = false
  }
}

async function deleteProject(p) {
  if (!confirm(`Xóa dự án "${p.name}"?\nThao tác này không thể hoàn tác.`)) return
  try {
    await projectService.delete(p.id)
    projects.value = projects.value.filter(x => x.id !== p.id)
  } catch (e) {
    alert('Không thể xóa: ' + (e.response?.data?.message || e.message))
  }
}

async function handleCreated(newProject) {
  showCreate.value = false
  // Reload from server to get server-generated ID
  await loadProjects()
  if (newProject?.id) router.push(`/projects/${newProject.id}`)
}

onMounted(loadProjects)
</script>
