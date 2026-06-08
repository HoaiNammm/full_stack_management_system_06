<template>
  <div class="flex-grow pt-24 px-lg pb-xl max-w-7xl mx-auto w-full flex flex-col gap-lg">
    <!-- Page Title -->
    <div class="flex justify-between items-end mb-sm">
      <div>
        <h2 class="font-headline-lg text-headline-lg text-on-surface">Tổng quan hệ thống</h2>
        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Theo dõi tiến độ và hiệu suất các dự án hiện tại.</p>
      </div>
      <div class="flex items-center gap-sm text-on-surface-variant">
        <span class="material-symbols-outlined">calendar_today</span>
        <span class="font-label-md text-label-md">{{ today }}</span>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-md">
      <StatCard icon="folder_copy" label="Tổng số dự án" :value="12"
        badge="+2 tháng này" badgeColor="text-secondary bg-secondary-container/30"
        iconBg="bg-primary/10 text-primary" />
      <StatCard icon="engineering" label="Task đang thực hiện" :value="64"
        badge="Trên 5 dự án" badgeColor="text-on-surface-variant"
        iconBg="bg-secondary/10 text-secondary" />
      <StatCard icon="warning" label="Task quá hạn" :value="8"
        badge="Xem chi tiết" badgeColor="text-on-surface-variant hover:underline cursor-pointer"
        iconBg="bg-error-container text-error" valueColor="text-error" borderClass="border-error/20" />
      <StatCard icon="mail" label="Thông báo mới" :value="24"
        badge="Từ 3 hệ thống" badgeColor="text-on-surface-variant"
        iconBg="bg-tertiary/10 text-tertiary" />
    </div>

    <!-- Sprint Hero -->
    <div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
      <div class="p-md border-b border-outline-variant bg-surface-container-low/50 flex justify-between items-center">
        <div class="flex items-center gap-sm">
          <span class="material-symbols-outlined text-primary" style="font-variation-settings: 'FILL' 1">sprint</span>
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Sprint Hiện Tại: Core Service v2.4</h3>
        </div>
        <div class="bg-surface-container-highest px-sm py-1 rounded-full flex items-center gap-xs">
          <span class="material-symbols-outlined text-[16px] text-on-surface-variant">schedule</span>
          <span class="font-label-md text-label-md text-on-surface-variant">Còn 3 ngày</span>
        </div>
      </div>
      <div class="p-md flex flex-col gap-md">
        <div class="flex justify-between items-end">
          <div class="w-2/3">
            <p class="font-label-lg text-label-lg text-on-surface mb-1">Mục tiêu Sprint</p>
            <p class="font-body-md text-body-md text-on-surface-variant">Hoàn thiện module Authentication và tối ưu hóa truy vấn Database cho Dashboard chính. Đảm bảo coverage &gt; 85%.</p>
          </div>
          <div class="text-right">
            <span class="font-headline-md text-headline-md text-primary">68%</span>
            <p class="font-label-sm text-label-sm text-on-surface-variant">Đã hoàn thành (42/62 SP)</p>
          </div>
        </div>
        <div class="w-full bg-surface-container-high rounded-full h-3 overflow-hidden flex">
          <div class="bg-secondary h-full" style="width:68%"></div>
          <div class="bg-tertiary-fixed-dim h-full" style="width:15%"></div>
        </div>
        <div class="flex gap-md mt-1">
          <div class="flex items-center gap-1"><div class="w-2 h-2 rounded-full bg-secondary"></div><span class="font-label-sm text-label-sm text-on-surface-variant">Hoàn thành (68%)</span></div>
          <div class="flex items-center gap-1"><div class="w-2 h-2 rounded-full bg-tertiary-fixed-dim"></div><span class="font-label-sm text-label-sm text-on-surface-variant">Đang kiểm tra (15%)</span></div>
          <div class="flex items-center gap-1"><div class="w-2 h-2 rounded-full bg-surface-container-high border border-outline-variant"></div><span class="font-label-sm text-label-sm text-on-surface-variant">Cần làm (17%)</span></div>
        </div>
      </div>
    </div>

    <!-- Bottom Row -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-lg flex-grow">
      <!-- Projects List -->
      <div class="lg:col-span-2 flex flex-col bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden">
        <div class="p-md border-b border-outline-variant flex justify-between items-center">
          <h3 class="font-headline-sm text-headline-sm text-on-surface">Dự án hoạt động gần đây</h3>
          <button class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</button>
        </div>
        <div class="flex flex-col">
          <ProjectRow v-for="p in projects" :key="p.id" :project="p" />
        </div>
      </div>

      <!-- Activity Feed -->
      <div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
        <div class="p-md border-b border-outline-variant flex justify-between items-center bg-surface-container-low/30">
          <h3 class="font-headline-sm text-headline-sm text-on-surface flex items-center gap-2">
            <span class="material-symbols-outlined text-[20px] text-on-surface-variant">history</span>
            Hoạt động gần đây
          </h3>
        </div>
        <div class="p-md flex flex-col gap-md overflow-y-auto max-h-[400px]">
          <ActivityItem v-for="(a, i) in activities" :key="i" :activity="a" :showLine="i < activities.length - 1" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import StatCard from '../components/StatCard.vue'
import ProjectRow from '../components/ProjectRow.vue'
import ActivityItem from '../components/ActivityItem.vue'

const today = new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' })

const projects = ref([
  {
    id: 1,
    initials: 'PAY',
    color: 'bg-primary/10 border-primary/20 text-primary',
    name: 'Payment Gateway Refactor',
    desc: 'Cập nhật API v3.0',
    tasks: 12,
    statusText: 'Đúng tiến độ',
    statusColor: 'text-secondary',
    avatars: ['https://lh3.googleusercontent.com/aida-public/AB6AXuBKMVFbUlGommGI8yYQoA4aw4pw0ut2FBOB5cib4cBpp0oFeZ0PSZ3LLFDfg2ZkGTPgSIdbRaPGioBkXt2UsTELsuEPlzEgOjtIPv4GVLm6pdREceoQkY6KCoz6xoNKmYLbVJ-4av4kS16YbnwxcrptQrIzrA_LaMKFh9CyBHt5VoN1Bl3CEBWetG5GKh4u5lrLbyzw33S18aVJXFiS2kSzUZS6cUCqn6YHJHNi_xR7ejLbmKrCO8vToqYaGBLC5PZBrC7PVaY82fY1', 'https://lh3.googleusercontent.com/aida-public/AB6AXuDu0HO-KCoPPegpfrcFvjQ_vezIVhq8jgwS9-DjlRfwjISw9hCqwGZEs7GGM7DigkKvZkzBlg93w6PYB5hel4xPDWB-nGOZwhXqBbY_QaydwyIH9Gt6TzjxIzCwjBy4nlPgBqYwPnDJDtQU-vH1ie93gfEB9siWvqTXhafdaNWx5i_sR_fNHH4UhHEqK3ZzY8QKc9yTBfWkUCHo15T0wbXCVABl3N8CxSDaluyBzuNVqC8sLrK5dG-myhg_1EgfeLqrpwPilQfBHpth'],
    extraCount: 3
  },
  {
    id: 2,
    initials: 'MBL',
    color: 'bg-tertiary/10 border-tertiary/20 text-tertiary',
    name: 'Mobile App - Release Q3',
    desc: 'Tích hợp Push Notifications',
    tasks: 28,
    statusText: 'Chậm 1 ngày',
    statusColor: 'text-error',
    avatars: ['https://lh3.googleusercontent.com/aida-public/AB6AXuDaZL0pmfESFFEZVT3g7seLIEIYoM6vgU3TK0tKTB9Uu33A4_snAIvU9maUYVrtI46DYstSKDOENB0fMO3MDXBQoZ3oLyoRDu6kIOeI8WFfkvy_NrgpYRQsLvM3nLyr9gVP-c_K4EgUcVHf312KMkhjl2FKwepImM73DmN1kIdcDWO8-YQ6Ddsu9Lg5Baf4tQ1TwkKrJJHcSZkz84s5PT1eyo5e8_KqkAtOXKxRgV6UPKKg8v9me-J36GWj53xZAJzXus3-kYOSBH5m'],
    extraCount: 1
  },
  {
    id: 3,
    initials: 'CRM',
    color: 'bg-secondary/10 border-secondary/20 text-secondary',
    name: 'Customer Portal',
    desc: 'Thiết kế UI/UX mới',
    tasks: 5,
    statusText: 'Mới tạo',
    statusColor: 'text-on-surface-variant',
    avatars: ['https://lh3.googleusercontent.com/aida-public/AB6AXuC7cQmYxux6mG1m5krBZ_3qH9-_OTlc221rzEWA46e3eb-MKFLfmARbSKRt6EatLRLb3ELpsqApmsXKnxXAxJAEn3p_vuRRt_D5wFYv5yLAr2N24GwAsVn_jaoitTVBbLiKBqvNlvyrWQKSewweou0f-cg0NLUBe6XaZLT_JJ6Mpvq6ibgnQvEHFNZcU8lzIVlkIKXfYh5fp0_AaBSOgv50nczpVAbrpaGaqMxsu5HeLPM3E2eqPWJMg_FERjkv_UACfNqenKi3bgzs'],
    extraCount: 0
  },
])

const activities = ref([
  {
    avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCLW0E87JakRDcIRs_cu3BAMdExbFlFcE7biPMqyeMUH_GQnG946Npk5ufaHhWd4Dq5ruRifpPIvEyINHRbdarBwtULDbx0pzfvfW2edhilfuHPjG8JHi7iFlBMbpb98Ai50SnB8eDhlLLP1_A1Te7ZW2LxLY37RcGuMQxb60EKn8qHXiYaXuXR9ZhOiOdgKkm0PUCI7pU0eKNnbh4ubkRNnTwyulVjweeLfFJdb17zy3NkmpmeTaSL_kq-BBxD8wT7K1FQ-XZU450K',
    name: 'Minh Anh',
    action: 'đã chuyển task',
    link: 'PAY-142',
    badge: 'Đang thực hiện',
    badgeColor: 'bg-secondary/10 text-secondary',
    time: '10 phút trước'
  },
  {
    icon: 'comment',
    iconBg: 'bg-tertiary-container/20 text-tertiary',
    name: 'Hoàng Nam',
    action: 'đã để lại bình luận trên',
    link: 'MBL-88',
    comment: '"API response đang bị thiếu trường user_id ở môi trường staging nhé."',
    time: '1 giờ trước'
  },
  {
    icon: 'new_releases',
    iconBg: 'bg-error-container/50 text-error',
    name: 'System',
    action: 'Notification: Build #4092 thất bại trên nhánh main.',
    link: '',
    time: 'Hôm qua, 15:30'
  },
])
</script>
