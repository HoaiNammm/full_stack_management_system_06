const projectCovers = [
  'https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=1200&q=80',
  'https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=80',
  'https://images.unsplash.com/photo-1551434678-e076c223a692?auto=format&fit=crop&w=1200&q=80',
  'https://images.unsplash.com/photo-1522202176988-66273c2fd55f?auto=format&fit=crop&w=1200&q=80',
  'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80',
  'https://images.unsplash.com/photo-1556761175-b413da4baf72?auto=format&fit=crop&w=1200&q=80',
]

const taskThumbs = [
  'https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=700&q=80',
  'https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=700&q=80',
  'https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=700&q=80',
  'https://images.unsplash.com/photo-1553877522-43269d4ea984?auto=format&fit=crop&w=700&q=80',
  'https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=700&q=80',
  'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=700&q=80',
]

export const authVisual =
  'https://images.unsplash.com/photo-1557804506-669a67965ba0?auto=format&fit=crop&w=1400&q=80'

export const dashboardVisual =
  'https://images.unsplash.com/photo-1556761175-4b46a572b786?auto=format&fit=crop&w=1400&q=80'

export function getProjectCover(project = {}, index = 0) {
  const key = `${project.template || project.name || ''}`.toLowerCase()
  if (key.includes('mobile')) return projectCovers[4]
  if (key.includes('marketing')) return projectCovers[5]
  if (key.includes('research')) return projectCovers[1]
  if (key.includes('website')) return projectCovers[2]
  return projectCovers[index % projectCovers.length]
}

export function getTaskThumbnail(task = {}, index = 0) {
  const key = `${task.title || ''} ${task.description || ''}`.toLowerCase()
  if (key.includes('design') || key.includes('giao diện')) return taskThumbs[2]
  if (key.includes('api') || key.includes('backend')) return taskThumbs[1]
  if (key.includes('test') || key.includes('kiểm')) return taskThumbs[3]
  if (key.includes('research') || key.includes('phân tích')) return taskThumbs[5]
  return taskThumbs[index % taskThumbs.length]
}
