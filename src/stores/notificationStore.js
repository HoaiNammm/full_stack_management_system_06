import { defineStore } from 'pinia'
import { notificationApi } from '../api/notifications'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [],
    unreadCount: 0,
    loading: false,
  }),
  actions: {
    async fetchAll() {
      this.loading = true
      try {
        this.notifications = await notificationApi.getAll()
        this.unreadCount = this.notifications.filter(n => !n.isRead).length
      } catch {
        this.notifications = []
      } finally {
        this.loading = false
      }
    },
    async fetchUnreadCount() {
      try {
        const data = await notificationApi.getUnreadCount()
        this.unreadCount = data.unreadCount ?? 0
      } catch {}
    },
    async markRead(id) {
      try {
        await notificationApi.markRead(id)
        const n = this.notifications.find(n => n.userNotificationId === id)
        if (n && !n.isRead) { n.isRead = true; this.unreadCount = Math.max(0, this.unreadCount - 1) }
      } catch {}
    },
    async markAllRead() {
      try {
        await notificationApi.markAllRead()
        this.notifications.forEach(n => { n.isRead = true })
        this.unreadCount = 0
      } catch {}
    },
  }
})
