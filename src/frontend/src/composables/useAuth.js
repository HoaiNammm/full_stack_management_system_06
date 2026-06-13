import { ref, computed } from 'vue'
import { authService, NOTIFY_ORIGIN } from '../services/api'
import { clearSession, readToken } from '../services/session'

function resolveAvatarUrl(url) {
  if (!url) return url
  if (/^https?:\/\//i.test(url) || url.startsWith('data:') || url.startsWith('blob:')) return url
  return `${NOTIFY_ORIGIN}${url.startsWith('/') ? url : `/${url}`}`
}

function normalizeUser(raw) {
  if (!raw) return raw
  const avatar = resolveAvatarUrl(raw.avatar || raw.avatarUrl)
  return {
    ...raw,
    avatar,
    avatarUrl: avatar || raw.avatarUrl,
  }
}

function readUser() {
  try {
    return JSON.parse(localStorage.getItem('user') || 'null')
  } catch {
    clearSession()
    return null
  }
}

const token = ref(readToken())
const user  = ref(normalizeUser(readUser()))

export function useAuth() {
  const isLoggedIn = computed(() => !!token.value)

  async function login(email, password) {
    const res = await authService.login({ email, password })
    token.value = res.token
    user.value  = normalizeUser(res.user)
    localStorage.setItem('token', res.token)
    localStorage.setItem('user', JSON.stringify(user.value))
    return res
  }

  async function refreshUser() {
    if (!token.value) return
    try {
      const fresh = await authService.me()
      user.value = normalizeUser(fresh)
      localStorage.setItem('user', JSON.stringify(user.value))
    } catch { /* token expired — interceptor will redirect to /login */ }
  }

  function logout() {
    token.value = ''
    user.value  = null
    clearSession()
  }

  return { token, user, isLoggedIn, login, logout, refreshUser }
}
