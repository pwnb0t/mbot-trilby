# m'bot Sidekick App (`ownbot-sidekick` repo)


Alright, we need to do a pass on the visuals of this thing. It is functional, and that's great. It looks really bad. I would like to improve the look of this and before we implement it, I want to make sure we have a plan for it.

What do you need to know in order to make this look better?

Here are a couple of ideas I have:
* I would like all the clip buttons to look the same no matter where they are (so this could be extracted as some kind of component maybe)
* I do not like the current look of the buttons. They are very bright. They are "unskinned" and ugly
* I want it to be a modern looking UI
* I'm ok with doing some research into what it needs to look like if needed, but I'll need some help in getting started.


## Codex understanding and plan

### Goals confirmed
- Dark, low-to-medium contrast UI.
- Modern look without visual noise.
- Keep bottom reserved strip behavior unchanged.
- Clip buttons should use one reusable style everywhere.
- Text-only clip buttons (no icon in button face).
- Optional subtle animation is acceptable but not required.
- Include light m'bot identity touch (small/secondary, not dominant).

### Option A: Minimal Dark (clean + safe)
- Visual direction:
  - Flat dark surfaces with soft borders.
  - Very restrained accent usage.
  - No blur/glass effects.
- Palette (example):
  - Background: `#101214`
  - Panel: `#161A1F`
  - Border: `#2A313A`
  - Text primary: `#E6EAF0`
  - Text secondary: `#AAB4C0`
  - Accent: `#5D8BFF`
- Buttons:
  - Rounded rectangle, medium padding, subtle hover tint.
  - Pressed state darkens slightly with tiny border accent.
  - All clip buttons share one `ClipButtonStyle`.
- Pros:
  - Fastest to implement and hardest to get wrong.
  - High readability and consistent feel.
- Cons:
  - Least visually distinct.

### Option B: Glass Dark (modern + atmospheric)
- Visual direction:
  - Semi-transparent layered panels.
  - Soft glow/border accents and subtle depth.
  - Slight frosted look (without overdoing blur).
- Palette (example):
  - Backdrop tint: `#66090B0F`
  - Glass panel: `#AA1A2028`
  - Glass border: `#55AFC3E8`
  - Text primary: `#F3F6FB`
  - Text secondary: `#B9C3D0`
  - Accent: `#7DB0FF`
- Buttons:
  - Glass-style dark tiles with gradient + border glow on hover.
  - Slight scale-up on hover (very subtle).
- Pros:
  - Looks more premium/modern.
  - Better visual identity potential.
- Cons:
  - More effort, easier to make too flashy if not careful.

### Option C: Hybrid (recommended first pass)
- Visual direction:
  - Minimal base with a light glass touch only on key containers.
  - Clean, dark UI with restrained accent and subtle polish.
- Palette (example):
  - Background: `#0F1217`
  - Primary panel: `#171C24`
  - Secondary panel: `#1F2630`
  - Border: `#2C3644`
  - Text primary: `#E9EDF4`
  - Text secondary: `#A8B2C0`
  - Accent: `#6A96FF`
- Buttons:
  - Unified clip button style used everywhere.
  - Slight hover transition (opacity + border color).
  - No heavy glow, no harsh brightness.
- m'bot identity touch:
  - Small icon/badge in top bar only.
  - Optional very subtle fedora-toned accent (muted blue-gray).
- Pros:
  - Strong improvement with low risk.
  - Modern but still practical for iterative tuning.
- Cons:
  - Less dramatic than full glass.

### Recommended implementation plan (after option choice)
1. Define design tokens in XAML resources (colors, spacing, radius, shadow).
2. Create reusable styles:
   - `ClipButtonStyle`
   - `TopBarButtonStyle`
   - `PanelStyle`
   - `SearchTextStyle`
3. Apply styles to all existing controls (no logic changes).
4. Add subtle hover/press transitions (if chosen).
5. Add small m'bot identity element in header.
6. Do one polish pass for spacing/alignment/contrast.

### Open decisions needed before implementation
1. Pick one: `Option A`, `Option B`, or `Option C`.
2. Accent color preference:
   - cool blue (recommended)
   - muted teal
   - neutral gray-only
3. Animation preference:
   - none
   - subtle hover only (recommended)
