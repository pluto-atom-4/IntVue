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

WinUI 3 automatically provides focus outlines. They are **critical for accessibility**.

**Default Focus Behavior (Automatic):**
```xaml
<!-- Focus automatically visible when using Tab -->
<Button Content="Action" AutomationProperties.Name="Do action" />
```

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
    <!-- Field 1 -->
    <StackPanel Spacing="4">
        <TextBlock Text="Camera" FontSize="14" FontWeight="SemiBold" />
        <ComboBox
            PlaceholderText="Select camera"
            ItemsSource="{x:Bind ViewModel.Cameras, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedCamera, Mode=TwoWay}"
            MinWidth="200"
            AutomationProperties.Name="Camera selection" />
    </StackPanel>
    
    <!-- Field 2 -->
    <StackPanel Spacing="4">
        <TextBlock Text="Resolution" FontSize="14" FontWeight="SemiBold" />
        <StackPanel Spacing="8">
            <RadioButton Content="1080p" GroupName="Resolution" />
            <RadioButton Content="720p" GroupName="Resolution" />
        </StackPanel>
    </StackPanel>
    
    <!-- Actions -->
    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Right">
        <Button Content="Cancel" />
        <Button Content="Apply" Style="{ThemeResource AccentButtonStyle}" />
    </StackPanel>
</StackPanel>
```

**Spacing Rules:**
- Label + control spacing: 4px (`spacing-xs`)
- Between fields: 16px (`spacing-lg`)
- Button group: 8px (`spacing-sm`)

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
- Border radius: 16px (`radius-large`)
- Padding: 24px (`spacing-xl`)
- Max width: 400px (prevents too-wide dialogs)

---

## List/Grid Patterns (Future)

For virtualized lists and grids:

**Virtualized List:**
```xaml
<ListView ItemsSource="{x:Bind ViewModel.Recordings, Mode=OneWay}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:Recording">
            <StackPanel Padding="12" Spacing="4">
                <TextBlock 
                    Text="{x:Bind Name, Mode=OneWay}" 
                    FontSize="14"
                    FontWeight="SemiBold" />
                <TextBlock 
                    Text="{x:Bind Date, Mode=OneWay}" 
                    FontSize="12"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

**Grid Layout:**
```xaml
<Grid ColumnSpacing="16" RowSpacing="16" Padding="16">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    
    <Border CornerRadius="8" Background="{ThemeResource ControlStrongFillColorDefaultBrush}" Padding="12" />
    <Border CornerRadius="8" Background="{ThemeResource ControlStrongFillColorDefaultBrush}" Padding="12" Grid.Column="1" />
</Grid>
```

**Do NOT:** Use StackPanel for long lists; always virtualize with `ListView` or `ItemsRepeater`

---

## Accessibility Requirements

**All Interactive Controls:**
```xaml
<!-- Every button, link, input must have automation property -->
<Button
    Content="Action"
    AutomationProperties.Name="Clear description of action"
    AutomationProperties.AutomationId="ButtonAutomationId" />
```

**Keyboard Navigation:**
- Tab: Navigate forward
- Shift+Tab: Navigate backward
- Enter/Space: Activate button
- Escape: Cancel dialog

**Color Contrast:**
- Text must meet WCAG AA: 4.5:1 ratio for normal text, 3:1 for large text
- Always test in light, dark, and high-contrast themes

**Screen Reader Testing:**
- Windows+Enter: Activate Narrator (built-in screen reader)
- Verify all buttons have `AutomationProperties.Name`
- Verify all inputs have labels (via `AutomationProperties.Name` or associated TextBlock)

**Focus Indicators:**
- Tab outlines must be visible on all backgrounds (WinUI provides)
- Never hide focus (critical for keyboard users)

---

## Essential Rules

1. **Button accessibility** — All buttons have `AutomationProperties.Name`
2. **Keyboard support** — Tab/Enter/Escape work everywhere
3. **Color contrast** — Sufficient contrast in all themes
4. **Focus indicators** — Always visible (trust WinUI)
5. **No hard-coded styles** — Use WinUI theme resources
6. **Form labels** — All inputs have clear labels
7. **Recording indicator** — Always visible when active
8. **Countdown visible** — Never hidden or off-screen

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

```xaml
<!-- ❌ Avoid: Conditional recording indicator visibility -->
<Grid Visibility="{x:Bind ShowOnHover}" />

<!-- ✓ Good: Always visible when recording -->
<Grid Visibility="{x:Bind ViewModel.IsRecording, Converter=...}" />
```

---

## Cross-References

- **Button colors:** See `.claude/rules/design-colors.rules.md` (button-accent-* tokens)
- **Spacing/padding:** See `.claude/rules/design-spacing.rules.md` (spacing-sm through spacing-xl)
- **Text sizing:** See `.claude/rules/design-typography.rules.md` (type-body, type-title, etc.)
- **Form field colors:** See `.claude/rules/design-colors.rules.md` (control-*, text-* tokens)
- **Accessibility details:** See `Views/CLAUDE.md` and `accessibility.instructions.md`
