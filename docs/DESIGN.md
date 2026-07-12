# DESIGN.md — Design Language

> Chuẩn thiết kế thực tế được trích xuất từ codebase hiện tại.
> Mọi quyết định ở đây phản ánh trực tiếp Tailwind CSS v4 và các lớp đang dùng trong `src/`.
> Các quy tắc bổ sung áp dụng nguyên tắc từ hệ thống thiết kế **Impeccable**.

---

## 0. Tech Stack Design Context

Được xác minh từ `package.json` và `src/index.css`:

| Công cụ | Phiên bản | Ghi chú |
|---------|-----------|---------|
| **Tailwind CSS** | v4.1.x | CSS-first config — **không có** `tailwind.config.js` |
| **Vue** | 3.5.x | Composition API, `<script setup>` |
| **Vite** | 7.x | `@tailwindcss/vite` plugin |
| **Lucide Vue Next** | 0.503.x | Icon library duy nhất |
| **Chart.js** + vue-chartjs | 4.4.x | Bar, Pie charts — `ProjectAnalytics.vue` |
| **vue-toastification** | 2.0.0-rc.5 | Toast notifications, timeout 3000ms |

**Tailwind v4 đặc điểm:**
- Config qua CSS (`@import "tailwindcss"` trong `src/index.css`)
- `@custom-variant` thay `darkMode: 'class'` trong config cũ
- Không có `tailwind.config.js` để thêm custom easing, custom colors

---

## 1. Design Language

### Nguyên tắc cốt lõi

**Minimal & Functional** — Giao diện tối giản, không có chi tiết trang trí thừa. Mọi element đều có mục đích rõ ràng.

**Density-aware** — Giao diện quản lý dự án phục vụ người dùng có kiến thức, không cần quá nhiều whitespace. Mật độ thông tin vừa phải.

**Dark-first** — Dark mode là trải nghiệm chính, light mode là alternative. Mọi component phải pass cả hai chế độ.

**Predictable** — Cùng loại dữ liệu luôn hiển thị cùng kiểu (status badge, priority color nhất quán).

### Nguyên tắc bổ sung (Impeccable)

**Semantic-first color** — Accent color chỉ dùng cho primary action, selection, state indicator. Không trang trí. Mỗi màu mang ý nghĩa nhất quán trên toàn app.

**Hierarchy over decoration** — Phân cấp thị giác qua size + weight + color + space. Không cần thêm border/shadow khi spacing và weight đã đủ.

**Every element earns its place** — Trước khi thêm bất kỳ element nào: nếu bỏ đi, user thiếu thông tin không? Nếu không — bỏ.

### Dark mode implementation

```javascript
// themeStore.js — class-based toggle trên <html>
document.documentElement.classList.toggle('dark')
localStorage.setItem('theme', this.theme)

// index.css — custom variant
@custom-variant dark (&:where(.dark, .dark *));
```

Dark mode được kích hoạt bằng class `.dark` trên `<html>`, không phải `prefers-color-scheme`. Người dùng chọn thủ công, lưu vào localStorage.

---

## 2. Typography

### Font Family — Outfit

**Nguồn:** Xác minh từ `src/index.css` dòng 1–6.

```css
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@100..900&display=swap');

* {
  font-family: 'Outfit', sans-serif;
}
```

**Outfit** là geometric sans-serif, human-friendly, phù hợp với productivity tool. Đây là **variable font** (wght@100..900 — tất cả weights từ 100 đến 900 trong một file duy nhất).

### Google Fonts Loading Strategy

**Thực tế hiện tại:**

| Tham số URL | Giá trị | Ý nghĩa |
|-------------|---------|---------|
| `family=Outfit:wght@100..900` | Variable font | Một file, toàn bộ weights |
| `&display=swap` | `font-display: swap` | Hiển thị fallback ngay, swap khi Outfit load xong |

**Điều chưa implement (TODO — tùy chọn, không bắt buộc):**
- Không có `<link rel="preload">` cho Outfit trong `index.html`
- Không có fallback font metrics (`size-adjust`, `ascent-override`) → có thể gây FOUT nhỏ khi swap

**Fallback hiện tại:** `'Outfit', sans-serif` — nếu Google Fonts không load, dùng sans-serif hệ thống.

### Thang cỡ chữ (Type Scale)

Xác minh từ toàn bộ components. Project dùng thang 8 bậc:

| Token Tailwind | px tương đương | Dùng cho |
|----------------|----------------|----------|
| `text-[10px]`  | 10px           | Badge section header (Navbar search), notification badge |
| `text-xs`      | 12px           | Metadata, timestamp, mô tả phụ, sub-item nav |
| `text-sm`      | 14px           | Body text chính, form labels, nav items, card content |
| `text-base`    | 16px           | Section heading phụ, dialog heading |
| `text-lg`      | 18px           | Task title trong TaskDetailsView |
| `text-xl`      | 20px           | Page heading (DashboardView welcome text) |
| `text-2xl`     | 24px           | Stat number trong StatsGrid, page title trên mobile |
| `text-3xl`     | 30px           | Stat value lớn (Time Logged) |

> **Impeccable note:** Body text tối thiểu 16px. Hiện tại body text chính là `text-sm` (14px) — chấp nhận được cho app UI dense nhưng cần đảm bảo không nhỏ hơn.

### Font Weight — Xác minh từ codebase

| Token | Dùng cho |
|-------|----------|
| `font-normal` (400) | Body text, mô tả task, comment |
| `font-medium` (500) | Nav items, button text, section label |
| `font-semibold` (600) | Section heading, card title, column header |
| `font-bold` (700) | Page title, dialog heading, stat value |

### Line Height

- Body text: `leading-relaxed` hoặc `leading-snug` (task title trên Kanban card)
- Heading: `leading-tight` (mặc định)
- Multi-line: `line-clamp-2` (project description trong ProjectCard)

### ALL CAPS Labels

Xác minh trong codebase: Kanban column headers và Search dropdown section headers dùng pattern:

```html
<!-- KanbanBoard.vue — column header -->
<span class="text-xs font-semibold uppercase tracking-wider text-zinc-500">
  {{ col.name }}
</span>

<!-- Navbar.vue — search section header -->
<div class="text-[10px] font-semibold uppercase tracking-wider text-zinc-400">
  Projects
</div>
```

**Chuẩn hóa cho ALL CAPS labels:** `uppercase tracking-wider` (Tailwind `letter-spacing: 0.05em`) — đúng với Impeccable guideline (5–12% letter-spacing cho all-caps nhỏ).

### Số liệu (tabular-nums)

**Chưa implement** trong codebase — stat numbers dùng default proportional numerals. TODO cho tương lai:

```css
.stat-value { font-variant-numeric: tabular-nums; }
```

---

## 3. Color

### Màu nền (Background)

Xác minh từ `LayoutView.vue`, `Navbar.vue`, `Sidebar.vue`, `StatsGrid.vue`:

| Ngữ cảnh | Light | Dark |
|----------|-------|------|
| App root background | `bg-white` | `dark:bg-zinc-950` |
| App root text | `text-gray-900` | `dark:text-slate-100` |
| Sidebar | `bg-white` | `dark:bg-zinc-900` |
| Navbar | `bg-white` | `dark:bg-zinc-900` |
| Card / panel (chuẩn) | `bg-white` | `dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50` |
| Card (Analytics) | `not-dark:bg-white` | `dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50` |
| Input | `bg-white` | `dark:bg-zinc-800` hoặc `dark:bg-zinc-900` |
| Kanban column | `bg-zinc-50` | `dark:bg-zinc-900/60` |
| Task card (Kanban) | `bg-white` | `dark:bg-zinc-800` |
| Modal overlay | `bg-black/20` | `dark:bg-black/60 backdrop-blur` |
| Modal container | `bg-white` | `dark:bg-zinc-950` |

> **Lưu ý:** `LayoutView.vue` dùng `dark:text-slate-100` nhưng các component con dùng `dark:text-zinc-100` hoặc `dark:text-white`. Không nhất quán nhỏ giữa `slate` và `zinc`.

### Màu viền (Border)

| Ngữ cảnh | Light | Dark |
|----------|-------|------|
| Default | `border-gray-200` | `dark:border-zinc-800` |
| Input | `border-zinc-300` | `dark:border-zinc-700` |
| Hover | `hover:border-gray-300` | `dark:hover:border-zinc-700` |
| Focus | `focus:ring-1 focus:ring-blue-500` | same |
| Drop zone (Kanban) | `border-zinc-200` dashed | `dark:border-zinc-700` |
| Drop zone active | `border-blue-400` | `bg-blue-50 dark:bg-blue-950/20` |

### Màu văn bản (Text)

| Ngữ cảnh | Light | Dark |
|----------|-------|------|
| Primary | `text-gray-900` / `text-zinc-900` | `dark:text-white` / `dark:text-zinc-100` |
| Secondary | `text-gray-500` / `text-zinc-500` | `dark:text-zinc-400` |
| Muted / caption | `text-gray-400` / `text-zinc-400` | `dark:text-zinc-500` |
| Link | `text-blue-600` | `dark:text-blue-400` |
| Destructive | `text-red-600` | `dark:text-red-400` |

### Màu Brand / Semantic

| Màu | Class | Dùng cho |
|-----|-------|----------|
| Blue | `blue-500` / `blue-600` | Primary action, link, progress bar, CTA |
| Emerald | `emerald-500` | Success, Done, Completed, Active status |
| Amber | `amber-500` | Warning, In Progress, On Hold |
| Red | `red-500` | Danger, Overdue, High priority, Delete |
| Purple | `purple-500` | Review status, My Tasks stat |
| Zinc | `zinc-400`–`zinc-700` | Neutral, Backlog, secondary info |

### Status Colors

#### Task Status (xác minh từ `TaskDetailsView.vue`, `KanbanBoard.vue`)

| Status | Background (light) | Background (dark) | Text |
|--------|-------------------|-------------------|------|
| Backlog | `bg-zinc-200` | `dark:bg-zinc-700` | `text-zinc-800` / `dark:text-zinc-300` |
| ToDo | `bg-blue-100` | `dark:bg-blue-900` | `text-blue-800` / `dark:text-blue-300` |
| InProgress | `bg-amber-100` | `dark:bg-amber-900` | `text-amber-800` / `dark:text-amber-300` |
| Review | `bg-purple-100` | `dark:bg-purple-900` | `text-purple-800` / `dark:text-purple-300` |
| Done | `bg-emerald-100` | `dark:bg-emerald-900` | `text-emerald-800` / `dark:text-emerald-300` |

> `Blocked` xuất hiện trong `statusBadge` object của `TaskDetailsView.vue` với màu red, nhưng **không phải** KanbanStatus enum của backend. Chỉ có 5 status: Backlog, ToDo, InProgress, Review, Done.

#### Project Status (xác minh từ `ProjectCard.vue`)

| Status | Light | Dark |
|--------|-------|------|
| Planning | `bg-zinc-200 text-zinc-800` | `dark:bg-zinc-600 dark:text-zinc-200` |
| Active | `bg-emerald-200 text-emerald-900` | `dark:bg-emerald-500 dark:text-emerald-900` |
| OnHold | `bg-amber-200 text-amber-900` | `dark:bg-amber-500 dark:text-amber-900` |
| Completed | `bg-blue-200 text-blue-900` | `dark:bg-blue-500 dark:text-blue-900` |
| Cancelled | `bg-purple-200 text-purple-900` | `dark:bg-purple-800 dark:text-purple-200` |

#### Task Priority (xác minh từ `KanbanBoard.vue`, `TaskDetailsView.vue`)

| Priority | Background (light) | Background (dark) |
|----------|-------------------|-------------------|
| High | `bg-red-100 text-red-700` | `dark:bg-red-900/60 dark:text-red-300` |
| Medium | `bg-amber-100 text-amber-700` | `dark:bg-amber-900/60 dark:text-amber-300` |
| Low | `bg-zinc-100 text-zinc-500` | `dark:bg-zinc-700 dark:text-zinc-400` |

#### Label Colors — Hash-deterministic Palette

Xác minh từ `KanbanBoard.vue` và `TaskDetailsView.vue`:

```javascript
const LABEL_COLORS = [
  'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
  'bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300',
  'bg-teal-100 text-teal-700 dark:bg-teal-900 dark:text-teal-300',
  'bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300',
  'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
  'bg-pink-100 text-pink-700 dark:bg-pink-900 dark:text-pink-300',
]
// Màu được chọn theo hash của tên label → deterministic, không random
function labelColor(str) {
  let h = 0; for (const c of str) h = c.charCodeAt(0) + ((h << 5) - h)
  return LABEL_COLORS[Math.abs(h) % LABEL_COLORS.length]
}
```

### Chart.js Colors (xác minh từ `ProjectAnalytics.vue`)

```javascript
// Hard-coded hex — không phải design token
const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']
// Mapping: blue-500, emerald-500, amber-500, red-500, purple-500
// Nhất quán với brand semantic colors ở trên
```

Bar chart tasks by status: đơn sắc `#3b82f6` (blue-500).

> **TODO:** Chart colors nên được tách thành CSS variables hoặc token constants để dễ maintain khi theme thay đổi.

### Màu gradient

```css
/* CTA buttons: New Project, Create Task, Post comment */
bg-gradient-to-br from-blue-500 to-blue-600

/* Stat icon backgrounds */
bg-blue-500/10    /* Total Projects */
bg-emerald-500/10 /* Completed */
bg-purple-500/10  /* My Tasks */
bg-amber-500/10   /* Overdue */

/* User avatar fallback */
bg-gradient-to-br from-blue-400 to-blue-600
```

### Quy tắc 60-30-10 (Impeccable)

| Phần | Tỉ lệ | Vai trò |
|------|--------|---------|
| Neutral (bg, whitespace, surfaces) | 60% | Nền tảng |
| Secondary (text, border, inactive) | 30% | Hỗ trợ |
| Accent (CTA, highlight, focus) | 10% | Thu hút sự chú ý — hiếm = hiệu quả |

### WCAG Contrast Requirements (Impeccable)

| Loại nội dung | Tối thiểu AA | Mục tiêu AAA |
|---------------|-------------|--------------|
| Body text | **4.5:1** | 7:1 |
| Large text (≥18px hoặc 14px bold) | **3:1** | 4.5:1 |
| UI component, icon | **3:1** | 4.5:1 |

**Tổ hợp màu nguy hiểm:**
- Gray text nhạt trên white (fail phổ biến nhất)
- Red trên green (8% nam mù màu)
- Yellow text trên white

**Quy tắc bổ sung (Impeccable):**
- Không gray text lên colored background — dùng shade đậm hơn hoặc rgba
- Không `border-left/right > 1px` làm accent stripe
- Alpha (`rgba` nặng) = design smell: palette chưa hoàn chỉnh
- Nhất quán ý nghĩa màu: green luôn = success, red luôn = danger

---

## 4. Layout

### Breakpoints (Tailwind mặc định)

| Prefix | Min-width | Dùng cho |
|--------|-----------|----------|
| (base) | 0px | Mobile, single column |
| `sm` | 640px | Mobile landscape, ẩn sidebar button |
| `md` | 768px | Stats grid 2 cột |
| `lg` | 1024px | Full layout, sidebar visible, dashboard 3 cột |
| `xl` | 1280px | Padding tăng (`xl:p-10 xl:px-16`) |
| `max-sm` | <640px | Sidebar absolute/overlay |

### App Shell Layout (xác minh từ `LayoutView.vue`)

```
┌──────────────────────────────────────────────────────────┐
│                        NAVBAR (top)                       │
│  Hamburger | Search | (flex-1) | Theme | Bell | Avatar    │
├──────────┬───────────────────────────────────────────────┤
│          │                                               │
│ SIDEBAR  │              MAIN CONTENT                     │
│ min-w-68 │         max-w-6xl mx-auto                     │
│          │         p-6 xl:p-10 xl:px-16                  │
│ Workspace│                                               │
│ Nav menu │                                               │
│ My Tasks │                                               │
│ Projects │                                               │
│ (sub-nav)│                                               │
└──────────┴───────────────────────────────────────────────┘
```

- Sidebar: `min-w-68` (272px), `h-screen`, `flex flex-col`, fixed left
- Mobile (`max-sm`): Sidebar `absolute`, toggle qua hamburger button, close on outside click
- Content area: `flex-1 h-full p-6 xl:p-10 xl:px-16 overflow-y-scroll`

### Content Max Width

```css
max-w-6xl mx-auto
```

Áp dụng nhất quán: DashboardView, TaskDetailsView.

### Dashboard Grid (xác minh từ `DashboardView.vue`)

```
lg:grid-cols-3 gap-8
  ├── lg:col-span-2: ProjectOverview + RecentActivity (space-y-8)
  └── lg:col-span-1: TasksSummary
```

### Stats Grid (xác minh từ `StatsGrid.vue`)

```
grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 my-9
```

### Kanban Board Layout (xác minh từ `KanbanBoard.vue`)

Kanban dùng **horizontal scroll** cho nhiều columns:

```html
<!-- Horizontal scroll wrapper -->
<div class="overflow-x-auto pb-2">
  <div class="flex gap-3 min-w-max">
    <!-- Columns: w-60 mỗi cột -->
    <div v-for="col" class="flex flex-col w-60 ...">
      <!-- Min height cho empty column: min-h-20 -->
    </div>
  </div>
</div>
```

### Nguyên tắc layout (Impeccable)

**Squint test:** Nheo mắt — vẫn nhận ra element quan trọng nhất và nhóm nội dung rõ ràng.

**Flexbox vs Grid:** Flexbox cho 1D (nav bar, button row, card content). Grid cho 2D (page layout, stats grid, dashboard).

**Card discipline:** Không nest card trong card. Cards chỉ khi nội dung distinct và actionable.

---

## 5. Spacing

### Thang spacing (4pt base scale — Tailwind v4)

| px | Tailwind | Dùng khi |
|----|---------|----------|
| 4 | `gap-1`, `p-1` | Tight inline, icon offset |
| 8 | `gap-2`, `p-2` | Sibling items trong group |
| 12 | `gap-3`, `p-3` | Kanban cards, sub-items |
| 16 | `gap-4`, `p-4` | Card internal nhỏ |
| 20 | `gap-5`, `p-5` | Card / panel padding chuẩn |
| 24 | `gap-6`, `p-6` | Page padding, grid gap |
| 32 | `gap-8` | Dashboard section gap |
| 36 | `my-9` | Stats grid vertical margin |

Giá trị ngoài thang này (13px, 17px, 22px) không được dùng.

### Padding nội dung (xác minh từ components)

| Ngữ cảnh | Class |
|----------|-------|
| Page container (mobile) | `p-6` |
| Page container (desktop) | `xl:p-10 xl:px-16` |
| Card / Panel | `p-5` hoặc `p-6` |
| Stat card | `p-6 py-4` |
| Button (primary) | `px-5 py-2` |
| Button (secondary/small) | `px-3 py-1.5` |
| Badge / chip | `px-2 py-0.5` |
| Badge (notification dot) | `px-1 py-0.5` |
| Input field | `px-3 py-1.5` hoặc `px-3 py-2` |
| Nav item | `py-2 px-4` |
| Dropdown item | `px-3 py-2` |

### Gap (xác minh từ components)

| Ngữ cảnh | Class |
|----------|-------|
| Section spacing (dashboard) | `space-y-8` |
| Card grid | `gap-6` hoặc `gap-8` |
| Form fields | `space-y-4` hoặc `space-y-3` |
| Inline items (icon + text) | `gap-2` hoặc `gap-3` |
| Kanban columns | `gap-3` |
| Task cards trong cột | `gap-2` |
| Nav items | `space-y-1` |

**Dùng `gap` thay `margin` cho sibling spacing** — tránh margin collapse.

### Spacing rhythm (Impeccable)

- **Tight grouping** (related elements): 8-12px
- **Generous separation** (distinct sections): 48-96px
- **Không equal padding khắp nơi** — variety tạo rhythm

### Border Radius (xác minh từ codebase)

| Ngữ cảnh | Class |
|----------|-------|
| Input, button (chuẩn) | `rounded` (4px) hoặc `rounded-md` (6px) |
| Card, panel, modal | `rounded-lg` (8px) |
| Dropdown, popover | `rounded-lg` hoặc `rounded-md` |
| Nav item (Sidebar) | `rounded-lg` |
| Avatar | `rounded-full` |
| Stat icon background | `rounded-xl` (12px) |
| Badge / chip | `rounded` (4px) |
| Kanban task card | `rounded-lg` |
| Progress bar | `rounded-full` hoặc `rounded` |

> **Inconsistency:** LoginView dùng `rounded-xl` cho card và `rounded-lg` cho inputs — khác với chuẩn `rounded-lg` card + `rounded` input ở nơi khác. Cần đồng nhất.

---

## 6. Visual Hierarchy

Phân cấp thị giác kết hợp nhiều chiều cùng lúc (Impeccable):

| Công cụ | Hierarchy mạnh | Hierarchy yếu |
|---------|----------------|----------------|
| **Size** | Tỉ lệ ≥ 3:1 | Tỉ lệ < 2:1 |
| **Weight** | Bold vs Regular | Medium vs Regular |
| **Color** | High contrast | Similar tones |
| **Position** | Top/left (primary) | Bottom/right |
| **Space** | Nhiều whitespace | Chen chúc |

**Reading flow:** Top-left → bottom-right (LTR). Primary action: dialog → bottom-right; navigation → top.

**Progressive disclosure:** Ẩn complexity. Một primary action rõ ràng.

**Shadow & Elevation:** Dùng nhất quán. Dark mode: depth từ surface lightness, không từ shadow.

---

## 7. Accessibility (WCAG AA)

### 8 trạng thái interactive bắt buộc (Impeccable)

| State | Khi nào | Visual treatment |
|-------|---------|-----------------|
| **Default** | Bình thường | Base styling |
| **Hover** | Pointer di qua | Subtle lift, color shift |
| **Focus** | Keyboard focus | Visible ring |
| **Active** | Đang nhấn | Scale down / darker |
| **Disabled** | Không tương tác | `opacity-50`, no pointer |
| **Loading** | Đang xử lý | Spinner / skeleton |
| **Error** | Lỗi | Red border, message |
| **Success** | Hoàn thành | Green check |

### Focus rings (xác minh từ codebase)

**Pattern hiện tại trong codebase:**

```html
<!-- Chuẩn (TaskDetailsView, CreateTaskDialog) -->
class="focus:outline-none focus:ring-1 focus:ring-blue-500"

<!-- LoginView — khác biệt -->
class="focus:outline-none focus:ring-2 focus:ring-blue-500"
```

> **Inconsistency:** `ring-1` vs `ring-2` không nhất quán. Chuẩn hóa: dùng `ring-2` (dày hơn = accessible hơn).

**Không bao giờ dùng `outline: none` mà không có ring replacement** — vi phạm accessibility.

**Chuẩn Impeccable:**
```html
<!-- Chỉ hiện focus ring với keyboard, không với mouse -->
class="focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
```

> **TODO:** Migrate từ `focus:` sang `focus-visible:` để không show ring khi click bằng mouse.

### Touch targets

- Minimum **44×44px** cho mọi element tap được
- Icon buttons trong Navbar: `size-8` (32px) — **dưới chuẩn**. Cần `size-11` hoặc thêm padding

### Keyboard navigation

- Toàn bộ chức năng accessible qua keyboard
- Skip links: chưa implement — TODO
- Modal focus trap: chưa implement rõ ràng — TODO
- Escape key: Comment textarea có `@keydown.escape="mentionQuery = null"` nhưng không close modal

### Semantic HTML

- Dùng `<button>` cho actions — ✓ (xác minh)
- `<label>` cho form inputs — ✓ (xác minh trong CreateTaskDialog, TaskDetailsView)
- Icon-only buttons: một số thiếu `aria-label` — TODO

### Không bao giờ disable zoom

`index.html` dùng `initial-scale=1.0` không có `user-scalable=no` — ✓ đúng.

---

## 8. Responsive Guidelines

### Mobile-first approach

Base styles cho mobile, `min-width` query cho màn hình lớn.

### Breakpoints (sử dụng thực tế)

| Prefix | Dùng ở component nào |
|--------|---------------------|
| `sm` | LayoutView (sidebar overlay toggle), DashboardView (heading size) |
| `md` | StatsGrid (2 cột), ProjectAnalytics (grid layout) |
| `lg` | DashboardView (3 cột), TaskDetailsView (flex-row), Charts (2 cột) |
| `xl` | LayoutView (padding tăng) |
| `max-sm` | Sidebar (absolute position) |

### Mobile adaptations (đã implement)

- Sidebar: overlay mode, toggle qua hamburger, click outside để close
- Stats grid: single column → 2 col → 4 col
- Dashboard: stacked → 3 cột
- DashboardView heading: `text-xl sm:text-2xl`

### Chưa implement

- `@media (pointer: coarse)` cho touch targets lớn hơn
- `@media (hover: none)` để ẩn hover-only interactions trên touch
- Safe area insets (`env(safe-area-inset-*)`) cho mobile notch

### Text overflow (xác minh từ components)

```css
/* Single line — pattern phổ biến trong codebase */
.truncate { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* Multi-line */
.line-clamp-2 { /* -webkit-line-clamp: 2 */ }
```

`min-w-0` được dùng trong một số flex containers (`min-w-0 flex-1` trong Navbar search, TaskDetailsView).

---

## 9. Motion Guidelines

### Duration rules (Impeccable — 100/300/500)

| Duration | Use case |
|----------|----------|
| **100–150ms** | Button press, toggle, color change |
| **200–300ms** | Menu open, tooltip, hover state |
| **300–500ms** | Accordion, modal open, drawer |
| **500–800ms** | Page load entrance |

### Easing curves chuẩn (Impeccable)

```css
/* Dùng — natural deceleration */
--ease-out-quart: cubic-bezier(0.25, 1, 0.5, 1);
--ease-out-quint: cubic-bezier(0.22, 1, 0.36, 1);
--ease-out-expo:  cubic-bezier(0.16, 1, 0.3, 1);
```

> **Thực tế codebase:** Không có custom easing tokens (không có `tailwind.config.js` để thêm). Hiện dùng Tailwind default `ease-out` (`cubic-bezier(0,0,0.2,1)`) qua `transition` class. Chấp nhận được cho v4.

### Transitions đang dùng (xác minh từ codebase)

```css
/* CTA button */
hover:opacity-90 transition

/* Icon button */
hover:scale-105 active:scale-95 transition

/* Card */
transition-all duration-200

/* Task card hover */
hover:shadow-md transition-shadow

/* Nav item, badge, dropdown item */
transition-colors

/* Ghost actions (edit/delete reveal) */
opacity-0 group-hover:opacity-100 transition-opacity

/* Sidebar chevron (project expand) */
transition-transform duration-200

/* Sidebar nav item */
transition-all
```

**Ghost action pattern** (xác minh): dùng nhất quán trong Navbar (drag handle), TaskDetailsView (comment actions), KanbanBoard (grip icon).

### Exit faster than entrance (Impeccable)

Exit animation nên ~75% duration của entrance. Hiện không apply vì chưa có animation JS/GSAP.

### Không bao giờ dùng

- Bounce easing: `cubic-bezier(0.34, 1.56, 0.64, 1)`
- Elastic easing: `cubic-bezier(0.68, -0.6, 0.32, 1.6)`
- Animate layout properties trực tiếp (`width`, `height`, `top`, `left`)
- Duration > 500ms cho feedback

### prefers-reduced-motion

**Chưa implement** trong codebase (không có `@media (prefers-reduced-motion)` trong `index.css`).

```css
/* TODO: Thêm vào src/index.css */
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 10. Component Guidelines

### Buttons

#### Primary (CTA chính — xác minh từ DashboardView, CreateTaskDialog)
```html
<button class="flex items-center gap-2 px-5 py-2 text-sm rounded
  bg-gradient-to-br from-blue-500 to-blue-600 text-white hover:opacity-90 transition">
  <Plus :size="16" /> New Project
</button>
```

#### Secondary outline (xác minh từ CreateTaskDialog)
```html
<button class="rounded border border-zinc-300 dark:border-zinc-700
  px-5 py-2 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800 transition">
  Cancel
</button>
```

#### Ghost / icon-only (xác minh từ Navbar)
```html
<button class="size-8 flex items-center justify-center
  bg-white dark:bg-zinc-800 shadow rounded-lg
  transition hover:scale-105 active:scale-95"
  aria-label="Toggle theme">
  <MoonIcon class="size-4" />
</button>
```

#### Disabled state (xác minh từ CreateTaskDialog, CreateProjectDialog)
```html
:disabled="isSubmitting"
class="disabled:opacity-50"
```

#### Button variant thứ ba — outline accent (xác minh từ DashboardView)
```html
<button class="flex items-center gap-2 px-4 py-2 text-sm rounded
  border border-blue-400 text-blue-600 dark:text-blue-400
  hover:bg-blue-50 dark:hover:bg-blue-950/30 transition">
  <Sparkles :size="14" /> AI Create
</button>
```

### Cards

#### Project Card (xác minh từ `ProjectCard.vue`)
- Border: `border border-gray-200 dark:border-zinc-800`
- Background: `bg-white dark:bg-zinc-950 dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50`
- Hover: `hover:border-gray-300 dark:hover:border-zinc-700`
- Padding: `p-5`
- Radius: `rounded-lg`
- Transition: `transition-all duration-200`

#### Stat Card (xác minh từ `StatsGrid.vue`)
- Cấu trúc: Icon top-right (`rounded-xl`, `bg-[color]/10`) + Label + Value + Subtitle
- Value: `text-3xl font-bold`
- Label: `text-sm text-zinc-500 dark:text-zinc-400`
- Subtitle: `text-xs text-zinc-400 dark:text-zinc-500 mt-1`

#### Card discipline (Impeccable)

- **Không nest card trong card**
- Cards chỉ khi content distinct và actionable
- Spacing và alignment tự tạo grouping — không cần card wrapper cho mọi thứ

### Kanban Column Accent (xác minh từ `KanbanBoard.vue`)

```javascript
const colAccent = {
  0: 'border-t-2 border-zinc-400',   // Backlog
  1: 'border-t-2 border-blue-400',   // ToDo
  2: 'border-t-2 border-amber-400',  // InProgress
  3: 'border-t-2 border-purple-400', // Review
  4: 'border-t-2 border-emerald-500', // Done
  5: 'border-t-2 border-red-400',    // Blocked (không trong enum backend)
}
```

**Drag state:**
- Dragging card: `opacity-50 ring-2 ring-blue-400`
- Drop target column: `ring-2 ring-blue-400`
- Empty column drop zone: `border-2 border-dashed border-zinc-200 dark:border-zinc-700`

### Form Inputs (xác minh từ `CreateTaskDialog.vue`, `TaskDetailsView.vue`)

```javascript
// Constant class pattern
const inputCls = 'w-full mt-1 px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200 focus:outline-none focus:ring-1 focus:ring-blue-500'
```

**Label visible:** Tất cả inputs có `<label>` visible (không chỉ dùng placeholder).

**Validation:** Client-side `required` attribute. Server error hiển thị inline.

### Modals / Dialogs (xác minh từ `CreateTaskDialog.vue`)

```html
<!-- Overlay -->
<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/20 dark:bg-black/60 backdrop-blur">
  <!-- Container -->
  <div class="bg-white dark:bg-zinc-950 border border-zinc-300 dark:border-zinc-800 rounded-lg shadow-lg w-full max-w-lg p-6 text-zinc-900 dark:text-white max-h-[90vh] overflow-y-auto">
  </div>
</div>
```

### Scrollbar Styling (xác minh từ `src/index.css`)

```css
/* Custom webkit scrollbar — áp dụng toàn app */
::-webkit-scrollbar { width: 0.5rem; }

::-webkit-scrollbar-track {
  background-color: #e5e7eb; /* gray-200 */
}
.dark ::-webkit-scrollbar-track {
  background-color: #1c1c1e; /* near-black */
}

::-webkit-scrollbar-thumb {
  background-color: #bbc3d1;
  border-radius: 0.375rem;
}
.dark ::-webkit-scrollbar-thumb {
  background-color: #374151; /* gray-700 */
}

/* Hidden scrollbar utility */
.no-scrollbar::-webkit-scrollbar { display: none; }
```

`.no-scrollbar` được dùng trong Sidebar để ẩn scrollbar nhưng vẫn scroll được.

### Checkbox Styling (xác minh từ `src/index.css`)

```css
input[type="checkbox"] {
  appearance: none;
  background-color: #d1d5db; /* gray-300 */
  border-radius: 9999px; /* rounded-full — hình tròn */
  cursor: pointer;
  width: 1rem; height: 1rem;
}
.dark input[type="checkbox"] { background-color: #374151; }
input[type="checkbox"]:checked { background-color: #3b82f6; }
```

**Lưu ý:** Checkbox dùng `border-radius: 9999px` (hình tròn), không phải hình vuông chuẩn. Đây là design choice của project.

### Date input — dark mode fix

```css
/* index.css */
.dark input[type="date"]::-webkit-calendar-picker-indicator {
  filter: invert(100%);
}
```

Calendar picker icon được invert trong dark mode để visible trên nền tối.

### Icons (xác minh từ codebase)

Chỉ dùng **Lucide Vue Next** — không mix libraries.

| Ngữ cảnh | Size |
|----------|------|
| Inline với text (nav, button) | `size-4` (16px) / `:size="16"` |
| Icon trong card metadata | `size-3.5` (14px) |
| Stat card icon | `:size="20"` (number prop) |
| Badge dot | `size-2` |
| Large decorative | `size-5` (20px) |
| Project sub-nav icon | `size-3` (12px) |

> **Quan trọng:** `:size="20"` là **number prop** của Lucide, KHÔNG phải Tailwind class `size-20`.

### Toast Notifications (xác minh từ `main.js`)

```javascript
app.use(Toast, { timeout: 3000 })
```

`vue-toastification` với timeout 3000ms (3 giây). Toast dùng cho server errors, success confirmations.

---

## 11. Empty / Loading / Error States

### Empty States (xác minh từ codebase)

Pattern tối giản hiện tại:

```html
<!-- Kanban — empty column -->
<div class="flex-1 flex items-center justify-center min-h-16
  border-2 border-dashed border-zinc-200 dark:border-zinc-700 rounded-lg">
  <span class="text-xs text-zinc-300 dark:text-zinc-600">Drop here</span>
</div>

<!-- TaskDetailsView — no comments -->
<div class="text-gray-500 dark:text-zinc-500 text-sm">
  No comments yet. Be the first!
</div>

<!-- Navbar search — no results -->
<div class="px-3 py-4 text-sm text-center text-zinc-400 dark:text-zinc-500">
  No results for "{{ searchQuery }}"
</div>

<!-- ProjectAnalytics — no chart data -->
<p class="text-sm text-zinc-500 dark:text-zinc-400 text-center py-8">
  No tasks to display
</p>
```

**Chuẩn Impeccable cho empty state đầy đủ:**
1. Icon/illustration mô tả loại content
2. Heading: "No projects yet"
3. Description ngắn
4. CTA nếu user có thể tạo mới

### Loading States (xác minh từ codebase)

```html
<!-- TaskDetailsView — loading skeleton text -->
<div v-if="taskStore.taskLoading" class="text-gray-500 dark:text-zinc-400 px-4 py-6">
  Loading task details...
</div>

<!-- Button loading -->
<button :disabled="isSubmitting">
  {{ isSubmitting ? 'Creating...' : 'Create Task' }}
</button>

<!-- Spinner (Lucide) -->
<Loader2 class="size-4 animate-spin" />
```

> **TODO:** Thay "Loading task details..." text bằng skeleton UI để hiển thị shape của content trước khi data đến.

### Error States (xác minh từ codebase)

```html
<!-- Inline error trong form -->
<p v-if="editError" class="text-xs text-red-500">{{ editError }}</p>

<!-- Comment error -->
<div v-if="commentError" class="mt-2 text-xs text-red-500">{{ commentError }}</div>

<!-- Kanban drag error -->
<p v-if="error" class="mb-3 text-sm text-red-500">{{ error }}</p>

<!-- Auth error (LoginView) -->
<p v-if="authStore.error" class="mb-4 text-sm text-red-500 bg-red-50 rounded p-3">
  {{ authStore.error }}
</p>

<!-- 404 state (TaskDetailsView) -->
<div v-else-if="!task" class="text-red-500 px-4 py-6">
  Task not found.
  <button class="ml-4 text-blue-500 hover:underline">Go back</button>
</div>
```

**Error message với recovery path:** TaskDetailsView 404 có "Go back" button — ✓ đúng pattern.

---

## 12. UI Anti-Patterns cần tránh

### Typography

- `outline: none` không có focus ring replacement
- Font size < 14px cho body text thao tác (hiện đã tuân thủ)
- > 3 font family (hiện chỉ dùng Outfit — đúng)
- `user-scalable=no` trong viewport meta (hiện không có — đúng)

### Color

- Gray text trên colored background → dùng shade đậm hơn
- `border-left/right > 1px` làm accent stripe → dùng full border hoặc tint
- Dùng color làm indicator duy nhất (không có text/icon phụ) → fail cho người mù màu
- Purple-blue gradient mặc định (AI slop aesthetic)
- Nhất quán ý nghĩa màu vi phạm (xem Known Inconsistencies)

### Layout

- Nest card trong card
- Card grid lặp vô tận (icon + heading + text)
- Spacing tùy tiện ngoài thang (13px, 17px...)
- Fixed width cho text container (text dài overflow)

### Motion

- Bounce easing: `cubic-bezier(0.34, 1.56, 0.64, 1)`
- Elastic easing: `cubic-bezier(0.68, -0.6, 0.32, 1.6)`
- Duration > 500ms cho feedback
- Animate `width`, `height`, `top`, `left` trực tiếp
- Bỏ qua `prefers-reduced-motion` (**hiện đang vi phạm** — xem Known Inconsistencies)

### Interaction

- Hover-only functionality (touch users không hover)
- Placeholder thay label
- Touch target < 44×44px (**một số icon buttons hiện là 32px** — vi phạm)
- Generic error message không có context
- Double-submit không prevent

### Accessibility

- `<div>` thay `<button>` (hiện tuân thủ — đúng)
- Icon-only button thiếu `aria-label` (**một số buttons trong Navbar thiếu** — TODO)
- Heading hierarchy bị skip

### States

- Blank space khi list rỗng (một số view chỉ có text, chưa có proper empty state)
- Text "Loading..." thay skeleton cho content có shape

---

## 13. Known Codebase Inconsistencies

Các vấn đề được phát hiện khi đọc source code. Cần chuẩn hóa trong future iterations.

### Màu sắc

| Vấn đề | Location | Chuẩn nên dùng |
|--------|----------|----------------|
| `gray-*` vs `zinc-*` | LoginView dùng `bg-gray-50`, `dark:bg-gray-900`, `bg-gray-800` | `zinc-*` cho consistency |
| `dark:text-slate-100` vs `dark:text-zinc-100` | LayoutView vs các components con | Chọn một: `zinc-100` |
| `focus:ring-1` vs `focus:ring-2` | TaskDetailsView (ring-1) vs LoginView (ring-2) | Chuẩn hóa `ring-2` |
| Priority colors inverted trong Analytics | `ProjectAnalytics.vue` dùng Low=red, Medium=blue, High=emerald | Đúng phải là High=red, Low=zinc |

### Border Radius

| Vấn đề | Location | Chuẩn nên dùng |
|--------|----------|----------------|
| `rounded-xl` cho card | LoginView | `rounded-lg` (chuẩn chung) |
| `rounded-lg` cho inputs | LoginView | `rounded` hoặc `rounded-md` |

### Focus Ring Strategy

| Vấn đề | Location | Chuẩn nên dùng |
|--------|----------|----------------|
| `focus:` thay vì `focus-visible:` | Toàn bộ codebase | `focus-visible:` để không show ring khi click bằng mouse |

### Design Tokens

| Vấn đề | Location |
|--------|----------|
| Chart colors hard-coded hex (`#3b82f6`) | `ProjectAnalytics.vue` |
| Scrollbar colors hard-coded hex (`#e5e7eb`, `#bbc3d1`) | `src/index.css` |
| Checkbox colors hard-coded hex (`#d1d5db`, `#374151`, `#3b82f6`) | `src/index.css` |

### UX

| Vấn đề | Location | Giải pháp tốt hơn |
|--------|----------|-------------------|
| `confirm()` native browser dialog | `TaskDetailsView.vue` (xóa comment) | Modal confirmation hoặc undo toast |
| Icon-only buttons thiếu `aria-label` | Navbar (theme toggle, notifications) | Thêm `aria-label` |

---

## Phụ lục A: Polish Checklist (Impeccable)

Trước khi xem là xong một feature:

- [ ] Aligned với design system (không drift)
- [ ] Typography hierarchy rõ ràng khi squint
- [ ] Spacing dùng design token (không arbitrary)
- [ ] Đủ 8 interactive states
- [ ] Transitions smooth 60fps, đúng duration
- [ ] Copy nhất quán, button labels action-oriented
- [ ] Icons nhất quán (Lucide only), sized đúng
- [ ] Form inputs có label visible + error handling
- [ ] Loading states (skeleton > spinner)
- [ ] Empty states có CTA
- [ ] Error states có recovery path
- [ ] Touch targets ≥ 44×44px
- [ ] WCAG AA contrast
- [ ] Keyboard navigation hoạt động
- [ ] Focus indicators visible (`focus-visible:ring-2`)
- [ ] `prefers-reduced-motion` được respect
- [ ] Dark mode đúng
- [ ] Không console errors

---

## Phụ lục B: Những phần chưa bổ sung được

### Đã xác minh — chưa implement trong codebase

| Phần | Trạng thái | Ghi chú |
|------|-----------|---------|
| `prefers-reduced-motion` | **Chưa có** trong `index.css` | Cần thêm — accessibility requirement |
| `focus-visible:` thay `focus:` | **Chưa dùng** | Migrate để không show ring khi click bằng mouse |
| `tabular-nums` cho số liệu | **Chưa có** | Thêm cho stat numbers, progress counts |
| Skeleton loading | **Chưa có** | Chỉ có text "Loading..." |
| Skip links | **Chưa có** | Cần cho keyboard navigation |
| Modal focus trap | **Chưa có** | Vue `<dialog>` hoặc `inert` attribute |
| `aria-label` cho một số icon buttons | **Thiếu** | Navbar theme button, notification button |
| Fallback font metrics | **Chưa có** | `size-adjust`, `ascent-override` để giảm FOUT |

### Không áp dụng cho project này

| Phần | Lý do |
|------|-------|
| Container queries | App dùng viewport-based layout — không có component cần container-aware sizing |
| Fluid `clamp()` typography | Product UI dùng fixed rem scale — fluid type không phù hợp |
| Safe area insets | App không được build như PWA/mobile app |
| Custom easing tokens | Tailwind v4 không có `tailwind.config.js` — cần thêm qua CSS variable trong `index.css` nếu muốn |
| RTL support | App chỉ hỗ trợ LTR (English/Vietnamese) |
| OKLCH color space | Tailwind v4 dùng oklch nội bộ, nhưng custom colors vẫn có thể dùng hex |
| Print styles | Không có use case |

### Thiếu thông tin để bổ sung

| Phần | Thiếu gì |
|------|---------|
| Data visualization color tokens | Chart colors hiện hard-coded — cần design decision về token structure |
| Animation library | Không có Framer Motion / GSAP — motion phức tạp hơn cần quyết định adopt library nào |
| Component library alignment | Chưa có shared component library — mỗi dialog có pattern riêng |
