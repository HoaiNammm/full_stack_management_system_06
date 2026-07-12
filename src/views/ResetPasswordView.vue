<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { Eye, EyeOff, AlertCircle, CheckCircle2 } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const token = computed(() => route.query.token || '')
const newPassword = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const submitted = ref(false)
const mismatchError = ref('')

async function handleSubmit() {
  authStore.clearError()
  mismatchError.value = ''

  if (newPassword.value !== confirmPassword.value) {
    mismatchError.value = 'Passwords do not match'
    return
  }

  const ok = await authStore.resetPassword({ token: token.value, newPassword: newPassword.value })
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
        <h1 class="text-xl font-semibold tracking-tight text-zinc-900 dark:text-zinc-100">Reset password</h1>
        <p class="mt-1 text-sm text-zinc-500 dark:text-zinc-400">Choose a new password for your account</p>
      </div>

      <!-- Card -->
      <div class="rounded-lg border border-zinc-200 bg-white p-8 shadow-sm dark:border-zinc-800 dark:bg-zinc-900">

        <div v-if="submitted" class="text-center">
          <div class="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-50 dark:bg-green-950/40">
            <CheckCircle2 class="size-6 text-green-600 dark:text-green-400" aria-hidden="true" />
          </div>
          <p class="text-sm text-zinc-700 dark:text-zinc-300">
            Your password has been reset successfully.
          </p>
          <router-link
            to="/login"
            class="mt-5 inline-flex h-9 items-center justify-center rounded-lg bg-blue-600 px-4 text-sm font-medium text-white transition-colors hover:bg-blue-700"
          >
            Go to sign in
          </router-link>
        </div>

        <template v-else-if="!token">
          <div role="alert" class="flex items-start gap-2.5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-900 dark:bg-red-950/40 dark:text-red-400">
            <AlertCircle class="mt-0.5 size-4 shrink-0" aria-hidden="true" />
            <span>This reset link is missing its token. Please request a new one.</span>
          </div>
          <router-link
            to="/forgot-password"
            class="mt-4 block text-center text-sm font-medium text-blue-600 transition-colors hover:underline dark:text-blue-400"
          >
            Request a new reset link
          </router-link>
        </template>

        <template v-else>
          <div
            v-if="authStore.error || mismatchError"
            role="alert"
            class="mb-5 flex items-start gap-2.5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-900 dark:bg-red-950/40 dark:text-red-400"
          >
            <AlertCircle class="mt-0.5 size-4 shrink-0" aria-hidden="true" />
            <span>{{ mismatchError || authStore.error }}</span>
          </div>

          <form @submit.prevent="handleSubmit" class="space-y-4">
            <div class="flex flex-col gap-1">
              <label for="new-password" class="select-none text-sm font-medium text-zinc-700 dark:text-zinc-300">
                New password<span class="ml-0.5 text-red-500" aria-hidden="true">*</span>
              </label>
              <div class="relative">
                <input
                  id="new-password"
                  v-model="newPassword"
                  :type="showPassword ? 'text' : 'password'"
                  required
                  minlength="6"
                  autocomplete="new-password"
                  placeholder="Min. 6 characters"
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

            <div class="flex flex-col gap-1">
              <label for="confirm-password" class="select-none text-sm font-medium text-zinc-700 dark:text-zinc-300">
                Confirm new password<span class="ml-0.5 text-red-500" aria-hidden="true">*</span>
              </label>
              <input
                id="confirm-password"
                v-model="confirmPassword"
                :type="showPassword ? 'text' : 'password'"
                required
                minlength="6"
                autocomplete="new-password"
                placeholder="Re-enter new password"
                class="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900 placeholder:text-zinc-400 transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100 dark:placeholder:text-zinc-500"
              />
            </div>

            <BaseButton
              type="submit"
              variant="primary"
              :loading="authStore.loading"
              :full-width="true"
            >
              {{ authStore.loading ? 'Resetting…' : 'Reset password' }}
            </BaseButton>
          </form>
        </template>

      </div>
    </div>
  </div>
</template>
