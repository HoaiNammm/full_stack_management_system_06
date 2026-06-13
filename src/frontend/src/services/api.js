import axios from 'axios'
import { clearSession, isTokenExpired, readToken } from './session'

const PROJECT_API = import.meta.env.VITE_PROJECT_API || 'http://localhost:5047/api'
const TASK_API    = import.meta.env.VITE_TASK_API || 'http://localhost:5217/api'
const NOTIFY_API  = import.meta.env.VITE_NOTIFY_API || 'http://localhost:5177/api'
export const NOTIFY_ORIGIN = NOTIFY_API.replace(/\/api\/?$/, '')

function makeClient(baseURL) {
  const client = axios.create({ baseURL })

  client.interceptors.request.use(cfg => {
    const token = readToken()
    if (token && isTokenExpired(token)) {
      clearSession()
      window.location.href = '/login'
      return Promise.reject(new Error('Session expired'))
    }
    if (token) cfg.headers.Authorization = `Bearer ${token}`
    return cfg
  })

  client.interceptors.response.use(
    res => res,
    err => {
      const isAuthEndpoint = err.config?.url?.includes('/auth/login')
      if (err.response?.status === 401 && !isAuthEndpoint) {
        clearSession()
        window.location.href = '/login'
      }
      return Promise.reject(err)
    }
  )

  return client
}

export const projectApi = makeClient(PROJECT_API)
export const taskApi    = makeClient(TASK_API)
export const notifyApi  = makeClient(NOTIFY_API)

// ── Project Service ──────────────────────────────────────────────
export const projectService = {
  getTemplates:        ()          => projectApi.get('/templates').then(r => r.data.data),
  getAll:              ()          => projectApi.get('/projects').then(r => r.data.data),
  getById:             (id)        => projectApi.get(`/projects/${id}`).then(r => r.data.data),
  create:              (body)      => projectApi.post('/projects', body).then(r => r.data.data),
  update:              (id, body)  => projectApi.put(`/projects/${id}`, body).then(r => r.data.data),
  delete:              (id)        => projectApi.delete(`/projects/${id}`),

  getSprints:          (pid)       => projectApi.get(`/projects/${pid}/sprints`).then(r => r.data.data),
  createSprint:        (pid, body) => projectApi.post(`/projects/${pid}/sprints`, body).then(r => r.data.data),
  updateSprint:        (pid, sid, body) => projectApi.put(`/projects/${pid}/sprints/${sid}`, body).then(r => r.data.data),
  deleteSprint:        (pid, sid) => projectApi.delete(`/projects/${pid}/sprints/${sid}`),

  getMilestones:       (pid)       => projectApi.get(`/projects/${pid}/milestones`).then(r => r.data.data),
  createMilestone:     (pid, body) => projectApi.post(`/projects/${pid}/milestones`, body).then(r => r.data.data),

  getMembers:          (pid)       => projectApi.get(`/projects/${pid}/members`).then(r => r.data.data),
  addMember:           (pid, body) => projectApi.post(`/projects/${pid}/members`, body).then(r => r.data.data),
  updateMemberRole:    (pid, mid, body) => projectApi.put(`/projects/${pid}/members/${mid}/role`, body).then(r => r.data.data),
  removeMember:        (pid, mid) => projectApi.delete(`/projects/${pid}/members/${mid}`),
}

// ── Task Service ─────────────────────────────────────────────────
export const taskService = {
  getAll:     (params) => taskApi.get('/tasks', { params }).then(r => r.data.data ?? r.data),
  getById:    (id)     => taskApi.get(`/tasks/${id}`).then(r => r.data.data),
  create:     (body)   => taskApi.post('/tasks', body).then(r => r.data.data),
  update:     (id, b)  => taskApi.put(`/tasks/${id}`, b).then(r => r.data.data),
  delete:     (id)     => taskApi.delete(`/tasks/${id}`),
  moveColumn: (id, columnId) => taskApi.put(`/tasks/${id}/column`, { columnId }),

  getColumns: (projectId) => {
    if (!projectId) return Promise.resolve([])
    return taskApi.get('/kanban-columns', { params: { projectId } }).then(r => r.data.data ?? r.data)
  },
}

// ── Notify Service ───────────────────────────────────────────────
export const authService = {
  login:   (body) => notifyApi.post('/auth/login', body).then(r => r.data),
  me:      ()     => notifyApi.get('/auth/me').then(r => r.data),
  register:(body) => notifyApi.post('/users/register', body).then(r => r.data),
  updateProfile:  (body) => notifyApi.put('/auth/profile', body).then(r => r.data),
  changePassword: (body) => notifyApi.post('/auth/change-password', body).then(r => r.data),
  uploadAvatar:   (file) => {
    const form = new FormData()
    form.append('file', file)
    return notifyApi.post('/auth/avatar', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(r => r.data)
  },
}

export const notifyService = {
  getAll:      (unreadOnly = false) => notifyApi.get('/notifications', { params: { unreadOnly } }).then(r => r.data.data),
  unreadCount: ()                   => notifyApi.get('/notifications/unread-count').then(r => r.data.data?.count ?? 0),
  markRead:    (id)                 => notifyApi.put(`/notifications/${id}/read`),
  markAllRead: ()                   => notifyApi.put('/notifications/read-all'),
  getProjectActivity: (projectId)   => notifyApi.get(`/activity-logs/project/${projectId}`).then(r => r.data.data ?? r.data),
  getTaskActivity:    (taskId)      => notifyApi.get(`/activity-logs/task/${taskId}`).then(r => r.data.data ?? r.data),
}

export const userService = {
  getAll:  () => notifyApi.get('/users').then(r => r.data.data),
  getById: (id) => notifyApi.get(`/users/${id}`).then(r => r.data.data),
}

export default notifyApi
