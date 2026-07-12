import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
  state: () => ({ theme: 'light' }),
  actions: {
    toggleTheme() {
      this.theme = this.theme === 'light' ? 'dark' : 'light'
      localStorage.setItem('theme', this.theme)
      document.documentElement.classList.toggle('dark')
    },
    loadTheme() {
      const theme = localStorage.getItem('theme')
      if (theme) {
        this.theme = theme
        if (theme === 'dark') document.documentElement.classList.add('dark')
      }
    }
  }
})
