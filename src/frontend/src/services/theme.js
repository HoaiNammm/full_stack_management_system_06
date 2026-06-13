import { computed, ref } from 'vue'

const STORAGE_KEY = 'pm-theme'
const theme = ref('light')

function preferredTheme() {
  if (typeof window === 'undefined') return 'light'
  const saved = localStorage.getItem(STORAGE_KEY)
  if (saved === 'light' || saved === 'dark') return saved
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function applyTheme(nextTheme) {
  theme.value = nextTheme === 'dark' ? 'dark' : 'light'
  document.documentElement.classList.toggle('dark', theme.value === 'dark')
  document.documentElement.dataset.theme = theme.value
  localStorage.setItem(STORAGE_KEY, theme.value)
}

export function initTheme() {
  applyTheme(preferredTheme())
}

export function useTheme() {
  const isDark = computed(() => theme.value === 'dark')
  const toggleTheme = () => applyTheme(isDark.value ? 'light' : 'dark')

  return {
    theme,
    isDark,
    toggleTheme,
    setTheme: applyTheme,
  }
}
