<template>
  <div class="min-h-screen app-shell flex items-center justify-center p-md">
    <button
      @click="toggleTheme"
      class="fixed right-4 top-4 app-button-ghost rounded-full"
      :title="isDark ? 'Chuyển sang Light mode' : 'Chuyển sang Dark mode'"
    >
      <span class="material-symbols-outlined">{{ isDark ? 'light_mode' : 'dark_mode' }}</span>
    </button>

    <div class="w-full max-w-5xl grid grid-cols-1 lg:grid-cols-[1.1fr_0.9fr] gap-lg items-stretch">
      <section class="hidden lg:flex app-panel overflow-hidden p-0 flex-col justify-between min-h-[560px]">
        <div class="relative h-60">
          <img :src="authVisual" class="h-full w-full object-cover" alt="" />
          <div class="absolute inset-0 bg-gradient-to-t from-black/65 via-black/15 to-transparent"></div>
          <div class="absolute bottom-5 left-5 flex items-center gap-sm">
            <div class="w-12 h-12 rounded-xl bg-white/90 text-primary flex items-center justify-center font-black shadow-sm">PM</div>
            <div>
              <h1 class="font-headline-md text-headline-md text-white">Project Manager</h1>
              <p class="font-label-md text-label-md text-white/80">Workspace demo</p>
            </div>
          </div>
        </div>
        <div class="flex flex-1 flex-col justify-between p-xl">
        <div>
          <div class="hidden flex items-center gap-sm mb-xl">
            <div class="w-12 h-12 rounded-xl bg-primary text-on-primary flex items-center justify-center font-black shadow-sm">PM</div>
            <div>
              <h1 class="font-headline-md text-headline-md text-on-surface">Project Manager</h1>
              <p class="font-label-md text-label-md text-on-surface-variant">Microservices workspace</p>
            </div>
          </div>
          <h2 class="text-[40px] leading-[48px] font-bold text-on-surface">
            Quản lý dự án, task và thành viên trong một workspace.
          </h2>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-md max-w-xl">
            Dashboard, Kanban, timeline, phân quyền và thông báo được kết nối trực tiếp với backend hiện tại.
          </p>
        </div>

        <div class="grid grid-cols-3 gap-sm">
          <div v-for="item in highlights" :key="item.label" class="rounded-xl bg-surface-container-low p-md border border-outline-variant">
            <span class="material-symbols-outlined text-primary">{{ item.icon }}</span>
            <p class="font-label-lg text-label-lg text-on-surface mt-2">{{ item.label }}</p>
            <p class="font-label-sm text-label-sm text-on-surface-variant">{{ item.text }}</p>
          </div>
        </div>
        </div>
      </section>

      <section class="app-panel p-lg md:p-xl flex flex-col justify-center">
        <div class="lg:hidden flex items-center gap-sm mb-lg">
          <div class="w-11 h-11 rounded-xl bg-primary text-on-primary flex items-center justify-center font-black">PM</div>
          <div>
            <h1 class="font-headline-sm text-headline-sm text-on-surface">Project Manager</h1>
            <p class="font-label-sm text-label-sm text-on-surface-variant">Team workspace</p>
          </div>
        </div>

        <p class="page-eyebrow">Welcome back</p>
        <h2 class="font-headline-md text-headline-md text-on-surface">Đăng nhập</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1 mb-lg">
          Sử dụng tài khoản demo hoặc tài khoản bạn đã đăng ký.
        </p>

        <p v-if="error" class="text-error font-label-md text-label-md bg-error-container/30 rounded-lg px-3 py-2 flex items-center gap-2 mb-md">
          <span class="material-symbols-outlined text-[16px]">error</span>{{ error }}
        </p>

        <div class="flex flex-col gap-md">
          <label class="flex flex-col gap-xs">
            <span class="font-label-lg text-label-lg text-on-surface">Email</span>
            <input v-model="form.email" type="email" @keyup.enter="handleLogin"
              class="app-input w-full px-3 py-2.5 rounded-lg font-body-md text-body-md"
              placeholder="email@company.com" />
          </label>

          <label class="flex flex-col gap-xs">
            <span class="font-label-lg text-label-lg text-on-surface">Mật khẩu</span>
            <div class="relative">
              <input v-model="form.password" :type="showPassword ? 'text' : 'password'" @keyup.enter="handleLogin"
                class="app-input w-full px-3 py-2.5 pr-10 rounded-lg font-body-md text-body-md"
                placeholder="••••••" />
              <button type="button" @click="showPassword = !showPassword"
                class="absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors"
                :title="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'">
                <span class="material-symbols-outlined text-[18px]">{{ showPassword ? 'visibility_off' : 'visibility' }}</span>
              </button>
            </div>
          </label>

          <button @click="handleLogin" :disabled="loading" class="app-button-primary w-full">
            <span v-if="loading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
            {{ loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
          </button>
        </div>

        <div class="mt-md rounded-lg bg-surface-container-low border border-outline-variant px-3 py-2 font-label-sm text-label-sm text-on-surface-variant flex flex-wrap items-center gap-1">
          <span>Demo:</span>
          <code class="text-on-surface bg-surface-container-high px-1.5 py-0.5 rounded">1@example.com</code>
          <span>/</span>
          <code class="text-on-surface bg-surface-container-high px-1.5 py-0.5 rounded">123456</code>
        </div>

        <p class="font-label-md text-label-md text-on-surface-variant text-center mt-lg">
          Chưa có tài khoản?
          <RouterLink to="/register" class="text-primary hover:underline font-bold">Đăng ký</RouterLink>
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
const route  = useRoute()
const { login } = useAuth()
const { isDark, toggleTheme } = useTheme()

const form    = ref({ email: '', password: '' })
const loading = ref(false)
const error   = ref('')
const showPassword = ref(false)
const highlights = [
  { icon: 'dashboard', label: 'Dashboard', text: 'Số liệu thật' },
  { icon: 'view_kanban', label: 'Kanban', text: 'Kéo thả task' },
  { icon: 'notifications', label: 'Notify', text: 'Bình luận, nhắc việc' },
]

async function handleLogin() {
  if (!form.value.email || !form.value.password) {
    error.value = 'Vui lòng nhập email và mật khẩu'
    return
  }
  loading.value = true
  error.value   = ''
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
