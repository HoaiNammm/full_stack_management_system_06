<template>
  <div class="flex flex-col gap-md">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h3 class="font-headline-sm text-headline-sm text-on-surface">Sprint</h3>
        <p class="font-body-md text-body-md text-on-surface-variant mt-0.5">{{ sprints.length }} sprint</p>
      </div>
      <button @click="showModal = true"
        class="flex items-center gap-xs bg-primary text-on-primary px-md py-sm rounded-lg font-label-lg text-label-lg hover:opacity-90 transition-opacity shadow-sm">
        <span class="material-symbols-outlined text-[18px]">add</span>
        Tạo sprint mới
      </button>
    </div>

    <!-- Sprint List -->
    <div v-if="sprints.length === 0"
      class="flex flex-col items-center justify-center gap-3 p-xl bg-surface-container-low rounded-xl border-2 border-dashed border-outline-variant text-center">
      <span class="material-symbols-outlined text-[48px] text-on-surface-variant/40">sprint</span>
      <p class="font-headline-sm text-headline-sm text-on-surface-variant">Chưa có sprint nào</p>
      <p class="font-body-md text-body-md text-outline">Tạo sprint đầu tiên để bắt đầu quản lý tiến độ dự án</p>
    </div>

    <div class="flex flex-col gap-md">
      <div v-for="sprint in sprints" :key="sprint.id"
        class="bg-surface-container-lowest rounded-xl border overflow-hidden transition-shadow hover:shadow-md"
        :class="sprint.status === 1 ? 'border-primary/30' : 'border-outline-variant'">
        <!-- Active bar -->
        <div v-if="sprint.status === 1" class="h-1 w-full bg-primary"></div>

        <div class="p-md">
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-3 flex-1 min-w-0">
              <div class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0"
                :class="sprint.status === 2 ? 'bg-secondary/10 text-secondary' : sprint.status === 1 ? 'bg-primary/10 text-primary' : 'bg-surface-container-high text-on-surface-variant'">
                <span class="material-symbols-outlined text-[22px]"
                  :style="sprint.status === 1 ? `font-variation-settings: 'FILL' 1` : `font-variation-settings: 'FILL' 0`">sprint</span>
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <h4 class="font-headline-sm text-headline-sm text-on-surface">{{ sprint.name }}</h4>
                  <span class="text-[11px] font-bold px-2 py-0.5 rounded-full"
                    :class="statusStyle(sprint.status)">
                    {{ statusLabel[sprint.status] }}
                  </span>
                </div>
                <div class="flex items-center gap-1 mt-1">
                  <span class="material-symbols-outlined text-[13px] text-on-surface-variant">calendar_today</span>
                  <span class="font-label-sm text-label-sm text-on-surface-variant">
                    {{ fmt(sprint.startDate) }} → {{ fmt(sprint.endDate) }}
                  </span>
                  <span class="font-label-sm text-label-sm text-outline ml-1">· 14 ngày</span>
                </div>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex-shrink-0">
              <button v-if="sprint.status === 0" @click="startSprint(sprint)"
                class="flex items-center gap-1 px-3 py-1.5 border border-primary text-primary rounded-lg font-label-md text-label-md hover:bg-primary hover:text-on-primary transition-colors">
                <span class="material-symbols-outlined text-[16px]">play_arrow</span>
                Bắt đầu
              </button>
              <button v-else-if="sprint.status === 1" @click="completeSprint(sprint)"
                class="flex items-center gap-1 px-3 py-1.5 border border-secondary text-secondary rounded-lg font-label-md text-label-md hover:bg-secondary hover:text-on-secondary transition-colors">
                <span class="material-symbols-outlined text-[16px]">check_circle</span>
                Hoàn thành
              </button>
              <div v-else class="flex items-center gap-1 text-secondary">
                <span class="material-symbols-outlined text-[16px]" style="font-variation-settings: 'FILL' 1">check_circle</span>
                <span class="font-label-md text-label-md">Đã hoàn thành</span>
              </div>
            </div>
          </div>

          <!-- Goal -->
          <div class="mt-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/50">
            <p class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider mb-1">Mục tiêu Sprint</p>
            <p class="font-body-md text-body-md text-on-surface">{{ sprint.goal }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>

  <CreateSprintModal v-if="showModal" @close="showModal = false" @created="handleCreated" />
</template>

<script setup>
import { ref } from 'vue'
import CreateSprintModal from './CreateSprintModal.vue'

const props = defineProps(['project'])
const emit = defineEmits(['event'])

const sprints = ref(props.project.sprints.map(s => ({ ...s })))
const showModal = ref(false)

const statusLabel = ['Planned', 'Active', 'Completed']
const statusStyle = (s) => [
  'bg-surface-container text-on-surface-variant',
  'bg-primary/10 text-primary',
  'bg-secondary-container/30 text-secondary',
][s]

function fmt(d) {
  return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function handleCreated(sprint) {
  sprints.value.push(sprint)
  showModal.value = false
}

function startSprint(sprint) {
  // Check if there's already an active sprint
  if (sprints.value.some(s => s.status === 1)) {
    alert('Đang có sprint Active. Hãy hoàn thành sprint hiện tại trước khi bắt đầu sprint mới.')
    return
  }
  sprint.status = 1
  emit('event', {
    type: 'sprint.started',
    icon: 'sprint',
    iconBg: 'bg-primary/10 text-primary',
    summary: `${sprint.name} đã bắt đầu — "${sprint.goal.slice(0, 60)}${sprint.goal.length > 60 ? '...' : ''}"`,
    time: new Date().toLocaleTimeString('vi-VN'),
  })
}

function completeSprint(sprint) {
  sprint.status = 2
}
</script>
