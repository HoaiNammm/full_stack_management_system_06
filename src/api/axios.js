import axios from 'axios'

const USER_API      = import.meta.env.VITE_USER_API      || 'http://localhost:5002'
const WORKSPACE_API = import.meta.env.VITE_WORKSPACE_API || 'http://localhost:5001'
const TASK_API      = import.meta.env.VITE_TASK_API      || 'http://localhost:5003'
const NOTIFY_API    = import.meta.env.VITE_NOTIFY_API    || 'http://localhost:5002'

function createClient(baseURL) {
  const client = axios.create({ baseURL })

  client.interceptors.request.use(config => {
    const token = localStorage.getItem('access_token')
    if (token) config.headers.Authorization = `Bearer ${token}`
    return config
  })

  client.interceptors.response.use(
    res => res,
    async err => {
      const original = err.config
      if (err.response?.status === 401 && !original._retry) {
        original._retry = true
        try {
          const refresh = localStorage.getItem('refresh_token')
          const { data } = await axios.post(`${USER_API}/api/auth/refresh`, { refreshToken: refresh })
          localStorage.setItem('access_token', data.accessToken)
          localStorage.setItem('refresh_token', data.refreshToken)
          original.headers.Authorization = `Bearer ${data.accessToken}`
          return client(original)
        } catch {
          localStorage.clear()
          window.location.href = '/login'
        }
      }
      return Promise.reject(err)
    }
  )

  return client
}

export const userClient      = createClient(USER_API)
export const workspaceClient = createClient(WORKSPACE_API)
export const taskClient      = createClient(TASK_API)
export const notifyClient    = createClient(NOTIFY_API)
