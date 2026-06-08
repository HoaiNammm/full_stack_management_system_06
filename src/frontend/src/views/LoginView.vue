<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { login } from "../services/authService";

const router = useRouter();

// Trạng thái Dark/Light Mode đồng bộ
const isDark = ref(false);

const toggleTheme = () => {
  isDark.value = !isDark.value;
  localStorage.setItem('theme', isDark.value ? 'dark' : 'light');
};

onMounted(() => {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true;
  }
});

const email = ref("");
const password = ref("");
const loading = ref(false);
const error = ref("");

const handleLogin = async () => {
  error.value = "";
  loading.value = true;

  try {
    await login(email.value, password.value);
    router.push("/dashboard");
  } catch (err) {
    console.error(err);
    error.value = "Email hoặc mật khẩu không đúng";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="auth-layout" :class="{ 'dark-theme': isDark }">
    <button class="theme-toggle" @click="toggleTheme" title="Chuyển đổi giao diện">
      <svg v-if="!isDark" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>
      <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>
    </button>

    <div class="auth-banner">
      <div class="banner-content">
        <h2>Project Management System</h2>
        <p>Comment & Notify Service Demo. Quản lý luồng công việc hiệu quả và tối ưu hóa giao tiếp nhóm.</p>
        
        <div class="feature-list">
          <div class="feature-item">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#4ade80" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
            <span>JWT Authentication</span>
          </div>
          <div class="feature-item">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#60a5fa" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
            <span>User Management</span>
          </div>
          <div class="feature-item">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#f472b6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
            <span>Comment & Notification</span>
          </div>
          <div class="feature-item">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#fbbf24" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline></svg>
            <span>Activity Logs</span>
          </div>
        </div>
      </div>
    </div>

    <div class="auth-form-container">
      <div class="auth-form-wrapper">
        <div class="header-text">
          <h1>Đăng nhập</h1>
          <p>Chào mừng bạn quay lại hệ thống quản lý dự án</p>
        </div>

        <div class="social-auth">
          <button class="btn-social">
            <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg"><path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/><path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.16v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/><path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.16C1.43 8.55 1 10.22 1 12s.43 3.45 1.16 4.93l3.68-2.84z" fill="#FBBC05"/><path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.16 7.07l3.68けると2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/></svg>
            Google
          </button>
          <button class="btn-social">
            <svg viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg"><path d="M12 2C6.477 2 2 6.477 2 12c0 4.42 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.008-.866-.013-1.7-2.782.603-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.464-1.11-1.464-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.831.092-.646.35-1.086.636-1.336-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.161 22 16.418 22 12c0-5.523-4.477-10-10-10z" :fill="isDark ? '#fff' : '#111827'"/></svg>
            GitHub
          </button>
        </div>

        <div class="divider">
          <span>HOẶC TIẾP TỤC BẰNG EMAIL</span>
        </div>

        <div class="form-group">
          <label>Email</label>
          <input v-model="email" type="email" placeholder="" />
        </div>

        <div class="form-group">
          <div class="label-row">
            <label>Mật khẩu</label>
            <a href="#" class="forgot-link">Quên mật khẩu?</a>
          </div>
          <input v-model="password" type="password" placeholder="••••••••" />
        </div>

        <button class="btn-primary" @click="handleLogin" :disabled="loading">
          {{ loading ? "Đang xác thực..." : "Đăng nhập" }}
          <svg v-if="!loading" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"></path><polyline points="10 17 15 12 10 7"></polyline><line x1="15" y1="12" x2="3" y2="12"></line></svg>
        </button>

        <div v-if="error" class="alert error">{{ error }}</div>

        <div class="demo-account">
            <strong>Lưu ý:</strong>
            <p>Tài khoản được cấp bởi quản trị viên hệ thống.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* BIẾN CSS ĐỒNG BỘ CẢ 2 BÊN SÁNG/TỐI */
.auth-layout {
  /* Biến cho Form (Bên phải) */
  --bg-main: #ffffff;
  --bg-card: #ffffff;
  --text-primary: #111827;
  --text-secondary: #6b7280;
  --border-color: #e5e7eb;
  --input-bg: #ffffff;
  --primary: #4f46e5;
  --primary-hover: #4338ca;
  
  /* Biến cho Banner (Bên trái) - Chế độ Light */
  --banner-bg: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  --banner-text: #0f172a;
  --banner-desc: #475569;
  --feature-bg: rgba(255, 255, 255, 0.6);
  --feature-border: rgba(15, 23, 42, 0.08);
  --feature-text: #1e293b;
  --feature-hover-bg: #ffffff;
  --grid-line: rgba(15, 23, 42, 0.03);
  
  display: flex;
  min-height: 100vh;
  width: 100vw;
  margin: 0;
  padding: 0;
  background-color: var(--bg-main);
  color: var(--text-primary);
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  transition: all 0.4s ease;
  position: relative;
}

.auth-layout.dark-theme {
  /* Biến cho Form (Bên phải) - Chế độ Dark */
  --bg-main: #0f172a;
  --bg-card: #0f172a;
  --text-primary: #f8fafc;
  --text-secondary: #94a3b8;
  --border-color: #334155;
  --input-bg: #1e293b;
  --primary: #6366f1;
  --primary-hover: #4f46e5;
  
  /* Biến cho Banner (Bên trái) - Chế độ Dark */
  --banner-bg: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  --banner-text: #ffffff;
  --banner-desc: #cbd5e1;
  --feature-bg: rgba(255, 255, 255, 0.05);
  --feature-border: rgba(255, 255, 255, 0.1);
  --feature-text: #f8fafc;
  --feature-hover-bg: rgba(255, 255, 255, 0.1);
  --grid-line: rgba(255, 255, 255, 0.04);
}

/* NÚT ĐỔI THEME GÓC PHẢI */
.theme-toggle {
  position: absolute;
  top: 24px;
  right: 32px;
  background: var(--input-bg);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  width: 44px;
  height: 44px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 10;
}
.theme-toggle:hover {
  background: var(--border-color);
  transform: rotate(15deg);
}

/* NỬA TRÁI: BANNER ĐỒ HỌA */
.auth-banner {
  flex: 1.1;
  background: var(--banner-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: var(--banner-text);
  position: relative;
  overflow: hidden;
  transition: background 0.4s ease;
}

.auth-banner::before {
  content: '';
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(var(--grid-line) 1px, transparent 1px),
    linear-gradient(90deg, var(--grid-line) 1px, transparent 1px);
  background-size: 30px 30px;
  transition: background-image 0.4s ease;
}

.banner-content {
  position: relative;
  z-index: 2;
  max-width: 480px;
}

.banner-content h2 {
  font-size: 40px;
  font-weight: 700;
  margin-bottom: 16px;
  line-height: 1.2;
  color: var(--banner-text);
  transition: color 0.4s ease;
}

.banner-content p {
  font-size: 18px;
  color: var(--banner-desc);
  margin-bottom: 40px;
  line-height: 1.6;
  transition: color 0.4s ease;
}

/* DANH SÁCH TÍNH NĂNG */
.feature-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  background: var(--feature-bg);
  border: 1px solid var(--feature-border);
  border-radius: 16px;
  backdrop-filter: blur(10px);
  font-weight: 500;
  font-size: 16px;
  color: var(--feature-text);
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.02);
}

.feature-item:hover {
  background: var(--feature-hover-bg);
  transform: translateX(8px);
  border-color: var(--primary);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05);
}

/* NỬA PHẢI: FORM CHÍNH */
.auth-form-container {
  flex: 0.9;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 40px 24px;
}

.auth-form-wrapper {
  width: 100%;
  max-width: 420px;
}

.header-text h1 {
  font-size: 32px;
  font-weight: 700;
  margin-bottom: 8px;
  color: var(--text-primary);
}

.header-text p {
  color: var(--text-secondary);
  font-size: 15px;
  margin-bottom: 32px;
}

/* NÚT SOCIAL */
.social-auth {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
}
.btn-social {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: var(--input-bg);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 12px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}
.btn-social:hover {
  background: var(--border-color);
}

/* DIVIDER */
.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin-bottom: 24px;
  color: var(--text-secondary);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.5px;
}
.divider::before, .divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid var(--border-color);
}
.divider span { padding: 0 16px; }

/* FORM INPUTS */
.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 20px;
}

.label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.forgot-link {
  font-size: 13px;
  color: var(--primary);
  text-decoration: none;
  font-weight: 500;
}

.forgot-link:hover {
  text-decoration: underline;
}

label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
}

input {
  width: 100%;
  box-sizing: border-box;
  padding: 14px 16px;
  background: var(--input-bg);
  color: var(--text-primary);
  border: 1.5px solid var(--border-color);
  border-radius: 10px;
  font-size: 14px;
  transition: all 0.3s ease;
}

input:focus {
  border-color: var(--primary);
  outline: none;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.15);
}

/* BUTTON ĐĂNG NHẬP */
.btn-primary {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  width: 100%;
  padding: 14px;
  margin-top: 12px;
  background: var(--primary);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}
.btn-primary:hover:not(:disabled) {
  background: var(--primary-hover);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}
.btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

/* THÔNG BÁO LỖI */
.alert {
  padding: 12px;
  border-radius: 8px;
  margin-top: 16px;
  font-size: 14px;
  text-align: center;
}
.alert.error { background: rgba(220, 38, 38, 0.1); color: #ef4444; border: 1px solid rgba(220, 38, 38, 0.2); }

/* LINK ĐĂNG KÝ */
.login-link {
  text-align: center;
  margin-top: 24px;
  color: var(--text-secondary);
  font-size: 14px;
}
.login-link a {
  color: var(--primary);
  font-weight: 600;
  text-decoration: none;
  transition: color 0.2s;
}
.login-link a:hover { text-decoration: underline; }

/* RESPONSIVE MÀN HÌNH NHỎ */
@media (max-width: 960px) {
  .auth-banner { display: none; }
  .auth-form-container { padding: 32px 16px; }
  .auth-form-wrapper { max-width: 100%; width: 100%; }
}
@media (max-width: 500px) {
  .social-auth { grid-template-columns: 1fr; }
  .theme-toggle { top: 16px; right: 16px; width: 38px; height: 38px; }
}
</style>