import { defineStore } from 'pinia'
import { taskApi, commentApi, subtaskApi, timelogApi } from '../api/tasks'

export const useTaskStore = defineStore('task', {
  state: () => ({
    tasksByProject: {},
    columnsByProject: {},   // always empty — kanban columns not in backend; KanbanBoard uses static fallback
    currentTask: null,
    myTasks: [],
    comments: [],
    subtasks: [],
    timelogs: [],
    loading: false,
    taskLoading: false,
    myTasksLoading: false,
  }),
  getters: {
    getProjectTasks: (state) => (projectId) => state.tasksByProject[projectId] || [],
    allTasks: (state) => Object.values(state.tasksByProject).flat(),
  },
  actions: {
    normalizeTask(t) {
      if (!t) return t
      return {
        ...t,
        dueDate: t.dueDate ?? t.deadline ?? null,
        assignee: t.assignee || (t.assigneeId ? { id: t.assigneeId } : null),
      }
    },

    // No-op: backend has no kanban columns; KanbanBoard falls back to static status columns
    async fetchColumns(projectId) {
      if (!this.columnsByProject[projectId]) this.columnsByProject[projectId] = []
      return this.columnsByProject[projectId]
    },

    async fetchTasks(projectId) {
      this.loading = true
      try {
        const tasks = await taskApi.getAll(projectId)
        this.tasksByProject[projectId] = (tasks || []).map(t => this.normalizeTask({ ...t, projectId: t.projectId ?? projectId }))
        return this.tasksByProject[projectId]
      } catch {
        this.tasksByProject[projectId] = []
      } finally {
        this.loading = false
      }
    },

    async fetchMyTasks() {
      this.myTasksLoading = true
      try {
        const tasks = await taskApi.getMyTasks()
        this.myTasks = (tasks || []).map(t => this.normalizeTask(t))
        return this.myTasks
      } catch {
        this.myTasks = []
      } finally {
        this.myTasksLoading = false
      }
    },

    async createTask(projectId, data) {
      const payload = {
        title:          data.title,
        description:    data.description || null,
        status:         data.status    || 'Backlog',
        type:           data.type      || 'Task',
        priority:       data.priority  || 'Medium',
        labels:         data.labels    || [],
        assigneeId:     data.assigneeId || data.assignedTo || null,
        deadline:       data.deadline  || data.dueDate     || null,
        sprintId:       data.sprintId  || null,
        estimatedHours: data.estimatedHours ? Number(data.estimatedHours) : null,
      }
      const task = await taskApi.create(projectId, payload)
      const normalized = this.normalizeTask(task)
      if (!this.tasksByProject[projectId]) this.tasksByProject[projectId] = []
      this.tasksByProject[projectId].push(normalized)
      return normalized
    },

    async updateTask(projectId, taskId, data) {
      const payload = {}
      if (data.title       !== undefined) payload.title       = data.title
      if (data.description !== undefined) payload.description = data.description
      if (data.status      !== undefined) payload.status      = data.status
      if (data.type        !== undefined) payload.type        = data.type
      if (data.priority    !== undefined) payload.priority    = data.priority
      if (data.labels      !== undefined) payload.labels      = data.labels
      if (data.assigneeId  !== undefined) { payload.assigneeId = data.assigneeId; if (!data.assigneeId) payload.clearAssignee = true }
      if (data.assignedTo  !== undefined) { payload.assigneeId = data.assignedTo;  if (!data.assignedTo)  payload.clearAssignee = true }
      if (data.deadline    !== undefined) { payload.deadline   = data.deadline;     if (!data.deadline)    payload.clearDeadline = true }
      if (data.dueDate     !== undefined) { payload.deadline   = data.dueDate;      if (!data.dueDate)     payload.clearDeadline = true }
      if (data.sprintId    !== undefined) { payload.sprintId   = data.sprintId;     if (!data.sprintId)    payload.clearSprint   = true }
      if (data.clearSprint !== undefined) payload.clearSprint = data.clearSprint
      if (data.estimatedHours !== undefined) {
        payload.estimatedHours = data.estimatedHours ? Number(data.estimatedHours) : null
        if (!data.estimatedHours) payload.clearEstimatedHours = true
      }

      const task = await taskApi.update(projectId, taskId, payload)
      const normalized = this.normalizeTask(task)
      const tasks = this.tasksByProject[projectId]
      if (tasks) {
        const idx = tasks.findIndex(t => t.id === taskId)
        if (idx !== -1) tasks[idx] = normalized
      }
      if (this.currentTask?.id === taskId) this.currentTask = normalized
      return normalized
    },

    async changeStatus(projectId, taskId, status) {
      return this.updateTask(projectId, taskId, { status })
    },

    async deleteTask(projectId, taskId) {
      await taskApi.delete(projectId, taskId)
      if (this.tasksByProject[projectId])
        this.tasksByProject[projectId] = this.tasksByProject[projectId].filter(t => t.id !== taskId)
    },

    async fetchTask(projectId, taskId) {
      this.taskLoading = true
      try {
        const task = await taskApi.getById(projectId, taskId)
        this.currentTask = this.normalizeTask(task)
        return this.currentTask
      } finally {
        this.taskLoading = false
      }
    },

    async fetchComments(taskId) {
      try {
        this.comments = await commentApi.getAll(taskId) || []
      } catch {
        this.comments = []
      }
    },
    async addComment(taskId, content, mentionedUserIds = []) {
      const comment = await commentApi.create(taskId, { content, mentionedUserIds })
      this.comments.push(comment)
      return comment
    },
    async updateComment(taskId, commentId, content) {
      const comment = await commentApi.update(taskId, commentId, { content })
      const idx = this.comments.findIndex(c => c.id === commentId)
      if (idx !== -1) this.comments[idx] = comment
      return comment
    },
    async deleteComment(taskId, commentId) {
      await commentApi.delete(taskId, commentId)
      this.comments = this.comments.filter(c => c.id !== commentId)
    },

    async fetchSubtasks(taskId) {
      try {
        this.subtasks = await subtaskApi.getAll(taskId) || []
      } catch {
        this.subtasks = []
      }
    },
    async createSubtask(taskId, title) {
      const sub = await subtaskApi.create(taskId, { title })
      this.subtasks.push(sub)
      return sub
    },
    async toggleSubtask(taskId, id, isCompleted) {
      const existing = this.subtasks.find(s => s.id === id)
      const sub = await subtaskApi.update(taskId, id, {
        title:  existing?.title || '',
        status: isCompleted ? 1 : 0,
      })
      const normalized = { ...sub, isCompleted: !!((sub.status ?? (isCompleted ? 1 : 0)) >= 1) }
      const idx = this.subtasks.findIndex(s => s.id === id)
      if (idx !== -1) this.subtasks[idx] = normalized
      return normalized
    },
    async deleteSubtask(taskId, id) {
      await subtaskApi.delete(taskId, id)
      this.subtasks = this.subtasks.filter(s => s.id !== id)
    },

    async fetchTimelogs(taskId) {
      try {
        this.timelogs = await timelogApi.getAll(taskId) || []
      } catch {
        this.timelogs = []
      }
    },
    async logTime(taskId, data) {
      const payload = {
        hoursLogged: data.hoursLogged ?? data.hours     ?? 1,
        loggedAt:    data.loggedAt    ?? data.loggedDate ?? new Date().toISOString().split('T')[0],
        description: data.description ?? null,
      }
      const log = await timelogApi.log(taskId, payload)
      this.timelogs.push(log)
      return log
    },
    async deleteTimelog(taskId, id) {
      await timelogApi.delete(taskId, id)
      this.timelogs = this.timelogs.filter(l => l.id !== id)
    },

    clearCurrentTask() {
      this.currentTask = null
      this.comments    = []
      this.subtasks    = []
      this.timelogs    = []
    },
  }
})
