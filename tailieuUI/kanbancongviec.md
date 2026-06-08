<!DOCTYPE html>

<html lang="vi"><head>
<meta charset="utf-8"/>
<meta content="width=device-width, initial-scale=1.0" name="viewport"/>
<title>Bảng Kanban - Quản Lý Dự Án</title>
<script src="https://cdn.tailwindcss.com?plugins=forms,container-queries"></script>
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@100..900&amp;display=swap" rel="stylesheet"/>
<style>
        .material-symbols-outlined {
            font-family: 'Material Symbols Outlined';
            font-weight: normal;
            font-style: normal;
            font-size: 24px;
            line-height: 1;
            letter-spacing: normal;
            text-transform: none;
            display: inline-block;
            white-space: nowrap;
            word-wrap: normal;
            direction: ltr;
            -webkit-font-feature-settings: 'liga';
            -webkit-font-smoothing: antialiased;
        }
        
        /* Custom Scrollbar for Kanban Board */
        .kanban-scroll::-webkit-scrollbar {
            width: 8px;
            height: 8px;
        }
        .kanban-scroll::-webkit-scrollbar-track {
            background: transparent;
        }
        .kanban-scroll::-webkit-scrollbar-thumb {
            background-color: #c7c4d8;
            border-radius: 4px;
        }
        .kanban-scroll::-webkit-scrollbar-thumb:hover {
            background-color: #777587;
        }
    </style>
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
</head>
<body class="bg-surface text-on-surface font-body-md text-body-md overflow-hidden h-screen w-screen flex antialiased">
<!-- SideNavBar -->
<aside class="w-[260px] h-screen fixed left-0 top-0 bg-surface-container-low dark:bg-surface-container-lowest border-r border-outline-variant dark:border-outline shadow-sm flex flex-col py-lg px-md z-40 hidden md:flex">
<div class="flex items-center gap-sm mb-xl px-2">
<div class="w-8 h-8 rounded bg-primary text-on-primary flex items-center justify-center font-bold">P</div>
<div>
<h1 class="font-headline-sm text-headline-sm font-bold text-primary dark:text-primary-fixed truncate">Project Manager</h1>
<p class="font-label-sm text-label-sm text-on-surface-variant truncate">Microservices Architecture</p>
</div>
</div>
<button class="mb-lg w-full py-2 px-4 rounded-lg bg-primary text-on-primary font-label-md text-label-md flex items-center justify-center gap-2 hover:bg-surface-tint transition-colors shadow-sm">
<span class="material-symbols-outlined text-[18px]">add</span>
            Tạo dự án mới
        </button>
<nav class="flex flex-col gap-1 flex-1">
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0;">dashboard</span>
<span class="font-label-lg text-label-lg">Tổng quan</span>
</a>
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0;">folder_shared</span>
<span class="font-label-lg text-label-lg">Dự án</span>
</a>
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-primary font-bold border-r-4 border-primary bg-primary-container/10 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1;">view_kanban</span>
<span class="font-label-lg text-label-lg">Bảng Kanban</span>
</a>
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0;">notifications</span>
<span class="font-label-lg text-label-lg">Thông báo</span>
</a>
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container-high transition-colors duration-200 cursor-pointer active:scale-95 mt-auto" href="#">
<span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 0;">settings</span>
<span class="font-label-lg text-label-lg">Cài đặt</span>
</a>
</nav>
</aside>
<!-- Main Content Area -->
<div class="flex-1 flex flex-col md:ml-[260px] h-screen w-full relative">
<!-- TopAppBar -->
<header class="h-16 w-full bg-surface-container-lowest dark:bg-surface-dim border-b border-outline-variant dark:border-outline shadow-sm flex justify-between items-center px-lg z-30 relative md:static">
<!-- Mobile Menu Toggle (Hidden on Desktop) -->
<button class="md:hidden p-2 text-on-surface-variant hover:bg-surface-container-high rounded-full">
<span class="material-symbols-outlined">menu</span>
</button>
<div class="flex items-center gap-md">
<h2 class="font-headline-md text-headline-md font-black text-primary hidden md:block">Quản Lý Dự Án</h2>
<div class="hidden lg:flex items-center gap-4 ml-xl h-full pt-1">
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-5" href="#">Sprints</a>
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-5" href="#">Báo cáo</a>
<a class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-5" href="#">Tài liệu</a>
</div>
</div>
<div class="flex items-center gap-sm">
<!-- Search -->
<div class="hidden md:flex items-center bg-surface-container-low rounded-full px-3 py-1.5 border border-outline-variant focus-within:border-primary focus-within:ring-1 focus-within:ring-primary transition-all">
<span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
<input class="bg-transparent border-none focus:ring-0 text-body-md font-body-md text-on-surface w-40 placeholder:text-outline outline-none ml-2" placeholder="Tìm kiếm..." type="text"/>
</div>
<!-- Actions -->
<button class="p-2 text-on-surface-variant hover:bg-surface-container-high rounded-full transition-all active:opacity-80">
<span class="material-symbols-outlined">notifications</span>
</button>
<button class="p-2 text-on-surface-variant hover:bg-surface-container-high rounded-full transition-all active:opacity-80">
<span class="material-symbols-outlined">help_outline</span>
</button>
<button class="hidden sm:flex px-4 py-1.5 bg-primary-container text-on-primary-container rounded-lg font-label-md text-label-md items-center gap-2 hover:bg-primary hover:text-on-primary transition-colors ml-2">
<span class="material-symbols-outlined text-[18px]">add</span>
                    Tạo Task
                </button>
<div class="w-8 h-8 rounded-full bg-secondary text-on-secondary flex items-center justify-center font-bold ml-2 cursor-pointer border-2 border-surface">
<img alt="Ảnh đại diện người dùng" class="w-full h-full rounded-full object-cover" data-alt="A professional headshot of a young male software engineer in a well-lit modern office environment. He is wearing a dark blue crew neck shirt and has short, neatly styled brown hair. The background is slightly blurred showing glass partitions and indoor plants, creating a bright, modern corporate aesthetic. Lighting is soft and natural, highlighting a friendly yet systematic professional persona." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDGc0QFp7Lq2PlnuRFOF3FHXPCxPO685f-Z7VXSJo1NUhPRBztU0Qut48QZfsw-dG2SmcNwOr8MW2KlfCVJMC1_v9rA0aYsbUh1bX6S60KUnOAqc0bAGV0mvpM8xRiWeXXcSp_W9z1tdRI1pAjBrUC9OykY2tK39TMIGgfw_cqK5ZNmhsQtTom2D5EMBRpdRvyTpObUbzPO34CQeWsvtnGL7r8nI6cIhYyoPFRa7TXylGuqQpj92h9dzswiXpzIzoonxwsVxBntiK4F"/>
</div>
</div>
</header>
<!-- Toolbar (Filters) -->
<div class="w-full px-md md:px-lg py-sm bg-surface-container-lowest border-b border-outline-variant flex flex-wrap items-center justify-between gap-4 z-20">
<div class="flex items-center gap-2 overflow-x-auto kanban-scroll pb-1 sm:pb-0">
<div class="flex items-center gap-2 pr-4 border-r border-outline-variant">
<div class="w-6 h-6 rounded-full bg-primary/20 text-primary flex items-center justify-center text-xs font-bold ring-2 ring-surface-container-lowest -mr-2 z-10 relative">
<img class="w-full h-full rounded-full object-cover" data-alt="A small circular avatar portrait of a professional woman with long dark hair, wearing a white blouse, set against a neutral light grey background." src="https://lh3.googleusercontent.com/aida-public/AB6AXuCt7BlIOXaH4tiqAB86AoewqYxZ6GTg25-yG0ewAydR_9CcalYzZb7Ns3RK9sV32qsJ_yY7gNDiNUL8-wsAZFsSq_-flMZegS8KmykrSEGTxRueXbFjDEUDNfZzWMSYdc0UnqNNtZ03Z8aAIiMU-G-2C-vxGJE70XtoYJULtWLcs9OLDvZ1GhmZcv8PkF9saA58I5aCYklwHdDBH_WaZaRqpSy9uLjnM3iSD-NjUVArd8J00Alo0AjeGGzDGJ5jU-6HcPvkooMSOZ_9"/>
</div>
<div class="w-6 h-6 rounded-full bg-secondary/20 text-secondary flex items-center justify-center text-xs font-bold ring-2 ring-surface-container-lowest -mr-2 z-20 relative">
<img class="w-full h-full rounded-full object-cover" data-alt="A small circular avatar portrait of a male team member with short dark hair and a beard, wearing a grey t-shirt, set against a bright minimal background." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBjc9iGywnYHlMEzEOCipFcwSc-2ZxqaWdFlqj-d2B3Xto1eVt31A3gKSVWaJ32JwtswDpdr4jpu5fMqcA6j2hKSQAPzR_J5HmpWkjxOnxUaTl3nek8Wirp6PrPxolMwXct0xvXzQEC6d9Eij7l9IbAEcu9gLEVHl89Aiu_1Rbh6U4fTOAgA5f5amRYusccGaPUC-GJ2xuDgAFjCPCSYCh-g4pNXyYx-FIxFW69VACfHAvfwrIjCOUrPIKs-zT3ADV4UuTguEDw7cj2"/>
</div>
<button class="w-6 h-6 rounded-full bg-surface-container-high text-on-surface-variant flex items-center justify-center ring-2 ring-surface-container-lowest z-30 hover:bg-surface-dim transition-colors">
<span class="material-symbols-outlined text-[14px]">add</span>
</button>
</div>
<button class="px-3 py-1.5 rounded-md border border-outline-variant bg-surface text-on-surface-variant font-label-md text-label-md flex items-center gap-1.5 hover:bg-surface-container-high transition-colors whitespace-nowrap">
<span class="material-symbols-outlined text-[16px]">person</span>
                    Thành viên
                    <span class="material-symbols-outlined text-[16px]">arrow_drop_down</span>
</button>
<button class="px-3 py-1.5 rounded-md border border-outline-variant bg-surface text-on-surface-variant font-label-md text-label-md flex items-center gap-1.5 hover:bg-surface-container-high transition-colors whitespace-nowrap">
<span class="material-symbols-outlined text-[16px]">flag</span>
                    Độ ưu tiên
                    <span class="material-symbols-outlined text-[16px]">arrow_drop_down</span>
</button>
<button class="px-3 py-1.5 rounded-md border border-outline-variant bg-surface text-on-surface-variant font-label-md text-label-md flex items-center gap-1.5 hover:bg-surface-container-high transition-colors whitespace-nowrap">
<span class="material-symbols-outlined text-[16px]">label</span>
                    Nhãn
                    <span class="material-symbols-outlined text-[16px]">arrow_drop_down</span>
</button>
</div>
<div class="flex items-center gap-2">
<button class="p-1.5 text-on-surface-variant hover:bg-surface-container-high rounded transition-colors" title="Chế độ xem danh sách">
<span class="material-symbols-outlined text-[20px]">format_list_bulleted</span>
</button>
<button class="p-1.5 text-primary bg-primary-container/10 rounded transition-colors" title="Chế độ xem bảng">
<span class="material-symbols-outlined text-[20px]" style="font-variation-settings: 'FILL' 1;">grid_view</span>
</button>
</div>
</div>
<!-- Kanban Board (Fluid Grid / Scrollable Horizontal) -->
<div class="flex-1 overflow-x-auto overflow-y-hidden kanban-scroll p-md md:p-lg bg-surface-container flex gap-md sm:gap-lg items-start">
<!-- Column: Backlog -->
<div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm">
<div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
<div class="flex items-center gap-2">
<div class="w-2 h-2 rounded-full bg-outline"></div>
<h3 class="font-label-lg text-label-lg text-on-surface">Backlog</h3>
<span class="bg-surface-container text-on-surface-variant px-2 py-0.5 rounded-full font-label-sm text-label-sm">4</span>
</div>
<button class="text-on-surface-variant hover:text-primary transition-colors">
<span class="material-symbols-outlined text-[20px]">more_horiz</span>
</button>
</div>
<div class="px-2 pt-2">
<button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary-container/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
<span class="material-symbols-outlined text-[16px]">add</span> Thêm task
                    </button>
</div>
<div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
<!-- Task Card 1 -->
<div class="bg-surface-container-lowest p-3 rounded shadow-[0_1px_2px_rgba(0,0,0,0.05)] border border-outline-variant cursor-grab hover:shadow-md transition-shadow group">
<div class="flex justify-between items-start mb-2">
<div class="flex flex-wrap gap-1">
<span class="bg-surface-container text-on-surface-variant px-1.5 py-0.5 rounded font-label-sm text-label-sm">API</span>
</div>
<span class="material-symbols-outlined text-[16px] text-tertiary" title="Ưu tiên Trung bình">remove</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface mb-1 leading-snug line-clamp-2">Thiết kế schema database cho User Service</h4>
<p class="font-label-md text-label-md text-outline mb-3">TKS-102</p>
<div class="flex justify-between items-end mt-auto">
<div class="flex items-center gap-1 text-outline">
<span class="material-symbols-outlined text-[14px]">calendar_today</span>
<span class="font-label-sm text-label-sm">Không có</span>
</div>
<div class="flex -space-x-2">
<!-- Unassigned -->
<div class="w-6 h-6 rounded-full border-2 border-surface-container-lowest bg-surface-container flex items-center justify-center text-outline">
<span class="material-symbols-outlined text-[14px]">person</span>
</div>
</div>
</div>
</div>
</div>
</div>
<!-- Column: To Do -->
<div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm">
<div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
<div class="flex items-center gap-2">
<div class="w-2 h-2 rounded-full bg-primary-container"></div>
<h3 class="font-label-lg text-label-lg text-on-surface">Cần làm</h3>
<span class="bg-primary-container/10 text-primary px-2 py-0.5 rounded-full font-label-sm text-label-sm">2</span>
</div>
<button class="text-on-surface-variant hover:text-primary transition-colors">
<span class="material-symbols-outlined text-[20px]">more_horiz</span>
</button>
</div>
<div class="px-2 pt-2">
<button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary-container/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
<span class="material-symbols-outlined text-[16px]">add</span> Thêm task
                    </button>
</div>
<div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
<!-- Task Card 2 -->
<div class="bg-surface-container-lowest p-3 rounded shadow-[0_1px_2px_rgba(0,0,0,0.05)] border border-outline-variant cursor-grab hover:shadow-md transition-shadow group">
<div class="flex justify-between items-start mb-2">
<div class="flex flex-wrap gap-1">
<span class="bg-secondary-container/30 text-secondary px-1.5 py-0.5 rounded font-label-sm text-label-sm">Frontend</span>
</div>
<span class="material-symbols-outlined text-[16px] text-error" title="Ưu tiên Cao">keyboard_double_arrow_up</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface mb-1 leading-snug line-clamp-2">Implement giao diện kéo thả cho Kanban Board</h4>
<p class="font-label-md text-label-md text-outline mb-3">TKS-105</p>
<div class="flex justify-between items-end mt-auto">
<div class="flex items-center gap-1 text-error">
<span class="material-symbols-outlined text-[14px]">schedule</span>
<span class="font-label-sm text-label-sm">Hôm nay</span>
</div>
<div class="flex -space-x-2">
<img class="w-6 h-6 rounded-full border-2 border-surface-container-lowest object-cover" data-alt="A small circular avatar portrait of a male team member with short dark hair and a beard, wearing a grey t-shirt." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBHi2sxb_WnFDRorHgpBa_rq4jas2_rRosmb8Ylb0pVUavpb9oecEySa62j6dxSHVHF1SH3upS9TT5Cy1kXm6FapfCnRn1ZaeBe1Cr1FteNOElZEoY8wLd47QWwEAI5tKCMDMI-pNQ2sjDcZCOSzKkfFRznYFD_vf_dMRFQnAy6rMY0MHEo0EAUzocFlFXKHCVCo3qFLgVrpBI5OQQMHmkLUhBwRVonpU9sWjFT3IkndDZlIAv5_EkgE8x-hy6JvZIz-mYvkPU2sxDj"/>
</div>
</div>
</div>
<!-- Task Card 3 -->
<div class="bg-surface-container-lowest p-3 rounded shadow-[0_1px_2px_rgba(0,0,0,0.05)] border border-outline-variant cursor-grab hover:shadow-md transition-shadow group">
<div class="flex justify-between items-start mb-2">
<div class="flex flex-wrap gap-1">
<span class="bg-tertiary-container/20 text-tertiary px-1.5 py-0.5 rounded font-label-sm text-label-sm">Design</span>
</div>
<span class="material-symbols-outlined text-[16px] text-tertiary" title="Ưu tiên Trung bình">remove</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface mb-1 leading-snug line-clamp-2">Review UI/UX với team Product</h4>
<p class="font-label-md text-label-md text-outline mb-3">TKS-108</p>
<div class="flex justify-between items-end mt-auto">
<div class="flex items-center gap-1 text-on-surface-variant">
<span class="material-symbols-outlined text-[14px]">calendar_today</span>
<span class="font-label-sm text-label-sm">12 Th10</span>
</div>
<div class="flex -space-x-2">
<img class="w-6 h-6 rounded-full border-2 border-surface-container-lowest object-cover" data-alt="A small circular avatar portrait of a professional woman with long dark hair, wearing a white blouse." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBFAR3AjsKItMOUyfe-PaCaAwanujQl2Gnmw6mVgzJxhc3jG0HSlUXB5hkrYmD4uNZKDbbQplyVUAptK-avivZdhk-Ju33moW2nHwvvDYNXYRcZEWETC7SRYKAMqnB_Uze7civXh5b5djL9UDdaYVdqCRKy_llvaXI-0S5cib-lWU0WqBIt5lBtvY9Zfpv_MCMtvtU3zsayr-QwvN0G8H_aJyMzHN7j3BmOUIymcje3UeOYJwaA0PWLp8CUUx9yo_dkbjrgDJv0Z2xV"/>
</div>
</div>
</div>
</div>
</div>
<!-- Column: In Progress -->
<div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm">
<div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
<div class="flex items-center gap-2">
<div class="w-2 h-2 rounded-full bg-secondary"></div>
<h3 class="font-label-lg text-label-lg text-on-surface">Đang thực hiện</h3>
<span class="bg-secondary-container/20 text-secondary px-2 py-0.5 rounded-full font-label-sm text-label-sm">1</span>
</div>
<button class="text-on-surface-variant hover:text-primary transition-colors">
<span class="material-symbols-outlined text-[20px]">more_horiz</span>
</button>
</div>
<div class="px-2 pt-2">
<button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary-container/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
<span class="material-symbols-outlined text-[16px]">add</span> Thêm task
                    </button>
</div>
<div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
<!-- Task Card 4 -->
<div class="bg-surface-container-lowest p-3 rounded shadow-[0_4px_6px_-1px_rgba(0,0,0,0.1)] border border-primary/30 cursor-grab hover:shadow-md transition-shadow group relative overflow-hidden">
<div class="absolute left-0 top-0 bottom-0 w-1 bg-secondary"></div>
<div class="flex justify-between items-start mb-2 pl-2">
<div class="flex flex-wrap gap-1">
<span class="bg-primary-container/20 text-primary px-1.5 py-0.5 rounded font-label-sm text-label-sm">Backend</span>
</div>
<span class="material-symbols-outlined text-[16px] text-error" title="Ưu tiên Cao">keyboard_double_arrow_up</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface mb-1 leading-snug line-clamp-2 pl-2">Tối ưu hóa query lấy danh sách task</h4>
<p class="font-label-md text-label-md text-outline mb-3 pl-2">TKS-099</p>
<div class="flex justify-between items-end mt-auto pl-2">
<div class="flex items-center gap-1 text-secondary font-medium">
<span class="material-symbols-outlined text-[14px] animate-spin" style="font-variation-settings: 'FILL' 1;">sync</span>
<span class="font-label-sm text-label-sm">Đang code</span>
</div>
<div class="flex -space-x-2">
<img class="w-6 h-6 rounded-full border-2 border-surface-container-lowest object-cover" data-alt="A small circular avatar portrait of a young male software engineer in a dark blue crew neck shirt." src="https://lh3.googleusercontent.com/aida-public/AB6AXuAlstB-pKpX4Rph6luFh04qZcAaC5v-R5AkKH_caa46owPPo006W6NWroo-dd93yxvLiYr2MRvlumz375xpmAwQy4UkpHkTyE7p4e-tbHKOx42nf7e_ZSusqS7uXERAvFRVaPKrJU2KWnjieA22nZj7DYcZ0PZvAy2d9DrhgKw7Ut1LwoQVCLEkH7yrPok1v4VBxviNz29cE1pZWUcen39K73T_F5_NRK--YBA8yYVl2lMx5BUvYUdmB2vm6Nq96TRfEtCwEiUVoZrf"/>
</div>
</div>
</div>
</div>
</div>
<!-- Column: Review -->
<div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm">
<div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
<div class="flex items-center gap-2">
<div class="w-2 h-2 rounded-full bg-tertiary"></div>
<h3 class="font-label-lg text-label-lg text-on-surface">Đang kiểm tra</h3>
<span class="bg-tertiary-container/20 text-tertiary px-2 py-0.5 rounded-full font-label-sm text-label-sm">1</span>
</div>
<button class="text-on-surface-variant hover:text-primary transition-colors">
<span class="material-symbols-outlined text-[20px]">more_horiz</span>
</button>
</div>
<div class="px-2 pt-2">
<button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary-container/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
<span class="material-symbols-outlined text-[16px]">add</span> Thêm task
                    </button>
</div>
<div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
<!-- Task Card 5 -->
<div class="bg-surface-container-lowest p-3 rounded shadow-[0_1px_2px_rgba(0,0,0,0.05)] border border-outline-variant cursor-grab hover:shadow-md transition-shadow group">
<div class="flex justify-between items-start mb-2">
<div class="flex flex-wrap gap-1">
<span class="bg-surface-container text-on-surface-variant px-1.5 py-0.5 rounded font-label-sm text-label-sm">Testing</span>
</div>
<span class="material-symbols-outlined text-[16px] text-outline" title="Ưu tiên Thấp">keyboard_arrow_down</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface mb-1 leading-snug line-clamp-2">Viết Unit Test cho Auth Service</h4>
<p class="font-label-md text-label-md text-outline mb-3">TKS-095</p>
<div class="flex items-center gap-2 mb-3">
<div class="flex-1 h-1.5 bg-surface-container rounded-full overflow-hidden">
<div class="w-3/4 h-full bg-tertiary"></div>
</div>
<span class="font-label-sm text-label-sm text-outline">75%</span>
</div>
<div class="flex justify-between items-end mt-auto">
<div class="flex items-center gap-2 text-outline">
<div class="flex items-center gap-1" title="2 Comment">
<span class="material-symbols-outlined text-[14px]">chat_bubble_outline</span>
<span class="font-label-sm text-label-sm">2</span>
</div>
</div>
<div class="flex -space-x-2">
<div class="w-6 h-6 rounded-full border-2 border-surface-container-lowest bg-primary text-on-primary flex items-center justify-center font-label-sm text-[10px]">
                                    QA
                                </div>
</div>
</div>
</div>
</div>
</div>
<!-- Column: Done -->
<div class="w-[300px] flex-shrink-0 flex flex-col max-h-full bg-surface-container-lowest border border-outline-variant rounded-lg shadow-sm opacity-80 hover:opacity-100 transition-opacity">
<div class="p-3 border-b border-outline-variant flex justify-between items-center bg-surface-bright rounded-t-lg">
<div class="flex items-center gap-2">
<div class="w-2 h-2 rounded-full bg-secondary"></div>
<h3 class="font-label-lg text-label-lg text-on-surface">Hoàn thành</h3>
<span class="bg-surface-container text-on-surface-variant px-2 py-0.5 rounded-full font-label-sm text-label-sm">12</span>
</div>
<button class="text-on-surface-variant hover:text-primary transition-colors">
<span class="material-symbols-outlined text-[20px]">more_horiz</span>
</button>
</div>
<div class="px-2 pt-2">
<button class="w-full py-1.5 border border-dashed border-outline-variant rounded text-on-surface-variant hover:text-primary hover:border-primary hover:bg-primary-container/5 transition-all flex justify-center items-center gap-1 font-label-md text-label-md">
<span class="material-symbols-outlined text-[16px]">add</span> Thêm task
                    </button>
</div>
<div class="flex-1 overflow-y-auto kanban-scroll p-2 flex flex-col gap-2">
<!-- Task Card 6 -->
<div class="bg-surface-bright p-3 rounded border border-outline-variant/50 cursor-grab group">
<div class="flex justify-between items-start mb-2">
<div class="flex flex-wrap gap-1 opacity-70">
<span class="bg-primary-container/20 text-primary px-1.5 py-0.5 rounded font-label-sm text-label-sm">Backend</span>
</div>
<span class="material-symbols-outlined text-[16px] text-secondary" style="font-variation-settings: 'FILL' 1;">check_circle</span>
</div>
<h4 class="font-headline-sm text-headline-sm text-on-surface-variant mb-1 leading-snug line-clamp-2 line-through">Setup CI/CD pipeline cho dev environment</h4>
<p class="font-label-md text-label-md text-outline mb-3">TKS-088</p>
<div class="flex justify-between items-end mt-auto opacity-70">
<div class="flex items-center gap-1 text-outline">
<span class="font-label-sm text-label-sm">Hoàn thành 09 Th10</span>
</div>
</div>
</div>
</div>
</div>
</div>
</div>
</body></html>