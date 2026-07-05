# Design Rule: Spacing, Sizing & Layout

IntVue uses an **8px base grid** for consistent spacing. All margins, padding, and sizing are multiples of 8px.

---

## Spacing Scale

| Token | Value | Use Case | Example |
|---|---|---|---|
| `spacing-xs` | 4px | Minimal padding (rare) | Badge padding |
| `spacing-sm` | 8px | Tight spacing, button gaps | StackPanel Spacing="8" |
| `spacing-md` | 12px | Standard padding | Panel Padding="12" |
| `spacing-lg` | 16px | Comfortable spacing | Section breaks |
| `spacing-xl` | 24px | Generous spacing | Major sections |

**Usage:**
```xaml
<!-- Button spacing (tight, 8px) -->
<StackPanel Orientation="Horizontal" Spacing="8">
    <Button Content="Preview" />
    <Button Content="Record" />
</StackPanel>

<!-- Panel padding (standard, 12px) -->
<StackPanel Padding="12" Spacing="8">
    <TextBlock Text="Recording Controls" />
</StackPanel>

<!-- Section breaks (generous, 16px) -->
<StackPanel Spacing="16">
    <TextBlock Text="Section 1" FontSize="20" FontWeight="Bold" />
    <StackPanel Spacing="8">
        <Button Content="Action 1" />
        <Button Content="Action 2" />
    </StackPanel>
</StackPanel>
```

**Do NOT:**
```xaml
<!-- ❌ Avoid hard-coded values -->
<StackPanel Spacing="10" />
<Button Margin="15,0,0,0" />

<!-- ✓ Use spacing tokens -->
<StackPanel Spacing="8" />
<Button Margin="8,0,0,0" />
```

---

## Border Radius Scale

Rounded corners support modern Fluent Design. Use predefined values:

| Token | Value | Use Case | Example |
|---|---|---|---|
| `radius-none` | 0px | Sharp corners | Rare, edge cases |
| `radius-sm` | 2px | Subtle rounding | Badges, small chips |
| `radius-card` | 8px | Standard rounding | Cards, panels |
| `radius-large` | 16px | Generous rounding | Dialogs, modals |

**Usage:**
```xaml
<!-- Card (standard rounding, 8px) -->
<Border CornerRadius="8" Background="{ThemeResource ControlStrongFillColorDefaultBrush}" Padding="12">
    <TextBlock Text="Recording Info" />
</Border>

<!-- Badge (small rounding, 2px) -->
<Grid CornerRadius="2" Background="{ThemeResource SystemFillColorCriticalBrush}" Padding="4,2">
    <TextBlock Text="Live" Foreground="{ThemeResource TextFillColorPrimaryBrush}" FontSize="12" />
</Grid>

<!-- Dialog (generous rounding, 16px) -->
<Border CornerRadius="16" Background="{ThemeResource SolidBackgroundFillColorBaseBrush}" Padding="24">
    <StackPanel>
        <TextBlock Text="Confirm action?" FontSize="20" FontWeight="Bold" />
    </StackPanel>
</Border>
```

**Do NOT:** Use arbitrary `CornerRadius` values; stick to the scale above

---

## Elevation & Depth

Use elevation to create visual hierarchy. WinUI 3 with Mica backdrop already provides elevation.

| Level | Technique | Use Case | Background | Shadow |
|---|---|---|---|---|
| **Flat** | No elevation | Base surface | `surface-primary` | None |
| **Raised** | Subtle layering | Grouped content | `surface-secondary` | Minimal |
| **Elevated** | Visual separation | Cards, popups | `surface-elevated` | Visible |

**Usage:**
```xaml
<!-- Flat (page background, no elevation) -->
<Page Background="{ThemeResource SolidBackgroundFillColorBaseBrush}" />

<!-- Raised (secondary panel, subtle separation) -->
<StackPanel 
    Background="{ThemeResource SolidBackgroundFillColorSecondaryBrush}" 
    Padding="12" 
    Spacing="8">
    <TextBlock Text="Controls" FontWeight="Bold" />
</StackPanel>

<!-- Elevated (card with visual separation) -->
<Border 
    Background="{ThemeResource ControlStrongFillColorDefaultBrush}" 
    CornerRadius="8" 
    Padding="12">
    <ThemeShadow />
    <StackPanel>
        <TextBlock Text="Recording Info" />
    </StackPanel>
</Border>
```

**Do NOT:** Stack multiple shadows; keep elevation simple and consistent

---

## Padding & Margin Rules

### Padding (Inside Containers)
```
Standard inner padding: 12px (spacing-md)
Small containers: 8px (spacing-sm)
Large sections: 16px (spacing-lg)
```

```xaml
<!-- Standard panel padding -->
<StackPanel Padding="12">
    <TextBlock Text="Content" />
</StackPanel>

<!-- Compact padding -->
<StackPanel Padding="8">
    <Button Content="Action" />
</StackPanel>
```

### Margin (Between Elements)
```
Use StackPanel Spacing instead of Margin when possible
Only use Margin for:
  - Spacing between StackPanel groups
  - Asymmetric spacing (e.g., top margin only)
```

```xaml
<!-- GOOD: Use Spacing for consistent gaps -->
<StackPanel Spacing="8">
    <Button Content="Action 1" />
    <Button Content="Action 2" />
</StackPanel>

<!-- GOOD: Use Margin for asymmetric spacing -->
<Button Content="Action" Margin="0,16,0,0" />

<!-- AVOID: Mixing Spacing and explicit Margin -->
<StackPanel Spacing="8">
    <Button Content="Action 1" Margin="10,0,0,0" />  <!-- Don't do this -->
</StackPanel>
```

---

## Grid & Flex Gaps

For Grid layouts (future):

| Gap Size | Use Case | Example |
|---|---|---|
| 8px | Tight column gaps | Related items |
| 12px | Standard gaps | Item grids |
| 16px | Comfortable gaps | Card grids |

```xaml
<!-- Grid with 16px gaps -->
<Grid ColumnSpacing="16" RowSpacing="16" Padding="16">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    
    <Border CornerRadius="8" Background="{ThemeResource ControlStrongFillColorDefaultBrush}" Padding="12" />
    <Border CornerRadius="8" Background="{ThemeResource ControlStrongFillColorDefaultBrush}" Padding="12" Grid.Column="1" />
</Grid>
```

---

## Common MainPage.xaml Patterns

From Phase 1 research, current usage in MainPage.xaml:

```xaml
<!-- Button control spacing (currently: Margin="10") -->
<!-- Update to: Margin="8" to align with 8px grid -->
<StackPanel Orientation="Horizontal" Spacing="8">
    <Button x:Name="BtnPreview" Content="Start Preview" />
    <Button x:Name="BtnRecord" Content="Start Recording" Margin="8,0,0,0" />
</StackPanel>

<!-- Panel padding (currently: Margin="10") -->
<!-- Update to: Padding="12" to align with spacing scale -->
<StackPanel Orientation="Horizontal" Grid.Row="1" Spacing="8" Padding="12">
    <!-- Controls -->
</StackPanel>

<!-- Device initialization section (currently: Spacing="8") ✓ Already correct -->
<StackPanel Orientation="Horizontal" Grid.Row="2" Spacing="8" Padding="12">
    <ComboBox x:Name="CbCameraList" MinWidth="200" />
    <Button x:Name="BtnInitializeDevice" Content="Initialize Device" />
</StackPanel>
```

**Legacy note:** Some existing code uses `Margin="10"`. Follow spacing scale above for new code.

---

## Sizing Conventions

### Min/Max Widths

```
ComboBox min-width: 200px (fixed, based on MainPage.xaml)
Dialog max-width: 400px
Panel min-height: Avoid (let content size naturally)
```

```xaml
<!-- ComboBox sizing -->
<ComboBox MinWidth="200" />

<!-- Dialog sizing -->
<Border MaxWidth="400" HorizontalAlignment="Center">
    <!-- Content -->
</Border>
```

### Panel Heights

```
Let content size naturally; avoid fixed Heights
Use Row definitions for structured layouts
Use Auto height for content-driven sizing
```

```xaml
<!-- Good: Content-driven sizing -->
<StackPanel Spacing="8">
    <Button Content="Action" />
    <TextBlock Text="Status" />
</StackPanel>

<!-- Good: Structured grid -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />         <!-- Flexible -->
        <RowDefinition Height="Auto" />      <!-- Content-sized -->
    </Grid.RowDefinitions>
</Grid>
```

---

## Responsive Layout Principles

For future multi-screen support:

1. **Base spacing:** Always 8px or multiples
2. **Content-first:** Let content determine size, then wrap
3. **Virtualization:** Use ListView/ItemsRepeater for long lists (not StackPanel)
4. **Adaptive padding:** Same scale (8/12/16px) on all screen sizes

```xaml
<!-- Content-first panel (wraps naturally) -->
<StackPanel Orientation="Horizontal" Spacing="8" Padding="12">
    <Button Content="Action 1" />
    <Button Content="Action 2" />
    <!-- Wraps if window too narrow -->
</StackPanel>
```

---

## Essential Rules

- **8px base grid** — All spacing is multiples of 8 (4, 8, 12, 16, 24)
- **Use StackPanel Spacing** — Prefer `Spacing="8"` over `Margin` for consistency
- **Consistent padding** — Standard `Padding="12"` for containers
- **Predefined radius** — Use `CornerRadius="8"` (cards) or `CornerRadius="16"` (dialogs)
- **Never hard-code spacing** — Always reference spacing scale or token names

---

## Anti-Patterns

```xaml
<!-- ❌ Avoid: Arbitrary margins & padding -->
<Button Margin="15,5,20,0" />
<StackPanel Padding="11" />

<!-- ✓ Good: Use spacing scale -->
<Button Margin="16,0,0,0" />
<StackPanel Padding="12" />
```

```xaml
<!-- ❌ Avoid: Non-standard border radius -->
<Border CornerRadius="5" />
<Border CornerRadius="12" />

<!-- ✓ Good: Use radius scale -->
<Border CornerRadius="8" />
<Border CornerRadius="16" />
```

---

## Cross-References

- **Color backgrounds:** See `.claude/rules/design-colors.rules.md` for surface tokens
- **Component padding:** See `.claude/rules/design-components.rules.md` for component-specific spacing
- **Typography sizing:** See `.claude/rules/design-typography.rules.md` for font sizes
- **WinUI best practices:** See `winui-best-practices.instructions.md` for layout patterns
