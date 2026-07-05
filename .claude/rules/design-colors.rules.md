# Design Rule: Color & Theme Tokens

All colors in IntVue use WinUI 3 theme resources. This ensures automatic light/dark/high-contrast theme support without hard-coding values.

---

## Token Naming Convention

| Pattern | Meaning | Example |
|---|---|---|
| `text-*` | Text/foreground colors | `text-primary`, `text-disabled` |
| `surface-*` | Background surfaces | `surface-primary`, `surface-elevated` |
| `control-*` | Control fills | `control-default`, `control-input` |
| `stroke-*` | Borders & dividers | `stroke-default`, `stroke-subtle` |
| `semantic-*` | Status/action colors | `semantic-error`, `semantic-success` |
| `button-*` | Button-specific colors | `button-accent-default`, `button-accent-hover` |

---

## Text Colors (5 Tokens)

| Token | WinUI Brush | Intent | Light | Dark |
|---|---|---|---|---|
| `text-primary` | `TextFillColorPrimaryBrush` | Main text | Black | Off-white |
| `text-secondary` | `TextFillColorSecondaryBrush` | Secondary, muted | Dark gray | Light gray |
| `text-tertiary` | `TextFillColorTertiaryBrush` | Tertiary, less emphasis | Medium gray | Medium gray |
| `text-disabled` | `TextFillColorDisabledBrush` | Disabled text | Very light gray | Very dark gray |
| `text-critical` | `SystemFillColorCriticalBrush` | Error text | Red (#E81B23) | Light red (#FF8A80) |

**Usage:**
```xaml
<!-- Main heading -->
<TextBlock Text="Interview Practice" Foreground="{ThemeResource TextFillColorPrimaryBrush}" />

<!-- Secondary info -->
<TextBlock Text="Ready to start" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />

<!-- Error message -->
<TextBlock Text="Camera failed" Foreground="{ThemeResource SystemFillColorCriticalBrush}" />
```

**Do NOT:** Hard-code foreground values like `Foreground="#000000"`

---

## Surface Colors (5 Tokens)

| Token | WinUI Brush | Intent | Light | Dark |
|---|---|---|---|---|
| `surface-primary` | `SolidBackgroundFillColorBaseBrush` | Main content background | Off-white | Very dark gray |
| `surface-secondary` | `SolidBackgroundFillColorSecondaryBrush` | Nested background | Light gray | Dark gray |
| `surface-tertiary` | `SolidBackgroundFillColorTertiaryBrush` | Further nesting | Lighter gray | Darker gray |
| `surface-dim` | `ControlFillColorTransparentBrush` | Dimmed/inactive | Transparent | Transparent |
| `surface-elevated` | `ControlStrongFillColorDefaultBrush` | Cards, popups (raised) | Off-white | Slightly lighter |

**Usage:**
```xaml
<!-- Page background -->
<Page Background="{ThemeResource SolidBackgroundFillColorBaseBrush}">
    <!-- Nested panel -->
    <StackPanel Background="{ThemeResource SolidBackgroundFillColorSecondaryBrush}" Padding="12">
        <TextBlock Text="Controls" />
    </StackPanel>
</Page>

<!-- Card (elevated) -->
<Border Background="{ThemeResource ControlStrongFillColorDefaultBrush}" CornerRadius="8" Padding="12">
    <TextBlock Text="Recording info" />
</Border>
```

**Do NOT:** Use for text fills, interactive states, or warnings

---

## Control Colors (5 Tokens)

| Token | WinUI Brush | Intent | Light | Dark |
|---|---|---|---|---|
| `control-default` | `ControlFillColorDefaultBrush` | Default control fill | Light gray | Dark gray |
| `control-secondary` | `ControlFillColorSecondaryBrush` | Secondary control state | Lighter gray | Darker gray |
| `control-tertiary` | `ControlFillColorTertiaryBrush` | Tertiary control state | Lightest gray | Very dark gray |
| `control-disabled` | `ControlFillColorDisabledBrush` | Disabled/inactive | Very light gray | Very dark gray |
| `control-input` | `ControlFillColorInputActiveBrush` | Active input field | White | Dark gray |

**Usage:**
```xaml
<!-- Default button (implicit) -->
<Button Content="Preview" />

<!-- ComboBox (implicit) -->
<ComboBox>
    <x:String>Camera 1</x:String>
    <x:String>Camera 2</x:String>
</ComboBox>

<!-- Disabled -->
<Button Content="Play" IsEnabled="False" />
```

**Do NOT:** Explicitly set control backgrounds; rely on WinUI default styles

---

## Border & Stroke Colors (3 Tokens)

| Token | WinUI Brush | Intent | Light | Dark |
|---|---|---|---|---|
| `stroke-default` | `ControlBorderBrush` | Default borders | Medium gray | Medium gray |
| `stroke-secondary` | `ControlStrokeColorSecondary` | Secondary borders | Light gray | Very dark gray |
| `stroke-subtle` | `DividerStrokeColorDefaultBrush` | Dividers, faint lines | Very light gray | Very dark gray |

**Usage:**
```xaml
<!-- Divider -->
<Rectangle Height="1" Fill="{ThemeResource DividerStrokeColorDefaultBrush}" />

<!-- Card border -->
<Border BorderBrush="{ThemeResource ControlBorderBrush}" BorderThickness="1">
    <TextBlock Text="Info" />
</Border>
```

**Do NOT:** Use for text, backgrounds, or semantic status

---

## Semantic Colors (4 Tokens)

| Token | WinUI Brush | Meaning | Light | Dark |
|---|---|---|---|---|
| `semantic-success` | `SystemFillColorSuccessBrush` | Success (green) | #107C10 | #6FD056 |
| `semantic-warning` | `SystemFillColorCautionBrush` | Warning (orange) | #FFB900 | #FFD335 |
| `semantic-error` | `SystemFillColorCriticalBrush` | Error/critical (red) | #E81B23 | #FF8A80 |
| `semantic-info` | `SystemFillColorAttentionBrush` | Info/attention (blue) | #0078D4 | #5EB7FF |

**Usage:**
```xaml
<!-- Recording active (red) -->
<Grid Background="{ThemeResource SystemFillColorCriticalBrush}" CornerRadius="4" Padding="8">
    <TextBlock Text="● Recording" Foreground="{ThemeResource TextFillColorPrimaryBrush}" FontWeight="Bold" />
</Grid>

<!-- Countdown warning (orange as seconds decrease) -->
<TextBlock Text="3" FontSize="72" Foreground="{ThemeResource SystemFillColorCautionBrush}" FontWeight="Bold" />

<!-- Success message (green) -->
<TextBlock Text="Saved successfully" Foreground="{ThemeResource SystemFillColorSuccessBrush}" />

<!-- Info message (blue) -->
<TextBlock Text="Initializing..." Foreground="{ThemeResource SystemFillColorAttentionBrush}" />
```

**Do NOT:** Use for decorative elements; use only for status/action messaging

---

## Button & Accent Colors (4 Tokens)

| Token | WinUI Brush | Intent | Light | Dark |
|---|---|---|---|---|
| `button-accent-default` | `AccentButtonBackground` | Accent button default | Blue (#0078D4) | Light blue (#60CDFF) |
| `button-accent-hover` | `AccentButtonBackgroundPointerOver` | Accent button hover | Darker blue (#106EBE) | Lighter blue (#8DDBF5) |
| `button-accent-pressed` | `AccentButtonBackgroundPressed` | Accent button pressed | Very dark blue (#005A9E) | Dark blue (#5AC4EB) |
| `button-accent-text` | `AccentButtonForeground` | Text on accent button | White | Black |

**Usage:**
```xaml
<!-- Primary action (Start Recording) -->
<Button
    Content="Start Recording"
    Style="{ThemeResource AccentButtonStyle}"
    Command="{x:Bind ViewModel.RecordCommand}"
    AutomationProperties.Name="Start recording" />

<!-- Secondary action -->
<Button
    Content="Cancel"
    Command="{x:Bind ViewModel.CancelCommand}"
    AutomationProperties.Name="Cancel" />
```

**Do NOT:** Hard-code button colors; use `Style="{ThemeResource AccentButtonStyle}"`

---

## Token → Brush Mapping

Quick reference for all 27 tokens:

```
TEXT (5):           TextFillColorPrimaryBrush, TextFillColorSecondaryBrush,
                    TextFillColorTertiaryBrush, TextFillColorDisabledBrush,
                    SystemFillColorCriticalBrush

SURFACE (5):        SolidBackgroundFillColorBaseBrush,
                    SolidBackgroundFillColorSecondaryBrush,
                    SolidBackgroundFillColorTertiaryBrush,
                    ControlFillColorTransparentBrush,
                    ControlStrongFillColorDefaultBrush

CONTROL (5):        ControlFillColorDefaultBrush,
                    ControlFillColorSecondaryBrush,
                    ControlFillColorTertiaryBrush,
                    ControlFillColorDisabledBrush,
                    ControlFillColorInputActiveBrush

STROKE (3):         ControlBorderBrush,
                    ControlStrokeColorSecondary,
                    DividerStrokeColorDefaultBrush

SEMANTIC (4):       SystemFillColorSuccessBrush,
                    SystemFillColorCautionBrush,
                    SystemFillColorCriticalBrush,
                    SystemFillColorAttentionBrush

BUTTON (4):         AccentButtonBackground,
                    AccentButtonBackgroundPointerOver,
                    AccentButtonBackgroundPressed,
                    AccentButtonForeground
```

---

## Light & Dark Theme Examples

| Component | Light Theme | Dark Theme | XAML |
|---|---|---|---|
| Main text | Black | Off-white | `{ThemeResource TextFillColorPrimaryBrush}` |
| Page bg | Off-white | Very dark gray | `{ThemeResource SolidBackgroundFillColorBaseBrush}` |
| Card bg | Off-white | Slightly lighter | `{ThemeResource ControlStrongFillColorDefaultBrush}` |
| Error text | Red | Light red | `{ThemeResource SystemFillColorCriticalBrush}` |
| Success | Green | Light green | `{ThemeResource SystemFillColorSuccessBrush}` |
| Divider | Very light gray | Very dark gray | `{ThemeResource DividerStrokeColorDefaultBrush}` |

---

## Essential Rules

- **Never hard-code colors** — Always use `{ThemeResource ...Brush}`
- **Always map to semantic tokens** — Use token names (e.g., `text-primary`) before referencing WinUI brushes
- **Support all themes** — Light/dark/high-contrast automatically adapts via theme resources
- **Test in dark mode** — Verify readability in Windows Settings > Colors > Dark theme
- **Test in high contrast** — Enable Settings > Ease of Access > High contrast mode

---

## Anti-Patterns

```xaml
<!-- ❌ AVOID: Hard-coded hex colors -->
<TextBlock Foreground="#000000" />
<Button Background="#0078D4" />

<!-- ✓ GOOD: Use theme resources -->
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
<Button Style="{ThemeResource AccentButtonStyle}" />
```

---

## Cross-References

- **Token naming & philosophy:** See `DESIGN.md` Section 2 (Three Core Rules)
- **Component-specific colors:** See `.claude/rules/design-components.rules.md`
- **Typography + colors together:** See `.claude/rules/design-typography.rules.md`
