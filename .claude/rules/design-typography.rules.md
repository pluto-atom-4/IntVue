# Design Rule: Typography Tokens

IntVue typography follows WinUI 3 and Fluent Design standards. Use predefined font sizes, weights, and line heights for consistency.

---

## Font Family

IntVue uses **Segoe UI** (standard for WinUI 3 and Windows apps):

```xaml
<!-- Implicit (all WinUI controls default to Segoe UI) -->
<TextBlock Text="Default text" />

<!-- Explicit (for custom components) -->
<TextBlock Text="Custom text" FontFamily="Segoe UI" />
```

**Do NOT:** Use custom fonts unless explicitly designed for IntVue

---

## Font Size Scale (5 Tiers)

| Token | Size | Use Case | Example |
|---|---|---|---|
| `type-caption` | 12px | Labels, timestamps, hints | "Modified 5 hours ago" |
| `type-body` | 14px | Body text, button content, default | Button text, paragraphs |
| `type-subtitle` | 16px | Section headers, emphasis | "Recording Controls" |
| `type-title` | 20px | Page titles, major headers | "Interview Practice" |
| `type-display` | 32px | Hero text, large displays | Large countdown (custom: 72px) |

**Usage:**
```xaml
<!-- Caption (12px) - Small labels -->
<TextBlock Text="Camera 1" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />

<!-- Body (14px) - Default text -->
<Button Content="Start Recording" />
<TextBlock Text="Ready to begin recording session" FontSize="14" />

<!-- Subtitle (16px) - Section header -->
<TextBlock Text="Recording Controls" FontSize="16" FontWeight="SemiBold" />

<!-- Title (20px) - Page header -->
<TextBlock Text="Interview Practice" FontSize="20" FontWeight="Bold" />

<!-- Display (32px) - Hero text -->
<TextBlock Text="3" FontSize="32" FontWeight="Bold" Foreground="{ThemeResource SystemFillColorCautionBrush}" />

<!-- Custom: Countdown (72px - feature-specific exception) -->
<TextBlock Text="3" FontSize="72" FontWeight="Bold" Foreground="{x:Bind ViewModel.CountdownColor, Mode=OneWay}" />
```

**Do NOT:**
```xaml
<!-- ❌ Avoid: Font sizes outside the scale -->
<TextBlock FontSize="11" />   <!-- Too small, use 12px -->
<TextBlock FontSize="18" />   <!-- Between scales, use 16px or 20px -->
<TextBlock FontSize="48" />   <!-- Use 32px unless feature-specific -->

<!-- ✓ Good: Use defined scale -->
<TextBlock FontSize="12" />   <!-- type-caption -->
<TextBlock FontSize="16" />   <!-- type-subtitle -->
<TextBlock FontSize="32" />   <!-- type-display -->
```

**Exception:** Countdown display uses 72px (documented as feature-specific override in Section 6 of `design-components.rules.md`)

---

## Font Weights (3 Weights)

| Weight | Value | Use Case | Example |
|---|---|---|---|
| Regular | 400 | Body text, default | Paragraphs, button content |
| SemiBold | 600 | Emphasis, labels, subtitles | Section headers, form labels |
| Bold | 700 | Headings, strong emphasis | Page titles, prominent text |

**Usage:**
```xaml
<!-- Regular (default, body text) -->
<TextBlock Text="Ready to start recording" FontWeight="Normal" />

<!-- SemiBold (section header) -->
<TextBlock Text="Recording Controls" FontSize="16" FontWeight="SemiBold" />

<!-- Bold (page title) -->
<TextBlock Text="Interview Practice" FontSize="20" FontWeight="Bold" />

<!-- Bold + Large (strong emphasis) -->
<TextBlock Text="● Recording" FontSize="14" FontWeight="Bold" Foreground="{ThemeResource SystemFillColorCriticalBrush}" />
```

**Do NOT:**
```xaml
<!-- ❌ Avoid: Light or Extra-Bold weights -->
<TextBlock FontWeight="Light" />          <!-- Not in design system -->
<TextBlock FontWeight="ExtraBlack" />     <!-- Not in design system -->

<!-- ✓ Good: Use three defined weights -->
<TextBlock FontWeight="Normal" />         <!-- 400 -->
<TextBlock FontWeight="SemiBold" />       <!-- 600 -->
<TextBlock FontWeight="Bold" />           <!-- 700 -->
```

---

## Line Height

Line height (leading) affects readability and accessibility:

| Context | Ratio | Pixels (for reference) | Usage |
|---|---|---|---|
| Body text | 1.4 | ~20px (for 14px font) | Default readable text, paragraphs |
| Headings | 1.2 | ~24px (for 20px font) | Compact, strong hierarchy |
| Large display | 1.5 | ~108px (for 72px font) | Accessibility for very large text |

**WinUI Implementation:**
Most WinUI controls use line-height automatically. For custom TextBlocks:

```xaml
<!-- Body text with standard line height (1.4) -->
<TextBlock 
    Text="Recording started at 10:30 AM" 
    FontSize="14"
    LineHeight="20" />

<!-- Heading with compact line height (1.2) -->
<TextBlock 
    Text="Interview Practice" 
    FontSize="20" 
    FontWeight="Bold"
    LineHeight="24" />

<!-- Large display with generous line height (1.5) -->
<TextBlock 
    Text="3" 
    FontSize="72" 
    FontWeight="Bold"
    LineHeight="108" />
```

**Do NOT:**
```xaml
<!-- ❌ Avoid: Excessive line-height (too much gap) -->
<TextBlock FontSize="14" LineHeight="28" />   <!-- 2.0 ratio, too large -->

<!-- ✓ Good: Use standard ratios -->
<TextBlock FontSize="14" LineHeight="20" />   <!-- 1.4 ratio -->
```

---

## Typography Hierarchy Examples

### Page Title + Content
```xaml
<StackPanel Spacing="16">
    <!-- Page title (20px, bold) -->
    <TextBlock 
        Text="Interview Practice" 
        FontSize="20" 
        FontWeight="Bold"
        Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
    
    <!-- Body text (14px, regular) -->
    <TextBlock 
        Text="Ready to begin recording session" 
        FontSize="14"
        Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
    
    <!-- Secondary info (12px, secondary color) -->
    <TextBlock 
        Text="Last recorded 2 hours ago" 
        FontSize="12"
        Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
</StackPanel>
```

### Section with Items
```xaml
<StackPanel Spacing="12">
    <!-- Section header (16px, semibold) -->
    <TextBlock 
        Text="Recording Controls" 
        FontSize="16" 
        FontWeight="SemiBold" />
    
    <!-- Control buttons (14px, body) -->
    <StackPanel Spacing="8">
        <Button Content="Start Preview" />
        <Button Content="Start Recording" Style="{ThemeResource AccentButtonStyle}" />
    </StackPanel>
</StackPanel>
```

### Status Message
```xaml
<!-- Error status (12px caption + 14px body) -->
<StackPanel Spacing="4">
    <TextBlock 
        Text="Error" 
        FontSize="12" 
        FontWeight="SemiBold"
        Foreground="{ThemeResource SystemFillColorCriticalBrush}" />
    <TextBlock 
        Text="Camera initialization failed" 
        FontSize="14"
        Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
</StackPanel>
```

---

## Countdown Display (Feature-Specific Exception)

The countdown timer uses **72px** (not in standard scale) because it's a focal point:

```xaml
<TextBlock 
    Text="3" 
    FontSize="72" 
    FontWeight="Bold"
    Foreground="{x:Bind ViewModel.CountdownColor, Mode=OneWay}"
    HorizontalAlignment="Center"
    VerticalAlignment="Center"
    LineHeight="108" />
```

**Justification:** Countdown is a key feature; large size ensures visibility and urgency during critical moments. This exception is documented and intentional.

---

## Accessibility Considerations

### Line Height for Readability
- **Body text:** 1.4 ratio (or higher) for comfortable reading
- **Large text:** 1.5 ratio for accessibility (less cramped)
- **Headings:** 1.2 ratio OK (short, strong text)

### Color Contrast
- Always pair font sizes with high-contrast colors
- Use `text-primary` for large text (sufficient contrast)
- Use `text-secondary` only for secondary information (not main content)

### Text Size for Accessibility
- Never use fonts below 12px for body text
- Page titles ≥20px for clear hierarchy
- Use SemiBold/Bold for section headers (visual emphasis without larger size)

---

## Essential Rules

1. **Use defined font sizes** — Caption (12px), Body (14px), Subtitle (16px), Title (20px), Display (32px)
2. **Use defined weights** — Regular (400), SemiBold (600), Bold (700) only
3. **Apply line-height** — 1.4 for body, 1.2 for headings, 1.5 for large display
4. **Maintain hierarchy** — Larger/bolder = more important
5. **Test readability** — Ensure sufficient contrast (see design-colors.rules.md)

---

## Anti-Patterns

```xaml
<!-- ❌ Avoid: Custom font sizes outside scale -->
<TextBlock FontSize="11" />    <!-- Use 12px instead -->
<TextBlock FontSize="18" />    <!-- Use 16px or 20px -->
<TextBlock FontSize="24" />    <!-- Use 20px or 32px -->

<!-- ✓ Good: Use defined scale -->
<TextBlock FontSize="12" />    <!-- type-caption -->
<TextBlock FontSize="16" />    <!-- type-subtitle -->
<TextBlock FontSize="20" />    <!-- type-title -->
```

```xaml
<!-- ❌ Avoid: Font weights outside system -->
<TextBlock FontWeight="Light" />
<TextBlock FontWeight="ExtraBlack" />

<!-- ✓ Good: Use three weights -->
<TextBlock FontWeight="Normal" />     <!-- Regular -->
<TextBlock FontWeight="SemiBold" />   <!-- Emphasis -->
<TextBlock FontWeight="Bold" />       <!-- Strong -->
```

```xaml
<!-- ❌ Avoid: Excessive line-height -->
<TextBlock FontSize="14" LineHeight="30" />  <!-- Too much gap -->

<!-- ✓ Good: Standard ratios -->
<TextBlock FontSize="14" LineHeight="20" />  <!-- 1.4 ratio -->
```

---

## Cross-References

- **Color + typography together:** See `.claude/rules/design-colors.rules.md`
- **Component text sizes:** See `.claude/rules/design-components.rules.md` (button text, etc.)
- **Heading hierarchy:** See `.claude/rules/design-components.rules.md` (section headers, etc.)
- **Accessibility:** See `Views/CLAUDE.md` and `accessibility.instructions.md`
