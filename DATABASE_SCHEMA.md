# DATABASE SCHEMA - HỆ THỐNG QUẢN LÝ DỰ ÁN & PHÂN CÔNG CÔNG VIỆC

## Mục lục
1. [Project Service (ProjectDB)](#1-project-service-projectdb)
2. [Task Service (TaskDB)](#2-task-service-taskdb)
3. [Notify Service (NotifyDB)](#3-notify-service-notifydb)
4. [Entity Relationships](#4-entity-relationships)

---

## 1. PROJECT SERVICE (ProjectDB)

### 1.1 Projects

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh dự án |
| Name | NVARCHAR(255) | NOT NULL | Tên dự án |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| Status | INT | DEFAULT 0 | Draft=0, Active=1, Completed=2 |
| CreatedBy | UNIQUEIDENTIFIER | NOT NULL | UserId từ NotifyDB |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Navigation Properties:**
- Members (ICollection<Member>)
- Sprints (ICollection<Sprint>)
- Milestones (ICollection<Milestone>)

---

### 1.2 Members

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh thành viên |
| ProjectId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Projects |
| UserId | UNIQUEIDENTIFIER | NOT NULL | UserId từ NotifyDB |
| Role | INT | NOT NULL | Owner=0, Manager=1, Member=2, Viewer=3 |
| JoinedAt | DATETIME2 | NOT NULL | Thời gian tham gia |

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
| StartDate | DATETIME2 | NOT NULL | Ngày bắt đầu |
| EndDate | DATETIME2 | NOT NULL | Ngày kết thúc (2 tuần) |
| Status | INT | DEFAULT 0 | Draft=0, Active=1, Completed=2 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- ProjectId → Projects(Id) ON DELETE CASCADE

**Navigation Properties:**
- Project (Project)

**Events:**
- project.member.added
- sprint.started

---

### 1.4 Milestones

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh milestone |
| ProjectId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Projects |
| Name | NVARCHAR(255) | NOT NULL | Tên milestone |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| TargetDate | DATETIME2 | NOT NULL | Ngày đạt được |
| Status | INT | DEFAULT 0 | Not Started=0, In Progress=1, Completed=2 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Foreign Keys:**
- ProjectId → Projects(Id) ON DELETE CASCADE

**Navigation Properties:**
- Project (Project)

---

## 2. TASK SERVICE (TaskDB)

### 2.1 Tasks

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh task |
| ProjectId | UNIQUEIDENTIFIER | NOT NULL | Tham chiếu ProjectDB |
| SprintId | UNIQUEIDENTIFIER | NULL | Tham chiếu ProjectDB (optional) |
| Title | NVARCHAR(255) | NOT NULL | Tên task |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| AssignedTo | UNIQUEIDENTIFIER | NULL | UserId từ NotifyDB |
| Status | INT | DEFAULT 0 | Backlog=0, ToDo=1, InProgress=2, Review=3, Done=4 |
| Priority | INT | DEFAULT 1 | Low=0, Medium=1, High=2 |
| EstimatedHours | DECIMAL(10,2) | NULL | Giờ ước tính |
| Deadline | DATETIME2 | NULL | Hạn chót |
| CreatedBy | UNIQUEIDENTIFIER | NOT NULL | UserId từ NotifyDB |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Navigation Properties:**
- SubTasks (ICollection<SubTask>)
- TimeLogs (ICollection<TaskTimeLog>)
- AssignmentHistory (ICollection<TaskAssignmentHistory>)

**Events:**
- task.status.changed
- task.assigned

---

### 2.2 SubTasks

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh subtask |
| TaskId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Tasks |
| Title | NVARCHAR(255) | NOT NULL | Tên subtask |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| AssignedTo | UNIQUEIDENTIFIER | NULL | UserId từ NotifyDB |
| Status | INT | DEFAULT 0 | Backlog=0, ToDo=1, InProgress=2, Review=3, Done=4 |
| EstimatedHours | DECIMAL(10,2) | NULL | Giờ ước tính |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- TaskId → Tasks(Id) ON DELETE CASCADE

**Navigation Properties:**
- Task (Task)

---

### 2.3 KanbanBoard

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh board |
| ProjectId | UNIQUEIDENTIFIER | NOT NULL | Tham chiếu ProjectDB |
| Name | NVARCHAR(255) | NOT NULL | Tên board |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Navigation Properties:**
- Columns (ICollection<KanbanColumn>)

**Columns:** Backlog → To Do → In Progress → Review → Done

---

### 2.4 KanbanColumns

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh cột |
| BoardId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu KanbanBoard |
| Name | NVARCHAR(100) | NOT NULL | Backlog, To Do, In Progress, Review, Done |
| Position | INT | NOT NULL | Vị trí cột (sắp xếp) |

**Foreign Keys:**
- BoardId → KanbanBoard(Id) ON DELETE CASCADE

**Navigation Properties:**
- Board (KanbanBoard)

---

### 2.5 TaskTimeLog

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh log |
| TaskId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Tasks |
| LoggedBy | UNIQUEIDENTIFIER | NOT NULL | UserId từ NotifyDB |
| Hours | DECIMAL(10,2) | NOT NULL | Số giờ làm việc |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
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
| PreviousAssignee | UNIQUEIDENTIFIER | NULL | UserId từ NotifyDB (người cũ) |
| NewAssignee | UNIQUEIDENTIFIER | NULL | UserId từ NotifyDB (người mới) |
| ChangedBy | UNIQUEIDENTIFIER | NOT NULL | Người thay đổi |
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
| PasswordHash | NVARCHAR(MAX) | NOT NULL | Hash mật khẩu |
| FullName | NVARCHAR(255) | NULL | Tên đầy đủ |
| Avatar | NVARCHAR(500) | NULL | URL ảnh đại diện |
| Status | INT | DEFAULT 1 | Active=1, Inactive=0 |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| LastLogin | DATETIME2 | NULL | Lần đăng nhập cuối |

**Navigation Properties:**
- Notifications (ICollection<Notification>)
- Comments (ICollection<Comment>)
- ActivityLogs (ICollection<ActivityLog>)
- UserPreference (UserPreference)

---

### 3.2 Comments

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh comment |
| TaskId | UNIQUEIDENTIFIER | NOT NULL | Tham chiếu TaskDB |
| AuthorId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Users |
| Content | NVARCHAR(MAX) | NOT NULL | Nội dung comment |
| MentionedUsers | NVARCHAR(MAX) | NULL | JSON: ["userId1", "userId2"] |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |
| UpdatedAt | DATETIME2 | NULL | Thời gian cập nhật |

**Foreign Keys:**
- AuthorId → Users(Id)

**Navigation Properties:**
- Author (User)

**Events:**
- comment.created
- user.mentioned (nếu MentionedUsers không rỗng)

---

### 3.3 Notifications

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh thông báo |
| UserId | UNIQUEIDENTIFIER | FOREIGN KEY | Người nhận (UserId) |
| Title | NVARCHAR(255) | NOT NULL | Tiêu đề |
| Content | NVARCHAR(MAX) | NOT NULL | Nội dung |
| Type | NVARCHAR(50) | NOT NULL | task_assigned, comment_mention, task_status_changed, member_added, sprint_started |
| RelatedTaskId | UNIQUEIDENTIFIER | NULL | Tham chiếu TaskDB (nếu có) |
| RelatedProjectId | UNIQUEIDENTIFIER | NULL | Tham chiếu ProjectDB (nếu có) |
| IsRead | BIT | DEFAULT 0 | Đã đọc chưa |
| ReadAt | DATETIME2 | NULL | Thời gian đọc |
| CreatedAt | DATETIME2 | NOT NULL | Thời gian tạo |

**Foreign Keys:**
- UserId → Users(Id) ON DELETE CASCADE

**Navigation Properties:**
- User (User)

---

### 3.4 UserPreferences

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

### 3.5 ActivityLogs

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh log |
| UserId | UNIQUEIDENTIFIER | FOREIGN KEY | Tham chiếu Users |
| Action | NVARCHAR(255) | NOT NULL | created_task, updated_task, commented, assigned_task |
| ResourceType | NVARCHAR(50) | NOT NULL | Task, Project, Comment |
| ResourceId | UNIQUEIDENTIFIER | NOT NULL | Định danh resource |
| Timestamp | DATETIME2 | NOT NULL | Thời gian hành động |

**Foreign Keys:**
- UserId → Users(Id)

**Navigation Properties:**
- User (User)

---

## 4. ENTITY RELATIONSHIPS

### Project Service → Task Service
```
Projects (ProjectDB)
    ↓ (ProjectId)
Tasks (TaskDB)
    ↓ (SprintId)
Sprints (ProjectDB)
```

### Project Service → Notify Service
```
Projects.CreatedBy → Users(Id)
Members.UserId → Users(Id)
```

### Task Service → Notify Service
```
Tasks.CreatedBy → Users(Id)
Tasks.AssignedTo → Users(Id)
SubTasks.AssignedTo → Users(Id)
TaskTimeLog.LoggedBy → Users(Id)
TaskAssignmentHistory.ChangedBy → Users(Id)
Comments.TaskId → Tasks(Id)
Comments.AuthorId → Users(Id)
```

### Notify Service → Task Service
```
Comments.TaskId → Tasks(Id) [TaskDB]
Notifications.RelatedTaskId → Tasks(Id) [TaskDB]
```

### Notify Service → Project Service
```
Notifications.RelatedProjectId → Projects(Id) [ProjectDB]
```

---

## 5. CROSS-SERVICE EVENTS

### Project Service Events
- `project.member.added` → Gửi thông báo tới Member
- `sprint.started` → Gửi thông báo tới Project Members

### Task Service Events
- `task.status.changed` → Gửi thông báo tới Assignee
- `task.assigned` → Gửi thông báo tới Assignee

### Notify Service Events
- `comment.created` → Gửi thông báo tới Task Assignee
- `user.mentioned` → Gửi thông báo tới Mentioned Users

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
CREATE INDEX idx_Members_ProjectId_UserId ON Members(ProjectId, UserId);
CREATE INDEX idx_Sprints_ProjectId ON Sprints(ProjectId);
CREATE INDEX idx_Milestones_ProjectId ON Milestones(ProjectId);
```

### TaskDB
```sql
CREATE INDEX idx_Tasks_ProjectId ON Tasks(ProjectId);
CREATE INDEX idx_Tasks_AssignedTo ON Tasks(AssignedTo);
CREATE INDEX idx_Tasks_Status ON Tasks(Status);
CREATE INDEX idx_SubTasks_TaskId ON SubTasks(TaskId);
CREATE INDEX idx_TaskTimeLog_TaskId ON TaskTimeLog(TaskId);
CREATE INDEX idx_KanbanBoard_ProjectId ON KanbanBoard(ProjectId);
```

### NotifyDB
```sql
CREATE INDEX idx_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
CREATE INDEX idx_Comments_TaskId ON Comments(TaskId);
CREATE INDEX idx_ActivityLog_UserId ON ActivityLog(UserId);
CREATE INDEX idx_UserPreference_UserId ON UserPreference(UserId);
```

---

**Cuối cùng: 2026-05-19**
