<template>
  <main class="min-h-screen overflow-hidden bg-background text-on-surface">
    <section class="relative min-h-screen px-md py-lg md:px-xl md:py-xl">
      <img :src="authVisual" class="absolute inset-0 h-full w-full object-cover opacity-35" alt="" />
      <div class="absolute inset-0 bg-gradient-to-br from-background via-background/82 to-background/55"></div>

      <nav class="relative z-10 mx-auto flex max-w-7xl items-center justify-between">
        <div class="flex items-center gap-sm">
          <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-primary text-on-primary font-black shadow-lg">PM</div>
          <div>
            <p class="font-headline-sm text-headline-sm">Project Manager</p>
            <p class="font-label-sm text-label-sm text-on-surface-variant">Team workspace</p>
          </div>
        </div>
        <div class="flex items-center gap-sm">
          <RouterLink to="/login" class="app-button-secondary">Đăng nhập</RouterLink>
          <RouterLink to="/register" class="app-button-primary">Bắt đầu</RouterLink>
        </div>
      </nav>

      <div class="relative z-10 mx-auto mt-xl grid max-w-7xl grid-cols-1 gap-xl lg:grid-cols-[0.88fr_1.12fr] lg:items-center">
        <div>
          <p class="page-eyebrow">Project operating system</p>
          <h1 class="mt-sm max-w-2xl text-[42px] font-black leading-[48px] md:text-[64px] md:leading-[70px]">
            Quản lý dự án trực quan như một sản phẩm SaaS thật.
          </h1>
          <p class="mt-md max-w-xl font-body-lg text-body-lg text-on-surface-variant">
            Dashboard, Kanban, timeline, thành viên và thông báo được trình bày bằng mockup, hình ảnh, avatar và biểu đồ thay vì chỉ toàn chữ.
          </p>
          <div class="mt-lg flex flex-wrap gap-sm">
            <RouterLink to="/register" class="app-button-primary">
              <span class="material-symbols-outlined text-[18px]">rocket_launch</span>
              Tạo tài khoản
            </RouterLink>
            <RouterLink to="/login" class="app-button-secondary">
              <span class="material-symbols-outlined text-[18px]">login</span>
              Xem demo
            </RouterLink>
          </div>
          <div class="mt-lg grid grid-cols-3 gap-sm max-w-lg">
            <div v-for="metric in metrics" :key="metric.label" class="metric-tile bg-surface-container-lowest/85">
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>
        </div>

        <div class="relative min-h-[560px]">
          <div class="absolute left-[10%] top-0 w-[68%] rotate-[-1deg]">
            <MockScreen title="Dashboard" theme="light" image="dashboard" />
          </div>
          <div class="absolute right-0 top-[8%] w-[43%] rotate-[2deg]">
            <MockScreen title="Projects" theme="dark" image="projects" />
          </div>
          <div class="absolute bottom-[5%] left-0 w-[46%] rotate-[1deg]">
            <MockScreen title="Analytics" theme="dark" image="analytics" />
          </div>
          <div class="absolute bottom-0 left-[35%] w-[54%] rotate-[-1deg]">
            <MockScreen title="Kanban Board" theme="light" image="kanban" />
          </div>
          <div class="absolute bottom-[6%] right-[2%] hidden w-[20%] rotate-[3deg] md:block">
            <MockPhone />
          </div>
        </div>
      </div>
    </section>

    <section class="relative z-10 mx-auto grid max-w-7xl grid-cols-1 gap-lg px-md pb-xl md:grid-cols-3 md:px-xl">
      <article v-for="feature in features" :key="feature.title"
        class="group overflow-hidden rounded-2xl border border-outline-variant bg-surface-container-low shadow-sm transition-all hover:-translate-y-1 hover:shadow-xl">
        <img :src="feature.image" class="h-40 w-full object-cover transition-transform duration-500 group-hover:scale-105" alt="" />
        <div class="p-md">
          <div class="flex items-center gap-xs">
            <span class="material-symbols-outlined text-primary text-[20px]">{{ feature.icon }}</span>
            <h2 class="font-headline-sm text-headline-sm text-on-surface">{{ feature.title }}</h2>
          </div>
          <p class="mt-xs font-body-sm text-body-sm text-on-surface-variant">{{ feature.desc }}</p>
        </div>
      </article>
    </section>

    <section class="relative z-10 mx-auto grid max-w-7xl grid-cols-1 gap-xl px-md pb-xl md:px-xl lg:grid-cols-[1.05fr_0.95fr]">
      <div class="overflow-hidden rounded-[28px] border border-outline-variant bg-surface-container-low shadow-xl">
        <div class="grid min-h-[460px] grid-cols-2">
          <div class="relative overflow-hidden bg-white p-md text-slate-950">
            <p class="text-xs font-black text-blue-600">Light workspace</p>
            <MockScreen title="Dashboard" theme="light" image="dashboard" />
          </div>
          <div class="relative overflow-hidden bg-slate-950 p-md text-white">
            <p class="text-xs font-black text-sky-300">Dark workspace</p>
            <MockScreen title="Timeline" theme="dark" image="analytics" />
          </div>
        </div>
      </div>

      <div class="rounded-[28px] border border-outline-variant bg-surface-container-low p-lg shadow-xl">
        <p class="page-eyebrow">Workflow demo</p>
        <h2 class="mt-xs font-headline-lg text-headline-lg text-on-surface">Tu dang ky den dashboard tien do</h2>
        <div class="mt-lg space-y-sm">
          <div v-for="(step, index) in workflow" :key="step.title" class="flex gap-sm rounded-2xl bg-surface-container-lowest p-sm">
            <div class="grid h-9 w-9 flex-shrink-0 place-items-center rounded-full bg-primary text-on-primary font-black">{{ index + 1 }}</div>
            <div>
              <p class="font-label-lg text-label-lg text-on-surface">{{ step.title }}</p>
              <p class="font-body-sm text-body-sm text-on-surface-variant">{{ step.desc }}</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="relative z-10 mx-auto max-w-7xl px-md pb-xl md:px-xl">
      <div class="grid grid-cols-1 gap-md rounded-[28px] border border-outline-variant bg-surface-container-low p-md shadow-xl md:grid-cols-5">
        <article v-for="role in roles" :key="role.name" class="rounded-2xl bg-surface-container-lowest p-md text-center">
          <div class="mx-auto grid h-14 w-14 place-items-center rounded-2xl text-white shadow-sm" :class="role.bg">
            <span class="material-symbols-outlined">{{ role.icon }}</span>
          </div>
          <h3 class="mt-sm font-label-lg text-label-lg text-on-surface">{{ role.name }}</h3>
          <p class="mt-xs font-label-sm text-label-sm text-on-surface-variant">{{ role.desc }}</p>
        </article>
      </div>
    </section>

    <section class="relative z-10 mx-auto max-w-7xl px-md pb-xl md:px-xl">
      <div class="relative overflow-hidden rounded-[28px] bg-primary p-lg text-on-primary shadow-2xl">
        <img :src="getProjectCover({}, 5)" class="absolute inset-0 h-full w-full object-cover opacity-25 mix-blend-screen" alt="" />
        <div class="relative z-10 flex flex-col gap-md md:flex-row md:items-center md:justify-between">
          <div>
            <p class="font-label-md text-label-md uppercase tracking-wider opacity-80">Ready for demo</p>
            <h2 class="mt-xs font-headline-lg text-headline-lg">San sang trinh bay voi giang vien</h2>
            <p class="mt-xs max-w-2xl font-body-md text-body-md opacity-85">Landing, dashboard, du an, Kanban, timeline va profile deu co hinh anh, icon, avatar va widget truc quan.</p>
          </div>
          <RouterLink to="/register" class="inline-flex items-center justify-center gap-xs rounded-xl bg-white px-md py-sm font-label-lg text-label-lg text-primary shadow-lg transition hover:-translate-y-0.5">
            Bat dau ngay
          </RouterLink>
        </div>
      </div>
    </section>
  </main>
</template>

<script setup>
import { defineComponent, h } from 'vue'
import { authVisual, dashboardVisual, getProjectCover, getTaskThumbnail } from '../services/visualAssets'

const metrics = [
  { label: 'Projects', value: '12+' },
  { label: 'Roles', value: '5' },
  { label: 'Views', value: '6' },
]

const features = [
  { title: 'Dashboard tong quan', icon: 'dashboard', image: dashboardVisual, desc: 'KPI, bieu do, recent activity va deadline trong mot man hinh dieu hanh.' },
  { title: 'Quan ly du an', icon: 'folder_copy', image: getProjectCover({}, 2), desc: 'Project cards co cover, tien do, thanh vien, deadline va trang thai.' },
  { title: 'Kanban task', icon: 'view_kanban', image: getTaskThumbnail({ title: 'design' }, 0), desc: 'Task card co anh, tag, priority, avatar, binh luan va deadline.' },
  { title: 'Timeline & lich', icon: 'calendar_month', image: getProjectCover({}, 1), desc: 'Theo doi sprint, milestone va release theo lich truc quan.' },
  { title: 'Thanh vien & role', icon: 'groups', image: getProjectCover({}, 3), desc: 'Owner, PM, Developer, Tester, Viewer voi badge va phan quyen ro rang.' },
  { title: 'Thong bao', icon: 'notifications', image: getTaskThumbnail({ title: 'api' }, 1), desc: 'Notification preview giup nguoi dung nam bat thay doi moi.' },
]

const workflow = [
  { title: 'Tao tai khoan', desc: 'Nguoi dung dang ky, dang nhap bang JWT va vao workspace.' },
  { title: 'Tao du an moi', desc: 'Chon template, ngay bat dau, deadline va mau nhan dien.' },
  { title: 'Them thanh vien', desc: 'Gan role theo du an de hien nut dung quyen.' },
  { title: 'Quan ly Kanban', desc: 'Tao task, gan nguoi phu trach, di chuyen qua cac cot.' },
  { title: 'Theo doi tien do', desc: 'Dashboard cap nhat task, deadline, notification va sprint.' },
]

const roles = [
  { name: 'Owner', icon: 'crown', bg: 'bg-amber-500', desc: 'Toan quyen' },
  { name: 'Project Manager', icon: 'workspace_premium', bg: 'bg-emerald-500', desc: 'Dieu phoi' },
  { name: 'Developer', icon: 'code', bg: 'bg-sky-500', desc: 'Thuc hien' },
  { name: 'Tester', icon: 'fact_check', bg: 'bg-indigo-500', desc: 'Kiem thu' },
  { name: 'Viewer', icon: 'visibility', bg: 'bg-slate-500', desc: 'Chi xem' },
]

const MockScreen = defineComponent({
  props: { title: String, theme: String, image: String },
  setup(props) {
    return () => h('div', {
      class: [
        'overflow-hidden rounded-2xl border shadow-2xl backdrop-blur',
        props.theme === 'dark'
          ? 'border-white/10 bg-slate-950 text-white'
          : 'border-white/70 bg-white text-slate-900',
      ],
    }, [
      h('div', { class: 'flex items-center justify-between border-b border-black/10 px-3 py-2 text-[11px] font-bold' }, [
        h('span', props.title),
        h('span', { class: props.theme === 'dark' ? 'text-white/50' : 'text-slate-400' }, 'Search'),
      ]),
      h('div', { class: 'grid grid-cols-[0.26fr_0.74fr] gap-2 p-3' }, [
        h('div', { class: props.theme === 'dark' ? 'space-y-2 text-white/60' : 'space-y-2 text-slate-400' }, [
          ...['Dashboard', 'Projects', 'Tasks', 'Team'].map((item, index) =>
            h('div', { class: ['rounded-lg px-2 py-1 text-[10px]', index === 0 ? 'bg-primary/20 text-primary' : ''] }, item)
          )
        ]),
        h('div', { class: 'space-y-2' }, [
          h('img', { src: props.image === 'projects' ? getProjectCover({}, 2) : dashboardVisual, class: 'h-20 w-full rounded-xl object-cover' }),
          h('div', { class: 'grid grid-cols-3 gap-2' }, [70, 22, 383].map(v => h('div', { class: 'rounded-lg bg-primary/10 p-2 text-[11px] font-bold' }, v))),
          h('div', { class: 'grid grid-cols-2 gap-2' }, [
            h('img', { src: getTaskThumbnail({ title: 'design' }, 0), class: 'h-16 rounded-lg object-cover' }),
            h('img', { src: getTaskThumbnail({ title: 'api' }, 1), class: 'h-16 rounded-lg object-cover' }),
          ]),
        ]),
      ]),
    ])
  }
})

const MockPhone = defineComponent({
  setup() {
    return () => h('div', { class: 'rounded-[28px] border-4 border-slate-900 bg-white p-3 shadow-2xl' }, [
      h('div', { class: 'mx-auto mb-2 h-1 w-12 rounded-full bg-slate-900' }),
      h('p', { class: 'text-xs font-bold text-slate-900' }, 'Members'),
      h('div', { class: 'mt-3 grid grid-cols-2 gap-2' }, ['An', 'PM', 'Dev', 'QA'].map((name, index) =>
        h('div', { class: 'rounded-xl bg-slate-100 p-2 text-center text-[10px] font-bold text-slate-700' }, [
          h('div', { class: 'mx-auto mb-1 flex h-8 w-8 items-center justify-center rounded-full bg-primary text-white' }, name[0]),
          name,
        ])
      )),
    ])
  }
})
</script>
