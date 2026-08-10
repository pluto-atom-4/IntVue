# Design Rule: Components & Patterns

IntVue uses WinUI 3 components with consistent styling and behavior. This file documents component patterns and accessibility requirements.

---

## Button States

Buttons have five primary states. WinUI styles handle hover/pressed automatically:

| State | Appearance | Trigger | Accessibility |
|---|---|---|---|
| **Default** | Normal appearance | Page load | Focusable (Tab) |
| **Hover** | Slightly darker (theme-aware) | Mouse over | Same |
| **Pressed** | Darker (theme-aware) | Click / Enter key | Pressed announced |
| **Disabled** | Grayed out | `IsEnabled="False"` | Disabled announced |
| **Focus** | Focus outline visible | Tab navigation | Outline visible |

**Button Patterns:**
```xaml
<!-- Secondary (default) -->
<Button Content="Preview" AutomationProperties.Name="Start preview" />

<!-- Primary/Accent -->
<Button Content="Record" Style="{ThemeResource AccentButtonStyle}" AutomationProperties.Name="Start recording" />

<!-- Disabled -->
<Button Content="Play" IsEnabled="{x:Bind ViewModel.HasRecording}" />

<!-- Group -->
<StackPanel Orientation="Horizontal" Spacing="8">
    <Button Content="Action 1" />
    <Button Content="Action 2" />
</StackPanel>
```

**Do NOT:**
- Hard-code button colors; trust WinUI `AccentButtonStyle`
- Use `Background` on buttons; use `Style` instead
- Disable focus indicators (WinUI provides them)

---

## Form Controls

Form controls inherit WinUI 3 styling automatically:

```xaml
<TextBox PlaceholderText="Search..." AutomationProperties.Name="Search" />
<ComboBox ItemsSource="{x:Bind ViewModel.Items}" SelectedItem="{x:Bind ViewModel.Selected, Mode=TwoWay}" />
<CheckBox Content="Auto-save" IsChecked="{x:Bind ViewModel.AutoSave, Mode=TwoWay}" />
<RadioButton Content="Option A" GroupName="Group1" />
```

**Do NOT:**
- Custom styling (use default WinUI styles)
- Hard-code form control colors
- Omit `AutomationProperties.Name`

---

## Keyboard Navigation & Focus Indicators

WinUI 3 automatically provides focus outlines when tabbing between controls. They are **critical for accessibility**.

**Tab Order (Optional, for Complex Layouts):**
```xaml
<Grid>
    <Button x:Name="Btn1" TabIndex="0" Content="First" />
    <Button x:Name="Btn2" TabIndex="1" Content="Second" />
    <Button x:Name="Btn3" TabIndex="2" Content="Third" />
</Grid>
```

**Access Keys (Alt+Letter Shortcuts):**
```xaml
<Button
    Content="Start Recording"
    AccessKey="R"
    Click="BtnRecord_Click"
    AutomationProperties.Name="Start recording" />

<!-- User presses: Alt+R to activate -->
```

**Keyboard Navigation Rules:**
- **Tab:** Navigate to next control
- **Shift+Tab:** Navigate to previous control
- **Enter / Space:** Activate button
- **Escape:** Cancel/close dialog
- **Alt+{AccessKey}:** Quick access

**Do NOT:**
- Custom focus outlines (trust WinUI)
- Disable focus (users need visible focus)
- Omit keyboard support on interactive elements

---

## Recording Indicator

The recording indicator must be **always visible, high-contrast, and accessible** when recording:

```xaml
<!-- Recording indicator (when ViewModel.IsRecording == true) -->
<Grid
    Background="{ThemeResource SystemFillColorCriticalBrush}"
    CornerRadius="4"
    Padding="8,4"
    Visibility="{x:Bind ViewModel.IsRecording, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
    
    <!-- Accessibility label -->
    <AutomationProperties.Name>Recording in progress</AutomationProperties.Name>
    
    <StackPanel Orientation="Horizontal" Spacing="4">
        <!-- Red dot indicator -->
        <Ellipse 
            Width="8" 
            Height="8" 
            Fill="{ThemeResource TextFillColorPrimaryBrush}" />
        
        <!-- "Recording" text -->
        <TextBlock
            Text="Recording"
            Foreground="{ThemeResource TextFillColorPrimaryBrush}"
            FontWeight="Bold"
            VerticalAlignment="Center"
            FontSize="14" />
    </StackPanel>
</Grid>

<!-- Recording elapsed time -->
<TextBlock
    Text="{x:Bind ViewModel.RecordingTime, Mode=OneWay}"
    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
    FontSize="12" />
```

**Styling Rules:**
- Background: `SystemFillColorCriticalBrush` (red, error color)
- Text: `TextFillColorPrimaryBrush` (high contrast)
- Font weight: Bold for emphasis
- Always visible when recording (no conditional hiding based on hover)
- Accessibility label: "Recording in progress"

**Do NOT:**
- Subtle colors (red is intentional, grabs attention)
- Blinking/pulsing animations
- Hide until hovering (must be always visible)

---

## Countdown Display

The countdown timer is a **feature-specific display** with large, dynamic typography:

```xaml
<!-- Countdown overlay (centered on screen) -->
<Grid
    HorizontalAlignment="Center"
    VerticalAlignment="Center"
    Visibility="{x:Bind ViewModel.IsCountingDown, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
    
    <!-- Large countdown number -->
    <TextBlock
        x:Name="TxtCountdown"
        Text="{x:Bind ViewModel.CountdownSeconds, Mode=OneWay}"
        FontSize="72"
        FontWeight="Bold"
        Foreground="{x:Bind ViewModel.CountdownColor, Mode=OneWay}"
        HorizontalAlignment="Center"
        VerticalAlignment="Center"
        AutomationProperties.Name="Countdown timer"
        LineHeight="108" />
</Grid>

<!-- Cancel countdown button -->
<Button
    x:Name="BtnCancelCountdown"
    Content="Cancel Countdown"
    Command="{x:Bind ViewModel.CancelCountdownCommand}"
    Visibility="{x:Bind ViewModel.IsCountingDown, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}"
    AutomationProperties.Name="Cancel countdown timer" />
```

**Color Progression (Converter Logic):**

```
Seconds Remaining  →  Color                           →  Meaning
4–10               →  SystemFillColorSuccessBrush     →  "Prepare, plenty of time"
2–3                →  SystemFillColorCautionBrush     →  "Get ready, count down"
0–1                →  SystemFillColorCriticalBrush    →  "Recording starting NOW"
```

**Styling Rules:**
- Font size: 72px (large, commanding presence)
- Font weight: Bold for emphasis
- Color: Dynamic via `CountdownColorConverter`
- Centered on screen (HorizontalAlignment, VerticalAlignment)
- Always visible during countdown (no disappearing)
- Accessibility label: "Countdown timer"

**Do NOT:**
- Small font sizes (must be visible across screen)
- Animated pulsing (may distract from focus)
- Hard-coded colors (use converter)
- Omit accessibility label

---

## Form Control Layout

For forms with multiple fields:

```xaml
<StackPanel Spacing="16" Padding="12">
    <!-- Field (repeat this label+control block per field) -->
    <StackPanel Spacing="4">
        <TextBlock Text="Camera" FontSize="14" FontWeight="SemiBold" />
        <ComboBox
            PlaceholderText="Select camera"
            ItemsSource="{x:Bind ViewModel.Cameras, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedCamera, Mode=TwoWay}"
            MinWidth="200"
            AutomationProperties.Name="Camera selection" />
    </StackPanel>

    <!-- Actions -->
    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Right">
        <Button Content="Cancel" />
        <Button Content="Apply" Style="{ThemeResource AccentButtonStyle}" />
    </StackPanel>
</StackPanel>
```

**Spacing Rules:** See `.claude/rules/design-spacing.rules.md` for the canonical spacing scale (label + control spacing, field spacing, button group spacing) applied above.

---

## Dialog/Modal Patterns

For confirm dialogs and modal overlays:

```xaml
<!-- Modal dialog (full-screen overlay) -->
<Grid
    Background="{ThemeResource ControlFillColorTransparentBrush}"
    Visibility="{x:Bind ViewModel.ShowDialog, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
    
    <!-- Centered dialog -->
    <Border
        Background="{ThemeResource SolidBackgroundFillColorBaseBrush}"
        CornerRadius="16"
        Padding="24"
        MaxWidth="400"
        HorizontalAlignment="Center"
        VerticalAlignment="Center">
        
        <StackPanel Spacing="16">
            <!-- Title -->
            <TextBlock 
                Text="Confirm Action" 
                FontSize="20" 
                FontWeight="Bold"
                Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
            
            <!-- Content -->
            <TextBlock 
                Text="Are you sure you want to proceed?" 
                FontSize="14"
                Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
            
            <!-- Buttons -->
            <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Right">
                <Button 
                    Content="Cancel" 
                    Click="CancelDialog_Click"
                    AutomationProperties.Name="Cancel" />
                <Button 
                    Content="Confirm" 
                    Style="{ThemeResource AccentButtonStyle}"
                    Click="ConfirmDialog_Click"
                    AutomationProperties.Name="Confirm action" />
            </StackPanel>
        </StackPanel>
    </Border>
</Grid>
```

**Dialog Styling Rules:**
- Overlay background: `ControlFillColorTransparentBrush` (semi-transparent)
- Dialog background: `SolidBackgroundFillColorBaseBrush` (matches page)
- Max width: 400px (prevents too-wide dialogs)
- Border radius and padding: see `.claude/rules/design-spacing.rules.md` (`radius-large`, `spacing-xl`) for the canonical values used above

---

## List/Grid Patterns (Future)

Not yet built. When implemented, virtualize long lists with `ListView` or `ItemsRepeater` (never `StackPanel`), and use `Grid` with `ColumnSpacing`/`RowSpacing` per the 8px scale in `design-spacing.rules.md` for grid layouts.

---

## Accessibility Requirements

**All Interactive Controls:** Every button, link, and input must have `AutomationProperties.Name` (see the Button Patterns example under "Button States" above).

**Keyboard Navigation:** See "Keyboard Navigation & Focus Indicators" above for the full rules and examples.

**Color Contrast:**
- Text must meet WCAG AA: 4.5:1 ratio for normal text, 3:1 for large text
- Always test in light, dark, and high-contrast themes

**Screen Reader & Focus Testing:**
- Windows+Enter: Activate Narrator (built-in screen reader) and verify all buttons/inputs have labels
- Tab outlines must be visible on all backgrounds (WinUI provides); never hide focus

---

## Essential Rules

These rules are already covered in detail above (Button States, Form Controls, Keyboard Navigation & Focus Indicators, Recording Indicator, Countdown Display, Accessibility Requirements). See `DESIGN.md`'s "Essential Rules Summary" for the project-wide version of this list.

---

## Anti-Patterns

```xaml
<!-- ❌ Avoid: Button without automation property -->
<Button Content="Action" />

<!-- ✓ Good: Button with accessibility label -->
<Button Content="Action" AutomationProperties.Name="Perform action" />
```

```xaml
<!-- ❌ Avoid: Hard-coded button colors -->
<Button Background="{ThemeResource AccentButtonBackground}" />

<!-- ✓ Good: Use style -->
<Button Style="{ThemeResource AccentButtonStyle}" />
```

---

## Cross-References

- **Button colors:** See `.claude/rules/design-colors.rules.md` (button-accent-* tokens)
- **Spacing/padding:** See `.claude/rules/design-spacing.rules.md` (spacing-sm through spacing-xl)
- **Text sizing:** See `.claude/rules/design-typography.rules.md` (type-body, type-title, etc.)
- **Form field colors:** See `.claude/rules/design-colors.rules.md` (control-*, text-* tokens)
- **Accessibility details:** See `Views/CLAUDE.md` and `accessibility.instructions.md`
