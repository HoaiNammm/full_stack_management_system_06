<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { logout } from "../services/authService";
import CommentSection from "../components/CommentSection.vue";

const router = useRouter();

const taskId = ref("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
const projectId = ref("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

// Trạng thái Dark/Light Mode
const isDark = ref(false);

const toggleTheme = () => {
  isDark.value = !isDark.value;
  localStorage.setItem('theme', isDark.value ? 'dark' : 'light');
};

const handleLogout = () => {
  logout();
  router.push("/login");
};

onMounted(() => {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true;
  }
});
</script>

<template>
  <div class="dashboard-layout" :class="{ 'dark-theme': isDark }">
    
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="logo-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 2 7 12 12 22 7 12 2"></polygon><polyline points="2 17 12 22 22 17"></polyline><polyline points="2 12 12 17 22 12"></polyline></svg>
        </div>
        <h2>PMS Demo</h2>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/dashboard">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="14" y="14" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect></svg>
          Tổng quan
        </router-link>
        <router-link to="/users">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
          Người dùng
        </router-link>
        <router-link to="/comments">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
          Bình luận demo
        </router-link>
        <router-link to="/notifications">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path><path d="M13.73 21a2 2 0 0 1-3.46 0"></path></svg>
          Thông báo
        </router-link>
        <router-link to="/logs">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline></svg>
          Logs
        </router-link>
      </nav>

      <button class="btn-logout" @click="handleLogout">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
        Đăng xuất
      </button>
    </aside>

    <main class="main-content">
      <header class="page-header">
        <div class="header-text">
          <h1>Demo Comment Section</h1>
          <p>
            Trang này chỉ dùng để test component bình luận. Khi tích hợp thật,
            component này sẽ được nhúng vào Task Detail của nhóm 2.
          </p>
        </div>

        <div class="header-actions">
          <button class="theme-toggle" @click="toggleTheme" title="Chuyển đổi giao diện">
            <svg v-if="!isDark" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>
          </button>
        </div>
      </header>

      <div class="card demo-box">
        <div class="grid-2-col">
          <div class="form-group">
            <label>TaskId demo</label>
            <input v-model="taskId" />
          </div>

          <div class="form-group">
            <label>ProjectId demo</label>
            <input v-model="projectId" />
          </div>
        </div>
      </div>

      <div class="comment-wrapper">
        <CommentSection
          :task-id="taskId"
          :project-id="projectId"
        />
      </div>
    </main>
  </div>
</template>

<style scoped>
/* CSS VARIABLES CHO DARK/LIGHT MODE */
.dashboard-layout {
  --bg-body: #f8fafc;
  --bg-sidebar: #ffffff;
  --bg-card: #ffffff;
  --text-main: #111827;
  --text-muted: #64748b;
  --border-color: #e5e7eb;
  --input-bg: #ffffff;
  --primary: #4f46e5;
  --primary-hover: #4338ca;
  --sidebar-hover: #f1f5f9;
  --sidebar-active: #eef2ff;
  --sidebar-active-text: #4f46e5;
  --card-shadow: 0 6px 18px rgba(15, 23, 42, 0.06);
  
  display: flex;
  min-height: 100vh;
  width: 100vw;
  margin: 0;
  padding: 0;
  background-color: var(--bg-body);
  color: var(--text-main);
  font-family: 'Inter', Arial, sans-serif;
  transition: all 0.3s ease;
}

.dashboard-layout.dark-theme {
  --bg-body: #0f172a;
  --bg-sidebar: #1e293b;
  --bg-card: #1e293b;
  --text-main: #f8fafc;
  --text-muted: #94a3b8;
  --border-color: #334155;
  --input-bg: #0f172a;
  --primary: #6366f1;
  --primary-hover: #4f46e5;
  --sidebar-hover: #334155;
  --sidebar-active: #3730a3;
  --sidebar-active-text: #e0e7ff;
  --card-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
}

/* SIDEBAR - Compact (220px) */
.sidebar {
  width: 220px;
  background: var(--bg-sidebar);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  padding: 20px 16px;
  transition: all 0.3s ease;
  z-index: 10;
}
.sidebar-header { display: flex; align-items: center; gap: 10px; margin-bottom: 32px; padding-left: 6px; }
.logo-icon { background: var(--primary); color: white; width: 32px; height: 32px; border-radius: 8px; display: flex; align-items: center; justify-content: center; }
.sidebar-header h2 { font-size: 18px; font-weight: 700; margin: 0; letter-spacing: -0.5px; color: var(--text-main); }
.sidebar-nav { display: flex; flex-direction: column; gap: 4px; }
.sidebar-nav a { display: flex; align-items: center; gap: 12px; padding: 10px 14px; border-radius: 10px; color: var(--text-muted); text-decoration: none; font-weight: 500; font-size: 14px; transition: all 0.2s; }
.sidebar-nav a:hover { background: var(--sidebar-hover); color: var(--text-main); }
.sidebar-nav a.router-link-active { background: var(--sidebar-active); color: var(--sidebar-active-text); font-weight: 600; }
.btn-logout { margin-top: auto; display: flex; align-items: center; justify-content: center; gap: 8px; padding: 10px; background: transparent; color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.2); border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; transition: all 0.2s; }
.btn-logout:hover { background: #ef4444; color: white; }

/* MAIN CONTENT - Compact Padding */
.main-content { flex: 1; display: flex; flex-direction: column; padding: 20px 24px; overflow-y: auto; }

/* HEADER */
.page-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 24px; }
.header-text h1 { margin: 0 0 6px 0; font-size: 24px; font-weight: 700; color: var(--text-main); }
.header-text p { margin: 0; color: var(--text-muted); font-size: 14px; max-width: 600px; line-height: 1.5; }
.header-actions { display: flex; gap: 10px; }

/* BUTTONS */
.theme-toggle { background: var(--bg-card); border: 1px solid var(--border-color); color: var(--text-main); width: 38px; height: 38px; border-radius: 50%; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all 0.3s ease; box-shadow: var(--card-shadow); }
.theme-toggle:hover { border-color: var(--primary); }

/* CARDS & FORMS */
.card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 14px; padding: 18px; box-shadow: var(--card-shadow); transition: all 0.3s ease; }
.demo-box { margin-bottom: 24px; }

.grid-2-col { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.form-group { display: flex; flex-direction: column; }

label { display: block; margin-bottom: 6px; color: var(--text-main); font-weight: 600; font-size: 13px; }

input {
  width: 100%; box-sizing: border-box; padding: 10px 12px;
  background: var(--input-bg); color: var(--text-main); border: 1px solid var(--border-color);
  border-radius: 10px; font-size: 14px; transition: all 0.3s ease; outline: none; height: 40px;
}
input:focus { border-color: var(--primary); }

.comment-wrapper {
  flex: 1;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 14px;
  box-shadow: var(--card-shadow);
  overflow: hidden; /* Tránh comment lấn ra ngoài bo góc */
}

/* RESPONSIVE */
@media (max-width: 900px) {
  .dashboard-layout { flex-direction: column; }
  .sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--border-color); padding: 16px; }
  .sidebar-header { margin-bottom: 16px; }
  .sidebar-nav { flex-direction: row; flex-wrap: wrap; }
  .btn-logout { margin-top: 16px; }
  .main-content { padding: 16px; }
  .grid-2-col { grid-template-columns: 1fr; gap: 12px; }
}
</style>