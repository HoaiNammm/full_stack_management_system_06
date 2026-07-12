# PRODUCT.md — Tài Liệu Sản Phẩm

> Phiên bản: 1.0 | Cập nhật: 2026-06-26

---

## 1. Mô Tả Sản Phẩm

> **TODO:** Tên sản phẩm "ProjectFlow" là tên bịa. `package.json` chỉ có `"name": "project-management"`. Không có brand name nào trong toàn bộ source code. Cần xác định tên chính thức trước khi dùng tài liệu này.

**ProjectFlow** là nền tảng quản lý dự án SaaS dành cho nhóm phát triển phần mềm theo phương pháp Agile/Scrum. Sản phẩm cho phép nhóm tổ chức công việc theo sprint, theo dõi tiến độ trực quan qua bảng Kanban, quản lý cột mốc (milestone), phân công thành viên theo vai trò và trao đổi nội bộ qua bình luận.

### Định vị sản phẩm

> **TODO:** Bảng so sánh với Jira/Trello là hoàn toàn bịa — không có tài liệu nào trong source code đề cập đến định vị sản phẩm so với đối thủ.

| | ProjectFlow | Jira | Trello |
|---|---|---|---|
| Đối tượng | Nhóm nhỏ–vừa (<50 người) | Doanh nghiệp lớn | Cá nhân / nhóm nhỏ |
| Phương pháp | Agile/Scrum | Agile/Kanban/Custom | Kanban thuần |
| Phức tạp | Trung bình | Cao | Thấp |
| Chi phí | Thấp (self-host) | Cao | Miễn phí / trả phí |

### Mục tiêu cốt lõi

1. Giúp team nhìn thấy toàn bộ tiến độ dự án trên một màn hình
2. Giảm thời gian họp standup bằng cách thay thế bằng Kanban board và activity log
3. Đảm bảo trách nhiệm rõ ràng qua phân quyền và giao task cụ thể

---

## 2. Personas

> **TODO:** Toàn bộ section Personas là nội dung bịa. Các nhân vật (Nguyễn Minh Tuấn, Trần Thị Mai, Lê Văn Hùng), tuổi, kinh nghiệm, nỗi đau — không có trong source code hay bất kỳ tài liệu nào của dự án. Cần khảo sát người dùng thực tế để viết personas chính xác.

### Persona 1 — Quản lý dự án (Project Manager)

```
Tên:         Nguyễn Minh Tuấn
Tuổi:        32
Vai trò:     Tech Lead / Scrum Master
Kinh nghiệm: 7 năm phát triển phần mềm, 3 năm quản lý team

Mục tiêu:
  - Nắm được tiến độ tổng thể mà không cần hỏi từng người
  - Lập kế hoạch sprint và phân công hợp lý
  - Phát hiện sớm task bị block hoặc trễ hạn

Nỗi đau:
  - Phải tổng hợp thông tin từ nhiều kênh (chat, email, meeting)
  - Khó biết ai đang làm gì và tiến độ đến đâu
  - Meetings quá nhiều, ít thời gian code thực sự

Cách dùng sản phẩm:
  - Mở Dashboard mỗi sáng để xem overview
  - Xem Kanban board trong daily standup
  - Tạo sprint mới và giao task cho team
  - Theo dõi milestone để báo cáo stakeholder
```

### Persona 2 — Lập trình viên (Developer)

```
Tên:         Trần Thị Mai
Tuổi:        26
Vai trò:     Backend Developer
Kinh nghiệm: 3 năm, quen làm việc theo ticket

Mục tiêu:
  - Biết hôm nay cần làm gì (My Work)
  - Cập nhật trạng thái task nhanh không cần nhiều click
  - Hỏi/trả lời câu hỏi kỹ thuật ngay trong task

Nỗi đau:
  - Hay bị ngắt quãng vì bị hỏi qua Slack/Zalo
  - Quên deadline vì task nằm rải rác ở nhiều nơi
  - Phải context-switch giữa công cụ trao đổi và quản lý công việc

Cách dùng sản phẩm:
  - Xem "My Work" để biết task được giao
  - Kéo task trên Kanban khi bắt đầu / hoàn thành
  - Bình luận trong task để trao đổi với PM/QA
  - Ghi time log sau khi xong việc
```

### Persona 3 — Người xem / Stakeholder (Viewer)

```
Tên:         Lê Văn Hùng
Tuổi:        42
Vai trò:     Product Owner / Khách hàng
Kinh nghiệm: Ít kinh nghiệm kỹ thuật, chủ yếu đọc báo cáo

Mục tiêu:
  - Theo dõi tiến độ dự án mà không phải hỏi team
  - Xem milestone và ngày dự kiến hoàn thành
  - Hiểu tổng quan mà không bị ngập trong chi tiết

Nỗi đau:
  - Phải chờ PM gửi báo cáo định kỳ
  - Không biết thực sự đang ở giai đoạn nào

Cách dùng sản phẩm:
  - Xem tab Analytics và Timeline
  - Xem danh sách milestone
  - Xem Activity log để biết hoạt động gần đây
```

---

## 3. User Journey

### Journey 1 — Tạo dự án mới và bắt đầu làm việc

```
[PM đăng nhập lần đầu]
        │
        ▼
Dashboard (trống) → nhấn "New Project"
        │
        ▼
Chọn template (Software Dev / Research / Event / Blank)
        │
        ▼
Điền thông tin: Tên, Mô tả, Ngày bắt đầu, Màu sắc
        │
        ▼
Dự án được tạo → tự động có Sprint + Milestone + Kanban columns
        │
        ▼
Mời thành viên vào dự án (Team tab)
        │
        ▼
Tạo các task trong sprint đầu tiên
        │
        ▼
Kích hoạt Sprint 1 → team bắt đầu làm việc
        │
        ▼
Team kéo task trên Kanban theo tiến độ thực tế
        │
        ▼
PM xem Analytics / Activity log để theo dõi
        │
        ▼
Hoàn thành sprint → chuyển task sang sprint tiếp
```

### Journey 2 — Lập trình viên xử lý task hàng ngày

```
[Dev mở app buổi sáng]
        │
        ▼
Xem "My Work" → thấy danh sách task được giao
        │
        ▼
Click vào task → xem chi tiết, mô tả, hạn chót
        │
        ▼
Kéo task từ "To Do" sang "In Progress" trên Kanban
        │
        ▼
Viết bình luận để hỏi PM (@mention)
        │
        ▼
PM nhận thông báo → trả lời trong comment
        │
        ▼
Dev hoàn thành → kéo task sang "Review"
        │
        ▼
Ghi time log (số giờ đã làm)
        │
        ▼
QA review → kéo sang "Done"
```

### Journey 3 — Theo dõi milestone và báo cáo

```
[PM chuẩn bị báo cáo cuối tháng]
        │
        ▼
Mở ProjectDetailsView → tab "Milestones"
        │
        ▼
Xem các cột mốc: Beta Release, Production Release  <!-- TODO: tên milestone là ví dụ bịa, không có trong source code -->
        │
        ▼
Tab "Analytics" → xem biểu đồ tiến độ, số task theo status
        │
        ▼
Tab "Activity" → xem nhật ký hoạt động gần đây
        │
        ▼
Xuất / chụp màn hình báo cáo cho stakeholder  <!-- TODO: không có tính năng xuất báo cáo trong source code. aiReportApi và aiGenerateApi đều là stubs luôn reject -->
```

---

## 4. Các Chức Năng

### 4.1 Quản lý Workspace

| Mã | Chức năng | Mô tả |
|----|-----------|-------|
| WS-01 | Tạo workspace | Tạo không gian làm việc với tên và mô tả |
| WS-02 | Đổi workspace | Chuyển giữa các workspace qua dropdown |
| WS-03 | Mời thành viên workspace | Thêm user vào workspace |
| WS-04 | Quản lý vai trò workspace | Owner / Manager / Member | <!-- TODO: tên và số lượng vai trò workspace chưa được xác minh từ C# enum trong ProjectService source code --> |

### 4.2 Quản lý Dự Án

| Mã | Chức năng | Mô tả | Vai trò |
|----|-----------|-------|---------|
| P-01 | Tạo dự án | Từ template có sẵn, tự động seed sprint/milestone/cột | Tất cả |
| P-02 | Xem danh sách | Dự án của tôi trong workspace | Thành viên |
| P-03 | Xem chi tiết | 10 tab trong ProjectDetailsView | Thành viên |
| P-04 | Sửa dự án | Tên, mô tả, màu, trạng thái, ngày | Owner/Manager |
| P-05 | Xóa dự án | Xóa vĩnh viễn | Owner |

### 4.3 Quản lý Task

| Mã | Chức năng | Mô tả | Vai trò |
|----|-----------|-------|---------|
| T-01 | Tạo task | Điền tiêu đề, mô tả, priority, assignee, due date, sprint, labels | Dev+ |
| T-02 | Xem Kanban | 5 cột: Backlog / To Do / In Progress / Review / Done | Tất cả |
| T-03 | Kéo thả Kanban | Di chuyển task giữa các cột bằng drag & drop | Dev+ |
| T-04 | Xem chi tiết task | Thông tin đầy đủ + comments + subtasks + timelogs | Tất cả |
| T-05 | Sửa task | Inline edit trong TaskDetailsView | Dev+ |
| T-06 | Xóa task | Xóa mềm | Dev+ |
| T-07 | Xem Backlog | Task chưa có sprint | Tất cả |
| T-08 | Gắn task vào sprint | Assign task vào sprint cụ thể | Dev+ |

### 4.4 Quản lý Subtask

| Mã | Chức năng |
|----|-----------|
| ST-01 | Thêm subtask vào task |
| ST-02 | Toggle hoàn thành subtask |
| ST-03 | Xóa subtask |

### 4.5 Sprint & Milestone

| Mã | Chức năng | Vai trò |
|----|-----------|---------|
| S-01 | Tạo sprint (tên, mục tiêu, ngày bắt đầu) | Owner/Manager |
| S-02 | Kích hoạt sprint (Draft → Active) | Owner/Manager |
| S-03 | Hoàn thành sprint (Active → Completed) | Owner/Manager |
| S-04 | Xóa sprint (chỉ khi Draft) | Owner/Manager |
| M-01 | Tạo / sửa / xóa milestone | Owner/Manager |
| M-02 | Xem timeline milestone | Tất cả |

### 4.6 Cộng Tác

| Mã | Chức năng |
|----|-----------|
| C-01 | Viết bình luận trong task |
| C-02 | @mention thành viên trong bình luận |
| C-03 | Sửa / xóa bình luận của chính mình |
| C-04 | Xem activity log dự án |

### 4.7 Time Tracking

| Mã | Chức năng |
|----|-----------|
| TL-01 | Ghi nhận giờ làm việc (hours + date + note) |
| TL-02 | Xem tổng giờ đã log cho task |

### 4.8 Thông Báo

| Mã | Chức năng |
|----|-----------|
| N-01 | Nhận thông báo khi được @mention hoặc giao task |
| N-02 | Xem số thông báo chưa đọc (badge trên chuông) |
| N-03 | Đánh dấu đã đọc từng thông báo hoặc tất cả |

### 4.9 Tài Khoản

| Mã | Chức năng |
|----|-----------|
| A-01 | Đăng ký / đăng nhập bằng email + mật khẩu |
| A-02 | Xem và cập nhật hồ sơ cá nhân |
| A-03 | Đổi mật khẩu |
| A-04 | Upload ảnh đại diện |

---

## 5. Workflow

### 5.1 Workflow Tạo Dự Án

```
User nhấn "New Project"
    │
    ▼
[Bước 1] Chọn template
> **TODO:** Số lượng sprint/milestone/cột Kanban theo từng template là số liệu từ TAILIEU_NGHIEPVU.md (tài liệu nghiệp vụ), chưa được xác minh từ source code của CreateProjectDialog.vue. Kanban columns là static frontend (không từ backend) và kanbanApi là stub hoàn toàn.
    ├── software-dev  → 3 sprints + 3 milestones + 5 cột Kanban
    ├── research      → 2 sprints + 2 milestones + 4 cột Kanban
    ├── event-mgmt    → 2 sprints + 2 milestones + 4 cột Kanban
    └── blank         → 0 sprints + 0 milestones + 4 cột cơ bản
    │
    ▼
[Bước 2] Điền chi tiết
    - Tên dự án (bắt buộc)
    - Mô tả (tùy chọn)
    - Ngày bắt đầu (bắt buộc, ≥ hôm nay)
    - Ngày kết thúc (tùy chọn, > ngày bắt đầu)
    - Màu sắc nhận diện
    │
    ▼
POST /api/workspaces/{wid}/projects
    → Server tạo Project + Sprint + Milestone
    → TaskService nhận event → tạo KanbanColumns  <!-- TODO: KanbanColumns KHÔNG được tạo từ backend. kanbanApi là stub hoàn toàn. KanbanBoard dùng 5 cột static hardcode ở frontend -->
    │
    ▼
Chuyển đến /projectsDetail?id=...&tab=tasks
```

### 5.2 Workflow Kanban

```
[Tab: Board trong ProjectDetailsView]
    │
    ├── Load columns (static: Backlog, ToDo, InProgress, Review, Done)
    └── GET /api/projects/{pid}/tasks
    │
    ▼
Hiển thị bảng Kanban:
  ┌─────────┐ ┌──────┐ ┌───────────┐ ┌────────┐ ┌──────┐
  │ Backlog │ │ ToDo │ │InProgress │ │ Review │ │ Done │
  └─────────┘ └──────┘ └───────────┘ └────────┘ └──────┘
    │
    ├── Nhấn "New Task" → CreateTaskDialog
    │       └── POST /api/projects/{pid}/tasks
    │
    ├── Kéo task sang cột khác
    │       └── PUT /api/projects/{pid}/tasks/{id} { status: "InProgress" }
    │
    └── Click vào task card → navigate /taskDetails?...
```

### 5.3 Workflow Sprint

```
Tạo sprint mới (tab Sprints)
    └── POST /api/workspaces/{wid}/projects/{pid}/sprints
         { name, goal, startDate }
         → endDate tự động = startDate + 14 ngày
         → Status = Draft

Gắn task vào sprint
    └── PUT task với { sprintId: "..." }
    └── Hoặc dùng dropdown "Move to Sprint..." trong tab Backlog

Kích hoạt sprint
    └── POST /api/.../sprints/{sid}/start
         → Validate: không có sprint nào khác đang Active
         → Status = Active

Hoàn thành sprint
    └── POST /api/.../sprints/{sid}/complete
         → Status = Completed
```

### 5.4 Workflow Bình Luận & Thông Báo

```
User A viết comment trong TaskDetailsView
    │  Gõ "@" → hiện dropdown gợi ý thành viên dự án
    │
    ▼
POST /api/tasks/{taskId}/comments
    { content: "...", mentionedUserIds: ["user-b-id"] }
    │
    ▼
TaskService lưu comment
    └── (Tùy chọn) Publish event → NotifyService tạo notification
    │
    ▼
User B mở app → icon chuông có badge số chưa đọc
    └── GET /api/notifications/unread-count (polling mỗi 60 giây)
    │
    ▼
User B nhấn chuông → /notifications
    └── GET /api/notifications
    └── POST /api/notifications/{id}/read
```

### 5.5 Workflow Phân Quyền

> **TODO:** Toàn bộ phân quyền vai trò (Owner/Manager/Developer/Viewer với số thứ tự 0-3) là từ TAILIEU_NGHIEPVU.md (tài liệu nghiệp vụ), chưa xác minh từ C# enum trong ProjectService. Quy tắc "Chỉ Owner/Manager xóa cột Kanban" là sai hoàn toàn vì Kanban columns là static frontend, không có API quản lý cột.

```
Vai trò trong dự án (theo workspace member):
  Owner (0)    → Toàn quyền: sửa/xóa dự án, quản lý thành viên, sprint, milestone, task
  Manager (1)  → Quản lý sprint, milestone, task; thêm Developer/Viewer
  Developer (2)→ Tạo/sửa/xóa task, subtask, timelog; bình luận
  Viewer (3)   → Chỉ xem: không tạo/sửa/xóa bất kỳ thứ gì

Quy tắc đặc biệt:
  - Chỉ 1 sprint Active tại một thời điểm  ← đã xác nhận từ sprint.start endpoint
  - Không xóa Owner cuối cùng của dự án     ← TODO: chưa xác minh từ source code
  - Chỉ Owner/Manager xóa cột Kanban (và cột phải trống)  ← TODO: KHÔNG ĐÚNG, không có API quản lý cột Kanban
```
