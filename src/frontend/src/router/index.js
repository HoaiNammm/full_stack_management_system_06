<<<<<<< HEAD
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
=======
import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import DashboardView from "../views/DashboardView.vue";
import UsersView from "../views/UsersView.vue";
import CommentsView from "../views/CommentsView.vue";


const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    component: LoginView,
  },
  {
    path: "/dashboard",
    component: DashboardView,
    meta: { requiresAuth: true },
  },
  {
  path: "/users",
  component: UsersView,
  meta: { requiresAuth: true },
},
  {
    path: "/comments",
    component: CommentsView,
    meta: { requiresAuth: true },
  },


];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");

  if (to.meta.requiresAuth && !token) {
    next("/login");
  } else {
    next();
  }
});

export default router;
>>>>>>> notify-login
