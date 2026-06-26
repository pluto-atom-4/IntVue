---
description: XAML, WinUI 3 controls, data binding, accessibility, theming, and localization for Views and Controls
applyTo: Views/**/*.xaml, Views/**/*.cs, Controls/**/*.xaml, Controls/**/*.cs
---

# Views & Controls — Scoped Guidance

Views (XAML pages and windows) and Controls (custom or reusable UI components) define the visual structure and interaction of the app. All Views use `x:Bind` data binding and are styled with WinUI 3 theme resources to support light/dark/high-contrast themes.

---

## XAML Data Binding

- **Always use `x:Bind`**, never `{Binding}`. Benefits: compile-time checking, better performance, IntelliSense support.
- **Choose the right mode:** Use `Mode=OneWay` or `Mode=OneTime` for read-only bindings (faster than `TwoWay`). Use `Mode=TwoWay` only when the UI updates the ViewModel property.
- **Format:** One attribute per line for controls with 3+ attributes. Order: `x:Name` → `x:Uid` → `AutomationProperties` → layout → data → style.

```xml
<!-- GOOD: x:Bind with appropriate mode -->
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}" />
<Button Command="{x:Bind ViewModel.SaveCommand}" Content="Save" />

<!-- AVOID: {Binding}, which is slower and lacks compile-time checks -->
<TextBlock Text="{Binding Title}" />
```

See `.github/instructions/performance.instructions.md` for details on binding performance.

---

## Theming & Colors

- **Never hard-code colors** in XAML or C#. Always use WinUI 3 theme resources.
- **Use theme resources** to support light, dark, and high-contrast themes automatically.

```xml
<!-- GOOD: Theme resource -->
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
<Button Background="{ThemeResource AccentButtonBackground}" />

<!-- AVOID: Hard-coded colors -->
<TextBlock Foreground="#000000" />
<Button Background="#0078D4" />
```

**Common theme resources:**
- Text: `TextFillColorPrimaryBrush`, `TextFillColorSecondaryBrush`
- Backgrounds: `SolidBackgroundFillColorBaseBrush`, `ControlFillColorDefaultBrush`
- Buttons: `AccentButtonBackground`, `AccentButtonBackgroundPointerOver`

Test your UI in **light, dark, and high-contrast themes** to ensure readability and visual consistency.

---

## WinUI 3 Controls

- **Use `Microsoft.UI.Xaml.Controls`**, never `Windows.UI.Xaml.Controls` (WinUI 3 desktop app).
- **Never call `Window.Current`**; it doesn't exist in WinUI 3. Pass window reference explicitly.
- **Use `DispatcherQueue`**, not `CoreDispatcher`.

```csharp
// GOOD: Pass window reference explicitly
public void DoSomething(Window window)
{
    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
    // ...
}

// AVOID: Window.Current (not available in WinUI 3)
// var window = Window.Current;
```

See `.github/instructions/winui-best-practices.instructions.md` for architecture and navigation patterns.

---

## Deferred Loading & Performance

- **Use `x:Load` for heavy content** (advanced options, secondary panels) to improve startup time. Load them on-demand.
- **Use virtualization** for long lists: `ListView`, `ItemsRepeater` with `StackLayout`, or `DataGrid`.
- **Avoid deep visual tree nesting** — deep XAML hierarchies hurt layout performance.

```xml
<!-- GOOD: Defer heavy content loading -->
<StackPanel x:Load="{x:Bind ViewModel.ShowAdvancedOptions, Mode=OneWay}">
    <!-- Heavy content loaded only when needed -->
</StackPanel>

<!-- GOOD: Virtualize long lists -->
<ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}" />

<!-- AVOID: Loading all content upfront -->
<StackPanel Visibility="{x:Bind ViewModel.ShowAdvancedOptions, Mode=OneWay, Converter=...}">
    <!-- Heavy content always in memory -->
</StackPanel>
```

See `.github/instructions/performance.instructions.md` for virtualization and layout optimization.

---

## Accessibility (Critical)

- **Add `AutomationProperties.Name` to all interactive controls** (buttons, text inputs, checkboxes). This enables screen readers to identify and describe each control.
- **Ensure keyboard navigation works end-to-end:** Tab order, Enter/Space to activate buttons, Escape to cancel dialogs.
- **Test with Narrator** (Windows built-in screen reader) to verify accessibility.
- **Color contrast:** Text must meet WCAG AA (4.5:1 for normal text, 3:1 for large text). Test in light, dark, and high-contrast themes.
- **Don't rely on color alone** to convey information (e.g., red = error); include text labels and icons.

```xml
<!-- GOOD: Clear automation properties -->
<Button
    x:Name="SaveButton"
    AutomationProperties.Name="Save recording"
    Command="{x:Bind ViewModel.SaveCommand}"
    Content="Save" />

<TextBlock AutomationProperties.Name="Recording status">
    <Run Foreground="{ThemeResource SystemFillColorCriticalBrush}">●</Run>
    <Run Text="{x:Bind ViewModel.RecordingTime, Mode=OneWay}" />
</TextBlock>

<!-- AVOID: No automation properties -->
<Button Content="Save" />
<TextBlock Foreground="Red" Text="Recording" />
```

See `.github/instructions/accessibility.instructions.md` for full accessibility guidelines.

---

## Localization

- **Put all user-facing strings in `.resw` resource files** (located in `Strings/en-us/Resources.resw`). Never hard-code strings in XAML or C#.
- **Use `x:Uid` in XAML** to link controls to resource strings. The resource key is derived from `x:Uid`.
- **Use `ResourceLoader` in C# code** to load strings dynamically.

```xml
<!-- GOOD: x:Uid links to resource strings -->
<Button
    x:Uid="SaveButton"
    Content="Save"
    ToolTipService.ToolTip="Save the recording" />

<!-- Resource file (Resources.resw):
     SaveButton.Content = "Save"
     SaveButton/ToolTipService.ToolTip = "Save the recording"
-->
```

See `.github/instructions/globalization.instructions.md` for full localization guidelines.

---

## MVVM & Data Binding

- **Set DataContext to the ViewModel** in XAML (not in code-behind).
- **Never set property values directly from code-behind**; use data bindings instead.
- **ViewModel properties should use `[ObservableProperty]`** (from CommunityToolkit.Mvvm) for automatic `INotifyPropertyChanged` support.
- **Commands should use `[RelayCommand]`** (from CommunityToolkit.Mvvm) for automatic command implementation.

```xml
<!-- GOOD: DataContext set in XAML -->
<Page x:Class="IntVue.Views.MainPage" xmlns:vm="using:IntVue.ViewModels">
    <Page.DataContext>
        <vm:MainViewModel />
    </Page.DataContext>

    <Grid>
        <TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}" />
        <Button Command="{x:Bind ViewModel.LoadCommand}" Content="Load" />
    </Grid>
</Page>
```

See `.github/instructions/winui-best-practices.instructions.md` for MVVM architecture details.

---

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Using `{Binding}` in XAML | Switch to `x:Bind` with `Mode=OneWay` or `Mode=OneTime` |
| Hard-coded colors (e.g., `Foreground="#000000"`) | Replace with `{ThemeResource TextFillColorPrimaryBrush}` |
| Using `Windows.UI.Xaml.Controls` | Switch to `Microsoft.UI.Xaml.Controls` (WinUI 3) |
| Calling `Window.Current` | Pass window reference explicitly in method parameters |
| Using `CoreDispatcher` | Use `DispatcherQueue` instead |
| Long startup time due to heavy UI loading | Use `x:Load` for secondary content; defer loading until needed |
| Controls not accessible to screen readers | Add `AutomationProperties.Name` to all interactive controls |
| Hard-coded strings in UI | Use `x:Uid` + `.resw` resource files for localization |

---

## References

| File | When to consult |
|---|---|
| `winui-best-practices.instructions.md` | MVVM architecture, x:Bind, DI, navigation, theming, system backdrop |
| `accessibility.instructions.md` | Keyboard navigation, screen readers, automation properties, contrast |
| `globalization.instructions.md` | Localization, x:Uid, .resw files, multi-language testing |
| `performance.instructions.md` | Data binding (x:Bind vs {Binding}), x:Load, XAML depth, virtualization |
| `design-principles.instructions.md` | UI component design, DRY, KISS, SOLID (component responsibility) |
| `testing.instructions.md` | ViewModel testing, mocking, AAA pattern |

---

## Quick Checklist

- [ ] All bindings use `x:Bind` with appropriate `Mode` (OneWay/OneTime/TwoWay)
- [ ] No hard-coded colors; all use `{ThemeResource ...}`
- [ ] All interactive controls have `AutomationProperties.Name`
- [ ] Keyboard navigation works end-to-end (Tab, Enter, Escape)
- [ ] User-facing strings use `x:Uid` and `.resw` resource files
- [ ] Heavy UI sections use `x:Load` for deferred loading
- [ ] Controls use `Microsoft.UI.Xaml`, not `Windows.UI.Xaml`
- [ ] DataContext set in XAML (not code-behind)
- [ ] Properties use `[ObservableProperty]` and commands use `[RelayCommand]`
- [ ] ViewModels have >80% test coverage
- [ ] UI tested in light, dark, and high-contrast themes
- [ ] UI tested with Narrator (screen reader)
- [ ] Color contrast meets WCAG AA (4.5:1 for text, 3:1 for large text)
