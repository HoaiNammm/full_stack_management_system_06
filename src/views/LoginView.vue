<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { Eye, EyeOff, AlertCircle } from 'lucide-vue-next'
import { BaseInput, BaseButton } from '@/components/base'

const router = useRouter()
const authStore = useAuthStore()

const form = ref({ email: '', password: '' })
const showPassword = ref(false)

async function handleSubmit() {
  authStore.clearError()
  const ok = await authStore.login(form.value)
  if (ok) router.push('/')
}
</script>

<template>
  <div class="min-h-dvh flex items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4 py-12">
    <div class="w-full max-w-md">

      <!-- Brand mark -->
      <div class="mb-8 flex flex-col items-center">
        <router-link to="/welcome" class="mb-3 flex h-14 w-14 items-center justify-center overflow-hidden rounded-2xl bg-white shadow-lg shadow-black/5 ring-1 ring-zinc-200 dark:ring-zinc-800">
          <img src="/logo.png" alt="Task logo" class="h-full w-full object-contain p-1.5" />
        </router-link>
        <h1 class="text-xl font-semibold tracking-tight text-zinc-900 dark:text-zinc-100">Sign in</h1>
        <p class="mt-1 text-sm text-zinc-500 dark:text-zinc-400">Welcome back to your workspace</p>
      </div>

      <!-- Card -->
      <div class="rounded-lg border border-zinc-200 bg-white p-8 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">

        <!-- Server error -->
        <div
          v-if="authStore.error"
          role="alert"
          class="mb-5 flex items-start gap-2.5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-900 dark:bg-red-950/40 dark:text-red-400"
        >
          <AlertCircle class="mt-0.5 size-4 shrink-0" aria-hidden="true" />
          <span>{{ authStore.error }}</span>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-4">

          <BaseInput
            id="email"
            v-model="form.email"
            type="email"
            label="Email"
            placeholder="you@example.com"
            autocomplete="email"
            required
          />

          <!-- Password — inline toggle, same visual style as BaseInput -->
          <div class="flex flex-col gap-1">
            <label for="password" class="flex select-none items-center justify-between text-sm font-medium text-zinc-700 dark:text-zinc-300">
              <span>Password<span class="ml-0.5 text-red-500" aria-hidden="true">*</span></span>
              <router-link
                to="/forgot-password"
                class="text-xs font-medium text-blue-600 transition-colors hover:underline dark:text-blue-400"
              >
                Forgot password?
              </router-link>
            </label>
            <div class="relative">
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                required
                autocomplete="current-password"
                placeholder="••••••••"
                class="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 pr-10 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100 dark:placeholder:text-zinc-500"
              />
              <button
                type="button"
                :aria-label="showPassword ? 'Hide password' : 'Show password'"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 rounded p-1.5 text-zinc-400 transition-colors hover:text-zinc-600 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:hover:text-zinc-300"
              >
                <EyeOff v-if="showPassword" class="size-4" aria-hidden="true" />
                <Eye v-else class="size-4" aria-hidden="true" />
              </button>
            </div>
          </div>

          <BaseButton
            type="submit"
            variant="primary"
            :loading="authStore.loading"
            :full-width="true"
          >
            {{ authStore.loading ? 'Signing in…' : 'Sign in' }}
          </BaseButton>

        </form>

        <p class="mt-6 text-center text-sm text-zinc-500 dark:text-zinc-400">
          Don't have an account?
          <router-link
            to="/register"
            class="font-medium text-blue-600 transition-colors hover:underline dark:text-blue-400"
          >
            Create one
          </router-link>
        </p>

      </div>
    </div>
  </div>
</template>
