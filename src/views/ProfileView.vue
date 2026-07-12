<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/authStore'
import { AlertCircle, Camera, CheckCircle, Lock, User } from 'lucide-vue-next'
import { BaseButton } from '@/components/base'

const authStore = useAuthStore()

const activeTab = ref('profile')

// Profile form — backend UpdateUserRequest only supports name + avatarUrl
const profileForm = ref({ name: '' })
const profileMsg  = ref('')
const profileErr  = ref('')

// Password form
const pwForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })
const pwMsg  = ref('')
const pwErr  = ref('')

// Avatar
const avatarInput    = ref(null)
const avatarErr      = ref('')
const avatarUploading = ref(false)

function readFileAsDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload  = () => resolve(reader.result)
    reader.onerror = () => reject(reader.error)
    reader.readAsDataURL(file)
  })
}

const user = computed(() => authStore.user)

onMounted(async () => {
  await authStore.fetchMe()
  profileForm.value.name = authStore.user?.fullName || authStore.user?.name || ''
})

async function handleUpdateProfile() {
  profileMsg.value = ''
  profileErr.value = ''
  if (!profileForm.value.name.trim()) { profileErr.value = 'Name is required'; return }
  const ok = await authStore.updateProfile({ name: profileForm.value.name.trim() })
  if (ok) profileMsg.value = 'Profile updated successfully.'
  else     profileErr.value = authStore.error || 'Update failed.'
}

async function handleChangePassword() {
  pwMsg.value = ''
  pwErr.value = ''
  if (!pwForm.value.currentPassword || !pwForm.value.newPassword) {
    pwErr.value = 'All fields are required.'
    return
  }
  if (pwForm.value.newPassword !== pwForm.value.confirmPassword) {
    pwErr.value = 'New passwords do not match.'
    return
  }
  if (pwForm.value.newPassword.length < 6) {
    pwErr.value = 'Password must be at least 6 characters.'
    return
  }
  const ok = await authStore.changePassword({
    currentPassword: pwForm.value.currentPassword,
    newPassword:     pwForm.value.newPassword,
  })
  if (ok) {
    pwMsg.value = 'Password changed successfully.'
    pwForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  } else {
    pwErr.value = authStore.error || 'Password change failed.'
  }
}

async function handleAvatarChange(e) {
  avatarErr.value = ''
  const file = e.target.files?.[0]
  if (!file) return
  if (!file.type.startsWith('image/')) { avatarErr.value = 'Please select an image file.'; e.target.value = ''; return }
  if (file.size > 2 * 1024 * 1024) { avatarErr.value = 'File must be under 2 MB.'; e.target.value = ''; return }
  avatarUploading.value = true
  try {
    const dataUrl = await readFileAsDataUrl(file)
    const ok = await authStore.updateProfile({ avatarUrl: dataUrl })
    if (!ok) avatarErr.value = authStore.error || 'Failed to update avatar.'
  } catch {
    avatarErr.value = 'Failed to read image file.'
  } finally {
    avatarUploading.value = false
    e.target.value = ''
  }
}

const inputCls = [
  'w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-900',
  'transition focus-visible:border-blue-500 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500',
  'dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200',
].join(' ')

const tabs = [
  { key: 'profile',  label: 'Profile',  icon: User },
  { key: 'password', label: 'Password', icon: Lock },
]
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-6 text-zinc-900 dark:text-zinc-100">
    <div class="page-header-banner">
      <h1 class="text-xl font-semibold text-white">Profile Settings</h1>
    </div>

    <!-- Avatar card -->
    <div class="flex items-center gap-5 rounded-lg border border-zinc-200 bg-white p-5 dark:border-zinc-800 dark:bg-zinc-900">
      <div class="relative">
        <img
          v-if="user?.avatarUrl"
          :src="user.avatarUrl"
          alt="Profile avatar"
          class="size-20 rounded-full border-2 border-zinc-200 object-cover dark:border-zinc-700"
        />
        <div
          v-else
          class="flex size-20 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-blue-600 text-2xl font-bold text-white"
          aria-hidden="true"
        >
          {{ user?.name?.[0]?.toUpperCase() || '?' }}
        </div>
        <button
          @click="avatarInput?.click()"
          aria-label="Change avatar"
          class="absolute bottom-0 right-0 flex size-6 items-center justify-center rounded-full bg-blue-500 text-white shadow transition hover:bg-blue-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-1"
        >
          <Camera class="size-3" aria-hidden="true" />
        </button>
        <input ref="avatarInput" type="file" accept="image/*" class="hidden" @change="handleAvatarChange" />
      </div>
      <div>
        <p class="font-medium text-zinc-900 dark:text-zinc-100">{{ user?.name || '—' }}</p>
        <p class="text-sm text-zinc-500 dark:text-zinc-400">{{ user?.email }}</p>
        <p v-if="avatarErr" role="alert" class="mt-1 text-xs text-red-500">{{ avatarErr }}</p>
      </div>
    </div>

    <!-- Tab bar -->
    <div
      role="tablist"
      aria-label="Profile sections"
      class="flex gap-1 border-b border-zinc-200 dark:border-zinc-800"
    >
      <button
        v-for="tab in tabs"
        :key="tab.key"
        role="tab"
        :aria-selected="activeTab === tab.key"
        @click="activeTab = tab.key"
        :class="[
          'flex items-center gap-2 -mb-px border-b-2 px-4 py-2 text-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-blue-500 focus-visible:ring-offset-1',
          activeTab === tab.key
            ? 'border-blue-500 font-medium text-blue-600 dark:text-blue-400'
            : 'border-transparent text-zinc-500 hover:text-zinc-800 dark:text-zinc-400 dark:hover:text-zinc-200',
        ]"
      >
        <component :is="tab.icon" class="size-3.5" aria-hidden="true" /> {{ tab.label }}
      </button>
    </div>

    <!-- Profile tab panel -->
    <div
      v-if="activeTab === 'profile'"
      role="tabpanel"
      aria-label="Profile"
      class="space-y-4 rounded-lg border border-zinc-200 bg-white p-5 dark:border-zinc-800 dark:bg-zinc-900"
    >
      <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Edit Profile</h2>

      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">Full Name</label>
        <input v-model="profileForm.name" :class="inputCls" placeholder="Your name" autocomplete="name" />
      </div>
      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">Email</label>
        <input :value="user?.email" :class="inputCls + ' cursor-not-allowed opacity-60'" type="email" disabled />
        <p class="mt-1 text-xs text-zinc-400 dark:text-zinc-500">Email cannot be changed</p>
      </div>

      <div v-if="profileMsg" class="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
        <CheckCircle class="size-4" aria-hidden="true" /> {{ profileMsg }}
      </div>
      <div v-if="profileErr" role="alert" class="flex items-center gap-2 text-sm text-red-500">
        <AlertCircle class="size-4" aria-hidden="true" /> {{ profileErr }}
      </div>

      <div class="flex justify-end">
        <BaseButton
          variant="primary"
          size="sm"
          :loading="authStore.loading"
          @click="handleUpdateProfile"
        >
          Save Changes
        </BaseButton>
      </div>
    </div>

    <!-- Password tab panel -->
    <div
      v-else
      role="tabpanel"
      aria-label="Password"
      class="space-y-4 rounded-lg border border-zinc-200 bg-white p-5 dark:border-zinc-800 dark:bg-zinc-900"
    >
      <h2 class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Change Password</h2>
      <p class="text-xs text-zinc-500 dark:text-zinc-400">Password must be at least 6 characters.</p>

      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">Current Password</label>
        <input v-model="pwForm.currentPassword" :class="inputCls" type="password" placeholder="Current password" autocomplete="current-password" />
      </div>
      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">New Password</label>
        <input v-model="pwForm.newPassword" :class="inputCls" type="password" placeholder="New password" autocomplete="new-password" />
      </div>
      <div>
        <label class="mb-1 block text-xs font-medium text-zinc-500 dark:text-zinc-400">Confirm New Password</label>
        <input
          v-model="pwForm.confirmPassword"
          :class="inputCls"
          type="password"
          placeholder="Repeat new password"
          autocomplete="new-password"
          @keydown.enter="handleChangePassword"
        />
      </div>

      <div v-if="pwMsg" class="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
        <CheckCircle class="size-4" aria-hidden="true" /> {{ pwMsg }}
      </div>
      <div v-if="pwErr" role="alert" class="flex items-center gap-2 text-sm text-red-500">
        <AlertCircle class="size-4" aria-hidden="true" /> {{ pwErr }}
      </div>

      <div class="flex justify-end">
        <BaseButton
          variant="primary"
          size="sm"
          :loading="authStore.loading"
          @click="handleChangePassword"
        >
          Change Password
        </BaseButton>
      </div>
    </div>
  </div>
</template>
