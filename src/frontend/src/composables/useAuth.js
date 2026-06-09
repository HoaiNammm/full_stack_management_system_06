import { ref, computed } from 'vue'
import { authService } from '../services/api'

const token = ref(localStorage.getItem('token') || '')
const user  = ref(JSON.parse(localStorage.getItem('user') || 'null'))

export function useAuth() {
  const isLoggedIn = computed(() => !!token.value)

  async function login(email, password) {
    const res = await authService.login({ email, password })
    token.value = res.token
    user.value  = res.user
    localStorage.setItem('token', res.token)
    localStorage.setItem('user', JSON.stringify(res.user))
    return res
  }

  async function refreshUser() {
    if (!token.value) return
    try {
      const fresh = await authService.me()
      user.value = fresh
      localStorage.setItem('user', JSON.stringify(fresh))
    } catch { /* token expired — interceptor will redirect to /login */ }
  }

  function logout() {
    token.value = ''
    user.value  = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  return { token, user, isLoggedIn, login, logout, refreshUser }
}
