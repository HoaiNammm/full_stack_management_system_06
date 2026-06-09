<template>
  <div class="p-lg max-w-2xl mx-auto flex flex-col gap-lg">
    <div>
      <h1 class="font-headline-md text-headline-md font-bold text-on-surface">Cài đặt</h1>
      <p class="font-body-md text-body-md text-on-surface-variant mt-1">Quản lý tài khoản và tùy chỉnh ứng dụng.</p>
    </div>

    <!-- Profile -->
    <div class="bg-surface-container-lowest rounded-xl border border-outline-variant overflow-hidden">
      <div class="px-md py-sm bg-surface-container-low border-b border-outline-variant">
        <h2 class="font-headline-sm text-headline-sm text-on-surface">Thông tin tài khoản</h2>
      </div>
      <div class="p-md flex items-center gap-md">
        <div class="w-16 h-16 rounded-full bg-primary flex items-center justify-center text-on-primary font-bold text-xl flex-shrink-0">
          <img v-if="user?.avatar" :src="user.avatar" class="w-full h-full rounded-full object-cover" />
          <span v-else>{{ initial }}</span>
        </div>
        <div>
          <p class="font-headline-sm text-headline-sm text-on-surface">{{ user?.fullName || 'Unknown' }}</p>
          <p class="font-body-md text-body-md text-on-surface-variant">{{ user?.email }}</p>
          <span class="mt-1 inline-block text-[11px] font-bold px-2 py-0.5 rounded-full"
            :class="roleBadge">{{ user?.role }}</span>
        </div>
      </div>
    </div>

    <!-- Services status -->
    <div class="bg-surface-container-lowest rounded-xl border border-outline-variant overflow-hidden">
      <div class="px-md py-sm bg-surface-container-low border-b border-outline-variant">
        <h2 class="font-headline-sm text-headline-sm text-on-surface">Trạng thái dịch vụ</h2>
      </div>
      <div class="divide-y divide-outline-variant/50">
        <div v-for="svc in services" :key="svc.name" class="px-md py-sm flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined text-[20px]" :class="svc.iconColor">{{ svc.icon }}</span>
            <div>
              <p class="font-label-lg text-label-lg text-on-surface">{{ svc.name }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant">{{ svc.url }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <span v-if="svc.loading" class="material-symbols-outlined animate-spin text-[16px] text-on-surface-variant">progress_activity</span>
            <template v-else>
              <div class="w-2 h-2 rounded-full" :class="svc.ok ? 'bg-secondary' : 'bg-error'"></div>
              <span class="font-label-sm text-label-sm" :class="svc.ok ? 'text-secondary' : 'text-error'">
                {{ svc.ok ? 'Online' : 'Offline' }}
              </span>
            </template>
          </div>
        </div>
      </div>
      <div class="px-md py-sm border-t border-outline-variant">
        <button @click="checkServices" :disabled="checking"
          class="flex items-center gap-1 font-label-md text-label-md text-primary hover:underline disabled:opacity-60">
          <span class="material-symbols-outlined text-[16px]" :class="{ 'animate-spin': checking }">refresh</span>
          Kiểm tra lại
        </button>
      </div>
    </div>

    <!-- Danger zone -->
    <div class="bg-surface-container-lowest rounded-xl border border-error/30 overflow-hidden">
      <div class="px-md py-sm bg-error-container/10 border-b border-error/20">
        <h2 class="font-headline-sm text-headline-sm text-error">Vùng nguy hiểm</h2>
      </div>
      <div class="p-md flex items-center justify-between">
        <div>
          <p class="font-label-lg text-label-lg text-on-surface">Đăng xuất</p>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Xóa session và quay về trang đăng nhập</p>
        </div>
        <button @click="handleLogout"
          class="flex items-center gap-1 px-md py-sm rounded-lg bg-error text-white font-label-lg text-label-lg hover:opacity-90 transition-opacity">
          <span class="material-symbols-outlined text-[18px]">logout</span>
          Đăng xuất
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { projectApi, taskApi, notifyApi } from '../services/api'

const router = useRouter()
const { user, logout, refreshUser } = useAuth()
const checking = ref(false)

const initial  = computed(() => (user.value?.fullName || user.value?.email || 'U').charAt(0).toUpperCase())
const roleBadge = computed(() => {
  const map = { Owner: 'bg-primary/10 text-primary', Manager: 'bg-secondary/10 text-secondary',
    Member: 'bg-surface-container text-on-surface-variant', Viewer: 'bg-surface-container text-outline',
    ProjectManager: 'bg-secondary/10 text-secondary' }
  return map[user.value?.role] || 'bg-surface-container text-on-surface-variant'
})

const services = ref([
  { name: 'ProjectService', url: 'http://localhost:5047', icon: 'folder_shared', iconColor: 'text-primary', loading: true, ok: false },
  { name: 'TaskService',    url: 'http://localhost:5217', icon: 'task_alt',      iconColor: 'text-secondary', loading: true, ok: false },
  { name: 'NotifyService',  url: 'http://localhost:5177', icon: 'notifications', iconColor: 'text-tertiary', loading: true, ok: false },
])

async function checkServices() {
  checking.value = true
  services.value.forEach(s => s.loading = true)
  await Promise.allSettled([
    projectApi.get('/projects').then(() => { services.value[0].ok = true }).catch(() => { services.value[0].ok = false }),
    taskApi.get('/kanban-columns').then(() => { services.value[1].ok = true }).catch(() => { services.value[1].ok = false }),
    notifyApi.get('/notifications/unread-count').then(() => { services.value[2].ok = true }).catch(() => { services.value[2].ok = false }),
  ])
  services.value.forEach(s => s.loading = false)
  checking.value = false
}

function handleLogout() {
  logout()
  router.push('/login')
}

onMounted(() => {
  refreshUser()
  checkServices()
})
</script>
