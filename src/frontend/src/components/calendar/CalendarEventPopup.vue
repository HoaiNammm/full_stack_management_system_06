<template>
  <Teleport to="body">
    <div v-if="event" class="fixed inset-0 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
      <div class="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant w-full max-w-sm">
        <!-- Header stripe -->
        <div class="h-1.5 rounded-t-2xl" :style="{ backgroundColor: event.backgroundColor }"></div>

        <!-- Content -->
        <div class="p-md flex flex-col gap-sm">
          <div class="flex items-start justify-between">
            <div class="flex items-center gap-2 flex-1 min-w-0">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
                :style="{ backgroundColor: event.backgroundColor + '20' }">
                <span class="material-symbols-outlined text-[18px]"
                  :style="{ color: event.backgroundColor }"
                  style="font-variation-settings: 'FILL' 1">
                  {{ typeIcon }}
                </span>
              </div>
              <div class="min-w-0">
                <p class="font-label-sm text-label-sm uppercase tracking-wider"
                  :style="{ color: event.backgroundColor }">{{ typeLabel }}</p>
                <h3 class="font-headline-sm text-headline-sm text-on-surface truncate">{{ event.title }}</h3>
              </div>
            </div>
            <button @click="$emit('close')" class="text-on-surface-variant hover:text-on-surface p-1 rounded-full hover:bg-surface-container-high transition-colors flex-shrink-0">
              <span class="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <!-- Details -->
          <div class="flex flex-col gap-2 mt-1">
            <div class="flex items-center gap-2 text-on-surface-variant">
              <span class="material-symbols-outlined text-[16px]">folder_shared</span>
              <span class="font-label-md text-label-md">{{ props.event.extendedProps?.projectName || 'Dự án' }}</span>
            </div>

            <div v-if="event.start" class="flex items-center gap-2 text-on-surface-variant">
              <span class="material-symbols-outlined text-[16px]">event</span>
              <span class="font-label-md text-label-md">{{ formatRange }}</span>
            </div>

            <div v-if="event.extendedProps?.description" class="flex items-start gap-2 text-on-surface-variant">
              <span class="material-symbols-outlined text-[16px] mt-0.5">notes</span>
              <span class="font-body-sm text-body-sm">{{ event.extendedProps.description }}</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-2 pt-2 border-t border-outline-variant">
            <button @click="$emit('close')"
              class="px-3 py-1.5 rounded-lg border border-outline-variant font-label-md text-label-md text-on-surface-variant hover:bg-surface-container-high transition-colors">
              Đóng
            </button>
            <button v-if="event.extendedProps?.projectId"
              @click="navigate"
              class="px-3 py-1.5 rounded-lg bg-primary text-on-primary font-label-md text-label-md hover:opacity-90 transition-opacity flex items-center gap-1">
              <span class="material-symbols-outlined text-[14px]">open_in_new</span>
              Xem dự án
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const props  = defineProps({ event: Object })
const emit   = defineEmits(['close'])
const router = useRouter()

const TYPE_META = {
  task:      { icon: 'task_alt',  label: 'Deadline Task', color: '#ba1a1a' },
  sprint:    { icon: 'sprint',    label: 'Sprint',        color: '#0f5e9c' },
  milestone: { icon: 'flag',      label: 'Milestone',     color: '#e65100' },
}

const typeIcon  = computed(() => TYPE_META[props.event?.extendedProps?.type]?.icon || 'event')
const typeLabel = computed(() => TYPE_META[props.event?.extendedProps?.type]?.label || 'Sự kiện')

const formatRange = computed(() => {
  if (!props.event) return ''
  const fmt  = d => new Date(d).toLocaleDateString('vi-VN', { day: 'numeric', month: 'short', year: 'numeric' })
  const end  = props.event.end
  const start = props.event.start
  if (!end || fmt(start) === fmt(end)) return fmt(start)
  return `${fmt(start)} → ${fmt(end)}`
})

function navigate() {
  const pid = props.event?.extendedProps?.projectId
  if (pid) { router.push(`/projects/${pid}`); emit('close') }
}
</script>
