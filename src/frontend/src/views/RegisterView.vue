<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { register } from "../services/authService";

const router = useRouter();

// Trạng thái Dark/Light Mode
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

const form = ref({
  fullName: "",
  email: "",
  password: "",
  confirmPassword: "", // Thêm trường xác nhận mật khẩu
  role: "Member",
  phoneNumber: "",
  avatarUrl: "",
  department: "",
  position: "",
});

const loading = ref(false);
const error = ref("");
const success = ref("");

const handleRegister = async () => {
  error.value = "";
  success.value = "";

  // Kiểm tra nhập đủ các trường bắt buộc
  if (!form.value.fullName.trim() || !form.value.email.trim() || !form.value.password.trim() || !form.value.confirmPassword.trim()) {
    error.value = "Vui lòng nhập đầy đủ thông tin bắt buộc";
    return;
  }

  // Kiểm tra mật khẩu và xác nhận mật khẩu có khớp không
  if (form.value.password !== form.value.confirmPassword) {
    error.value = "Mật khẩu xác nhận không khớp";
    return;
  }

  loading.value = true;

  try {
    await register({
      fullName: form.value.fullName.trim(),
      email: form.value.email.trim(),
      password: form.value.password,
      phoneNumber: form.value.phoneNumber,
      avatarUrl: form.value.avatarUrl,
      department: form.value.department,
      position: form.value.position,
      role: form.value.role
    });

    success.value = "Đăng ký thành công. Đang chuyển hướng...";

    setTimeout(() => {
      router.push("/login");
    }, 1200);
  } catch (err) {
    error.value = err.response?.data?.message || "Đăng ký thất bại";
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
        <h2>Tham gia cùng chúng tôi</h2>
        <p>Quản lý dự án, theo dõi tiến độ và cộng tác mượt mà trong một nền tảng duy nhất.</p>
        <div class="mockup-window">
          <div class="mockup-header"><span class="dot"></span><span class="dot"></span><span class="dot"></span></div>
          <div class="mockup-body">
            <div class="skeleton-line w-3/4"></div>
            <div class="skeleton-line w-1/2"></div>
            <div class="skeleton-box"></div>
          </div>
        </div>
      </div>
    </div>

    <div class="auth-form-container">
      <div class="auth-form-wrapper">
        <div class="header-text">
          <h1>Đăng ký tài khoản</h1>
          <p>Tạo tài khoản người dùng trong hệ thống</p>
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
          <span>HOẶC TIẾP TỤC VỚI</span>
        </div>

        <div class="form-grid">
          <div class="form-group">
            <label>Họ tên</label>
            <input v-model="form.fullName" placeholder="Nhập đầy đủ họ tên" />
          </div>

          <div class="form-group">
            <label>Email</label>
            <input v-model="form.email" type="email" placeholder="" />
          </div> 
          <div class="form-group">
            <label>Mật khẩu</label>
            <input v-model="form.password" type="password" placeholder="••••••••" />
          </div>
          <div class="form-group">
            <label>Xác nhận mật khẩu</label>
            <input v-model="form.confirmPassword" type="password" placeholder="••••••••" />
          </div>

         
        </div>

        <button class="btn-primary" @click="handleRegister" :disabled="loading">
          {{ loading ? "Đang xử lý..." : "Đăng ký ngay" }}
          <svg v-if="!loading" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line><polyline points="12 5 19 12 12 19"></polyline></svg>
        </button>

        <div v-if="error" class="alert error">{{ error }}</div>
        <div v-if="success" class="alert success">{{ success }}</div>

        <p class="login-link">
          Đã có tài khoản? <router-link to="/login">Đăng nhập</router-link>
        </p>
      </div>
    </div>
  </div>
</template>
<style scoped>
/* BIẾN CSS ĐỒNG BỘ CẢ 2 BÊN SÁNG/TỐI (DÀNH CHO TRANG ĐĂNG KÝ) */
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
  --grid-line: rgba(15, 23, 42, 0.03);

  /* Biến cho Mockup Đồ họa - Chế độ Light */
  --mockup-bg: rgba(255, 255, 255, 0.6);
  --mockup-border: rgba(15, 23, 42, 0.08);
  --mockup-header: rgba(15, 23, 42, 0.05);
  --mockup-skeleton: rgba(15, 23, 42, 0.08);
  --mockup-shadow: rgba(15, 23, 42, 0.05);
  
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
  --grid-line: rgba(255, 255, 255, 0.04);

  /* Biến cho Mockup Đồ họa - Chế độ Dark */
  --mockup-bg: rgba(255, 255, 255, 0.05);
  --mockup-border: rgba(255, 255, 255, 0.1);
  --mockup-header: rgba(0, 0, 0, 0.2);
  --mockup-skeleton: rgba(255, 255, 255, 0.1);
  --mockup-shadow: rgba(0, 0, 0, 0.5);
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
  margin-bottom: 48px;
  line-height: 1.6;
  transition: color 0.4s ease;
}

/* UI MOCKUP BẰNG CSS ĐỒNG BỘ THEME */
.mockup-window {
  width: 100%;
  background: var(--mockup-bg);
  border: 1px solid var(--mockup-border);
  border-radius: 12px;
  backdrop-filter: blur(10px);
  overflow: hidden;
  box-shadow: 0 25px 50px -12px var(--mockup-shadow);
  transition: all 0.4s ease;
}
.mockup-header {
  background: var(--mockup-header);
  padding: 12px 16px;
  display: flex;
  gap: 6px;
  transition: background 0.4s ease;
}
.dot { width: 10px; height: 10px; border-radius: 50%; background: #ef4444; }
.dot:nth-child(2) { background: #eab308; }
.dot:nth-child(3) { background: #22c55e; }
.mockup-body { padding: 24px; }
.skeleton-line { 
  height: 12px; 
  background: var(--mockup-skeleton); 
  border-radius: 6px; 
  margin-bottom: 12px;
  transition: background 0.4s ease;
}
.w-3\/4 { width: 75%; }
.w-1\/2 { width: 50%; }
.skeleton-box { 
  height: 80px; 
  background: var(--mockup-skeleton); 
  border-radius: 8px; 
  margin-top: 24px; 
  border: 1px solid var(--mockup-border); 
  transition: all 0.4s ease;
}

/* NỬA PHẢI: FORM CHÍNH */
.auth-form-container {
  flex: 0.9;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 40px 24px;
  overflow-y: auto;
}

.auth-form-wrapper {
  width: 100%;
  max-width: 480px;
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

/* GRID FORM */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px 12px;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.full-width {
  grid-column: 1 / -1;
}

label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
}

input, select {
  width: 100%;
  box-sizing: border-box;
  padding: 12px 14px;
  background: var(--input-bg);
  color: var(--text-primary);
  border: 1.5px solid var(--border-color);
  border-radius: 10px;
  font-size: 14px;
  transition: all 0.3s ease;
}

input:focus, select:focus {
  border-color: var(--primary);
  outline: none;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.15);
}

/* BUTTON ĐĂNG KÝ CHÍNH */
.btn-primary {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  width: 100%;
  padding: 14px;
  margin-top: 28px;
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

/* THÔNG BÁO */
.alert {
  padding: 12px;
  border-radius: 8px;
  margin-top: 16px;
  font-size: 14px;
  text-align: center;
}
.alert.error { background: rgba(220, 38, 38, 0.1); color: #ef4444; border: 1px solid rgba(220, 38, 38, 0.2); }
.alert.success { background: rgba(22, 163, 74, 0.1); color: #22c55e; border: 1px solid rgba(22, 163, 74, 0.2); }

/* LINK ĐĂNG NHẬP */
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
  .form-grid { grid-template-columns: 1fr; }
  .social-auth { grid-template-columns: 1fr; }
  .theme-toggle { top: 16px; right: 16px; width: 38px; height: 38px; }
}
</style>