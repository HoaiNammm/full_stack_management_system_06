<template>
  <div class="page-wrap">
    <section class="page-hero">
      <div class="relative z-10 flex flex-col gap-md lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="page-eyebrow">People</p>
          <h2 class="font-headline-lg text-headline-lg text-on-surface">Danh sách thành viên</h2>
          <p class="font-body-lg text-body-lg text-on-surface-variant mt-1">
            Quản lý và tra cứu người dùng trong hệ thống, kết hợp dữ liệu thành viên dự án hiện có.
          </p>
        </div>
      <button
        @click="$router.push('/projects')"
        class="app-button-primary"
      >
        <span class="material-symbols-outlined text-[18px]">group_add</span>
        Thêm vào dự án
      </button>
      </div>
    </section>

    <div v-if="error" class="bg-error-container/20 border border-error/20 rounded-lg px-md py-sm text-error font-label-md text-label-md">
      {{ error }}
    </div>

    <div class="grid grid-cols-1 md:grid-cols-4 gap-md">
      <div class="app-card p-md">
        <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Tổng thành viên</span>
        <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ users.length }}</p>
      </div>
      <div class="app-card p-md">
        <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Đang hoạt động</span>
        <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ activeUsers }}</p>
      </div>
      <div class="app-card p-md">
        <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Có trong dự án</span>
        <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ usersInProjects }}</p>
      </div>
      <div class="app-card p-md">
        <span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Tổng dự án</span>
        <p class="font-headline-lg text-headline-lg text-on-surface mt-2">{{ projects.length }}</p>
      </div>
    </div>

    <div class="app-panel">
      <div class="px-md py-sm border-b border-outline-variant flex flex-col gap-sm lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Thành viên hệ thống</h3>
          <p class="font-body-sm text-body-sm text-on-surface-variant">Click vào một dòng để xem dự án liên quan.</p>
        </div>
        <div class="flex items-center gap-xs app-input rounded-full px-3 py-1.5">
          <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
          <input
            v-model="search"
            class="bg-transparent border-none outline-none text-on-surface font-body-md text-body-md placeholder:text-outline w-56"
            placeholder="Tìm tên hoặc email..."
          />
        </div>
      </div>

      <div v-if="loading" class="p-lg text-on-surface-variant flex items-center gap-2">
        <span class="material-symbols-outlined animate-spin">progress_activity</span>
        Đang tải thành viên...
      </div>

      <div v-else-if="filteredMembers.length === 0" class="p-xl text-center text-on-surface-variant">
        <span class="material-symbols-outlined text-[48px] block mb-sm">group_off</span>
        Không có thành viên phù hợp.
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full min-w-[760px]">
          <thead class="bg-surface-container-low border-b border-outline-variant">
            <tr class="text-left font-label-md text-label-md text-on-surface-variant">
              <th class="px-md py-sm">Thành viên</th>
              <th class="px-md py-sm">Vai trò hệ thống</th>
              <th class="px-md py-sm">Trạng thái</th>
              <th class="px-md py-sm">Dự án tham gia</th>
              <th class="px-md py-sm">Vai trò dự án</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-outline-variant/50">
            <tr
              v-for="member in filteredMembers"
              :key="member.id"
              @click="openMemberProject(member)"
              class="hover:bg-surface-container-low transition-colors cursor-pointer"
            >
              <td class="px-md py-sm">
                <div class="flex items-center gap-sm">
                  <div
                    class="w-9 h-9 rounded-full text-white flex items-center justify-center font-bold"
                    :style="{ backgroundColor: member.color }"
                  >
                    {{ initials(member.fullName || member.email) }}
                  </div>
                  <div class="min-w-0">
                    <p class="font-label-lg text-label-lg text-on-surface truncate">{{ member.fullName || 'Chưa có tên' }}</p>
                    <p class="font-label-sm text-label-sm text-on-surface-variant truncate">{{ member.email }}</p>
                  </div>
                </div>
              </td>
              <td class="px-md py-sm">
                <span class="font-label-sm text-label-sm px-2 py-0.5 rounded-full bg-primary/10 text-primary">
                  {{ member.role || 'User' }}
                </span>
              </td>
              <td class="px-md py-sm">
                <span class="font-label-sm text-label-sm px-2 py-0.5 rounded-full"
                  :class="member.isActive === false ? 'bg-error-container/30 text-error' : 'bg-secondary-container/30 text-secondary'">
                  {{ member.isActive === false ? 'Inactive' : 'Active' }}
                </span>
              </td>
              <td class="px-md py-sm font-label-md text-label-md text-on-surface">
                {{ member.projectCount }}
              </td>
              <td class="px-md py-sm">
                <div class="flex flex-wrap gap-1">
                  <span
                    v-for="role in member.projectRoles"
                    :key="role"
                    class="font-label-sm text-label-sm px-2 py-0.5 rounded-full bg-surface-container text-on-surface-variant"
                  >
                    {{ role }}
                  </span>
                  <span v-if="member.projectRoles.length === 0" class="text-on-surface-variant font-label-sm text-label-sm">
                    Chưa tham gia dự án
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { projectService, userService } from '../services/api'

const router = useRouter()
const loading = ref(true)
const error = ref('')
const search = ref('')
const users = ref([])
const projects = ref([])
const memberships = ref([])
const colors = ['#3525cd', '#006a61', '#684000', '#ba1a1a', '#0f5e9c', '#6a0dad', '#2e7d32', '#e65100']
const roleNames = {
  0: 'Owner',
  1: 'Project Manager',
  2: 'Developer',
  3: 'Tester',
  4: 'Viewer',
}

const activeUsers = computed(() => users.value.filter(user => user.isActive !== false).length)
const usersInProjects = computed(() => new Set(memberships.value.map(item => item.userId)).size)

const enrichedMembers = computed(() => {
  const byUser = new Map()
  for (const membership of memberships.value) {
    if (!byUser.has(membership.userId)) byUser.set(membership.userId, [])
    byUser.get(membership.userId).push(membership)
  }

  return users.value.map((user, index) => {
    const rows = byUser.get(user.id) || []
    const roles = [...new Set(rows.map(row => roleNames[row.role] || 'Member'))]
    return {
      ...user,
      color: colors[index % colors.length],
      projectCount: new Set(rows.map(row => row.projectId)).size,
      projectRoles: roles,
      firstProjectId: rows[0]?.projectId || '',
    }
  })
})

const filteredMembers = computed(() => {
  const keyword = search.value.trim().toLowerCase()
  if (!keyword) return enrichedMembers.value
  return enrichedMembers.value.filter(member =>
    [member.fullName, member.email, member.role, member.projectRoles.join(' ')]
      .some(value => (value || '').toLowerCase().includes(keyword))
  )
})

function initials(name) {
  return (name || 'U').split(' ').filter(Boolean).map(word => word[0]).slice(0, 2).join('').toUpperCase()
}

function openMemberProject(member) {
  if (member.firstProjectId) router.push(`/projects/${member.firstProjectId}`)
}

async function loadMembers() {
  loading.value = true
  error.value = ''
  try {
    const [userList, projectList] = await Promise.all([
      userService.getAll().catch(() => []),
      projectService.getAll().catch(() => []),
    ])
    users.value = userList || []
    projects.value = projectList || []

    const memberResults = await Promise.allSettled(projects.value.map(project => projectService.getMembers(project.id)))
    memberships.value = memberResults.flatMap((result, index) => {
      if (result.status !== 'fulfilled') return []
      const projectId = projects.value[index].id
      return (result.value || []).map(member => ({ ...member, projectId }))
    })
  } catch {
    error.value = 'Không tải được danh sách thành viên. Kiểm tra NotifyService và ProjectService.'
  } finally {
    loading.value = false
  }
}

onMounted(loadMembers)
</script>
