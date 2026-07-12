# Design System Inspired by Notion

## 1. Visual Theme & Atmosphere

Notion's design system embodies a clean, intelligent, and collaborative workspace aesthetic. The visual language prioritizes clarity and accessibility through generous whitespace, a refined neutral palette anchored by deep blacks and pure whites, and thoughtful accent colors that guide user attention without overwhelming. The atmosphere is modern yet approachable—professional enough for enterprise teams, intuitive enough for individual users. Subtle shadows and micro-interactions create a sense of depth and responsiveness, while playful illustrated agent avatars inject personality and warmth. The system balances minimalism with functionality, supporting complex information hierarchies while maintaining visual simplicity.

**Key Characteristics**
- Clean, spacious layouts with generous whitespace
- Vibrant primary blue driving user actions and focus
- Diverse accent colors (purple, orange, pink, teal) for visual variety and status differentiation
- Subtle elevation and shadow treatments
- Playful, illustrated character elements
- High contrast for accessibility and readability
- Consistent spacing and radius for coherence
- Lightweight, modern typography

## 2. Color Palette & Roles

### Primary
- **Primary Blue** (`#0075DE`): Core CTA buttons, primary interactive elements, and main brand accent
- **Primary Blue Light** (`#097FE8`): Primary hover states, highlighted elements, and secondary emphasis

### Accent Colors
- **Accent Purple** (`#AD6DED`): Alternative accent for special features, premium elements
- **Accent Orange** (`#FF8A33`): Warm accent for notifications, highlights, and visual variety
- **Accent Pink** (`#FF83DD`): Energetic accent for notifications and special status indicators
- **Accent Teal** (`#03C1BA`): Cool accent for additional UI variety and information layering

### Interactive
- **Link Blue** (`#0075DE`): Text links and interactive copy
- **Light Blue Background** (`#62AEF0`): Hover states, subtle backgrounds for interactive zones

### Neutral Scale
- **Black** (`#000000`): Primary text, headings, and dark UI elements
- **Dark Gray** (`#31302E`): Secondary text, muted content, disabled states
- **White** (`#FFFFFF`): Primary background, card surfaces, text on dark backgrounds
- **Light Gray** (`#F6F5F4`): Subtle backgrounds, dividers, reduced-contrast zones

### Surface & Borders
- **Very Light Gray** (`#F9F9F8`): Alternative subtle background
- **Cool White** (`#F2F9FF`): Soft background for information cards and highlighted sections
- **Warm White** (`#FEF3F1`): Warm-toned background for certain content zones
- **Border Gray** (`#E6F3FE`): Subtle borders and dividers on light backgrounds

### Semantic / Status
- **Warning** (`#FFB110`): Warning messages, caution indicators
- **Error** (`#F64932`): Error messages, destructive actions, failure states
- **Success** (`#1AAE39`): Success messages, completed tasks, positive confirmations
- **Alternate Warning** (`#FFB600`): Secondary warning variant

## 3. Typography Rules

### Font Family
**Primary:** NotionInter, -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif

**Secondary:** NotionInter (system defaults if unavailable)

### Hierarchy

| Role | Font | Size | Weight | Line Height | Letter Spacing | Notes |
|------|------|------|--------|-------------|----------------|-------|
| Display / H1 | NotionInter | 96px | 600 | 100px | 0px | Hero headlines, primary page titles |
| Heading 1 / H2 | NotionInter | 54px | 700 | 56px | 0px | Major section headers |
| Heading 2 / H3 | NotionInter | 22px | 700 | 28px | 0px | Subsection headers, card titles |
| Body | NotionInter | 14px | 400 | 20px | 0px | Primary content, descriptions |
| Body Emphasis | NotionInter | 14px | 500 | 20px | 0px | Highlighted inline text, labels |
| Link | NotionInter | 16px | 400 | 24px | 0px | Navigation links, hypertext |
| Link Small | NotionInter | 12px | 500 | 16px | 0px | Footer links, secondary actions |
| List Item | NotionInter | 12px | 500 | 16px | 0px | Bullet points, list content |
| Code / Monospace | NotionInter | 14px | 400 | 20px | 0px | Code blocks, inline code |

### Principles
- Start with generous, readable sizes; reduce only for metadata and secondary content
- Use weight progression (400 → 500 → 600 → 700) to establish hierarchy, not size alone
- Maintain line-height at 1.4–1.5× font size for optimal readability
- Use the body role (14px) as the system baseline; scale up or down contextually
- Apply emphasis weight (500) to interactive text to improve scannability
- Preserve 14px minimum for body text; never drop below 12px except for captions

## 4. Component Stylings

### Buttons

#### Primary Button
- **Background:** `#0075DE`
- **Text Color:** `#FFFFFF`
- **Font Size:** `16px`
- **Font Weight:** `500`
- **Padding:** `6px 15px`
- **Border Radius:** `8px`
- **Border:** `1px solid rgba(255, 255, 255, 0)`
- **Height:** `38px`
- **Box Shadow:** `none`
- **Line Height:** `24px`
- **Hover State:** Background `#0063BA`, brightness -10%
- **Active State:** Background `#005299`, brightness -20%
- **Disabled State:** Background `#E6E6E6`, Text `#999999`

#### Secondary Button (Ghost)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.898)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `5px 10px`
- **Border Radius:** `4px`
- **Border:** `0px none rgba(0, 0, 0, 0.898)`
- **Height:** `30px`
- **Box Shadow:** `none`
- **Line Height:** `24px`
- **Hover State:** Background `rgba(0, 0, 0, 0.05)`, Text `rgba(0, 0, 0, 0.95)`
- **Active State:** Background `rgba(0, 0, 0, 0.1)`

#### Icon Button (Minimal)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.898)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `11px 11px`
- **Border Radius:** `0px`
- **Border:** `0px none`
- **Box Shadow:** `none`
- **Height:** `auto`
- **Hover State:** Background `rgba(0, 0, 0, 0.05)`

#### Rounded Button (Pill)
- **Background:** `#0075DE`
- **Text Color:** `#FFFFFF`
- **Font Size:** `16px`
- **Font Weight:** `500`
- **Padding:** `8px 20px`
- **Border Radius:** `9999px`
- **Border:** `1px solid rgba(255, 255, 255, 0)`
- **Height:** `40px`
- **Hover State:** Background `#0063BA`

### Cards & Containers

#### Featured Card (Light Blue)
- **Background:** `#F2F9FF`
- **Text Color:** `rgba(0, 0, 0, 0.898)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `16px`
- **Border Radius:** `8px`
- **Border:** `1px solid rgba(0, 0, 0, 0.05)`
- **Height:** `256px`
- **Width:** `254px`
- **Box Shadow:** `none`
- **Line Height:** `24px`

#### Content Card (No Border)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.95)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `0px`
- **Border Radius:** `0px`
- **Border:** `0px none`
- **Height:** `254px`
- **Width:** `252px`
- **Box Shadow:** `none`

#### Image Card (Rounded Top)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.898)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `0px`
- **Border Radius:** `8px 8px 0px 0px`
- **Border:** `0px none`
- **Height:** `126px`
- **Width:** `220px`
- **Box Shadow:** `none`
- **Line Height:** `0px`

### Inputs & Forms

#### Text Input
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.95)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `7px 10px 7px 30px`
- **Border Radius:** `5px`
- **Border:** `1px solid rgba(0, 0, 0, 0.08)`
- **Height:** `100%`
- **Width:** `100%`
- **Box Shadow:** `none`
- **Line Height:** `24px`
- **Focus State:** Border `1px solid #0075DE`, Box Shadow `0px 0px 0px 3px rgba(7, 127, 232, 0.1)`
- **Placeholder:** Color `rgba(0, 0, 0, 0.4)`, Font Weight `400`

#### Input with Icon (Search)
- **Padding Left:** `30px` (icon space)
- **Icon Color:** `rgba(0, 0, 0, 0.5)`
- **Icon Size:** `16px`

### Navigation

#### Main Navigation Bar
- **Background:** `#FFFFFF`
- **Text Color:** `rgba(0, 0, 0, 0.898)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `0px`
- **Border Radius:** `0px`
- **Border:** `0px none`
- **Height:** `64px`
- **Width:** `100%`
- **Box Shadow:** `rgba(0, 0, 0, 0) 0px 1px 0px 0px`
- **Line Height:** `24px`
- **Hover State (Links):** Color `#0075DE`, underline appears
- **Active State (Links):** Color `#0075DE`, Font Weight `500`

### Links

#### Text Link (Primary)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `rgba(0, 0, 0, 0.95)`
- **Font Size:** `16px`
- **Font Weight:** `400`
- **Padding:** `0px`
- **Border Radius:** `0px`
- **Border:** `0px none`
- **Box Shadow:** `none`
- **Line Height:** `24px`
- **Hover State:** Text Decoration `underline`, Color `#0075DE`

#### Link Small (Secondary/Footer)
- **Background:** `rgba(0, 0, 0, 0)`
- **Text Color:** `#0075DE`
- **Font Size:** `12px`
- **Font Weight:** `500`
- **Padding:** `0px`
- **Border Radius:** `0px`
- **Border:** `0px none`
- **Box Shadow:** `none`
- **Line Height:** `16px`
- **Hover State:** Color `#0063BA`, Text Decoration `underline`

### Status Badges

#### Success Badge
- **Background:** `rgba(26, 174, 57, 0.1)`
- **Text Color:** `#1AAE39`
- **Font Size:** `12px`
- **Font Weight:** `500`
- **Padding:** `4px 8px`
- **Border Radius:** `4px`
- **Border:** `1px solid #1AAE39`

#### Warning Badge
- **Background:** `rgba(255, 177, 16, 0.1)`
- **Text Color:** `#FFB110`
- **Font Size:** `12px`
- **Font Weight:** `500`
- **Padding:** `4px 8px`
- **Border Radius:** `4px`
- **Border:** `1px solid #FFB110`

#### Error Badge
- **Background:** `rgba(246, 73, 50, 0.1)`
- **Text Color:** `#F64932`
- **Font Size:** `12px`
- **Font Weight:** `500`
- **Padding:** `4px 8px`
- **Border Radius:** `4px`
- **Border:** `1px solid #F64932`

## 5. Layout Principles

### Spacing System
- **Base Unit:** `4px`
- **Scale:** `4px`, `8px`, `12px`, `16px`, `20px`, `24px`, `32px`, `36px`, `60px`, `64px`, `80px`, `104px`
- **Usage Context:**
  - Micro gaps (icon-to-text): `4px`–`8px`
  - Component padding: `12px`–`24px`
  - Section margins: `32px`–`64px`
  - Page-level padding: `60px`–`104px`
  - Grid gutters: `20px`–`36px`

### Grid & Container
- **Max Width:** `1440px` (desktop containers)
- **Column Strategy:** 12-column responsive grid, collapsing to 6-column at tablet (768px) and 1-column at mobile (480px)
- **Section Patterns:**
  - Hero sections: Full bleed with centered content container
  - Two-column layouts: 60/40 or 50/50 split with `36px` gutter
  - Three-column card grids: Equal columns with `20px` gap
  - Sidebar + content: Fixed 280px sidebar + flexible content, `36px` gutter

### Whitespace Philosophy
Generous whitespace is a core principle. Breathing room around text, cards, and sections improves cognitive load and visual hierarchy. Minimum margins between major sections: `64px` vertical. Cards and components within sections: `20px`–`24px` internal padding. Use white (or light neutral) backgrounds to maximize whitespace perception and support focus.

### Border Radius Scale
- **Sharp Corners:** `0px` (navigation bars, full-width sections)
- **Subtle Radius:** `4px` (buttons, badges, minimal UI elements)
- **Standard Radius:** `5px` (form inputs, small components)
- **Rounded Components:** `8px` (cards, dropdowns, moderate UI elements)
- **Pill-Shaped:** `9999px` (buttons with max rounding, tags, avatars)
- **Contextual Rounding:** `8px 8px 0px 0px` (card images with flat bottom edge)

## 6. Depth & Elevation

| Level | Treatment | Use |
|-------|-----------|-----|
| Flat (L0) | No shadow, `box-shadow: none` | Backgrounds, body text, cards on light surfaces |
| Subtle (L1) | `rgba(0, 0, 0, 0.01) 0px 0.175px 1.041px 0px, rgba(0, 0, 0, 0.02) 0px 0.8px 2.925px 0px, rgba(0, 0, 0, 0.027) 0px 2.025px 7.847px 0px, rgba(0, 0, 0, 0.04) 0px 4px 18px 0px` | Dropdowns, small overlays, floating elements |
| Nav Divider (L2) | `rgba(0, 0, 0, 0) 0px 1px 0px 0px` | Navigation bar bottom border, horizontal dividers |
| Elevated (Custom) | `0px 8px 24px rgba(0, 0, 0, 0.12)` | Modals, full-screen overlays (inferred) |

**Shadow Philosophy:**
Notion's shadow system is restrained and naturalistic. Shadows are used sparingly to create layering and separation without drawing excessive attention. Subtle Level 1 shadows on dropdowns provide just enough depth to distinguish them from the background. Navigation uses a minimal divider rather than a shadow. Overall, the system relies on background color contrast and layout spacing more than shadows for visual hierarchy. This creates a clean, modern aesthetic that feels light and approachable.

## 7. Do's and Don'ts

### Do
- Use the primary blue (`#0075DE`) for all primary CTAs and main interactive elements
- Apply `16px` / `24px` line height for body text to ensure legibility
- Maintain consistent `8px` or `16px` internal padding on cards and containers
- Use accent colors (purple, orange, pink, teal) for status differentiation and visual variety
- Include ample whitespace (minimum `32px` margin) between major page sections
- Apply the subtle shadow (L1) only to floating/overlay elements like dropdowns
- Use `12px` / `500` weight for labels, badges, and list items
- Ensure hover states for all interactive elements (buttons, links, inputs)
- Test contrast ratios for text on colored backgrounds (minimum 4.5:1 for WCAG AA)

### Don't
- Overuse shadows; default to no shadow (`box-shadow: none`)
- Mix primary blue with heavy shadows; keep depth subtle
- Apply border radius exceeding `9999px` or use inconsistent values
- Use text color darker than `#000000` or lighter than `#FFFFFF` for critical content
- Reduce font size below `12px` for body content; use `14px` as the baseline
- Stack too many accent colors in a single view; limit to 2–3 per section
- Add padding smaller than `4px` or larger than `24px` without design justification
- Apply colors from the status palette (green, red, yellow) to primary CTAs; reserve for feedback
- Use the dark gray (`#31302E`) for primary text; reserve for secondary or disabled states

## 8. Responsive Behavior

### Breakpoints

| Breakpoint | Width | Key Changes |
|-----------|-------|------------|
| Mobile | 320px – 479px | Single column, stacked cards, full-width buttons, `16px` padding, `12px` gaps |
| Tablet | 480px – 767px | 2-column grid, half-width cards, `24px` padding, `16px` gaps |
| Desktop | 768px – 1440px | 3-column grid, optimized card layouts, `32px–64px` padding, `20px–36px` gaps |
| Large Desktop | 1440px+ | Max-width container at `1440px` centered, generous side margins |

### Touch Targets
- **Minimum Interactive Size:** `48px × 48px` (buttons, icon buttons, touch-friendly inputs)
- **Comfortable Size:** `44px – 56px` for mobile touch targets
- **Spacing Between:** Minimum `8px` gap between interactive elements to prevent mis-taps
- **Link / Button Padding:** Ensure clickable area extends `8px` beyond visual bounds on mobile

### Collapsing Strategy
- **Navigation:** Full horizontal menu on desktop; collapse to hamburger icon on tablet (768px breakpoint); stack vertically in overlay on mobile
- **Sidebar:** Fixed 280px sidebar on desktop; converts to bottom drawer or hidden accordion on tablet; removes entirely on mobile (content flows single-column)
- **Multi-Column Layouts:** 3 columns on desktop → 2 columns at tablet → 1 column (stacked) on mobile
- **Cards:** Full-width cards on mobile with `12px`–`16px` padding; 2–3 columns at tablet; 3–4 columns on desktop
- **Typography:** H1 stays `96px` on large screens but reduces to `48px` on mobile, H2 scales from `54px` → `36px`
- **Spacing:** All margins and padding scale down by ~30%–40% on mobile (e.g., `64px` section margins → `32px` on mobile)

## 9. Agent Prompt Guide

### Quick Color Reference
- **Primary CTA:** Primary Blue (`#0075DE`)
- **Hover / Secondary Emphasis:** Primary Blue Light (`#097FE8`)
- **Headings / Body Text:** Black (`#000000`)
- **Secondary Text:** Dark Gray (`#31302E`)
- **Background:** White (`#FFFFFF`)
- **Card Background (Featured):** Cool White (`#F2F9FF`)
- **Subtle Divider / Border:** Border Gray (`#E6F3FE`)
- **Success:** Success (`#1AAE39`)
- **Warning:** Warning (`#FFB110`)
- **Error:** Error (`#F64932`)
- **Accent (Alt):** Accent Purple (`#AD6DED`), Accent Orange (`#FF8A33`), Accent Pink (`#FF83DD`), Accent Teal (`#03C1BA`)

### Iteration Guide

1. **Start with Baseline:** Use NotionInter at `14px` / `400` weight for all body text; scale up to `16px` for interactive elements, down to `12px` for labels/badges.

2. **Hierarchy via Weight:** Establish visual hierarchy through font weight (400 → 500 → 600 → 700), not size alone. Use `700` weight exclusively for headings.

3. **Primary Actions:** All primary CTAs use `#0075DE` background with white text, `16px` / `500` weight, `8px` border radius, and `6px 15px` padding. Always include a hover state (`#0063BA`).

4. **Spacing Discipline:** Build all layouts using the spacing scale (`4px`, `8px`, `12px`, `16px`, `20px`, `24px`, `32px`, `36px`, `60px`, `64px`, `80px`, `104px`). Never use arbitrary spacing values.

5. **Cards & Containers:** Default card styling is `16px` padding, `8px` border radius, `1px solid rgba(0, 0, 0, 0.05)` border, and no shadow. Use light blue background (`#F2F9FF`) for featured/highlighted cards.

6. **Inputs:** Text inputs default to transparent background, `1px solid rgba(0, 0, 0, 0.08)` border, `5px` radius, and `7px 10px 7px 30px` padding (left-aligned icon). Focus state adds `3px` box-shadow with `rgba(7, 127, 232, 0.1)` blur.

7. **Links & Navigation:** Text links are black by default (`rgba(0, 0, 0, 0.95)`), shift to primary blue (`#0075DE`) on hover with underline. Small links in footer are `12px` / `500` weight, blue (`#0075DE`).

8. **Shadows:** Use the Subtle (L1) shadow only for dropdowns and floating elements. Navigation uses a minimal `1px` bottom border instead. Default all other components to no shadow.

9. **Status Colors:** Reserve green (`#1AAE39`), yellow (`#FFB110`), and red (`#F64932`) exclusively for feedback (success, warning, error states). Never use for primary actions or neutral elements.

10. **Responsive Collapse:** At `768px` breakpoint, collapse multi-column layouts to 2 columns; at `480px`, collapse to single column. Reduce all padding by ~30% on mobile. Navigation converts to hamburger menu at `768px`.

11. **Whitespace & Breathing Room:** Maintain minimum `32px` vertical margin between major sections on desktop, `16px` on mobile. Internal card padding: `16px` standard, `20px`–`24px` for spacious layouts.

12. **Accessibility:** Ensure all text meets WCAG AA contrast (4.5:1 minimum). Never rely solely on color to convey meaning; use icons, text, or patterns. Test interactive elements at `48px × 48px` minimum on touch devices.