# DATABASE SCHEMA - HỆ THỐNG QUẢN LÝ DỰ ÁN & PHÂN CÔNG CÔNG VIỆC

## Mục lục
1. [Project Service (ProjectDB)](#1-project-service-projectdb)
2. [Task Service (TaskDB)](#2-task-service-taskdb)
3. [Notify Service (NotifyDB)](#3-notify-service-notifydb)
4. [Entity Relationships](#4-entity-relationships)
5. [Cross-service Events](#5-cross-service-events)
6. [Migration Strategy](#6-migration-strategy)

7. [Indexing Recommendations](#7-indexing-recommendations)

> **Quy ước:** Các cột ghi `*(logical ref)*` là tham chiếu logic giữa service — **không có FK thật**, ứng dụng tự đảm bảo tính nhất quán qua events.

---

## 1. PROJECT SERVICE (ProjectDB)

### 1.1 Projects

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh dự án |
| Name | NVARCHAR(255) | NOT NULL | Tên dự án |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| Status | INT | DEFAULT 0 | Draft=0, Active=1, Completed=2 |
| Color | NVARCHAR(20) | NULL | Màu hiển thị (hex, e.g. `#4F46E5`) |
| StartDate | DATETIME2 | NULL | Ngày bắt đầu dự án |
| EndDate | DATETIME2 | NULL | Ngày kết thúc dự án |
| CreatedBy | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId từ NotifyDB |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |
| DeletedAt | DATETIME2 | NULL | Soft delete — `NULL` = chưa xóa |

**Navigation Properties:**
- Members (ICollection\<Member\>)
- Sprints (ICollection\<Sprint\>)
- Milestones (ICollection\<Milestone\>)

---

### 1.2 Members

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh thành viên |
| ProjectId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Projects |
| UserId | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId từ NotifyDB |
| Role | INT | NOT NULL, DEFAULT 2 | Owner=0, Manager=1, Member=2, Viewer=3 |
| JoinedAt | DATETIME2 | NOT NULL | Thời gian tham gia |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật role |
| DeletedAt | DATETIME2 | NULL | Soft delete — `NULL` = còn trong project |

**Foreign Keys:**
- ProjectId → Projects(Id) ON DELETE CASCADE

**Navigation Properties:**
- Project (Project)

---

### 1.3 Sprints

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh sprint |
| ProjectId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Projects |
| Name | NVARCHAR(255) | NOT NULL | Tên sprint |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| Goal | NVARCHAR(MAX) | NULL | Mục tiêu sprint |
| StartDate | DATETIME2 | NOT NULL | Ngày bắt đầu |
| EndDate | DATETIME2 | NOT NULL | Ngày kết thúc (thường 2 tuần) |
| Status | INT | DEFAULT 0 | Draft=0, Active=1, Completed=2 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- ProjectId → Projects(Id) ON DELETE CASCADE

**Navigation Properties:**
- Project (Project)

**Events:**
- `project.member.added`
- `sprint.started`

---

### 1.4 Milestones

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh milestone |
| ProjectId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Projects |
| Name | NVARCHAR(255) | NOT NULL | Tên milestone |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| TargetDate | DATETIME2 | NOT NULL | Ngày mục tiêu |
| Status | INT | DEFAULT 0 | NotStarted=0, InProgress=1, Completed=2 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

> Milestone là mốc thời gian của Project, **không gắn trực tiếp với Task**.

**Foreign Keys:**
- ProjectId → Projects(Id) ON DELETE CASCADE

**Navigation Properties:**
- Project (Project)

---

## 2. TASK SERVICE (TaskDB)

> **Thiết kế Kanban:** Task không dùng `Status` enum. Vị trí của task trên board được xác định bằng `ColumnId` → `KanbanColumns`. KanbanColumn có `Type` để phân loại ngữ nghĩa.

---

### 2.1 KanbanBoards

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh board |
| ProjectId | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* Tham chiếu ProjectDB |
| Name | NVARCHAR(255) | NOT NULL | Tên board |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Navigation Properties:**
- Columns (ICollection\<KanbanColumn\>)

---

### 2.2 KanbanColumns

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh cột |
| BoardId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu KanbanBoards |
| Name | NVARCHAR(100) | NOT NULL | Tên cột hiển thị |
| Type | NVARCHAR(50) | NOT NULL, DEFAULT 'custom' | Loại cột: `backlog` / `active` / `done` / `custom` |
| Position | INT | NOT NULL | Vị trí cột (sắp xếp tăng dần) |

**Foreign Keys:**
- BoardId → KanbanBoards(Id) ON DELETE CASCADE

**Navigation Properties:**
- Board (KanbanBoard)
- Tasks (ICollection\<Task\>)

**Ghi chú `Type`:**
- `backlog` — task chưa lên kế hoạch
- `active` — task đang được làm (ToDo, InProgress, Review…)
- `done` — cột hoàn thành, dùng tính % progress của sprint/project
- `custom` — cột do team tự định nghĩa

> Khi tạo board mới, seed 5 columns mặc định: Backlog(`backlog`) → To Do(`active`) → In Progress(`active`) → Review(`active`) → Done(`done`).  
> Trước khi xóa column, ứng dụng **phải chuyển toàn bộ task** sang column khác (FK RESTRICT ngăn xóa nếu còn task).

---

### 2.3 Tasks

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh task |
| ProjectId | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* Tham chiếu ProjectDB |
| SprintId | UNIQUEIDENTIFIER | NULL | *(logical ref)* Tham chiếu ProjectDB (optional) |
| ColumnId | UNIQUEIDENTIFIER | FOREIGN KEY, NOT NULL | Tham chiếu KanbanColumns — vị trí trên board |
| Title | NVARCHAR(255) | NOT NULL | Tên task |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| AssignedTo | UNIQUEIDENTIFIER | NULL | *(logical ref)* UserId từ NotifyDB |
| Priority | INT | DEFAULT 1 | Low=0, Medium=1, High=2 |
| EstimatedHours | DECIMAL(10,2) | NULL | Giờ ước tính |
| Deadline | DATETIME2 | NULL | Hạn chót |
| CreatedBy | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId từ NotifyDB |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |
| DeletedAt | DATETIME2 | NULL | Soft delete — `NULL` = chưa xóa |

**Foreign Keys:**
- ColumnId → KanbanColumns(Id) ON DELETE RESTRICT

**Navigation Properties:**
- Column (KanbanColumn)
- SubTasks (ICollection\<SubTask\>)
- TimeLogs (ICollection\<TaskTimeLog\>)
- AssignmentHistory (ICollection\<TaskAssignmentHistory\>)

**Events:**
- `task.column.changed` — khi `ColumnId` thay đổi
- `task.assigned` — khi `AssignedTo` thay đổi

---

### 2.4 SubTasks

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh subtask |
| TaskId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Tasks |
| Title | NVARCHAR(255) | NOT NULL | Tên subtask |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| AssignedTo | UNIQUEIDENTIFIER | NULL | *(logical ref)* UserId từ NotifyDB |
| Status | INT | DEFAULT 0 | Backlog=0, ToDo=1, InProgress=2, Review=3, Done=4 |
| EstimatedHours | DECIMAL(10,2) | NULL | Giờ ước tính |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |
| DeletedAt | DATETIME2 | NULL | Soft delete — `NULL` = chưa xóa |

> SubTask dùng `Status` enum (không lên Kanban board), không cần ColumnId.

**Foreign Keys:**
- TaskId → Tasks(Id) ON DELETE CASCADE

**Navigation Properties:**
- Task (Task)

---

### 2.5 TaskTimeLogs

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh log |
| TaskId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Tasks |
| LoggedBy | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId từ NotifyDB |
| Hours | DECIMAL(10,2) | NOT NULL | Số giờ làm việc |
| Description | NVARCHAR(MAX) | NULL | Mô tả công việc |
| LoggedDate | DATETIME2 | NOT NULL | Ngày ghi nhận |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Foreign Keys:**
- TaskId → Tasks(Id) ON DELETE CASCADE

**Navigation Properties:**
- Task (Task)

---

### 2.6 TaskAssignmentHistory

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh record |
| TaskId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Tasks |
| PreviousAssignee | UNIQUEIDENTIFIER | NULL | *(logical ref)* UserId từ NotifyDB (người cũ) |
| NewAssignee | UNIQUEIDENTIFIER | NULL | *(logical ref)* UserId từ NotifyDB (người mới) |
| ChangedBy | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId từ NotifyDB |
| ChangedAt | DATETIME2 | NOT NULL | Thời gian thay đổi |

**Foreign Keys:**
- TaskId → Tasks(Id) ON DELETE CASCADE

**Navigation Properties:**
- Task (Task)

---

## 3. NOTIFY SERVICE (NotifyDB)

### 3.1 Users

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh người dùng |
| Email | NVARCHAR(255) | UNIQUE, NOT NULL | Email đăng nhập |
| PasswordHash | NVARCHAR(MAX) | NOT NULL | Hash mật khẩu (BCrypt) |
| FullName | NVARCHAR(255) | NULL | Tên đầy đủ |
| Avatar | NVARCHAR(500) | NULL | URL ảnh đại diện |
| Status | INT | DEFAULT 1 | Active=1, Inactive=0 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| LastLogin | DATETIME2 | NULL | Lần đăng nhập cuối |

**Navigation Properties:**
- Notifications (ICollection\<Notification\>)
- Comments (ICollection\<Comment\>)
- ActivityLogs (ICollection\<ActivityLog\>)
- UserPreference (UserPreference)

---

### 3.2 Comments

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh comment |
| TaskId | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* Tham chiếu TaskDB |
| AuthorId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Users |
| Content | NVARCHAR(MAX) | NOT NULL | Nội dung comment |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- AuthorId → Users(Id)

**Navigation Properties:**
- Author (User)
- Mentions (ICollection\<CommentMention\>)

**Events:**
- `comment.created`
- `user.mentioned` — nếu `Mentions` không rỗng

---

### 3.3 CommentMentions *(thay thế cột `MentionedUsers JSON`)*

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh mention |
| CommentId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Comments |
| UserId | UNIQUEIDENTIFIER | NOT NULL | *(logical ref)* UserId được mention từ NotifyDB |

**Foreign Keys:**
- CommentId → Comments(Id) ON DELETE CASCADE

**Navigation Properties:**
- Comment (Comment)

---

### 3.4 Notifications

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh thông báo |
| UserId | UNIQUEIDENTIFIER | FOREIGN KEY | Người nhận |
| Title | NVARCHAR(255) | NOT NULL | Tiêu đề |
| Content | NVARCHAR(MAX) | NOT NULL | Nội dung |
| Type | NVARCHAR(50) | NOT NULL | `task_assigned` / `task_column_changed` / `comment_mention` / `member_added` / `sprint_started` |
| RelatedTaskId | UNIQUEIDENTIFIER | NULL | *(logical ref)* Tham chiếu TaskDB |
| RelatedProjectId | UNIQUEIDENTIFIER | NULL | *(logical ref)* Tham chiếu ProjectDB |
| IsRead | BIT | DEFAULT 0 | Đã đọc chưa |
| ReadAt | DATETIME2 | NULL | Thời gian đọc |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Foreign Keys:**
- UserId → Users(Id) ON DELETE CASCADE

**Navigation Properties:**
- User (User)

---

### 3.5 UserPreferences

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh cấu hình |
| UserId | UNIQUEIDENTIFIER | UNIQUE, FOREIGN KEY | Tham chiếu Users |
| EnableTaskNotification | BIT | DEFAULT 1 | Bật thông báo task |
| EnableCommentNotification | BIT | DEFAULT 1 | Bật thông báo comment |
| EnableMentionNotification | BIT | DEFAULT 1 | Bật thông báo mention |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- UserId → Users(Id) ON DELETE CASCADE

**Navigation Properties:**
- User (User)

---

### 3.6 ActivityLogs

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh log |
| UserId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Users |
| Action | NVARCHAR(255) | NOT NULL | `created_task` / `updated_task` / `moved_task` / `commented` / `assigned_task` |
| ResourceType | NVARCHAR(50) | NOT NULL | `Task` / `Project` / `Comment` |
| ResourceId | UNIQUEIDENTIFIER | NOT NULL | Định danh resource |
| Timestamp | DATETIME2 | NOT NULL | Thời gian hành động |

**Foreign Keys:**
- UserId → Users(Id)

**Navigation Properties:**
- User (User)

---

## 4. ENTITY RELATIONSHIPS

### Intra-service FKs (FK thật trong cùng DB)

**ProjectDB:**
```
Members.ProjectId    → Projects(Id)  CASCADE
Sprints.ProjectId    → Projects(Id)  CASCADE
Milestones.ProjectId → Projects(Id)  CASCADE
```

**TaskDB:**
```
KanbanColumns.BoardId        → KanbanBoards(Id)   CASCADE
Tasks.ColumnId               → KanbanColumns(Id)  RESTRICT
SubTasks.TaskId              → Tasks(Id)           CASCADE
TaskTimeLogs.TaskId          → Tasks(Id)           CASCADE
TaskAssignmentHistory.TaskId → Tasks(Id)           CASCADE
```

**NotifyDB:**
```
Comments.AuthorId          → Users(Id)
CommentMentions.CommentId  → Comments(Id)  CASCADE
Notifications.UserId       → Users(Id)     CASCADE
UserPreferences.UserId     → Users(Id)     CASCADE
ActivityLogs.UserId        → Users(Id)
```

---

### Cross-service logical references (không có FK thật)

```
ProjectDB.Projects.CreatedBy  ──(logical)──► NotifyDB.Users.Id
ProjectDB.Members.UserId      ──(logical)──► NotifyDB.Users.Id
```

```
TaskDB.Tasks.ProjectId                      ──(logical)──► ProjectDB.Projects.Id
TaskDB.Tasks.SprintId                       ──(logical)──► ProjectDB.Sprints.Id
TaskDB.Tasks.CreatedBy                      ──(logical)──► NotifyDB.Users.Id
TaskDB.Tasks.AssignedTo                     ──(logical)──► NotifyDB.Users.Id
TaskDB.SubTasks.AssignedTo                  ──(logical)──► NotifyDB.Users.Id
TaskDB.TaskTimeLogs.LoggedBy                ──(logical)──► NotifyDB.Users.Id
TaskDB.TaskAssignmentHistory.PreviousAssignee ─(logical)──► NotifyDB.Users.Id
TaskDB.TaskAssignmentHistory.NewAssignee      ─(logical)──► NotifyDB.Users.Id
TaskDB.TaskAssignmentHistory.ChangedBy        ─(logical)──► NotifyDB.Users.Id
```

```
NotifyDB.Comments.TaskId              ──(logical)──► TaskDB.Tasks.Id
NotifyDB.CommentMentions.UserId       ──(logical)──► NotifyDB.Users.Id
NotifyDB.Notifications.RelatedTaskId  ──(logical)──► TaskDB.Tasks.Id
NotifyDB.Notifications.RelatedProjectId ─(logical)─► ProjectDB.Projects.Id
```

---

## 5. CROSS-SERVICE EVENTS

### Project Service Events
- `project.member.added` → Gửi thông báo tới Member mới
- `sprint.started` → Gửi thông báo tới toàn bộ Project Members

### Task Service Events
- `task.column.changed` → Gửi thông báo tới Assignee khi task bị chuyển column
- `task.assigned` → Gửi thông báo tới Assignee mới

### Notify Service Events
- `comment.created` → Gửi thông báo tới Task Assignee
- `user.mentioned` → Gửi thông báo tới từng UserId trong CommentMentions

---

## 6. MIGRATION STRATEGY

### Mỗi Service có migration riêng:
```bash
# ProjectService
dotnet ef migrations add Initial -o Data/Migrations
dotnet ef database update

# TaskService
dotnet ef migrations add Initial -o Data/Migrations
dotnet ef database update

# NotifyService
dotnet ef migrations add Initial -o Data/Migrations
dotnet ef database update
```

---

## 7. INDEXING RECOMMENDATIONS

### ProjectDB
```sql
CREATE INDEX idx_Projects_CreatedBy ON Projects(CreatedBy);
CREATE INDEX idx_Projects_Active ON Projects(DeletedAt) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Members_ProjectId_UserId ON Members(ProjectId, UserId);
CREATE INDEX idx_Members_UserId ON Members(UserId) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Sprints_ProjectId ON Sprints(ProjectId);
CREATE INDEX idx_Milestones_ProjectId ON Milestones(ProjectId);
```

### TaskDB
```sql
-- Board & Column
CREATE INDEX idx_KanbanBoards_ProjectId ON KanbanBoards(ProjectId);
CREATE INDEX idx_KanbanColumns_BoardId ON KanbanColumns(BoardId);

-- Task — queries phổ biến nhất
CREATE INDEX idx_Tasks_ProjectId ON Tasks(ProjectId) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Tasks_ColumnId ON Tasks(ColumnId) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Tasks_ProjectId_ColumnId ON Tasks(ProjectId, ColumnId) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Tasks_AssignedTo ON Tasks(AssignedTo) WHERE DeletedAt IS NULL;
CREATE INDEX idx_Tasks_SprintId ON Tasks(SprintId) WHERE SprintId IS NOT NULL;
CREATE INDEX idx_Tasks_Deadline ON Tasks(Deadline) WHERE Deadline IS NOT NULL AND DeletedAt IS NULL;

-- SubTask, TimeLog
CREATE INDEX idx_SubTasks_TaskId ON SubTasks(TaskId) WHERE DeletedAt IS NULL;
CREATE INDEX idx_TaskTimeLogs_TaskId ON TaskTimeLogs(TaskId);
CREATE INDEX idx_TaskAssignmentHistory_TaskId ON TaskAssignmentHistory(TaskId);
```

### NotifyDB
```sql
-- Notifications
CREATE INDEX idx_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
CREATE INDEX idx_Notifications_UserId_CreatedAt ON Notifications(UserId, CreatedAt DESC);

-- Comments & Mentions
CREATE INDEX idx_Comments_TaskId ON Comments(TaskId);
CREATE INDEX idx_Comments_TaskId_CreatedAt ON Comments(TaskId, CreatedAt);
CREATE INDEX idx_CommentMentions_CommentId ON CommentMentions(CommentId);
CREATE INDEX idx_CommentMentions_UserId ON CommentMentions(UserId);

-- Activity
CREATE INDEX idx_ActivityLogs_UserId ON ActivityLogs(UserId);
CREATE INDEX idx_ActivityLogs_ResourceType_ResourceId ON ActivityLogs(ResourceType, ResourceId);

-- Preferences
CREATE INDEX idx_UserPreferences_UserId ON UserPreferences(UserId);
```

---

**Cập nhật lần cuối: 2026-06-08**
