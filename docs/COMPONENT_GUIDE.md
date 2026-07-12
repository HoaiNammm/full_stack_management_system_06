# COMPONENT_GUIDE.md — Hướng Dẫn Component

> Tài liệu này mô tả cách sử dụng, biến thể và code mẫu cho từng component UI cốt lõi.
> Tất cả đều được trích xuất từ codebase thực tế (`src/components/`, `src/views/`).

---

## 1. Button

### Biến thể

#### Primary (Gradient Blue)

Dùng cho: hành động chính trong page/dialog (New Project, Create Task, Sign In, Post comment)

```html
<button class="flex items-center gap-2 px-5 py-2 text-sm rounded
  bg-gradient-to-br from-blue-500 to-blue-600
  text-white hover:opacity-90 transition">
  <Plus :size="16" />
  New Project
</button>
```

#### Secondary (Border)

Dùng cho: hành động thứ cấp, Cancel, hành động ít quan trọng hơn

```html
<button class="flex items-center gap-2 px-4 py-2 text-sm rounded
  border border-zinc-300 dark:border-zinc-700
  text-zinc-700 dark:text-zinc-300
  hover:bg-zinc-100 dark:hover:bg-zinc-800 transition">
  Cancel
</button>
```

#### Icon Button (Ghost)

Dùng cho: Navbar actions (theme toggle, notification bell), back button

```html
<button class="size-8 flex items-center justify-center
  bg-white dark:bg-zinc-800 shadow rounded-lg
  transition hover:scale-105 active:scale-95">
  <Bell class="size-4 text-gray-700 dark:text-gray-300" />
</button>
```

#### Danger (Destructive)

Dùng cho: Xóa, Remove member

```html
<button class="flex items-center gap-2 px-4 py-2 text-sm rounded
  text-red-600 dark:text-red-400
  hover:bg-red-50 dark:hover:bg-zinc-800 transition">
  <Trash2 class="size-4" />
  Delete
</button>
```

#### Small / Inline

Dùng cho: hành động trong row (Edit, Log time), sub-action trong card

```html
<button class="flex items-center gap-1.5 px-3 py-1.5 text-sm rounded
  border border-zinc-300 dark:border-zinc-700
  hover:bg-zinc-100 dark:hover:bg-zinc-800">
  <Plus class="size-3.5" />
  Add
</button>
```

### States

| State | Modifier |
|-------|---------|
| Default | — |
| Hover | `hover:opacity-90` hoặc `hover:bg-zinc-100` |
| Disabled | `:disabled="loading"` + `class="disabled:opacity-50"` |
| Loading | Thay text: `"Creating..."` + `:disabled="true"` |
| Active/Pressed | `active:scale-95` |

### Quy tắc sử dụng

- Mỗi form/dialog tối đa **1 primary button**
- Primary button luôn ở góc **phải dưới** của form/dialog
- Cancel button ở **bên trái** primary button
- Loading state: disable nút + đổi text, không thêm spinner riêng biệt

---

## 2. Table / List

### Project Tasks List (ProjectTasks.vue)

Dùng pattern list thay vì HTML `<table>` truyền thống:

```html
<!-- Row pattern -->
<div class="flex items-center gap-3 px-4 py-3 rounded
  border border-zinc-200 dark:border-zinc-700
  bg-white dark:bg-zinc-900
  cursor-pointer hover:shadow-sm transition-shadow group">

  <!-- Status dot hoặc badge -->
  <span :class="['text-xs px-2 py-0.5 rounded', statusCls[task.status]]">
    {{ task.status }}
  </span>

  <!-- Title -->
  <p class="text-sm font-medium text-zinc-900 dark:text-zinc-100 truncate flex-1">
    {{ task.title }}
  </p>

  <!-- Meta: priority, assignee, due date -->
  <span class="text-xs px-2 py-0.5 rounded ...">{{ task.priority }}</span>

  <!-- Ghost actions — hiện khi hover row -->
  <div class="opacity-0 group-hover:opacity-100 flex items-center gap-1">
    <button @click.stop="..."><Pencil class="size-3.5" /></button>
    <button @click.stop="..."><Trash2 class="size-3.5" /></button>
  </div>
</div>
```

### Quy tắc

1. `group` trên row, `group-hover:opacity-100` trên action buttons
2. `@click.stop` trên action buttons để không trigger row click
3. Actions chỉ hiện khi hover — không chiếm space khi không dùng
4. Truncate long text, không let overflow

---

## 3. Modal / Dialog

### Cấu trúc chuẩn

```html
<!-- Overlay -->
<div v-if="show"
  class="fixed inset-0 z-50 flex items-center justify-center
    bg-black/20 dark:bg-black/60 backdrop-blur">

  <!-- Dialog container -->
  <div class="bg-white dark:bg-zinc-950
    border border-zinc-300 dark:border-zinc-800
    rounded-lg shadow-lg
    w-full max-w-lg p-6
    text-zinc-900 dark:text-white
    max-h-[90vh] overflow-y-auto">

    <!-- Header -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl font-bold">Dialog Title</h2>
      <button @click="$emit('close')" class="p-1 text-zinc-400 hover:text-zinc-700">
        <X class="size-4" />
      </button>
    </div>

    <!-- Body -->
    <div class="space-y-4">
      <!-- content -->
    </div>

    <!-- Footer -->
    <div class="flex justify-end gap-2 pt-4 mt-4
      border-t border-zinc-200 dark:border-zinc-700">
      <button @click="$emit('close')" class="...">Cancel</button>
      <button type="submit" class="...">Confirm</button>
    </div>
  </div>
</div>
```

### Kích thước

| Size | Class | Dùng cho |
|------|-------|---------|
| Small | `max-w-sm` | Confirmation dialog, simple form |
| Medium (default) | `max-w-lg` | Create Task, Edit Project |
| Large | `max-w-2xl` | Multi-step wizard (Create Project) |

### Đóng modal

- Nút X ở góc trên phải
- Click overlay (tùy chọn, không bắt buộc)
- `Escape` key (nên implement)
- Emit `'close'` event lên parent

### Quy tắc

1. Không nest modal trong modal
2. Z-index: `z-50` cho modal, `z-20` cho dropdown trong modal
3. Scroll nội dung bên trong modal, không scroll cả trang
4. Khi submit xong: close modal + reset form

---

## 4. Form

### Form field chuẩn

```html
<div class="space-y-1">
  <label class="text-sm font-medium text-zinc-700 dark:text-zinc-300">
    Field Label *
  </label>
  <input
    v-model="formData.field"
    type="text"
    required
    placeholder="Placeholder text"
    class="w-full mt-1 px-3 py-1.5 text-sm rounded
      border border-zinc-300 dark:border-zinc-700
      dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200
      focus:outline-none focus:ring-1 focus:ring-blue-500"
  />
  <p v-if="errors.field" class="text-xs text-red-500 mt-1">
    {{ errors.field }}
  </p>
</div>
```

### Input types

#### Text Input
```html
<input type="text" :class="inputCls" v-model="..." />
```

#### Textarea
```html
<textarea
  v-model="..."
  rows="3"
  :class="inputCls + ' resize-none'"
/>
```

#### Select
```html
<select v-model="..." :class="inputCls">
  <option value="">-- Select --</option>
  <option v-for="item in items" :key="item.id" :value="item.id">
    {{ item.name }}
  </option>
</select>
```

#### Date Input
```html
<input
  type="date"
  v-model="formData.date"
  :min="today"
  :class="inputCls"
/>
```

#### Number Input
```html
<input
  type="number"
  v-model.number="formData.hours"
  min="0"
  step="0.5"
  :class="inputCls"
/>
```

### CSS token chuẩn (tái sử dụng)

> **TODO:** `inputCls` không nhất quán giữa các component. TaskDetailsView dùng `py-1.5 dark:bg-zinc-800`; CreateTaskDialog dùng `py-2 dark:bg-zinc-900`. Giá trị dưới đây là từ TaskDetailsView — không phải chuẩn duy nhất trong codebase.

```javascript
// Trong component script
const inputCls = 'w-full mt-1 px-3 py-1.5 text-sm rounded border border-zinc-300 dark:border-zinc-700 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-200 focus:outline-none focus:ring-1 focus:ring-blue-500'
const selectCls = inputCls  // Giống nhau — TODO: CreateTaskDialog dùng py-2 và dark:bg-zinc-900 thay vì py-1.5 và dark:bg-zinc-800
```

### Form layout

#### Single column (mặc định)
```html
<form class="space-y-4">...</form>
```

#### Two columns (fields ngang hàng)
```html
<div class="grid grid-cols-2 gap-4">
  <div class="space-y-1"><!-- Field 1 --></div>
  <div class="space-y-1"><!-- Field 2 --></div>
</div>
```

### Validation pattern

```javascript
// Validate trước submit
function validate() {
  errors.value = {}
  if (!form.value.title.trim()) errors.value.title = 'Title is required'
  if (!form.value.startDate) errors.value.startDate = 'Start date is required'
  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  isSubmitting.value = true
  // ...
}
```

### Label với dấu bắt buộc

```html
<!-- Trường bắt buộc -->
<label class="text-sm font-medium">Title <span class="text-red-500">*</span></label>

<!-- Trường tùy chọn — không cần ghi gì thêm -->
<label class="text-sm font-medium">Description</label>
```

---

## 5. Card

### Project Card

> **TODO:** `<StatusBadge>` KHÔNG phải Vue component thực sự — status badge là inline `<span>` với class binding. `project.priority`, `project.memberCount`, `project.sprintCount`, `project.progress` — các field tên này chưa được xác minh là tên chính xác trong API response từ ProjectService.

```html
<router-link :to="`/projectsDetail?id=${project.id}&tab=tasks`"
  class="bg-white dark:bg-zinc-950
    dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50
    border border-gray-200 dark:border-zinc-800
    hover:border-gray-300 dark:hover:border-zinc-700
    rounded-lg p-5
    transition-all duration-200 group block">

  <!-- Header: Tên + Mô tả -->
  <h3 class="font-semibold text-gray-900 dark:text-zinc-200 mb-1 truncate
    group-hover:text-blue-500 dark:group-hover:text-blue-400 transition-colors">
    {{ project.name }}
  </h3>
  <p class="text-gray-500 dark:text-zinc-400 text-sm line-clamp-2 mb-3">
    {{ project.description || 'No description' }}
  </p>

  <!-- Status badge + priority -->
  <div class="flex items-center justify-between mb-4">
    <StatusBadge :status="project.status" />  <!-- TODO: không phải component thực, dùng inline span -->
    <span class="text-xs text-gray-500">{{ project.priority }} priority</span>  <!-- TODO: project.priority chưa xác minh field name -->
  </div>

  <!-- Progress bar -->
  <div class="space-y-2">
    <div class="flex items-center justify-between text-xs">
      <span class="text-gray-500">Progress</span>
      <span>{{ project.progress || 0 }}%</span>  <!-- TODO: project.progress chưa xác minh field name -->
    </div>
    <div class="w-full bg-gray-200 dark:bg-zinc-800 h-1.5 rounded">
      <div class="h-1.5 rounded bg-blue-500" :style="{ width: `${project.progress || 0}%` }" />
    </div>
  </div>

  <!-- Footer meta -->
  <div class="flex items-center gap-3 mt-3 text-xs text-gray-400 dark:text-zinc-500">
    <span>{{ project.memberCount }} members</span>  <!-- TODO: project.memberCount chưa xác minh field name -->
    <span>{{ project.sprintCount }} sprints</span>  <!-- TODO: project.sprintCount chưa xác minh field name -->
  </div>
</router-link>
```

### Stat Card (Dashboard)

```html
<div class="bg-white dark:bg-zinc-950
  dark:bg-gradient-to-br dark:from-zinc-800/70 dark:to-zinc-900/50
  border border-zinc-200 dark:border-zinc-800
  hover:border-zinc-300 dark:hover:border-zinc-700
  transition duration-200 rounded-md">
  <div class="p-6 py-4">
    <div class="flex items-start justify-between">
      <div>
        <p class="text-sm text-zinc-500 dark:text-zinc-400 mb-1">{{ card.title }}</p>
        <p class="text-3xl font-bold text-zinc-800 dark:text-white">{{ card.value }}</p>
        <p class="text-xs text-zinc-400 dark:text-zinc-500 mt-1">{{ card.subtitle }}</p>
      </div>
      <!-- Icon box -->
      <div :class="['p-3 rounded-xl bg-opacity-20', card.bgColor]">
        <component :is="card.icon" :size="20" :class="card.textColor" />
      </div>
    </div>
  </div>
</div>
```

### Task Card (Kanban)

> **TODO:** `<PriorityBadge>`, `<LabelChip>`, `<AssigneeAvatar>`, `<DueDateChip>`, `<SubtaskProgress>` KHÔNG phải là Vue component thực sự trong codebase. Toàn bộ task card là inline HTML bên trong KanbanBoard.vue, không có component con riêng biệt. Code dưới đây là pseudo-code minh họa cấu trúc, không phải code thực tế.

```html
<div draggable="true"
  class="group relative bg-white dark:bg-zinc-800
    border border-zinc-200 dark:border-zinc-700
    rounded-lg p-3
    cursor-grab active:cursor-grabbing
    hover:shadow-md transition-shadow select-none">

  <!-- Drag handle hint -->
  <GripVertical class="absolute right-2 top-3 size-3.5 text-zinc-300
    opacity-0 group-hover:opacity-100 transition-opacity" />

  <!-- Title -->
  <p class="text-sm font-medium leading-snug pr-5 mb-2">{{ task.title }}</p>

  <!-- Priority + Labels — inline spans, KHÔNG phải component -->
  <div class="flex items-center gap-2 flex-wrap">
    <PriorityBadge :priority="task.priority" />  <!-- TODO: không phải component thực -->
    <LabelChip v-for="lbl in task.labels" :key="lbl" :label="lbl" />  <!-- TODO: không phải component thực -->
  </div>

  <!-- Footer: Assignee + Due date — inline HTML -->
  <div class="flex items-center justify-between mt-2 gap-1">
    <AssigneeAvatar :assignee="task.assignee" />  <!-- TODO: không phải component thực -->
    <DueDateChip v-if="task.dueDate" :date="task.dueDate" />  <!-- TODO: không phải component thực -->
  </div>

  <!-- Subtask progress bar — inline HTML -->
  <SubtaskProgress v-if="task.subTaskCount > 0"
    :completed="task.completedSubTaskCount" :total="task.subTaskCount" />  <!-- TODO: không phải component thực -->
</div>
```

---

## 6. Badge

### Status Badge (Task)

```html
<span :class="['px-2 py-0.5 rounded text-xs', statusBadge[task.status]]">
  {{ task.status }}
</span>

<!-- statusBadge map -->
const statusBadge = {
  Backlog:    'bg-zinc-200 text-zinc-800 dark:bg-zinc-700 dark:text-zinc-300',
  ToDo:       'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  InProgress: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-300',
  Review:     'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
  Done:       'bg-emerald-100 text-emerald-800 dark:bg-emerald-900 dark:text-emerald-300',
  // TODO: `Blocked` KHÔNG tồn tại trong KanbanStatus enum backend (Backlog/ToDo/InProgress/Review/Done). Xóa entry này.
  Blocked:    'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
}
```

### Priority Badge (Task)

```html
<span :class="['text-xs px-1.5 py-0.5 rounded', priorityCls[task.priority]]">
  {{ task.priority }}
</span>

const priorityCls = {
  High:   'bg-red-100 text-red-700 dark:bg-red-900/60 dark:text-red-300',
  Medium: 'bg-amber-100 text-amber-700 dark:bg-amber-900/60 dark:text-amber-300',
  Low:    'bg-zinc-100 text-zinc-500 dark:bg-zinc-700 dark:text-zinc-400',
}
```

### Project Status Badge

```html
<span :class="['px-2 py-1 rounded text-xs', statusColors[project.status]]">
  {{ project.status }}
</span>

const statusColors = {
  Planning:  'bg-zinc-200 text-zinc-900 dark:bg-zinc-600 dark:text-zinc-200',
  Active:    'bg-emerald-200 text-emerald-900 dark:bg-emerald-500 dark:text-emerald-900',
  OnHold:    'bg-amber-200 text-amber-900 dark:bg-amber-500 dark:text-amber-900',
  Completed: 'bg-blue-200 text-blue-900 dark:bg-blue-500 dark:text-blue-900',
  Cancelled: 'bg-purple-200 text-purple-900 dark:bg-purple-800 dark:text-purple-200',
}
```

### Label Chip (Task)

Label chips dùng màu tính từ hash string (không đổi với cùng label text):

```javascript
const LABEL_COLORS = [
  'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
  'bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300',
  'bg-teal-100 text-teal-700 dark:bg-teal-900 dark:text-teal-300',
  'bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300',
  'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
  'bg-pink-100 text-pink-700 dark:bg-pink-900 dark:text-pink-300',
]

function labelColor(str) {
  let h = 0
  for (const c of str) h = c.charCodeAt(0) + ((h << 5) - h)
  return LABEL_COLORS[Math.abs(h) % LABEL_COLORS.length]
}
```

```html
<span :class="['text-[10px] px-1.5 py-0.5 rounded', labelColor(lbl)]">
  {{ lbl }}
</span>
```

### Notification Count Badge

```html
<span v-if="count > 0"
  class="absolute -top-1 -right-1
    min-w-[16px] h-4 px-1
    bg-red-500 text-white text-[10px] font-bold
    rounded-full flex items-center justify-center">
  {{ count > 99 ? '99+' : count }}
</span>
```

### Quy tắc chung cho Badge

1. Luôn có cả **màu nền** và **màu text** — không chỉ màu text
2. Radius: `rounded` (4px) cho status/priority, `rounded-full` cho count
3. Padding: `px-2 py-0.5` cho text badge, `px-1.5 py-0.5` cho chip nhỏ
4. Font size: `text-xs` (12px) hoặc `text-[10px]` cho badge rất nhỏ
5. Không có icon trong badge — badge chỉ chứa text

---

## 7. Avatar

### User Avatar

```html
<!-- Có ảnh -->
<img
  v-if="user.avatarUrl"
  :src="user.avatarUrl"
  alt="avatar"
  class="size-7 rounded-full object-cover"
/>

<!-- Không có ảnh — hiển thị chữ cái đầu -->
<div v-else
  class="size-7 rounded-full
    bg-gradient-to-br from-blue-400 to-blue-600
    flex items-center justify-center
    text-white text-xs font-bold">
  {{ user.name?.[0]?.toUpperCase() || '?' }}
</div>
```

### Kích thước Avatar

| Kích thước | Class | Dùng ở đâu |
|-----------|-------|-----------|
| XS | `size-5` (20px) | Task card footer, comment list |
| Small | `size-6` (24px) | @mention dropdown |
| Medium | `size-7` (28px) | Navbar user button |
| Large | `size-8` (32px) | Profile page, team list |

### Avatar Group (nhiều người)

> **TODO:** Avatar Group component KHÔNG tồn tại trong codebase. Không có file `AvatarGroup.vue` hay pattern `-space-x-2` nào được xác minh trong source. Đây là pattern đề xuất, chưa implement.

Hiển thị tối đa N avatar xếp chồng nhau:

```html
<div class="flex -space-x-2">
  <img v-for="(m, i) in members.slice(0, 4)" :key="m.id"
    :src="m.avatarUrl"
    class="size-6 rounded-full border-2 border-white dark:border-zinc-900" />
  <div v-if="members.length > 4"
    class="size-6 rounded-full
      bg-zinc-200 dark:bg-zinc-700
      border-2 border-white dark:border-zinc-900
      flex items-center justify-center
      text-[10px] text-zinc-600 dark:text-zinc-300">
    +{{ members.length - 4 }}
  </div>
</div>
```

### Role Indicator

> **TODO:** `<Avatar>` KHÔNG phải Vue component thực sự — không có `Avatar.vue` trong codebase. Pattern hiển thị avatar + role là inline HTML (img + div) trong từng component. Đây là pseudo-code minh họa.

Hiển thị role của member kết hợp với avatar:

```html
<div class="flex items-center gap-2">
  <Avatar :user="member.user" size="sm" />  <!-- TODO: không phải component thực -->
  <div>
    <p class="text-sm font-medium">{{ member.user?.name }}</p>
    <p class="text-xs text-zinc-500">{{ member.role }}</p>
  </div>
</div>
```

### Fallback Avatar Color

Hiện tại dùng blue gradient cho tất cả fallback (`bg-gradient-to-br from-blue-400 to-blue-600`).

> **TODO:** Hàm `avatarColor(name)` bên dưới KHÔNG tồn tại trong codebase. Đây là đề xuất cải thiện. Code thực tế dùng fixed gradient `from-blue-400 to-blue-600` cho tất cả user không có avatar.

Có thể cải thiện bằng cách hash màu từ tên user (chưa implement):

```javascript
// TODO: đây là đề xuất, chưa có trong source code
const AVATAR_COLORS = [
  'from-blue-400 to-blue-600',
  'from-emerald-400 to-emerald-600',
  'from-purple-400 to-purple-600',
  'from-amber-400 to-amber-600',
  'from-red-400 to-red-600',
]

function avatarColor(name) {
  let h = 0
  for (const c of name) h = c.charCodeAt(0) + ((h << 5) - h)
  return `bg-gradient-to-br ${AVATAR_COLORS[Math.abs(h) % AVATAR_COLORS.length]}`
}
```

---

## 8. Progress Bar

### Task Progress (trong project header)

```html
<div class="flex items-center gap-2 min-w-48">
  <span class="flex-shrink-0 font-medium text-sm">Progress</span>
  <div class="flex-1 h-1.5 bg-zinc-200 dark:bg-zinc-700 rounded-full overflow-hidden">
    <div
      class="h-full bg-emerald-500 rounded-full transition-all"
      :style="{ width: progress + '%' }"
    />
  </div>
  <span class="flex-shrink-0 text-xs text-emerald-600 dark:text-emerald-400 font-semibold">
    {{ progress }}%
  </span>
</div>
```

### Subtask Progress (trong task card)

```html
<div v-if="task.subTaskCount > 0" class="mt-2">
  <div class="flex justify-between text-xs text-zinc-400 mb-0.5">
    <span>{{ task.completedSubTaskCount }}/{{ task.subTaskCount }} subtasks</span>
  </div>
  <div class="h-1 bg-zinc-200 dark:bg-zinc-700 rounded-full overflow-hidden">
    <div
      class="h-full bg-emerald-500 rounded-full transition-all"
      :style="{ width: `${Math.round((task.completedSubTaskCount / task.subTaskCount) * 100)}%` }"
    />
  </div>
</div>
```

### Quy tắc Progress Bar

| Thuộc tính | Giá trị |
|-----------|--------|
| Màu fill | `bg-emerald-500` (progress), `bg-blue-500` (project card) |
| Background | `bg-gray-200 dark:bg-zinc-800` hoặc `bg-zinc-200 dark:bg-zinc-700` |
| Height | `h-1` (4px, subtask) hoặc `h-1.5` (6px, project) |
| Border radius | `rounded-full` cho cả track và fill |
| Overflow | `overflow-hidden` trên track |
| Animation | `transition-all` khi width thay đổi |
