import { createRouter, createWebHistory } from 'vue-router'
import Dashboard      from '../views/Dashboard.vue'
import KanbanBoard    from '../views/KanbanBoard.vue'
import ProjectsPage   from '../views/ProjectsPage.vue'
import ProjectDetail  from '../views/ProjectDetail.vue'
import CalendarView       from '../views/CalendarView.vue'
import LoginView          from '../views/LoginView.vue'
import RegisterView       from '../views/RegisterView.vue'
import NotificationsPage  from '../views/NotificationsPage.vue'
import SettingsPage       from '../views/SettingsPage.vue'

const routes = [
  { path: '/',           redirect: '/dashboard' },
  { path: '/login',      component: LoginView,    meta: { public: true } },
  { path: '/register',   component: RegisterView, meta: { public: true } },
  { path: '/dashboard',  component: Dashboard },
  { path: '/kanban',     component: KanbanBoard },
  { path: '/projects',   component: ProjectsPage },
  { path: '/projects/:id', component: ProjectDetail },
  { path: '/calendar',       component: CalendarView },
  { path: '/notifications',  component: NotificationsPage },
  { path: '/settings',       component: SettingsPage },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const token   = localStorage.getItem('token')
  const isValid = token && token !== 'undefined' && token !== 'null' && token.startsWith('eyJ')

  // Already logged in → skip login page, go to dashboard
  if (to.meta.public && isValid) {
    return { path: '/dashboard' }
  }

  // Not logged in → redirect to login
  if (!to.meta.public && !isValid) {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    return { path: '/login', query: { redirect: to.fullPath } }
  }
})

export default router
