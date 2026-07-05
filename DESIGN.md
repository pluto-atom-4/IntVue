# DESIGN.md — Visual Design System

IntVue's design system ensures consistent, professional, accessible UI across all pages. This file provides an overview and index to focused design rules in the `.claude/rules/` directory.

---

## Product Vision & Design Philosophy

**IntVue** is a professional interview practice platform. The visual design reflects this mission:

- **Professional:** Clean, distraction-free interface focused on interview preparation
- **Accessible:** Full keyboard navigation, screen reader support, high-contrast themes
- **Consistent:** Semantic tokens ensure uniform styling across all pages
- **Clear:** High contrast, readable text, obvious interactive affordances

The design is guided by **Fluent Design System** principles (Microsoft's modern design language) and integrates seamlessly with WinUI 3's built-in theme resources.

---

## Three Core Design Rules

### Rule 1: Semantic Tokens (Role-Based Naming)

All colors, spacing, and typography use **semantic token names**, not hard-coded values. Tokens are named by their *role* in the UI:

```
text-primary       → Main text
surface-primary    → Main background
control-default    → Default control fill
semantic-error     → Error state (red)
spacing-md         → Standard padding (12px)
```

**Why:** Semantic tokens automatically adapt to light/dark/high-contrast themes via WinUI 3's theme resources. Never hard-code colors or spacing.

### Rule 2: Explicit Reasoning (Intent + Boundaries)

Each token has clear intent and boundaries:

```
Token: surface-primary
Intent: Primary background for main content areas
Use Cases: Page backgrounds, main content panels
Boundaries: Do NOT use for text, hover states, warnings
```

### Rule 3: Product Narrative

Tokens support IntVue's mission: **clear, professional, accessible**. When styling, ask:
- Does this support focused, distraction-free practice?
- Is this readable for all users?
- Is the hierarchy clear?

---

## Design Rules by Category

| Category | File | What's Inside |
|---|---|---|
| **Colors & Themes** | `.claude/rules/design-colors.rules.md` | Text, surface, control, semantic, button colors + light/dark mappings |
| **Spacing & Layout** | `.claude/rules/design-spacing.rules.md` | 8px base grid, border radius, elevation, padding rules |
| **Typography** | `.claude/rules/design-typography.rules.md` | Font sizes (caption–display), weights, line heights |
| **Components** | `.claude/rules/design-components.rules.md` | Button states, form controls, keyboard navigation, patterns |

---

## Quick Reference: Semantic Tokens

### Text Colors
```
text-primary          → Main text (readable)
text-secondary        → Secondary/muted
text-tertiary         → Tertiary (less emphasis)
text-disabled         → Disabled/inactive
text-critical         → Error text (red)
```

### Surface Colors
```
surface-primary       → Main background
surface-secondary     → Nested/secondary area
surface-tertiary      → Further nesting
surface-dim           → Dimmed/inactive
surface-elevated      → Floating/cards (raised)
```

### Control Colors
```
control-default       → Default fill
control-secondary     → Secondary fill
control-tertiary      → Tertiary fill
control-disabled      → Disabled fill
control-input         → Active input field
```

### Semantic Colors
```
semantic-success      → Success state (green)
semantic-warning      → Warning state (orange)
semantic-error        → Error/critical (red)
semantic-info         → Informational (blue)
```

### Spacing Scale (8px Base)
```
spacing-xs            → 4px (minimal)
spacing-sm            → 8px (tight)
spacing-md            → 12px (standard)
spacing-lg            → 16px (comfortable)
spacing-xl            → 24px (generous)
```

### Typography Scale
```
type-caption          → 12px (labels, timestamps)
type-body             → 14px (default text, buttons)
type-subtitle         → 16px (section headers)
type-title            → 20px (page titles)
type-display          → 32px (hero text)
```

---

## How AI Agents Use This

1. **Read DESIGN.md first** — Understand IntVue's design philosophy and token naming
2. **Consult specific rule files** — Based on the task:
   - Choosing colors? → `design-colors.rules.md`
   - Styling buttons? → `design-components.rules.md`
   - Setting padding? → `design-spacing.rules.md`
   - Font size? → `design-typography.rules.md`
3. **Apply tokens in XAML** — Never hard-code values
   ```xaml
   <!-- GOOD: Uses token-mapped brush -->
   <TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
   
   <!-- AVOID: Hard-coded color -->
   <TextBlock Foreground="#000000" />
   ```

---

## Essential Rules Summary

1. **Never hard-code colors** → Use `{ThemeResource ...Brush}`
2. **Never hard-code spacing** → Use semantic tokens (8px base grid)
3. **Never hard-code typography** → Use defined font sizes and weights
4. **Always use x:Bind** → Never use `{Binding}` (performance & safety)
5. **Always add automation properties** → `AutomationProperties.Name` on interactive controls
6. **Always support themes** → Light/dark/high-contrast via `{ThemeResource}`
7. **Always test keyboard navigation** → Tab, Escape, Alt+AccessKey

---

## Cross-References

| Document | When to Consult |
|---|---|
| `CLAUDE.md` | Project-wide rules, architecture, development commands |
| `Views/CLAUDE.md` | XAML data binding, theming, accessibility |
| `winui-best-practices.instructions.md` | WinUI 3 patterns, MVVM, dependency injection |
| `.claude/rules/design-colors.rules.md` | Color tokens & WinUI brush mappings |
| `.claude/rules/design-spacing.rules.md` | Spacing, sizing, layout rules |
| `.claude/rules/design-typography.rules.md` | Font sizes, weights, line heights |
| `.claude/rules/design-components.rules.md` | Component patterns & accessibility |
| [Fluent Design System](https://learn.microsoft.com/en-us/windows/apps/design/) | Official spacing, typography, color principles |
| [WinUI 3 Gallery](https://github.com/microsoft/WinUI-Gallery) | Control examples & demonstrations |

---

## Validation Checklist

Before generating UI:
- [ ] Every color uses `{ThemeResource ...}` (no hex values)
- [ ] Every spacing references semantic tokens (no arbitrary margins)
- [ ] Every font size matches the typography scale
- [ ] Every button has `AutomationProperties.Name`
- [ ] Focus indicators are visible (trust WinUI defaults)
- [ ] Component patterns match Section 6 in design-components.rules.md
- [ ] Light/dark/high-contrast themes supported via `{ThemeResource}`
