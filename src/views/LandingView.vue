<script setup>
import { ref } from 'vue'
import {
  Columns3, ListChecks, Clock, MessageSquare, Bell, Activity,
  ArrowRight, Check, Menu, X, Quote, Star
} from 'lucide-vue-next'

const mobileMenuOpen = ref(false)

const features = [
  {
    icon: Columns3,
    accent: '#0075DE',
    accentBg: '#F2F9FF',
    title: 'Kanban task boards',
    description: 'Organize work into Backlog, To Do, In Progress, Review and Done. Filter by priority or status to see exactly what matters right now.'
  },
  {
    icon: ListChecks,
    accent: '#AD6DED',
    accentBg: '#F9F3FE',
    title: 'Subtask checklists',
    description: 'Break a big task into smaller steps, assign each one, and watch the progress bar fill up as the team ticks them off.'
  },
  {
    icon: Clock,
    accent: '#FF8A33',
    accentBg: '#FEF3F1',
    title: 'Time tracking',
    description: 'Log hours against any task with a note and a timestamp, then see total effort logged roll up automatically.'
  },
  {
    icon: MessageSquare,
    accent: '#FF83DD',
    accentBg: '#FEF2FB',
    title: 'Comments & mentions',
    description: 'Discuss a task right where the work happens. Mention a teammate and they get notified instantly.'
  },
  {
    icon: Bell,
    accent: '#03C1BA',
    accentBg: '#EFFCFB',
    title: 'Real-time notifications',
    description: 'Assignments, status changes and mentions reach the right person the moment they happen — no digging required.'
  },
  {
    icon: Activity,
    accent: '#0075DE',
    accentBg: '#F2F9FF',
    title: 'Activity timeline',
    description: 'Every create, update and comment is logged automatically, so the whole team can see how a project is moving.'
  }
]

const steps = [
  {
    number: '01',
    title: 'Create a project',
    description: 'Spin up a workspace, invite your team, and set up the sprints you plan to work in.'
  },
  {
    number: '02',
    title: 'Break down the work',
    description: 'Add tasks, split them into subtasks, set priority and deadlines, then assign an owner to each one.'
  },
  {
    number: '03',
    title: 'Track & collaborate',
    description: 'Move cards across the board, log hours, comment and mention teammates — everything stays in sync.'
  }
]

const testimonials = [
  {
    quote: 'Moving our sprint board here cut our stand-up time in half — everyone already knows what changed overnight.',
    name: 'Minh Anh',
    role: 'Engineering Lead',
    initials: 'MA'
  },
  {
    quote: 'The activity timeline is the first place I check in the morning. It’s like a diary of the whole project.',
    name: 'Thu Ha',
    role: 'Product Manager',
    initials: 'TH'
  },
  {
    quote: 'Subtasks plus time logs finally gave us real numbers on where our hours actually go.',
    name: 'Duc Anh',
    role: 'Team Lead',
    initials: 'DA'
  }
]
</script>

<template>
  <div class="landing-page bg-white text-black">

    <!-- ───────────────────────── Navigation ───────────────────────── -->
    <header class="sticky top-0 z-30 h-16 w-full border-b border-black/[0.06] bg-white/95 backdrop-blur">
      <div class="mx-auto flex h-16 max-w-[1440px] items-center justify-between px-6 lg:px-[60px]">
        <router-link to="/welcome" class="flex items-center gap-2.5">
          <div class="flex h-8 w-8 items-center justify-center overflow-hidden rounded-lg ring-1 ring-black/5">
            <img src="/logo.png" alt="Product logo" class="h-full w-full object-contain p-1" />
          </div>
          <span class="text-base font-semibold tracking-tight">TaskFlow</span>
        </router-link>

        <nav class="hidden items-center gap-8 md:flex">
          <a href="#features" class="text-base text-black/90 transition-colors hover:text-[#0075DE] hover:underline">Features</a>
          <a href="#how-it-works" class="text-base text-black/90 transition-colors hover:text-[#0075DE] hover:underline">How it works</a>
          <a href="#testimonials" class="text-base text-black/90 transition-colors hover:text-[#0075DE] hover:underline">Testimonials</a>
        </nav>

        <div class="hidden items-center gap-3 md:flex">
          <router-link to="/login" class="text-base text-black/90 transition-colors hover:text-[#0075DE] hover:underline">
            Log in
          </router-link>
          <router-link
            to="/register"
            class="inline-flex h-10 items-center justify-center rounded-full bg-[#0075DE] px-5 text-base font-medium text-white transition-colors hover:bg-[#0063BA] active:bg-[#005299]"
          >
            Get started
          </router-link>
        </div>

        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-lg text-black/90 hover:bg-black/5 md:hidden"
          :aria-label="mobileMenuOpen ? 'Close menu' : 'Open menu'"
          @click="mobileMenuOpen = !mobileMenuOpen"
        >
          <X v-if="mobileMenuOpen" class="size-5" aria-hidden="true" />
          <Menu v-else class="size-5" aria-hidden="true" />
        </button>
      </div>

      <!-- Mobile menu -->
      <div v-if="mobileMenuOpen" class="flex flex-col gap-1 border-t border-black/[0.06] bg-white px-6 py-4 md:hidden">
        <a href="#features" class="rounded-lg px-3 py-2 text-base text-black/90 hover:bg-black/5" @click="mobileMenuOpen = false">Features</a>
        <a href="#how-it-works" class="rounded-lg px-3 py-2 text-base text-black/90 hover:bg-black/5" @click="mobileMenuOpen = false">How it works</a>
        <a href="#testimonials" class="rounded-lg px-3 py-2 text-base text-black/90 hover:bg-black/5" @click="mobileMenuOpen = false">Testimonials</a>
        <div class="mt-2 flex flex-col gap-2 border-t border-black/[0.06] pt-3">
          <router-link to="/login" class="rounded-lg px-3 py-2 text-base text-black/90 hover:bg-black/5">Log in</router-link>
          <router-link to="/register" class="inline-flex h-10 items-center justify-center rounded-full bg-[#0075DE] px-5 text-base font-medium text-white hover:bg-[#0063BA]">Get started</router-link>
        </div>
      </div>
    </header>

    <main>
      <!-- ───────────────────────── Hero ───────────────────────── -->
      <section class="mx-auto max-w-[1440px] px-6 pb-16 pt-16 sm:pt-20 lg:px-[60px] lg:pb-24 lg:pt-24">
        <div class="grid items-center gap-12 lg:grid-cols-2 lg:gap-16">
          <div>
            <span class="inline-flex items-center gap-1.5 rounded-[4px] bg-[#F2F9FF] px-2 py-1 text-xs font-medium text-[#0075DE]">
              New · Real-time activity timeline
            </span>
            <h1 class="mt-5 text-[48px] font-semibold leading-[52px] tracking-tight md:text-[72px] md:leading-[76px] lg:text-[96px] lg:leading-[100px]">
              Plan work your team actually finishes.
            </h1>
            <p class="mt-6 max-w-lg text-sm leading-5 text-[#31302E] sm:text-base sm:leading-6">
              TaskFlow brings Kanban boards, subtasks, time tracking and team discussion into one place —
              so nothing falls through the cracks between planning and shipping.
            </p>
            <div class="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
              <router-link
                to="/register"
                class="inline-flex h-[38px] items-center justify-center gap-2 rounded-lg bg-[#0075DE] px-[15px] text-base font-medium leading-6 text-white transition-colors hover:bg-[#0063BA] active:bg-[#005299]"
              >
                Get started free
                <ArrowRight class="size-4" aria-hidden="true" />
              </router-link>
              <a
                href="#how-it-works"
                class="inline-flex h-[38px] items-center justify-center rounded-lg px-[15px] text-base leading-6 text-black/90 transition-colors hover:bg-black/5 hover:text-black/95"
              >
                See how it works
              </a>
            </div>
          </div>

          <!-- Product preview -->
          <div class="overflow-hidden rounded-lg border border-black/[0.05] bg-[#F9F9F8] p-2 shadow-[0px_0.175px_1.041px_rgba(0,0,0,0.01),0px_0.8px_2.925px_rgba(0,0,0,0.02),0px_2.025px_7.847px_rgba(0,0,0,0.027),0px_4px_18px_rgba(0,0,0,0.04)] sm:p-3">
            <img
              src="/dashboard-preview.jpg"
              alt="TaskFlow dashboard showing project overview, task stats and project cards"
              class="w-full rounded-md ring-1 ring-black/[0.05]"
            />
          </div>
        </div>
      </section>

      <!-- ───────────────────────── Features ───────────────────────── -->
      <section id="features" class="border-t border-black/[0.06] bg-[#F9F9F8]">
        <div class="mx-auto max-w-[1440px] px-6 py-16 lg:px-[60px] lg:py-24">
          <div class="mx-auto max-w-2xl text-center">
            <h2 class="text-[36px] font-bold leading-[40px] tracking-tight md:text-[44px] md:leading-[48px] lg:text-[54px] lg:leading-[56px]">
              Everything a task needs, in one card
            </h2>
            <p class="mt-4 text-sm leading-5 text-[#31302E] sm:text-base sm:leading-6">
              Six building blocks that cover the full life of a task — from creation to done.
            </p>
          </div>

          <div class="mt-12 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            <div
              v-for="feature in features"
              :key="feature.title"
              class="rounded-lg border border-black/[0.05] bg-white p-6"
            >
              <div
                class="flex h-11 w-11 items-center justify-center rounded-lg"
                :style="{ backgroundColor: feature.accentBg }"
              >
                <component :is="feature.icon" class="size-5" :style="{ color: feature.accent }" aria-hidden="true" />
              </div>
              <h3 class="mt-4 text-[20px] font-bold leading-[26px] lg:text-[22px] lg:leading-[28px]">
                {{ feature.title }}
              </h3>
              <p class="mt-2 text-sm leading-5 text-[#31302E]">
                {{ feature.description }}
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- ───────────────────────── How it works ───────────────────────── -->
      <section id="how-it-works" class="mx-auto max-w-[1440px] px-6 py-16 lg:px-[60px] lg:py-24">
        <div class="mx-auto max-w-2xl text-center">
          <h2 class="text-[36px] font-bold leading-[40px] tracking-tight md:text-[44px] md:leading-[48px] lg:text-[54px] lg:leading-[56px]">
            Up and running in three steps
          </h2>
        </div>

        <div class="mt-12 grid gap-8 lg:grid-cols-3 lg:gap-6">
          <div v-for="(step, i) in steps" :key="step.number" class="relative">
            <div class="flex items-center gap-4">
              <span class="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[#0075DE] text-sm font-semibold text-white">
                {{ step.number }}
              </span>
              <div class="hidden h-px flex-1 bg-black/[0.08] lg:block" v-if="i < steps.length - 1"></div>
            </div>
            <h3 class="mt-4 text-[20px] font-bold leading-[26px]">{{ step.title }}</h3>
            <p class="mt-2 max-w-sm text-sm leading-5 text-[#31302E]">{{ step.description }}</p>
          </div>
        </div>
      </section>

      <!-- ───────────────────────── Testimonials ───────────────────────── -->
      <section id="testimonials" class="border-t border-black/[0.06] bg-[#F2F9FF]">
        <div class="mx-auto max-w-[1440px] px-6 py-16 lg:px-[60px] lg:py-24">
          <div class="mx-auto max-w-2xl text-center">
            <h2 class="text-[36px] font-bold leading-[40px] tracking-tight md:text-[44px] md:leading-[48px] lg:text-[54px] lg:leading-[56px]">
              Teams keep coming back to the board
            </h2>
          </div>

          <div class="mt-12 grid gap-5 md:grid-cols-3">
            <div
              v-for="t in testimonials"
              :key="t.name"
              class="flex h-full flex-col rounded-lg border border-black/[0.05] bg-white p-6"
            >
              <Quote class="size-5 text-[#0075DE]" aria-hidden="true" />
              <p class="mt-4 flex-1 text-sm leading-5 text-black/90">{{ t.quote }}</p>
              <div class="mt-6 flex items-center gap-3">
                <span class="flex h-9 w-9 items-center justify-center rounded-full bg-[#0075DE] text-xs font-semibold text-white">
                  {{ t.initials }}
                </span>
                <div>
                  <p class="text-sm font-medium leading-5">{{ t.name }}</p>
                  <p class="text-xs leading-4 text-black/50">{{ t.role }}</p>
                </div>
                <div class="ml-auto flex gap-0.5">
                  <Star v-for="n in 5" :key="n" class="size-3.5 fill-[#FFB110] text-[#FFB110]" aria-hidden="true" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ───────────────────────── Final CTA ───────────────────────── -->
      <section class="mx-auto max-w-[1440px] px-6 py-16 lg:px-[60px] lg:py-24">
        <div class="rounded-lg bg-[#0075DE] px-6 py-12 text-center sm:px-12 lg:py-16">
          <h2 class="text-[36px] font-bold leading-[40px] tracking-tight text-white md:text-[44px] md:leading-[48px]">
            Bring your team's work into one board
          </h2>
          <p class="mx-auto mt-4 max-w-md text-sm leading-5 text-white/80 sm:text-base sm:leading-6">
            Free to start. Set up your first project in under two minutes.
          </p>
          <div class="mt-8 flex flex-col items-center justify-center gap-3 sm:flex-row">
            <router-link
              to="/register"
              class="inline-flex h-[38px] items-center justify-center gap-2 rounded-lg bg-white px-[15px] text-base font-medium leading-6 text-[#0075DE] transition-colors hover:bg-white/90"
            >
              Get started free
              <ArrowRight class="size-4" aria-hidden="true" />
            </router-link>
            <router-link
              to="/login"
              class="inline-flex h-[38px] items-center justify-center rounded-lg border border-white/30 px-[15px] text-base leading-6 text-white transition-colors hover:bg-white/10"
            >
              Log in
            </router-link>
          </div>
          <ul class="mt-8 flex flex-wrap items-center justify-center gap-x-6 gap-y-2 text-xs leading-4 text-white/80">
            <li class="flex items-center gap-1.5"><Check class="size-3.5" aria-hidden="true" /> No credit card required</li>
            <li class="flex items-center gap-1.5"><Check class="size-3.5" aria-hidden="true" /> Unlimited tasks</li>
            <li class="flex items-center gap-1.5"><Check class="size-3.5" aria-hidden="true" /> Set up in minutes</li>
          </ul>
        </div>
      </section>
    </main>

    <!-- ───────────────────────── Footer ───────────────────────── -->
    <footer class="border-t border-black/[0.06]">
      <div class="mx-auto max-w-[1440px] px-6 py-12 lg:px-[60px]">
        <div class="grid gap-10 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <div class="flex items-center gap-2.5">
              <div class="flex h-8 w-8 items-center justify-center overflow-hidden rounded-lg ring-1 ring-black/5">
                <img src="/logo.png" alt="Product logo" class="h-full w-full object-contain p-1" />
              </div>
              <span class="text-base font-semibold tracking-tight">TaskFlow</span>
            </div>
            <p class="mt-3 max-w-xs text-sm leading-5 text-[#31302E]">
              Kanban boards, subtasks, time tracking and team discussion — in one connected workspace.
            </p>
          </div>

          <div>
            <p class="text-xs font-medium leading-4 text-black/45">Product</p>
            <ul class="mt-3 space-y-2.5">
              <li><a href="#features" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">Features</a></li>
              <li><a href="#how-it-works" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">How it works</a></li>
              <li><router-link to="/register" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">Get started</router-link></li>
            </ul>
          </div>

          <div>
            <p class="text-xs font-medium leading-4 text-black/45">Account</p>
            <ul class="mt-3 space-y-2.5">
              <li><router-link to="/login" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">Log in</router-link></li>
              <li><router-link to="/register" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">Create account</router-link></li>
            </ul>
          </div>

          <div>
            <p class="text-xs font-medium leading-4 text-black/45">Resources</p>
            <ul class="mt-3 space-y-2.5">
              <li><a href="#testimonials" class="text-xs font-medium leading-4 text-[#0075DE] hover:text-[#0063BA] hover:underline">Testimonials</a></li>
            </ul>
          </div>
        </div>

        <div class="mt-10 border-t border-black/[0.06] pt-6">
          <p class="text-xs leading-4 text-black/45">© {{ new Date().getFullYear() }} TaskFlow. All rights reserved.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.landing-page {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
}
</style>
