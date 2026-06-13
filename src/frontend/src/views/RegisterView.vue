<template>
  <div class="min-h-screen app-shell flex items-center justify-center p-md">
    <button
      @click="toggleTheme"
      class="fixed right-4 top-4 app-button-ghost rounded-full"
      :title="isDark ? 'Chuyển sang Light mode' : 'Chuyển sang Dark mode'"
    >
      <span class="material-symbols-outlined">{{ isDark ? 'light_mode' : 'dark_mode' }}</span>
    </button>

    <section class="app-panel w-full max-w-md p-lg md:p-xl">
      <div class="flex items-center gap-sm mb-lg">
        <div class="w-11 h-11 rounded-xl bg-primary text-on-primary flex items-center justify-center font-black">PM</div>
        <div>
          <h1 class="font-headline-sm text-headline-sm text-on-surface">Project Manager</h1>
          <p class="font-label-sm text-label-sm text-on-surface-variant">Tạo workspace account</p>
        </div>
      </div>

      <p class="page-eyebrow">Get started</p>
      <h2 class="font-headline-md text-headline-md text-on-surface">Tạo tài khoản</h2>
      <p class="font-body-md text-body-md text-on-surface-variant mt-1 mb-lg">
        Tài khoản sẽ được lưu trong NotifyService và đăng nhập bằng JWT.
      </p>

      <p v-if="error" class="text-error font-label-md text-label-md bg-error-container/30 rounded-lg px-3 py-2 flex items-center gap-2 mb-md">
        <span class="material-symbols-outlined text-[16px]">error</span>{{ error }}
      </p>

      <div class="flex flex-col gap-md">
        <label class="flex flex-col gap-xs">
          <span class="font-label-lg text-label-lg text-on-surface">Họ và tên <span class="text-error">*</span></span>
          <input v-model="form.fullName" type="text" @keyup.enter="handleRegister"
            :class="['app-input w-full px-3 py-2.5 rounded-lg font-body-md text-body-md', errors.fullName ? 'border-error ring-2 ring-error/10' : '']"
            placeholder="Nguyễn Văn A" />
          <p v-if="errors.fullName" class="font-label-sm text-label-sm text-error">{{ errors.fullName }}</p>
        </label>

        <label class="flex flex-col gap-xs">
          <span class="font-label-lg text-label-lg text-on-surface">Email <span class="text-error">*</span></span>
          <input v-model="form.email" type="email" @keyup.enter="handleRegister"
            :class="['app-input w-full px-3 py-2.5 rounded-lg font-body-md text-body-md', errors.email ? 'border-error ring-2 ring-error/10' : '']"
            placeholder="email@company.com" />
          <p v-if="errors.email" class="font-label-sm text-label-sm text-error">{{ errors.email }}</p>
        </label>

        <label class="flex flex-col gap-xs">
          <span class="font-label-lg text-label-lg text-on-surface">Mật khẩu <span class="text-error">*</span></span>
          <div class="relative">
            <input v-model="form.password" :type="showPassword ? 'text' : 'password'" @keyup.enter="handleRegister"
              :class="['app-input w-full px-3 py-2.5 pr-10 rounded-lg font-body-md text-body-md', errors.password ? 'border-error ring-2 ring-error/10' : '']"
              placeholder="Ít nhất 6 ký tự" />
            <button type="button" @click="showPassword = !showPassword"
              class="absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors">
              <span class="material-symbols-outlined text-[18px]">{{ showPassword ? 'visibility_off' : 'visibility' }}</span>
            </button>
          </div>
          <p v-if="errors.password" class="font-label-sm text-label-sm text-error">{{ errors.password }}</p>
        </label>

        <label class="flex flex-col gap-xs">
          <span class="font-label-lg text-label-lg text-on-surface">Xác nhận mật khẩu <span class="text-error">*</span></span>
          <div class="relative">
            <input v-model="form.confirmPassword" :type="showConfirmPassword ? 'text' : 'password'" @keyup.enter="handleRegister"
              :class="['app-input w-full px-3 py-2.5 pr-10 rounded-lg font-body-md text-body-md', errors.confirmPassword ? 'border-error ring-2 ring-error/10' : '']"
              placeholder="Nhập lại mật khẩu" />
            <button type="button" @click="showConfirmPassword = !showConfirmPassword"
              class="absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors"
              :title="showConfirmPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'">
              <span class="material-symbols-outlined text-[18px]">{{ showConfirmPassword ? 'visibility_off' : 'visibility' }}</span>
            </button>
          </div>
          <p v-if="errors.confirmPassword" class="font-label-sm text-label-sm text-error">{{ errors.confirmPassword }}</p>
        </label>

        <button @click="handleRegister" :disabled="loading" class="app-button-primary w-full">
          <span v-if="loading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
          {{ loading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản' }}
        </button>
      </div>

      <p class="font-label-md text-label-md text-on-surface-variant text-center mt-lg">
        Đã có tài khoản?
        <RouterLink to="/login" class="text-primary hover:underline font-bold">Đăng nhập</RouterLink>
      </p>
    </section>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '../services/api'
import { useAuth } from '../composables/useAuth'
import { useTheme } from '../services/theme'

const router       = useRouter()
const { login }    = useAuth()
const { isDark, toggleTheme } = useTheme()
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const loading      = ref(false)
const error        = ref('')
const errors       = ref({})

const form = ref({ fullName: '', email: '', password: '', confirmPassword: '' })

function validate() {
  errors.value = {}
  if (!form.value.fullName.trim())
    errors.value.fullName = 'Vui lòng nhập họ và tên'
  if (!form.value.email.trim())
    errors.value.email = 'Vui lòng nhập email'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
    errors.value.email = 'Email không hợp lệ'
  if (!form.value.password)
    errors.value.password = 'Vui lòng nhập mật khẩu'
  else if (form.value.password.length < 6)
    errors.value.password = 'Mật khẩu ít nhất 6 ký tự'
  if (form.value.confirmPassword !== form.value.password)
    errors.value.confirmPassword = 'Mật khẩu xác nhận không khớp'
  return Object.keys(errors.value).length === 0
}

async function handleRegister() {
  if (!validate()) return
  loading.value = true
  error.value   = ''
  try {
    await authService.register({
      fullName: form.value.fullName.trim(),
      email:    form.value.email.trim(),
      password: form.value.password,
    })
    await login(form.value.email.trim(), form.value.password)
    router.push('/dashboard')
  } catch (e) {
    const code = e.response?.data?.error?.code
    if (code === 'EMAIL_TAKEN')
      errors.value.email = 'Email này đã được sử dụng'
    else
      error.value = e.response?.data?.message || 'Đăng ký thất bại. Kiểm tra NotifyService.'
  } finally {
    loading.value = false
  }
}
</script>
