<template>
  <div class="flex flex-col gap-md">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h3 class="font-headline-sm text-headline-sm text-on-surface">Thành viên dự án</h3>
        <p class="font-body-md text-body-md text-on-surface-variant mt-0.5">{{ members.length }} thành viên</p>
      </div>
      <button @click="showAddForm = !showAddForm"
        class="flex items-center gap-xs px-md py-sm rounded-lg font-label-lg text-label-lg transition-colors"
        :class="showAddForm ? 'bg-surface-container-high text-on-surface border border-outline-variant' : 'bg-primary text-on-primary hover:opacity-90'">
        <span class="material-symbols-outlined text-[18px]">{{ showAddForm ? 'close' : 'person_add' }}</span>
        {{ showAddForm ? 'Đóng' : 'Thêm thành viên' }}
      </button>
    </div>

    <!-- Add Form -->
    <Transition name="slide">
      <div v-if="showAddForm"
        class="bg-surface-container-low rounded-xl border border-outline-variant p-md flex flex-col gap-md">
        <h4 class="font-headline-sm text-headline-sm text-on-surface">Thêm thành viên mới</h4>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-md">
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Email <span class="text-error">*</span></label>
            <input v-model="addForm.email" type="email"
              :class="['w-full px-3 py-2 rounded-lg border font-body-md text-body-md bg-surface-container-lowest outline-none transition-all',
                addError ? 'border-error focus:ring-1 focus:ring-error' : 'border-outline-variant focus:border-primary focus:ring-1 focus:ring-primary']"
              placeholder="user@company.com" />
            <p v-if="addError" class="font-label-sm text-label-sm text-error flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px]">error</span>{{ addError }}
            </p>
          </div>
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Tên hiển thị</label>
            <input v-model="addForm.name" type="text"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-lowest outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all"
              placeholder="Nguyễn Văn A" />
          </div>
          <div class="flex flex-col gap-xs">
            <label class="font-label-lg text-label-lg text-on-surface">Vai trò</label>
            <select v-model="addForm.role"
              class="w-full px-3 py-2 rounded-lg border border-outline-variant font-body-md text-body-md bg-surface-container-lowest outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all">
              <option v-for="r in editableRoles" :key="r">{{ r }}</option>
            </select>
          </div>
        </div>
        <div class="flex gap-3 justify-end">
          <button @click="showAddForm = false"
            class="px-md py-sm rounded-lg border border-outline-variant font-label-lg text-label-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
            Hủy
          </button>
          <button @click="handleAdd" :disabled="addLoading"
            class="flex items-center gap-xs px-md py-sm rounded-lg bg-primary text-on-primary font-label-lg text-label-lg hover:opacity-90 transition-opacity disabled:opacity-60">
            <span v-if="addLoading" class="material-symbols-outlined animate-spin text-[18px]">progress_activity</span>
            <span v-else class="material-symbols-outlined text-[18px]">person_add</span>
            Thêm thành viên
          </button>
        </div>
      </div>
    </Transition>

    <!-- Role Legend -->
    <div class="flex gap-3 flex-wrap p-3 bg-surface-container-low rounded-lg border border-outline-variant/50">
      <div v-for="r in roleInfo" :key="r.name" class="flex items-center gap-2">
        <span class="text-[11px] font-bold px-2 py-0.5 rounded-full" :class="r.style">{{ r.name }}</span>
        <span class="font-label-sm text-label-sm text-on-surface-variant">{{ r.desc }}</span>
      </div>
    </div>

    <!-- Members Table -->
    <div class="bg-surface-container-lowest rounded-xl border border-outline-variant overflow-hidden">
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
              <span v-if="m.role === 'Owner'"
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
              <button v-if="m.role !== 'Owner'"
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
import { ref } from 'vue'
import { projectService, userService } from '../services/api'

const props = defineProps(['project'])
const emit  = defineEmits(['event', 'reload'])

const members     = ref(props.project.members.map(m => ({ ...m })))
const showAddForm = ref(false)
const addError    = ref('')
const addLoading  = ref(false)
const editableRoles = ['Manager', 'Member', 'Viewer']
const addForm = ref({ email: '', role: 'Member' })

const ROLE_INT = { Owner: 0, Manager: 1, Member: 2, Viewer: 3 }
const ROLE_STR = ['Owner', 'Manager', 'Member', 'Viewer']
const avatarColors = ['#3525cd', '#006a61', '#684000', '#ba1a1a', '#0f5e9c', '#6a0dad', '#2e7d32', '#e65100']

const roleInfo = [
  { name: 'Owner',   style: 'bg-primary/10 text-primary',             desc: 'Toàn quyền quản lý' },
  { name: 'Manager', style: 'bg-secondary/10 text-secondary',         desc: 'Quản lý task & sprint' },
  { name: 'Member',  style: 'bg-surface-container text-on-surface-variant', desc: 'Tham gia cập nhật task' },
  { name: 'Viewer',  style: 'bg-surface-container text-outline',      desc: 'Chỉ xem' },
]

function roleStyle(role) {
  return roleInfo.find(r => r.name === role)?.style || ''
}

async function updateRole(member, newRole) {
  const oldRole = member.role
  member.role = newRole
  try {
    await projectService.updateMemberRole(props.project.id, member.id, { role: ROLE_INT[newRole] ?? 2 })
  } catch {
    member.role = oldRole
  }
}

async function removeMember(m) {
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
    addForm.value = { email: '', role: 'Member' }
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
