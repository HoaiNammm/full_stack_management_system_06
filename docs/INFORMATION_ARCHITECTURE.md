# INFORMATION_ARCHITECTURE.md — Kiến Trúc Thông Tin

> Sitemap, navigation, và phân cấp màn hình của ứng dụng.

---

## 1. Sitemap

> **TODO:** "ProjectFlow" là tên bịa. `package.json` chỉ có `"name": "project-management"`. Không có brand name trong source code.

```
ProjectFlow  <!-- TODO: tên bịa — xem ghi chú trên -->
│
├── [PUBLIC — không cần đăng nhập]
│   ├── /login           Login
│   └── /register        Register
│
└── [PRIVATE — yêu cầu JWT token]
    │
    ├── /                Dashboard
    │   ├── StatsGrid    (4 stat cards: Projects, Completed, My Tasks, Overdue)
    │   ├── ProjectOverview  (danh sách dự án tóm tắt)
    │   ├── RecentActivity   (activity log gần đây)
    │   └── TasksSummary     (task giao cho tôi)
    │
    ├── /projects         Danh sách dự án
    │   └── ProjectCard  (click → /projectsDetail)
    │
    ├── /projectsDetail?id={projectId}&tab={tab}  Chi tiết dự án
    │   ├── tab: tasks        ProjectTasks (danh sách)
    │   ├── tab: board        KanbanBoard (kéo thả)
    │   ├── tab: backlog      Backlog (task chưa có sprint)
    │   ├── tab: sprints      ProjectSprints
    │   ├── tab: milestones   ProjectMilestones
    │   ├── tab: calendar     ProjectCalendar
    │   ├── tab: analytics    ProjectAnalytics (charts)
    │   ├── tab: activity     ActivityLog
    │   ├── tab: ai-report    AIReportPanel [stub]
    │   └── tab: settings     ProjectSettings
    │
    ├── /taskDetails?projectId={pid}&taskId={tid}  Chi tiết task
    │   ├── Task info (view / inline edit)
    │   ├── Comments + @mention
    │   ├── Subtasks (checklist)
    │   └── Time Tracking
    │
    ├── /my-work          Công việc của tôi
    │   └── Tasks được giao (filter, sort)
    │
    ├── /team             Thành viên workspace
    │
    ├── /notifications    Thông báo
    │   └── Mark read / all read
    │
    ├── /profile          Hồ sơ cá nhân
    │   ├── Xem thông tin
    │   ├── Sửa tên / điện thoại / phòng ban
    │   ├── Đổi mật khẩu
    │   └── Upload avatar
    │
    └── /settings         Cài đặt ứng dụng
        ├── Theme (light/dark)
        └── Preferences
```

---

## 2. Navigation

### 2.1 Cấu Trúc Navigation

Ứng dụng có **2 tầng navigation**:

```
Tầng 1: Global Navigation (Sidebar)
  → Di chuyển giữa các section chính của app

Tầng 2: Contextual Navigation (Tab bar trong ProjectDetails)
  → Di chuyển giữa các view trong một dự án cụ thể
```

### 2.2 Sidebar Navigation (Global)

Vị trí: trái màn hình, `min-w-68` (272px), sticky.

```
┌─────────────────────────────┐
│  [WorkspaceDropdown]        │  ← Switch workspace
├─────────────────────────────┤
│  ⬡ Dashboard               │  /
│  💼 My Work                 │  /my-work
│  📁 Projects                │  /projects
│  👥 Team                    │  /team
│  🔔 Notifications     [3]   │  /notifications + unread badge
│  👤 Profile                 │  /profile
│  ⚙️  Settings               │  /settings
├─────────────────────────────┤
│  MY TASKS                   │  (MyTasksSidebar component)
│  > Task 1                   │
│  > Task 2                   │
├─────────────────────────────┤
│  PROJECTS                   │  (ProjectsSidebar component)
│  > Project Alpha            │
│  > Project Beta             │
└─────────────────────────────┘
```

**Behavior:**
- Active item: `bg-gray-100 dark:bg-gradient-to-br dark:from-zinc-800 dark:to-zinc-800/50`
- Notifications badge: hiển thị khi `unreadCount > 0`, màu `bg-blue-500`
- Mobile: sidebar ẩn mặc định, toggle bằng hamburger button ở Navbar

### 2.3 Navbar (Top Bar)

```
┌──────────────────────────────────────────────────────────────┐
│  [≡ hamburger]  [🔍 Search projects, tasks...]  [🌙] [🔔] [Avatar]  │
└──────────────────────────────────────────────────────────────┘
```

**Search:**
- Real-time filter trên `workspaceStore.projects` + `taskStore.allTasks`
- Kết quả dropdown phân theo 2 nhóm: Projects / Tasks
- Click kết quả → navigate trực tiếp

**Notifications bell:**
- Badge count đỏ khi có unread
- Click → navigate `/notifications`
- Polling mỗi 60 giây

**Avatar:**
- Click → dropdown: Profile, Sign Out
- Hiển thị chữ cái đầu tên nếu không có avatar

### 2.4 Tab Navigation (ProjectDetails)

Vị trí: trong `/projectsDetail`, dưới project header.

```
[ Tasks ] [ Board ] [ Backlog ] [ Sprints ] [ Milestones ]
[ Calendar ] [ Analytics ] [ Activity ] [ AI Report ] [ Settings ]
```

**10 tabs, thứ tự cố định:**

> **TODO:** Tên icon Lucide trong cột "Icon" chưa được xác minh trực tiếp từ source code ProjectDetailsView.vue. Tên tab và component là đúng (xác minh từ router và component imports), nhưng icon name là đoán theo ngữ nghĩa.

| # | Tab | Icon | Component |
|---|-----|------|-----------|
| 1 | Tasks | FileStack | ProjectTasks |
| 2 | Board | Kanban | KanbanBoard |
| 3 | Backlog | ListTodo | inline |
| 4 | Sprints | GitBranch | ProjectSprints |
| 5 | Milestones | Flag | ProjectMilestones |
| 6 | Calendar | Calendar | ProjectCalendar |
| 7 | Analytics | BarChart3 | ProjectAnalytics |
| 8 | Activity | Activity | ActivityLog |
| 9 | AI Report | Sparkles | AIReportPanel |
| 10 | Settings | Settings | ProjectSettings |

**URL persistence:** tab được lưu vào query string `?tab=board`, đảm bảo back/forward browser hoạt động đúng.

**Mobile:** tab chuyển sang `grid grid-cols-5`, icon + label nhỏ.

### 2.5 Breadcrumb / Back Navigation

Không có breadcrumb truyền thống. Thay vào đó dùng:

- **Arrow back button**: trong ProjectDetailsView (`ArrowLeft` → `/projects`) và TaskDetailsView (`Back` → `router.back()`)
- **Sidebar luôn hiển thị**: user biết mình đang ở section nào nhờ active state

---

## 3. Screen Hierarchy

### 3.1 Phân Cấp Màn Hình

```
Level 0: Authentication
  ├── /login
  └── /register

Level 1: Top-level sections (sidebar nav)
  ├── / (Dashboard)
  ├── /projects
  ├── /my-work
  ├── /team
  ├── /notifications
  ├── /profile
  └── /settings

Level 2: Detail views
  └── /projectsDetail?id=...  (từ /projects hoặc Dashboard)

Level 3: Deep detail
  └── /taskDetails?...  (từ /projectsDetail tabs: board, tasks, backlog)

Level 4: Modals / Dialogs (overlay trên level 2/3)
  ├── CreateProjectDialog
  ├── CreateTaskDialog
  └── (Inline edit trong TaskDetailsView)
```

### 3.2 Entry Points vào Màn Hình

#### Vào `/projectsDetail`

| Từ đâu | Cách |
|--------|------|
| `/projects` | Click `ProjectCard` |
| Dashboard `ProjectOverview` | Click project trong danh sách |
| Sidebar `ProjectsSidebar` | Click tên project |
| Navbar Search | Click kết quả project |
| Sau khi tạo project | Auto-redirect |

#### Vào `/taskDetails`

| Từ đâu | Cách |
|--------|------|
| KanbanBoard | Click task card |
| ProjectTasks (tab tasks) | Click task row |
| Backlog | Click task row |
| Navbar Search | Click kết quả task |
| My Work (`/my-work`) | Click task |

### 3.3 Modal Hierarchy

Modals được render ở top level với `fixed inset-0 z-50`. Không có modal-on-modal trong thiết kế hiện tại.

```
Page content (z-0)
  └── Modal overlay (z-50)
        └── Modal dialog
              └── Form content
```

Mentions dropdown trong TaskDetailsView dùng `z-20` (thấp hơn modal).

---

## 4. Data Flow theo Screen

### Dashboard (`/`)

```
onMounted (App.vue):
  authStore.fetchMe()
    └── watch user → workspaceStore.fetchWorkspaces()
              └── workspaceStore.fetchProjects()

Dashboard components pull từ store:
  StatsGrid       ← workspaceStore.projects + taskStore.allTasks
  ProjectOverview ← workspaceStore.projects
  TasksSummary    ← taskStore.myTasks (fetchMyTasks khi mount)
  RecentActivity  ← activityLogApi (fetch khi mount)
```

### ProjectDetails (`/projectsDetail?id=...`)

```
onMounted:
  workspaceStore.fetchProject(projectId)  → project + members
  taskStore.fetchTasks(projectId)          → tasks[]

Tab changes:
  'backlog'     → sprintApi.getAll()  (lazy)
  'sprints'     → ProjectSprints mount
  'milestones'  → ProjectMilestones mount
  'calendar'    → ProjectCalendar mount (tasks prop)
  'analytics'   → ProjectAnalytics mount (tasks prop)
  'activity'    → ActivityLog mount (fetch logs)
```

### TaskDetails (`/taskDetails`)

```
onMounted (parallel):
  taskStore.fetchTask(projectId, taskId)
  taskStore.fetchComments(taskId)
  taskStore.fetchSubtasks(taskId)
  taskStore.fetchTimelogs(taskId)

onUnmounted:
  taskStore.clearCurrentTask()
```

---

## 5. URL Strategy

### Pattern hiện tại

```
/projects                              Danh sách dự án
/projectsDetail?id={guid}             Chi tiết (tab mặc định: tasks)
/projectsDetail?id={guid}&tab=board   Chi tiết tab cụ thể
/taskDetails?projectId={guid}&taskId={guid}  Chi tiết task
```

**Lưu ý:** Route `projectsDetail` và `taskDetails` dùng query params thay vì path params (`/projects/:id`). Đây là pattern hiện tại trong codebase — cần nhất quán khi thêm link mới.

### Deep Linking

- Tab trong ProjectDetails được persist vào URL query (`?tab=board`) → share-able link
- Task details cần cả `projectId` VÀ `taskId` trong URL

---

## 6. Context Boundaries

### Workspace Context

Toàn bộ data phụ thuộc vào `currentWorkspaceId` trong `workspaceStore`. Khi switch workspace:
1. `workspaceStore.setCurrentWorkspace(newId)` được gọi
2. `projects` reset về `[]`
3. `fetchProjects(newId)` fetch lại
4. Components re-render với data mới

### Project Context

Trong `/projectsDetail`, project hiện tại được xác định bằng `route.query.id`. Không có global "current project" state — mỗi visit là fresh load.

### Task Context

`taskStore.currentTask` được set khi vào TaskDetailsView và cleared khi unmount (`taskStore.clearCurrentTask()`).

---

## 7. Navigation Anti-Patterns (cần tránh)

1. **Không hard-link sang `/projectsDetail/:id`** — phải dùng query param `?id=`
2. **Không navigate bằng `window.location.href`** — dùng `router.push()`
3. **Không giữ task/project state giữa các visits** — fetch lại khi mount
4. **Không tạo link sang tab mà không có `?tab=...`** — tab mặc định là `tasks`, nhưng nên explicit
