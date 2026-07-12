import { workspaceClient } from './axios'

export const workspaceApi = {
  getAll:       ()           => workspaceClient.get('/api/workspaces').then(r => r.data),
  getById:      (id)         => workspaceClient.get(`/api/workspaces/${id}`).then(r => r.data),
  create:       (data)       => workspaceClient.post('/api/workspaces', data).then(r => r.data),
  update:       (id, data)   => workspaceClient.put(`/api/workspaces/${id}`, data).then(r => r.data),
  delete:       (id)         => workspaceClient.delete(`/api/workspaces/${id}`),

  getMembers:   (id)         => workspaceClient.get(`/api/workspaces/${id}/members`).then(r => r.data),
  addMember:    (id, data)   => workspaceClient.post(`/api/workspaces/${id}/members`, data).then(r => r.data),
  updateMember: (id, mid, data) => workspaceClient.put(`/api/workspaces/${id}/members/${mid}`, data).then(r => r.data),
  removeMember: (id, mid)    => workspaceClient.delete(`/api/workspaces/${id}/members/${mid}`),
}
