import { defineStore } from 'pinia'
import { authApi, saveTokens, clearTokens } from '../api/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    loading: false,
    error: null,
  }),
  actions: {
    clearError() {
      this.error = null
    },
    async login(credentials) {
      this.loading = true
      this.error = null
      try {
        const data = await authApi.login(credentials)
        saveTokens(data)
        this.user = data.user
        return true
      } catch (err) {
        this.error = err.response?.data?.error || 'Login failed'
        return false
      } finally {
        this.loading = false
      }
    },
    async register(payload) {
      this.loading = true
      this.error = null
      try {
        const data = await authApi.register(payload)
        saveTokens(data)
        this.user = data.user
        return true
      } catch (err) {
        this.error = err.response?.data?.error || 'Register failed'
        return false
      } finally {
        this.loading = false
      }
    },
    async fetchMe() {
      try {
        this.user = await authApi.me()
      } catch {
        this.user = null
      }
    },
    async logout() {
      const refresh = localStorage.getItem('refresh_token')
      if (refresh) await authApi.logout(refresh).catch(() => {})
      clearTokens()
      this.user = null
    },
    async updateProfile(data) {
      this.loading = true
      this.error = null
      try {
        const updated = await authApi.updateProfile(this.user.id, data)
        this.user = { ...this.user, ...updated }
        return true
      } catch (err) {
        this.error = err.response?.data?.error || err.response?.data?.message || 'Update failed'
        return false
      } finally {
        this.loading = false
      }
    },
    async changePassword(data) {
      this.loading = true
      this.error = null
      try {
        await authApi.changePassword(this.user.id, data)
        return true
      } catch (err) {
        this.error = err.response?.data?.error || err.response?.data?.message || 'Password change failed'
        return false
      } finally {
        this.loading = false
      }
    },
    async forgotPassword(email) {
      this.loading = true
      this.error = null
      try {
        await authApi.forgotPassword(email)
        return true
      } catch (err) {
        this.error = err.response?.data?.error || 'Something went wrong'
        return false
      } finally {
        this.loading = false
      }
    },
    async resetPassword(data) {
      this.loading = true
      this.error = null
      try {
        await authApi.resetPassword(data)
        return true
      } catch (err) {
        this.error = err.response?.data?.error || 'Reset failed'
        return false
      } finally {
        this.loading = false
      }
    },
  }
})
