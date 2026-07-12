<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/authStore'
import { AlertCircle, CheckCircle2 } from 'lucide-vue-next'
import { BaseInput, BaseButton } from '@/components/base'

const authStore = useAuthStore()

const email = ref('')
const submitted = ref(false)

async function handleSubmit() {
  authStore.clearError()
  const ok = await authStore.forgotPassword(email.value)
  if (ok) submitted.value = true
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
        <h1 class="text-xl font-semibold tracking-tight text-zinc-900 dark:text-zinc-100">Forgot password</h1>
        <p class="mt-1 text-sm text-zinc-500 dark:text-zinc-400">We'll help you get back into your account</p>
      </div>

      <!-- Card -->
      <div class="rounded-lg border border-zinc-200 bg-white p-8 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">

        <div v-if="submitted" class="text-center">
          <div class="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-50 dark:bg-green-950/40">
            <CheckCircle2 class="size-6 text-green-600 dark:text-green-400" aria-hidden="true" />
          </div>
          <p class="text-sm text-zinc-700 dark:text-zinc-300">
            If an account exists for <span class="font-medium">{{ email }}</span>, a password reset link has been generated.
          </p>
          <p class="mt-3 text-xs text-zinc-400 dark:text-zinc-500">
            Dev mode: no email service is configured yet, so the reset link is printed to the NotifyService server console instead of being emailed.
          </p>
        </div>

        <template v-else>
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
              v-model="email"
              type="email"
              label="Email"
              placeholder="you@example.com"
              autocomplete="email"
              required
            />

            <BaseButton
              type="submit"
              variant="primary"
              :loading="authStore.loading"
              :full-width="true"
            >
              {{ authStore.loading ? 'Sending…' : 'Send reset link' }}
            </BaseButton>
          </form>
        </template>

        <p class="mt-6 text-center text-sm text-zinc-500 dark:text-zinc-400">
          Remembered your password?
          <router-link
            to="/login"
            class="font-medium text-blue-600 transition-colors hover:underline dark:text-blue-400"
          >
            Back to sign in
          </router-link>
        </p>

      </div>
    </div>
  </div>
</template>
