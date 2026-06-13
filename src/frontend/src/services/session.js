export function readToken() {
  const token = localStorage.getItem('token')
  if (!token || token === 'undefined' || token === 'null') return ''
  return token
}

export function clearSession() {
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
}

export function parseJwtPayload(token) {
  try {
    const [, payload] = token.split('.')
    if (!payload) return null
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(normalized)
        .split('')
        .map(ch => `%${(`00${ch.charCodeAt(0).toString(16)}`).slice(-2)}`)
        .join('')
    )
    return JSON.parse(json)
  } catch {
    return null
  }
}

export function isTokenExpired(token = readToken()) {
  if (!token || !token.startsWith('eyJ')) return true
  const payload = parseJwtPayload(token)
  if (!payload?.exp) return false
  return payload.exp * 1000 <= Date.now()
}

export function hasValidSession() {
  const token = readToken()
  if (!token || isTokenExpired(token)) {
    clearSession()
    return false
  }
  return true
}
