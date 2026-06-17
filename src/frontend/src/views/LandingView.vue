<template>
  <main class="min-h-screen overflow-hidden bg-background text-on-surface">
    <!-- Hero -->
    <section class="relative min-h-screen px-md py-lg md:px-xl md:py-xl">
      <img
        :src="authVisual"
        class="absolute inset-0 h-full w-full object-cover opacity-30"
        alt=""
      />
      <div
        class="absolute inset-0 bg-gradient-to-br from-background via-background/90 to-background/70"
      ></div>

      <nav class="relative z-10 mx-auto flex max-w-7xl items-center justify-between">
        <div class="flex items-center gap-sm">
          <div
            class="flex h-11 w-11 items-center justify-center rounded-xl bg-primary text-on-primary font-black shadow-lg"
          >
            PM
          </div>
          <div>
            <p class="font-headline-sm text-headline-sm font-bold">Project Manager</p>
            <p class="font-label-sm text-label-sm text-on-surface-variant">
              Quản lý dự án thông minh
            </p>
          </div>
        </div>

        <div class="flex items-center gap-sm">
          <RouterLink to="/login" class="app-button-secondary">Đăng nhập</RouterLink>
          <RouterLink to="/register" class="app-button-primary">Dùng thử</RouterLink>
        </div>
      </nav>

      <div
        class="relative z-10 mx-auto mt-xl grid max-w-7xl grid-cols-1 gap-xl lg:grid-cols-[0.9fr_1.1fr] lg:items-center"
      >
        <div>
          <p class="page-eyebrow">Project Management Platform</p>

          <h1
            class="mt-sm max-w-3xl text-[44px] font-black leading-[52px] md:text-[68px] md:leading-[76px]"
          >
            Quản lý dự án, công việc và đội nhóm trong một workspace.
          </h1>

          <p class="mt-md max-w-2xl font-body-lg text-body-lg text-on-surface-variant">
            Project Manager giúp nhóm lập kế hoạch, phân công nhiệm vụ, theo dõi tiến độ, quản lý
            thành viên và nhận thông báo ngay trong một giao diện trực quan.
          </p>

          <div class="mt-lg flex flex-wrap gap-sm">
            <RouterLink to="/register" class="app-button-primary">
              <span class="material-symbols-outlined text-[18px]">rocket_launch</span>
              Bắt đầu miễn phí
            </RouterLink>

            <RouterLink to="/login" class="app-button-secondary">
              <span class="material-symbols-outlined text-[18px]">play_circle</span>
              Xem bản demo
            </RouterLink>
          </div>

          <div class="mt-lg grid max-w-xl grid-cols-3 gap-sm">
            <div
              v-for="metric in metrics"
              :key="metric.label"
              class="metric-tile bg-surface-container-lowest/85"
            >
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>
        </div>

        <div class="relative min-h-[560px]">
          <div class="absolute left-[8%] top-0 w-[70%] rotate-[-1deg]">
            <MockScreen title="Dashboard" theme="light" image="dashboard" />
          </div>

          <div class="absolute right-0 top-[9%] w-[44%] rotate-[2deg]">
            <MockScreen title="Projects" theme="dark" image="projects" />
          </div>

          <div class="absolute bottom-[4%] left-0 w-[48%] rotate-[1deg]">
            <MockScreen title="Timeline" theme="dark" image="analytics" />
          </div>

          <div class="absolute bottom-0 left-[34%] w-[55%] rotate-[-1deg]">
            <MockScreen title="Kanban Board" theme="light" image="kanban" />
          </div>

          <div class="absolute bottom-[6%] right-[2%] hidden w-[20%] rotate-[3deg] md:block">
            <MockPhone />
          </div>
        </div>
      </div>
    </section>

    <!-- Features -->
    <section class="relative z-10 mx-auto max-w-7xl px-md pb-xl md:px-xl">
      <div class="mb-lg text-center">
        <p class="page-eyebrow">Tính năng nổi bật</p>
        <h2 class="mt-xs font-headline-lg text-headline-lg text-on-surface">
          Mọi thứ nhóm cần để quản lý dự án hiệu quả
        </h2>
        <p class="mx-auto mt-sm max-w-2xl font-body-md text-body-md text-on-surface-variant">
          Từ tạo dự án, giao việc, kéo thả Kanban đến theo dõi timeline và nhận thông báo.
        </p>
      </div>

      <div class="grid grid-cols-1 gap-lg md:grid-cols-3">
        <article
          v-for="feature in features"
          :key="feature.title"
          class="group overflow-hidden rounded-2xl border border-outline-variant bg-surface-container-low shadow-sm transition-all hover:-translate-y-1 hover:shadow-xl"
        >
          <img
            :src="feature.image"
            class="h-40 w-full object-cover transition-transform duration-500 group-hover:scale-105"
            alt=""
          />

          <div class="p-md">
            <div class="flex items-center gap-xs">
              <span class="material-symbols-outlined text-primary text-[20px]">{{
                feature.icon
              }}</span>
              <h3 class="font-headline-sm text-headline-sm text-on-surface">{{ feature.title }}</h3>
            </div>

            <p class="mt-xs font-body-sm text-body-sm text-on-surface-variant">
              {{ feature.desc }}
            </p>
          </div>
        </article>
      </div>
    </section>

    <!-- Product preview -->
    <section
      class="relative z-10 mx-auto grid max-w-7xl grid-cols-1 gap-xl px-md pb-xl md:px-xl lg:grid-cols-[1.05fr_0.95fr]"
    >
      <div
        class="overflow-hidden rounded-[28px] border border-outline-variant bg-surface-container-low shadow-xl"
      >
        <div class="grid min-h-[460px] grid-cols-2">
          <div class="relative overflow-hidden bg-white p-md text-slate-950">
            <p class="text-xs font-black uppercase tracking-wider text-blue-600">Light workspace</p>
            <MockScreen title="Dashboard" theme="light" image="dashboard" />
          </div>

          <div class="relative overflow-hidden bg-slate-950 p-md text-white">
            <p class="text-xs font-black uppercase tracking-wider text-sky-300">Dark workspace</p>
            <MockScreen title="Timeline" theme="dark" image="analytics" />
          </div>
        </div>
      </div>

      <div
        class="rounded-[28px] border border-outline-variant bg-surface-container-low p-lg shadow-xl"
      >
        <p class="page-eyebrow">Quy trình làm việc</p>
        <h2 class="mt-xs font-headline-lg text-headline-lg text-on-surface">
          Từ ý tưởng đến hoàn thành trong vài bước
        </h2>

        <div class="mt-lg space-y-sm">
          <div
            v-for="(step, index) in workflow"
            :key="step.title"
            class="flex gap-sm rounded-2xl bg-surface-container-lowest p-sm"
          >
            <div
              class="grid h-9 w-9 flex-shrink-0 place-items-center rounded-full bg-primary text-on-primary font-black"
            >
              {{ index + 1 }}
            </div>

            <div>
              <p class="font-label-lg text-label-lg text-on-surface">{{ step.title }}</p>
              <p class="font-body-sm text-body-sm text-on-surface-variant">{{ step.desc }}</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Roles -->
    <section class="relative z-10 mx-auto max-w-7xl px-md pb-xl md:px-xl">
      <div class="mb-lg text-center">
        <p class="page-eyebrow">Phân quyền linh hoạt</p>
        <h2 class="mt-xs font-headline-lg text-headline-lg text-on-surface">
          Mỗi thành viên có đúng vai trò trong dự án
        </h2>
      </div>

      <div
        class="grid grid-cols-1 gap-md rounded-[28px] border border-outline-variant bg-surface-container-low p-md shadow-xl md:grid-cols-5"
      >
        <article
          v-for="role in roles"
          :key="role.name"
          class="rounded-2xl bg-surface-container-lowest p-md text-center"
        >
          <div
            class="mx-auto grid h-14 w-14 place-items-center rounded-2xl text-white shadow-sm"
            :class="role.bg"
          >
            <span class="material-symbols-outlined">{{ role.icon }}</span>
          </div>

          <h3 class="mt-sm font-label-lg text-label-lg text-on-surface">{{ role.name }}</h3>
          <p class="mt-xs font-label-sm text-label-sm text-on-surface-variant">{{ role.desc }}</p>
        </article>
      </div>
    </section>

    <!-- CTA -->
    <section class="relative z-10 mx-auto max-w-7xl px-md pb-xl md:px-xl">
      <div
        class="relative overflow-hidden rounded-[28px] bg-primary p-lg text-on-primary shadow-2xl"
      >
        <img
          :src="getProjectCover({}, 5)"
          class="absolute inset-0 h-full w-full object-cover opacity-25 mix-blend-screen"
          alt=""
        />

        <div
          class="relative z-10 flex flex-col gap-md md:flex-row md:items-center md:justify-between"
        >
          <div>
            <p class="font-label-md text-label-md uppercase tracking-wider opacity-80">
              Sẵn sàng bắt đầu
            </p>

            <h2 class="mt-xs font-headline-lg text-headline-lg">
              Tạo workspace đầu tiên cho nhóm của bạn
            </h2>

            <p class="mt-xs max-w-2xl font-body-md text-body-md opacity-85">
              Quản lý dự án, phân công công việc, theo dõi tiến độ và cộng tác với thành viên trong
              cùng một nền tảng.
            </p>
          </div>

          <RouterLink
            to="/register"
            class="inline-flex items-center justify-center gap-xs rounded-xl bg-white px-md py-sm font-label-lg text-label-lg text-primary shadow-lg transition hover:-translate-y-0.5"
          >
            Bắt đầu ngay
          </RouterLink>
        </div>
      </div>
    </section>
  </main>
</template>

<script setup>
import { defineComponent, h } from 'vue'
import {
  authVisual,
  dashboardVisual,
  getProjectCover,
  getTaskThumbnail,
} from '../services/visualAssets'

const metrics = [
  { label: 'Dự án', value: '12+' },
  { label: 'Vai trò', value: '5' },
  { label: 'Màn hình', value: '8+' },
]

const features = [
  {
    title: 'Dashboard tổng quan',
    icon: 'dashboard',
    image: dashboardVisual,
    desc: 'Theo dõi số liệu, tiến độ, deadline và hoạt động mới nhất của dự án trong một màn hình.',
  },
  {
    title: 'Quản lý dự án',
    icon: 'folder_copy',
    image: getProjectCover({}, 2),
    desc: 'Tạo dự án, thêm thành viên, đặt thời gian, trạng thái và phân quyền cho từng người.',
  },
  {
    title: 'Kanban công việc',
    icon: 'view_kanban',
    image: getTaskThumbnail({ title: 'design' }, 0),
    desc: 'Kéo thả task qua các cột, gán người phụ trách, đặt mức ưu tiên và deadline rõ ràng.',
  },
  {
    title: 'Timeline & lịch',
    icon: 'calendar_month',
    image: getProjectCover({}, 1),
    desc: 'Theo dõi sprint, milestone, lịch làm việc và các mốc quan trọng của nhóm.',
  },
  {
    title: 'Thành viên & vai trò',
    icon: 'groups',
    image: getProjectCover({}, 3),
    desc: 'Quản lý Owner, Project Manager, Developer, Tester và Viewer bằng giao diện trực quan.',
  },
  {
    title: 'Thông báo & thảo luận',
    icon: 'notifications',
    image: getTaskThumbnail({ title: 'api' }, 1),
    desc: 'Cập nhật thông báo, hoạt động và bình luận để nhóm không bỏ lỡ thay đổi quan trọng.',
  },
]

const workflow = [
  {
    title: 'Tạo workspace',
    desc: 'Bắt đầu bằng tài khoản của bạn và truy cập không gian làm việc chung của nhóm.',
  },
  {
    title: 'Khởi tạo dự án',
    desc: 'Tạo dự án mới, chọn trạng thái, thời gian bắt đầu, deadline và mục tiêu chính.',
  },
  {
    title: 'Thêm thành viên',
    desc: 'Mời thành viên vào dự án và gán vai trò phù hợp với từng người.',
  },
  {
    title: 'Phân công công việc',
    desc: 'Tạo task, gán người phụ trách, đặt priority và quản lý tiến độ bằng Kanban.',
  },
  {
    title: 'Theo dõi & cộng tác',
    desc: 'Xem dashboard, timeline, activity, notification và bình luận để cập nhật tiến độ.',
  },
]

const roles = [
  { name: 'Owner', icon: 'crown', bg: 'bg-amber-500', desc: 'Toàn quyền quản lý' },
  {
    name: 'Project Manager',
    icon: 'workspace_premium',
    bg: 'bg-emerald-500',
    desc: 'Điều phối dự án',
  },
  { name: 'Developer', icon: 'code', bg: 'bg-sky-500', desc: 'Thực hiện task' },
  { name: 'Tester', icon: 'fact_check', bg: 'bg-indigo-500', desc: 'Kiểm thử chất lượng' },
  { name: 'Viewer', icon: 'visibility', bg: 'bg-slate-500', desc: 'Theo dõi tiến độ' },
]

const MockScreen = defineComponent({
  props: { title: String, theme: String, image: String },
  setup(props) {
    return () =>
      h(
        'div',
        {
          class: [
            'overflow-hidden rounded-2xl border shadow-2xl backdrop-blur',
            props.theme === 'dark'
              ? 'border-white/10 bg-slate-950 text-white'
              : 'border-white/70 bg-white text-slate-900',
          ],
        },
        [
          h(
            'div',
            {
              class:
                'flex items-center justify-between border-b border-black/10 px-3 py-2 text-[11px] font-bold',
            },
            [
              h('span', props.title),
              h(
                'span',
                { class: props.theme === 'dark' ? 'text-white/50' : 'text-slate-400' },
                'Search',
              ),
            ],
          ),
          h('div', { class: 'grid grid-cols-[0.26fr_0.74fr] gap-2 p-3' }, [
            h(
              'div',
              {
                class:
                  props.theme === 'dark' ? 'space-y-2 text-white/60' : 'space-y-2 text-slate-400',
              },
              [
                ...['Dashboard', 'Projects', 'Tasks', 'Team'].map((item, index) =>
                  h(
                    'div',
                    {
                      class: [
                        'rounded-lg px-2 py-1 text-[10px]',
                        index === 0 ? 'bg-primary/20 text-primary' : '',
                      ],
                    },
                    item,
                  ),
                ),
              ],
            ),
            h('div', { class: 'space-y-2' }, [
              h('img', {
                src: props.image === 'projects' ? getProjectCover({}, 2) : dashboardVisual,
                class: 'h-20 w-full rounded-xl object-cover',
              }),
              h(
                'div',
                { class: 'grid grid-cols-3 gap-2' },
                [70, 22, 383].map((v) =>
                  h('div', { class: 'rounded-lg bg-primary/10 p-2 text-[11px] font-bold' }, v),
                ),
              ),
              h('div', { class: 'grid grid-cols-2 gap-2' }, [
                h('img', {
                  src: getTaskThumbnail({ title: 'design' }, 0),
                  class: 'h-16 rounded-lg object-cover',
                }),
                h('img', {
                  src: getTaskThumbnail({ title: 'api' }, 1),
                  class: 'h-16 rounded-lg object-cover',
                }),
              ]),
            ]),
          ]),
        ],
      )
  },
})

const MockPhone = defineComponent({
  setup() {
    return () =>
      h('div', { class: 'rounded-[28px] border-4 border-slate-900 bg-white p-3 shadow-2xl' }, [
        h('div', { class: 'mx-auto mb-2 h-1 w-12 rounded-full bg-slate-900' }),
        h('p', { class: 'text-xs font-bold text-slate-900' }, 'Members'),
        h('div', { class: 'mt-3 grid grid-cols-2 gap-2' }, [
          ...['An', 'PM', 'Dev', 'QA'].map((name) =>
            h(
              'div',
              {
                class:
                  'rounded-xl bg-slate-100 p-2 text-center text-[10px] font-bold text-slate-700',
              },
              [
                h(
                  'div',
                  {
                    class:
                      'mx-auto mb-1 flex h-8 w-8 items-center justify-center rounded-full bg-primary text-white',
                  },
                  name[0],
                ),
                name,
              ],
            ),
          ),
        ]),
      ])
  },
})
</script>
