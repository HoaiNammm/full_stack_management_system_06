

import api from "./api";

export async function getCommentsByTask(taskId) {
  const response = await api.get(`/comments/task/${taskId}`);
  return response.data;
}

export async function createComment(payload) {
  const response = await api.post("/comments", payload);
  return response.data;
}

export async function updateComment(commentId, payload) {
  const response = await api.put(`/comments/${commentId}`, payload);
  return response.data;
}

export async function deleteComment(commentId) {
  const response = await api.delete(`/comments/${commentId}`);
  return response.data;
}