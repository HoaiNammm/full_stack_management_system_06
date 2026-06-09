<template>
  <div class="min-h-screen bg-surface flex items-center justify-center p-4">
    <div class="w-full max-w-sm">
      <!-- Brand -->
      <div class="flex items-center gap-3 justify-center mb-xl">
        <div class="w-12 h-12 rounded-xl bg-primary flex items-center justify-center text-on-primary font-bold font-headline-sm">PM</div>
        <div>
          <h1 class="font-headline-md text-headline-md font-bold text-primary">Project Manager</h1>
          <p class="font-label-sm text-label-sm text-on-surface-variant">Microservices Architecture</p>
        </div>
      </div>

      <div class="bg-surface-container-lowest rounded-2xl shadow-sm border border-outline-variant p-lg flex flex-col gap-md">
        <h2 class="font-headline-sm text-headline-sm text-on-surface text-center">Tạo tài khoản</h2>

        <!-- Error -->
        <p v-if="error" class="text-error font-label-md text-label-md bg-error-container/30 rounded-lg px-3 py-2 flex items-center gap-2">
          <span class="material-symbols-outlined text-[16px]">error</span>{{ error }}
        </p>

        <!-- Full name -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Họ và tên <span class="text-error">*</span></label>
          <input v-model="form.fullName" type="text" @keyup.enter="handleRegister"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
              errors.fullName ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
            placeholder="Nguyễn Văn A" />
          <p v-if="errors.fullName" class="font-label-sm text-label-sm text-error">{{ errors.fullName }}</p>
        </div>

        <!-- Email -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Email <span class="text-error">*</span></label>
          <input v-model="form.email" type="email" @keyup.enter="handleRegister"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
              errors.email ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
            placeholder="email@company.com" />
          <p v-if="errors.email" class="font-label-sm text-label-sm text-error">{{ errors.email }}</p>
        </div>

        <!-- Password -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Mật khẩu <span class="text-error">*</span></label>
          <div class="relative">
            <input v-model="form.password" :type="showPassword ? 'text' : 'password'" @keyup.enter="handleRegister"
              :class="['w-full px-3 py-2 pr-10 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
                errors.password ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
              placeholder="Ít nhất 6 ký tự" />
            <button type="button" @click="showPassword = !showPassword"
              class="absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors">
              <span class="material-symbols-outlined text-[18px]">{{ showPassword ? 'visibility_off' : 'visibility' }}</span>
            </button>
          </div>
          <p v-if="errors.password" class="font-label-sm text-label-sm text-error">{{ errors.password }}</p>
        </div>

        <!-- Confirm password -->
        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Xác nhận mật khẩu <span class="text-error">*</span></label>
          <input v-model="form.confirmPassword" :type="showPassword ? 'text' : 'password'" @keyup.enter="handleRegister"
            :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-low outline-none transition-all',
              errors.confirmPassword ? 'border-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
            placeholder="Nhập lại mật khẩu" />
          <p v-if="errors.confirmPassword" class="font-label-sm text-label-sm text-error">{{ errors.confirmPassword }}</p>
        </div>

        <!-- Submit -->
        <button @click="handleRegister" :disabled="loading"
          class="w-full py-sm bg-primary text-on-primary rounded-lg font-label-lg text-label-lg shadow-sm hover:opacity-90 transition-opacity disabled:opacity-60 flex items-center justify-center gap-2">
          <span v-if="loading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
          {{ loading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản' }}
        </button>

        <!-- Login link -->
        <p class="font-label-sm text-label-sm text-on-surface-variant text-center">
          Đã có tài khoản?
          <RouterLink to="/login" class="text-primary hover:underline font-medium">Đăng nhập</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '../services/api'
import { useAuth } from '../composables/useAuth'

const router       = useRouter()
const { login }    = useAuth()
const showPassword = ref(false)
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
    // Auto-login after successful registration
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
