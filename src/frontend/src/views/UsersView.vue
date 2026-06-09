<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { logout } from "../services/authService";
import {
  getUsers,
  createUser,
  updateUser,
  updateUserStatus,
  updateUserRole,
  deleteUser,
} from "../services/userService";
import { getRoles, seedRoles } from "../services/roleService";

const router = useRouter();

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

// State quản lý danh sách & form
const users = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");
const roles = ref([]);

// State quản lý Modal
const isModalOpen = ref(false);
const editingUserId = ref(null);

const form = ref({
  fullName: "",
  email: "",
  password: "",
  role: "",
  phoneNumber: "",
  avatarUrl: "",
  department: "",
  position: "",
});

// Hàm điều khiển Modal
const openCreateModal = () => {
  resetForm();
  isModalOpen.value = true;
};

const closeModal = () => {
  isModalOpen.value = false;
  resetForm();
  error.value = "";
};

const resetForm = () => {
  editingUserId.value = null;
  form.value = {
    fullName: "",
    email: "",
    password: "",
    role: roles.value.length > 0 ? roles.value[0].code : "",
    phoneNumber: "",
    avatarUrl: "",
    department: "",
    position: "",
  };
};

const startEdit = (user) => {
  editingUserId.value = user.id;
  form.value = {
    fullName: user.fullName || "",
    email: user.email || "",
    password: "",
    role: user.role || "User",
    phoneNumber: user.phoneNumber || "",
    avatarUrl: user.avatarUrl || "",
    department: user.department || "",
    position: user.position || "",
  };
  isModalOpen.value = true; // Mở modal khi bấm sửa
};

// API Calls
const loadUsers = async () => {
  loading.value = true;
  error.value = "";
  try {
    await new Promise(resolve => setTimeout(resolve, 400));
    users.value = await getUsers();
  } catch (err) {
    console.error(err);
    error.value = "Không tải được danh sách người dùng";
  } finally {
    loading.value = false;
  }
};

const loadRoles = async () => {
  try {
    roles.value = await getRoles();
    if (roles.value.length === 0) {
      await seedRoles();
      roles.value = await getRoles();
    }
    if (roles.value.length > 0 && !form.value.role) {
      form.value.role = roles.value[0].code;
    }
  } catch (err) {
    console.error(err);
    error.value = "Không tải được danh sách role";
  }
};

const handleSubmit = async () => {
  error.value = "";
  success.value = "";

  if (!form.value.fullName.trim()) {
    error.value = "Họ tên không được để trống";
    return;
  }

  try {
    if (editingUserId.value) {
      await updateUser(editingUserId.value, {
        fullName: form.value.fullName,
        phoneNumber: form.value.phoneNumber,
        avatarUrl: form.value.avatarUrl,
        department: form.value.department,
        position: form.value.position,
      });
      success.value = "Cập nhật người dùng thành công";
    } else {
      if (!form.value.email.trim() || !form.value.password.trim()) {
        error.value = "Email và mật khẩu không được để trống";
        return;
      }
      await createUser({
        fullName: form.value.fullName,
        email: form.value.email,
        password: form.value.password,
        role: form.value.role,
        phoneNumber: form.value.phoneNumber,
        avatarUrl: form.value.avatarUrl,
        department: form.value.department,
        position: form.value.position,
      });
      success.value = "Tạo người dùng thành công";
    }

    closeModal(); // Đóng modal sau khi thành công
    await loadUsers();
  } catch (err) {
    error.value = err.response?.data?.message || "Thao tác thất bại";
  }
};

const handleToggleStatus = async (user) => {
  error.value = "";
  success.value = "";
  try {
    await updateUserStatus(user.id, !user.isActive);
    success.value = user.isActive ? "Đã khóa tài khoản" : "Đã mở lại tài khoản";
    await loadUsers();
  } catch (err) {
    console.error(err);
    error.value = "Không cập nhật được trạng thái tài khoản";
  }
};

const handleChangeRole = async (user, event) => {
  const newRole = event.target.value;
  error.value = "";
  success.value = "";
  try {
    await updateUserRole(user.id, newRole);
    success.value = "Cập nhật role thành công";
    await loadUsers();
  } catch (err) {
    error.value = err.response?.data?.message || "Không cập nhật được role";
  }
};

const handleDelete = async (user) => {
  const ok = confirm(`Bạn có chắc muốn xóa/khóa tài khoản ${user.email}?`);
  if (!ok) return;
  error.value = "";
  success.value = "";
  try {
    await deleteUser(user.id);
    success.value = "Đã khóa tài khoản người dùng";
    await loadUsers();
  } catch (err) {
    console.error(err);
    error.value = "Không xóa/khóa được người dùng";
  }
};

onMounted(async () => {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true;
  }
  await loadRoles();
  await loadUsers();
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
      <header class="page-header">
        <div class="header-text">
          <h1>Quản lý tài khoản</h1>
          <p>Thêm, cập nhật, khóa/mở tài khoản và phân quyền hệ thống.</p>
        </div>

        <div class="header-actions">
          <button class="theme-toggle" @click="toggleTheme" title="Chuyển đổi giao diện">
            <svg v-if="!isDark" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>
          </button>
          
          <button class="refresh-btn" @click="loadUsers">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"></polyline><polyline points="1 20 1 14 7 14"></polyline><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path></svg>
            Tải lại
          </button>

          <button class="primary-btn add-btn" @click="openCreateModal">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
            Tạo người dùng
          </button>
        </div>
      </header>

      <div v-if="error && !isModalOpen" class="alert error">{{ error }}</div>
      <div v-if="success && !isModalOpen" class="alert success">{{ success }}</div>

      <section class="content-section">
        <div class="card table-card full-width">
          <div class="table-header">
            <h2>Danh sách người dùng</h2>
            <span class="badge count-badge">{{ users.length }} tài khoản</span>
          </div>

          <div class="table-responsive">
            <table>
              <thead>
                <tr>
                  <th>Người dùng</th>
                  <th>Role</th>
                  <th>Phòng ban & Chức vụ</th>
                  <th>Trạng thái</th>
                  <th>Đăng nhập</th>
                  <th class="text-right">Thao tác</th>
                </tr>
              </thead>

              <tbody v-if="loading">
                <tr v-for="i in 5" :key="i">
                  <td>
                    <div class="user-cell">
                      <div class="skeleton avatar"></div>
                      <div class="skeleton-text-group">
                        <div class="skeleton skeleton-line w-3/4"></div>
                        <div class="skeleton skeleton-line w-1/2"></div>
                      </div>
                    </div>
                  </td>
                  <td><div class="skeleton skeleton-line w-full"></div></td>
                  <td><div class="skeleton skeleton-line w-3/4"></div></td>
                  <td><div class="skeleton skeleton-line w-1/2" style="border-radius: 99px;"></div></td>
                  <td><div class="skeleton skeleton-line w-full"></div></td>
                  <td><div class="skeleton skeleton-line w-full"></div></td>
                </tr>
              </tbody>

              <tbody v-else>
                <tr v-for="user in users" :key="user.id">
                  <td>
                    <div class="user-cell">
                      <img v-if="user.avatarUrl" :src="user.avatarUrl" alt="avatar" class="avatar" />
                      <div v-else class="avatar placeholder">
                        {{ user.fullName?.charAt(0)?.toUpperCase() || "U" }}
                      </div>
                      <div>
                        <strong class="user-name">{{ user.fullName }}</strong>
                        <p class="user-email">{{ user.email }}</p>
                      </div>
                    </div>
                  </td>
                  <td>
                    <select class="role-select" :value="user.role" @change="handleChangeRole(user, $event)">
                      <option v-for="role in roles" :key="role.id" :value="role.code">{{ role.name }}</option>
                    </select>
                  </td>
                  <td>
                    <div class="dept-cell">
                      <p>{{ user.department || "—" }}</p>
                      <small>{{ user.position || "—" }}</small>
                    </div>
                  </td>
                  <td>
                    <span :class="['badge', user.isActive ? 'active' : 'inactive']">
                      {{ user.isActive ? "Active" : "Inactive" }}
                    </span>
                  </td>
                  <td>
                    <span class="login-time">{{ user.lastLoginAt || "Chưa có" }}</span>
                  </td>
                  <td class="text-right">
                    <div class="action-group">
                      <button class="icon-btn edit" @click="startEdit(user)" title="Chỉnh sửa">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                      </button>

                      <button class="icon-btn" :class="user.isActive ? 'warning' : 'success'" @click="handleToggleStatus(user)" :title="user.isActive ? 'Khóa tài khoản' : 'Mở khóa'">
                        <svg v-if="user.isActive" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 9.9-1"></path></svg>
                      </button>

                      <button class="icon-btn danger" @click="handleDelete(user)" title="Xóa người dùng">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-if="!loading && users.length === 0" class="empty-state">
            <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><line x1="17" y1="8" x2="23" y2="14"></line><line x1="23" y1="8" x2="17" y2="14"></line></svg>
            <p>Chưa có dữ liệu người dùng nào trong hệ thống.</p>
          </div>
        </div>
      </section>

      <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
        <div class="modal-content">
          <div class="modal-header">
            <h2>{{ editingUserId ? "Cập nhật người dùng" : "Tạo người dùng mới" }}</h2>
            <button class="close-btn" @click="closeModal">&times;</button>
          </div>

          <div v-if="error" class="alert error">{{ error }}</div>

          <div class="modal-body">
            <div class="form-group">
              <label>Họ tên</label>
              <input v-model="form.fullName" placeholder="Bui The Hoang" />
            </div>

            <div class="form-group">
              <label>Email</label>
              <input v-model="form.email" type="email" placeholder="example@gmail.com" :disabled="!!editingUserId" />
            </div>

            <div v-if="!editingUserId" class="form-group grid-2-col">
              <div>
                <label>Mật khẩu</label>
                <input v-model="form.password" type="password" placeholder="••••••••" />
              </div>
              <div>
                <label>Role hệ thống</label>
                <select v-model="form.role">
                  <option v-for="role in roles" :key="role.id" :value="role.code">{{ role.name }}</option>
                </select>
              </div>
            </div>

            <div class="form-group grid-2-col">
              <div>
                <label>Phòng ban</label>
                <input v-model="form.department" placeholder="IT" />
              </div>
              <div>
                <label>Chức vụ</label>
                <input v-model="form.position" placeholder="Backend Dev" />
              </div>
            </div>

            <div class="form-group">
              <label>Số điện thoại</label>
              <input v-model="form.phoneNumber" placeholder="0912345678" />
            </div>

            <div class="form-group">
              <label>Avatar URL</label>
              <input v-model="form.avatarUrl" placeholder="https://example.com/avatar.png" />
            </div>
          </div>

          <div class="modal-actions">
            <button class="secondary-btn" @click="closeModal">Hủy</button>
            <button class="primary-btn" @click="handleSubmit" :disabled="loading">
              {{ editingUserId ? "Lưu cập nhật" : "Xác nhận tạo" }}
            </button>
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
  --input-bg: #ffffff;
  --primary: #4f46e5;
  --primary-hover: #4338ca;
  --sidebar-hover: #f1f5f9;
  --sidebar-active: #eef2ff;
  --sidebar-active-text: #4f46e5;
  --table-hover: #f8fafc;
  --card-shadow: 0 6px 18px rgba(15, 23, 42, 0.06);
  --modal-overlay: rgba(15, 23, 42, 0.5);
  
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
  --table-hover: #334155;
  --card-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
  --modal-overlay: rgba(0, 0, 0, 0.7);
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
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; }
.header-text h1 { margin: 0 0 6px 0; font-size: 24px; font-weight: 700; color: var(--text-main); }
.header-text p { margin: 0; color: var(--text-muted); font-size: 14px; }
.header-actions { display: flex; gap: 10px; }

/* BUTTONS */
.theme-toggle { background: var(--bg-card); border: 1px solid var(--border-color); color: var(--text-main); width: 38px; height: 38px; border-radius: 50%; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all 0.3s ease; box-shadow: var(--card-shadow); }
.theme-toggle:hover { border-color: var(--primary); }
.refresh-btn { display: flex; align-items: center; gap: 6px; background: var(--bg-card); color: var(--text-main); border: 1px solid var(--border-color); padding: 0 14px; border-radius: 10px; font-weight: 600; font-size: 13px; cursor: pointer; box-shadow: var(--card-shadow); transition: all 0.2s ease; }
.refresh-btn:hover { border-color: var(--primary); color: var(--primary); }
.primary-btn { display: flex; align-items: center; justify-content: center; gap: 6px; padding: 10px 14px; border: none; border-radius: 10px; background: var(--primary); color: white; font-weight: 600; font-size: 14px; cursor: pointer; transition: all 0.2s ease; box-shadow: var(--card-shadow); }
.primary-btn:hover:not(:disabled) { background: var(--primary-hover); transform: translateY(-1px); }
.primary-btn:disabled { opacity: 0.7; cursor: not-allowed; }
.secondary-btn { padding: 10px 14px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; background: var(--bg-body); color: var(--text-main); border: 1px solid var(--border-color); transition: all 0.2s; }
.secondary-btn:hover { background: var(--border-color); }

/* CONTENT SECTION */
.content-section { display: block; width: 100%; }
.card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 14px; padding: 18px; box-shadow: var(--card-shadow); transition: all 0.3s ease; }

/* TABLE UI */
.table-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--border-color); }
.table-header h2 { margin: 0; font-size: 16px; font-weight: 600; color: var(--text-main); }
.badge.count-badge { background: rgba(79, 70, 229, 0.1); color: var(--primary); font-size: 12px; }

.table-responsive { overflow-x: auto; }
table { width: 100%; border-collapse: collapse; text-align: left; font-size: 14px; }
th { font-size: 13px; font-weight: 600; color: var(--text-muted); border-bottom: 1px solid var(--border-color); padding: 10px 14px; white-space: nowrap; }
td { border-bottom: 1px solid var(--border-color); padding: 12px 14px; vertical-align: middle; }
tr:hover td { background-color: var(--table-hover); }
.text-right { text-align: right; }

.user-cell { display: flex; gap: 10px; align-items: center; }
.avatar { width: 34px; height: 34px; border-radius: 50%; object-fit: cover; flex-shrink: 0; }
.avatar.placeholder { display: flex; align-items: center; justify-content: center; background: rgba(79, 70, 229, 0.1); color: var(--primary); font-weight: 700; font-size: 14px; }
.user-name { display: block; color: var(--text-main); font-weight: 600; margin-bottom: 2px; }
.user-email { margin: 0; color: var(--text-muted); font-size: 12px; }
.dept-cell p { margin: 0 0 2px 0; font-weight: 500; }
.dept-cell small { color: var(--text-muted); font-size: 12px; }
.role-select { padding: 6px 10px; font-size: 13px; border-radius: 8px; width: auto; min-width: 110px; border: 1px solid var(--border-color); background: var(--input-bg); color: var(--text-main); outline: none; }
.login-time { color: var(--text-muted); font-size: 12px; white-space: nowrap; }

/* BADGES */
.badge { padding: 5px 9px; border-radius: 99px; font-size: 11px; font-weight: 600; display: inline-block; }
.badge.active { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.badge.inactive { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

/* ACTION BUTTONS (IN TABLE) */
.action-group { display: flex; gap: 6px; justify-content: flex-end; }
.icon-btn { width: 30px; height: 30px; border-radius: 8px; border: 1px solid var(--border-color); background: var(--bg-card); color: var(--text-main); display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all 0.2s ease; }
.icon-btn:hover { background: var(--table-hover); transform: translateY(-1px); }
.icon-btn.edit:hover { color: var(--primary); border-color: var(--primary); }
.icon-btn.warning:hover { color: #f59e0b; border-color: #f59e0b; }
.icon-btn.success:hover { color: #10b981; border-color: #10b981; }
.icon-btn.danger:hover { color: #ef4444; border-color: #ef4444; }

/* MODAL OVERLAY & CONTENT */
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: var(--modal-overlay);
  backdrop-filter: blur(2px);
  z-index: 100;
  display: flex; align-items: center; justify-content: center;
  padding: 20px;
}
.modal-content {
  background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 16px;
  width: 100%; max-width: 500px; padding: 24px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  z-index: 101; max-height: 90vh; overflow-y: auto;
}
.modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.modal-header h2 { margin: 0; font-size: 18px; color: var(--text-main); }
.close-btn { background: transparent; border: none; font-size: 24px; cursor: pointer; color: var(--text-muted); padding: 0; display: flex; align-items: center; justify-content: center; }
.close-btn:hover { color: #ef4444; }

/* FORM IN MODAL */
.modal-body { margin-bottom: 24px; }
.form-group { margin-bottom: 14px; }
.grid-2-col { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
label { display: block; margin-bottom: 6px; color: var(--text-main); font-weight: 600; font-size: 13px; }
input, select {
  width: 100%; box-sizing: border-box; padding: 10px 12px;
  background: var(--input-bg); color: var(--text-main); border: 1px solid var(--border-color);
  border-radius: 10px; font-size: 14px; transition: all 0.3s ease; outline: none; height: 40px;
}
input:disabled { opacity: 0.6; cursor: not-allowed; }
input:focus, select:focus { border-color: var(--primary); }
.modal-actions { display: flex; gap: 10px; justify-content: flex-end; border-top: 1px solid var(--border-color); padding-top: 16px; }

/* ALERTS & EMPTY */
.alert { padding: 12px 14px; border-radius: 10px; margin-bottom: 18px; font-weight: 600; font-size: 13px; }
.alert.error { background: rgba(239, 68, 68, 0.1); color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.2); }
.alert.success { background: rgba(16, 185, 129, 0.1); color: #10b981; border: 1px solid rgba(16, 185, 129, 0.2); }
.empty-state { text-align: center; padding: 40px 20px; color: var(--text-muted); font-size: 14px; }
.empty-state svg { margin-bottom: 12px; opacity: 0.5; }

/* SKELETON */
.skeleton { background: linear-gradient(90deg, var(--border-color) 25%, var(--bg-body) 50%, var(--border-color) 75%); background-size: 200% 100%; animation: loading 1.5s infinite; border-radius: 6px; }
.skeleton-line { height: 14px; margin-bottom: 6px; }
.skeleton-text-group { flex: 1; }
.w-full { width: 100%; } .w-3\/4 { width: 75%; } .w-1\/2 { width: 50%; }
@keyframes loading { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

/* RESPONSIVE */
@media (max-width: 900px) {
  .dashboard-layout { flex-direction: column; }
  .sidebar { width: 100%; border-right: none; border-bottom: 1px solid var(--border-color); padding: 16px; }
  .sidebar-header { margin-bottom: 16px; }
  .sidebar-nav { flex-direction: row; flex-wrap: wrap; }
  .btn-logout { margin-top: 16px; }
  .main-content { padding: 16px; }
  .grid-2-col { grid-template-columns: 1fr; }
}
</style>