# Huong dan thuyet trinh voi giang vien

## 1. Mo dau

Gioi thieu ngan gon:

> De tai cua nhom la xay dung he thong quan ly du an va phan cong cong viec. He thong ho tro dang ky, dang nhap, tao du an, quan ly thanh vien, phan quyen, sprint, milestone, task, Kanban, binh luan, thong bao va dashboard thong ke.

Gia tri chinh:

- Quan ly du an tap trung.
- Phan cong cong viec ro rang.
- Theo doi tien do bang Kanban va Dashboard.
- Ho tro cong tac nhom qua comment, mention va notification.
- Giao dien hien dai, co dark mode.

## 2. Kien truc he thong

Trinh bay so do logic:

```text
Vue 3 Frontend
   |
   | JWT + Axios
   |
   +-- NotifyService.Api
   |      Auth, Users, Comments, Notifications, Activity Logs
   |
   +-- ProjectService
   |      Projects, Members, Roles, Templates, Sprints, Milestones
   |
   +-- TaskService
          Tasks, Kanban Columns, Subtasks, Time Logs

SQL Server: luu du lieu tung service
RabbitMQ: dong bo event project.created de TaskService tao Kanban columns
```

Noi khi thuyet trinh:

- Frontend dung Vue 3, Vue Router, Pinia, Axios, TailwindCSS.
- Backend tach theo service de ro trach nhiem.
- JWT duoc dung de xac thuc lien service.
- FE khong dung mock data cho cac API da co.

## 3. Chuc nang tung service

### ProjectService

- Tao/xem/sua/xoa du an.
- Tao project theo template.
- Tu dong tao sprint va milestone.
- Quan ly thanh vien trong project.
- Gan role: Owner, Project Manager, Developer, Tester, Viewer.
- Kiem tra quyen:
  - Owner co toan quyen.
  - Manager co quyen quan ly mot phan.
  - Developer/Tester/Viewer bi gioi han theo vai tro.

### TaskService

- Tao task.
- Gan task cho thanh vien.
- Dat priority, deadline, sprint.
- Kanban columns: Backlog, To Do, In Progress, Review, Testing, Done.
- Cap nhat trang thai task bang move column.
- Lay task theo project/sprint/assignee/status.

### NotifyService.Api

- Dang ky, dang nhap, JWT.
- Quan ly user.
- Binh luan theo task.
- Mention thanh vien.
- Tao va hien thi notification.
- Unread count va mark read.
- Activity log endpoint.

## 4. Luong hoat dong

Trinh bay theo 10 buoc:

1. Nguoi dung dang ky tai khoan.
2. Nguoi dung dang nhap he thong.
3. Frontend luu JWT va chuyen vao Dashboard.
4. Nguoi dung tao du an moi.
5. Nguoi dung chon template Software Development.
6. ProjectService tao project, sprint, milestone va owner member.
7. ProjectService phat event `project.created`, TaskService tao Kanban columns.
8. Nguoi dung them thanh vien va gan role.
9. Nguoi dung tao task, gan assignee, deadline, priority.
10. Thanh vien cap nhat trang thai tren Kanban; comment/notification/dashboard duoc cap nhat qua API.

## 5. Demo thuc te tren website

Thu tu demo de tranh lac mach:

1. Dang nhap.
2. Dashboard.
3. Projects.
4. Tao project theo template.
5. Project Detail.
6. Members va role.
7. Sprint va Timeline.
8. Kanban va task.
9. Comment va Notification.
10. Dark mode.

Noi nhanh trong luc demo:

- "Day la du lieu that tu API, khong phai UI tinh."
- "Moi request deu di qua JWT."
- "Khi tao project theo template, backend sinh sprint/milestone va TaskService sinh cot Kanban."
- "Dashboard tong hop tu nhieu service."

## 6. Ket qua dat duoc

Da dat:

- FE/BE chay duoc tren trinh duyet.
- Build backend va frontend thanh cong.
- Dang ky/dang nhap JWT.
- Project CRUD co ban.
- Template project.
- Member va role.
- Sprint/milestone/timeline.
- Task/Kanban.
- Comment/notification.
- Dashboard.
- Dark mode.
- Responsive co ban.

Bang chung test:

- API E2E pass voi project `89a422be-bbfd-4475-ad7c-0ceb06ff57a2`.
- Browser audit pass cac man hinh: dashboard, projects, project detail, kanban, calendar, notifications, settings, mobile dashboard.
- Console/API audit: `BAD_COUNT 0`.

## 7. Han che hien tai

Noi thang, gon:

- Activity log co endpoint nhung chua ghi day du moi action.
- Project-level comment va reply comment chua hoan thien.
- Notification chua realtime bang SignalR/WebSocket.
- Chua co trang `/members` overview rieng.
- Chua co trang `/projects/:id/tasks` task list rieng.
- Chua co route `/tasks/:taskId` rieng, hien tai dung modal.

## 8. Huong phat trien

- Them SignalR cho realtime notification.
- Hoan thien activity logging bang event-driven.
- Bo sung project comments va reply comments.
- Them Gantt chart keo tha ngay neu backend schedule support.
- Them task list page, members overview page.
- Code splitting frontend de giam chunk size.
- Bo sung unit/integration tests tu dong.

## 9. Cau tra loi nhanh khi bi hoi

**He thong co dung microservices khong?**

Co. Source tach thanh ProjectService, TaskService, NotifyService.Api. Moi service co DB context va controller rieng.

**Co dung API that khong?**

Co. FE goi Axios den 3 backend service. Dashboard, project, task, notification deu lay du lieu API.

**Phan quyen nam o dau?**

ProjectService quan ly role trong bang Members. Controller check role truoc khi add/update/delete member va update/delete project.

**Task va Project lien ket the nao?**

Task co `ProjectId`, `ColumnId`, `SprintId`, `AssignedTo`. TaskService kiem tra user co la member project bang ProjectService.

**Thong bao tao the nao?**

NotifyService tao notification khi co comment/mention/assignee theo logic hien co. Unread count hien tren TopBar.

**Con thieu gi de thanh san pham that?**

Can realtime, activity log day du, project comments, task list route, members overview, test automation.
