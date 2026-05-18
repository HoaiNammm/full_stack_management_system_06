## Comment Model
- **Id**: Guid (Primary Key) - ID tự sinh của bình luận.
- [cite_start]**TaskId**: Guid - ID của Task được bình luận (Plain column, không tạo FK sang TaskDB)[cite: 9, 128].
- [cite_start]**UserId**: Guid - ID của người bình luận (Lấy từ JWT token sau này)[cite: 6, 11].
- **Content**: String - Nội dung bình luận.
- [cite_start]**CreatedAt**: DateTime (UTC) - Thời gian tạo.