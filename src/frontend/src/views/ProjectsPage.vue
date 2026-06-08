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

    <!-- Project Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-md">
      <div v-for="p in filteredProjects" :key="p.id"
        @click="$router.push(`/projects/${p.id}`)"
        class="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-sm hover:shadow-md transition-all cursor-pointer group overflow-hidden">
        <!-- Color bar -->
        <div class="h-1.5 w-full" :style="{ backgroundColor: p.color }"></div>
        <div class="p-md">
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-lg flex items-center justify-center font-bold font-label-lg text-white flex-shrink-0"
                :style="{ backgroundColor: p.color }">{{ p.initials }}</div>
              <div class="min-w-0">
                <h3 class="font-headline-sm text-headline-sm text-on-surface group-hover:text-primary transition-colors truncate">{{ p.name }}</h3>
                <p class="font-body-md text-body-md text-on-surface-variant truncate">{{ p.desc }}</p>
              </div>
            </div>
            <span class="text-[11px] font-bold px-2 py-0.5 rounded-full flex-shrink-0 ml-2"
              :class="p.status === 1 ? 'bg-secondary-container/30 text-secondary' : 'bg-surface-container text-on-surface-variant'">
              {{ p.status === 1 ? 'Active' : 'Draft' }}
            </span>
          </div>

          <div class="flex items-center gap-1 text-on-surface-variant mb-3">
            <span class="material-symbols-outlined text-[14px]">calendar_today</span>
            <span class="font-label-sm text-label-sm">{{ p.startDate }} → {{ p.endDate }}</span>
          </div>

          <div class="flex justify-between items-center pt-3 border-t border-outline-variant/50">
            <div class="flex items-center gap-1 text-on-surface-variant">
              <span class="material-symbols-outlined text-[16px]">group</span>
              <span class="font-label-md text-label-md">{{ p.memberCount }} thành viên</span>
            </div>
            <div v-if="p.sprint" class="flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px] text-secondary">sprint</span>
              <span class="font-label-sm text-label-sm text-secondary">{{ p.sprint }}</span>
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
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import CreateProjectModal from '../components/CreateProjectModal.vue'

const router = useRouter()
const showCreate = ref(false)
const search = ref('')
const activeFilter = ref('all')

const filters = [
  { id: 'all', label: 'Tất cả' },
  { id: 'active', label: 'Active' },
  { id: 'draft', label: 'Draft' },
]

const projects = ref([
  { id: 1, initials: 'PAY', color: '#3525cd', name: 'Payment Gateway Refactor', desc: 'Cập nhật API v3.0', memberCount: 5, sprint: 'Sprint 3', status: 1, startDate: '01/05/2026', endDate: '30/07/2026' },
  { id: 2, initials: 'MBL', color: '#684000', name: 'Mobile App - Release Q3', desc: 'Tích hợp Push Notifications', memberCount: 3, sprint: 'Sprint 2', status: 1, startDate: '15/04/2026', endDate: '30/09/2026' },
  { id: 3, initials: 'CRM', color: '#006a61', name: 'Customer Portal', desc: 'Thiết kế UI/UX mới', memberCount: 2, sprint: null, status: 0, startDate: '01/06/2026', endDate: '31/12/2026' },
])

const filteredProjects = computed(() => {
  let list = projects.value
  if (activeFilter.value === 'active') list = list.filter(p => p.status === 1)
  if (activeFilter.value === 'draft') list = list.filter(p => p.status === 0)
  if (search.value) list = list.filter(p => p.name.toLowerCase().includes(search.value.toLowerCase()))
  return list
})

function handleCreated(p) {
  projects.value.push(p)
  showCreate.value = false
  router.push(`/projects/${p.id}`)
}
</script>
