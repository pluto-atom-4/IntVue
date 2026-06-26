---
name: accessibility-review
description: Audits XAML UI for accessibility compliance (keyboard navigation, automation properties, color contrast, screen reader support)
trigger: "audit accessibility|check a11y|review keyboard nav|verify contrast|accessibility review"
---

# Accessibility Review Playbook

This skill audits XAML UI for accessibility compliance. Use this when implementing new pages, dialogs, or controls to ensure they meet WCAG AA standards and work with assistive technologies like screen readers.

---

## When to Use This Skill

- Adding new XAML pages or dialogs
- Creating custom UI controls
- Modifying existing UI (especially interactive controls)
- Preparing for accessibility audit or compliance review
- Testing with screen readers (Narrator, JAWS)
- Verifying keyboard navigation end-to-end
- Checking color contrast in light, dark, and high-contrast themes

---

## Playbook

### 1. Automation Properties (Screen Reader Support)

**Critical:** All interactive controls must have `AutomationProperties.Name`.

**Checklist:**

- [ ] **Every button has a name:**

```xml
<!-- GOOD: Clear automation name -->
<Button
    x:Name="StartRecordingButton"
    AutomationProperties.Name="Start recording"
    Content="Start" />

<!-- AVOID: No automation name -->
<Button Content="Start" />
```

- [ ] **Text inputs have labels:**

```xml
<!-- GOOD: Label + TextBox with automation -->
<TextBlock Text="Recording name:" />
<TextBox
    AutomationProperties.Name="Recording name"
    PlaceholderText="Enter name" />

<!-- AVOID: No label or automation name -->
<TextBox PlaceholderText="Enter name" />
```

- [ ] **Checkboxes and toggles have clear names:**

```xml
<!-- GOOD: Clear toggle name -->
<ToggleSwitch
    AutomationProperties.Name="Enable notifications"
    Header="Notifications" />

<!-- AVOID: Vague name -->
<ToggleSwitch />
```

- [ ] **Icons/symbols have text descriptions:**

```xml
<!-- GOOD: Text describes the icon -->
<Grid>
    <TextBlock Text="● " Foreground="Red" />
    <TextBlock Text="Recording" />
</Grid>

<!-- AVOID: Icon alone, no text -->
<FontIcon Glyph="●" Foreground="Red" />
```

- [ ] **List/collection items are accessible:**

```xml
<!-- GOOD: Each item is a keyboard-accessible control -->
<ListView ItemsSource="{x:Bind Items}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <Button
                AutomationProperties.Name="{x:Bind ItemName}"
                Command="{x:Bind SelectCommand}"
                Content="{x:Bind ItemName}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

<!-- AVOID: Items are just text; not keyboard-accessible -->
<ItemsControl ItemsSource="{x:Bind Items}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{x:Bind ItemName}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

**Verify with Narrator:**
1. Open Narrator (Windows + Alt + N)
2. Tab through the UI
3. Each control should announce its name and type (e.g., "Start Recording, button")
4. No orphaned text or unlabeled controls should exist

### 2. Keyboard Navigation

**Critical:** UI must be fully operable via keyboard alone (no mouse required).

**Checklist:**

- [ ] **Tab order is logical:**

```xml
<!-- GOOD: Tab order follows left-to-right, top-to-bottom -->
<StackPanel Spacing="12">
    <TextBox AutomationProperties.Name="Recording name" />
    <Button Content="Start" />
    <Button Content="Cancel" />
</StackPanel>

<!-- AVOID: Tab order jumps around illogically -->
<!-- (use TabIndex only if natural order is broken) -->
<StackPanel>
    <Button TabIndex="3" Content="Cancel" />
    <Button TabIndex="1" Content="Start" />
    <TextBox TabIndex="2" />
</StackPanel>
```

- [ ] **Enter/Space activates buttons:**

```csharp
// GOOD: Button responds to Enter/Space (automatic in WinUI 3)
<Button Content="Start Recording" Command="{x:Bind ViewModel.StartCommand}" />

// AVOID: Custom key handling instead of using Button
<TextBlock KeyDown="OnKeyDown" Text="Click to start" />
```

- [ ] **Escape closes dialogs/cancels:**

```csharp
// GOOD: ContentDialog Escape handling (automatic)
<ContentDialog>
    <Button Content="Cancel" IsCancel="True" />
</ContentDialog>

// AVOID: Not responding to Escape
<ContentDialog>
    <Button Content="Cancel" Click="OnCancelClick" />
    <!-- Escape doesn't close dialog -->
</ContentDialog>
```

- [ ] **Focus indicator is visible:**

```xml
<!-- GOOD: Focus rect visible (WinUI 3 default) -->
<!-- No action needed; WinUI 3 shows focus by default -->

<!-- AVOID: Hiding focus indicator -->
<!-- <Control UseSystemFocusVisuals="False" /> -->
```

- [ ] **No keyboard traps** (focus stuck in a region):

```csharp
// TEST: Tab through the entire UI
// Verify you can Tab out of every region
// No focus should be "trapped" in a dropdown, dialog, or section

// Example trap (AVOID):
// Modal dialog opens
// Tab loops inside dialog forever
// Cannot return to main window without closing dialog
// (This is actually correct behavior; ignore this if intended)
```

**Test Keyboard Navigation:**
1. Use only keyboard to navigate and interact:
   - Tab: Move focus forward
   - Shift+Tab: Move focus backward
   - Enter: Activate button or default action
   - Space: Activate button or toggle checkbox
   - Arrow keys: Navigate within lists, menus
   - Escape: Close dialog or cancel action
2. Verify every control is reachable via Tab
3. Verify every action is accessible via keyboard
4. Verify no focus traps (unless intended, e.g., modal dialogs)

### 3. Color Contrast

**Critical:** Text must meet WCAG AA color contrast ratio (4.5:1 for normal text, 3:1 for large text).

**Checklist:**

- [ ] **Normal text (≤18pt) has 4.5:1 contrast:**

```xml
<!-- GOOD: High contrast -->
<TextBlock
    Foreground="{ThemeResource TextFillColorPrimaryBrush}"
    Text="Important information" />

<!-- AVOID: Low contrast -->
<TextBlock
    Foreground="#999999"
    Background="White"
    Text="Important information" />
```

- [ ] **Large text (≥18pt or ≥14pt bold) has 3:1 contrast:**

```xml
<!-- GOOD: Title with high contrast -->
<TextBlock
    Style="{StaticResource TitleTextBlockStyle}"
    Foreground="{ThemeResource TextFillColorPrimaryBrush}"
    Text="Page Title" />

<!-- AVOID: Low contrast title -->
<TextBlock
    FontSize="20"
    Foreground="#CCCCCC"
    Text="Page Title" />
```

- [ ] **Always use theme resources:**

```xml
<!-- GOOD: Theme resources auto-adjust for light/dark/high-contrast -->
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" Text="Text" />

<!-- AVOID: Hard-coded colors (may fail in some themes) -->
<TextBlock Foreground="#333333" Text="Text" />
```

- [ ] **Error messages have high contrast and icon:**

```xml
<!-- GOOD: Red text + error icon -->
<StackPanel Orientation="Horizontal">
    <FontIcon Glyph="⚠" Foreground="{ThemeResource SystemFillColorCriticalBrush}" />
    <TextBlock
        Foreground="{ThemeResource SystemFillColorCriticalBrush}"
        Text="Recording failed. Please try again." />
</StackPanel>

<!-- AVOID: Red text only (color-blind users can't see it) -->
<TextBlock Foreground="Red" Text="Recording failed" />
```

- [ ] **Don't rely on color alone:**

```xml
<!-- GOOD: Color + text + icon -->
<Grid>
    <TextBlock Text="● " Foreground="Red" />
    <TextBlock Text="Recording (Red)" />
</Grid>

<!-- AVOID: Color only -->
<TextBlock Foreground="Red" Text="●" />
```

**Test Contrast:**
1. Use a contrast checker tool: [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
2. Test in light, dark, and high-contrast Windows themes
3. Verify all text meets 4.5:1 (normal) or 3:1 (large)
4. Verify color-blind users can distinguish UI elements (use text + icons, not color alone)

### 4. Theme Support

**Checklist:**

- [ ] **Test in light theme:**

```powershell
# Windows Settings → Colors → Light
# Launch app
# Verify all text is readable
# Verify buttons are clearly clickable
```

- [ ] **Test in dark theme:**

```powershell
# Windows Settings → Colors → Dark
# Launch app
# Verify all text is readable (high contrast)
# Verify theme colors adapt (not hard-coded)
```

- [ ] **Test in high-contrast theme:**

```powershell
# Windows Settings → Ease of Access → Display → High Contrast
# Select "High Contrast White" or similar
# Launch app
# Verify all UI is visible and high-contrast
# Verify theme resources adapt (not hard-coded colors)
```

**Verify:**
1. All colors use theme resources, not hard-coded values
2. UI adapts correctly in all three themes
3. No colors become unreadable in any theme

### 5. Screen Reader Testing

**Checklist:**

- [ ] **Install and enable Narrator:**

```powershell
# Windows + Alt + N  (or Settings → Ease of Access → Narrator)
```

- [ ] **Tab through entire UI:**
- Each control announces clearly (name + type)
- No hidden or unlabeled controls
- List items are clearly identified

**Example narration (GOOD):**
```
"Recording name, edit text"
"Start recording, button"
"Cancel, button"
```

**Example narration (AVOID):**
```
"Edit text"  (no name)
"Button"  (no name or context)
"Loading..."  (generic, confusing)
```

- [ ] **Test status messages:**

```csharp
// GOOD: StatusMessage property updated; screen reader announces it
[ObservableProperty]
private string statusMessage = string.Empty;

// User performs action
StatusMessage = "Recording started"; // Narrator announces this

// AVOID: Showing message only visually
// Narrator doesn't know about it
```

- [ ] **Test dialogs:**

```xml
<!-- GOOD: Dialog focuses first interactive element -->
<ContentDialog
    Title="Confirm"
    PrimaryButtonText="Yes"
    SecondaryButtonText="No" />

<!-- AVOID: Dialog with no clear focus or buttons -->
<ContentDialog>
    <TextBlock Text="Confirm action?" />
</ContentDialog>
```

**Test with Narrator:**
1. Open Narrator (Windows + Alt + N)
2. Turn on "Listening mode" (Caps Lock + D)
3. Navigate UI:
   - Tab to focus controls
   - Listen to announcements
   - Verify each control is identified
   - Verify actions are accessible
4. Close Narrator (Windows + Alt + N)

### 6. Images & Icons

**Checklist:**

- [ ] **Decorative images have empty alt text:**

```xml
<!-- GOOD: Decorative image (Alt text empty) -->
<Image
    Source="separator-line.png"
    AutomationProperties.AccessibilityView="Raw" />

<!-- AVOID: Redundant alt text for decoration -->
<!-- <Image Source="separator-line.png" AutomationProperties.Name="Separator" /> -->
```

- [ ] **Meaningful images have descriptive alt text:**

```xml
<!-- GOOD: Descriptive alt text for icon -->
<Image
    Source="warning-icon.png"
    AutomationProperties.Name="Warning: Camera unavailable" />

<!-- AVOID: Generic alt text -->
<!-- <Image Source="warning-icon.png" AutomationProperties.Name="Icon" /> -->
```

- [ ] **Icons + text together (not icon alone):**

```xml
<!-- GOOD: Icon + text together -->
<StackPanel Orientation="Horizontal">
    <FontIcon Glyph="⚠" />
    <TextBlock Text="Camera unavailable" />
</StackPanel>

<!-- AVOID: Icon only (no text description) -->
<FontIcon Glyph="⚠" />
```

### 7. Code Review Checklist

Before submitting UI changes, search for:

```xaml
<!-- 1. AutomationProperties.Name -->
<!-- Ensure all interactive controls (Button, TextBox, ToggleSwitch) have it -->

<!-- 2. Hard-coded colors -->
<!-- Replace #RRGGBB with {ThemeResource ...} -->

<!-- 3. {Binding} -->
<!-- Replace with x:Bind for compile-time checking -->

<!-- 4. FontIcon without text -->
<!-- Add text description next to it -->

<!-- 5. Deep nesting -->
<!-- Simplify XAML hierarchy if nesting >5 levels deep -->

<!-- 6. Visibility="Visible/Collapsed" without keyboard alternative -->
<!-- Ensure keyboard-only users can access all content -->
```

---

## Testing Checklist

| Test | Tool/Method | Expected Result |
|---|---|---|
| Automation Properties | Narrator (Win+Alt+N) | Each control announces name + type |
| Keyboard Navigation | Tab, Shift+Tab, Enter, Space, Escape | All controls reachable; no traps |
| Color Contrast | WebAIM Contrast Checker | ≥4.5:1 normal text, ≥3:1 large text |
| Light Theme | Windows Settings → Colors | Text readable; colors correct |
| Dark Theme | Windows Settings → Colors | Text readable; colors adapted |
| High Contrast | Windows Settings → Ease of Access | Maximized contrast; no low-contrast areas |
| Screen Reader | Narrator | Clear announcements; all UI operable |

---

## References

- **Accessibility Guidelines:** `.github/instructions/accessibility.instructions.md`
- **XAML Best Practices:** `.github/instructions/winui-best-practices.instructions.md`
- **Views/CLAUDE.md:** `Views/CLAUDE.md`
- **Color Contrast Checker:** [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- **WCAG 2.1 AA Standards:** [WCAG Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

## Checklist for Submission

**Automation Properties:**
- [ ] All buttons have `AutomationProperties.Name`
- [ ] All text inputs have `AutomationProperties.Name`
- [ ] All interactive controls have clear names
- [ ] List items are keyboard-accessible
- [ ] No hidden or orphaned text

**Keyboard Navigation:**
- [ ] Tab order is logical
- [ ] All controls reachable via Tab
- [ ] Buttons respond to Enter/Space
- [ ] Dialogs respond to Escape
- [ ] Focus indicator is visible
- [ ] No keyboard traps

**Color Contrast:**
- [ ] Normal text has ≥4.5:1 contrast
- [ ] Large text has ≥3:1 contrast
- [ ] All colors use theme resources (no hard-coded colors)
- [ ] Error messages clearly visible
- [ ] Not relying on color alone

**Theme Support:**
- [ ] Tested in light theme
- [ ] Tested in dark theme
- [ ] Tested in high-contrast theme
- [ ] All themes render correctly

**Screen Reader:**
- [ ] Narrator announces each control
- [ ] Status messages announced
- [ ] Dialogs work with screen readers
- [ ] Images have appropriate alt text

**Final Check:**
- [ ] Keyboard navigation tested end-to-end
- [ ] Narrator testing completed
- [ ] All three themes verified
- [ ] Contrast verified (WebAIM Contrast Checker)
- [ ] No hard-coded colors
- [ ] No {Binding} (only x:Bind)
