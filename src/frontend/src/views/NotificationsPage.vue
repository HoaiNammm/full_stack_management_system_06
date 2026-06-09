<template>
  <div class="p-lg max-w-2xl mx-auto">
    <div class="flex items-center justify-between mb-lg">
      <h1 class="font-headline-md text-headline-md font-bold text-on-surface">Thông báo</h1>
      <button v-if="unreadCount > 0" @click="markAllRead"
        class="font-label-md text-label-md text-primary hover:underline">
        Đánh dấu tất cả đã đọc
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-xl">
      <span class="material-symbols-outlined animate-spin text-primary text-[32px]">progress_activity</span>
    </div>

    <!-- Empty -->
    <div v-else-if="!notifications.length" class="flex flex-col items-center py-xl text-on-surface-variant gap-md">
      <span class="material-symbols-outlined text-[48px]">notifications_none</span>
      <p class="font-body-md text-body-md">Không có thông báo nào</p>
    </div>

    <!-- List -->
    <div v-else class="flex flex-col gap-xs">
      <div v-for="n in notifications" :key="n.id"
        @click="markRead(n)"
        class="flex gap-md p-md rounded-xl border cursor-pointer transition-all"
        :class="n.isRead
          ? 'bg-surface-container-lowest border-outline-variant opacity-70 hover:opacity-100'
          : 'bg-primary-container/20 border-primary/30 hover:bg-primary-container/30'">

        <!-- Icon -->
        <div class="flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center"
          :class="iconBg(n.type)">
          <span class="material-symbols-outlined text-[20px]" :class="iconColor(n.type)">
            {{ iconName(n.type) }}
          </span>
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0">
          <p class="font-label-lg text-label-lg text-on-surface" :class="{ 'font-bold': !n.isRead }">
            {{ n.title }}
          </p>
          <p class="font-body-sm text-body-sm text-on-surface-variant mt-0.5 line-clamp-2">{{ n.content }}</p>
          <p class="font-label-sm text-label-sm text-outline mt-1">{{ timeAgo(n.createdAt) }}</p>
        </div>

        <!-- Unread dot -->
        <div v-if="!n.isRead" class="flex-shrink-0 mt-2">
          <div class="w-2 h-2 rounded-full bg-primary"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { notifyService } from '../services/api'

const notifications = ref([])
const loading       = ref(true)

const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)

async function load() {
  loading.value = true
  try {
    notifications.value = await notifyService.getAll()
  } catch { /* ignore */ } finally {
    loading.value = false
  }
}

async function markRead(n) {
  if (n.isRead) return
  try {
    await notifyService.markRead(n.id)
    n.isRead = true
  } catch { /* ignore */ }
}

async function markAllRead() {
  try {
    await notifyService.markAllRead()
    notifications.value.forEach(n => n.isRead = true)
  } catch { /* ignore */ }
}

function iconName(type) {
  const map = {
    task_assigned:       'assignment_ind',
    task_column_changed: 'swap_horiz',
    comment_mention:     'alternate_email',
    member_added:        'person_add',
    sprint_started:      'rocket_launch',
  }
  return map[type] || 'notifications'
}

function iconBg(type) {
  const map = {
    task_assigned:       'bg-blue-100',
    task_column_changed: 'bg-purple-100',
    comment_mention:     'bg-orange-100',
    member_added:        'bg-green-100',
    sprint_started:      'bg-indigo-100',
  }
  return map[type] || 'bg-surface-container'
}

function iconColor(type) {
  const map = {
    task_assigned:       'text-blue-600',
    task_column_changed: 'text-purple-600',
    comment_mention:     'text-orange-600',
    member_added:        'text-green-600',
    sprint_started:      'text-indigo-600',
  }
  return map[type] || 'text-on-surface-variant'
}

function timeAgo(dateStr) {
  const diff = Date.now() - new Date(dateStr).getTime()
  const min  = Math.floor(diff / 60000)
  if (min < 1)  return 'Vừa xong'
  if (min < 60) return `${min} phút trước`
  const h = Math.floor(min / 60)
  if (h < 24)   return `${h} giờ trước`
  const d = Math.floor(h / 24)
  return `${d} ngày trước`
}

onMounted(load)
</script>
