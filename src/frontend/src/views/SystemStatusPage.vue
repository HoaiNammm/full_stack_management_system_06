<template>
  <div class="page-wrap max-w-6xl">
    <section class="page-hero">
      <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p class="page-eyebrow">System status</p>
          <h1 class="font-headline-lg text-headline-lg text-on-surface">Trạng thái hệ thống</h1>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-1">
            Kiểm tra nhanh Frontend đang kết nối được backend service nào.
          </p>
        </div>
        <button @click="checkServices" :disabled="checking" class="app-button-primary">
          <span class="material-symbols-outlined text-[18px]" :class="{ 'animate-spin': checking }">refresh</span>
          {{ checking ? 'Đang kiểm tra...' : 'Kiểm tra lại' }}
        </button>
      </div>
    </section>

    <section class="grid grid-cols-1 md:grid-cols-3 gap-md">
      <div v-for="svc in services" :key="svc.name" class="workspace-card">
        <div class="flex items-start justify-between gap-3">
          <div class="w-12 h-12 rounded-2xl flex items-center justify-center" :class="svc.bg">
            <span class="material-symbols-outlined text-[24px]" :class="svc.iconColor">{{ svc.icon }}</span>
          </div>
          <span v-if="svc.loading" class="material-symbols-outlined animate-spin text-on-surface-variant">progress_activity</span>
          <span v-else class="soft-badge" :class="svc.ok ? 'bg-secondary/10 text-secondary' : 'bg-error-container/40 text-error'">
            {{ svc.ok ? 'Online' : 'Offline' }}
          </span>
        </div>

        <h2 class="font-headline-sm text-headline-sm text-on-surface mt-md">{{ svc.name }}</h2>
        <p class="font-label-md text-label-md text-on-surface-variant mt-1">{{ svc.url }}</p>
        <p class="font-body-sm text-body-sm text-on-surface-variant mt-md">{{ svc.description }}</p>

        <div class="mt-md rounded-xl border border-outline-variant bg-surface-container-low p-sm">
          <p class="font-label-sm text-label-sm text-on-surface-variant">Endpoint kiểm tra</p>
          <code class="font-label-md text-label-md text-on-surface break-all">{{ svc.checkPath }}</code>
        </div>
      </div>
    </section>

    <section class="app-panel p-md">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-md">
        <div class="metric-tile">
          <span>Tổng service</span>
          <strong>{{ services.length }}</strong>
        </div>
        <div class="metric-tile">
          <span>Online</span>
          <strong>{{ onlineCount }}</strong>
        </div>
        <div class="metric-tile">
          <span>Offline</span>
          <strong>{{ services.length - onlineCount }}</strong>
        </div>
      </div>
      <p class="font-body-md text-body-md text-on-surface-variant mt-md">
        Trang này phục vụ kiểm tra kỹ thuật khi demo: nếu service hiển thị Online nghĩa là FE gọi API thật thành công.
      </p>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { projectApi, taskApi, notifyApi } from '../services/api'

const checking = ref(false)
const services = ref([
  {
    name: 'ProjectService',
    url: 'http://localhost:5047',
    checkPath: 'GET /api/projects',
    icon: 'folder_shared',
    iconColor: 'text-primary',
    bg: 'bg-primary/10',
    description: 'Quản lý project, member, role, template, sprint và milestone.',
    loading: true,
    ok: false,
  },
  {
    name: 'TaskService',
    url: 'http://localhost:5217',
    checkPath: 'GET /api/tasks',
    icon: 'task_alt',
    iconColor: 'text-secondary',
    bg: 'bg-secondary/10',
    description: 'Quản lý task, Kanban column, deadline, priority và trạng thái công việc.',
    loading: true,
    ok: false,
  },
  {
    name: 'NotifyService',
    url: 'http://localhost:5177',
    checkPath: 'GET /api/notifications/unread-count',
    icon: 'notifications',
    iconColor: 'text-tertiary',
    bg: 'bg-tertiary/10',
    description: 'Quản lý auth, user profile, comment, notification và activity.',
    loading: true,
    ok: false,
  },
])

const onlineCount = computed(() => services.value.filter(s => s.ok).length)

async function checkServices() {
  checking.value = true
  services.value.forEach(s => { s.loading = true })
  await Promise.allSettled([
    projectApi.get('/projects').then(() => { services.value[0].ok = true }).catch(() => { services.value[0].ok = false }),
    taskApi.get('/tasks').then(() => { services.value[1].ok = true }).catch(() => { services.value[1].ok = false }),
    notifyApi.get('/notifications/unread-count').then(() => { services.value[2].ok = true }).catch(() => { services.value[2].ok = false }),
  ])
  services.value.forEach(s => { s.loading = false })
  checking.value = false
}

onMounted(checkServices)
</script>
