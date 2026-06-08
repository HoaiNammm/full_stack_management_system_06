<template>
  <div class="flex-1 flex flex-col h-full overflow-hidden">
    <!-- Toolbar -->
    <div class="w-full px-md md:px-lg py-sm bg-surface-container-lowest border-b border-outline-variant flex flex-wrap items-center justify-between gap-4 z-20">
      <div class="flex items-center gap-2 overflow-x-auto kanban-scroll pb-1 sm:pb-0">
        <div class="flex items-center gap-2 pr-4 border-r border-outline-variant">
          <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuCt7BlIOXaH4tiqAB86AoewqYxZ6GTg25-yG0ewAydR_9CcalYzZb7Ns3RK9sV32qsJ_yY7gNDiNUL8-wsAZFsSq_-flMZegS8KmykrSEGTxRueXbFjDEUDNfZzWMSYdc0UnqNNtZ03Z8aAIiMU-G-2C-vxGJE70XtoYJULtWLcs9OLDvZ1GhmZcv8PkF9saA58I5aCYklwHdDBH_WaZaRqpSy9uLjnM3iSD-NjUVArd8J00Alo0AjeGGzDGJ5jU-6HcPvkooMSOZ_9"
            class="w-6 h-6 rounded-full object-cover ring-2 ring-surface-container-lowest -mr-2 z-10 relative" />
          <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuBjc9iGywnYHlMEzEOCipFcwSc-2ZxqaWdFlqj-d2B3Xto1eVt31A3gKSVWaJ32JwtswDpdr4jpu5fMqcA6j2hKSQAPzR_J5HmpWkjxOnxUaTl3nek8Wirp6PrPxolMwXct0xvXzQEC6d9Eij7l9IbAEcu9gLEVHl89Aiu_1Rbh6U4fTOAgA5f5amRYusccGaPUC-GJ2xuDgAFjCPCSYCh-g4pNXyYx-FIxFW69VACfHAvfwrIjCOUrPIKs-zT3ADV4UuTguEDw7cj2"
            class="w-6 h-6 rounded-full object-cover ring-2 ring-surface-container-lowest -mr-2 z-20 relative" />
          <button class="w-6 h-6 rounded-full bg-surface-container-high text-on-surface-variant flex items-center justify-center ring-2 ring-surface-container-lowest z-30 hover:bg-surface-dim transition-colors">
            <span class="material-symbols-outlined text-[14px]">add</span>
          </button>
        </div>
        <FilterBtn icon="person" label="Thành viên" />
        <FilterBtn icon="flag" label="Độ ưu tiên" />
        <FilterBtn icon="label" label="Nhãn" />
      </div>
      <div class="flex items-center gap-2">
        <button class="p-1.5 text-on-surface-variant hover:bg-surface-container-high rounded transition-colors" title="Danh sách">
          <span class="material-symbols-outlined text-[20px]">format_list_bulleted</span>
        </button>
        <button class="p-1.5 text-primary bg-primary/10 rounded transition-colors" title="Bảng">
          <span class="material-symbols-outlined text-[20px]" style="font-variation-settings: 'FILL' 1">grid_view</span>
        </button>
      </div>
    </div>

    <!-- Kanban Board -->
    <div class="flex-1 overflow-x-auto overflow-y-hidden kanban-scroll p-md md:p-lg bg-surface-container flex gap-md sm:gap-lg items-start">
      <KanbanColumn v-for="col in columns" :key="col.id" :column="col" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import KanbanColumn from '../components/KanbanColumn.vue'
import FilterBtn from '../components/FilterBtn.vue'

const columns = ref([
  {
    id: 'backlog', title: 'Backlog',
    dotColor: 'bg-outline',
    countBg: 'bg-surface-container text-on-surface-variant',
    count: 4,
    tasks: [
      { id: 'TKS-102', title: 'Thiết kế schema database cho User Service', tags: [{ text: 'API', color: 'bg-surface-container text-on-surface-variant' }], priority: 'medium', deadline: 'Không có', deadlineColor: 'text-outline', unassigned: true },
    ]
  },
  {
    id: 'todo', title: 'Cần làm',
    dotColor: 'bg-primary-container',
    countBg: 'bg-primary/10 text-primary',
    count: 2,
    tasks: [
      { id: 'TKS-105', title: 'Implement giao diện kéo thả cho Kanban Board', tags: [{ text: 'Frontend', color: 'bg-secondary-container/30 text-secondary' }], priority: 'high', deadline: 'Hôm nay', deadlineColor: 'text-error', avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBHi2sxb_WnFDRorHgpBa_rq4jas2_rRosmb8Ylb0pVUavpb9oecEySa62j6dxSHVHF1SH3upS9TT5Cy1kXm6FapfCnRn1ZaeBe1Cr1FteNOElZEoY8wLd47QWwEAI5tKCMDMI-pNQ2sjDcZCOSzKkfFRznYFD_vf_dMRFQnAy6rMY0MHEo0EAUzocFlFXKHCVCo3qFLgVrpBI5OQQMHmkLUhBwRVonpU9sWjFT3IkndDZlIAv5_EkgE8x-hy6JvZIz-mYvkPU2sxDj' },
      { id: 'TKS-108', title: 'Review UI/UX với team Product', tags: [{ text: 'Design', color: 'bg-tertiary-container/20 text-tertiary' }], priority: 'medium', deadline: '12 Th10', deadlineColor: 'text-on-surface-variant', avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBFAR3AjsKItMOUyfe-PaCaAwanujQl2Gnmw6mVgzJxhc3jG0HSlUXB5hkrYmD4uNZKDbbQplyVUAptK-avivZdhk-Ju33moW2nHwvvDYNXYRcZEWETC7SRYKAMqnB_Uze7civXh5b5djL9UDdaYVdqCRKy_llvaXI-0S5cib-lWU0WqBIt5lBtvY9Zfpv_MCMtvtU3zsayr-QwvN0G8H_aJyMzHN7j3BmOUIymcje3UeOYJwaA0PWLp8CUUx9yo_dkbjrgDJv0Z2xV' },
    ]
  },
  {
    id: 'inprogress', title: 'Đang thực hiện',
    dotColor: 'bg-secondary',
    countBg: 'bg-secondary-container/20 text-secondary',
    count: 1,
    tasks: [
      { id: 'TKS-099', title: 'Tối ưu hóa query lấy danh sách task', tags: [{ text: 'Backend', color: 'bg-primary/10 text-primary' }], priority: 'high', accentBar: true, inProgress: true, avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAlstB-pKpX4Rph6luFh04qZcAaC5v-R5AkKH_caa46owPPo006W6NWroo-dd93yxvLiYr2MRvlumz375xpmAwQy4UkpHkTyE7p4e-tbHKOx42nf7e_ZSusqS7uXERAvFRVaPKrJU2KWnjieA22nZj7DYcZ0PZvAy2d9DrhgKw7Ut1LwoQVCLEkH7yrPok1v4VBxviNz29cE1pZWUcen39K73T_F5_NRK--YBA8yYVl2lMx5BUvYUdmB2vm6Nq96TRfEtCwEiUVoZrf' },
    ]
  },
  {
    id: 'review', title: 'Đang kiểm tra',
    dotColor: 'bg-tertiary',
    countBg: 'bg-tertiary-container/20 text-tertiary',
    count: 1,
    tasks: [
      { id: 'TKS-095', title: 'Viết Unit Test cho Auth Service', tags: [{ text: 'Testing', color: 'bg-surface-container text-on-surface-variant' }], priority: 'low', progress: 75, comments: 2, avatarText: 'QA' },
    ]
  },
  {
    id: 'done', title: 'Hoàn thành',
    dotColor: 'bg-secondary',
    countBg: 'bg-surface-container text-on-surface-variant',
    count: 12,
    dimmed: true,
    tasks: [
      { id: 'TKS-088', title: 'Setup CI/CD pipeline cho dev environment', tags: [{ text: 'Backend', color: 'bg-primary/10 text-primary' }], done: true, doneDate: 'Hoàn thành 09 Th10' },
    ]
  },
])
</script>
