<template>
  <div class="flex flex-col gap-lg">
    <!-- Header -->
    <div class="gradient-panel p-lg overflow-hidden">
      <div class="relative z-10 flex flex-col gap-md md:flex-row md:items-center md:justify-between">
        <div>
          <p class="page-eyebrow">Project members</p>
          <h3 class="font-headline-md text-headline-md text-on-surface">Đội ngũ dự án</h3>
          <p class="font-body-md text-body-md text-on-surface-variant mt-1">
            Quản lý thành viên, vai trò và quyền truy cập trong workspace.
          </p>
        </div>
        <button v-if="canAddMembers" @click="showAddForm = !showAddForm"
          class="app-button-primary flex items-center gap-xs">
          <span class="material-symbols-outlined text-[18px]">{{ showAddForm ? 'close' : 'person_add' }}</span>
          {{ showAddForm ? 'Đóng' : 'Thêm thành viên' }}
        </button>
        <span v-else class="soft-badge bg-surface-container text-on-surface-variant">
          Chỉ Owner/Manager được thêm thành viên
        </span>
      </div>
      <div class="relative z-10 grid grid-cols-2 md:grid-cols-4 gap-sm mt-lg">
        <div class="metric-tile">
          <span>Members</span>
          <strong>{{ members.length }}</strong>
        </div>
        <div class="metric-tile">
          <span>Owner</span>
          <strong>{{ members.filter(m => m.role === 'Owner').length }}</strong>
        </div>
        <div class="metric-tile">
          <span>Builders</span>
          <strong>{{ members.filter(m => ['Developer', 'Tester'].includes(m.role)).length }}</strong>
        </div>
        <div class="metric-tile">
          <span>Viewers</span>
          <strong>{{ members.filter(m => m.role === 'Viewer').length }}</strong>
        </div>
      </div>
    </div>

    <!-- Add Form -->
    <Transition name="slide">
      <div v-if="showAddForm && canAddMembers"
        class="glass-card p-lg flex flex-col gap-md">
      <div>
          <h4 class="font-headline-sm text-headline-sm text-on-surface">Thêm thành viên mới</h4>
          <p class="font-body-md text-body-md text-on-surface-variant">Nhập email tài khoản đã đăng ký và chọn vai trò phù hợp.</p>
      </div>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-md">
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Email <span class="text-error">*</span></label>
            <input v-model="addForm.email" type="email"
              :class="['app-input',
                addError ? 'border-error focus:ring-1 focus:ring-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
              placeholder="user@company.com" />
            <p v-if="addError" class="font-label-sm text-label-sm text-error flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px]">error</span>{{ addError }}
            </p>
          </div>
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Tên hiển thị</label>
            <input v-model="addForm.name" type="text"
              class="app-input"
              placeholder="Nguyễn Văn A" />
          </div>
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Vai trò</label>
            <select v-model="addForm.role"
              class="app-input">
              <option v-for="r in editableRoles" :key="r">{{ r }}</option>
            </select>
          </div>
        </div>
        <div class="flex gap-3 justify-end">
          <button @click="showAddForm = false"
            class="app-button-secondary">
            Hủy
          </button>
          <button @click="handleAdd" :disabled="addLoading"
            class="app-button-primary flex items-center gap-xs disabled:opacity-60">
            <span v-if="addLoading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
            <span v-else class="material-symbols-outlined text-[18px]">person_add</span>
            Thêm thành viên
          </button>
        </div>
      </div>
    </Transition>

    <!-- Role Legend -->
    <div class="app-panel p-md">
      <div class="flex items-center justify-between gap-3 mb-3">
        <div>
          <h4 class="font-headline-sm text-headline-sm text-on-surface">Role matrix</h4>
          <p class="font-body-md text-body-md text-on-surface-variant">Phân quyền theo trách nhiệm trong dự án.</p>
        </div>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-sm">
        <div v-for="r in roleInfo" :key="r.name" class="rounded-2xl border border-outline-variant bg-surface-container-low p-md">
          <span class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="r.style">{{ r.name }}</span>
          <p class="font-label-sm text-label-sm text-on-surface-variant mt-2">{{ r.desc }}</p>
        </div>
      </div>
    </div>

    <!-- Members Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-md">
      <div v-for="m in members" :key="m.id" class="workspace-card group">
        <div class="flex items-start justify-between gap-3">
          <div class="flex items-center gap-3 min-w-0">
            <div class="w-12 h-12 rounded-2xl flex items-center justify-center font-bold text-white shadow-sm flex-shrink-0"
              :style="{ backgroundColor: m.avatarColor }">{{ (m.name || '?')[0] }}</div>
            <div class="min-w-0">
              <h4 class="font-headline-sm text-headline-sm text-on-surface truncate">{{ m.name }}</h4>
              <p class="font-label-md text-label-md text-on-surface-variant truncate">{{ m.email || 'Chưa có email' }}</p>
            </div>
          </div>
          <button v-if="canManageRoles && m.role !== 'Owner'"
            @click="removeMember(m)"
            class="opacity-70 group-hover:opacity-100 text-on-surface-variant hover:text-error transition-all p-2 rounded-xl hover:bg-error-container/30">
            <span class="material-symbols-outlined text-[18px]">person_remove</span>
          </button>
        </div>
        <div class="mt-md flex items-center justify-between gap-3">
          <select v-if="canManageRoles && m.role !== 'Owner'" :value="m.role" @change="updateRole(m, $event.target.value)"
            class="text-[11px] font-bold px-3 py-1 rounded-full border-0 outline-none cursor-pointer appearance-none"
            :class="roleStyle(m.role)">
            <option v-for="r in editableRoles" :key="r" :value="r">{{ r }}</option>
          </select>
          <span v-else class="text-[11px] font-bold px-3 py-1 rounded-full" :class="roleStyle(m.role)">{{ m.role }}</span>
          <span class="font-label-sm text-label-sm text-on-surface-variant">Joined {{ m.joinedAt || 'recently' }}</span>
        </div>
      </div>
    </div>

    <!-- Members Table -->
    <div class="app-panel overflow-hidden">
      <div class="px-md py-sm border-b border-outline-variant">
        <h4 class="font-headline-sm text-headline-sm text-on-surface">Member directory</h4>
      </div>
      <table class="w-full">
        <thead>
          <tr class="bg-surface-container-low border-b border-outline-variant">
            <th class="text-left px-md py-sm font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Thành viên</th>
            <th class="text-left px-md py-sm font-label-md text-label-md text-on-surface-variant uppercase tracking-wider hidden md:table-cell">Email</th>
            <th class="text-left px-md py-sm font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Vai trò</th>
            <th class="text-left px-md py-sm font-label-md text-label-md text-on-surface-variant uppercase tracking-wider hidden sm:table-cell">Ngày tham gia</th>
            <th class="px-md py-sm w-12"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-outline-variant/50">
          <tr v-for="m in members" :key="m.id" class="hover:bg-surface-container-low/50 transition-colors group">
            <td class="px-md py-sm">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-[12px] text-white flex-shrink-0"
                  :style="{ backgroundColor: m.avatarColor }">{{ m.name[0] }}</div>
                <span class="font-label-lg text-label-lg text-on-surface">{{ m.name }}</span>
              </div>
            </td>
            <td class="px-md py-sm font-body-md text-body-md text-on-surface-variant hidden md:table-cell">{{ m.email }}</td>
            <td class="px-md py-sm">
              <!-- Owner cannot be changed -->
              <span v-if="!canManageRoles || m.role === 'Owner'"
                class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="roleStyle(m.role)">
                {{ m.role }}
              </span>
              <!-- Other roles can be changed -->
              <select v-else :value="m.role" @change="updateRole(m, $event.target.value)"
                class="text-[11px] font-bold px-2 py-0.5 rounded-full border-0 outline-none cursor-pointer appearance-none pr-4"
                :class="roleStyle(m.role)">
                <option v-for="r in editableRoles" :key="r" :value="r">{{ r }}</option>
              </select>
            </td>
            <td class="px-md py-sm font-label-md text-label-md text-on-surface-variant hidden sm:table-cell">
              {{ m.joinedAt }}
            </td>
            <td class="px-md py-sm">
              <button v-if="canManageRoles && m.role !== 'Owner'"
                @click="removeMember(m)"
                class="opacity-0 group-hover:opacity-100 text-on-surface-variant hover:text-error transition-all p-1 rounded hover:bg-error-container/30">
                <span class="material-symbols-outlined text-[18px]">person_remove</span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useAuth } from '../composables/useAuth'
import { projectService, userService } from '../services/api'

const props = defineProps(['project'])
const emit  = defineEmits(['event', 'reload'])
const { user } = useAuth()

const members     = ref(props.project.members.map(m => ({ ...m })))
const showAddForm = ref(false)
const addError    = ref('')
const addLoading  = ref(false)
const editableRoles = ['Project Manager', 'Developer', 'Tester', 'Viewer']
const addForm = ref({ email: '', role: 'Developer' })

const ROLE_INT = { Owner: 0, 'Project Manager': 1, Developer: 2, Tester: 3, Viewer: 4 }
const avatarColors = ['#3525cd', '#006a61', '#684000', '#ba1a1a', '#0f5e9c', '#6a0dad', '#2e7d32', '#e65100']

const roleInfo = [
  { name: 'Owner',           style: 'bg-primary/10 text-primary', desc: 'Toàn quyền quản lý' },
  { name: 'Project Manager', style: 'bg-secondary/10 text-secondary', desc: 'Quản lý dự án' },
  { name: 'Developer',       style: 'bg-surface-container text-on-surface-variant', desc: 'Thực hiện công việc' },
  { name: 'Tester',          style: 'bg-tertiary/10 text-tertiary', desc: 'Kiểm thử' },
  { name: 'Viewer',          style: 'bg-surface-container text-outline', desc: 'Chỉ xem' },
]

const currentMember = computed(() =>
  members.value.find(member => String(member.userId).toLowerCase() === String(user.value?.id || '').toLowerCase())
)
const currentRole = computed(() => currentMember.value?.role || '')
const canAddMembers = computed(() => ['Owner', 'Project Manager'].includes(currentRole.value))
const canManageRoles = computed(() => currentRole.value === 'Owner')

function roleStyle(role) {
  return roleInfo.find(r => r.name === role)?.style || ''
}

async function updateRole(member, newRole) {
  if (!canManageRoles.value || member.role === 'Owner') return
  const oldRole = member.role
  member.role = newRole
  try {
    await projectService.updateMemberRole(props.project.id, member.id, { role: ROLE_INT[newRole] ?? 2 })
  } catch {
    member.role = oldRole
  }
}

async function removeMember(m) {
  if (!canManageRoles.value || m.role === 'Owner') return
  members.value = members.value.filter(x => x.id !== m.id)
  try {
    await projectService.removeMember(props.project.id, m.id)
    emit('reload')
  } catch {
    members.value.push(m)
  }
}

async function handleAdd() {
  addError.value = ''
  if (!canAddMembers.value) { addError.value = 'Bạn không có quyền thêm thành viên'; return }
  if (!addForm.value.email.trim()) { addError.value = 'Vui lòng nhập email'; return }
  if (members.value.find(m => m.email === addForm.value.email)) {
    addError.value = 'Email này đã là thành viên'; return
  }

  addLoading.value = true
  try {
    // Look up user by email
    const allUsers = await userService.getAll()
    const user = (allUsers || []).find(u => u.email?.toLowerCase() === addForm.value.email.toLowerCase())
    if (!user) { addError.value = 'Không tìm thấy người dùng với email này'; return }

    await projectService.addMember(props.project.id, { userId: user.id, role: ROLE_INT[addForm.value.role] ?? 2 })
    showAddForm.value = false
    addForm.value = { email: '', role: 'Developer' }
    emit('event', {
      type: 'project.member.added', icon: 'person_add', iconBg: 'bg-secondary/10 text-secondary',
      summary: `${user.fullName || user.email} được thêm với vai trò ${addForm.value.role}`,
      time: new Date().toLocaleTimeString('vi-VN'),
    })
    emit('reload')
  } catch (e) {
    addError.value = e.response?.data?.message || 'Không thể thêm thành viên'
  } finally {
    addLoading.value = false
  }
}
</script>

<style scoped>
.slide-enter-active, .slide-leave-active { transition: all 0.25s ease; }
.slide-enter-from, .slide-leave-to { opacity: 0; transform: translateY(-12px); }
</style>
