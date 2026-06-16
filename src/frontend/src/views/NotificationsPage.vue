<template>
  <div class="page-wrap max-w-6xl">
    <section class="page-hero overflow-hidden">
      <img :src="dashboardVisual" class="absolute inset-0 h-full w-full object-cover opacity-20" alt="" />
      <div class="absolute inset-0 bg-gradient-to-r from-surface via-surface/90 to-surface/30"></div>
      <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="page-eyebrow">Inbox</p>
          <h1 class="font-headline-lg text-headline-lg text-on-surface">Thông báo</h1>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-1">
            Theo dõi các cập nhật từ task, project, thành viên và bình luận.
          </p>
        </div>
        <button v-if="unreadCount > 0" @click="markAllRead" class="app-button-primary">
          <span class="material-symbols-outlined text-[18px]">done_all</span>
          Đánh dấu đã đọc
        </button>
      </div>
    </section>

    <div class="toolbar-panel">
      <div class="flex flex-wrap gap-xs">
        <button v-for="item in filters" :key="item.value" @click="filter = item.value"
          class="px-3 py-1.5 rounded-full border font-label-md text-label-md transition-colors"
          :class="filter === item.value
            ? 'bg-primary text-on-primary border-primary'
            : 'bg-surface-container-lowest text-on-surface-variant border-outline-variant hover:border-primary hover:text-primary'">
          {{ item.label }}
        </button>
      </div>
      <div class="font-label-md text-label-md text-on-surface-variant">
        {{ unreadCount }} thông báo chưa đọc
      </div>
    </div>

    <section class="grid grid-cols-1 lg:grid-cols-[minmax(0,1fr)_360px] gap-md items-start">
      <div class="app-panel h-fit">
      <div v-if="loading" class="flex justify-center py-xl">
        <span class="material-symbols-outlined animate-spin text-primary text-[32px]">progress_activity</span>
      </div>

      <div v-else-if="!visibleNotifications.length" class="flex flex-col items-center py-xl text-on-surface-variant gap-md">
        <span class="material-symbols-outlined text-[56px]">notifications_none</span>
        <p class="font-body-md text-body-md">Không có thông báo phù hợp.</p>
      </div>

      <div v-else class="divide-y divide-outline-variant/50">
        <button v-for="n in visibleNotifications" :key="n.id" @click="selectNotification(n)"
          class="w-full text-left p-md transition-colors hover:bg-surface-container-low flex gap-md"
          :class="[
            !n.isRead ? 'bg-primary/5' : '',
            selectedNotification?.id === n.id ? 'ring-2 ring-primary/20 bg-primary/10' : ''
          ]">
          <div class="flex-shrink-0 w-11 h-11 rounded-xl flex items-center justify-center" :class="iconBg(n.type)">
            <span class="material-symbols-outlined text-[21px]" :class="iconColor(n.type)">{{ iconName(n.type) }}</span>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-xs">
              <p class="font-label-lg text-label-lg text-on-surface truncate" :class="{ 'font-bold': !n.isRead }">
                {{ n.title }}
              </p>
              <span v-if="!n.isRead" class="w-2 h-2 rounded-full bg-primary flex-shrink-0"></span>
            </div>
            <p class="font-body-sm text-body-sm text-on-surface-variant mt-1 line-clamp-2">{{ n.content }}</p>
            <p class="font-label-sm text-label-sm text-outline mt-2">{{ timeAgo(n.createdAt) }}</p>
          </div>
        </button>
      </div>
      </div>

      <aside class="app-panel p-md h-fit lg:sticky lg:top-24">
        <div v-if="selectedNotification" class="flex flex-col gap-md">
          <div class="gradient-panel p-md">
            <div class="relative z-10 flex items-start gap-3">
              <div class="flex-shrink-0 w-12 h-12 rounded-2xl flex items-center justify-center" :class="iconBg(selectedNotification.type)">
                <span class="material-symbols-outlined text-[22px]" :class="iconColor(selectedNotification.type)">
                  {{ iconName(selectedNotification.type) }}
                </span>
              </div>
              <div class="min-w-0">
                <p class="page-eyebrow">Notification preview</p>
                <h3 class="font-headline-sm text-headline-sm text-on-surface mt-1">{{ selectedNotification.title }}</h3>
                <p class="font-label-sm text-label-sm text-on-surface-variant mt-1">{{ timeAgo(selectedNotification.createdAt) }}</p>
              </div>
            </div>
          </div>

          <div class="workspace-card">
            <img :src="notificationVisual" class="mb-md h-32 w-full rounded-xl object-cover" alt="" />
            <p class="font-body-md text-body-md text-on-surface-variant leading-relaxed">
              {{ selectedNotification.content }}
            </p>
            <div class="mt-md flex flex-wrap gap-sm">
              <span class="soft-badge bg-surface-container text-on-surface-variant">{{ selectedNotification.type || 'system' }}</span>
              <span class="soft-badge" :class="selectedNotification.isRead ? 'bg-secondary/10 text-secondary' : 'bg-primary/10 text-primary'">
                {{ selectedNotification.isRead ? 'Đã đọc' : 'Chưa đọc' }}
              </span>
            </div>
          </div>

          <button v-if="!selectedNotification.isRead" @click="markRead(selectedNotification)" class="app-button-primary w-full">
            <span class="material-symbols-outlined text-[18px]">done</span>
            Đánh dấu đã đọc
          </button>
        </div>
        <div v-else class="flex flex-col items-center text-center gap-sm py-lg text-on-surface-variant">
          <span class="material-symbols-outlined text-[48px]">mark_email_unread</span>
          <p class="font-label-lg text-label-lg">Chọn một thông báo để xem chi tiết.</p>
        </div>
      </aside>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { notifyService } from '../services/api'
import { dashboardVisual, getTaskThumbnail } from '../services/visualAssets'

const notifications = ref([])
const loading = ref(true)
const filter = ref('all')
const selectedId = ref('')

const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)
const filters = computed(() => [
  { value: 'all', label: `Tất cả (${notifications.value.length})` },
  { value: 'unread', label: `Chưa đọc (${unreadCount.value})` },
  { value: 'read', label: `Đã đọc (${notifications.value.length - unreadCount.value})` },
])
const visibleNotifications = computed(() => {
  if (filter.value === 'unread') return notifications.value.filter(n => !n.isRead)
  if (filter.value === 'read') return notifications.value.filter(n => n.isRead)
  return notifications.value
})
const selectedNotification = computed(() =>
  visibleNotifications.value.find(n => n.id === selectedId.value) || visibleNotifications.value[0] || null
)
const notificationVisual = computed(() => getTaskThumbnail({ title: selectedNotification.value?.type || 'notification' }, 2))

async function load() {
  loading.value = true
  try {
    notifications.value = await notifyService.getAll()
    selectedId.value = notifications.value[0]?.id || ''
  } catch {
    notifications.value = []
  } finally {
    loading.value = false
  }
}

async function markRead(n) {
  if (n.isRead) return
  try {
    await notifyService.markRead(n.id)
    n.isRead = true
    window.dispatchEvent(new CustomEvent('notifications:changed'))
  } catch { /* ignore */ }
}

async function selectNotification(n) {
  selectedId.value = n.id
  if (!n.isRead) await markRead(n)
}

async function markAllRead() {
  try {
    await notifyService.markAllRead()
    notifications.value.forEach(n => n.isRead = true)
    window.dispatchEvent(new CustomEvent('notifications:changed'))
  } catch { /* ignore */ }
}

function iconName(type) {
  const map = {
    task_assigned: 'assignment_ind',
    task_column_changed: 'swap_horiz',
    comment_mention: 'alternate_email',
    member_added: 'person_add',
    sprint_started: 'rocket_launch',
  }
  return map[type] || 'notifications'
}

function iconBg(type) {
  const map = {
    task_assigned: 'bg-blue-100 dark:bg-blue-950/40',
    task_column_changed: 'bg-purple-100 dark:bg-purple-950/40',
    comment_mention: 'bg-orange-100 dark:bg-orange-950/40',
    member_added: 'bg-green-100 dark:bg-green-950/40',
    sprint_started: 'bg-indigo-100 dark:bg-indigo-950/40',
  }
  return map[type] || 'bg-surface-container'
}

function iconColor(type) {
  const map = {
    task_assigned: 'text-blue-600 dark:text-blue-300',
    task_column_changed: 'text-purple-600 dark:text-purple-300',
    comment_mention: 'text-orange-600 dark:text-orange-300',
    member_added: 'text-green-600 dark:text-green-300',
    sprint_started: 'text-indigo-600 dark:text-indigo-300',
  }
  return map[type] || 'text-on-surface-variant'
}

function timeAgo(dateStr) {
  const diff = Date.now() - new Date(dateStr).getTime()
  const min = Math.floor(diff / 60000)
  if (min < 1) return 'Vừa xong'
  if (min < 60) return `${min} phút trước`
  const h = Math.floor(min / 60)
  if (h < 24) return `${h} giờ trước`
  return `${Math.floor(h / 24)} ngày trước`
}

watch(visibleNotifications, (items) => {
  if (!items.some(n => n.id === selectedId.value)) selectedId.value = items[0]?.id || ''
})

onMounted(load)
</script>
