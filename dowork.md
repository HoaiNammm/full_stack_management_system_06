# Danh sách công việc — Đạt 100% khớp thiết kế nghiệp vụ

> Bám theo 21 quy tắc QT đã đối chiếu (60% hiện trạng). Mỗi việc ghi rõ quy tắc giải quyết, file/tầng bị động và định nghĩa hoàn thành (DoD).
> Gộp theo cụm vì nhiều việc dùng chung một thay đổi schema — làm theo cụm sẽ nhanh hơn làm rời rạc.

---

## Cụm 1 — Máy trạng thái Task (giải quyết QT-12, QT-13, QT-15)

Đây là cụm nặng nhất, là phần lõi của nghiệp vụ Kanban.

### 1.1 Thêm trường `Status` vào bảng Tasks, tách khỏi `ColumnId`
- **DoD:** `Tasks` có cột `Status INT` (Backlog=0, ToDo=1, InProgress=2, Review=3, Done=4, Blocked=5). Mỗi `KanbanColumn` có một `Status` mặc định ánh xạ tới; khi task vào cột, `Status` tự đồng bộ theo `Type` của cột.
- **Vì sao tách:** cột là khái niệm hiển thị (tùy biến được), trạng thái là khái niệm nghiệp vụ (mang ràng buộc). Nhập chung hai cái khiến không thể validate.

### 1.2 Định nghĩa bảng cạnh chuyển trạng thái hợp lệ
- **DoD:** một bảng tĩnh (constant hoặc bảng DB) liệt kê các cặp `(từ, đến)` được phép: Backlog→ToDo, ToDo→InProgress, InProgress→Review, Review→Done, bất kỳ trạng thái nào (trừ Done)→Blocked, Blocked→trạng thái trước khi bị chặn.
- **Vì cần lưu "trạng thái trước khi Blocked":** thêm cột `PreviousStatus` vào `Tasks` để khi gỡ chặn, biết quay lại đâu.

### 1.3 Validate bước chuyển ở backend khi gọi `PUT /api/tasks/{id}/column`
- **DoD:** trước khi đổi `ColumnId`/`Status`, kiểm cặp `(statusHiệnTại, statusĐích)` có trong bảng cạnh hợp lệ không. Nếu không → 400, thông báo "Không thể chuyển trực tiếp từ [A] sang [B]".

### 1.4 Chặn chuyển sang Done khi còn sub-task chưa xong
- **DoD:** trước khi cho phép `Status → Done`, đếm `SubTasks` của task có `Status=0` (chưa làm) và `DeletedAt IS NULL`. Nếu còn → 400, "Còn công việc con chưa hoàn thành".

### 1.5 Roll-up tiến độ phần trăm task cha từ sub-task
- **DoD:** thêm cột `ProgressPercent` (tính toán, không cho nhập tay qua API) vào `Tasks`. Mỗi khi một `SubTask` đổi `Status`, trigger tính lại: `(số sub-task Done / tổng số sub-task) × 100`, cập nhật task cha. Nếu task không có sub-task nào, `ProgressPercent` lấy theo `Status` của chính nó (Done=100, còn lại=0) hoặc để null tùy quyết định UI.

### 1.6 Cập nhật frontend kéo-thả theo trạng thái hợp lệ
- **DoD:** Kanban board chỉ cho kéo thẻ sang cột tương ứng trạng thái hợp lệ; nếu server từ chối, thẻ trả về vị trí cũ (không để UI lệch với dữ liệu thật).

---

## Cụm 2 — Kiểm quyền theo dự án xuyên service (giải quyết QT-07, QT-14)

### 2.1 Xây endpoint nội bộ kiểm quyền ở ProjectService
- **DoD:** `GET /internal/projects/{projectId}/members/{userId}/permission?action={action}` trả về `{ allowed, role }`. Chỉ gọi được trong mạng nội bộ (không qua Gateway công khai).
- **Danh sách `action` cần định nghĩa:** `task.create`, `task.update`, `task.delete`, `task.status.change`, `subtask.write`, `timelog.write`, `kanban.column.write`, tối thiểu chừng đó cho TaskService.

### 2.2 Middleware/Filter gọi kiểm quyền ở TaskService
- **DoD:** mọi endpoint ghi dữ liệu (POST/PUT/DELETE) của TaskService đi qua một filter chung gọi bước 2.1 trước khi vào logic xử lý. Nếu ProjectService không phản hồi hoặc trả `allowed=false` → 403, không xử lý tiếp (fail-closed).

### 2.3 Cache ngắn hạn kết quả quyền + vô hiệu hóa qua sự kiện
- **DoD:** TaskService cache `(userId, projectId) → role` trong bộ nhớ, TTL khoảng 30-60 giây. ProjectService phát sự kiện `project.member.role.changed` và `project.member.removed`; TaskService lắng nghe và xóa cache tương ứng ngay khi nhận.

### 2.4 Áp dụng lại ma trận quyền mục 7 dựa trên kết quả thật từ ProjectService
- **DoD:** rà lại từng dòng "Tạo/sửa/xóa task", "sub-task", "time log", "cột Kanban" — đảm bảo role dùng để so sánh là role trả về từ bước 2.1, không phải `Role` toàn cục trong JWT.

### 2.5 Viết test xuyên service cho trường hợp bypass
- **DoD:** test gọi thẳng API với JWT của một Viewer, xác nhận `POST /api/tasks` trả 403 dù không qua UI.

---

## Cụm 3 — Vòng đời dự án đầy đủ (giải quyết QT-03, QT-04, QT-25, QT-27)

### 3.1 Soft-delete cho Project
- **DoD:** thêm cột `DeletedAt DATETIME2 NULL` vào `Projects`. `DELETE /api/projects/{id}` không xóa bản ghi, chỉ set `DeletedAt`. Mọi truy vấn danh sách dự án thêm điều kiện `DeletedAt IS NULL`.

### 3.2 Thêm trạng thái Archived và hai thao tác Lưu trữ / Khôi phục
- **DoD:** mở rộng `Status` thêm giá trị Archived. Hai endpoint mới: `PUT /api/projects/{id}/archive`, `PUT /api/projects/{id}/restore`, chỉ Owner gọi được, kiểm trạng thái nguồn hợp lệ trước khi chuyển (Active→Archived, Archived→Active).

### 3.3 Khóa ghi dữ liệu khi dự án Archived hoặc Deleted
- **DoD:** ProjectService phát `project.archived` / `project.deleted` / `project.restored`. TaskService và NotifyService lắng nghe, tự chặn các request ghi (tạo/sửa task, comment...) cho `projectId` đang ở trạng thái không cho ghi — kiểm ở chính bước 2.2 (filter quyền), không cần thêm tầng riêng.

### 3.4 Chức năng chuyển quyền sở hữu dự án
- **DoD:** endpoint `PUT /api/projects/{id}/transfer-owner` nhận `newOwnerId`. Kiểm người nhận là thành viên hiện hữu. Gán người nhận role=Owner, hạ Owner cũ xuống Manager (không gỡ khỏi dự án). Phát sự kiện `project.owner.changed`.

---

## Cụm 4 — Validation backend còn thiếu (giải quyết QT-01, QT-11, QT-23, F-08, F-09)

### 4.1 Kiểm `startDate ≥ hôm nay` ở backend cho tạo dự án và tạo sprint
- **DoD:** so sánh với giờ server (UTC), không tin giá trị client gửi. Áp QT-23: chỉ chặn khi tạo mới hoặc khi người dùng đổi `startDate` hiện hữu sang một mốc quá khứ mới — không chặn khi sửa các trường khác của một dự án có `startDate` vốn đã ở quá khứ.

### 4.2 Kiểm tên dự án duy nhất theo Owner
- **DoD:** thêm chỉ mục `UNIQUE (CreatedBy, Name)` hoặc kiểm tra truy vấn trước khi insert; trả lỗi "Bạn đã có một dự án trùng tên này".

### 4.3 Chống double-submit khi tạo dự án/sprint/task
- **DoD:** disable nút Lưu ngay sau lần bấm đầu (frontend); cân nhắc thêm idempotency key ở server cho các action quan trọng.

### 4.4 Kiểm hạn chót task nằm trong khoảng sprint (nếu có gắn sprint)
- **DoD:** khi `SprintId` được gán, kiểm `Task.DueDate ≤ Sprint.EndDate`. Báo lỗi "Hạn chót vượt quá thời gian sprint".

### 4.5 Kiểm sprint nằm trong vòng đời dự án
- **DoD:** khi tạo/sửa sprint, kiểm `Sprint.StartDate ≥ Project.StartDate` và (nếu `Project.EndDate` có) `Sprint.EndDate ≤ Project.EndDate`.

### 4.6 Kiểm task gắn sprint phải cùng dự án (làm tường minh, không chỉ suy luận từ cấu trúc)
- **DoD:** thêm kiểm tra rõ ràng `Sprint.ProjectId == Task.ProjectId` khi gán `SprintId` cho task, trả lỗi nếu sai.

---

## Cụm 5 — Nghiệp vụ phân công và cộng tác (giải quyết QT-17, F-05, F-06)

### 5.1 Kiểm người được gán task thuộc dự án
- **DoD:** khi set `AssignedTo`, gọi ProjectService kiểm người này có trong `Members` của `ProjectId` không. Báo lỗi "Người phụ trách phải là thành viên dự án".

### 5.2 Kiểm @mention chỉ trong phạm vi thành viên dự án
- **DoD:** trước khi tạo `CommentMention`, lọc `mentionedUserIds` chỉ giữ những người thuộc dự án chứa task/comment đó (gọi ProjectService hoặc cache thành viên). Người ngoài dự án bị nhắc thì bỏ qua, không tạo thông báo.

### 5.3 Sửa schema Comment cho bình luận cấp dự án
- **DoD:** cho `Comments.TaskId` nullable; thêm ràng buộc CHECK đảm bảo `TaskId IS NOT NULL OR ProjectId IS NOT NULL` (một bình luận phải gắn vào ít nhất một trong hai).

### 5.4 Khử trùng lặp thông báo
- **DoD:** khi một sự kiện (vd `task.status.changed`) sinh thông báo cho nhiều vai trò liên quan tới cùng một người (vừa assignee vừa được mention), chỉ tạo một bản ghi `UserNotification` cho người đó.

---

## Cụm 6 — Bảo mật và hoàn thiện (giải quyết F-10, F-12, F-13)

### 6.1 Nâng chính sách mật khẩu, bắt buộc thay vì khuyến nghị
- **DoD:** tối thiểu 8 ký tự, bắt buộc kết hợp chữ và số, enforce ở server (không chỉ hiển thị độ mạnh ở UI).

### 6.2 Thông báo đăng nhập sai không phân biệt nguyên nhân
- **DoD:** cả hai trường hợp "email không tồn tại" và "sai mật khẩu" trả cùng một thông báo "Email hoặc mật khẩu không đúng", cùng một HTTP status.

### 6.3 Đảm bảo mọi thao tác Task phát sự kiện ghi ActivityLog
- **DoD:** rà từng endpoint ghi của TaskService (tạo, sửa, di chuyển, xóa task/sub-task), xác nhận mỗi cái có publish event hoặc gọi NotifyService để ghi log. Bổ sung chỗ còn thiếu.

---

## Bảng tổng hợp theo quy tắc QT

| QT | Việc giải quyết |
|---|---|
| QT-01 | 4.2, 4.3 |
| QT-03 | 3.2 |
| QT-04 | 3.2, 3.3 |
| QT-07 | 2.1, 2.2, 2.4 |
| QT-10 | 4.6 |
| QT-11 | 4.4, 4.5 |
| QT-12 | 1.1, 1.2, 1.3, 1.6 |
| QT-13 | 1.4 |
| QT-14 | 2.4 |
| QT-15 | 1.5 |
| QT-17 | 5.2 |
| QT-18 | 5.4 |
| QT-23 | 4.1 |
| QT-25 | 3.1 |
| QT-27 | 3.4 |

---

## Khuyến nghị về phạm vi thực hiện

21 việc trên là danh sách đầy đủ để chạm 100% theo đúng yêu cầu. Nhưng trước khi bạn lập kế hoạch thời gian, ba điều cần cân nhắc:

**Không phải mọi điểm phần trăm có giá trị ngang nhau.** Cụm 1 và Cụm 2 (12 việc) chiếm gần hết trọng số nghiệp vụ thật — đây là phần phân biệt "có Kanban" với "có nghiệp vụ Kanban", và phần phân biệt "có phân quyền" với "phân quyền chỉ trên giao diện". Cụm 4, 5, 6 (15 việc còn lại) là các validation rời rạc — cần thiết nhưng không việc nào trong đó, nếu thiếu một mình, làm sụp toàn bộ tính đúng đắn của hệ thống.

**Nếu thời gian không đủ làm hết 21 việc**, ưu tiên dừng lại sau khi xong Cụm 1, 2, 3 (17 việc) — đó là lúc bạn đã giải quyết toàn bộ các quy tắc "Chưa đạt" trong bảng đối chiếu trước, tức là đã lên khoảng 90%+ về mặt thực chất nghiệp vụ. Cụm 4, 5, 6 có thể làm một phần và ghi rõ phần còn lại vào "giới hạn của đồ án" trong báo cáo — đây là lựa chọn chuyên nghiệp hơn việc cố nhồi cho đủ 100% rồi làm ẩu.

**Việc 1.2 và 2.1-2.3 là rủi ro kỹ thuật cao nhất** vì đụng tới kiến trúc xuyên service (event, cache, đồng bộ). Nên làm sớm trong lộ trình, không để cuối — nếu phát sinh vấn đề kỹ thuật khó lường (ví dụ RabbitMQ không ổn định), bạn còn thời gian xoay sang phương án đơn giản hơn (gọi REST đồng bộ thuần, bỏ cache).

# Bảng đối chiếu quy tắc nghiệp vụ — Hiện trạng hệ thống

> Đối chiếu 21 quy tắc nghiệp vụ cốt lõi (QT) với hiện trạng trong `TAILIEU_NGHIEPVU.md`.
> Thang chấm: **Đạt** = 1 điểm | **Một phần** = 0.5 điểm | **Chưa đạt** = 0 điểm.
> Tổng: 12.5 / 21 ≈ **60%**.

---

## Bảng đối chiếu

| Mã QT | Nội dung quy tắc | Trạng thái | Ghi chú | Việc cần làm |
|---|---|---|---|---|
| QT-01 | Tên dự án bắt buộc, duy nhất | Một phần | Bắt buộc có, duy nhất không | 4.2, 4.3 |
| QT-02 | Ngày kết thúc ≥ ngày bắt đầu | Đạt | Có cả UI lẫn backend | — |
| QT-03 | Chỉ Owner lưu trữ/xóa dự án | Một phần | Có kiểm Owner khi xóa, nhưng chưa có thao tác "lưu trữ" | 3.2 |
| QT-04 | Dự án Archived chỉ đọc | Chưa đạt | Không có trạng thái Archived khóa ghi | 3.2, 3.3 |
| QT-05 | Chỉ Owner/Manager mời, gỡ thành viên | Đạt | Đúng theo ma trận quyền | — |
| QT-06 | Một người một vai trò trong một dự án | Đạt | Cấu trúc bảng Members đảm bảo | — |
| QT-07 | Quyền theo dự án bắt nguồn từ Project Service | Chưa đạt | TaskService không có cơ chế hỏi quyền theo dự án — lỗ hổng nghiêm trọng nhất | 2.1, 2.2, 2.4 |
| QT-08 | Tối đa một sprint Active tại một thời điểm | Đạt | Có kiểm ở backend | — |
| QT-09 | Chỉ Owner/Manager bắt đầu/kết thúc sprint | Đạt | | — |
| QT-10 | Task gắn sprint phải cùng dự án | Một phần | Không kiểm tường minh, chỉ suy luận từ cấu trúc | 4.6 |
| QT-11 | Hạn chót task hợp lệ theo ngày tạo và phạm vi | Một phần | Chỉ kiểm `≥ hôm nay`, chưa kiểm trong khoảng sprint/dự án | 4.4, 4.5 |
| QT-12 | Bước chuyển trạng thái hợp lệ theo máy trạng thái | Chưa đạt | Không có máy trạng thái, chỉ đổi `ColumnId` tự do | 1.1, 1.2, 1.3, 1.6 |
| QT-13 | Không cho Done khi sub-task chưa xong | Chưa đạt | Không có ràng buộc này | 1.4 |
| QT-14 | Chỉ người phụ trách/Manager/Owner đổi trạng thái | Một phần | Có ở ma trận UI, chưa rõ enforce ở backend | 2.4 |
| QT-15 | Tiến độ task cha tự tính từ sub-task | Chưa đạt | Không có roll-up tự động | 1.5 |
| QT-16 | Giờ ghi nhận dương, đúng người, đúng ngày | Đạt | Có đầy đủ ở backend | — |
| QT-17 | Chỉ @mention thành viên dự án | Chưa đạt | Không kiểm người được nhắc thuộc dự án | 5.2 |
| QT-18 | Khử trùng lặp thông báo cho cùng người nhận | Một phần | Có tách bảng UserNotifications, chưa rõ có khử trùng lặp | 5.4 |
| QT-23 | Ràng buộc ngày quá khứ chỉ áp khi tạo mới / đổi sang quá khứ | Một phần | Có ở UI, thiếu hoàn toàn ở backend | 4.1 |
| QT-25 | Xóa dự án là xóa mềm | Chưa đạt | Đang xóa cứng, gây dữ liệu mồ côi xuyên service | 3.1 |
| QT-26 | Mỗi dự án luôn có đúng một Owner | Đạt | Có chặn xóa Owner cuối cùng | — |
| QT-27 | Chuyển quyền sở hữu, Owner cũ thành Manager | Chưa đạt | Chưa có chức năng chuyển quyền sở hữu | 3.4 |

---

## Tổng hợp theo mức độ

| Trạng thái | Số lượng | Tỷ lệ |
|---|---|---|
| Đạt | 9 | 43% |
| Một phần | 7 | 33% |
| Chưa đạt | 5 | 24% |

**Điểm quy đổi:** (9 × 1) + (7 × 0.5) + (5 × 0) = 12.5 / 21 ≈ **60%**

---

## Ba cụm gây mất điểm nhiều nhất

| Cụm | Các QT liên quan | Số điểm có thể lấy lại |
|---|---|---|
| Máy trạng thái Kanban | QT-12, QT-13, QT-15 | 3.0 điểm (cả 3 đều "Chưa đạt") |
| Kiểm quyền xuyên service | QT-07, QT-14 | 1.5 điểm |
| Vòng đời dự án | QT-03, QT-04, QT-25, QT-27 | 3.0 điểm |

Làm xong ba cụm này (tương ứng Cụm 1, 2, 3 trong danh sách công việc) đưa tổng điểm từ 12.5 lên khoảng 19/21 ≈ **90%**, mà không cần chạm tới các quy tắc còn lại.

