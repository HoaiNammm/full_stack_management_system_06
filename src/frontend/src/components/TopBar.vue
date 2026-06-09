<template>
  <header class="h-16 flex-shrink-0 bg-surface-container-lowest border-b border-outline-variant shadow-sm flex justify-between items-center px-lg z-30">
    <!-- Left -->
    <div class="flex items-center gap-lg">
      <div class="font-headline-md text-headline-md font-black text-primary hidden md:block">Quản Lý Dự Án</div>
    </div>

    <!-- Right -->
    <div class="flex items-center gap-sm">
      <!-- Search -->
      <div class="hidden md:flex items-center bg-surface-container-low rounded-full px-3 py-1.5 border border-outline-variant focus-within:border-primary focus-within:ring-1 focus-within:ring-primary transition-all">
        <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
        <input v-model="searchQuery" @keyup.enter="doSearch"
          class="bg-transparent border-none focus:ring-0 font-body-md text-body-md text-on-surface w-40 placeholder:text-outline outline-none ml-2"
          placeholder="Tìm kiếm dự án..." type="text" />
      </div>

      <!-- Notification bell -->
      <RouterLink to="/calendar" class="text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all">
        <span class="material-symbols-outlined">calendar_month</span>
      </RouterLink>

      <!-- Notification bell + dropdown -->
      <div class="relative">
        <button @click="toggleNotif"
          class="relative text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all active:opacity-80">
          <span class="material-symbols-outlined">notifications</span>
          <span v-if="unreadCount > 0"
            class="absolute top-1 right-1 w-4 h-4 bg-error rounded-full border border-surface-container-lowest flex items-center justify-center text-[9px] text-white font-bold">
            {{ unreadCount > 9 ? '9+' : unreadCount }}
          </span>
        </button>

        <!-- Dropdown preview -->
        <div v-if="showNotif" @click.stop
          class="absolute right-0 top-full mt-1 w-80 bg-surface-container-lowest rounded-xl shadow-lg border border-outline-variant z-50 overflow-hidden">
          <div class="flex items-center justify-between px-3 py-2 border-b border-outline-variant">
            <span class="font-label-lg text-label-lg text-on-surface font-bold">Thông báo</span>
            <RouterLink to="/notifications" @click="showNotif = false"
              class="font-label-sm text-label-sm text-primary hover:underline">Xem tất cả</RouterLink>
          </div>
          <div v-if="recentNotifs.length === 0" class="px-3 py-4 text-center font-body-sm text-body-sm text-on-surface-variant">
            Không có thông báo mới
          </div>
          <div v-for="n in recentNotifs" :key="n.id"
            class="flex gap-2 px-3 py-2 hover:bg-surface-container-low transition-colors cursor-pointer border-b border-outline-variant/50 last:border-0"
            :class="{ 'bg-primary-container/10': !n.isRead }"
            @click="openNotif(n)">
            <div class="w-2 h-2 rounded-full mt-2 flex-shrink-0" :class="n.isRead ? 'bg-transparent' : 'bg-primary'"></div>
            <div class="flex-1 min-w-0">
              <p class="font-label-md text-label-md text-on-surface truncate">{{ n.title }}</p>
              <p class="font-body-sm text-body-sm text-on-surface-variant truncate">{{ n.content }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="w-px h-6 bg-outline-variant mx-xs hidden sm:block"></div>

      <!-- User avatar + menu -->
      <div class="relative">
        <button @click="showMenu = !showMenu"
          class="flex items-center gap-2 rounded-lg px-2 py-1 hover:bg-surface-container-high transition-colors">
          <div class="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-on-primary font-bold text-[13px] overflow-hidden">
            <img v-if="user?.avatar" :src="user.avatar" alt="Avatar" class="w-full h-full object-cover" />
            <span v-else>{{ userInitial }}</span>
          </div>
          <span class="hidden sm:block font-label-md text-label-md text-on-surface max-w-[120px] truncate">{{ user?.fullName || user?.email || 'User' }}</span>
          <span class="material-symbols-outlined text-on-surface-variant text-[16px]">expand_more</span>
        </button>

        <!-- Dropdown menu -->
        <div v-if="showMenu" @click.stop
          class="absolute right-0 top-full mt-1 w-48 bg-surface-container-lowest rounded-xl shadow-lg border border-outline-variant py-1 z-50">
          <div class="px-3 py-2 border-b border-outline-variant">
            <p class="font-label-lg text-label-lg text-on-surface truncate">{{ user?.fullName }}</p>
            <p class="font-label-sm text-label-sm text-on-surface-variant truncate">{{ user?.email }}</p>
          </div>
          <button @click="handleLogout"
            class="w-full text-left px-3 py-2 font-label-md text-label-md text-error hover:bg-error-container/20 flex items-center gap-2 transition-colors">
            <span class="material-symbols-outlined text-[16px]">logout</span>
            Đăng xuất
          </button>
        </div>
      </div>
    </div>
  </header>

  <!-- Backdrops -->
  <div v-if="showMenu"  class="fixed inset-0 z-40" @click="showMenu = false"></div>
  <div v-if="showNotif" class="fixed inset-0 z-40" @click="showNotif = false"></div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { notifyService } from '../services/api'

const router       = useRouter()
const { user, logout } = useAuth()
const showMenu     = ref(false)
const showNotif    = ref(false)
const unreadCount  = ref(0)
const recentNotifs = ref([])
const searchQuery  = ref('')

const userInitial = computed(() => {
  const name = user.value?.fullName || user.value?.email || 'U'
  return name.charAt(0).toUpperCase()
})

function handleLogout() {
  logout()
  router.push('/login')
}

async function toggleNotif() {
  showNotif.value = !showNotif.value
  if (showNotif.value && recentNotifs.value.length === 0) {
    try {
      const all = await notifyService.getAll()
      recentNotifs.value = all.slice(0, 5)
    } catch { /* ignore */ }
  }
}

function openNotif(n) {
  showNotif.value = false
  router.push('/notifications')
}

function doSearch() {
  if (!searchQuery.value.trim()) return
  router.push({ path: '/projects', query: { search: searchQuery.value.trim() } })
  searchQuery.value = ''
}

onMounted(async () => {
  try {
    unreadCount.value = await notifyService.unreadCount()
  } catch { /* ignore */ }
})
</script>
