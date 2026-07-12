import { defineStore } from 'pinia'
import { workspaceApi } from '../api/workspaces'
import { projectApi } from '../api/projects'

export const useWorkspaceStore = defineStore('workspace', {
  state: () => ({
    workspaces: [],
    currentWorkspaceId: localStorage.getItem('current_workspace') || null,
    projects: [],
    loading: false,
    error: null,
  }),
  getters: {
    currentWorkspace: (state) => {
      const ws = state.workspaces.find(w => w.id === state.currentWorkspaceId)
      if (!ws) return null
      return { ...ws, projects: state.projects }
    }
  },
  actions: {
    async setCurrentWorkspace(id) {
      this.currentWorkspaceId = id
      this.projects = []
      localStorage.setItem('current_workspace', id)
      if (id) await this.fetchProjects(id)
    },

    async fetchWorkspaces() {
      try {
        const data = await workspaceApi.getAll()
        const list = Array.isArray(data) ? data : []
        this.workspaces = list
        if (list.length > 0) {
          if (!this.currentWorkspaceId || !list.find(w => w.id === this.currentWorkspaceId))
            this.currentWorkspaceId = list[0].id
          localStorage.setItem('current_workspace', this.currentWorkspaceId)
          await this.fetchProjects(this.currentWorkspaceId)
        }
      } catch {
        this.workspaces = []
      }
    },

    async fetchProjects(workspaceId) {
      const wid = workspaceId || this.currentWorkspaceId
      if (!wid) return
      this.loading = true
      try {
        const list = await projectApi.getAll(wid)
        this.projects = Array.isArray(list) ? list : []
      } catch {
        this.projects = []
      } finally {
        this.loading = false
      }
    },

    // Load single project with full detail (includes members) and merge into store
    async fetchProject(id) {
      const wid = this.currentWorkspaceId
      if (!wid) return null
      try {
        const [project, members] = await Promise.all([
          projectApi.getById(wid, id),
          projectApi.getMembers(wid, id).catch(() => []),
        ])
        const merged = { ...project, members: members || [] }
        const idx = this.projects.findIndex(p => p.id === id)
        if (idx !== -1) this.projects[idx] = { ...this.projects[idx], ...merged }
        else this.projects.push(merged)
        return merged
      } catch { return null }
    },

    async createWorkspace(data) {
      const ws = await workspaceApi.create(data)
      this.workspaces.push(ws)
      this.currentWorkspaceId = ws.id
      localStorage.setItem('current_workspace', ws.id)
      return ws
    },
    async updateWorkspace(id, data) {
      const ws = await workspaceApi.update(id, data)
      const idx = this.workspaces.findIndex(w => w.id === id)
      if (idx !== -1) this.workspaces[idx] = ws
      return ws
    },
    async deleteWorkspace(id) {
      await workspaceApi.delete(id)
      this.workspaces = this.workspaces.filter(w => w.id !== id)
      if (this.currentWorkspaceId === id)
        this.currentWorkspaceId = this.workspaces[0]?.id || null
    },

    async createProject(workspaceId, data) {
      const project = await projectApi.create(workspaceId, data)
      this.projects.push(project)
      return project
    },
    async updateProject(workspaceId, id, data) {
      const project = await projectApi.update(workspaceId, id, data)
      const idx = this.projects.findIndex(p => p.id === id)
      if (idx !== -1) this.projects[idx] = project
      return project
    },
    async deleteProject(workspaceId, id) {
      await projectApi.delete(workspaceId, id)
      this.projects = this.projects.filter(p => p.id !== id)
    },
  }
})
