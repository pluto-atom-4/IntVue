---
description: 'WinUI 3 / WinAppSDK architecture, MVVM, XAML patterns, DI, theming, and controls guidance'
applyTo: '**/*.cs, **/*.xaml, **/*.csproj'
---

# WinUI 3 / WinAppSDK -- Best Practices & Patterns

This file covers WinUI 3-specific patterns, conventions, and architecture guidance for this project.

---

## 1. Architecture -- MVVM Pattern

### Overview

Use **Model-View-ViewModel (MVVM)** for all UI features:

| Layer | Responsibility | Example |
|---|---|---|
| **Model** | Data structures & business entities | `Item.cs`, `UserProfile.cs` |
| **View** | XAML UI -- layout, styles, animations | `MainPage.xaml` |
| **ViewModel** | UI state, commands, data transformation | `MainViewModel.cs` |
| **Service** | Business logic, data access, navigation | `IDataService.cs`, `NavigationService.cs` |

### Project Folder Structure

```
<ProjectName>/
  Models/           -> Data classes
  ViewModels/       -> ViewModels (one per page/dialog)
  Views/            -> XAML pages and windows
  Services/         -> Business logic & platform services
  Converters/       -> IValueConverter implementations
  Helpers/          -> Static utility methods
  Controls/         -> Custom/reusable controls
  Strings/
    en-us/
      Resources.resw
  Assets/           -> Images, icons, splash screens
```

### ViewModel Base

Use `CommunityToolkit.Mvvm` (recommended) for boilerplate-free ViewModels -- `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, constructor injection:

```xml
<!-- Use latest stable version; do not hard-code version numbers in instructions -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="*" />
```

See `ViewModels/CLAUDE.md` -> "ViewModel Base Class" for the canonical example.

### View-ViewModel Binding

Set `DataContext` in XAML (not code-behind) and bind with `x:Bind`. See `Views/CLAUDE.md` -> "MVVM & Data Binding" for the canonical example.

---

## 2. XAML Best Practices

### Use `x:Bind` Over `{Binding}`

| Feature | `x:Bind` | `{Binding}` |
|---|---|---|
| Compile-time check | Yes | No |
| Performance | Faster (compiled) | Slower (reflection) |
| Default mode | OneTime | OneWay |
| IntelliSense | Yes | No |

See `Views/CLAUDE.md` -> "XAML Data Binding" for a code example.

### Use `x:Load` for Deferred Loading

See `Views/CLAUDE.md` -> "Deferred Loading & Performance" for a code example.

### Use WinUI Controls from the SDK

Prefer the WinUI 3 controls from `Microsoft.UI.Xaml.Controls`, **not** the older UWP `Windows.UI.Xaml.Controls`:

```csharp
// GOOD -- WinUI 3
using Microsoft.UI.Xaml.Controls;

// AVOID -- UWP (won't work in WinUI 3 desktop)
// using Windows.UI.Xaml.Controls;
```

### XAML Formatting Convention

```xml
<Button
    x:Name="SaveButton"
    x:Uid="SaveButton"
    AutomationProperties.Name="Save"
    Command="{x:Bind ViewModel.SaveCommand}"
    Style="{StaticResource AccentButtonStyle}" />
```

- One attribute per line for controls with 3+ attributes.
- Order: `x:Name` -> `x:Uid` -> `AutomationProperties` -> layout -> data -> style.

---

## 3. Dependency Injection

### Setup with `Microsoft.Extensions.DependencyInjection`

```xml
<!-- Use latest stable version; do not hard-code version numbers in instructions -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="*" />
```

```csharp
// App.xaml.cs
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IDataService, DataService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
```

---

## 4. Navigation

### Frame-based Navigation

```csharp
public interface INavigationService
{
    bool CanGoBack { get; }
    void NavigateTo<TPage>() where TPage : Page;
    void GoBack();
}
```

Use a `NavigationView` with a `Frame`:

```xml
<NavigationView SelectionChanged="OnNavigationChanged">
    <NavigationView.MenuItems>
        <NavigationViewItem Content="Home" Tag="Home" />
        <NavigationViewItem Content="Settings" Tag="Settings" />
    </NavigationView.MenuItems>
    <Frame x:Name="ContentFrame" />
</NavigationView>
```

---

## 5. Windowing & Title Bar

### Custom Title Bar

```csharp
// In Window constructor or Activated handler
ExtendsContentIntoTitleBar = true;
SetTitleBar(AppTitleBar); // AppTitleBar is a UIElement in your XAML
```

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="48" /> <!-- Title bar -->
        <RowDefinition Height="*" />  <!-- Content -->
    </Grid.RowDefinitions>

    <Grid x:Name="AppTitleBar" Grid.Row="0">
        <TextBlock Text="My App" VerticalAlignment="Center" Margin="16,0" />
    </Grid>

    <Frame Grid.Row="1" x:Name="ContentFrame" />
</Grid>
```

### Window Sizing

```csharp
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
var appWindow = AppWindow.GetFromWindowId(windowId);
appWindow.Resize(new SizeInt32(1280, 720));
```

---

## 6. Theming

### Support Light/Dark/High Contrast

```xml
<!-- In App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Detect Theme Changes

```csharp
if (Content is FrameworkElement rootElement)
{
    rootElement.ActualThemeChanged += (s, e) =>
    {
        // React to theme change
    };
}
```

### Use Theme Resources, Not Hard-coded Colors

Never hard-code colors; always use `{ThemeResource ...Brush}`. See `DESIGN.md` and `.claude/rules/design-colors.rules.md` for the full token set and usage examples.

---

## 7. System Backdrop (Mica / Acrylic)

This project uses **Mica** backdrop (already configured in `MainWindow.xaml`):

```xml
<Window.SystemBackdrop>
    <MicaBackdrop />
</Window.SystemBackdrop>
```

Alternatives:
```xml
<DesktopAcrylicBackdrop />   <!-- Acrylic -->
<MicaBackdrop Kind="BaseAlt" /> <!-- Mica Alt -->
```

---

## 8. Community Toolkit

### Recommended Packages

```xml
<!-- Use latest stable versions; do not hard-code version numbers in instructions -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="*" />
<PackageReference Include="CommunityToolkit.WinUI.Controls.SettingsControls" Version="*" />
<PackageReference Include="CommunityToolkit.WinUI.Helpers" Version="*" />
<PackageReference Include="CommunityToolkit.WinUI.Animations" Version="*" />
```

### Useful Toolkit Features

| Feature | Package | Use Case |
|---|---|---|
| `ObservableObject` | CommunityToolkit.Mvvm | ViewModel base class |
| `RelayCommand` | CommunityToolkit.Mvvm | Command implementation |
| `ObservableProperty` | CommunityToolkit.Mvvm | Auto INotifyPropertyChanged |
| `SettingsCard` | CommunityToolkit.WinUI.Controls | Settings pages |
| `IncrementalLoadingCollection` | CommunityToolkit.WinUI | Lazy-loading lists |

---

## 9. Common Pitfalls

For general pitfalls (`Windows.UI.Xaml`, `Window.Current`, `CoreDispatcher`, `{Binding}` not updating), see the larger table in `Views/CLAUDE.md` -> "Common Pitfalls". This table covers pitfalls specific to build/deploy tooling:

| Pitfall | Solution |
|---|---|
| `REGDB_E_CLASSNOTREG` error | Ensure Developer Mode is enabled, then re-register: run `winapp unregister` followed by `dotnet run` (or `winapp run <build-output>`) to refresh the loose-layout registration |
| Stale package state after manifest changes | Run `winapp unregister`, then `dotnet run` -- using `winapp run --clean` additionally wipes `LocalState`/settings to test first-run behavior |
| XAML Designer crashes | Clean & rebuild; ensure platform matches (x64 vs AnyCPU) |

---

## 10. Validation

Build & register the MSIX package -- see **Build, Run & Deploy** in `.github/agents/Agents.md`.

### Verify

- Run the app and verify the changed UI renders correctly on x64.
- Search XAML for `{Binding` -- replace with `x:Bind`.
- Search XAML for `Foreground="#` or `Background="#` -- replace with `{ThemeResource}`.
- Search C# for `Windows.UI.Xaml` -- replace with `Microsoft.UI.Xaml`.
- Search C# for `Window.Current` -- replace with explicit window reference.
- Search C# for `CoreDispatcher` -- replace with `DispatcherQueue`.
- Test Light, Dark, and High Contrast themes.

---

## 11. Must Read & Research

> **Agent Rule:** Before making any WinUI/WinAppSDK-related change, you **must** fetch and review the relevant references below using `fetch_webpage`. Consult the appropriate section based on the type of change. Apply what you learn -- do not skip this step.

### Reference Library (Docs, Architecture, Controls, Windows AI)

| # | Reference | When to consult |
|---|---|---|
| 1 | [WinUI 3 Overview](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/) | Starting new features, onboarding to the project |
| 2 | [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/) | Using SDK-specific APIs (windowing, lifecycle, activation) |
| 3 | [WinUI 3 API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/) | Looking up specific class/method signatures |
| 4 | [WinUI 3 Gallery App](https://learn.microsoft.com/en-us/windows/apps/design/controls/) | Choosing controls, reviewing control patterns |
| 5 | [AI Dev Gallery](https://github.com/microsoft/ai-dev-gallery) | On-device AI/ML integration patterns, model usage examples in WinUI |
| 6 | [Windows App SDK Release Notes](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel) | Checking for breaking changes, new APIs, known issues |
| 7 | [CommunityToolkit.Mvvm Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) | Implementing ViewModels, commands, `ObservableProperty` |
| 8 | [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) | Registering services, constructor injection, lifetime management |
| 9 | [Template Studio for WinUI](https://github.com/microsoft/TemplateStudio) | Scaffolding pages, navigation, project structure patterns |
| 10 | [WinUI 3 Gallery (GitHub)](https://github.com/microsoft/WinUI-Gallery) | Example implementations of any WinUI control |
| 11 | [Windows Community Toolkit (GitHub)](https://github.com/CommunityToolkit/Windows) | Before building custom controls -- check if the toolkit already has one |
| 12 | [Fluent Design System](https://learn.microsoft.com/en-us/windows/apps/design/) | Spacing, typography, colour, motion, layout decisions |
| 13 | [XAML Controls Gallery](https://apps.microsoft.com/store/detail/winui-3-gallery/9P3JFPWWDZRC) | Interactive demo of all controls and their properties |
| 14 | [Windows APIs instruction file](windows-apis.instructions.md) | **First stop** -- check if a built-in API already exists for the capability you need (AI, windowing, notifications, widgets, lifecycle, etc.) |
| 15 | [Windows AI APIs](https://learn.microsoft.com/en-us/windows/ai/apis/) | On-device AI: Phi Silica (text gen), OCR, imaging (super-res, description, object extract, erase) |
| 16 | [Windows ML](https://learn.microsoft.com/en-us/windows/ai/new-windows-ml/overview) | Custom ONNX model inference on CPU/GPU/NPU |
| 17 | [Foundry Local](https://learn.microsoft.com/en-us/windows/ai/foundry-local/get-started) | Run OSS LLMs (Llama, Mistral, Phi) locally via REST API |
| 18 | [Windows AI on Windows](https://learn.microsoft.com/en-us/windows/ai/) | AI landing page -- all AI options for Windows apps |

### Samples

> **Agent Rule -- MANDATORY:** Before implementing any WinAppSDK or Platform SDK API you have not used before, **search the samples repo first** and study the working example. Do not guess API usage from docs alone -- see the [Sample-First Rule](windows-apis.instructions.md#sample-first-rule) for details and known pitfalls.

| # | Reference | When to consult |
|---|---|---|
| 19 | [Windows App SDK Samples](https://github.com/microsoft/WindowsAppSDK-Samples) | **Always search here first** before implementing any SDK API for the first time |
| 20 | [Windows AI API Samples](https://github.com/microsoft/WindowsAppSDK-Samples/tree/main/Samples/WindowsAIFoundry/cs-winui) | AI API usage with WinUI (ImageDescription, TextRecognizer, LanguageModel, etc.) |
