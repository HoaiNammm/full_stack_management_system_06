import { createRouter, createWebHistory } from 'vue-router'
import LandingView from '../views/LandingView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import ForgotPasswordView from '../views/ForgotPasswordView.vue'
import ResetPasswordView from '../views/ResetPasswordView.vue'
import LayoutView from '../views/LayoutView.vue'
import DashboardView from '../views/DashboardView.vue'
import ProjectsView from '../views/ProjectsView.vue'
import ProjectDetailsView from '../views/ProjectDetailsView.vue'
import TaskDetailsView from '../views/TaskDetailsView.vue'
import TeamView from '../views/TeamView.vue'
import NotificationsView from '../views/NotificationsView.vue'
import ProfileView from '../views/ProfileView.vue'
import MyWorkView from '../views/MyWorkView.vue'
import SettingsView from '../views/SettingsView.vue'

const routes = [
  { path: '/welcome', component: LandingView },
  { path: '/login', component: LoginView },
  { path: '/register', component: RegisterView },
  { path: '/forgot-password', component: ForgotPasswordView },
  { path: '/reset-password', component: ResetPasswordView },
  {
    path: '/',
    component: LayoutView,
    meta: { requiresAuth: true },
    children: [
      { path: '', component: DashboardView },
      { path: 'team', component: TeamView },
      { path: 'projects', component: ProjectsView },
      { path: 'projectsDetail', component: ProjectDetailsView },
      { path: 'taskDetails', component: TaskDetailsView },
      { path: 'notifications', component: NotificationsView },
      { path: 'profile', component: ProfileView },
      { path: 'my-work', component: MyWorkView },
      { path: 'settings', component: SettingsView },
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('access_token')
  if (to.meta.requiresAuth && !token) {
    next(to.path === '/' ? '/welcome' : '/login')
  } else {
    next()
  }
})

export default router
