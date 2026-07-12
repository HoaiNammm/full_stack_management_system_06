<script setup>
import { onMounted, ref } from 'vue'
import { useNotificationStore } from '../stores/notificationStore'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { notificationApi } from '../api/notifications'
import { formatDistanceToNow } from 'date-fns'
import { Bell, Check, CheckCheck, ListTodo, MessageCircle, UserPlus, UserX, Zap } from 'lucide-vue-next'
import { BaseButton, EmptyState } from '@/components/base'

const notificationStore = useNotificationStore()
const workspaceStore    = useWorkspaceStore()
const inviteError       = ref('')

onMounted(() => notificationStore.fetchAll())

function timeAgo(date) {
  try {
    const s   = String(date)
    const iso = s.endsWith('Z') || s.includes('+') ? s : s + 'Z'
    return formatDistanceToNow(new Date(iso), { addSuffix: true })
  } catch { return '' }
}

async function handleMarkRead(n) {
  if (!n.isRead) await notificationStore.markRead(n.userNotificationId)
}

async function handleAccept(n) {
  inviteError.value = ''
  const token = n.metadata?.token || n.token
  if (!token) return
  try {
    await notificationApi.acceptInvitation(token)
    await notificationStore.markRead(n.userNotificationId)
    await workspaceStore.fetchWorkspaces()
    n.responded        = true
    n.respondedMessage = 'Đã tham gia dự án'
  } catch (e) {
    inviteError.value = e?.response?.data?.message || 'Có lỗi xảy ra'
  }
}

async function handleDecline(n) {
  inviteError.value = ''
  const token = n.metadata?.token || n.token
  if (!token) return
  try {
    await notificationApi.declineInvitation(token)
    await notificationStore.markRead(n.userNotificationId)
    n.responded        = true
    n.respondedMessage = 'Đã từ chối lời mời'
  } catch (e) {
    inviteError.value = e?.response?.data?.message || 'Có lỗi xảy ra'
  }
}

const iconMeta = {
  project_invitation:  { icon: UserPlus,       color: 'text-blue-500 dark:text-blue-400',    bg: 'bg-blue-100 dark:bg-blue-900/40' },
  invitation_declined: { icon: UserX,           color: 'text-red-500 dark:text-red-400',      bg: 'bg-red-100 dark:bg-red-900/40' },
  task_assigned:       { icon: ListTodo,        color: 'text-amber-500 dark:text-amber-400',  bg: 'bg-amber-100 dark:bg-amber-900/40' },
  comment_mention:     { icon: MessageCircle,   color: 'text-purple-500 dark:text-purple-400', bg: 'bg-purple-100 dark:bg-purple-900/40' },
  sprint_started:      { icon: Zap,             color: 'text-emerald-500 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-900/40' },
}
const defaultIcon = { icon: Bell, color: 'text-zinc-500 dark:text-zinc-400', bg: 'bg-zinc-100 dark:bg-zinc-800' }
function notifIcon(type) { return iconMeta[type] || defaultIcon }
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-4 text-zinc-900 dark:text-zinc-100">

    <!-- Header -->
    <div class="page-header-banner flex items-center justify-between">
      <div class="flex items-center gap-3">
        <h1 class="text-xl font-semibold text-white">Notifications</h1>
        <span
          v-if="notificationStore.unreadCount > 0"
          class="rounded-full bg-white/25 px-2 py-0.5 text-xs font-medium text-white"
        >
          {{ notificationStore.unreadCount }} new
        </span>
      </div>
      <BaseButton
        v-if="notificationStore.unreadCount > 0"
        variant="secondary"
        size="sm"
        @click="notificationStore.markAllRead()"
      >
        <CheckCheck class="size-4" aria-hidden="true" /> Mark all read
      </BaseButton>
    </div>

    <!-- Invite error -->
    <div
      v-if="inviteError"
      role="alert"
      class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-800 dark:bg-red-950/30 dark:text-red-400"
    >
      {{ inviteError }}
    </div>

    <!-- Loading -->
    <div v-if="notificationStore.loading" class="py-12 text-center text-sm text-zinc-400 dark:text-zinc-500">
      Loading notifications…
    </div>

    <!-- Empty state -->
    <EmptyState
      v-else-if="notificationStore.notifications.length === 0"
      :icon="Bell"
      title="No notifications"
      description="You're all caught up — nothing new right now."
      size="lg"
    />

    <!-- Notification list -->
    <div v-else class="space-y-2">
      <div
        v-for="n in notificationStore.notifications"
        :key="n.userNotificationId"
        tabindex="0"
        @click="handleMarkRead(n)"
        @keydown.enter="handleMarkRead(n)"
        :class="[
          'cursor-pointer rounded-lg border p-4 transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
          n.isRead
            ? 'border-zinc-200 bg-white hover:bg-zinc-50 dark:border-zinc-800 dark:bg-zinc-900 dark:hover:bg-zinc-800/60'
            : 'border-blue-200 bg-blue-50 hover:bg-blue-50/80 dark:border-blue-800 dark:bg-blue-950/30 dark:hover:bg-blue-950/40',
        ]"
      >
        <div class="flex items-start gap-3">
          <!-- Type icon -->
          <div :class="['mt-0.5 flex-shrink-0 rounded-lg p-2', notifIcon(n.type).bg]">
            <component
              :is="notifIcon(n.type).icon"
              :class="['size-4', notifIcon(n.type).color]"
              aria-hidden="true"
            />
          </div>

          <div class="min-w-0 flex-1">
            <!-- Title + timestamp row -->
            <div class="flex items-start justify-between gap-3">
              <div class="min-w-0">
                <p class="text-sm font-medium text-zinc-900 dark:text-zinc-100">{{ n.title }}</p>
                <p class="mt-0.5 text-sm text-zinc-500 dark:text-zinc-400">{{ n.message }}</p>
              </div>
              <div class="flex flex-shrink-0 items-center gap-2">
                <span v-if="!n.isRead" class="size-2 rounded-full bg-blue-500" aria-label="Unread" />
                <span class="whitespace-nowrap text-xs text-zinc-400 dark:text-zinc-500">{{ timeAgo(n.createdAt) }}</span>
              </div>
            </div>

            <!-- Invitation actions -->
            <div v-if="n.type === 'project_invitation' && !n.responded" class="mt-3 flex gap-2">
              <BaseButton variant="primary" size="xs" @click.stop="handleAccept(n)">
                <Check class="size-3" aria-hidden="true" /> Chấp nhận
              </BaseButton>
              <BaseButton variant="secondary" size="xs" @click.stop="handleDecline(n)">
                Từ chối
              </BaseButton>
            </div>

            <p v-if="n.responded" class="mt-2 text-xs italic text-zinc-400 dark:text-zinc-500">
              {{ n.respondedMessage }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
