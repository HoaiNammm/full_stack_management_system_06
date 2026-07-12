import { notifyClient } from './axios'

export const notificationApi = {
  getAll:         ()    => notifyClient.get('/api/notifications').then(r => r.data),
  getUnreadCount: ()    => notifyClient.get('/api/notifications/unread-count').then(r => r.data),
  markRead:       (id)  => notifyClient.post(`/api/notifications/${id}/read`).then(r => r.data),
  markAllRead:    ()    => notifyClient.post('/api/notifications/read-all').then(r => r.data),
}
