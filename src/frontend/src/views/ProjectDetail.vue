<template>
  <div class="flex flex-col h-full overflow-hidden">
    <!-- Page Header -->
    <div class="px-lg pt-6 pb-0 border-b border-outline-variant bg-surface-container-lowest flex-shrink-0">
      <div class="max-w-7xl mx-auto">
        <!-- Breadcrumb -->
        <div class="flex items-center gap-2 mb-4">
          <button @click="$router.push('/projects')"
            class="text-on-surface-variant hover:text-primary flex items-center gap-1 font-label-md text-label-md transition-colors">
            <span class="material-symbols-outlined text-[16px]">arrow_back</span>
            Dự án
          </button>
          <span class="text-outline font-label-md text-label-md">/</span>
          <span class="font-label-md text-label-md text-on-surface truncate max-w-[200px]">{{ project.name }}</span>
        </div>

        <!-- Project info -->
        <div class="flex items-start gap-md mb-4">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center font-bold text-white font-headline-sm flex-shrink-0"
            :style="{ backgroundColor: project.color }">{{ project.initials }}</div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-3 flex-wrap">
              <h1 class="font-headline-md text-headline-md text-on-surface">{{ project.name }}</h1>
              <span class="text-[11px] font-bold px-2 py-0.5 rounded-full bg-secondary-container/30 text-secondary">Active</span>
            </div>
            <p class="font-body-md text-body-md text-on-surface-variant mt-1">{{ project.desc }}</p>
            <div class="flex items-center gap-4 mt-2 flex-wrap">
              <div class="flex items-center gap-1 text-on-surface-variant">
                <span class="material-symbols-outlined text-[14px]">calendar_today</span>
                <span class="font-label-sm text-label-sm">{{ project.startDate }} → {{ project.endDate }}</span>
              </div>
              <div class="flex items-center gap-1 text-on-surface-variant">
                <span class="material-symbols-outlined text-[14px]">group</span>
                <span class="font-label-sm text-label-sm">{{ project.members.length }} thành viên</span>
              </div>
              <div class="flex items-center gap-1 text-on-surface-variant">
                <span class="material-symbols-outlined text-[14px]">sprint</span>
                <span class="font-label-sm text-label-sm">{{ project.sprints.length }} sprint</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Tabs -->
        <div class="flex gap-0 -mb-px">
          <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
            class="px-md py-sm font-label-lg text-label-lg border-b-2 transition-colors flex items-center gap-1.5 whitespace-nowrap"
            :class="activeTab === tab.id
              ? 'border-primary text-primary'
              : 'border-transparent text-on-surface-variant hover:text-on-surface hover:border-outline-variant'">
            <span class="material-symbols-outlined text-[16px]"
              :style="activeTab === tab.id ? `font-variation-settings: 'FILL' 1` : `font-variation-settings: 'FILL' 0`">
              {{ tab.icon }}
            </span>
            {{ tab.label }}
          </button>
        </div>
      </div>
    </div>

    <!-- Tab Content -->
    <div class="flex-1 overflow-y-auto">
      <div class="max-w-7xl mx-auto px-lg py-lg">

        <!-- Overview Tab -->
        <div v-if="activeTab === 'overview'" class="grid grid-cols-1 md:grid-cols-3 gap-md">
          <div class="md:col-span-2 flex flex-col gap-md">
            <!-- Stats -->
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-md">
              <div v-for="stat in overviewStats" :key="stat.label"
                class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md flex flex-col gap-xs hover:shadow-sm transition-shadow">
                <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">{{ stat.label }}</span>
                <div class="flex items-center gap-2">
                  <span class="material-symbols-outlined text-[20px]" :class="stat.iconColor">{{ stat.icon }}</span>
                  <span class="font-headline-sm text-headline-sm" :class="stat.color">{{ stat.value }}</span>
                </div>
              </div>
            </div>
            <!-- Description -->
            <div class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md">
              <h3 class="font-headline-sm text-headline-sm text-on-surface mb-3">Mô tả dự án</h3>
              <p class="font-body-md text-body-md text-on-surface-variant">{{ project.desc }}</p>
            </div>
          </div>

          <!-- Members sidebar -->
          <div class="bg-surface-container-lowest rounded-xl border border-outline-variant p-md">
            <div class="flex justify-between items-center mb-3">
              <h3 class="font-headline-sm text-headline-sm text-on-surface">Thành viên</h3>
              <button @click="activeTab = 'members'"
                class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</button>
            </div>
            <div class="flex flex-col gap-2">
              <div v-for="m in project.members.slice(0, 6)" :key="m.id" class="flex items-center gap-2">
                <div class="w-7 h-7 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
                  :style="{ backgroundColor: m.avatarColor }">{{ m.name[0] }}</div>
                <span class="font-label-lg text-label-lg text-on-surface flex-1 truncate">{{ m.name }}</span>
                <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="roleStyle(m.role)">{{ m.role }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Members Tab -->
        <MembersSection v-if="activeTab === 'members'" :project="project" @event="fireEvent" />

        <!-- Sprints Tab -->
        <SprintsSection v-if="activeTab === 'sprints'" :project="project" @event="fireEvent" />

      </div>
    </div>
  </div>

  <!-- Event Toast -->
  <EventToast :events="toasts" @dismiss="id => toasts = toasts.filter(e => e.id !== id)" />
</template>

<script setup>
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import MembersSection from '../components/MembersSection.vue'
import SprintsSection from '../components/SprintsSection.vue'
import EventToast from '../components/EventToast.vue'

const route = useRoute()
const activeTab = ref('overview')

const tabs = [
  { id: 'overview', label: 'Tổng quan', icon: 'dashboard' },
  { id: 'members', label: 'Thành viên', icon: 'group' },
  { id: 'sprints', label: 'Sprint', icon: 'sprint' },
]

// Mock project data (in real app: fetch from API using route.params.id)
const project = ref({
  id: Number(route.params.id) || 1,
  initials: 'PAY',
  color: '#3525cd',
  name: 'Payment Gateway Refactor',
  desc: 'Cập nhật và tối ưu hóa API thanh toán lên phiên bản 3.0, tích hợp với hệ thống mới.',
  startDate: '01/05/2026',
  endDate: '30/07/2026',
  members: [
    { id: 1, name: 'Nguyễn Minh Anh', email: 'minhanh@company.com', role: 'Owner', avatarColor: '#3525cd', joinedAt: '01/05/2026' },
    { id: 2, name: 'Trần Hoàng Nam', email: 'hoangnam@company.com', role: 'Manager', avatarColor: '#006a61', joinedAt: '02/05/2026' },
    { id: 3, name: 'Lê Thị Hoa', email: 'thihoa@company.com', role: 'Member', avatarColor: '#684000', joinedAt: '05/05/2026' },
    { id: 4, name: 'Phạm Văn Đức', email: 'vanduc@company.com', role: 'Member', avatarColor: '#ba1a1a', joinedAt: '10/05/2026' },
    { id: 5, name: 'Vũ Thị Lan', email: 'thilan@company.com', role: 'Viewer', avatarColor: '#6a0dad', joinedAt: '15/05/2026' },
  ],
  sprints: [
    { id: 1, name: 'Sprint 1', goal: 'Setup cơ sở hạ tầng và thiết kế database schema cho toàn bộ payment module', startDate: '2026-05-01', endDate: '2026-05-14', status: 2 },
    { id: 2, name: 'Sprint 2', goal: 'Implement API endpoints cho payment flow cơ bản và unit tests', startDate: '2026-05-15', endDate: '2026-05-28', status: 2 },
    { id: 3, name: 'Sprint 3', goal: 'Tích hợp payment gateway và viết unit tests — Coverage > 85%', startDate: '2026-05-29', endDate: '2026-06-11', status: 1 },
    { id: 4, name: 'Sprint 4', goal: 'Performance tuning, security audit và chuẩn bị deploy staging', startDate: '2026-06-12', endDate: '2026-06-25', status: 0 },
  ],
})

const overviewStats = [
  { label: 'Trạng thái', value: 'Active', color: 'text-secondary', icon: 'check_circle', iconColor: 'text-secondary' },
  { label: 'Thành viên', value: project.value.members.length, color: 'text-on-surface', icon: 'group', iconColor: 'text-primary' },
  { label: 'Sprint', value: project.value.sprints.length, color: 'text-on-surface', icon: 'sprint', iconColor: 'text-tertiary' },
  { label: 'Quá hạn', value: 0, color: 'text-secondary', icon: 'warning', iconColor: 'text-secondary' },
]

const roleInfo = [
  { name: 'Owner', style: 'bg-primary/10 text-primary' },
  { name: 'Manager', style: 'bg-secondary/10 text-secondary' },
  { name: 'Member', style: 'bg-surface-container text-on-surface-variant' },
  { name: 'Viewer', style: 'bg-surface-container text-outline' },
]
function roleStyle(role) { return roleInfo.find(r => r.name === role)?.style || '' }

// Toast system
const toasts = ref([])
let toastCounter = 0

function fireEvent(event) {
  const id = ++toastCounter
  toasts.value.push({ ...event, id })
  setTimeout(() => {
    toasts.value = toasts.value.filter(e => e.id !== id)
  }, 5000)
}
</script>
