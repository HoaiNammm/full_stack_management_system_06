import { workspaceClient } from './axios'

// ProjectService (port 5001) — workspace-scoped routes, responses are direct data (no wrapper)

export const projectApi = {
  getAll:       (wid)                => workspaceClient.get(`/api/workspaces/${wid}/projects`).then(r => r.data),
  getById:      (wid, id)            => workspaceClient.get(`/api/workspaces/${wid}/projects/${id}`).then(r => r.data),
  create:       (wid, data)          => workspaceClient.post(`/api/workspaces/${wid}/projects`, data).then(r => r.data),
  update:       (wid, id, data)      => workspaceClient.put(`/api/workspaces/${wid}/projects/${id}`, data).then(r => r.data),
  delete:       (wid, id)            => workspaceClient.delete(`/api/workspaces/${wid}/projects/${id}`),

  getMembers:   (wid, id)            => workspaceClient.get(`/api/workspaces/${wid}/projects/${id}/members`).then(r => r.data),
  addMember:    (wid, id, data)      => workspaceClient.post(`/api/workspaces/${wid}/projects/${id}/members`, data).then(r => r.data),
  updateMember: (wid, id, mid, data) => workspaceClient.put(`/api/workspaces/${wid}/projects/${id}/members/${mid}`, data).then(r => r.data),
  removeMember: (wid, id, mid)       => workspaceClient.delete(`/api/workspaces/${wid}/projects/${id}/members/${mid}`),
}

export const sprintApi = {
  getAll:   (wid, pid)            => workspaceClient.get(`/api/workspaces/${wid}/projects/${pid}/sprints`).then(r => r.data),
  create:   (wid, pid, data)      => workspaceClient.post(`/api/workspaces/${wid}/projects/${pid}/sprints`, data).then(r => r.data),
  update:   (wid, pid, sid, data) => workspaceClient.put(`/api/workspaces/${wid}/projects/${pid}/sprints/${sid}`, data).then(r => r.data),
  start:    (wid, pid, sid)       => workspaceClient.post(`/api/workspaces/${wid}/projects/${pid}/sprints/${sid}/start`).then(r => r.data),
  complete: (wid, pid, sid)       => workspaceClient.post(`/api/workspaces/${wid}/projects/${pid}/sprints/${sid}/complete`).then(r => r.data),
  delete:   (wid, pid, sid)       => workspaceClient.delete(`/api/workspaces/${wid}/projects/${pid}/sprints/${sid}`),
}

export const milestoneApi = {
  getAll:  (wid, pid)             => workspaceClient.get(`/api/workspaces/${wid}/projects/${pid}/milestones`).then(r => r.data),
  create:  (wid, pid, data)       => workspaceClient.post(`/api/workspaces/${wid}/projects/${pid}/milestones`, data).then(r => r.data),
  update:  (wid, pid, mid, data)  => workspaceClient.put(`/api/workspaces/${wid}/projects/${pid}/milestones/${mid}`, data).then(r => r.data),
  delete:  (wid, pid, mid)        => workspaceClient.delete(`/api/workspaces/${wid}/projects/${pid}/milestones/${mid}`),
}

export const aiGenerateApi = {
  preview: (wid, prompt)            => workspaceClient.post(`/api/workspaces/${wid}/ai-projects/preview`, { prompt }).then(r => r.data),
  refine:  (wid, plan, refinement)  => workspaceClient.post(`/api/workspaces/${wid}/ai-projects/refine`, { plan, refinement }).then(r => r.data),
  confirm: (wid, plan)              => workspaceClient.post(`/api/workspaces/${wid}/ai-projects/confirm`, { plan }).then(r => r.data),
}

export const aiReportApi = {
  generate: (wid, pid, question) => workspaceClient.post(`/api/workspaces/${wid}/projects/${pid}/ai-report`, { question }).then(r => r.data),
}
