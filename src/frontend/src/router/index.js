import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import KanbanBoard from '../views/KanbanBoard.vue'
import ProjectsPage from '../views/ProjectsPage.vue'
import ProjectDetail from '../views/ProjectDetail.vue'

const routes = [
  { path: '/', redirect: '/dashboard' },
  { path: '/dashboard', component: Dashboard },
  { path: '/kanban', component: KanbanBoard },
  { path: '/projects', component: ProjectsPage },
  { path: '/projects/:id', component: ProjectDetail },
]

export default createRouter({
  history: createWebHistory(),
  routes,
})
