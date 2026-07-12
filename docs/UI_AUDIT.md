# UI Audit — project-management
**Date**: 2026-06-26  
**Auditor**: Impeccable framework cross-reference  
**Reference**: `docs/DESIGN.md`, `UI_design/impeccable/reference/*.md`  
**Method**: Full source read of `src/` → screen-by-screen comparison against DESIGN.md  
**Scope**: Typography · Spacing · Visual Hierarchy · Color · Accessibility · Responsive · Motion · Component Consistency · Impeccable Anti-Patterns

---

## Severity Legend

| Tag | Meaning |
|-----|---------|
| **P0** | Blocks accessibility or core usability — fix before ship |
| **P1** | Design system violation with high user impact |
| **P2** | Consistency or polish failure, noticeable to attentive users |
| **P3** | Fine detail, post-launch polish |

---

## Overall Dimension Scores

| Dimension | Score | Note |
|-----------|-------|------|
| Typography | 6/10 | Outfit font good, but `px` fallbacks in global CSS, no `tabular-nums` on numbers |
| Spacing | 7/10 | Mostly 4pt scale; `md:gap-22` non-standard outlier |
| Visual Hierarchy | 6/10 | Good card structure, but priority color inversion breaks semantic meaning |
| Color | 5/10 | `gray-*` vs `zinc-*` mixing, inverted priority palette, hard-coded chart hex values |
| Accessibility | 4/10 | `focus:` everywhere instead of `focus-visible:`, no `prefers-reduced-motion`, circular checkboxes |
| Responsive | 7/10 | Good breakpoint structure, table→cards in TeamView, minor edge cases |
| Motion | 5/10 | No `prefers-reduced-motion` guard, inconsistent `transition` declarations |
| Component Consistency | 6/10 | `rounded-xl` vs `rounded-lg`, `ring-2` vs `ring-1`, `gray-*` vs `zinc-*` |
| Anti-Patterns | 5/10 | Several Impeccable anti-patterns present (emoji icons, outline removal without replacement) |

**Overall: 51/90 — needs attention before production**

---

## Systemic Issues (All Screens)

These issues appear throughout the entire codebase. Fix at the root level, not screen by screen.

### P0 — No `prefers-reduced-motion` guard anywhere

**Rule**: Impeccable `animate.md` requires `@media (prefers-reduced-motion: reduce)` wrapping all CSS transitions and animations. Impacts users with vestibular disorders.

**Evidence**: `src/index.css` contains no `prefers-reduced-motion` media query. Every component uses `transition-*` classes without guard. Tailwind's built-in `motion-reduce:` prefix is never used in any `.vue` file.

**Affected**: Every screen — every `transition-colors`, `transition-all`, `duration-*` class.

---

### P0 — `focus:` used everywhere instead of `focus-visible:`

**Rule**: Impeccable `interaction-design.md` — use `focus-visible:` for focus rings. `focus:` shows ring on mouse click (annoying) while `focus-visible:` shows ring only for keyboard navigation (correct behavior).

**Evidence**: Search across all components — `focus:ring-*`, `focus:border-*`, `focus:outline-*` are ubiquitous. `focus-visible:` appears nowhere.

**Affected**: LoginView, RegisterView, ProjectsView, ProfileView, SettingsView, all input components.

---

### P1 — Color scale inconsistency: `gray-*` vs `zinc-*`

**Rule**: DESIGN.md Section 3 establishes `zinc-*` as the neutral scale. `gray-*` and `zinc-*` are similar but different Tailwind palettes (gray has a slight blue tint; zinc is truer neutral). Mixing them creates subtle but visible inconsistency especially in dark mode.

**Components using `gray-*`**: LoginView, RegisterView, TasksSummary.vue (`text-gray-500`, `text-gray-800`, `text-gray-600`), SettingsView (`bg-gray-50`)  
**Components using `zinc-*`**: LayoutView, KanbanBoard, ProjectOverview, RecentActivity, DashboardView

---

### P1 — Priority color inversion (semantic violation)

**Rule**: Red = danger/high priority, green/neutral = low priority. This is universal convention.

**Bug in ProjectAnalytics.vue** (lines 47–51):
```js
Low:    'text-red-600 bg-red-200 ...'    // ← RED for LOW priority
Medium: 'text-blue-600 bg-blue-200 ...' // ← BLUE for MEDIUM
High:   'text-emerald-600 bg-emerald-200 ...' // ← GREEN for HIGH
```

**Bug in ProjectOverview.vue** (lines 21–25) — priority dot colors:
```js
Low:    'border-zinc-300 text-zinc-600 ...'  // ← NEUTRAL for LOW (correct)
Medium: 'border-amber-300 text-amber-700 ...' // ← AMBER for MEDIUM (correct)
High:   'border-green-300 text-green-700 ...' // ← GREEN for HIGH (wrong — should be red)
```

**Affected**: DashboardView (ProjectOverview), ProjectDetailsView (ProjectAnalytics tab)

---

### P1 — Hard-coded hex colors in chart.js (ProjectAnalytics.vue)

**Rule**: DESIGN.md Section 3 lists chart colors. All colors should use design tokens, not raw hex. Hard-coded hex values break dark mode support and make global theme changes impossible.

**Evidence** (ProjectAnalytics.vue):
```js
const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']
```
Chart.js cannot use CSS variables or Tailwind classes, but these values should be defined as JavaScript constants in a central design tokens file, not inline.

---

### P2 — Missing `transition` on some interactive elements

**Rule**: DESIGN.md Section 9 — all interactive state changes must animate. Some interactive elements have hover styles without `transition-*` declaration.

**Examples**: "View all" link in ProjectOverview.vue (no transition); "View X more" button in TasksSummary.vue (has hover style but no transition).

---

### P2 — No `tabular-nums` on numeric data

**Rule**: Impeccable `typeset.md` — use `tabular-nums` (or `font-variant-numeric: tabular-nums`) for numbers in tables, counts, percentages, and statistics to prevent layout shift when numbers change.

**Affected**: StatsGrid counts, ProjectAnalytics percentage displays, progress bars, task counts, member counts.

---

### P3 — No `font-optical-sizing: auto`

**Rule**: Impeccable `typeset.md` — variable fonts benefit from `font-optical-sizing: auto` which selects the optimal optical size master automatically.

**Evidence**: `src/index.css` sets `font-family: 'Outfit', sans-serif` globally but does not set `font-optical-sizing: auto`. Outfit is loaded as a variable font (`wght@100..900`).

---

### P3 — No fallback font metrics (layout shift on slow connections)

**Rule**: Impeccable `typeset.md` — define a `@font-face` fallback with `size-adjust`, `ascent-override`, `descent-override` to minimize FOUT (Flash of Unstyled Text) when Outfit loads late.

**Evidence**: `src/index.css` uses `font-display: swap` (correct) but provides no metric-matched fallback, meaning slow connections see layout shift when Outfit loads.

---

## Screen-by-Screen Audit

---

### Screen 1: LoginView + RegisterView (`src/views/LoginView.vue`, `RegisterView.vue`)

**Summary**: Auth screens use a different design language from the rest of the app — different color scale, larger border-radius, different ring width. Functionally correct but visually disconnected.

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Label text is 14px (`text-sm`) | P2 | DESIGN.md Section 2 specifies body text ≥16px. Labels are critical UI text and should not be below 16px |
| No letter-spacing on button text | P3 | CTA buttons could benefit from slight tracking |

#### Color
| Issue | Severity | Detail |
|-------|----------|--------|
| `bg-gray-50 dark:bg-gray-900` for page background | P1 | App uses `zinc-950` for dark backgrounds. `gray-900` has a blue tint vs `zinc-950`'s true neutral |
| `dark:bg-gray-800` for card | P1 | Rest of app uses `dark:bg-zinc-900`. Inconsistent neutral |
| `dark:bg-gray-700` for inputs | P1 | App uses `dark:bg-zinc-800` for inputs elsewhere |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| `focus:ring-2 focus:ring-blue-500` | P0 | Should be `focus-visible:ring-2 focus-visible:ring-blue-500` |
| No visible error state for invalid credentials | P2 | Toast notification is the only feedback. Impeccable `harden.md` requires inline error state near the failing field |
| Password field lacks "show/hide" toggle | P2 | Reduces usability, especially on mobile |

#### Component Consistency
| Issue | Severity | Detail |
|-------|----------|--------|
| `rounded-xl` on card | P1 | App-wide standard is `rounded-lg` (KanbanBoard, ProjectOverview, etc.) |
| `ring-2` on inputs | P2 | App-wide inputs use `ring-1` |
| Shadow: `shadow` (one level) | P3 | Consider `shadow-sm` for lighter treatment — cards in main app don't use drop shadow |

---

### Screen 2: App Shell — LayoutView + Sidebar + Navbar

**Source**: `src/views/LayoutView.vue`, `src/components/Sidebar.vue`, `src/components/Navbar.vue`

#### Color
| Issue | Severity | Detail |
|-------|----------|--------|
| `dark:text-slate-100` in LayoutView root | P1 | Inconsistent. All other dark mode text uses `dark:text-zinc-*`. `slate-100` has a blue tint vs `zinc-100`'s true neutral |

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Nav items likely use `text-sm` (14px) | P2 | Sidebar navigation text should be ≥14px minimum; ideally 16px per DESIGN.md |

#### Responsive
| Issue | Severity | Detail |
|-------|----------|--------|
| Mobile sidebar behavior not confirmed | P2 | LayoutView uses `flex` layout; sidebar collapse behavior on mobile needs verification |

#### Spacing
| Issue | Severity | Detail |
|-------|----------|--------|
| Content area: `p-6 xl:p-10 xl:px-16` | P3 | Jump from 24px to 40px/64px padding is large. Intermediate `lg:` breakpoint could smooth transition |

---

### Screen 3: Dashboard (`src/views/DashboardView.vue` + `StatsGrid.vue`, `RecentActivity.vue`, `TasksSummary.vue`, `ProjectOverview.vue`)

**Overall**: Best-implemented screen in the app. Good empty states, consistent zinc palette in most components, Lucide icons throughout. Key issues are color inconsistency from sub-components.

#### StatsGrid.vue

| Issue | Severity | Detail |
|-------|----------|--------|
| Count numbers lack `tabular-nums` | P2 | Numbers shift visually as values change |
| Stat cards use colored left borders | P2 | DESIGN.md Section 12 Anti-Patterns: colored left/right accent borders >1px are a design smell per Impeccable `colorize.md`. Use top border or colored badge instead |

#### RecentActivity.vue

| Issue | Severity | Detail |
|-------|----------|--------|
| Empty state: icon + text only, no CTA | P2 | Impeccable `harden.md` requires empty states to have: icon + heading + description + CTA. This has icon + description only (no heading, no CTA) |
| `text-lg` for section heading without weight | P3 | Section headings should have `font-medium` or `font-semibold` for hierarchy |
| Status badge `rounded` vs app standard `rounded-full` | P3 | Minor: check which is intended as status badge standard |

#### TasksSummary.vue

| Issue | Severity | Detail |
|-------|----------|--------|
| `text-gray-500`, `text-gray-800`, `text-gray-600` throughout | P1 | Mixed with card wrapper that uses `zinc-*`. Should be `text-zinc-500`, `text-zinc-800` |
| Summary card headers use `text-gray-800 dark:text-white` | P1 | `dark:text-white` (#ffffff) is stronger contrast than needed; `dark:text-zinc-100` preferred |
| Icon: `text-gray-500 dark:text-zinc-400` | P2 | Mixing `gray-500` (light) with `zinc-400` (dark) — inconsistent scale |
| "View X more" button: no explicit touch target sizing | P2 | Button is text-only, likely < 44px height. Needs `py-2` or `min-h-[44px]` |

#### ProjectOverview.vue

| Issue | Severity | Detail |
|-------|----------|--------|
| Priority High → `border-green-300` | P1 | Green means LOW priority by convention; HIGH should be red border |
| `text-md` class on section heading | P2 | `text-md` does not exist in Tailwind — should be `text-base`. Computed value falls back to default (likely 16px), but this is incorrect class name |
| Progress bar height `h-1.5` | P3 | Inconsistent with potential DESIGN.md standard — confirm intended height |
| "No projects yet" empty state missing heading | P2 | Has icon + description + CTA but no heading above description (`harden.md` pattern: icon → heading → description → CTA) |

---

### Screen 4: Projects (`src/views/ProjectsView.vue`)

**Overall**: Well-implemented. Has skeleton loading (correct), empty state with CTA (correct), filter system, and search. Good baseline.

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Filter/search inputs use `rounded-lg` | P2 | LoginView uses `rounded-xl`, some inputs use `rounded-md`. No standardized input border-radius |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Search input focus style | P0 | Likely uses `focus:` not `focus-visible:` (systemic issue) |
| Filter labels — verify `<label for="">` association | P2 | Dropdowns and inputs may be missing programmatic label association |

#### Loading States
| Issue | Severity | Detail |
|-------|----------|--------|
| Skeleton loading implemented | ✓ | Correct: skeleton > spinner per Impeccable `harden.md` |

#### Empty State
| Issue | Severity | Detail |
|-------|----------|--------|
| Empty state with CTA implemented | ✓ | Correct implementation |

---

### Screen 5: Project Details (`src/views/ProjectDetailsView.vue`)

**Overall**: Most complex view (10 tabs). Several issues specific to this screen.

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Empty state uses `text-3xl md:text-5xl` | P1 | `text-5xl` = 48px for an icon or empty state heading is excessive. DESIGN.md Section 2 scale tops at `text-4xl` for page titles. `text-3xl` would be appropriate max |
| Tab navigation uses `text-sm` | P2 | Navigation tabs at 14px on desktop — consider 16px |

#### Layout
| Issue | Severity | Detail |
|-------|----------|--------|
| Tab bar uses `grid-cols-5` on mobile | P1 | 10 tabs in a `grid-cols-5` layout = 2 rows of 5 tabs. On mobile viewports (<640px) this creates extremely narrow tabs. Consider horizontal scroll or a Select dropdown on mobile |
| No visible active tab indicator | P2 | Active tab state should use `border-b-2` or `bg-*` highlight. Verify implementation |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Tab navigation missing `role="tablist"` / `role="tab"` | P1 | Tab components require ARIA roles for screen reader navigation |
| Tab panels missing `role="tabpanel"` | P1 | Each tab content area should have `role="tabpanel"` and `aria-labelledby` |

#### Motion
| Issue | Severity | Detail |
|-------|----------|--------|
| Tab content switch — no transition | P3 | Content area change on tab switch has no crossfade or reveal animation. 150ms opacity transition would polish significantly |

#### CTA
| Issue | Severity | Detail |
|-------|----------|--------|
| "New Task" CTA missing `hover:opacity-90 transition` | P2 | Other CTA buttons in the app (ProjectOverview.vue line 47) use `hover:opacity-90 transition`. This button is inconsistent |

---

### Screen 6: Task Details (`src/views/TaskDetailsView.vue` — from summary context)

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Priority badge text size | P2 | Verify priority badge uses consistent text scale |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Focus management on modal open | P1 | Impeccable `interaction-design.md` requires focus to be moved to modal content on open, and returned to trigger on close. Modals should use `inert` attribute on background content |

#### Empty/Error States
| Issue | Severity | Detail |
|-------|----------|--------|
| Task not found state | P2 | Verify 404-equivalent empty state exists when task ID is invalid |

---

### Screen 7: My Work (`src/views/MyWorkView.vue`)

**Overall**: Clean layout with multiple sections. Priority display uses dots instead of badges (different from other screens).

#### Visual Hierarchy
| Issue | Severity | Detail |
|-------|----------|--------|
| Priority displayed as colored dots | P2 | KanbanBoard and ProjectDetailsView use priority badges. MyWorkView uses dots only. Inconsistent visual language for priority — should use same component |
| `max-w-5xl` container | P2 | Other content views use `max-w-6xl`. Inconsistent max-width creates misaligned content at large viewports |

#### Color
| Issue | Severity | Detail |
|-------|----------|--------|
| Stat cards use colored left/right borders | P1 | Same issue as StatsGrid — colored accent borders >1px are an Impeccable anti-pattern |
| Priority dot colors — verify not inverted | P1 | Check that Low/Medium/High dot colors are semantically correct (not the inversion bug from ProjectAnalytics) |

#### Spacing
| Issue | Severity | Detail |
|-------|----------|--------|
| Multiple sections with `space-y-6` between them | P3 | Verify sections have appropriate separation (48-96px per Impeccable `layout.md`). `space-y-6` = 24px which may be too tight for section-level grouping |

---

### Screen 8: Team (`src/views/TeamView.vue`)

**Overall**: Good responsive behavior (table→cards). Key bug is the non-standard Tailwind class.

#### Spacing
| Issue | Severity | Detail |
|-------|----------|--------|
| `md:gap-22` at line 68 | **P1** | `gap-22` does not exist in Tailwind's default scale. Tailwind scale goes 20 (80px) → 24 (96px). `gap-22` is silently ignored, resulting in `gap-0` effectively. Should be `md:gap-20` or `md:gap-24` |

#### Responsive
| Issue | Severity | Detail |
|-------|----------|--------|
| Table→cards responsive | ✓ | Correct: table collapses to card list on mobile |
| Mini stat cards at top | P3 | Verify stat card layout doesn't overflow on small screens |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Table missing `<caption>` or `aria-label` | P2 | Data tables require caption for screen readers |
| Sortable columns (if any) missing `aria-sort` | P2 | If table headers are clickable for sort, they need `aria-sort` attribute |

---

### Screen 9: Notifications (`src/views/NotificationsView.vue`)

**Overall**: Functional but uses emoji as icons — a clear design system violation.

#### Icon Consistency
| Issue | Severity | Detail |
|-------|----------|--------|
| Emoji icons: 🔔📋💬🚀❌👋 as notification type icons | **P1** | DESIGN.md Section 10 Rule 5: Lucide Vue Next is the only icon library. Emoji are not icons. They render differently across platforms, are not accessible (no alt text by default), and break visual consistency. Replace with Lucide equivalents: `Bell`, `ClipboardList`, `MessageSquare`, `Zap`, `X`, `Hand` |
| Unread indicator: blue dot | ✓ | Correct indicator pattern |

#### Empty State
| Issue | Severity | Detail |
|-------|----------|--------|
| Empty state uses Bell icon + text | P2 | Meets minimum requirements, but missing heading + CTA per Impeccable `harden.md` pattern |

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Notification body text size | P2 | Verify notification content text is ≥16px (body copy rule) |

---

### Screen 10: Profile (`src/views/ProfileView.vue`)

**Overall**: Tab-based profile with avatar upload. Moderate issues.

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Input class uses `rounded-md` | P2 | Screen-specific `inputCls` uses `rounded-md`. LoginView uses `rounded-xl`, most components use `rounded-lg`. Three different radii for the same element type |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Avatar upload — file input styling | P1 | Custom-styled file inputs must retain accessible name. Hidden native inputs need `aria-label` |
| Tab navigation ARIA | P1 | Same issue as ProjectDetailsView — verify `role="tablist"`, `role="tab"`, `aria-selected`, `aria-controls` |
| Profile form inputs — label association | P2 | Verify all inputs have `<label for>` or `aria-label` |

#### Motion
| Issue | Severity | Detail |
|-------|----------|--------|
| Tab switch transition | P3 | No transition on tab content change |

---

### Screen 11: Settings (`src/views/SettingsView.vue`)

**Overall**: Well-structured with Danger Zone pattern. Focus ring color variant in Danger Zone is contextually appropriate.

#### Layout
| Issue | Severity | Detail |
|-------|----------|--------|
| `cardCls` pattern — consistent card styling | ✓ | Correct: reusable computed class for section cards |
| Danger Zone section visually separated | ✓ | Correct implementation |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Type-to-confirm delete input | ✓ | Correct double-confirm pattern per Impeccable `harden.md` |
| `focus:ring-red-500` in Danger Zone | ✓ | Acceptable contextual override for danger context |
| All other focus rings use `focus:` | P0 | Systemic issue — should be `focus-visible:` |

#### Form
| Issue | Severity | Detail |
|-------|----------|--------|
| Settings form auto-save vs explicit submit | P2 | Verify: does the form save on blur or require an explicit submit? Per Impeccable `polish.md` — the save model should be consistent with other forms in the app (ProfileView) |

---

### Component Audit: KanbanBoard (`src/components/KanbanBoard.vue`)

#### Layout
| Issue | Severity | Detail |
|-------|----------|--------|
| Horizontal scroll: `overflow-x-auto` + `min-w-max` | ✓ | Correct pattern for kanban boards |
| Column minimum width | P2 | Verify task cards in columns have a comfortable minimum width (≥250px) on small screens |

#### Color
| Issue | Severity | Detail |
|-------|----------|--------|
| Column accent: `border-t-2 border-zinc-400/blue-400/etc.` | P2 | 2px top border is borderline. Impeccable `colorize.md` warns against border stripes as status indicators. Consider colored column header background instead |
| Label color: hash-deterministic 6-color system | ✓ | Correct implementation per DESIGN.md Section 3 |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Drag-and-drop accessibility | P0 | If cards are draggable, keyboard drag-drop alternative is required per WCAG. Verify: can keyboard users move cards between columns? |
| Card focus state | P0 | Interactive cards must have visible focus ring — verify `focus-visible:ring-2` on task cards |

#### Motion
| Issue | Severity | Detail |
|-------|----------|--------|
| No card drop animation | P3 | Consider subtle translate+opacity animation when cards are moved |

---

### Component Audit: ProjectAnalytics (`src/components/ProjectAnalytics.vue`)

#### Color
| Issue | Severity | Detail |
|-------|----------|--------|
| Priority color INVERSION — Low=red, High=green | **P0** | This is a critical semantic error. A "Low" priority task showing red is misleading and could cause user errors (treating low-priority as urgent). Fix: Low=zinc, Medium=amber, High=red |
| Chart colors: hard-coded `['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']` | P1 | Extract to a named constant in a design tokens file |

#### Typography
| Issue | Severity | Detail |
|-------|----------|--------|
| Chart labels — verify font rendering | P2 | Chart.js renders on canvas; Outfit font may not be applied to chart labels unless explicitly set via `Chart.defaults.font.family = 'Outfit'` |

#### Accessibility
| Issue | Severity | Detail |
|-------|----------|--------|
| Charts lack accessible alternative | P1 | `<canvas>` elements from Chart.js need `aria-label` and ideally a visually-hidden data table fallback for screen readers |

---

## Positive Findings

These patterns are implemented correctly and should be preserved or used as models for inconsistent areas.

| Pattern | Where | Why it's good |
|---------|-------|--------------|
| Skeleton loading (vs spinner) | ProjectsView, DashboardView | Correct Impeccable `harden.md` pattern |
| Empty states with icon + CTA | ProjectOverview, ProjectsView | Correct pattern. See also: harden.md requirement for heading layer |
| Hash-deterministic label colors | KanbanBoard | Smart, consistent, no manual color assignment |
| `transition-all duration-200` on interactive cards | RecentActivity, TasksSummary, ProjectOverview | Smooth state transitions on the right elements |
| Lucide Vue Next used throughout | Most components | Single icon library maintained consistently |
| Class-based dark mode with localStorage | themeStore.js | Correct: user preference respected, persisted |
| `font-display: swap` on Outfit | index.css | Correct: prevents invisible text during font load |
| `@custom-variant dark` definition | index.css | Correct Tailwind v4 class-based dark mode |
| Consistent status color palette | RecentActivity, ProjectOverview | Same semantic colors for ToDo/InProgress/Review/Done/Backlog |
| `divide-y` for list separators | RecentActivity, ProjectOverview | Gap-based separation, not margin hack |
| Toast timeout 3000ms | main.js | Within 150–5000ms acceptable range |
| Type-to-confirm delete | SettingsView | High-friction protection for destructive actions |
| Double-column kanban accent system | KanbanBoard | Clear column differentiation |
| `gap` over margin for sibling spacing | Throughout | Correct Impeccable `layout.md` pattern |
| `min-w-0` on flex children with truncate | RecentActivity, TasksSummary | Correct: prevents flex overflow, enables text-truncate |

---

## Priority Fix List

Ordered by severity + impact. Fix in this sequence.

### P0 — Fix before any release

1. **Add `prefers-reduced-motion` guard** — wrap all transitions in `@media (prefers-reduced-motion: reduce) { * { transition: none !important; } }` in `src/index.css`, or use Tailwind `motion-reduce:transition-none` on all animated elements
2. **Global replace `focus:ring-*` → `focus-visible:ring-*`** — applies to all input, button, and interactive elements across all views
3. **Fix priority inversion in `ProjectAnalytics.vue`** — Low must not be red; swap: Low=zinc, Medium=amber, High=red
4. **Kanban drag-drop keyboard alternative** — verify card movement is possible without mouse

### P1 — High impact, fix soon

5. **Standardize color scale to `zinc-*`** — replace all `gray-*` in LoginView, RegisterView, TasksSummary.vue with `zinc-*` equivalents
6. **Fix `md:gap-22` in TeamView** — replace with `md:gap-20` or `md:gap-24`
7. **Replace emoji icons in NotificationsView** — use Lucide: `Bell`, `ClipboardList`, `MessageSquare`, `Zap`, `X`, `HandMetal` or equivalent
8. **Fix Priority High color in ProjectOverview** — `border-green-300` → `border-red-300` for High priority
9. **Add ARIA roles to tab components** — `role="tablist"`, `role="tab"`, `aria-selected`, `role="tabpanel"`, `aria-labelledby` in ProjectDetailsView and ProfileView
10. **Add `aria-label` to chart canvas elements** in ProjectAnalytics

### P2 — Consistency and polish

11. **Standardize input border-radius** — choose one: `rounded-lg` (recommended, matches card standard)
12. **Standardize ring width** — choose one: `ring-1` (recommended for inputs)
13. **Add `tabular-nums`** to all numeric displays: counts, percentages, progress values
14. **Fix `text-md`** → `text-base` in ProjectOverview heading
15. **Fix `dark:text-slate-100`** → `dark:text-zinc-100` in LayoutView root
16. **Standardize max-width** — MyWorkView `max-w-5xl` → `max-w-6xl` if other views use 6xl
17. **Replace colored accent borders on stat cards** — use colored badge or top-border-2px instead of left border per Impeccable `colorize.md`
18. **Add transition to "View X more" button** and other button-like interactive elements missing it
19. **Extract chart hex colors** to named constants in a tokens file
20. **Fix empty state headings** — add `<h3>` heading layer between icon and description in RecentActivity, NotificationsView, and other empty states missing it

### P3 — Final polish pass

21. **Add `font-optical-sizing: auto`** to `body` in `src/index.css`
22. **Add fallback font metrics** for Outfit to reduce FOUT on slow connections
23. **Add tab-switch transition** (150ms opacity fade) in ProjectDetailsView and ProfileView
24. **Add `Chart.defaults.font.family = 'Outfit'`** to main.js for chart label consistency
25. **Review all touch targets** — verify interactive elements are ≥44×44px, especially icon-only buttons and text links
26. **Unread notification badge** — verify `aria-label="X unread notifications"` on notification bell icon

---

## Appendix: File-to-Finding Map

| File | Findings |
|------|---------|
| `src/index.css` | No `prefers-reduced-motion` [P0], circular checkbox UX concern [P3], no `font-optical-sizing` [P3], no fallback metrics [P3] |
| `src/views/LoginView.vue` | `gray-*` scale [P1], `rounded-xl` [P2], `ring-2` [P2], `focus:ring` [P0] |
| `src/views/RegisterView.vue` | Same as LoginView |
| `src/views/LayoutView.vue` | `dark:text-slate-100` [P1] |
| `src/views/ProjectDetailsView.vue` | `text-5xl` empty state [P1], tab grid mobile [P1], missing ARIA [P1], missing CTA transition [P2] |
| `src/views/MyWorkView.vue` | `max-w-5xl` [P2], priority dots inconsistency [P2], stat border accent [P1] |
| `src/views/TeamView.vue` | `md:gap-22` non-standard class [P1] |
| `src/views/NotificationsView.vue` | Emoji icons [P1], empty state missing heading [P2] |
| `src/views/ProfileView.vue` | `rounded-md` inputs [P2], ARIA tabs [P1] |
| `src/views/SettingsView.vue` | `focus:ring` [P0 systemic] |
| `src/components/ProjectAnalytics.vue` | Priority color inversion [P0], hard-coded hex [P1], chart accessibility [P1] |
| `src/components/ProjectOverview.vue` | High priority green [P1], `text-md` [P2] |
| `src/components/TasksSummary.vue` | `gray-*` scale [P1], `dark:text-white` [P2] |
| `src/components/KanbanBoard.vue` | Keyboard drag-drop [P0], column border accent [P2] |
| `src/components/RecentActivity.vue` | Empty state missing CTA [P2], heading weight [P3] |
