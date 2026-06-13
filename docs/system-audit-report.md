# Bao cao ra soat he thong quan ly du an va phan cong cong viec

Ngay kiem tra: 11/06/2026

## 1. Pham vi doi chieu

Nguon yeu cau:

- File Excel `Nhom 4.xlsx`, sheet `Phan tach he thong`, gom 18 tieu chi.
- Tai lieu du an trong `docs/`, `contracts/`, `README.md`.
- Source hien co trong `src/frontend`, `src/ProjectService`, `src/TaskService`, `src/NotifyService.Api`.

Kien truc hien tai:

- Frontend: Vue 3, Vue Router, Pinia, Axios, TailwindCSS, Vite.
- Backend:
  - `NotifyService.Api`: auth, users, comments, notifications, activity logs.
  - `ProjectService`: projects, members, roles, templates, sprints, milestones.
  - `TaskService`: tasks, kanban columns, subtasks, time logs.
- Infrastructure local:
  - SQL Server container port `1433`.
  - RabbitMQ container port `5672`, management `15672`.

## 2. Ket qua build va runtime

Build:

- `dotnet build src/ProjectService/ProjectService/ProjectService.csproj`: pass, 0 error.
- `dotnet build src/TaskService/TaskService/TaskService.csproj`: pass, 0 error.
- `dotnet build src/NotifyService.Api/NotifyService.Api.csproj`: pass, 0 error, con 3 warning nullability cu.
- `npm run build` trong `src/frontend`: pass, co warning chunk lon cua Vite.

Runtime:

- `http://localhost:5177/swagger`: 200.
- `http://localhost:5047/swagger`: 200.
- `http://localhost:5217/swagger`: 200.
- `http://127.0.0.1:5173`: 200.

## 3. Doi chieu theo Excel

| STT | Nhom | Tieu chi | Trang thai | Ghi chu |
| --- | --- | --- | --- | --- |
| 1 | N1 Project & Member | Dashboard tong quan | Da hoan thanh co ban | Dashboard lay du lieu that tu Project/User/Task/Notification API, hien thi tong du an, thanh vien, task, notification, tien do. |
| 2 | N1 Project & Member | CRUD du an | Da hoan thanh co ban | Co danh sach, tao, xem chi tiet, xoa; update du an co API nhung FE can bo sung man hinh edit ro hon. |
| 3 | N1 Project & Member | Project Detail | Da hoan thanh co ban | Co overview, members, sprints, timeline, kanban summary, activity, comments placeholder. |
| 4 | N1 Project & Member | Project Members | Da hoan thanh | Them, xoa, doi role trong `MembersSection`; tao project co chon thanh vien ban dau va role. |
| 5 | N1 Project & Member | Role Assignment | Da hoan thanh co ban | Backend phan quyen Owner/Manager; FE ho tro Owner, Project Manager, Developer, Tester, Viewer. Can dong bo enum backend comment cu van ghi Owner/Manager/Member/Viewer. |
| 6 | N1 Project & Member | Members Overview | Con thieu mot phan | Co API users va Settings/user info, nhung chua co route `/members` rieng trong router hien tai. |
| 7 | N1 Project & Member | Khoi tao du lieu nen | Da chuyen huong dung API that | Yeu cau moi uu tien khong mock. He thong seed user demo va template project that. |
| 8 | N2 Task & Kanban | Kanban Board | Da hoan thanh co ban | Co 6 cot Backlog, To Do, In Progress, Review, Testing, Done; co tao task nhanh va move column qua API. Drag-drop can kiem tra/bo sung UI neu muon dung thao tac keo tha that. |
| 9 | N2 Task & Kanban | Task List | Con thieu mot phan | Co API filter theo project/column/assignee/sprint; FE hien chu yeu qua Kanban, chua co trang task list rieng `/projects/:id/tasks`. |
| 10 | N2 Task & Kanban | Task Form | Da hoan thanh co ban | Co tao task, sua task trong modal/detail, priority, assignee, deadline, sprint. |
| 11 | N2 Task & Kanban | Sprint Management | Da hoan thanh co ban | Co sprint list/detail, tao sprint; gan task vao sprint qua task form/API. |
| 12 | N2 Task & Kanban | Task Detail | Da hoan thanh co ban | Co `TaskDetailModal`; route rieng `/tasks/:taskId` chua co trong router. |
| 13 | N3 Comment & Notify | Comment Panel | Da hoan thanh mot phan | Co comment theo task, create/update/delete API. Project-level comment va reply comment chua hoan thien. |
| 14 | N3 Comment & Notify | Mention Member | Da hoan thanh mot phan | API comment nhan `mentionedUserIds`; FE can nang cap autocomplete `@ten` chuan hon theo member project. |
| 15 | N3 Comment & Notify | Notification Page | Da hoan thanh co ban | Co notification page/dropdown, unread count, mark read/read all API. Filter theo project/task can bo sung. |
| 16 | N3 Comment & Notify | Notification Dropdown | Da hoan thanh | Topbar co chuong thong bao, unread count, dropdown preview. |
| 17 | N3 Comment & Notify | Activity Log | Con thieu mot phan | API project/task activity hoat dong 200, nhung E2E cho thay `activityCount = 0`; can ghi log cho create/move task, add member, comment. |
| 18 | Tich hop | Dong bo FE/BE, layout, dark mode | Da hoan thanh co ban | Shared layout, Tailwind, light/dark mode, FE dung API that. N2 dung data N1; N3 dung user/task. Realtime chua co. |

## 4. Danh sach theo service

### NotifyService.Api

Da co:

- Dang ky, dang nhap JWT, lay user hien tai.
- Quan ly users co ban.
- Task comments.
- Notifications, unread count, mark read.
- Activity log endpoints.

Can cai thien:

- Doi mat khau, refresh token, logout/revoke token.
- Profile update cho user hien tai.
- Project-level comments.
- Reply comments.
- Mention autocomplete endpoint hoac FE filter theo project members.
- Realtime notification qua SignalR/WebSocket.
- Ghi activity log cho cac hanh dong Project/Task/Member/Comment.

Loi da phat hien va da xu ly:

- Trung route `api/notifications` giua 2 controller.
- Trung route `api/users` giua 2 controller.
- Trung route `api/comments` giua 2 controller.
- `NotifyDbContext` chua dang ky DI cho mot so controller.
- Schema cu thieu cot ActivityLogs/Comments/Notifications, da co startup patch.

### ProjectService

Da co:

- CRUD projects.
- Template project.
- Auto tao sprints/milestones tu template.
- Members CRUD.
- Role assignment.
- Authorization theo role Owner/Manager.
- Publish event `project.created` cho TaskService tao Kanban columns.

Can cai thien:

- API dashboard summary rieng neu muon giam FE aggregation.
- API timeline tong hop project/sprint/milestone/task.
- Dong bo enum role trong comment/model: Owner, Project Manager, Developer, Tester, Viewer.
- Edit project UI can ro hon.
- Members overview route `/members`.

### TaskService

Da co:

- Tasks CRUD.
- Move task giua columns.
- Kanban columns CRUD.
- Subtasks va time logs API.
- Task filter theo project/column/assignee/sprint.
- Default columns theo template, gom Backlog, To Do, In Progress, Review, Testing, Done.

Can cai thien:

- Drag/drop UI that neu can thao tac keo tha dung nghia.
- Task list page rieng.
- Task detail route rieng.
- Assignment history display.
- Schedule endpoint rieng cho timeline/Gantt drag-drop.

Loi da phat hien va da xu ly:

- TaskDB cu thieu cot `SubTasks.Description`, `AssignedTo`, `Status`, `UpdatedAt`, `TaskTimeLogs.LoggedBy`, `LoggedDate`, `CreatedAt`; da them startup schema patch.

### Frontend

Da co:

- Login/register, token storage, route guard, auto clear session khi token het han.
- Dashboard dung API that.
- Project list, create project, template, initial members/roles.
- Project detail tabs: overview, members, sprints, timeline, kanban, activity.
- Kanban page dung TaskService.
- Calendar/timeline view.
- Notifications page/dropdown.
- Settings service health.
- Light/dark mode.
- Responsive co ban.

Can cai thien:

- Members overview page rieng.
- Project edit form.
- Task list page rieng.
- Task detail route rieng.
- Project-level comments.
- Error/loading/empty states can tiep tuc lam sau hon cho tung man hinh.
- Code splitting de giam warning chunk lon.

## 5. Kiem thu he thong

Kich ban API E2E da chay:

- Dang ky 4 tai khoan: manager, developer, tester, viewer.
- Dang nhap manager.
- Tao project `He thong quan ly du an va phan cong cong viec`.
- Chon template `software-dev`.
- Them 3 thanh vien.
- Doi role developer thanh manager.
- Lay members/sprints/milestones.
- Kiem tra Kanban columns.
- Tao 2 task.
- Move task sang In Progress va Done.
- Tao comment co mention.
- Lay comments/notifications/unread/activity/tasks.

Ket qua E2E API:

```json
{
  "projectId": "89a422be-bbfd-4475-ad7c-0ceb06ff57a2",
  "members": 4,
  "sprints": 3,
  "milestones": 3,
  "columns": ["Backlog", "To Do", "In Progress", "Review", "Testing", "Done"],
  "tasks": 2,
  "comments": 1,
  "notifications": 1,
  "unread": 1,
  "activity": 0
}
```

Kich ban FE browser audit da chay:

- Dashboard: pass.
- Projects: pass.
- Project Detail: pass.
- Project Detail Kanban tab: pass.
- Kanban page: pass.
- Calendar: pass.
- Notifications: pass.
- Settings: pass.
- Mobile dashboard viewport 390x844: pass.
- Console/API errors: `BAD_COUNT 0`.
- Dark mode toggle: pass, `DARK_TOGGLE_OK true`.

Screenshots trong `src/frontend/dist/`:

- `audit-dashboard.png`
- `audit-projects.png`
- `audit-project-detail.png`
- `audit-kanban.png`
- `audit-calendar.png`
- `audit-notifications.png`
- `audit-settings.png`
- `audit-mobile-dashboard.png`
- `audit-dark-mode.png`

## 6. Loi con lai va huong xu ly

1. Activity log chua ghi nhan cac hanh dong E2E.
   - Huong xu ly: publish/consume event cho `member.added`, `task.created`, `task.moved`, `comment.created`; ghi vao `ActivityLogs`.

2. Project-level comments chua hoan thien.
   - Huong xu ly: them `GET/POST /api/comments/project/{projectId}` hoac mo rong CommentService dung `ProjectId` khong bat buoc `TaskId`.

3. Realtime notifications chua co.
   - Huong xu ly: them SignalR Hub trong NotifyService va FE subscribe.

4. Members overview page chua co route rieng.
   - Huong xu ly: them `/members`, dung `GET /api/users` va filter/search/role badges.

5. Task list page va task detail route rieng chua co.
   - Huong xu ly: them `/projects/:id/tasks` va `/tasks/:taskId`, tai su dung TaskDetailModal/service.

6. Warning FE chunk lon.
   - Huong xu ly: lazy-load routes bang dynamic import trong router.

## 7. Danh gia tong quan

He thong da du dieu kien demo luong chinh truoc giang vien:

- FE/BE chay that.
- Co auth JWT.
- Co project/member/role/template.
- Co sprint/milestone/timeline.
- Co task/kanban/move task.
- Co comment/notification.
- Co dashboard va dark mode.

Chua phai san pham hoan chinh 100% theo toan bo mo ta mo rong, vi con thieu activity log day du, project comments, realtime notifications, task list route va members overview route. Tuy nhien phan loi nghiem trong trong runtime/API da duoc xu ly trong dot audit nay.
