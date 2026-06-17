<template>
  <div class="min-h-screen app-shell flex items-center justify-center p-md">
    <button
      @click="toggleTheme"
      class="fixed right-4 top-4 app-button-ghost rounded-full"
      :title="isDark ? 'Chuyển sang Light mode' : 'Chuyển sang Dark mode'"
    >
      <span class="material-symbols-outlined">{{ isDark ? 'light_mode' : 'dark_mode' }}</span>
    </button>

    <div
      class="w-full max-w-6xl grid grid-cols-1 lg:grid-cols-[1.15fr_0.85fr] gap-lg items-stretch"
    >
      <section
        class="hidden lg:flex app-panel overflow-hidden p-0 flex-col justify-between min-h-[590px]"
      >
        <div class="relative h-64">
          <img :src="authVisual" class="h-full w-full object-cover" alt="" />
          <div
            class="absolute inset-0 bg-gradient-to-t from-black/75 via-black/25 to-transparent"
          ></div>

          <div class="absolute bottom-5 left-5 flex items-center gap-sm">
            <div
              class="w-12 h-12 rounded-xl bg-white/95 text-primary flex items-center justify-center font-black shadow-sm"
            >
              PM
            </div>
            <div>
              <h1 class="font-headline-md text-headline-md text-white">Project Manager</h1>
              <p class="font-label-md text-label-md text-white/80">Microservices workspace</p>
            </div>
          </div>
        </div>

        <div class="flex flex-1 flex-col justify-between p-xl">
          <div>
            <div class="flex flex-wrap gap-sm mb-md">
              <span
                v-for="service in services"
                :key="service"
                class="rounded-full bg-primary/10 text-primary px-3 py-1 font-label-sm text-label-sm"
              >
                {{ service }}
              </span>
            </div>

            <h2 class="text-[40px] leading-[48px] font-bold text-on-surface">
              Hệ thống quản lý dự án & phân công công việc.
            </h2>

            <p class="font-body-lg text-body-lg text-on-surface-variant mt-md max-w-xl">
              Kết nối Project, Member, Task, Kanban, Comment và Notification trong một workspace
              thống nhất để nhóm dễ quản lý tiến độ và cộng tác.
            </p>

            <div class="grid grid-cols-3 gap-sm mt-lg">
              <div
                v-for="stat in stats"
                :key="stat.label"
                class="rounded-xl bg-surface-container-low p-md border border-outline-variant"
              >
                <p class="font-headline-sm text-headline-sm text-primary">{{ stat.value }}</p>
                <p class="font-label-sm text-label-sm text-on-surface-variant">{{ stat.label }}</p>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-3 gap-sm mt-lg">
            <div
              v-for="item in highlights"
              :key="item.label"
              class="rounded-xl bg-surface-container-low p-md border border-outline-variant"
            >
              <span class="material-symbols-outlined text-primary">{{ item.icon }}</span>
              <p class="font-label-lg text-label-lg text-on-surface mt-2">{{ item.label }}</p>
              <p class="font-label-sm text-label-sm text-on-surface-variant">{{ item.text }}</p>
            </div>
          </div>
        </div>
      </section>

      <section class="app-panel p-lg md:p-xl flex flex-col justify-center">
        <div class="lg:hidden flex items-center gap-sm mb-lg">
          <div
            class="w-11 h-11 rounded-xl bg-primary text-on-primary flex items-center justify-center font-black"
          >
            PM
          </div>
          <div>
            <h1 class="font-headline-sm text-headline-sm text-on-surface">Project Manager</h1>
            <p class="font-label-sm text-label-sm text-on-surface-variant">Team workspace</p>
          </div>
        </div>

        <p class="page-eyebrow">Welcome back</p>
        <h2 class="font-headline-md text-headline-md text-on-surface">Đăng nhập workspace</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1 mb-lg">
          Truy cập hệ thống demo để quản lý dự án, task, thành viên và thông báo của nhóm.
        </p>

        <p
          v-if="error"
          class="text-error font-label-md text-label-md bg-error-container/30 rounded-lg px-3 py-2 flex items-center gap-2 mb-md"
        >
          <span class="material-symbols-outlined text-[16px]">error</span>
          {{ error }}
        </p>

        <div class="flex flex-col gap-md">
          <label class="flex flex-col gap-xs">
            <span class="font-label-lg text-label-lg text-on-surface">Email</span>
            <input
              v-model="form.email"
              type="email"
              @keyup.enter="handleLogin"
              class="app-input w-full px-3 py-2.5 rounded-lg font-body-md text-body-md"
              placeholder="demo@projectmanager.vn"
            />
          </label>

          <label class="flex flex-col gap-xs">
            <span class="font-label-lg text-label-lg text-on-surface">Mật khẩu</span>
            <div class="relative">
              <input
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                @keyup.enter="handleLogin"
                class="app-input w-full px-3 py-2.5 pr-10 rounded-lg font-body-md text-body-md"
                placeholder="••••••"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors"
                :title="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
              >
                <span class="material-symbols-outlined text-[18px]">
                  {{ showPassword ? 'visibility_off' : 'visibility' }}
                </span>
              </button>
            </div>
          </label>

          <button @click="handleLogin" :disabled="loading" class="app-button-primary w-full">
            <span v-if="loading" class="material-symbols-outlined animate-spin text-[18px]"
              >progress_activity</span
            >
            {{ loading ? 'Đang đăng nhập...' : 'Truy cập hệ thống' }}
          </button>
        </div>

        <div
          class="mt-md rounded-xl bg-surface-container-low border border-outline-variant px-3 py-3 font-label-sm text-label-sm text-on-surface-variant"
        >
          <div class="flex items-center justify-between gap-2">
            <div>
              <p class="font-label-md text-label-md text-on-surface">Tài khoản demo</p>
              <p class="text-on-surface-variant">Dùng để trình bày nhanh trong buổi demo.</p>
            </div>

            <button
              type="button"
              @click="fillDemoAccount"
              class="rounded-lg bg-primary/10 text-primary px-3 py-2 font-label-md text-label-md hover:bg-primary/15 transition-colors"
            >
              Điền nhanh
            </button>
          </div>

          <div class="mt-sm flex flex-wrap items-center gap-1">
            <code class="text-on-surface bg-surface-container-high px-1.5 py-0.5 rounded"
              >1@example.com</code
            >
            <span>/</span>
            <code class="text-on-surface bg-surface-container-high px-1.5 py-0.5 rounded"
              >123456</code
            >
          </div>
        </div>

        <p class="font-label-md text-label-md text-on-surface-variant text-center mt-lg">
          Chưa có tài khoản?
          <RouterLink to="/register" class="text-primary hover:underline font-bold"
            >Đăng ký</RouterLink
          >
        </p>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { useTheme } from '../services/theme'
import { authVisual } from '../services/visualAssets'

const router = useRouter()
const route = useRoute()
const { login } = useAuth()
const { isDark, toggleTheme } = useTheme()

const form = ref({ email: '', password: '' })
const loading = ref(false)
const error = ref('')
const showPassword = ref(false)

const services = ['Project & Member Service', 'Task & Kanban Service', 'Comment & Notify Service']

const stats = [
  { value: '3', label: 'Service tích hợp' },
  { value: '8+', label: 'Màn hình quản lý' },
  { value: '100%', label: 'Luồng demo FE' },
]

const highlights = [
  { icon: 'folder_shared', label: 'Project', text: 'Dự án & thành viên' },
  { icon: 'view_kanban', label: 'Kanban', text: 'Quản lý task' },
  { icon: 'forum', label: 'Discuss', text: 'Bình luận, thông báo' },
]

function fillDemoAccount() {
  form.value.email = '1@example.com'
  form.value.password = '123456'
}

async function handleLogin() {
  if (!form.value.email || !form.value.password) {
    error.value = 'Vui lòng nhập email và mật khẩu'
    return
  }

  loading.value = true
  error.value = ''

  try {
    await login(form.value.email, form.value.password)
    const redirect = route.query.redirect || '/dashboard'
    router.push(redirect)
  } catch (e) {
    error.value = e.response?.data?.message || 'Email hoặc mật khẩu không đúng'
  } finally {
    loading.value = false
  }
}
</script>
