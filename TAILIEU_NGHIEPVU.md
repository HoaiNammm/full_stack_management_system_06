# Tài Liệu Nghiệp Vụ — Hệ Thống Quản Lý Dự Án

> **Phiên bản:** 1.0 | **Ngày:** 20/06/2026 | **Nền tảng:** Microservices ASP.NET Core + Vue 3

---

## Mục Lục

1. [Tổng Quan Hệ Thống](#1-tổng-quan-hệ-thống)
2. [Lược Đồ Phân Rã Hệ Thống](#2-lược-đồ-phân-rã-hệ-thống)
3. [Phân Rã Chức Năng](#3-phân-rã-chức-năng)
4. [Mô Hình Dữ Liệu](#4-mô-hình-dữ-liệu)
5. [Đặc Tả API](#5-đặc-tả-api)
6. [Đặc Tả Validation](#6-đặc-tả-validation)
7. [Phân Quyền Người Dùng](#7-phân-quyền-người-dùng)
8. [Luồng Nghiệp Vụ Chính](#8-luồng-nghiệp-vụ-chính)
9. [Giao Diện & Các Thành Phần Frontend](#9-giao-diện--các-thành-phần-frontend)

---

## 1. Tổng Quan Hệ Thống

### Mô Tả

Hệ thống quản lý dự án dạng SaaS hỗ trợ nhóm phát triển phần mềm theo phương pháp Agile/Scrum. Ứng dụng cho phép tổ chức dự án theo sprint, cột mốc (milestone), bảng Kanban, quản lý công việc (task), phân công thành viên, theo dõi tiến độ và trao đổi nội bộ thông qua bình luận.

### Kiến Trúc Tổng Thể

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CLIENT LAYER                               │
│                  Vue 3 + Vite  (port 5176)                          │
│          Tailwind CSS · Material Symbols · Axios                    │
└──────────────────────────┬──────────────────────────────────────────┘
                           │  HTTP/REST (JWT Bearer)
          ┌────────────────┼────────────────────────┐
          │                │                        │
          ▼                ▼                        ▼
┌─────────────────┐ ┌──────────────────┐ ┌─────────────────────┐
│  ProjectService │ │   TaskService    │ │   NotifyService     │
│   (port 5047)   │ │   (port 5217)    │ │    (port 5177)      │
│  ASP.NET Core   │ │  ASP.NET Core    │ │   ASP.NET Core      │
│   .NET 8/9      │ │   .NET 8/9       │ │    .NET 8/9         │
└────────┬────────┘ └────────┬─────────┘ └──────────┬──────────┘
         │                   │                       │
         ▼                   ▼                       ▼
  ┌─────────────┐    ┌─────────────┐       ┌──────────────────┐
  │  ProjectDB  │    │   TaskDB    │       │    NotifyDB      │
  │ SQL Server  │    │ SQL Server  │       │   SQL Server     │
  │ (HANOIMD)   │    │ (HANOIMD)   │       │   (HANOIMD)      │
  └─────────────┘    └─────────────┘       └──────────────────┘
                           │
                    ┌──────┴──────┐
                    │  RabbitMQ   │  (tùy chọn – event bus)
                    └─────────────┘
```

### Công Nghệ Sử Dụng

| Lớp | Công nghệ |
|-----|-----------|
| Frontend | Vue 3, Vite, Tailwind CSS, Axios, Vue Router |
| Backend | ASP.NET Core (.NET 8/9), Entity Framework Core |
| Database | SQL Server (Windows Authentication) |
| Auth | JWT Bearer Token (shared secret giữa các service) |
| Message Bus | RabbitMQ (optional) |
| ORM | EF Core – Code-First Migrations |

---

## 2. Lược Đồ Phân Rã Hệ Thống

```
HỆ THỐNG QUẢN LÝ DỰ ÁN
│
├── ProjectService  ──────────────────────────────────────────────────
│   │  Quản lý toàn bộ cấu trúc dự án
│   │
│   ├── Dự án (Projects)
│   │   ├── Tạo dự án từ template
│   │   ├── Xem danh sách dự án của tôi
│   │   ├── Xem chi tiết dự án
│   │   ├── Cập nhật thông tin dự án
│   │   └── Xóa dự án
│   │
│   ├── Thành viên (Members)
│   │   ├── Xem danh sách thành viên
│   │   ├── Thêm thành viên vào dự án
│   │   ├── Thay đổi vai trò thành viên
│   │   └── Xóa thành viên khỏi dự án
│   │
│   ├── Sprint
│   │   ├── Xem danh sách sprint
│   │   ├── Tạo sprint mới
│   │   ├── Cập nhật sprint
│   │   └── Xóa sprint
│   │
│   ├── Cột mốc (Milestones)
│   │   ├── Xem danh sách cột mốc
│   │   ├── Tạo cột mốc mới
│   │   ├── Cập nhật cột mốc
│   │   └── Xóa cột mốc
│   │
│   └── Template dự án (Templates)
│       └── Xem danh sách template có sẵn
│
├── TaskService  ─────────────────────────────────────────────────────
│   │  Quản lý công việc và tiến độ
│   │
│   ├── Công việc (Tasks)
│   │   ├── Tạo task mới
│   │   ├── Xem danh sách task (theo dự án / cột / sprint / người phụ trách)
│   │   ├── Xem chi tiết task
│   │   ├── Cập nhật thông tin task
│   │   ├── Di chuyển task sang cột khác
│   │   └── Xóa task (soft-delete)
│   │
│   ├── Công việc con (SubTasks)
│   │   ├── Xem sub-task của task
│   │   ├── Tạo sub-task
│   │   ├── Cập nhật sub-task
│   │   └── Xóa sub-task
│   │
│   ├── Cột Kanban (KanbanColumns)
│   │   ├── Xem cột của dự án
│   │   ├── Thêm cột mới
│   │   ├── Cập nhật cột
│   │   └── Xóa cột (chỉ khi không có task)
│   │
│   └── Nhật ký thời gian (TimeLogs)
│       ├── Xem time log của task
│       ├── Ghi nhận giờ làm việc
│       └── Xóa time log
│
└── NotifyService  ───────────────────────────────────────────────────
    │  Xác thực, thông báo, bình luận, nhật ký
    │
    ├── Xác thực (Auth)
    │   ├── Đăng nhập
    │   ├── Đăng ký
    │   ├── Xem thông tin cá nhân
    │   ├── Cập nhật hồ sơ
    │   ├── Đổi mật khẩu
    │   └── Upload ảnh đại diện
    │
    ├── Người dùng (Users)
    │   ├── Xem danh sách user
    │   └── Xem chi tiết user
    │
    ├── Thông báo (Notifications)
    │   ├── Xem tất cả thông báo của tôi
    │   ├── Đếm thông báo chưa đọc
    │   ├── Đánh dấu đã đọc (1 hoặc tất cả)
    │   └── Quản lý template thông báo
    │
    ├── Bình luận (Comments)
    │   ├── Bình luận theo task
    │   ├── Bình luận theo dự án
    │   ├── Trả lời bình luận (reply)
    │   ├── Chỉnh sửa bình luận
    │   └── Xóa bình luận
    │
    └── Nhật ký hoạt động (ActivityLogs)
        ├── Xem nhật ký hoạt động theo task
        └── Xem nhật ký hoạt động theo dự án
```
---

## 3. Phân Rã Chức Năng

### 3.1 Quản Lý Dự Án

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-P01 | Tạo dự án | Tạo dự án mới từ template có sẵn (Software Dev, Research, Event, Blank). Tự động tạo các sprint, cột mốc và cột Kanban theo template. | Tất cả user đã đăng nhập |
| CN-P02 | Xem danh sách dự án | Hiển thị các dự án mà user đang là thành viên, kèm thông tin: tên, trạng thái, ngày, số thành viên, số sprint. | Thành viên dự án |
| CN-P03 | Xem chi tiết dự án | Hiển thị đầy đủ thông tin dự án theo các tab: Kanban, Sprint, Timeline, Thành viên, Hoạt động. | Thành viên dự án |
| CN-P04 | Sửa dự án | Cập nhật tên, mô tả, màu sắc, trạng thái, ngày bắt đầu/kết thúc của dự án. | Owner, Manager |
| CN-P05 | Xóa dự án | Xóa vĩnh viễn dự án và toàn bộ dữ liệu liên quan. | Owner |

### 3.2 Quản Lý Thành Viên

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-M01 | Xem thành viên | Hiển thị danh sách thành viên, vai trò, ngày tham gia. | Tất cả thành viên |
| CN-M02 | Thêm thành viên | Mời user có sẵn trong hệ thống vào dự án với vai trò cụ thể. | Owner, Manager |
| CN-M03 | Thay đổi vai trò | Thay đổi vai trò của thành viên hiện tại. | Owner |
| CN-M04 | Xóa thành viên | Loại thành viên ra khỏi dự án. | Owner |

### 3.3 Quản Lý Sprint

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-S01 | Xem sprint | Danh sách sprint với trạng thái, ngày, mục tiêu, danh sách task. | Tất cả thành viên |
| CN-S02 | Tạo sprint | Tạo sprint mới với tên, mục tiêu, ngày bắt đầu. Ngày kết thúc tự động = +14 ngày. | Owner, Manager |
| CN-S03 | Cập nhật sprint | Sửa thông tin sprint, thay đổi trạng thái (Draft → Active → Completed). | Owner, Manager |
| CN-S04 | Xóa sprint | Xóa sprint (không thể xóa sprint đang Active). | Owner, Manager |

### 3.4 Quản Lý Cột Mốc (Milestone)

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-MS01 | Xem cột mốc | Danh sách cột mốc sắp xếp theo ngày mục tiêu. | Tất cả thành viên |
| CN-MS02 | Tạo cột mốc | Tạo cột mốc mới với tên, mô tả và ngày mục tiêu. | Owner, Manager |
| CN-MS03 | Cập nhật cột mốc | Sửa thông tin cột mốc, cập nhật trạng thái. | Owner, Manager |
| CN-MS04 | Xóa cột mốc | Xóa cột mốc khỏi dự án. | Owner, Manager |

### 3.5 Quản Lý Công Việc (Task)

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-T01 | Xem task | Hiển thị task trên bảng Kanban hoặc danh sách; lọc theo cột, sprint, người phụ trách. | Tất cả thành viên |
| CN-T02 | Tạo task | Tạo task mới với tiêu đề, mô tả, độ ưu tiên, người phụ trách, hạn chót, giờ ước tính. | Developer, Manager, Owner |
| CN-T03 | Cập nhật task | Sửa thông tin task. | Developer, Manager, Owner |
| CN-T04 | Di chuyển task | Kéo-thả hoặc chọn để chuyển task sang cột Kanban khác. | Developer, Manager, Owner |
| CN-T05 | Xóa task | Xóa mềm task (soft-delete). | Developer, Manager, Owner |
| CN-T06 | Xem chi tiết task | Xem đầy đủ: thông tin, sub-tasks, bình luận, time logs, nhật ký. | Tất cả thành viên |

### 3.6 Quản Lý Sub-Task

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-ST01 | Xem sub-task | Danh sách công việc con của task cha. | Tất cả thành viên |
| CN-ST02 | Tạo sub-task | Tạo công việc con với tiêu đề, người phụ trách, giờ ước tính. | Developer, Manager, Owner |
| CN-ST03 | Cập nhật sub-task | Sửa thông tin, đánh dấu hoàn thành. | Developer, Manager, Owner |
| CN-ST04 | Xóa sub-task | Xóa mềm sub-task. | Developer, Manager, Owner |

### 3.7 Quản Lý Cột Kanban

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-K01 | Xem cột Kanban | Hiển thị các cột theo thứ tự position. | Tất cả thành viên |
| CN-K02 | Thêm cột | Tạo cột mới với tên và loại. | Owner, Manager |
| CN-K03 | Sửa cột | Cập nhật tên cột. | Owner, Manager |
| CN-K04 | Xóa cột | Xóa cột (chỉ khi cột không còn task nào). | Owner, Manager |

### 3.8 Bình Luận & Cộng Tác

| Mã CN | Tên chức năng | Mô tả | Vai trò |
|-------|---------------|-------|---------|
| CN-C01 | Bình luận task | Viết bình luận vào task cụ thể. | Tất cả thành viên |
| CN-C02 | Bình luận dự án | Viết bình luận cấp dự án (thảo luận chung). | Tất cả thành viên |
| CN-C03 | Trả lời bình luận | Trả lời một bình luận đã có (nested reply). | Tất cả thành viên |
| CN-C04 | Sửa bình luận | Chỉnh sửa nội dung bình luận của chính mình. | Tác giả bình luận |
| CN-C05 | Xóa bình luận | Xóa mềm bình luận. | Tác giả bình luận |
| CN-C06 | Đề cập (@mention) | Tag người dùng trong bình luận, hệ thống tự gửi thông báo. | Tất cả thành viên |

### 3.9 Thông Báo

| Mã CN | Tên chức năng | Mô tả |
|-------|---------------|-------|
| CN-N01 | Xem thông báo | Danh sách thông báo của tôi, sắp xếp theo thời gian mới nhất. |
| CN-N02 | Đếm chưa đọc | Hiển thị số thông báo chưa đọc trên icon chuông. |
| CN-N03 | Đánh dấu đã đọc | Đánh dấu 1 hoặc tất cả thông báo là đã đọc. |

### 3.10 Tài Khoản & Hồ Sơ

| Mã CN | Tên chức năng | Mô tả |
|-------|---------------|-------|
| CN-A01 | Đăng nhập | Đăng nhập bằng email và mật khẩu, nhận JWT token. |
| CN-A02 | Đăng ký | Tạo tài khoản mới. |
| CN-A03 | Xem hồ sơ | Xem thông tin cá nhân hiện tại. |
| CN-A04 | Cập nhật hồ sơ | Sửa tên, số điện thoại, phòng ban, chức vụ. |
| CN-A05 | Đổi mật khẩu | Đổi mật khẩu với xác nhận mật khẩu cũ. |
| CN-A06 | Upload ảnh đại diện | Upload ảnh đại diện mới. |

---

## 4. Mô Hình Dữ Liệu

### 4.1 ProjectDB — ProjectService

```
Projects
├── Id              UNIQUEIDENTIFIER  PK, DEFAULT NEWID()
├── Name            NVARCHAR(200)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── Status          INT               DEFAULT 0
│                   0=Bản nháp | 1=Đang hoạt động | 2=Hoàn thành | 3=Tạm dừng
├── Color           NVARCHAR(50)      NULL  (hex, vd: #3525cd)
├── StartDate       DATETIME2         NULL
├── EndDate         DATETIME2         NULL
├── CreatedBy       UNIQUEIDENTIFIER  NOT NULL  (UserId)
├── CreatedAt       DATETIME2         NOT NULL
└── UpdatedAt       DATETIME2         NULL

Members
├── Id              UNIQUEIDENTIFIER  PK
├── ProjectId       UNIQUEIDENTIFIER  FK → Projects(Id)
├── UserId          UNIQUEIDENTIFIER  NOT NULL
├── Role            INT               NOT NULL
│                   0=Owner | 1=Manager | 2=Developer | 3=Viewer
└── JoinedAt        DATETIME2         NOT NULL

Sprints
├── Id              UNIQUEIDENTIFIER  PK
├── ProjectId       UNIQUEIDENTIFIER  FK → Projects(Id)
├── Name            NVARCHAR(200)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── Goal            NVARCHAR(MAX)     NULL
├── StartDate       DATETIME2         NOT NULL
├── EndDate         DATETIME2         NOT NULL  (= StartDate + 14 ngày)
├── Status          INT               DEFAULT 0
│                   0=Bản nháp | 1=Đang chạy | 2=Hoàn thành
├── CreatedAt       DATETIME2         NOT NULL
└── UpdatedAt       DATETIME2         NULL

Milestones
├── Id              UNIQUEIDENTIFIER  PK
├── ProjectId       UNIQUEIDENTIFIER  FK → Projects(Id)
├── Name            NVARCHAR(200)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── TargetDate      DATETIME2         NOT NULL
├── Status          INT               DEFAULT 0
│                   0=Chưa bắt đầu | 1=Đang thực hiện | 2=Hoàn thành
└── CreatedAt       DATETIME2         NOT NULL
```

### 4.2 TaskDB — TaskService

```
KanbanColumns
├── Id              UNIQUEIDENTIFIER  PK
├── ProjectId       UNIQUEIDENTIFIER  NOT NULL  (INDEX)
├── Name            NVARCHAR(100)     NOT NULL
├── Position        INT               NOT NULL
└── Type            NVARCHAR(50)      DEFAULT 'custom'
                    'backlog' | 'active' | 'done' | 'custom'

Tasks
├── Id              UNIQUEIDENTIFIER  PK
├── ProjectId       UNIQUEIDENTIFIER  NOT NULL  (INDEX)
├── ColumnId        UNIQUEIDENTIFIER  FK → KanbanColumns(Id)
├── Title           NVARCHAR(500)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── Priority        INT               DEFAULT 2
│                   1=Thấp | 2=Trung bình | 3=Cao
├── AssignedTo      UNIQUEIDENTIFIER  NULL  (INDEX)
├── ReporterId      UNIQUEIDENTIFIER  NULL
├── DueDate         DATETIME2         NULL
├── EstimatedHours  DECIMAL(10,2)     NULL
├── SprintId        UNIQUEIDENTIFIER  NULL  (INDEX)
├── CreatedAt       DATETIME2         NOT NULL
├── CreatedBy       UNIQUEIDENTIFIER  NOT NULL
└── DeletedAt       DATETIME2         NULL  (soft-delete; INDEX)

SubTasks
├── Id              UNIQUEIDENTIFIER  PK
├── TaskId          UNIQUEIDENTIFIER  FK → Tasks(Id) CASCADE DELETE
├── Title           NVARCHAR(500)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── AssignedTo      UNIQUEIDENTIFIER  NULL
├── Status          INT               DEFAULT 0
│                   0=Chưa làm | 1=Hoàn thành
├── EstimatedHours  DECIMAL(10,2)     NULL
├── CreatedAt       DATETIME2         NOT NULL
├── UpdatedAt       DATETIME2         NULL
└── DeletedAt       DATETIME2         NULL  (soft-delete)

TaskTimeLogs
├── Id              UNIQUEIDENTIFIER  PK
├── TaskId          UNIQUEIDENTIFIER  FK → Tasks(Id) CASCADE DELETE
├── UserId          UNIQUEIDENTIFIER  NOT NULL
├── LoggedBy        UNIQUEIDENTIFIER  NOT NULL
├── Hours           DECIMAL(10,2)     NOT NULL
├── Description     NVARCHAR(MAX)     NULL
├── LoggedDate      DATETIME2         NOT NULL
├── LoggedAt        DATETIME2         NOT NULL
└── CreatedAt       DATETIME2         NOT NULL
```

### 4.3 NotifyDB — NotifyService

```
Users
├── Id              UNIQUEIDENTIFIER  PK
├── FullName        NVARCHAR(100)     NOT NULL
├── Email           NVARCHAR(150)     NOT NULL  UNIQUE
├── PasswordHash    NVARCHAR(MAX)     NOT NULL
├── Role            NVARCHAR(50)      DEFAULT 'Member'
├── PhoneNumber     NVARCHAR(20)      NULL
├── AvatarUrl       NVARCHAR(500)     NULL
├── Department      NVARCHAR(100)     NULL
├── Position        NVARCHAR(100)     NULL
├── IsActive        BIT               DEFAULT 1
├── EmailConfirmed  BIT               DEFAULT 0
├── CreatedAt       DATETIME2         NOT NULL
├── UpdatedAt       DATETIME2         NULL
└── LastLoginAt     DATETIME2         NULL

Notifications
├── Id              UNIQUEIDENTIFIER  PK
├── TaskId          UNIQUEIDENTIFIER  NULL  (INDEX)
├── ProjectId       UNIQUEIDENTIFIER  NULL  (INDEX)
├── Title           NVARCHAR(200)     NOT NULL
├── Content         NVARCHAR(MAX)     NOT NULL
├── Message         NVARCHAR(1000)    NOT NULL
├── Type            NVARCHAR(100)     NOT NULL
├── IsRead          BIT               DEFAULT 0
├── ReadAt          DATETIME2         NULL
└── CreatedAt       DATETIME2         NOT NULL

UserNotifications
├── Id              UNIQUEIDENTIFIER  PK
├── NotificationId  UNIQUEIDENTIFIER  FK → Notifications(Id) CASCADE
├── UserId          UNIQUEIDENTIFIER  NOT NULL
├── IsRead          BIT               DEFAULT 0
├── ReadAt          DATETIME2         NULL
└── CreatedAt       DATETIME2         NOT NULL

Comments
├── Id              UNIQUEIDENTIFIER  PK
├── TaskId          UNIQUEIDENTIFIER  NOT NULL  (INDEX)
├── ProjectId       UNIQUEIDENTIFIER  NULL
├── UserId          UNIQUEIDENTIFIER  NOT NULL  (INDEX)
├── Content         NVARCHAR(2000)    NOT NULL
├── IsDeleted       BIT               DEFAULT 0
├── CreatedAt       DATETIME2         NOT NULL
├── UpdatedAt       DATETIME2         NULL
└── ParentCommentId UNIQUEIDENTIFIER  NULL  FK → Comments(Id) RESTRICT

CommentAttachments
├── Id              UNIQUEIDENTIFIER  PK
├── CommentId       UNIQUEIDENTIFIER  FK → Comments(Id) CASCADE
├── FileName        NVARCHAR(255)     NOT NULL
├── FileUrl         NVARCHAR(1000)    NOT NULL
├── ContentType     NVARCHAR(100)     NULL
├── FileSize        BIGINT            NOT NULL
└── CreatedAt       DATETIME2         NOT NULL

CommentMentions
├── Id              UNIQUEIDENTIFIER  PK
├── CommentId       UNIQUEIDENTIFIER  FK → Comments(Id) CASCADE
├── MentionedUserId UNIQUEIDENTIFIER  NOT NULL  (INDEX)
└── CreatedAt       DATETIME2         NOT NULL

ActivityLogs
├── Id              UNIQUEIDENTIFIER  PK
├── UserId          UNIQUEIDENTIFIER  NOT NULL  (INDEX)
├── Action          NVARCHAR(100)     NOT NULL
├── Description     NVARCHAR(1000)    NULL
├── ResourceType    NVARCHAR(MAX)     NOT NULL
├── ResourceId      UNIQUEIDENTIFIER  NOT NULL
├── TaskId          UNIQUEIDENTIFIER  NULL  (INDEX)
├── ProjectId       UNIQUEIDENTIFIER  NULL  (INDEX)
├── MetadataJson    NVARCHAR(MAX)     NULL
├── Timestamp       DATETIME2         NOT NULL
└── CreatedAt       DATETIME2         NOT NULL  DEFAULT GETUTCDATE()
```

---

## 5. Đặc Tả API

### 5.1 ProjectService — `/api` (port 5047)

#### Dự án

| Method | Endpoint | Yêu cầu xác thực | Mô tả |
|--------|----------|-------------------|-------|
| `GET` | `/projects` | Có | Lấy danh sách dự án của user hiện tại |
| `GET` | `/projects/{id}` | Có | Lấy chi tiết một dự án (phải là thành viên) |
| `POST` | `/projects` | Có | Tạo dự án mới |
| `PUT` | `/projects/{id}` | Có | Cập nhật dự án (Owner hoặc Manager) |
| `DELETE` | `/projects/{id}` | Có | Xóa dự án (chỉ Owner) |

**Body POST /projects:**
```json
{
  "name": "Tên dự án",
  "description": "Mô tả dự án",
  "color": "#3525cd",
  "startDate": "2026-06-20T00:00:00Z",
  "endDate": "2026-09-30T00:00:00Z",
  "templateId": "software-dev"
}
```

**Body PUT /projects/{id}:**
```json
{
  "name": "Tên dự án mới",
  "description": "Mô tả mới",
  "color": "#006a61",
  "status": 1,
  "startDate": "2026-06-20T00:00:00Z",
  "endDate": "2026-09-30T00:00:00Z"
}
```

---

#### Thành viên

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/projects/{pid}/members` | Tất cả | Danh sách thành viên |
| `POST` | `/projects/{pid}/members` | Owner, Manager | Thêm thành viên |
| `PUT` | `/projects/{pid}/members/{mid}/role` | Owner | Đổi vai trò |
| `DELETE` | `/projects/{pid}/members/{mid}` | Owner | Xóa thành viên |

**Body POST /members:**
```json
{
  "userId": "guid-người-dùng",
  "role": 2
}
```

---

#### Sprint

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/projects/{pid}/sprints` | Tất cả | Danh sách sprint |
| `GET` | `/projects/{pid}/sprints/{id}` | Tất cả | Chi tiết sprint |
| `POST` | `/projects/{pid}/sprints` | Owner, Manager | Tạo sprint |
| `PUT` | `/projects/{pid}/sprints/{id}` | Owner, Manager | Cập nhật sprint |
| `DELETE` | `/projects/{pid}/sprints/{id}` | Owner, Manager | Xóa sprint |

**Body POST /sprints:**
```json
{
  "name": "Sprint 1",
  "goal": "Hoàn thành module đăng nhập",
  "startDate": "2026-06-20T00:00:00Z"
}
```

---

#### Cột mốc (Milestone)

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/projects/{pid}/milestones` | Tất cả | Danh sách cột mốc |
| `GET` | `/projects/{pid}/milestones/{id}` | Tất cả | Chi tiết cột mốc |
| `POST` | `/projects/{pid}/milestones` | Owner, Manager | Tạo cột mốc |
| `PUT` | `/projects/{pid}/milestones/{id}` | Owner, Manager | Cập nhật cột mốc |
| `DELETE` | `/projects/{pid}/milestones/{id}` | Owner, Manager | Xóa cột mốc |

**Body POST /milestones:**
```json
{
  "name": "Ra mắt phiên bản beta",
  "description": "Phiên bản thử nghiệm nội bộ",
  "targetDate": "2026-07-31T00:00:00Z"
}
```

**Body PUT /milestones/{id}:**
```json
{
  "name": "Ra mắt phiên bản beta",
  "description": "Phiên bản thử nghiệm nội bộ",
  "targetDate": "2026-07-31T00:00:00Z",
  "status": 1
}
```

---

#### Template

| Method | Endpoint | Yêu cầu xác thực | Mô tả |
|--------|----------|-------------------|-------|
| `GET` | `/templates` | Không | Lấy danh sách template |

**Template có sẵn:**

| ID | Tên | Sprint | Cột mốc |
|----|-----|--------|---------|
| `software-dev` | Phát triển phần mềm | 3 × 14 ngày | Alpha, Beta, Release |
| `research` | Nghiên cứu & Phân tích | 2 × 14 ngày | Nghiên cứu hoàn thành, Báo cáo cuối |
| `event-mgmt` | Quản lý sự kiện | 2 × 14 ngày | Chuẩn bị xong, Sự kiện diễn ra |
| `blank` | Trống | 0 | 0 |

---

### 5.2 TaskService — `/api` (port 5217)

#### Công việc (Task)

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/tasks?projectId=&columnId=&assignedTo=&sprintId=` | Tất cả | Danh sách task (có filter) |
| `GET` | `/tasks/{id}` | Tất cả | Chi tiết task |
| `POST` | `/tasks` | Developer+ | Tạo task mới |
| `PUT` | `/tasks/{id}` | Developer+ | Cập nhật task |
| `PUT` | `/tasks/{id}/column` | Developer+ | Di chuyển task sang cột khác |
| `DELETE` | `/tasks/{id}` | Developer+ | Xóa mềm task |

**Body POST /tasks:**
```json
{
  "projectId": "guid-dự-án",
  "columnId": "guid-cột-kanban",
  "title": "Xây dựng API đăng nhập",
  "description": "Implement JWT authentication endpoint",
  "priority": 3,
  "assignedTo": "guid-user",
  "dueDate": "2026-07-05T00:00:00Z",
  "estimatedHours": 8.5,
  "sprintId": "guid-sprint"
}
```

---

#### Cột Kanban

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/kanban-columns?projectId=` | Tất cả | Danh sách cột của dự án |
| `POST` | `/kanban-columns` | Owner, Manager | Thêm cột |
| `PUT` | `/kanban-columns/{id}` | Owner, Manager | Sửa tên cột |
| `DELETE` | `/kanban-columns/{id}` | Owner, Manager | Xóa cột |

---

#### Sub-Task

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/subtasks/task/{taskId}` | Tất cả | Danh sách sub-task |
| `POST` | `/subtasks` | Developer+ | Tạo sub-task |
| `PUT` | `/subtasks/{id}` | Developer+ | Cập nhật sub-task |
| `DELETE` | `/subtasks/{id}` | Developer+ | Xóa sub-task |

---

#### Time Log

| Method | Endpoint | Vai trò | Mô tả |
|--------|----------|---------|-------|
| `GET` | `/timelogs/task/{taskId}` | Tất cả | Time log của task |
| `POST` | `/timelogs` | Developer+ | Ghi nhận giờ làm |
| `DELETE` | `/timelogs/{id}` | Tác giả, Owner, Manager | Xóa time log |

---

### 5.3 NotifyService — `/api` (port 5177)

#### Xác thực

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `POST` | `/auth/login` | Đăng nhập → trả về JWT token |
| `POST` | `/users/register` | Đăng ký tài khoản mới |
| `GET` | `/auth/me` | Lấy thông tin user hiện tại |
| `PUT` | `/auth/profile` | Cập nhật hồ sơ |
| `POST` | `/auth/change-password` | Đổi mật khẩu |
| `POST` | `/auth/avatar` | Upload ảnh đại diện (multipart/form-data) |

**Body POST /auth/login:**
```json
{
  "email": "user@example.com",
  "password": "matkhau123"
}
```

**Response /auth/login:**
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "fullName": "Nguyễn Văn A",
      "role": "Member",
      "avatarUrl": null
    }
  }
}
```

---

#### Thông báo

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/notifications` | Danh sách thông báo của tôi |
| `GET` | `/notifications/unread-count` | Số lượng chưa đọc |
| `PUT` | `/notifications/{id}/read` | Đánh dấu đã đọc |
| `PUT` | `/notifications/read-all` | Đánh dấu tất cả đã đọc |

---

#### Bình luận

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/comment-workflows/task/{taskId}` | Bình luận của task |
| `POST` | `/comment-workflows` | Tạo bình luận task |
| `GET` | `/comment-workflows/project/{projectId}` | Bình luận của dự án |
| `POST` | `/comment-workflows/project/{projectId}` | Tạo bình luận dự án |
| `POST` | `/comment-workflows/{commentId}/reply` | Trả lời bình luận |
| `PUT` | `/comment-workflows/{commentId}` | Sửa bình luận |
| `DELETE` | `/comment-workflows/{commentId}` | Xóa bình luận |

---

## 6. Đặc Tả Validation

### 6.1 Validation Backend (ASP.NET Core)

#### Dự án (Project)

| Trường | Loại | Ràng buộc | Lỗi trả về |
|--------|------|-----------|------------|
| `name` | string | Bắt buộc, không được rỗng, ≤ 200 ký tự | `"Tên dự án là bắt buộc"` |
| `endDate` | DateTime? | Nếu có, phải > `startDate` | `"Ngày kết thúc phải sau ngày bắt đầu"` |
| `status` | int | ∈ {0, 1, 2, 3} | HTTP 400 |
| `color` | string? | Tùy chọn, khuyến khích định dạng hex | — |
| **Quyền xóa** | — | Chỉ Owner (role=0) | HTTP 403 |
| **Quyền sửa** | — | Owner hoặc Manager (role ≤ 1) | HTTP 403 |

---

#### Sprint

| Trường | Loại | Ràng buộc | Lỗi trả về |
|--------|------|-----------|------------|
| `name` | string | Bắt buộc, không rỗng | HTTP 400 |
| `startDate` | DateTime | Bắt buộc | HTTP 400 |
| `endDate` | DateTime | Bắt buộc khi update; phải > startDate | `"Ngày kết thúc phải sau ngày bắt đầu"` |
| `status` | int | ∈ {0, 1, 2} | HTTP 400 |
| **Trạng thái active** | — | Chỉ 1 sprint được Active tại 1 thời điểm | `"Đã có sprint đang chạy"` |
| **Xóa sprint active** | — | Không thể xóa sprint đang Active | HTTP 400 |
| **Quyền tạo/sửa/xóa** | — | Owner hoặc Manager | HTTP 403 |

---

#### Cột mốc (Milestone)

| Trường | Loại | Ràng buộc | Lỗi trả về |
|--------|------|-----------|------------|
| `name` | string | Bắt buộc, không rỗng | HTTP 400 |
| `targetDate` | DateTime | Bắt buộc; ≥ ngày hôm nay (UTC) khi **tạo** | `"Ngày mục tiêu không được là ngày trong quá khứ"` |
| `status` | int | ∈ {0, 1, 2} | HTTP 400 |
| **Quyền tạo/sửa/xóa** | — | Owner hoặc Manager | HTTP 403 |

---

#### Công việc (Task)

| Trường | Loại | Ràng buộc | Lỗi trả về |
|--------|------|-----------|------------|
| `title` | string | Bắt buộc, không rỗng, ≤ 500 ký tự | HTTP 400 |
| `priority` | int | ∈ {1, 2, 3} (Thấp, Trung bình, Cao) | HTTP 400 |
| `dueDate` | DateTime? | Nếu có: ≥ ngày hôm nay | HTTP 400 |
| `estimatedHours` | decimal? | Nếu có: > 0 | HTTP 400 |
| `columnId` | Guid | Phải thuộc cùng dự án với task | HTTP 400 |
| **Viewer** | — | Không được tạo, sửa, xóa | HTTP 403 |

---

#### Sub-Task

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `title` | string | Bắt buộc, ≤ 500 ký tự |
| `estimatedHours` | decimal? | Nếu có: > 0 |
| `status` | int | ∈ {0, 1} |
| **Viewer** | — | Không được tạo, sửa, xóa |

---

#### Time Log

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `hours` | decimal | Bắt buộc, > 0 |
| `loggedDate` | DateTime | Bắt buộc, ≤ ngày hôm nay |
| **Viewer** | — | Không được tạo |
| **Xóa** | — | Chỉ tác giả hoặc Owner/Manager |

---

#### Thành viên (Member)

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `userId` | Guid | Bắt buộc, phải là user tồn tại |
| `role` | int | ∈ {1, 2, 3} (không thể gán role Owner=0 qua API) |
| **Owner duy nhất** | — | Không thể xóa Owner cuối cùng |
| **Quyền thêm** | — | Owner hoặc Manager |
| **Manager giới hạn** | — | Manager chỉ được thêm Developer (role=2) hoặc Viewer (role=3) |
| **Quyền xóa** | — | Chỉ Owner |

---

#### Cột Kanban

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `name` | string | Bắt buộc, ≤ 100 ký tự |
| `type` | string | ∈ {'backlog', 'active', 'done', 'custom'} |
| **Xóa cột có task** | — | Không thể xóa khi cột đang có task |
| **Quyền tạo/sửa/xóa** | — | Owner hoặc Manager |

---

#### Bình luận (Comment)

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `content` | string | Bắt buộc, ≤ 2000 ký tự |
| `taskId` | Guid | Bắt buộc (với bình luận task) |
| **Sửa/xóa** | — | Chỉ tác giả bình luận |
| `fileName` | string | ≤ 255 ký tự (đính kèm) |
| `fileUrl` | string | ≤ 1000 ký tự (đính kèm) |

---

#### Xác thực (Auth)

| Trường | Loại | Ràng buộc |
|--------|------|-----------|
| `email` | string | Bắt buộc, định dạng email hợp lệ, unique |
| `password` | string | Bắt buộc, khuyến nghị ≥ 6 ký tự |
| `fullName` | string | Bắt buộc khi đăng ký, ≤ 100 ký tự |
| **Đổi mật khẩu** | — | Mật khẩu cũ phải đúng |

---

### 6.2 Validation Frontend (Vue 3)

#### CreateProjectModal.vue

```
Bước 2 – validate():
  ❌ name rỗng          → "Vui lòng nhập tên dự án"
  ❌ startDate rỗng     → "Vui lòng chọn ngày bắt đầu"
  ❌ startDate < today  → "Ngày bắt đầu không được ở trong quá khứ"
  ❌ endDate ≤ startDate → "Ngày kết thúc phải sau ngày bắt đầu"

Input constraints:
  startDate[type=date] :min="today"   (ngăn chọn ngày quá khứ ở UI)
  endDate[type=date]   :min="startDate"
```

---

#### EditProjectModal.vue

```
validate():
  ❌ name rỗng                      → "Vui lòng nhập tên dự án"
  ❌ endDate < startDate            → "Ngày kết thúc phải sau ngày bắt đầu"

Input constraints:
  endDate[type=date] :min="form.startDate"
```

---

#### CreateSprintModal.vue

```
validate():
  ❌ name rỗng          → "Vui lòng nhập tên sprint"
  ❌ goal rỗng          → "Vui lòng nhập mục tiêu sprint"
  ❌ startDate rỗng     → "Vui lòng chọn ngày bắt đầu"
  ❌ startDate < today  → "Ngày bắt đầu không được ở trong quá khứ"

Input constraints:
  startDate[type=date] :min="today"

Tự động tính:
  endDate = startDate + 14 ngày  (hiển thị preview, không cho sửa)
```

---

#### CreateMilestoneModal.vue (Tạo và Sửa)

```
validate():
  ❌ name rỗng              → "Vui lòng nhập tên cột mốc"
  ❌ targetDate rỗng        → "Vui lòng chọn ngày mục tiêu"
  ❌ targetDate < today     → "Ngày mục tiêu không được ở trong quá khứ"

Input constraints:
  targetDate[type=date] :min="today"

Chế độ:
  Khi prop milestone = null  → tạo mới (emit 'created')
  Khi prop milestone = {...}  → chỉnh sửa (emit 'updated', hiện thêm dropdown Trạng thái)
```

---

#### TaskDetailModal.vue (Task)

```
Trường được validate:
  ❌ title rỗng             → Không cho submit
  ❌ estimatedHours ≤ 0    → Không hợp lệ
  ❌ dueDate < today       → Cảnh báo hoặc chặn

Trường không bắt buộc: description, assignedTo, sprintId, priority
```

---

#### LoginView.vue

```
  ❌ email rỗng     → "Vui lòng nhập email"
  ❌ password rỗng  → "Vui lòng nhập mật khẩu"
  ❌ Sai thông tin  → Hiển thị lỗi từ API
```

---

### 6.3 Bảng Tổng Hợp Validation Ngày Tháng

| Màn hình | Trường | Ràng buộc | Kiểm tra UI | Kiểm tra BE |
|----------|--------|-----------|:-----------:|:-----------:|
| Tạo dự án | startDate | ≥ hôm nay | ✅ | ❌ |
| Tạo dự án | endDate | > startDate | ✅ | ✅ |
| Sửa dự án | endDate | > startDate | ✅ | ✅ |
| Tạo sprint | startDate | ≥ hôm nay | ✅ | ❌ |
| Tạo sprint | endDate | tự động +14 ngày | — | — |
| Tạo cột mốc | targetDate | ≥ hôm nay | ✅ | ✅ |
| Sửa cột mốc | targetDate | ≥ hôm nay | ✅ | ✅ |
| Tạo task | dueDate | ≥ hôm nay | ⚠️ | ✅ |
| Ghi time log | loggedDate | ≤ hôm nay | ❌ | ✅ |

> ✅ = Có kiểm tra | ❌ = Chưa có | ⚠️ = Kiểm tra một phần

---

## 7. Phân Quyền Người Dùng

### 7.1 Ma Trận Quyền

| Chức năng | Viewer (3) | Developer (2) | Manager (1) | Owner (0) |
|-----------|:----------:|:-------------:|:-----------:|:---------:|
| Xem dự án | ✅ | ✅ | ✅ | ✅ |
| Sửa dự án | ❌ | ❌ | ✅ | ✅ |
| Xóa dự án | ❌ | ❌ | ❌ | ✅ |
| Xem thành viên | ✅ | ✅ | ✅ | ✅ |
| Thêm thành viên | ❌ | ❌ | ✅¹ | ✅ |
| Thay đổi vai trò | ❌ | ❌ | ❌ | ✅ |
| Xóa thành viên | ❌ | ❌ | ❌ | ✅ |
| Xem sprint | ✅ | ✅ | ✅ | ✅ |
| Tạo/sửa/xóa sprint | ❌ | ❌ | ✅ | ✅ |
| Xem milestone | ✅ | ✅ | ✅ | ✅ |
| Tạo/sửa/xóa milestone | ❌ | ❌ | ✅ | ✅ |
| Xem task | ✅ | ✅ | ✅ | ✅ |
| Tạo task | ❌ | ✅ | ✅ | ✅ |
| Sửa task | ❌ | ✅ | ✅ | ✅ |
| Xóa task | ❌ | ✅ | ✅ | ✅ |
| Xem sub-task | ✅ | ✅ | ✅ | ✅ |
| Tạo/sửa/xóa sub-task | ❌ | ✅ | ✅ | ✅ |
| Tạo time log | ❌ | ✅ | ✅ | ✅ |
| Xóa time log | ❌ | Tác giả | ✅ | ✅ |
| Bình luận | ✅ | ✅ | ✅ | ✅ |
| Sửa/xóa bình luận | Tác giả | Tác giả | Tác giả | Tác giả |
| Thêm/sửa/xóa cột Kanban | ❌ | ❌ | ✅ | ✅ |

> ¹ Manager chỉ có thể thêm Developer (role=2) và Viewer (role=3)

### 7.2 Quy Tắc Đặc Biệt

1. **Không thể tự xóa Owner cuối cùng** — Dự án luôn phải có ít nhất 1 Owner.
2. **Không thể xóa sprint đang Active** — Phải hoàn thành hoặc chuyển về Draft trước.
3. **Chỉ 1 sprint Active tại một thời điểm** — Không thể kích hoạt sprint thứ 2 khi đã có sprint đang chạy.
4. **Xóa cột Kanban phải trống** — Cột chỉ được xóa khi không còn task nào.
5. **Owner được tự động gán khi tạo dự án** — Người tạo dự án luôn có role=0 (Owner).

---

## 8. Luồng Nghiệp Vụ Chính

### 8.1 Luồng Tạo Dự Án

```
User nhấn "Tạo dự án mới"
    │
    ▼
Bước 1: Chọn template
    ├── software-dev  → 3 sprints, 3 milestones, 5 cột Kanban
    ├── research      → 2 sprints, 2 milestones, 4 cột Kanban
    ├── event-mgmt    → 2 sprints, 2 milestones, 4 cột Kanban
    └── blank         → 0 sprints, 0 milestones, 4 cột Kanban cơ bản
    │
    ▼
Bước 2: Điền thông tin dự án
    ├── Tên (bắt buộc)
    ├── Mô tả (tùy chọn)
    ├── Ngày bắt đầu (bắt buộc, ≥ hôm nay)
    ├── Ngày kết thúc (tùy chọn, > ngày bắt đầu)
    ├── Màu sắc
    └── Thêm thành viên ban đầu (tùy chọn)
    │
    ▼
POST /api/projects → ProjectService
    ├── Tạo bản ghi Project
    ├── Gán Owner (role=0) cho người tạo
    ├── Seed Sprints từ template (tính ngày bắt đầu từ project.startDate)
    ├── Seed Milestones từ template
    └── Publish event "project.created" → TaskService
            └── TaskService nhận event → Seed KanbanColumns
    │
    ▼
Thêm thành viên ban đầu (nếu có)
    └── POST /api/projects/{id}/members (lần lượt từng member)
    │
    ▼
Chuyển đến trang chi tiết dự án
```

---

### 8.2 Luồng Quản Lý Task Trên Kanban

```
Xem bảng Kanban
    │
    ├── GET /api/kanban-columns?projectId=X  → Lấy danh sách cột (có Position)
    └── GET /api/tasks?projectId=X           → Lấy tất cả task của dự án
    │
    ▼
Hiển thị bảng: mỗi cột chứa các task có ColumnId tương ứng
    │
    ├── Nhấn "+ Thêm task"
    │       └── POST /api/tasks → Tạo task mới trong cột được chọn
    │
    ├── Kéo task sang cột khác
    │       └── PUT /api/tasks/{id}/column { columnId: newColumnId }
    │
    ├── Nhấn vào task → Mở TaskDetailModal
    │       ├── GET /api/subtasks/task/{taskId}
    │       ├── GET /api/comment-workflows/task/{taskId}
    │       ├── GET /api/timelogs/task/{taskId}
    │       └── GET /api/activity-logs/task/{taskId}
    │
    └── Xóa task
            └── DELETE /api/tasks/{id}  (soft-delete: DeletedAt = now)
```

---

### 8.3 Luồng Bình Luận & Thông Báo

```
User A viết bình luận trong TaskDetailModal
    │   Content: "Cần review @UserB trước khi merge"
    │
    ▼
POST /api/comment-workflows
    {
      taskId: "...",
      content: "Cần review @UserB trước khi merge",
      mentionedUserIds: ["guid-of-UserB"],
      attachments: []
    }
    │
    ▼
NotifyService xử lý:
    ├── Lưu Comment vào DB
    ├── Lưu CommentMention (mentionedUserId = UserB)
    ├── Lưu ActivityLog (Action="COMMENT_CREATED")
    ├── Tìm template COMMENT_MENTION
    ├── Tạo Notification: "User A đề cập đến bạn trong bình luận"
    └── Tạo UserNotification cho UserB (isRead=false)
    │
    ▼
UserB đăng nhập, icon chuông hiển thị badge số chưa đọc
    └── GET /api/notifications/unread-count
    │
    ▼
UserB nhấn xem thông báo
    ├── GET /api/notifications
    └── PUT /api/notifications/{id}/read
```

---

### 8.4 Luồng Quản Lý Sprint

```
Owner/Manager tạo Sprint mới
    │
    ▼
POST /api/projects/{pid}/sprints
    {
      name: "Sprint 1",
      goal: "Hoàn thành module xác thực",
      startDate: "2026-06-20"
    }
    └── endDate tự động = startDate + 14 ngày
    │
    ▼
Sprint được tạo với status=0 (Bản nháp)
    │
    ▼
Thêm task vào sprint:
    PUT /api/tasks/{taskId} { sprintId: "sprint-guid" }
    │
    ▼
Kích hoạt sprint:
    PUT /api/projects/{pid}/sprints/{sid} { status: 1 }
    └── Validate: không có sprint nào khác đang Active
    │
    ▼
Hoàn thành sprint:
    PUT /api/projects/{pid}/sprints/{sid} { status: 2 }
    └── Tùy chọn: chuyển task chưa xong sang sprint tiếp theo
```

---

## 9. Giao Diện & Các Thành Phần Frontend

### 9.1 Màn Hình Chính

| Đường dẫn | View | Mô tả |
|-----------|------|-------|
| `/` | LandingView | Trang chủ giới thiệu hệ thống |
| `/login` | LoginView | Đăng nhập |
| `/register` | RegisterView | Đăng ký tài khoản |
| `/dashboard` | Dashboard | Tổng quan: dự án, task giao cho tôi, hoạt động gần đây |
| `/projects` | ProjectsPage | Danh sách dự án của tôi |
| `/projects/:id` | ProjectDetail | Chi tiết dự án (Kanban, Sprint, Timeline, Members, Activity) |
| `/projects/:id/kanban` | KanbanBoard | Bảng Kanban đầy đủ |
| `/projects/:id/calendar` | CalendarView | Xem lịch timeline sprint/milestone |
| `/settings` | SettingsPage | Cài đặt cá nhân: hồ sơ, mật khẩu, avatar |
| `/notifications` | NotificationsPage | Danh sách thông báo |
| `/system-status` | SystemStatusPage | Trạng thái các service |

---

### 9.2 Tab Trong ProjectDetail.vue

| Tab | Nội dung | Chức năng |
|-----|----------|-----------|
| **Kanban** | Bảng công việc chia theo cột | Xem, tạo, di chuyển, xóa task |
| **Sprints** | Danh sách sprint và task trong sprint | Tạo, kích hoạt, hoàn thành sprint |
| **Timeline** | Danh sách cột mốc sắp xếp theo ngày | Tạo, sửa, xóa milestone |
| **Thành viên** | Danh sách + vai trò | Thêm, đổi vai trò, xóa thành viên |
| **Hoạt động** | Nhật ký hoạt động dự án | Chỉ xem |

---

### 9.3 Các Modal / Dialog Chính

| Component | Trigger | Chức năng |
|-----------|---------|-----------|
| `CreateProjectModal.vue` | Nút "Tạo dự án mới" | 2 bước: chọn template → điền chi tiết |
| `EditProjectModal.vue` | Nút "Sửa dự án" (header) | Sửa: tên, mô tả, màu, trạng thái, ngày |
| `CreateSprintModal.vue` | Nút "Tạo Sprint" | Điền: tên, mục tiêu, ngày bắt đầu |
| `CreateMilestoneModal.vue` | Nút "Thêm cột mốc" hoặc "Sửa" | Tạo mới hoặc chỉnh sửa cột mốc |
| `TaskDetailModal.vue` | Click vào task card | Xem/sửa chi tiết task + sub-task + bình luận + timelog |

---

### 9.4 Các Trạng Thái Hiển Thị

#### Trạng thái Dự án

| Giá trị | Tên | Màu UI |
|---------|-----|--------|
| 0 | Bản nháp | Xám |
| 1 | Đang hoạt động | Xanh lá |
| 2 | Hoàn thành | Xanh dương |
| 3 | Tạm dừng | Vàng cam |

#### Trạng thái Sprint

| Giá trị | Tên | Màu UI |
|---------|-----|--------|
| 0 | Bản nháp | Xám |
| 1 | Đang chạy | Xanh lá |
| 2 | Hoàn thành | Xanh dương |

#### Trạng thái Cột mốc

| Giá trị | Tên | Màu UI |
|---------|-----|--------|
| 0 | Chưa bắt đầu | Xám |
| 1 | Đang thực hiện | Xanh lam |
| 2 | Hoàn thành | Xanh lá |

#### Độ ưu tiên Task

| Giá trị | Tên | Icon |
|---------|-----|------|
| 1 | Thấp | Mũi tên xuống — xanh lá |
| 2 | Trung bình | Dấu gạch ngang — vàng |
| 3 | Cao | Mũi tên lên — đỏ |

#### Vai trò thành viên

| Giá trị | Tên | Quyền hạn |
|---------|-----|-----------|
| 0 | Chủ dự án | Toàn quyền, duy nhất |
| 1 | Quản lý dự án | Quản lý sprint, milestone, thành viên |
| 2 | Lập trình viên | Tạo và quản lý task |
| 3 | Người xem | Chỉ xem |

---

*Tài liệu được tạo tự động dựa trên phân tích mã nguồn thực tế của hệ thống.*
*Cập nhật lần cuối: 20/06/2026*
