# Kich ban demo website

Muc tieu demo: Chung minh he thong quan ly du an va phan cong cong viec chay that voi FE + 3 backend service.

## 1. Chuan bi

Chay backend:

```powershell
cd D:\Fullstack\BTL Fullstack
docker compose up -d
docker start pm_rabbitmq
powershell.exe -ExecutionPolicy Bypass -File scripts\start-dev-services.ps1
```

Chay frontend:

```powershell
cd D:\Fullstack\BTL Fullstack\src\frontend
npm run dev -- --host 127.0.0.1
```

Mo website:

```text
http://127.0.0.1:5173
```

Tai khoan co san:

```text
Email: 1@example.com
Password: 123456
```

Project demo da tao san cho tai khoan tren:

```text
Ten project: DEMO THUYET TRINH - He thong quan ly du an va phan cong cong viec
Project ID: ef8b3368-1848-4859-848a-a4c59cab297d
Noi dung demo: 5 thanh vien, 3 sprint, 3 milestone, 6 cot Kanban, 6 task, comment va notification.
```

Tai khoan E2E da tao khi audit:

```text
Email: demo.manager.20260611075252@example.com
Password: 123456
Project demo: He thong quan ly du an va phan cong cong viec
Project ID: 89a422be-bbfd-4475-ad7c-0ceb06ff57a2
```

## 2. Luong demo chinh

### Buoc 1. Dang ky tai khoan

1. Mo trang Register.
2. Nhap ho ten, email, mat khau, xac nhan mat khau.
3. Tao tai khoan.
4. Giai thich: tai khoan duoc tao trong NotifyService, mat khau duoc hash, login bang JWT.

### Buoc 2. Dang nhap

1. Mo Login.
2. Dang nhap bang tai khoan manager.
3. Sau login, he thong chuyen vao Dashboard.
4. Giai thich: FE luu token, gan Authorization header cho ProjectService/TaskService/NotifyService.

### Buoc 3. Xem Dashboard

1. Chi ra cac chi so:
   - Tong du an.
   - Thanh vien.
   - Task.
   - Task hoan thanh/dang thuc hien.
   - Thong bao moi.
2. Giai thich: Dashboard tong hop tu API that, khong dung mock data.

### Buoc 4. Tao du an

1. Vao Projects.
2. Bam `Tao du an moi`.
3. Chon template `Phat trien phan mem / Software Development`.
4. Nhap ten: `He thong quan ly du an va phan cong cong viec`.
5. Nhap mo ta, ngay bat dau, ngay ket thuc, mau nhan dien.
6. O phan `Phan quyen du an`, giai thich:
   - Nguoi tao la Owner.
   - Co the them thanh vien ban dau va gan role.
7. Bam tao du an.
8. Giai thich: ProjectService tao project, owner member, sprint, milestone; publish event de TaskService tao Kanban columns.

### Buoc 5. Them thanh vien va phan quyen

1. Vao Project Detail.
2. Chon tab `Thanh vien`.
3. Them thanh vien bang email.
4. Gan vai tro:
   - Owner.
   - Project Manager.
   - Developer.
   - Tester.
   - Viewer.
5. Doi role mot thanh vien.
6. Giai thich: Owner/Manager moi co quyen them member; Owner moi doi role/xoa member.

### Buoc 6. Xem Sprint va Timeline

1. Chon tab `Sprint`.
2. Chi ra 3 sprint duoc tao tu template.
3. Chon tab `Timeline`.
4. Chi ra project start/end, Sprint 1/2/3, Alpha/Beta/Final Release.

### Buoc 7. Tao task

1. Vao Kanban.
2. Chon project.
3. Tao task o Backlog/To Do.
4. Nhap title, description, assignee, priority, deadline, sprint.
5. Giai thich: TaskService luu task, co lien ket ProjectId, ColumnId, SprintId, AssignedTo.

### Buoc 8. Cap nhat trang thai task

1. Di chuyen task sang In Progress.
2. Di chuyen task khac sang Done.
3. Giai thich: FE goi `PUT /api/tasks/{id}/column`, Dashboard se cap nhat tien do theo columns.

### Buoc 9. Binh luan va thong bao

1. Mo task detail.
2. Them comment co mention thanh vien.
3. Vao Notifications.
4. Chi ra thong bao moi/unread.
5. Giai thich: NotifyService luu comment, tao notification cho mention/assignee.

### Buoc 10. Dark mode va responsive

1. Bam icon `dark_mode` tren TopBar.
2. Giai thich theme duoc luu localStorage.
3. Thu thu nho man hinh hoac mo mobile viewport.
4. Giai thich giao dien responsive.

## 3. Ket qua can noi khi demo

- He thong la microservices don gian gom 3 backend service.
- FE dung chung layout va goi API that.
- ProjectService quan ly du an/thanh vien/role/template/sprint/milestone.
- TaskService quan ly task va Kanban.
- NotifyService quan ly auth/comment/notification.
- Du lieu chay lien thong qua JWT, SQL Server va RabbitMQ.
- Da co flow nghiep vu hoan chinh tu dang ky den dashboard thong ke.

## 4. Luu y neu giang vien hoi

- Activity log hien co endpoint nhung chua ghi day du moi action, can bo sung event logging.
- Realtime notification chua co SignalR/WebSocket, hien tai notification cap nhat qua API.
- Project-level comments va reply comments la huong phat trien tiep.
- Task list route rieng va members overview route rieng chua tach thanh trang doc lap.
