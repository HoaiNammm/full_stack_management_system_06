<template>
  <header class="h-16 flex-shrink-0 bg-surface-container-lowest border-b border-outline-variant shadow-sm flex justify-between items-center px-lg z-30">
    <!-- Left -->
    <div class="flex items-center gap-lg">
      <div class="font-headline-md text-headline-md font-black text-primary hidden md:block">Quản Lý Dự Án</div>
      <div class="hidden lg:flex items-center gap-md">
        <a href="#" class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors">Sprints</a>
        <a href="#" class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors">Báo cáo</a>
        <a href="#" class="font-label-md text-label-md text-on-surface-variant hover:text-on-surface py-xs transition-colors">Tài liệu</a>
      </div>
    </div>

    <!-- Right -->
    <div class="flex items-center gap-sm">
      <!-- Search -->
      <div class="hidden md:flex items-center bg-surface-container-low rounded-full px-3 py-1.5 border border-outline-variant focus-within:border-primary focus-within:ring-1 focus-within:ring-primary transition-all">
        <span class="material-symbols-outlined text-on-surface-variant text-[18px]">search</span>
        <input class="bg-transparent border-none focus:ring-0 font-body-md text-body-md text-on-surface w-40 placeholder:text-outline outline-none ml-2" placeholder="Tìm kiếm..." type="text" />
      </div>
      <button class="text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all active:opacity-80">
        <span class="material-symbols-outlined">help_outline</span>
      </button>
      <button class="relative text-on-surface-variant hover:bg-surface-container-high rounded-full p-2 transition-all active:opacity-80">
        <span class="material-symbols-outlined">notifications</span>
        <span class="absolute top-1 right-1 w-2 h-2 bg-error rounded-full border border-surface-container-lowest"></span>
      </button>
      <div class="w-px h-6 bg-outline-variant mx-xs hidden sm:block"></div>
      <button class="hidden sm:flex items-center gap-xs bg-primary/10 text-primary px-sm py-xs rounded-lg font-label-md text-label-md hover:bg-primary/20 transition-colors">
        <span class="material-symbols-outlined text-[18px]">add_task</span>
        Tạo Task
      </button>
      <div class="ml-xs cursor-pointer rounded-full overflow-hidden border-2 border-surface-container-lowest hover:border-primary transition-colors w-8 h-8">
        <img src="https://i.pravatar.cc/32?img=47" alt="Avatar" class="w-full h-full object-cover" />
      </div>
    </div>
  </header>
</template>

<script setup></script>
