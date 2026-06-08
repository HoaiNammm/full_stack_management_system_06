<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getCurrentUser, logout } from "../services/authService";

const router = useRouter();
const user = ref(null);
const loading = ref(true);

// Trạng thái Dark/Light Mode đồng bộ
const isDark = ref(false);

const toggleTheme = () => {
  isDark.value = !isDark.value;
  localStorage.setItem('theme', isDark.value ? 'dark' : 'light');
};

const loadUser = async () => {
  try {
    user.value = await getCurrentUser();
  } catch (err) {
    console.error(err);
    logout();
    router.push("/login");
  } finally {
    setTimeout(() => { loading.value = false; }, 500); // Thêm delay nhỏ để thấy hiệu ứng Skeleton
  }
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
  loadUser();
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
          Bình luận
        </router-link>
        <router-link to="/notifications">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path><path d="M13.73 21a2 2 0 0 1-3.46 0"></path></svg>
          Thông báo
        </router-link>
        <router-link to="/logs">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline></svg>
          Activity Logs
        </router-link>
      </nav>

      <button class="btn-logout" @click="handleLogout">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
        Đăng xuất
      </button>
    </aside>

    <main class="main-content">
      
      <header class="topbar">
        <div class="greeting">
          <h1 v-if="!loading">Chào, {{ user?.fullName?.split(' ').pop() || 'Bạn' }}!</h1>
          <div v-else class="skeleton skeleton-title"></div>
          <p>Kiểm tra kết nối hệ thống và thông tin tài khoản.</p>
        </div>
        
        <button class="theme-toggle" @click="toggleTheme" title="Chuyển đổi giao diện">
          <svg v-if="!isDark" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>
        </button>
      </header>

      <div class="grid-content">
        
        <div class="card">
          <div class="card-header">
            <h3>Hồ sơ người dùng</h3>
            <span v-if="!loading" class="badge" :class="user?.role?.toLowerCase()">{{ user?.role }}</span>
          </div>
          
          <div v-if="loading" class="skeleton-wrapper">
            <div class="skeleton skeleton-line" v-for="i in 5" :key="i"></div>
          </div>
          
          <div v-else class="info-list">
            <div class="info-item">
              <span class="label">Họ tên</span>
              <span class="value">{{ user?.fullName }}</span>
            </div>
            <div class="info-item">
              <span class="label">Email</span>
              <span class="value">{{ user?.email }}</span>
            </div>
            <div class="info-item">
              <span class="label">Phòng ban</span>
              <span class="value">{{ user?.department || "—" }}</span>
            </div>
            <div class="info-item">
              <span class="label">Chức vụ</span>
              <span class="value">{{ user?.position || "—" }}</span>
            </div>
            <div class="info-item">
              <span class="label">Số điện thoại</span>
              <span class="value">{{ user?.phoneNumber || "—" }}</span>
            </div>
          </div>
        </div>

        <div class="card status-card">
          <div class="card-header">
            <h3>Trạng thái hệ thống</h3>
          </div>
          
          <div v-if="loading" class="skeleton-wrapper">
            <div class="skeleton skeleton-line"></div>
            <div class="skeleton skeleton-line w-2/3"></div>
          </div>
          
          <div v-else class="status-list">
            <div class="status-item success">
              <div class="status-icon"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg></div>
              <div class="status-text">
                <h4>Đăng nhập thành công</h4>
                <p>Phiên làm việc đã được xác thực</p>
              </div>
            </div>
            
            <div class="status-item">
              <div class="status-icon info"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3m-3.5 3.5L19 4"></path></svg></div>
              <div class="status-text">
                <h4>Token Active</h4>
                <p>Đã lưu trữ an toàn trong LocalStorage</p>
              </div>
            </div>

            <div class="status-item">
              <div class="status-icon info"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><ellipse cx="12" cy="5" rx="9" ry="3"></ellipse><path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"></path><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"></path></svg></div>
              <div class="status-text">
                <h4>Data Fetched</h4>
                <p>API `/auth/me` phản hồi tốt</p>
              </div>
            </div>
          </div>
        </div>

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
  --primary: #6366f1;
  --primary-hover: #4f46e5;
  --sidebar-hover: #334155;
  --sidebar-active: #3730a3;
  --sidebar-active-text: #e0e7ff;
  --card-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
}

/* SIDEBAR - Giảm width xuống 220px */
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

.sidebar-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 32px;
  padding-left: 6px;
}

.logo-icon {
  background: var(--primary);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.sidebar-header h2 {
  font-size: 18px;
  font-weight: 700;
  margin: 0;
  letter-spacing: -0.5px;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

/* SIDEBAR LINKS - Font nhỏ hơn, padding gọn hơn */
.sidebar-nav a {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 10px;
  color: var(--text-muted);
  text-decoration: none;
  font-weight: 500;
  font-size: 14px;
  transition: all 0.2s;
}

.sidebar-nav a:hover {
  background: var(--sidebar-hover);
  color: var(--text-main);
}

.sidebar-nav a.router-link-active {
  background: var(--sidebar-active);
  color: var(--sidebar-active-text);
  font-weight: 600;
}

.btn-logout {
  margin-top: auto;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px;
  background: transparent;
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 10px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-logout:hover {
  background: #ef4444;
  color: white;
}

/* MAIN CONTENT - Giảm padding trang */
.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 20px 24px;
  overflow-y: auto;
}

/* TOPBAR */
.topbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
}

.greeting h1 {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 6px 0;
  color: var(--text-main);
}

.greeting p {
  color: var(--text-muted);
  margin: 0;
  font-size: 14px;
}

.theme-toggle {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  color: var(--text-main);
  width: 38px;
  height: 38px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: var(--card-shadow);
}

.theme-toggle:hover {
  transform: rotate(15deg);
  border-color: var(--primary);
}

/* GRID CARDS - Form gọn hơn */
.grid-content {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 18px;
}

/* CARD - Giảm padding & border-radius */
.card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 14px;
  padding: 18px;
  box-shadow: var(--card-shadow);
  transition: all 0.3s ease;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-color);
}

.card-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main);
}

/* BADGE - Thu nhỏ */
.badge {
  padding: 5px 9px;
  border-radius: 99px;
  font-size: 11px;
  font-weight: 700;
  background: rgba(79, 70, 229, 0.1);
  color: var(--primary);
}
.badge.admin { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.badge.user { background: rgba(16, 185, 129, 0.1); color: #10b981; }

/* INFO LIST */
.info-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-item .label {
  color: var(--text-muted);
  font-size: 13px;
}

.info-item .value {
  font-weight: 500;
  font-size: 14px;
}

/* STATUS LIST */
.status-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.status-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.status-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.status-item.success .status-icon { background: rgba(16, 185, 129, 0.15); color: #10b981; }
.status-icon.info { background: rgba(59, 130, 246, 0.15); color: #3b82f6; }

.status-text h4 {
  margin: 0 0 2px 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.status-text p {
  margin: 0;
  font-size: 12px;
  color: var(--text-muted);
}

/* SKELETON LOADING */
.skeleton-wrapper {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.skeleton {
  background: linear-gradient(90deg, var(--border-color) 25%, var(--bg-body) 50%, var(--border-color) 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 6px;
}
.skeleton-title { height: 28px; width: 220px; margin-bottom: 6px; }
.skeleton-line { height: 14px; width: 100%; }
.w-2\/3 { width: 66%; }

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* RESPONSIVE */
@media (max-width: 900px) {
  .dashboard-layout { flex-direction: column; }
  .sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--border-color); padding: 16px; }
  .sidebar-header { margin-bottom: 16px; }
  .sidebar-nav { flex-direction: row; flex-wrap: wrap; }
  .btn-logout { margin-top: 16px; }
  .main-content { padding: 16px; }
}
</style>