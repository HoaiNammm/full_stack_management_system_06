# Phân công & Tường thuật công việc — TaskService
## 2. Đoàn: Quản lý Task & Thống kê

**Xem danh sách task theo dự án**: cho phép lọc theo trạng thái (đang làm, xong, tồn đọng...) và theo
độ ưu tiên, để màn hình Kanban chỉ hiển thị đúng cột cần xem. Sau khi lấy được danh sách task,  

**Tạo task mới**: sau khi kiểm tra quyền xong, lưu task với đầy đủ tiêu đề, mô tả, loại việc, độ ưu
tiên, nhãn dán, người được giao, hạn chót, số giờ dự kiến. 

**Cập nhật task**:Trước khi sửa, lưu lại trạng thái cũ và người phụ
trách cũ. 

**Xoá task**: ghi log lại là "task này vừa bị xoá" trước, rồi mới xoá thật, cuối cùng báo cho hệ thống biết.
Ghi log trước xoá vì sau khi xoá thì thông tin task không còn nữa để tham chiếu.

**Thống kê task theo dự án**:dùng để đếm xem dự án có bao nhiêu task ở mỗi trạng thái, và bao nhiêu task đã quá
hạn mà vẫn chưa xong

**Task của tôi**: tạo một API riêng để mỗi người xem nhanh những task nào đang được giao cho chính mình, sắp xếp
theo hạn chót gần nhất lên trước, để không ai bỏ sót việc gấp.

**Nhật ký hoạt động (Activity Log)**: 
mỗi khi có ai tạo task, đổi trạng thái, xoá task, hay thêm bình luận, hệ thống đều tự động ghi một dòng nhật
ký. 

---

## 3. Phương: Việc con (Subtask) & Chấm công (Time Log)

**Việc con (Subtask)**: ý tưởng là một task lớn có thể chẻ thành nhiều đầu việc nhỏ, giống như một checklist.
 cho xem danh sách việc con theo đúng thứ tự tạo trước sau, cho tạo mới (chỉ cần tiêu đề và có thể gán
người làm), cho cập nhật (đổi tiêu đề, tick hoàn thành, đổi người làm — cái nào gửi lên mới đổi cái đó), và
cho xoá. 

**Chấm công (Time Log)**: mỗi lần một ai đó làm xong một khoảng thời gian cho task, họ có thể ghi lại: làm gì,
mất bao nhiêu giờ, và ghi vào lúc nào (nếu không chọn thời điểm thì mặc định là ngay bây giờ). để ý nhất là quyền xoá: một bản ghi giờ làm chỉ có chính người đã tạo ra
nó mới được xoá, người khác — kể cả người phụ trách task — cũng không được đụng vào, để tránh việc sửa giờ
công của người khác.

---

## 4. Hoài: Bình luận & Kết nối liên service

**Bình luận (Comment)**:chỉ tác giả mới được sửa hoặc xoá bình luận của chính mình, người khác không có
quyền động vào.

**Gắn thẻ & thông báo (Mention)**:Khi ai đó viết bình luận và gắn thẻ
(@) một hoặc nhiều người khác, lọc ra danh sách những người được nhắc tới,Nếu bình luận dài, mình chỉ trích một đoạn
ngắn kèm dấu ba chấm cho gọn, không gửi nguyên văn dài dòng.

---
## 5. Tổng kết 

Đoàn lo phần lõi (task, trạng thái, thống kê, nhật
ký), Phương lo phần chi tiết đi kèm (việc con, giờ công), Hoài lo phần con người tương tác với nhau và với các
service khác (bình luận, thông báo, sự kiện).
