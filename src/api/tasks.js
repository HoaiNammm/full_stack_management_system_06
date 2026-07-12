import { taskClient } from './axios'

// TaskService (port 5003) — direct data responses (no wrapper)

export const taskApi = {
  getAll:      (projectId, params) => taskClient.get(`/api/projects/${projectId}/tasks`, { params }).then(r => r.data),
  getById:     (projectId, id)     => taskClient.get(`/api/projects/${projectId}/tasks/${id}`).then(r => r.data),
  create:      (projectId, data)   => taskClient.post(`/api/projects/${projectId}/tasks`, data).then(r => r.data),
  update:      (projectId, id, data) => taskClient.put(`/api/projects/${projectId}/tasks/${id}`, data).then(r => r.data),
  delete:      (projectId, id)     => taskClient.delete(`/api/projects/${projectId}/tasks/${id}`),
  getStats:    (projectId)         => taskClient.get(`/internal/projects/${projectId}/task-stats`).then(r => r.data),
  getMyTasks:  ()                  => taskClient.get('/api/tasks/mine').then(r => r.data),
}

export const commentApi = {
  getAll: (taskId)            => taskClient.get(`/api/tasks/${taskId}/comments`).then(r => r.data),
  create: (taskId, data)      => taskClient.post(`/api/tasks/${taskId}/comments`, data).then(r => r.data),
  update: (taskId, id, data)  => taskClient.put(`/api/tasks/${taskId}/comments/${id}`, data).then(r => r.data),
  delete: (taskId, id)        => taskClient.delete(`/api/tasks/${taskId}/comments/${id}`),
}

export const subtaskApi = {
  getAll:  (taskId)           => taskClient.get(`/api/tasks/${taskId}/subtasks`).then(r => r.data),
  create:  (taskId, data)     => taskClient.post(`/api/tasks/${taskId}/subtasks`, data).then(r => r.data),
  update:  (taskId, id, data) => taskClient.put(`/api/tasks/${taskId}/subtasks/${id}`, data).then(r => r.data),
  delete:  (taskId, id)       => taskClient.delete(`/api/tasks/${taskId}/subtasks/${id}`),
}

export const timelogApi = {
  getAll: (taskId)       => taskClient.get(`/api/tasks/${taskId}/timelogs`).then(r => r.data),
  log:    (taskId, data) => taskClient.post(`/api/tasks/${taskId}/timelogs`, data).then(r => r.data),
  delete: (taskId, id)   => taskClient.delete(`/api/tasks/${taskId}/timelogs/${id}`),
}

// Not available in running backend — stubs so imports don't crash
export const kanbanApi = {
  getColumns:   () => Promise.resolve([]),
  createColumn: () => Promise.reject(new Error('Kanban columns not supported')),
  updateColumn: () => Promise.reject(new Error('Kanban columns not supported')),
  deleteColumn: () => Promise.reject(new Error('Kanban columns not supported')),
}

export const activityLogApi = {
  getProject: (projectId) => taskClient.get(`/api/projects/${projectId}/activity`).then(r => r.data),
}
