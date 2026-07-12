/**
 * Base component library — Phase 0 foundation.
 *
 * Import from this barrel file in any view or component:
 *
 *   import { BaseButton, StatusBadge, EmptyState } from '@/components/base'
 *
 * Components and their purpose:
 *   BaseButton      — all button variants (primary/secondary/danger/ghost/icon)
 *   BaseInput       — text/email/password/date/number/textarea inputs
 *   BaseSelect      — <select> dropdowns with consistent styling
 *   StatusBadge     — task status + project status colors (single source of truth)
 *   PriorityBadge   — priority colors, fixes High=red / Medium=amber / Low=zinc
 *   EmptyState      — icon+title+description+CTA pattern (harden.md compliant)
 *   LoadingSkeleton — page-level skeletons (stat/card/list/text variants)
 */
export { default as BaseButton }      from './BaseButton.vue'
export { default as BaseInput }       from './BaseInput.vue'
export { default as BaseSelect }      from './BaseSelect.vue'
export { default as StatusBadge }     from './StatusBadge.vue'
export { default as PriorityBadge }   from './PriorityBadge.vue'
export { default as EmptyState }      from './EmptyState.vue'
export { default as LoadingSkeleton } from './LoadingSkeleton.vue'
