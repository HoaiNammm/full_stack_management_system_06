# UI_RULES.md — Quy Tắc Thiết Kế Giao Diện

> Các quy tắc bắt buộc khi phát triển và mở rộng UI.
> Mục tiêu: nhất quán, dễ dùng, không gây nhầm lẫn.

---

## 1. Quy Tắc Tổng Quát

### R-01: Không để người dùng đoán mò trạng thái

Mọi hành động async phải có ít nhất một trong ba: loading indicator, success feedback, error message. Không được để nút "trơ" sau khi bấm.

```
Sai:  Nhấn "Create Task" → không có phản hồi gì → user bấm lại nhiều lần
Đúng: Nhấn → nút disabled + text "Creating..." → toast success hoặc error message
```

### R-02: Destructive action phải có xác nhận

> **TODO:** Chỉ xác minh được `confirm()` cho **xóa comment** trong TaskDetailsView.vue. Xóa task và xóa project chưa được xác minh có dùng confirm hay không trong source hiện tại. Đây là quy tắc nên áp dụng, nhưng chưa chắc đã implement đầy đủ.

Mọi thao tác xóa (delete project, delete task, delete comment) phải hiển thị dialog xác nhận hoặc ít nhất `confirm()` trước khi thực hiện. Không xóa ngay khi bấm.

```javascript
// Hiện tại đang dùng (chấp nhận tạm thời):
if (!confirm('Delete this comment?')) return

// Tốt hơn: custom confirmation dialog với mô tả rõ ràng
```

### R-03: Hành động nguy hiểm phải khác biệt về màu

- Delete, Remove: nút đỏ (`text-red-600` hoặc `bg-red-500`)
- Archive, Disable: màu cam (`text-amber-600`)
- Primary action: blue gradient
- Secondary action: border gray

### R-04: Trạng thái active của nav phải rõ ràng

```css
/* Sidebar nav item active */
active-class="bg-gray-100 dark:bg-gradient-to-br dark:from-zinc-800 dark:to-zinc-800/50"

/* Tab active trong ProjectDetailsView */
bg-zinc-100 dark:bg-zinc-800/80
```

Người dùng phải biết đang ở đâu trong app chỉ nhìn vào sidebar và tab header.

### R-05: Truncate text dài, không overflow layout

- Title trong card: `truncate` (1 dòng) hoặc `line-clamp-2` (2 dòng)
- Email/tên trong dropdown: `truncate` với `max-w-[...]`
- Không để text tràn ra ngoài container

### R-06: Icon luôn kèm theo text hoặc tooltip khi không đủ rõ ràng

Icon-only button phải đủ self-explanatory hoặc có tooltip/aria-label. Ngoại lệ: Bell, Search, Moon/Sun — đủ rõ ràng theo context.

---

## 2. Accessibility

> **TODO:** Các quy tắc accessibility A-01 đến A-06 dưới đây là best practices được đề xuất — phần lớn CHƯA được xác minh là đã implement trong code hiện tại. Nhiều rule là tiêu chuẩn ngành, không phải mô tả trạng thái hiện tại của codebase.

### A-01: Tất cả input đều có label

```html
<!-- Đúng -->
<label class="text-xs text-zinc-500 dark:text-zinc-400">Title *</label>
<input v-model="..." required />

<!-- Sai: placeholder không thay thế được label -->
<input placeholder="Enter title" />
```

### A-02: Nút submit trong form phải là `type="submit"`

Không dùng `<div @click>` thay thế nút. Đảm bảo `Enter` trong form submit được.

### A-03: Focus ring phải hiển thị

```css
/* Mọi input, button đều cần */
focus:outline-none focus:ring-1 focus:ring-blue-500
/* Hoặc */
focus:ring-2 focus:ring-blue-500
```

Không `outline-none` mà không thêm ring thay thế.

### A-04: Màu sắc không là thông tin duy nhất

Status badge phải có cả màu VÀ text. Không dùng chỉ màu đỏ/xanh mà không có nhãn chữ:

```html
<!-- Đúng -->
<span class="bg-red-100 text-red-700">High</span>

<!-- Sai: không rõ khi in đen trắng hoặc cho người mù màu -->
<span class="size-3 rounded-full bg-red-500" />
```

### A-05: Contrast đủ tối thiểu

Tuân theo WCAG AA (4.5:1 cho normal text). Palette đang dùng (zinc, blue, emerald...) đã đạt khi dùng dark text trên light background và ngược lại.

Không dùng `text-zinc-300` trên `bg-white` hoặc `text-zinc-600` trên `bg-zinc-500`.

### A-06: Keyboard navigation

> **TODO:** Escape chỉ được implement cho **@mention dropdown** trong TaskDetailsView.vue (`@keydown.escape="mentionQuery = null"`). Các modal thực sự (CreateTaskDialog, CreateProjectDialog, InviteMemberDialog...) KHÔNG có Escape handler. "đã implement trong comment @mention" đúng nhưng không đại diện cho toàn bộ app.

- Modal/dialog: `Escape` để đóng (đã implement trong comment @mention)
- Form: `Enter` để submit
- Tab flow hợp lý trong form (không nhảy lung tung)

---

## 3. Responsive

### Breakpoint Strategy

```
Mobile first. Tất cả class mặc định là mobile, override lên larger screens.
```

### R-01: Sidebar ẩn trên mobile

```css
/* Sidebar */
max-sm:absolute  /* Overlay trên mobile */
transition-all   /* Animate vào/ra */
left-0 / -left-full  /* Toggle bằng isSidebarOpen state */
```

Trên mobile, sidebar không chiếm space — content full width.

### R-02: Grid layout phải responsive

```css
/* Stats Grid */
grid-cols-1 md:grid-cols-2 lg:grid-cols-4

/* Dashboard layout */
grid lg:grid-cols-3

/* Form grids */
grid-cols-2  /* OK trên ≥ 400px, luôn dùng cho field pairs trong dialog */
```

### R-03: Text phải readable trên mọi kích thước

Heading page: `text-xl sm:text-2xl`
Không hardcode font size nhỏ hơn 12px (text-xs) cho body text trên mobile.

### R-04: Kanban board có horizontal scroll

```css
/* KanbanBoard wrapper */
overflow-x-auto pb-2
/* Inner */
flex gap-3 min-w-max  /* Columns không wrap, scroll ngang */
```

Column width fixed `w-60` (240px), không responsive — intentional để maintain readability.

### R-05: Modal trên mobile

```css
w-full max-w-lg  /* Tự fill width trên mobile */
max-h-[90vh] overflow-y-auto  /* Scroll nếu cao hơn màn hình */
```

### R-06: Tab navigation trong ProjectDetails

```css
inline-flex flex-wrap max-sm:grid grid-cols-5
```

Trên mobile: tab chuyển sang grid 5 cột để không overflow.

---

## 4. Empty State

### Khi nào cần Empty State

Mọi danh sách có thể trống đều phải có empty state. Không được để component trống mà không có gì hiển thị.

### Pattern chuẩn

#### Empty state nhỏ (trong card, column)

```html
<!-- Kanban column trống -->
<div class="flex-1 flex items-center justify-center min-h-16
  border-2 border-dashed border-zinc-200 dark:border-zinc-700 rounded-lg">
  <span class="text-xs text-zinc-300 dark:text-zinc-600">Drop here</span>
</div>
```

#### Empty state trung bình (section trong page)

```html
<!-- Backlog trống -->
<div class="py-12 text-center text-sm text-zinc-500 dark:text-zinc-400">
  No unassigned tasks — all tasks are in a sprint.
</div>
```

#### Empty state toàn trang

```html
<!-- Project not found -->
<div class="p-6 text-center text-zinc-900 dark:text-zinc-200">
  <p class="text-3xl md:text-5xl mt-40 mb-10">Project not found</p>
  <button @click="router.push('/projects')"
    class="mt-4 px-4 py-2 rounded bg-zinc-200 text-zinc-900 ...">
    Back to Projects
  </button>
</div>
```

### Quy tắc

1. Empty state phải **giải thích lý do** hoặc **gợi ý hành động tiếp theo**
2. Không chỉ hiển thị "No data" mà không có context
3. Nếu có quyền, cung cấp **CTA** để tạo nội dung mới
4. Nếu không có quyền, giải thích ngắn gọn ai có quyền

```
Sai:  "No tasks"
Đúng: "No tasks yet. Create the first task to get started." + button "New Task"
Đúng: "No tasks in this sprint. Add tasks from Backlog."
```

---

## 5. Error State

### E-01: Error message phải cụ thể

```
Sai:  "An error occurred"
Đúng: "Failed to create task. Please check your input and try again."
Đúng: "Task title is required."
Đúng: "You don't have permission to delete this project."
```

### E-02: Vị trí hiển thị error

| Loại lỗi | Vị trí hiển thị |
|----------|----------------|
| Form validation | Ngay dưới field bị lỗi, hoặc trên cùng form |
| API error (action) | Dưới form hoặc toast notification |
| Page-level error | Banner cảnh báo ở trên cùng content |
| 404 / Not found | Full-page empty state với back button |

### E-03: Màu sắc error

```css
/* Text error */
text-red-500  hoặc  text-red-600

/* Background error (form) */
bg-red-50 text-red-500  (dùng trong LoginView)

/* Error border */
border-red-400 focus:ring-red-400  (khi field invalid)
```

### E-04: Error phải auto-clear khi user bắt đầu fix

Khi user bắt đầu nhập lại vào field bị lỗi, error message của field đó phải biến mất. Không để error cũ lơ lửng.

### E-05: Pattern hiện tại trong codebase

```javascript
// authStore pattern (đúng):
clearError()  // gọi trước khi submit
this.error = err.response?.data?.error || 'Login failed'

// component pattern:
const error = ref('')
// Reset trước khi retry
error.value = ''
// Hiển thị:
<p v-if="error" class="mb-3 text-sm text-red-500 bg-red-50 rounded p-3">
  {{ error }}
</p>
```

---

## 6. Loading State

### L-01: Mọi async action phải block UI

Khi đang loading:
- Nút phải `disabled` và đổi text (`Creating...`, `Saving...`, `Posting...`)
- Nếu là page load: hiện loading skeleton hoặc spinner

```html
<button type="submit" :disabled="isSubmitting">
  {{ isSubmitting ? 'Creating...' : 'Create Task' }}
</button>
```

### L-02: Loading text pattern

| Action | Loading text |
|--------|-------------|
| Create | "Creating..." |
| Save / Update | "Saving..." |
| Delete | "Deleting..." |
| Post comment | "Posting..." |
| Sign in | "Signing in..." |
| Loading data | "Loading..." |

### L-03: Skeleton vs Spinner

> **TODO:** Skeleton components KHÔNG tồn tại trong codebase. Không có file `*Skeleton*.vue` hay skeleton UI nào trong `src/components/`. Thực tế chỉ dùng loading text đơn giản (vd: `"Loading task details..."` trong TaskDetailsView). "Skeleton: cho page-level data fetch" là quy tắc được đề xuất, không phải mô tả hiện trạng.

- **Spinner / loading text**: cho action đơn lẻ (submit button, inline edit)
- **Skeleton**: cho page-level data fetch (danh sách project, task) ← TODO: chưa implement
- **"Loading task details..."** text: dùng cho TaskDetailsView khi `taskStore.taskLoading === true`

### L-04: Không block toàn màn hình trừ khi cần thiết

Tránh overlay loading toàn trang cho các action không quan trọng. Prefer inline loading indicator trong component liên quan.

### L-05: Optimistic UI (tùy chọn nhưng khuyến nghị)

Cho các action có tỷ lệ thành công cao (toggle subtask, mark notification read), có thể cập nhật UI ngay trước khi API trả về. Rollback nếu API lỗi.

```javascript
// Ví dụ: notificationStore.markRead
const n = this.notifications.find(n => n.userNotificationId === id)
if (n && !n.isRead) {
  n.isRead = true  // Optimistic
  this.unreadCount = Math.max(0, this.unreadCount - 1)
}
// API call sau đó
await notificationApi.markRead(id)
```

### L-06: Polling phải có cleanup

Notification unread count polling mỗi 60 giây phải được `clearInterval` trong `onBeforeUnmount`:

```javascript
// Đã implement đúng trong Navbar.vue
let _timer = null
onMounted(() => { _timer = setInterval(..., 60000) })
onBeforeUnmount(() => { clearInterval(_timer) })
```

---

## 7. Interaction Patterns

### Hover States

```
Card:      hover:border-gray-300 + transition (subtle)
Nav item:  hover:bg-gray-50 dark:hover:bg-zinc-800/60
Button:    hover:opacity-90 hoặc hover:bg-zinc-100
Ghost btn: opacity-0 group-hover:opacity-100 (edit/delete actions)
```

Hover actions trong list items (edit/delete buttons) phải dùng `group-hover` để chỉ hiện khi hover vào row.

### Focus States

Mọi interactive element phải có visible focus ring:
```css
focus:outline-none focus:ring-1 focus:ring-blue-500
```

### Active / Pressed States

```css
active:scale-95  /* Icon buttons */
active:cursor-grabbing  /* Kanban task card khi drag */
```

### Drag & Drop (Kanban)

1. Drag start: `opacity-50 ring-2 ring-blue-400` trên task đang kéo
2. Drag over column: `ring-2 ring-blue-400` trên column
3. Drop zone empty: `border-2 border-dashed`, chuyển sang `border-blue-400 bg-blue-50` khi có task bay vào
4. Sau khi drop: task trở về opacity bình thường, column ring mất
5. Khi API fail: hiển thị error message, **không giữ task ở vị trí mới** (cần revert)
