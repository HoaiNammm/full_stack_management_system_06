// Dates from .NET API have no timezone suffix when Kind=Unspecified.
// Appending 'Z' forces browser to treat them as UTC (which they are).
export function parseUtc(v) {
  if (!v) return null
  const s = String(v)
  return new Date(s.endsWith('Z') || s.includes('+') ? s : s + 'Z')
}
