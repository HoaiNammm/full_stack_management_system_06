<script setup>
import CommentSection from "@/components/CommentSection.vue";
</script>

<template>
  <TaskDetailLayout>
    <!-- Thông tin task của nhóm 2 -->

    <CommentSection
      :task-id="task.id"
      :project-id="task.projectId"
    />
  </TaskDetailLayout>
</template>

// 