<script setup>
import { onMounted, ref, watch } from "vue";
import { getUsers } from "../services/userService";
import {
  getCommentsByTask,
  createComment,
  updateComment,
  deleteComment,
} from "../services/commentService";

const props = defineProps({
  taskId: {
    type: String,
    required: true,
  },
  projectId: {
    type: String,
    default: null,
  },
});

const users = ref([]);
const comments = ref([]);
const content = ref("");
const selectedMentionIds = ref([]);

const attachmentFileName = ref("");
const attachmentFileUrl = ref("");

const loading = ref(false);
const error = ref("");
const success = ref("");

const editingCommentId = ref(null);
const editingContent = ref("");

const loadUsers = async () => {
  try {
    users.value = await getUsers();
  } catch (err) {
    console.error(err);
    error.value = "Không tải được danh sách user";
  }
};

const loadComments = async () => {
  if (!props.taskId) return;

  loading.value = true;
  error.value = "";

  try {
    comments.value = await getCommentsByTask(props.taskId);
  } catch (err) {
    console.error(err);
    error.value = "Không tải được bình luận";
  } finally {
    loading.value = false;
  }
};

const handleCreateComment = async () => {
  error.value = "";
  success.value = "";

  if (!content.value.trim()) {
    error.value = "Nội dung bình luận không được để trống";
    return;
  }

  const attachments = [];

  if (attachmentFileName.value.trim() && attachmentFileUrl.value.trim()) {
    attachments.push({
      fileName: attachmentFileName.value.trim(),
      fileUrl: attachmentFileUrl.value.trim(),
      contentType: "application/octet-stream",
      fileSize: 0,
    });
  }

  try {
    await createComment({
      taskId: props.taskId,
      projectId: props.projectId,
      content: content.value.trim(),
      mentionedUserIds: selectedMentionIds.value,
      attachments,
    });

    success.value = "Đã gửi bình luận";
    content.value = "";
    selectedMentionIds.value = [];
    attachmentFileName.value = "";
    attachmentFileUrl.value = "";

    await loadComments();
  } catch (err) {
    console.error(err);
    error.value = err.response?.data?.message || "Tạo bình luận thất bại";
  }
};

const startEdit = (comment) => {
  editingCommentId.value = comment.id;
  editingContent.value = comment.content;
};

const cancelEdit = () => {
  editingCommentId.value = null;
  editingContent.value = "";
};

const handleUpdateComment = async (commentId) => {
  error.value = "";
  success.value = "";

  if (!editingContent.value.trim()) {
    error.value = "Nội dung cập nhật không được để trống";
    return;
  }

  try {
    await updateComment(commentId, {
      content: editingContent.value.trim(),
    });

    success.value = "Đã cập nhật bình luận";
    cancelEdit();
    await loadComments();
  } catch (err) {
    console.error(err);
    error.value = err.response?.data?.message || "Cập nhật bình luận thất bại";
  }
};

const handleDeleteComment = async (comment) => {
  const ok = confirm("Bạn có chắc muốn xóa bình luận này?");

  if (!ok) return;

  error.value = "";
  success.value = "";

  try {
    await deleteComment(comment.id);
    success.value = "Đã xóa bình luận";
    await loadComments();
  } catch (err) {
    console.error(err);
    error.value = err.response?.data?.message || "Xóa bình luận thất bại";
  }
};

const getUserName = (userId) => {
  const user = users.value.find((x) => x.id === userId);
  return user ? user.fullName : userId;
};

// Lấy chữ cái đầu làm Avatar
const getInitials = (name) => {
  if (!name) return "U";
  return name.charAt(0).toUpperCase();
};

onMounted(async () => {
  await loadUsers();
  await loadComments();
});

watch(
  () => props.taskId,
  async () => {
    await loadComments();
  }
);
</script>

<template>
  <section class="comment-section">
    <div class="section-header">
      <div class="header-text">
        <h3>Bình luận</h3>
        <p>Trao đổi và ghi nhận hoạt động liên quan đến task.</p>
      </div>

      <button class="refresh-btn" @click="loadComments" title="Tải lại bình luận">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"></polyline><polyline points="1 20 1 14 7 14"></polyline><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path></svg>
        Tải lại
      </button>
    </div>

    <div v-if="error" class="alert error">{{ error }}</div>
    <div v-if="success" class="alert success">{{ success }}</div>

    <div class="comment-form">
      <textarea
        v-model="content"
        rows="3"
        placeholder="Nhập bình luận cho task này..."
      ></textarea>

      <div class="form-row">
        <div class="form-group">
          <label>Mention user</label>
          <select v-model="selectedMentionIds" multiple>
            <option v-for="user in users" :key="user.id" :value="user.id">
              @{{ user.fullName }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>File name</label>
          <input v-model="attachmentFileName" placeholder="report.pdf" />
        </div>

        <div class="form-group">
          <label>File URL</label>
          <input v-model="attachmentFileUrl" placeholder="https://..." />
        </div>
      </div>

      <div class="form-actions">
        <button class="primary-btn" @click="handleCreateComment" :disabled="loading">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="22" y1="2" x2="11" y2="13"></line><polygon points="22 2 15 22 11 13 2 9 22 2"></polygon></svg>
          Gửi bình luận
        </button>
      </div>
    </div>

    <div class="comment-list">
      <div v-if="loading" class="state-message">
        <span class="spinner"></span> Đang tải bình luận...
      </div>

      <div v-else-if="comments.length === 0" class="state-message empty">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
        <p>Chưa có bình luận nào.</p>
      </div>

      <article v-else v-for="comment in comments" :key="comment.id" class="comment-item">
        
        <div class="comment-avatar">
          {{ getInitials(getUserName(comment.userId)) }}
        </div>

        <div class="comment-body">
          <div class="comment-header">
            <div class="user-info">
              <strong class="user-name">{{ getUserName(comment.userId) }}</strong>
              <span class="timestamp">{{ comment.createdAt || "Vừa xong" }}</span>
            </div>

            <div class="actions">
              <button class="icon-btn edit" @click="startEdit(comment)" title="Sửa">
                <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
              </button>
              <button class="icon-btn danger" @click="handleDeleteComment(comment)" title="Xóa">
                <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
              </button>
            </div>
          </div>

          <div v-if="editingCommentId === comment.id" class="edit-box">
            <textarea v-model="editingContent" rows="3" class="edit-textarea"></textarea>
            <div class="edit-actions">
              <button class="secondary-btn" @click="cancelEdit">Hủy</button>
              <button class="primary-btn small" @click="handleUpdateComment(comment.id)">Lưu lại</button>
            </div>
          </div>

          <div v-else>
            <p class="comment-content">{{ comment.content }}</p>

            <div class="comment-footer" v-if="comment.mentionedUserIds?.length || comment.attachments?.length">
              
              <div v-if="comment.mentionedUserIds?.length" class="meta-tags mentions">
                <span v-for="id in comment.mentionedUserIds" :key="id" class="mention-tag">
                  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="4"></circle><path d="M16 8v5a3 3 0 0 0 6 0v-1a10 10 0 1 0-3.92 7.94"></path></svg>
                  {{ getUserName(id) }}
                </span>
              </div>

              <div v-if="comment.attachments?.length" class="meta-tags attachments">
                <a
                  v-for="file in comment.attachments"
                  :key="file.id"
                  :href="file.fileUrl"
                  target="_blank"
                  class="file-tag"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"></path></svg>
                  {{ file.fileName }}
                </a>
              </div>

            </div>
          </div>

        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
/* WRAPPER CHÍNH */
.comment-section {
  background: var(--bg-card, #ffffff);
  color: var(--text-main, #111827);
  padding: 20px 24px;
  font-family: 'Inter', Arial, sans-serif;
  height: 100%;
}

/* HEADER */
.section-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
.header-text h3 { margin: 0; font-size: 18px; font-weight: 700; color: var(--text-main); }
.header-text p { margin: 4px 0 0; font-size: 13px; color: var(--text-muted, #64748b); }

/* NÚT TẢI LẠI */
.refresh-btn { 
  display: flex; align-items: center; gap: 6px; 
  border: 1px solid var(--border-color, #e5e7eb); 
  background: var(--bg-card); color: var(--text-main);
  padding: 8px 12px; border-radius: 10px; font-size: 13px; font-weight: 600; 
  cursor: pointer; transition: all 0.2s ease; 
}
.refresh-btn:hover { border-color: var(--primary, #4f46e5); color: var(--primary, #4f46e5); }

/* FORM NHẬP BÌNH LUẬN */
.comment-form {
  background: var(--bg-body, #f8fafc);
  border: 1px solid var(--border-color, #e5e7eb);
  border-radius: 14px;
  padding: 16px;
  margin-bottom: 24px;
}

textarea, input, select {
  width: 100%; box-sizing: border-box;
  background: var(--input-bg, #ffffff); color: var(--text-main);
  border: 1px solid var(--border-color, #e5e7eb);
  border-radius: 10px; padding: 10px 12px; font-size: 14px; 
  outline: none; transition: border-color 0.2s ease;
  font-family: inherit;
}
textarea { resize: vertical; min-height: 80px; }
textarea:focus, input:focus, select:focus { border-color: var(--primary, #4f46e5); }

.form-row { display: grid; grid-template-columns: 1.5fr 1fr 1fr; gap: 12px; margin-top: 12px; }
.form-group label { display: block; font-size: 13px; font-weight: 600; color: var(--text-main); margin-bottom: 6px; }
select[multiple] { height: 80px; padding: 6px; }

.form-actions { display: flex; justify-content: flex-end; margin-top: 14px; }
.primary-btn {
  display: flex; align-items: center; gap: 6px;
  background: var(--primary, #4f46e5); color: white;
  border: none; border-radius: 10px; padding: 10px 14px;
  font-size: 14px; font-weight: 600; cursor: pointer; transition: background 0.2s;
}
.primary-btn:hover:not(:disabled) { background: var(--primary-hover, #4338ca); }
.primary-btn:disabled { opacity: 0.7; cursor: not-allowed; }

.secondary-btn {
  background: transparent; color: var(--text-main);
  border: 1px solid var(--border-color); border-radius: 10px; 
  padding: 8px 12px; font-size: 13px; font-weight: 600; cursor: pointer;
}
.secondary-btn:hover { background: var(--border-color); }
.primary-btn.small { padding: 8px 12px; font-size: 13px; }

/* DANH SÁCH BÌNH LUẬN */
.comment-list { display: flex; flex-direction: column; gap: 16px; }

.state-message { color: var(--text-muted); font-size: 14px; padding: 20px 0; display: flex; align-items: center; gap: 8px; }
.state-message.empty { flex-direction: column; text-align: center; opacity: 0.7; padding: 40px 0; }
.state-message.empty svg { margin-bottom: 8px; }

.comment-item { display: flex; gap: 14px; }

/* AVATAR BÌNH LUẬN */
.comment-avatar {
  width: 36px; height: 36px; border-radius: 50%; flex-shrink: 0;
  background: rgba(79, 70, 229, 0.1); color: var(--primary, #4f46e5);
  display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 14px; margin-top: 4px;
}

/* KHUNG NỘI DUNG */
.comment-body {
  flex: 1;
  background: var(--bg-body, #f8fafc);
  border: 1px solid var(--border-color, #e5e7eb);
  border-radius: 14px;
  padding: 14px 16px;
}

.comment-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px; }
.user-info { display: flex; flex-direction: column; gap: 2px; }
.user-name { font-size: 14px; font-weight: 600; color: var(--text-main); }
.timestamp { font-size: 12px; color: var(--text-muted); }

/* NÚT THAO TÁC TRONG BÌNH LUẬN */
.actions { display: flex; gap: 6px; }
.icon-btn {
  width: 28px; height: 28px; border-radius: 8px; border: none;
  background: transparent; color: var(--text-muted);
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: all 0.2s;
}
.icon-btn:hover { background: var(--border-color); color: var(--text-main); }
.icon-btn.danger:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

.comment-content { margin: 0; font-size: 14px; line-height: 1.5; color: var(--text-main); white-space: pre-wrap; }

/* EDIT BOX */
.edit-box { margin-top: 8px; }
.edit-textarea { margin-bottom: 10px; background: var(--input-bg); }
.edit-actions { display: flex; gap: 8px; justify-content: flex-end; }

/* META TAGS (Mentions, Attachments) */
.comment-footer { display: flex; flex-direction: column; gap: 8px; margin-top: 12px; padding-top: 12px; border-top: 1px dashed var(--border-color); }
.meta-tags { display: flex; flex-wrap: wrap; gap: 8px; }
.mention-tag {
  display: inline-flex; align-items: center; gap: 4px;
  background: rgba(79, 70, 229, 0.1); color: var(--primary, #4f46e5);
  padding: 4px 10px; border-radius: 99px; font-size: 12px; font-weight: 600;
}
.file-tag {
  display: inline-flex; align-items: center; gap: 4px;
  background: var(--bg-card); border: 1px solid var(--border-color);
  color: var(--text-main); text-decoration: none;
  padding: 4px 10px; border-radius: 8px; font-size: 12px; font-weight: 500;
  transition: border-color 0.2s;
}
.file-tag:hover { border-color: var(--primary, #4f46e5); color: var(--primary, #4f46e5); }

/* THÔNG BÁO ALERT */
.alert { padding: 10px 14px; border-radius: 10px; margin-bottom: 16px; font-weight: 600; font-size: 13px; }
.alert.error { background: rgba(239, 68, 68, 0.1); color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.2); }
.alert.success { background: rgba(16, 185, 129, 0.1); color: #10b981; border: 1px solid rgba(16, 185, 129, 0.2); }

/* RESPONSIVE */
@media (max-width: 768px) {
  .form-row { grid-template-columns: 1fr; }
  .comment-item { flex-direction: column; gap: 10px; }
  .comment-avatar { margin-bottom: -4px; width: 32px; height: 32px; font-size: 12px; }
}
</style>