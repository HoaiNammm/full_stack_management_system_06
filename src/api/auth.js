import { userClient } from './axios'

export const authApi = {
  register: (data) => userClient.post('/api/auth/register', data).then(r => r.data),
  login:    (data) => userClient.post('/api/auth/login', data).then(r => r.data),
  logout:   (refreshToken) => userClient.post('/api/auth/logout', { refreshToken }).catch(() => {}),
  me:       ()   => userClient.get('/api/auth/me').then(r => r.data),
  refresh:  (refreshToken) => userClient.post('/api/auth/refresh', { refreshToken }).then(r => r.data),
  forgotPassword: (email) => userClient.post('/api/auth/forgot-password', { email }).then(r => r.data),
  resetPassword:  (data)  => userClient.post('/api/auth/reset-password', data).then(r => r.data),

  // Needs userId in URL
  updateProfile:  (userId, data) => userClient.put(`/api/users/${userId}`, data).then(r => r.data),
  changePassword: (userId, data) => userClient.put(`/api/users/${userId}/change-password`, data).then(r => r.data),
}

export function saveTokens({ accessToken, refreshToken }) {
  if (accessToken)  localStorage.setItem('access_token', accessToken)
  if (refreshToken) localStorage.setItem('refresh_token', refreshToken)
}

export function clearTokens() {
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
}

export function isLoggedIn() {
  return !!localStorage.getItem('access_token')
}
