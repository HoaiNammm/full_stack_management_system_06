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
        <h2 class="font-headline-sm text-headline-sm text-on-surface text-center">Đăng nhập</h2>

        <p v-if="error" class="text-error font-label-md text-label-md bg-error-container/30 rounded-lg px-3 py-2 flex items-center gap-2">
          <span class="material-symbols-outlined text-[16px]">error</span>{{ error }}
        </p>

        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Email</label>
          <input v-model="form.email" type="email" @keyup.enter="handleLogin"
            class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-low outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all"
            placeholder="email@company.com" />
        </div>

        <div class="flex flex-col gap-xs">
          <label class="font-label-lg text-label-lg text-on-surface">Mật khẩu</label>
          <input v-model="form.password" type="password" @keyup.enter="handleLogin"
            class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-low outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all"
            placeholder="••••••" />
        </div>

        <button @click="handleLogin" :disabled="loading"
          class="w-full py-sm bg-primary text-on-primary rounded-lg font-label-lg text-label-lg shadow-sm hover:opacity-90 transition-opacity disabled:opacity-60 flex items-center justify-center gap-2">
          <span v-if="loading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
          {{ loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
        </button>

        <p class="font-label-sm text-label-sm text-on-surface-variant text-center">
          Demo: <code class="bg-surface-container px-1 rounded">1@example.com</code> / <code class="bg-surface-container px-1 rounded">123456</code>
        </p>

        <p class="font-label-sm text-label-sm text-on-surface-variant text-center">
          Chưa có tài khoản?
          <RouterLink to="/register" class="text-primary hover:underline font-medium">Đăng ký</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuth } from '../composables/useAuth'

const router = useRouter()
const route  = useRoute()
const { login } = useAuth()

const form    = ref({ email: '', password: '' })
const loading = ref(false)
const error   = ref('')

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
