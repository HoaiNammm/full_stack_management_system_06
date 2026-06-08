<!DOCTYPE html>

<html lang="vi"><head>
<meta charset="utf-8"/>
<meta content="width=device-width, initial-scale=1.0" name="viewport"/>
<title>Project Manager - Tổng quan</title>
<!-- Material Symbols -->
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
<!-- Inter Font (Assumed from design system config) -->
<link href="https://fonts.googleapis.com" rel="preconnect"/>
<link crossorigin="" href="https://fonts.gstatic.com" rel="preconnect"/>
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;900&amp;display=swap" rel="stylesheet"/>
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
<!-- Tailwind CSS -->
<script src="https://cdn.tailwindcss.com?plugins=forms,container-queries"></script>
<!-- Tailwind Config injected from Style Guidance -->
<script id="tailwind-config">
      tailwind.config = {
        darkMode: "class",
        theme: {
          extend: {
            "colors": {
                    "surface-bright": "#f8f9fa",
                    "surface-container": "#edeeef",
                    "primary-container": "#4f46e5",
                    "surface-dim": "#d9dadb",
                    "error": "#ba1a1a",
                    "tertiary-container": "#885500",
                    "surface-container-high": "#e7e8e9",
                    "on-tertiary-container": "#ffd4a4",
                    "on-error": "#ffffff",
                    "error-container": "#ffdad6",
                    "outline": "#777587",
                    "on-primary-fixed": "#0f0069",
                    "on-primary-container": "#dad7ff",
                    "primary-fixed-dim": "#c3c0ff",
                    "on-tertiary-fixed": "#2a1700",
                    "outline-variant": "#c7c4d8",
                    "surface": "#f8f9fa",
                    "on-error-container": "#93000a",
                    "on-surface-variant": "#464555",
                    "secondary": "#006a61",
                    "on-surface": "#191c1d",
                    "surface-tint": "#4d44e3",
                    "tertiary": "#684000",
                    "tertiary-fixed": "#ffddb8",
                    "tertiary-fixed-dim": "#ffb95f",
                    "on-tertiary": "#ffffff",
                    "on-primary": "#ffffff",
                    "secondary-fixed-dim": "#6bd8cb",
                    "surface-container-highest": "#e1e3e4",
                    "on-primary-fixed-variant": "#3323cc",
                    "on-secondary": "#ffffff",
                    "on-background": "#191c1d",
                    "primary": "#3525cd",
                    "surface-container-low": "#f3f4f5",
                    "inverse-surface": "#2e3132",
                    "on-secondary-container": "#006f66",
                    "secondary-container": "#86f2e4",
                    "background": "#f8f9fa",
                    "on-secondary-fixed-variant": "#005049",
                    "inverse-on-surface": "#f0f1f2",
                    "on-secondary-fixed": "#00201d",
                    "surface-variant": "#e1e3e4",
                    "surface-container-lowest": "#ffffff",
                    "primary-fixed": "#e2dfff",
                    "secondary-fixed": "#89f5e7",
                    "on-tertiary-fixed-variant": "#653e00",
                    "inverse-primary": "#c3c0ff"
            },
            "borderRadius": {
                    "DEFAULT": "0.25rem",
                    "lg": "0.5rem",
                    "xl": "0.75rem",
                    "full": "9999px"
            },
            "spacing": {
                    "lg": "24px",
                    "xs": "8px",
                    "base": "4px",
                    "margin-desktop": "24px",
                    "sm": "12px",
                    "md": "16px",
                    "gutter": "16px",
                    "margin-mobile": "16px",
                    "xl": "32px"
            },
            "fontFamily": {
                    "label-sm": ["Inter"],
                    "label-md": ["Inter"],
                    "headline-md": ["Inter"],
                    "body-md": ["Inter"],
                    "headline-lg": ["Inter"],
                    "headline-sm": ["Inter"],
                    "body-lg": ["Inter"],
                    "label-lg": ["Inter"]
            },
            "fontSize": {
                    "label-sm": ["11px", {"lineHeight": "14px", "fontWeight": "700"}],
                    "label-md": ["12px", {"lineHeight": "16px", "fontWeight": "500"}],
                    "headline-md": ["24px", {"lineHeight": "32px", "letterSpacing": "-0.01em", "fontWeight": "600"}],
                    "body-md": ["14px", {"lineHeight": "20px", "fontWeight": "400"}],
                    "headline-lg": ["32px", {"lineHeight": "40px", "letterSpacing": "-0.02em", "fontWeight": "700"}],
                    "headline-sm": ["18px", {"lineHeight": "28px", "fontWeight": "600"}],
                    "body-lg": ["16px", {"lineHeight": "24px", "fontWeight": "400"}],
                    "label-lg": ["14px", {"lineHeight": "20px", "fontWeight": "600"}]
            }
          }
        }
      }
    </script>
<style>
        body { font-family: 'Inter', sans-serif; }
    </style>
</head>
<body class="bg-background text-on-background min-h-screen">
<!-- App Shell Container -->
<div class="flex">
<!-- Component: SideNavBar -->
<nav class="w-[260px] h-screen fixed left-0 top-0 bg-surface-container-low border-r border-outline-variant shadow-sm flex flex-col py-lg px-md z-40">
<!-- Header/Brand -->
<div class="flex items-center gap-sm mb-xl px-xs">
<div class="w-10 h-10 rounded-lg bg-primary flex items-center justify-center text-on-primary font-bold font-headline-sm">
                    PM
                </div>
<div>
<h1 class="font-headline-sm text-headline-sm font-bold text-primary">Project Manager</h1>
<p class="font-label-sm text-label-sm text-on-surface-variant">Microservices Architecture</p>
</div>
</div>
<!-- Navigation Links -->
<div class="flex flex-col gap-xs flex-grow">
<!-- Active Tab: Tổng quan -->
<a class="flex items-center gap-sm px-sm py-sm rounded-lg text-primary font-bold border-r-4 border-primary bg-primary-container/10 cursor-pointer active:scale-95 transition-all" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1;">dashboard</span>
<span class="font-label-lg text-label-lg">Tổng quan</span>
</a>
<!-- Inactive Tabs -->
<a class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined">folder_shared</span>
<span class="font-label-lg text-label-lg">Dự án</span>
</a>
<a class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined">view_kanban</span>
<span class="font-label-lg text-label-lg">Bảng Kanban</span>
</a>
<a class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined">notifications</span>
<span class="font-label-lg text-label-lg">Thông báo</span>
</a>
<a class="flex items-center gap-sm px-sm py-sm rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined">settings</span>
<span class="font-label-lg text-label-lg">Cài đặt</span>
</a>
</div>
<!-- CTA -->
<button class="mt-auto flex items-center justify-center gap-xs w-full py-sm bg-primary text-on-primary rounded-lg font-label-lg text-label-lg shadow-sm hover:opacity-90 transition-opacity">
<span class="material-symbols-outlined">add</span>
                Tạo dự án mới
            </button>
</nav>
<!-- Main Content Area -->
<main class="ml-[260px] w-full min-h-screen relative flex flex-col">
<!-- Component: TopAppBar -->
<header class="h-16 fixed top-0 right-0 left-[260px] z-30 bg-surface-container-lowest border-b border-outline-variant shadow-sm flex justify-between items-center px-lg">
<!-- Left: Branding / Search Context -->
<div class="flex items-center gap-lg">
<div class="font-headline-md text-headline-md font-black text-primary">Quản Lý Dự Án</div>
<!-- Search Bar -->
<div class="relative w-64 hidden lg:block">
<span class="material-symbols-outlined absolute left-sm top-1/2 -translate-y-1/2 text-on-surface-variant text-sm">search</span>
<input class="w-full pl-10 pr-sm py-xs bg-surface-container-low border border-outline-variant rounded-full font-body-md text-body-md focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all" placeholder="Tìm kiếm task, dự án..." type="text"/>
</div>
</div>
<!-- Center: Navigation Links -->
<nav class="hidden md:flex items-center gap-md">
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors" href="#">Sprints</a>
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors" href="#">Báo cáo</a>
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors" href="#">Tài liệu</a>
</nav>
<!-- Right: Actions -->
<div class="flex items-center gap-sm">
<button class="text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all active:opacity-80">
<span class="material-symbols-outlined">help_outline</span>
</button>
<button class="relative text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all active:opacity-80">
<span class="material-symbols-outlined">notifications</span>
<span class="absolute top-1 right-1 w-2 h-2 bg-error rounded-full border border-surface-container-lowest"></span>
</button>
<div class="w-px h-6 bg-outline-variant mx-xs hidden sm:block"></div>
<button class="hidden sm:flex items-center gap-xs bg-primary-container/10 text-primary px-sm py-xs rounded-lg font-label-md text-label-md hover:bg-primary-container/20 transition-colors">
<span class="material-symbols-outlined text-sm">add_task</span>
                        Tạo Task
                    </button>
<!-- Profile -->
<div class="ml-xs cursor-pointer rounded-full overflow-hidden border-2 border-surface-container-lowest hover:border-primary transition-colors w-8 h-8">
<img alt="Ảnh đại diện người dùng" class="w-full h-full object-cover" data-alt="A professional headshot of a young woman with a neutral expression, serving as a user profile avatar in a minimalist corporate UI. The lighting is soft and flattering, set against a clean, light-mode background. The color palette is natural and subtle, integrating seamlessly with a polished, modern digital interface." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBr61WxXsr5W7aKkPC49SkSNwbbF65I9za6GquJAHhvKTU3h50iAOc5RE53kpNVrZAZ67DBKoCETMiXKOseVuJFAEwSCK5EDxmBjziaKgAa3_k0T_i9Pvh7rmUj1BfiPcqcKePc9ezPQ5y1s5QtzAXcHav1Ox86JGfYSWVkrZ42FdyqUWst16CLsWGKof-QiGeVw_HaOQx8pIpteeSM_SuoqCPrDRitQn0YiVwcdeYCI3ws1uHIiomuLdxhXTBOkJWUsppyQYSYNNRH"/>
</div>
</div>
</header>
<!-- Dashboard Content Canvas -->
<div class="flex-grow pt-24 px-lg pb-xl max-w-7xl mx-auto w-full flex flex-col gap-lg">
<!-- Page Title Area (Contextual) -->
<div class="flex justify-between items-end mb-sm">
<div>
<h2 class="font-headline-lg text-headline-lg text-on-surface">Tổng quan hệ thống</h2>
<p class="font-body-md text-body-md text-on-surface-variant mt-1">Theo dõi tiến độ và hiệu suất các dự án hiện tại.</p>
</div>
<div class="flex items-center gap-sm text-on-surface-variant">
<span class="material-symbols-outlined">calendar_today</span>
<span class="font-label-md text-label-md">Hôm nay, 24 Thg 10</span>
</div>
</div>
<!-- 1. Quick Stats (Bento Grid Top Row) -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-md">
<!-- Stat 1 -->
<div class="bg-surface-container-lowest p-md rounded-xl shadow-sm border border-outline-variant flex flex-col gap-xs hover:shadow-md transition-shadow">
<div class="flex justify-between items-center">
<span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Tổng số dự án</span>
<div class="w-8 h-8 rounded-full bg-primary-container/10 flex items-center justify-center text-primary">
<span class="material-symbols-outlined text-[18px]">folder_copy</span>
</div>
</div>
<div class="flex items-baseline gap-sm">
<span class="font-headline-lg text-headline-lg text-on-surface">12</span>
<span class="font-label-sm text-label-sm text-secondary bg-secondary-container/30 px-2 py-0.5 rounded-full">+2 tháng này</span>
</div>
</div>
<!-- Stat 2 -->
<div class="bg-surface-container-lowest p-md rounded-xl shadow-sm border border-outline-variant flex flex-col gap-xs hover:shadow-md transition-shadow">
<div class="flex justify-between items-center">
<span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Task đang thực hiện</span>
<div class="w-8 h-8 rounded-full bg-secondary/10 flex items-center justify-center text-secondary">
<span class="material-symbols-outlined text-[18px]">engineering</span>
</div>
</div>
<div class="flex items-baseline gap-sm">
<span class="font-headline-lg text-headline-lg text-on-surface">64</span>
<span class="font-label-sm text-label-sm text-on-surface-variant">Trên 5 dự án</span>
</div>
</div>
<!-- Stat 3 -->
<div class="bg-surface-container-lowest p-md rounded-xl shadow-sm border border-error/20 flex flex-col gap-xs hover:shadow-md transition-shadow relative overflow-hidden">
<div class="absolute top-0 right-0 w-16 h-16 bg-error/5 rounded-bl-full -z-10"></div>
<div class="flex justify-between items-center">
<span class="font-label-md text-label-md text-error font-semibold uppercase tracking-wider">Task quá hạn</span>
<div class="w-8 h-8 rounded-full bg-error-container flex items-center justify-center text-error">
<span class="material-symbols-outlined text-[18px]">warning</span>
</div>
</div>
<div class="flex items-baseline gap-sm">
<span class="font-headline-lg text-headline-lg text-error">8</span>
<span class="font-label-sm text-label-sm text-on-surface-variant hover:underline cursor-pointer">Xem chi tiết</span>
</div>
</div>
<!-- Stat 4 -->
<div class="bg-surface-container-lowest p-md rounded-xl shadow-sm border border-outline-variant flex flex-col gap-xs hover:shadow-md transition-shadow">
<div class="flex justify-between items-center">
<span class="font-label-md text-label-md text-on-surface-variant uppercase tracking-wider">Thông báo mới</span>
<div class="w-8 h-8 rounded-full bg-tertiary/10 flex items-center justify-center text-tertiary">
<span class="material-symbols-outlined text-[18px]">mail</span>
</div>
</div>
<div class="flex items-baseline gap-sm">
<span class="font-headline-lg text-headline-lg text-on-surface">24</span>
<span class="font-label-sm text-label-sm text-on-surface-variant">Từ 3 hệ thống</span>
</div>
</div>
</div>
<!-- 2. Current Sprint Status (Hero Widget) -->
<div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
<div class="p-md border-b border-outline-variant bg-surface-container-low/50 flex justify-between items-center">
<div class="flex items-center gap-sm">
<span class="material-symbols-outlined text-primary" style="font-variation-settings: 'FILL' 1;">sprint</span>
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
<!-- Progress Bar -->
<div class="w-full bg-surface-container-high rounded-full h-3 overflow-hidden flex">
<!-- Done -->
<div class="bg-secondary h-full" style="width: 68%;"></div>
<!-- In Review -->
<div class="bg-tertiary-fixed-dim h-full" style="width: 15%;"></div>
</div>
<div class="flex gap-md mt-1">
<div class="flex items-center gap-1">
<div class="w-2 h-2 rounded-full bg-secondary"></div>
<span class="font-label-sm text-label-sm text-on-surface-variant">Hoàn thành (68%)</span>
</div>
<div class="flex items-center gap-1">
<div class="w-2 h-2 rounded-full bg-tertiary-fixed-dim"></div>
<span class="font-label-sm text-label-sm text-on-surface-variant">Đang kiểm tra (15%)</span>
</div>
<div class="flex items-center gap-1">
<div class="w-2 h-2 rounded-full bg-surface-container-high border border-outline-variant"></div>
<span class="font-label-sm text-label-sm text-on-surface-variant">Cần làm (17%)</span>
</div>
</div>
</div>
</div>
<!-- Bottom Row: Projects List & Activity Feed -->
<div class="grid grid-cols-1 lg:grid-cols-3 gap-lg flex-grow">
<!-- 3. Danh sách dự án gần đây (2/3 width on large screens) -->
<div class="lg:col-span-2 flex flex-col bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden">
<div class="p-md border-b border-outline-variant flex justify-between items-center">
<h3 class="font-headline-sm text-headline-sm text-on-surface">Dự án hoạt động gần đây</h3>
<button class="font-label-md text-label-md text-primary hover:underline">Xem tất cả</button>
</div>
<div class="flex flex-col">
<!-- Project Item 1 -->
<div class="flex items-center justify-between p-md border-b border-outline-variant/50 hover:bg-surface-container-low transition-colors group cursor-pointer">
<div class="flex items-center gap-md">
<div class="w-10 h-10 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary font-bold">
                                        PAY
                                    </div>
<div>
<h4 class="font-label-lg text-label-lg text-on-surface group-hover:text-primary transition-colors">Payment Gateway Refactor</h4>
<p class="font-body-md text-body-md text-on-surface-variant">Cập nhật API v3.0</p>
</div>
</div>
<div class="flex items-center gap-xl">
<div class="hidden sm:block text-right">
<div class="font-label-md text-label-md text-on-surface">12 Tasks</div>
<div class="font-label-sm text-label-sm text-secondary">Đúng tiến độ</div>
</div>
<!-- Avatars -->
<div class="flex -space-x-2">
<img alt="Member 1" class="w-8 h-8 rounded-full border-2 border-surface-container-lowest" data-alt="A small circular avatar placeholder showing a smiling professional woman. Used in a team member cluster within a modern corporate dashboard. Light mode setting, clean design." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBKMVFbUlGommGI8yYQoA4aw4pw0ut2FBOB5cib4cBpp0oFeZ0PSZ3LLFDfg2ZkGTPgSIdbRaPGioBkXt2UsTELsuEPlzEgOjtIPv4GVLm6pdREceoQkY6KCoz6xoNKmYLbVJ-4av4kS16YbnwxcrptQrIzrA_LaMKFh9CyBHt5VoN1Bl3CEBWetG5GKh4u5lrLbyzw33S18aVJXFiS2kSzUZS6cUCqn6YHJHNi_xR7ejLbmKrCO8vToqYaGBLC5PZBrC7PVaY82fY1"/>
<img alt="Member 2" class="w-8 h-8 rounded-full border-2 border-surface-container-lowest" data-alt="A small circular avatar placeholder showing a man with glasses. Used in a team member cluster within a modern corporate project management tool. Clean, professional lighting." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDu0HO-KCoPPegpfrcFvjQ_vezIVhq8jgwS9-DjlRfwjISw9hCqwGZEs7GGM7DigkKvZkzBlg93w6PYB5hel4xPDWB-nGOZwhXqBbY_QaydwyIH9Gt6TzjxIzCwjBy4nlPgBqYwPnDJDtQU-vH1ie93gfEB9siWvqTXhafdaNWx5i_sR_fNHH4UhHEqK3ZzY8QKc9yTBfWkUCHo15T0wbXCVABl3N8CxSDaluyBzuNVqC8sLrK5dG-myhg_1EgfeLqrpwPilQfBHpth"/>
<div class="w-8 h-8 rounded-full border-2 border-surface-container-lowest bg-surface-container-high flex items-center justify-center font-label-sm text-label-sm text-on-surface-variant z-10">
                                            +3
                                        </div>
</div>
</div>
</div>
<!-- Project Item 2 -->
<div class="flex items-center justify-between p-md border-b border-outline-variant/50 hover:bg-surface-container-low transition-colors group cursor-pointer">
<div class="flex items-center gap-md">
<div class="w-10 h-10 rounded-lg bg-tertiary/10 border border-tertiary/20 flex items-center justify-center text-tertiary font-bold">
                                        MBL
                                    </div>
<div>
<h4 class="font-label-lg text-label-lg text-on-surface group-hover:text-primary transition-colors">Mobile App - Release Q3</h4>
<p class="font-body-md text-body-md text-on-surface-variant">Tích hợp Push Notifications</p>
</div>
</div>
<div class="flex items-center gap-xl">
<div class="hidden sm:block text-right">
<div class="font-label-md text-label-md text-on-surface">28 Tasks</div>
<div class="font-label-sm text-label-sm text-error">Chậm 1 ngày</div>
</div>
<div class="flex -space-x-2">
<img alt="Member" class="w-8 h-8 rounded-full border-2 border-surface-container-lowest" data-alt="A small circular avatar placeholder showing a professional woman. Used in a team cluster within a modern corporate dashboard. Light mode setting." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDaZL0pmfESFFEZVT3g7seLIEIYoM6vgU3TK0tKTB9Uu33A4_snAIvU9maUYVrtI46DYstSKDOENB0fMO3MDXBQoZ3oLyoRDu6kIOeI8WFfkvy_NrgpYRQsLvM3nLyr9gVP-c_K4EgUcVHf312KMkhjl2FKwepImM73DmN1kIdcDWO8-YQ6Ddsu9Lg5Baf4tQ1TwkKrJJHcSZkz84s5PT1eyo5e8_KqkAtOXKxRgV6UPKKg8v9me-J36GWj53xZAJzXus3-kYOSBH5m"/>
<div class="w-8 h-8 rounded-full border-2 border-surface-container-lowest bg-surface-container-high flex items-center justify-center font-label-sm text-label-sm text-on-surface-variant z-10">
                                            +1
                                        </div>
</div>
</div>
</div>
<!-- Project Item 3 -->
<div class="flex items-center justify-between p-md hover:bg-surface-container-low transition-colors group cursor-pointer">
<div class="flex items-center gap-md">
<div class="w-10 h-10 rounded-lg bg-secondary/10 border border-secondary/20 flex items-center justify-center text-secondary font-bold">
                                        CRM
                                    </div>
<div>
<h4 class="font-label-lg text-label-lg text-on-surface group-hover:text-primary transition-colors">Customer Portal</h4>
<p class="font-body-md text-body-md text-on-surface-variant">Thiết kế UI/UX mới</p>
</div>
</div>
<div class="flex items-center gap-xl">
<div class="hidden sm:block text-right">
<div class="font-label-md text-label-md text-on-surface">5 Tasks</div>
<div class="font-label-sm text-label-sm text-on-surface-variant">Mới tạo</div>
</div>
<div class="flex -space-x-2">
<img alt="Member" class="w-8 h-8 rounded-full border-2 border-surface-container-lowest" data-alt="A small circular avatar placeholder showing a professional man. Used in a team cluster within a modern corporate dashboard. Clean background." src="https://lh3.googleusercontent.com/aida-public/AB6AXuC7cQmYxux6mG1m5krBZ_3qH9-_OTlc221rzEWA46e3eb-MKFLfmARbSKRt6EatLRLb3ELpsqApmsXKnxXAxJAEn3p_vuRRt_D5wFYv5yLAr2N24GwAsVn_jaoitTVBbLiKBqvNlvyrWQKSewweou0f-cg0NLUBe6XaZLT_JJ6Mpvq6ibgnQvEHFNZcU8lzIVlkIKXfYh5fp0_AaBSOgv50nczpVAbrpaGaqMxsu5HeLPM3E2eqPWJMg_FERjkv_UACfNqenKi3bgzs"/>
</div>
</div>
</div>
</div>
</div>
<!-- 4. Widget Hoạt động gần đây (1/3 width) -->
<div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant overflow-hidden flex flex-col">
<div class="p-md border-b border-outline-variant flex justify-between items-center bg-surface-container-low/30">
<h3 class="font-headline-sm text-headline-sm text-on-surface flex items-center gap-2">
<span class="material-symbols-outlined text-[20px] text-on-surface-variant">history</span>
                                Hoạt động gần đây
                            </h3>
</div>
<div class="p-md flex flex-col gap-md overflow-y-auto max-h-[400px]">
<!-- Activity Item 1 -->
<div class="flex gap-sm relative">
<!-- Timeline line -->
<div class="absolute left-4 top-8 bottom-[-16px] w-px bg-outline-variant/50"></div>
<img alt="User" class="w-8 h-8 rounded-full z-10 outline outline-2 outline-surface-container-lowest" data-alt="Small avatar for activity feed showing a woman. Minimalist UI context." src="https://lh3.googleusercontent.com/aida-public/AB6AXuCLW0E87JakRDcIRs_cu3BAMdExbFlFcE7biPMqyeMUH_GQnG946Npk5ufaHhWd4Dq5ruRifpPIvEyINHRbdarBwtULDbx0pzfvfW2edhilfuHPjG8JHi7iFlBMbpb98Ai50SnB8eDhlLLP1_A1Te7ZW2LxLY37RcGuMQxb60EKn8qHXiYaXuXR9ZhOiOdgKkm0PUCI7pU0eKNnbh4ubkRNnTwyulVjweeLfFJdb17zy3NkmpmeTaSL_kq-BBxD8wT7K1FQ-XZU450K"/>
<div>
<p class="font-body-md text-body-md text-on-surface">
<span class="font-label-md font-bold">Minh Anh</span> đã chuyển task <span class="text-primary cursor-pointer hover:underline">PAY-142</span> sang 
                                        <span class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-bold bg-secondary/10 text-secondary ml-1 uppercase">Đang thực hiện</span>
</p>
<p class="font-label-sm text-label-sm text-on-surface-variant mt-1">10 phút trước</p>
</div>
</div>
<!-- Activity Item 2 -->
<div class="flex gap-sm relative">
<div class="absolute left-4 top-8 bottom-[-16px] w-px bg-outline-variant/50"></div>
<div class="w-8 h-8 rounded-full bg-tertiary-container/20 text-tertiary flex items-center justify-center font-label-sm z-10 outline outline-2 outline-surface-container-lowest">
<span class="material-symbols-outlined text-[16px]">comment</span>
</div>
<div>
<p class="font-body-md text-body-md text-on-surface">
<span class="font-label-md font-bold">Hoàng Nam</span> đã để lại bình luận trên <span class="text-primary cursor-pointer hover:underline">MBL-88</span>
</p>
<div class="mt-2 p-2 bg-surface-container-low rounded border border-outline-variant/30 font-body-md text-[13px] text-on-surface-variant italic">
                                        "API response đang bị thiếu trường user_id ở môi trường staging nhé."
                                    </div>
<p class="font-label-sm text-label-sm text-on-surface-variant mt-2">1 giờ trước</p>
</div>
</div>
<!-- Activity Item 3 -->
<div class="flex gap-sm relative">
<div class="w-8 h-8 rounded-full bg-error-container/50 text-error flex items-center justify-center font-label-sm z-10 outline outline-2 outline-surface-container-lowest">
<span class="material-symbols-outlined text-[16px]">new_releases</span>
</div>
<div>
<p class="font-body-md text-body-md text-on-surface">
                                        System Notification: Build <span class="font-mono text-xs bg-surface-container px-1 rounded">#4092</span> thất bại trên nhánh <span class="font-mono text-xs">main</span>.
                                    </p>
<p class="font-label-sm text-label-sm text-on-surface-variant mt-1">Hôm qua, 15:30</p>
</div>
</div>
</div>
</div>
</div>
</div>
</main>
</div>
</body></html>