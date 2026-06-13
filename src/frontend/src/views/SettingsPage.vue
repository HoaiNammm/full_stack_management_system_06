<template>
  <div class="page-wrap max-w-5xl">
    <section class="page-hero">
      <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-center lg:justify-between">
        <div class="flex items-center gap-md">
          <div class="w-16 h-16 rounded-2xl bg-primary flex items-center justify-center text-on-primary font-black text-2xl shadow-sm overflow-hidden">
            <img v-if="avatarSrc" :src="avatarSrc" class="w-full h-full object-cover" />
            <span v-else>{{ initial }}</span>
          </div>
          <div>
            <p class="page-eyebrow">Profile</p>
            <h1 class="font-headline-lg text-headline-lg text-on-surface">{{ user?.fullName || 'Người dùng' }}</h1>
            <p class="font-body-md text-body-md text-on-surface-variant">{{ user?.email }}</p>
          </div>
        </div>
        <button @click="handleLogout" class="inline-flex items-center justify-center gap-xs rounded-lg bg-error px-md py-sm text-white font-label-lg text-label-lg shadow-sm hover:opacity-90 active:scale-95 transition-all">
          <span class="material-symbols-outlined text-[18px]">logout</span>
          Đăng xuất
        </button>
      </div>
    </section>

    <div class="grid grid-cols-1 lg:grid-cols-[0.9fr_1.1fr] gap-lg">
      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm">
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Thông tin tài khoản</h2>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Bạn có thể cập nhật các thông tin cá nhân bên dưới.</p>
        </div>
        <div class="p-md">
          <p v-if="profileMessage" class="mb-md rounded-xl px-3 py-2 font-label-md text-label-md"
            :class="profileMessageType === 'success' ? 'bg-secondary/10 text-secondary' : 'bg-error-container/30 text-error'">
            {{ profileMessage }}
          </p>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-md">
            <label class="flex flex-col gap-xs md:col-span-2">
              <span class="font-label-lg text-label-lg text-on-surface">Họ tên <span class="text-error">*</span></span>
              <input v-model="profileForm.fullName" class="app-input font-body-md text-body-md" placeholder="Nhập họ tên" />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Email</span>
              <input :value="user?.email" class="app-input font-body-md text-body-md opacity-70 cursor-not-allowed" disabled />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Vai trò hệ thống</span>
              <div class="app-input flex items-center">
                <span class="soft-badge" :class="roleBadge">{{ user?.role || 'Member' }}</span>
              </div>
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Số điện thoại</span>
              <input v-model="profileForm.phoneNumber" class="app-input font-body-md text-body-md" placeholder="Ví dụ: 0987654321" />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Phòng ban</span>
              <input v-model="profileForm.department" class="app-input font-body-md text-body-md" placeholder="Ví dụ: Development" />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Chức vụ</span>
              <input v-model="profileForm.position" class="app-input font-body-md text-body-md" placeholder="Ví dụ: Project Manager" />
            </label>

            <div class="flex flex-col gap-xs md:col-span-2">
              <span class="font-label-lg text-label-lg text-on-surface">Ảnh đại diện</span>
              <div class="flex flex-col sm:flex-row sm:items-center gap-md rounded-2xl border border-outline-variant bg-surface-container-low p-md">
                <div class="w-20 h-20 rounded-2xl bg-primary flex items-center justify-center text-on-primary font-black text-2xl overflow-hidden">
                  <img v-if="avatarPreviewSrc" :src="avatarPreviewSrc" class="w-full h-full object-cover" />
                  <span v-else>{{ initial }}</span>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="font-label-lg text-label-lg text-on-surface">Upload JPG, PNG, WebP hoặc GIF</p>
                  <p class="font-body-sm text-body-sm text-on-surface-variant">Dung lượng tối đa 2MB. Ảnh sẽ được lưu ở NotifyService.</p>
                  <p v-if="avatarMessage" class="mt-xs font-label-sm text-label-sm" :class="avatarMessageType === 'success' ? 'text-secondary' : 'text-error'">{{ avatarMessage }}</p>
                </div>
                <div class="flex items-center gap-sm">
                  <input ref="avatarInput" type="file" accept="image/png,image/jpeg,image/webp,image/gif" class="hidden" @change="uploadAvatar" />
                  <button type="button" class="app-button-secondary" :disabled="uploadingAvatar" @click="avatarInput?.click()">
                    <span v-if="uploadingAvatar" class="material-symbols-outlined text-[18px] animate-spin">progress_activity</span>
                    <span v-else class="material-symbols-outlined text-[18px]">upload</span>
                    Chọn ảnh
                  </button>
                </div>
              </div>
            </div>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Avatar URL</span>
              <input v-model="profileForm.avatarUrl" class="app-input font-body-md text-body-md" placeholder="https://..." />
            </label>
          </div>

          <div class="mt-md flex justify-end gap-sm">
            <button @click="resetProfileForm" class="app-button-secondary" :disabled="savingProfile">Hoàn tác</button>
            <button @click="saveProfile" class="app-button-primary" :disabled="savingProfile || !profileForm.fullName.trim()">
              <span v-if="savingProfile" class="material-symbols-outlined text-[18px] animate-spin">progress_activity</span>
              Lưu hồ sơ
            </button>
          </div>
        </div>
      </section>

      <section class="app-panel">
        <div class="border-b border-outline-variant px-md py-sm">
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Bảo mật tài khoản</h2>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Các thao tác cá nhân của người dùng.</p>
        </div>
        <div class="p-md flex flex-col gap-md">
          <p v-if="passwordMessage" class="rounded-xl px-3 py-2 font-label-md text-label-md"
            :class="passwordMessageType === 'success' ? 'bg-secondary/10 text-secondary' : 'bg-error-container/30 text-error'">
            {{ passwordMessage }}
          </p>

          <div class="grid grid-cols-1 gap-md">
            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Mật khẩu hiện tại</span>
              <PasswordInput v-model="passwordForm.currentPassword" placeholder="Nhập mật khẩu hiện tại" />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Mật khẩu mới</span>
              <PasswordInput v-model="passwordForm.newPassword" placeholder="Ít nhất 6 ký tự" />
            </label>

            <label class="flex flex-col gap-xs">
              <span class="font-label-lg text-label-lg text-on-surface">Xác nhận mật khẩu mới</span>
              <PasswordInput v-model="passwordForm.confirmPassword" placeholder="Nhập lại mật khẩu mới" />
            </label>
          </div>

          <button @click="changePassword" class="app-button-primary w-fit self-end"
            :disabled="savingPassword || !canChangePassword">
            <span v-if="savingPassword" class="material-symbols-outlined text-[18px] animate-spin">progress_activity</span>
            Đổi mật khẩu
          </button>

          <div class="data-row flex items-center justify-between gap-md">
            <div>
              <p class="font-label-lg text-label-lg text-on-surface">Trạng thái backend</p>
              <p class="font-body-sm text-body-sm text-on-surface-variant">Đã tách sang trang System Status riêng.</p>
            </div>
            <button @click="$router.push('/system-status')" class="app-button-secondary">
              <span class="material-symbols-outlined text-[18px]">monitor_heart</span>
              Mở System Status
            </button>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { computed, defineComponent, h, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { authService, NOTIFY_ORIGIN } from '../services/api'

const router = useRouter()
const { user, logout, refreshUser } = useAuth()
const savingProfile = ref(false)
const savingPassword = ref(false)
const uploadingAvatar = ref(false)
const avatarInput = ref(null)
const profileMessage = ref('')
const passwordMessage = ref('')
const avatarMessage = ref('')
const profileMessageType = ref('success')
const passwordMessageType = ref('success')
const avatarMessageType = ref('success')

const profileForm = reactive({
  fullName: '',
  phoneNumber: '',
  department: '',
  position: '',
  avatarUrl: '',
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const initial = computed(() => (user.value?.fullName || user.value?.email || 'U').charAt(0).toUpperCase())
const resolveAvatarUrl = (url) => {
  if (!url) return ''
  if (/^https?:\/\//i.test(url) || url.startsWith('data:') || url.startsWith('blob:')) return url
  return `${NOTIFY_ORIGIN}${url.startsWith('/') ? url : `/${url}`}`
}
const avatarSrc = computed(() => resolveAvatarUrl(user.value?.avatar || user.value?.avatarUrl))
const avatarPreviewSrc = computed(() => resolveAvatarUrl(profileForm.avatarUrl) || avatarSrc.value)
const roleBadge = computed(() => {
  const map = {
    Owner: 'bg-primary/10 text-primary',
    Manager: 'bg-secondary/10 text-secondary',
    Member: 'bg-surface-container text-on-surface-variant',
    Viewer: 'bg-surface-container text-outline',
    ProjectManager: 'bg-secondary/10 text-secondary',
  }
  return map[user.value?.role] || 'bg-surface-container text-on-surface-variant'
})

const canChangePassword = computed(() =>
  passwordForm.currentPassword &&
  passwordForm.newPassword.length >= 6 &&
  passwordForm.confirmPassword === passwordForm.newPassword
)

const PasswordInput = defineComponent({
  props: {
    modelValue: { type: String, default: '' },
    placeholder: { type: String, default: '' },
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    const visible = ref(false)
    return () => h('div', { class: 'relative' }, [
      h('input', {
        value: props.modelValue,
        type: visible.value ? 'text' : 'password',
        placeholder: props.placeholder,
        class: 'app-input font-body-md text-body-md pr-10',
        onInput: event => emit('update:modelValue', event.target.value),
      }),
      h('button', {
        type: 'button',
        title: visible.value ? 'Ẩn mật khẩu' : 'Hiện mật khẩu',
        class: 'absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant hover:text-on-surface transition-colors',
        onClick: () => { visible.value = !visible.value },
      }, [
        h('span', { class: 'material-symbols-outlined text-[18px]' }, visible.value ? 'visibility_off' : 'visibility')
      ])
    ])
  }
})

function resetProfileForm() {
  profileForm.fullName = user.value?.fullName || ''
  profileForm.phoneNumber = user.value?.phoneNumber || ''
  profileForm.department = user.value?.department || ''
  profileForm.position = user.value?.position || ''
  profileForm.avatarUrl = user.value?.avatarUrl || user.value?.avatar || ''
}

async function saveProfile() {
  if (!profileForm.fullName.trim() || savingProfile.value) return
  savingProfile.value = true
  profileMessage.value = ''
  try {
    const updated = await authService.updateProfile({
      fullName: profileForm.fullName.trim(),
      phoneNumber: profileForm.phoneNumber || null,
      department: profileForm.department || null,
      position: profileForm.position || null,
      avatarUrl: profileForm.avatarUrl || null,
    })
    const avatar = resolveAvatarUrl(updated.avatar || updated.avatarUrl)
    user.value = { ...updated, avatar, avatarUrl: avatar || updated.avatarUrl }
    localStorage.setItem('user', JSON.stringify(updated))
    resetProfileForm()
    profileMessageType.value = 'success'
    profileMessage.value = 'Cập nhật hồ sơ thành công'
  } catch (e) {
    profileMessageType.value = 'error'
    profileMessage.value = e.response?.data?.message || 'Không thể cập nhật hồ sơ'
  } finally {
    savingProfile.value = false
  }
}

async function uploadAvatar(event) {
  const file = event.target.files?.[0]
  event.target.value = ''
  avatarMessage.value = ''
  if (!file) return

  if (!file.type.startsWith('image/')) {
    avatarMessageType.value = 'error'
    avatarMessage.value = 'Vui lòng chọn file ảnh'
    return
  }

  if (file.size > 2 * 1024 * 1024) {
    avatarMessageType.value = 'error'
    avatarMessage.value = 'Ảnh đại diện tối đa 2MB'
    return
  }

  uploadingAvatar.value = true
  try {
    const updated = await authService.uploadAvatar(file)
    const avatar = resolveAvatarUrl(updated.avatar || updated.avatarUrl)
    user.value = { ...updated, avatar, avatarUrl: avatar || updated.avatarUrl }
    localStorage.setItem('user', JSON.stringify(updated))
    resetProfileForm()
    avatarMessageType.value = 'success'
    avatarMessage.value = 'Cập nhật ảnh đại diện thành công'
  } catch (e) {
    avatarMessageType.value = 'error'
    avatarMessage.value = e.response?.data?.message || 'Không thể upload ảnh đại diện'
  } finally {
    uploadingAvatar.value = false
  }
}

async function changePassword() {
  passwordMessage.value = ''
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordMessageType.value = 'error'
    passwordMessage.value = 'Mật khẩu xác nhận không khớp'
    return
  }
  savingPassword.value = true
  try {
    await authService.changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    })
    passwordForm.currentPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    passwordMessageType.value = 'success'
    passwordMessage.value = 'Đổi mật khẩu thành công'
  } catch (e) {
    passwordMessageType.value = 'error'
    passwordMessage.value = e.response?.data?.message || 'Không thể đổi mật khẩu'
  } finally {
    savingPassword.value = false
  }
}

function handleLogout() {
  logout()
  router.push('/login')
}

onMounted(() => {
  refreshUser()
  resetProfileForm()
})

watch(user, resetProfileForm, { deep: true })
</script>
