## Thứ TỰ LÀN BTL

1. LoginHistories(Auth / User)

Login thành công/thất bại → lưu LoginHistories
1.1 User

    Mục tiêu
    - Tạo user vào bảng Users
    - Password được hash bằng BCrypt
    - Login đọc user từ DB
    - Login trả JWT token

    API cần có
    - POST /api/auth/register
    - POST /api/auth/login
    - GET  /api/auth/me

    Kiểm tra
    - Register có lưu vào Users không?
    - PasswordHash có phải dạng BCrypt không?
    - Login có đọc từ Users không?
    - Login đúng thì có token không?

1.2 LoginHistories

    Mục tiêu
    - Login thành công thì ghi IsSuccess = true
    - Login sai mật khẩu thì ghi IsSuccess = false
    - Lưu IP, UserAgent, thời gian login

    API cần có
    - GET /api/auth/login-histories

1.3 RefreshTokens

    Mục tiêu
    Khi login thành công:
    - Trả về Access Token như hiện tại
    - Tạo thêm Refresh Token
    - Lưu Refresh Token vào bảng RefreshTokens
    - Sau này dùng Refresh Token để xin Access Token mới

    Luồng hoạt động
    Login đúng
    → Backend trả accessToken + refreshToken
    → AccessToken dùng gọi API
    → RefreshToken lưu để xin token mới khi accessToken hết hạn

1.4 PasswordResetTokens
Mục tiêu
Luồng hoạt đông
User nhập email
→ Email tồn tại
→ Backend tạo reset token
→ Lưu vào bảng PasswordResetTokens
→ User dùng reset token để đặt mật khẩu mới

    API cần có
    - POST /api/auth/forgot-password
    - POST /api/auth/reset-password

## KẾT QUẢ

    - Tạo và quản lý user trong bảng Users.
    - Đăng nhập bằng email/password từ database.
    - Mật khẩu được mã hóa bằng BCrypt.
    - Login thành công trả về JWT access token và refresh token.
    - Lưu lịch sử đăng nhập vào bảng LoginHistories.
    - Lưu refresh token vào bảng RefreshTokens.
    - Hỗ trợ luồng forgot password/reset password thông qua PasswordResetTokens.

2. Comments
   2.1 Comments
   2.2 CommentMentions
   2.3 CommentAttachments
   2.4 ActivityLogs

- Thứ tự thực hiện
    Làm API tạo comment vào task
    - OST /api/comments
    - tạo bình luận vào task

    API lấy comment theo task
    - GET /api/comments/task/{taskId}
    
    Lấy danh sách bình luận của một task 
    - PUT /api/comments/{commentId}
    - Chỉ người tạo comment mới được sửa comment
    - Không sửa comment đã bị xóa mềm
    - Cập nhật Content
    - Cập nhật UpdatedAt


   3 API sửa comment  
   4 API xóa mềm comment
   5 Khi tạo/sửa/xóa comment thì ghi ActivityLogs
   6 Sau đó mới xử lý Mentions và Attachments

3. ActivityLogs
4. Notifications + UserNotifications
5. NotificationLogs
6. IncomingEvents + EventProcessingLogs
7. AuditLogs / SystemLogs
