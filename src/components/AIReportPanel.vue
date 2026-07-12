<script setup>
import { ref } from 'vue'
import { aiReportApi } from '../api/projects'
import { useWorkspaceStore } from '../stores/workspaceStore'
import { Sparkles, Send, Loader2 } from 'lucide-vue-next'

const props = defineProps({ projectId: String })
const workspaceStore = useWorkspaceStore()

const question = ref('')
const report = ref('')
const loading = ref(false)
const error = ref('')

const presetQuestions = [
  'Tiến độ dự án hiện tại như thế nào?',
  'Có task nào bị trễ deadline không?',
  'Sprint hiện tại đang hoàn thành được bao nhiêu %?',
  'Thành viên nào đang có nhiều task nhất?',
]

async function handleAsk(q) {
  const text = q || question.value
  if (!text.trim() || !props.projectId) return
  question.value = text
  loading.value = true
  error.value = ''
  report.value = ''
  try {
    const data = await aiReportApi.generate(workspaceStore.currentWorkspaceId, props.projectId, text)
    report.value = data.report || data
  } catch (e) {
    error.value = e?.response?.data?.error || e?.response?.data?.message || 'Cannot generate report. Check if AI API key is configured.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/30 dark:to-indigo-950/30 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
      <div class="flex items-center gap-2 mb-3">
        <Sparkles class="size-4 text-blue-600 dark:text-blue-400" />
        <h3 class="text-sm font-medium text-blue-900 dark:text-blue-200">AI Progress Report</h3>
      </div>
      <p class="text-xs text-blue-700 dark:text-blue-300 mb-3">
        Ask the AI about this project's progress, risks, and recommendations.
      </p>

      <!-- Preset questions -->
      <div class="flex flex-wrap gap-2 mb-3">
        <button v-for="q in presetQuestions" :key="q" @click="handleAsk(q)"
          :disabled="loading"
          class="text-xs px-2.5 py-1 border border-blue-300 dark:border-blue-700 text-blue-700 dark:text-blue-300 rounded-full hover:bg-blue-100 dark:hover:bg-blue-900/50 transition disabled:opacity-50">
          {{ q }}
        </button>
      </div>

      <!-- Custom question -->
      <div class="flex gap-2">
        <input v-model="question" @keyup.enter="handleAsk()"
          placeholder="Hoặc đặt câu hỏi của bạn..."
          class="flex-1 px-3 py-2 text-sm rounded-lg border border-blue-300 dark:border-blue-700 bg-white dark:bg-zinc-900 text-zinc-900 dark:text-zinc-200 placeholder-zinc-400 dark:placeholder-zinc-500 focus:outline-none focus:border-blue-500"
        />
        <button @click="handleAsk()" :disabled="loading || !question.trim()"
          class="p-2 bg-blue-500 text-white rounded-lg disabled:opacity-50 hover:bg-blue-600 transition">
          <Loader2 v-if="loading" class="size-4 animate-spin" />
          <Send v-else class="size-4" />
        </button>
      </div>
    </div>

    <!-- Report output -->
    <div v-if="error" class="p-3 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800 text-sm text-red-600 dark:text-red-400">
      {{ error }}
    </div>

    <div v-if="loading && !report" class="p-6 text-center text-sm text-zinc-500 dark:text-zinc-400">
      <Loader2 class="size-6 animate-spin mx-auto mb-2 text-blue-500" />
      Generating report...
    </div>

    <div v-if="report" class="border border-zinc-200 dark:border-zinc-800 rounded-lg p-4 bg-white dark:bg-zinc-900">
      <div class="flex items-center gap-2 mb-3 pb-2 border-b border-zinc-100 dark:border-zinc-800">
        <Sparkles class="size-3.5 text-blue-500" />
        <span class="text-xs font-medium text-zinc-500 dark:text-zinc-400">AI Response — {{ question }}</span>
      </div>
      <div class="text-sm text-zinc-800 dark:text-zinc-200 whitespace-pre-wrap leading-relaxed">{{ report }}</div>
    </div>
  </div>
</template>
