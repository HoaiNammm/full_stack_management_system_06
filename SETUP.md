# Hướng dẫn Setup & Chạy hệ thống

## Kiến trúc hệ thống

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  ProjectService  │     │   TaskService    │     │  NotifyService   │
│  :5047  (Máy A)  │     │  :5217  (Máy B)  │     │  :5177  (Máy C)  │
└────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘
         │                        │                        │
         └────────────────────────┴────────────────────────┘
                                  │ RabbitMQ (chạy trên 1 máy)
                          ┌───────┴───────┐
                          │   Frontend    │
                          │  :5173 (Máy D)│
                          └───────────────┘
```

| Vai trò | Service | Port | Database |
|---|---|---|---|
| Máy A | ProjectService | 5047 | ProjectDB |
| Máy B | TaskService | 5217 | TaskDB |
| Máy C | NotifyService | 5177 | NotifyDB |
| Máy D | Frontend (Vue) | 5173 | — |

> **RabbitMQ**: Cần chạy trên **1 máy duy nhất** (khuyến nghị Máy A hoặc máy riêng).  
> Tất cả service phải cấu hình trỏ đến IP của máy chạy RabbitMQ.

---

## Yêu cầu chung cho mọi máy Back-end (A, B, C)

| Phần mềm | Phiên bản | Link |
|---|---|---|
| .NET SDK | **8.0** (TaskService, ProjectService) / **9.0** (NotifyService) | https://dotnet.microsoft.com/download |
| SQL Server | 2019/2022/Express | https://www.microsoft.com/sql-server/sql-server-downloads |
| Git | Bất kỳ | https://git-scm.com |

Kiểm tra sau khi cài:
```powershell
dotnet --version   # 8.0.x hoặc 9.0.x
git --version
```

---

## Cài RabbitMQ (Chỉ 1 máy — khuyến nghị Máy A)

1. Cài **Erlang** trước: https://www.erlang.org/downloads  
2. Cài **RabbitMQ**: https://www.rabbitmq.com/docs/install-windows  
3. Bật Management UI (tùy chọn, tiện theo dõi):
   ```powershell
   rabbitmq-plugins enable rabbitmq_management
   # Truy cập: http://localhost:15672  (guest/guest)
   ```
4. Mở port **5672** trên firewall của máy này để các máy khác kết nối được:
   ```powershell
   # Chạy với quyền Admin
   netsh advfirewall firewall add rule name="RabbitMQ" dir=in action=allow protocol=TCP localport=5672
   ```
5. Ghi lại IP của máy này (ví dụ: `192.168.1.10`) — các máy khác sẽ cần dùng IP này.

---

## Bước chung cho mọi máy Back-end

```powershell
# 1. Clone repo
git clone <repo-url>
cd full_stack_management_system_06

# 2. Checkout nhánh develop
git checkout develop
git pull origin develop

# 3. Cài EF Tools (1 lần duy nhất)
dotnet tool install --global dotnet-ef
```

---

## Máy A — ProjectService (port 5047)

### 1. Sửa `appsettings.json`

File: `src/ProjectService/ProjectService/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectDB;Integrated Security=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "BrOk7FswV4wrTKJ4DOlPQ1psFMgTkNMo",
    "Issuer": "project-management-system",
    "Audience": "all-services",
    "ExpirationHours": 24
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

> Nếu RabbitMQ ở máy khác, đổi `"Host": "localhost"` → `"Host": "<IP máy chạy RabbitMQ>"`.  
> Nếu SQL Server dùng instance name, đổi `Server=localhost` → `Server=localhost\SQLEXPRESS` (tùy edition).

### 2. Build, migrate, chạy

```powershell
cd src/ProjectService/ProjectService

dotnet restore
dotnet ef database update
dotnet run
```

### 3. Kiểm tra

- Swagger: http://localhost:5047/swagger
- Mở port 5047 trên firewall nếu máy khác cần gọi vào:
  ```powershell
  netsh advfirewall firewall add rule name="ProjectService" dir=in action=allow protocol=TCP localport=5047
  ```

---

## Máy B — TaskService (port 5217)

### 1. Sửa `appsettings.json`

File: `src/TaskService/TaskService/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskDB;Integrated Security=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "BrOk7FswV4wrTKJ4DOlPQ1psFMgTkNMo",
    "Issuer": "project-management-system",
    "Audience": "all-services",
    "ExpirationHours": 24
  },
  "RabbitMQ": {
    "Host": "<IP máy chạy RabbitMQ>",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Services": {
    "ProjectServiceUrl": "http://<IP Máy A>:5047"
  }
}
```

> **Quan trọng**: `Services.ProjectServiceUrl` phải trỏ đúng IP/port của Máy A.

### 2. Build, migrate, chạy

```powershell
cd src/TaskService/TaskService

dotnet restore
dotnet ef database update
dotnet run
```

### 3. Kiểm tra

- Swagger: http://localhost:5217/swagger
- Mở port 5217:
  ```powershell
  netsh advfirewall firewall add rule name="TaskService" dir=in action=allow protocol=TCP localport=5217
  ```

---

## Máy C — NotifyService (port 5177)

### 1. Sửa `appsettings.json`

File: `src/NotifyService.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NotifyDB;Integrated Security=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "BrOk7FswV4wrTKJ4DOlPQ1psFMgTkNMo",
    "Issuer": "project-management-system",
    "Audience": "all-services",
    "ExpirationHours": 24
  },
  "RabbitMQ": {
    "Host": "<IP máy chạy RabbitMQ>",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### 2. Build, migrate, chạy

NotifyService có **2 DbContext**, cần migrate cả hai:

```powershell
cd src/NotifyService.Api

dotnet restore
dotnet ef database update --context NotifyDbContext
dotnet ef database update --context AppDbContext
dotnet run
```

### 3. Kiểm tra

- Swagger: http://localhost:5177/swagger
- Mở port 5177:
  ```powershell
  netsh advfirewall firewall add rule name="NotifyService" dir=in action=allow protocol=TCP localport=5177
  ```

---

## Máy D — Frontend (Vue 3 + Vite)

### Yêu cầu

| Phần mềm | Phiên bản |
|---|---|
| Node.js | **20.x** hoặc **22.x** |
| npm | đi kèm Node |

Kiểm tra: `node --version`  
Link cài: https://nodejs.org

### 1. Sửa `.env.local`

File: `src/frontend/.env.local`

```env
VITE_PROJECT_API=http://<IP Máy A>:5047/api
VITE_TASK_API=http://<IP Máy B>:5217/api
VITE_NOTIFY_API=http://<IP Máy C>:5177/api
```

Thay `<IP Máy A/B/C>` bằng IP thực của từng máy.  
Nếu chạy tất cả trên cùng máy dùng `localhost`.

### 2. Giải quyết conflict trong code (nếu có)

Repo có một số file bị merge conflict chưa giải quyết:

```powershell
# Kiểm tra file conflict
git diff --name-only --diff-filter=U

# Sau khi sửa xong conflict:
git add .
git commit -m "resolve merge conflicts"
```

### 3. Cài dependencies và chạy

```powershell
cd src/frontend

npm install
npm run dev
```

Frontend chạy tại: http://localhost:5173

---

## Lưu ý quan trọng

### JWT Secret phải giống nhau trên tất cả máy

Tất cả 3 service dùng cùng `Jwt.Secret`. Nếu thay đổi thì phải đổi đồng bộ cả 3 file `appsettings.json`.

### `Integrated Security=true` yêu cầu Windows Authentication

Connection string đang dùng Windows Auth — SQL Server phải cài trên cùng máy và user Windows phải có quyền.  
Nếu dùng SQL Server ở máy khác hoặc dùng SQL Authentication, đổi thành:

```
Server=<IP>;Database=<DB>;User Id=sa;Password=<pass>;TrustServerCertificate=True
```

### RabbitMQ — chỉ 1 máy chạy, các máy còn lại kết nối qua IP

Tất cả service phải chung 1 RabbitMQ để events được xử lý đúng.

### Thứ tự khởi động khuyến nghị

```
1. Máy chạy RabbitMQ → khởi động RabbitMQ trước
2. Máy A → ProjectService
3. Máy B → TaskService
4. Máy C → NotifyService
5. Máy D → Frontend
```

---

## Bảng tóm tắt IP cần điền (điền vào đây khi biết)

| Máy | Vai trò | IP thực tế |
|---|---|---|
| Máy A | ProjectService | `___________` |
| Máy B | TaskService | `___________` |
| Máy C | NotifyService | `___________` |
| Máy RabbitMQ | RabbitMQ | `___________` |
