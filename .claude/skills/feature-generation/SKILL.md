---
name: feature-generation
description: Scaffolds new WinUI pages, ViewModels, and services following MVVM conventions and project structure
trigger: "scaffold page|generate feature|create page|new winui page|new feature"
---

# Feature Generation Playbook

This skill scaffolds a new feature (page, ViewModel, service) following IntVue's MVVM architecture. Use this when you need to create a complete, working feature from scratch.

---

## When to Use This Skill

- Creating a new page or dialog in the app
- Adding a new user-facing feature with backing ViewModel and Service
- Scaffolding multiple related components at once
- Ensuring consistency with project structure and naming conventions

---

## Playbook

### 1. Define the Feature Concept

Ask yourself:
- **What does the user want to do?** (e.g., "view interview history")
- **Where does this belong?** (e.g., main app, a dialog, a sidebar panel)
- **What data does it need?** (e.g., list of past interviews, recording metadata)
- **What actions can the user take?** (e.g., play recording, delete recording, export)

### 2. Create the Model (Data Structure)

**File location:** `Models/FeatureModel.cs`

```csharp
public class FeatureModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Do:**
- Define properties for all data the feature displays
- Use clear, descriptive property names
- Include IDs, timestamps, status flags as needed

**Avoid:**
- No business logic; data only
- No UI concerns (no Visibility, Color, etc.)
- No service calls; models are data containers

### 3. Create the Service (Business Logic)

**File location:** `Services/IFeatureService.cs` (interface) and `Services/FeatureService.cs` (implementation)

```csharp
public interface IFeatureService
{
    Task<List<FeatureModel>> GetFeaturesAsync();
    Task<FeatureModel> GetFeatureAsync(string id);
    Task SaveFeatureAsync(FeatureModel feature);
    Task DeleteFeatureAsync(string id);
}

public class FeatureService : IFeatureService
{
    // Constructor-inject any dependencies (IMediaCaptureService, file services, etc.)
    public FeatureService(/* dependencies */) { }

    // Implement interface methods
    public async Task<List<FeatureModel>> GetFeaturesAsync()
    {
        // Business logic here: fetch from storage, media, etc.
        // Always use async/await
    }
}
```

**Do:**
- Define interface first, then implementation
- Use `async Task` / `async Task<T>` for all I/O
- Inject dependencies via constructor
- Handle errors gracefully (throw or return error state)

**Avoid:**
- No UI references; no DispatcherQueue, Views, etc.
- No blocking calls (no `.Result`, `.Wait()`)
- Avoid deep logic; keep methods focused

### 4. Create the ViewModel (UI State & Commands)

**File location:** `ViewModels/FeatureViewModel.cs`

```csharp
public partial class FeatureViewModel : ObservableObject
{
    private readonly IFeatureService _featureService;

    [ObservableProperty]
    private ObservableCollection<FeatureModel> items = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public FeatureViewModel(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;
            var items = await _featureService.GetFeaturesAsync();
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load items.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(FeatureModel item)
    {
        try
        {
            await _featureService.DeleteFeatureAsync(item.Id);
            Items.Remove(item);
            StatusMessage = "Item deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to delete item.";
        }
    }
}
```

**Do:**
- Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Use `[ObservableProperty]` for all properties
- Use `[RelayCommand]` for all commands
- Constructor-inject service dependencies
- Handle errors; set `StatusMessage` for user feedback
- Use `IsLoading` flag to disable UI during async operations

**Avoid:**
- No business logic; delegate to Service
- No UI references (Window, View, DispatcherQueue)
- No blocking async calls

### 5. Create the View (XAML Page)

**File location:** `Views/FeaturePage.xaml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="IntVue.Views.FeaturePage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:IntVue.ViewModels">

    <Page.DataContext>
        <vm:FeatureViewModel />
    </Page.DataContext>

    <Grid Padding="20" RowSpacing="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock
            Grid.Row="0"
            Text="Features"
            Style="{StaticResource TitleTextBlockStyle}" />

        <!-- List -->
        <ListView
            Grid.Row="1"
            ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
            IsEnabled="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource ...}}"
            SelectionMode="Single">
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="vm:FeatureModel">
                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>
                        <TextBlock Text="{x:Bind Name, Mode=OneWay}" />
                        <Button
                            Grid.Column="1"
                            x:Name="DeleteButton"
                            AutomationProperties.Name="Delete feature"
                            Command="{x:Bind ViewModel.DeleteItemCommand, Mode=OneTime}"
                            CommandParameter="{x:Bind}"
                            Content="Delete" />
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Status & Actions -->
        <StackPanel Grid.Row="2" Spacing="8">
            <ProgressRing IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}" />
            <TextBlock
                Foreground="{ThemeResource SystemFillColorCaution}"
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
            <Button
                Command="{x:Bind ViewModel.LoadItemsCommand, Mode=OneTime}"
                Content="Refresh"
                IsEnabled="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource ...}}" />
        </StackPanel>
    </Grid>
</Page>
```

**Do:**
- Use `x:Bind` with appropriate `Mode` (OneWay/OneTime/TwoWay)
- Set DataContext to ViewModel in XAML
- Use theme resources, never hard-coded colors
- Add `AutomationProperties.Name` to interactive controls
- Use user-friendly labels in status messages

**Avoid:**
- Never use `{Binding}` (slower, no compile-time checks)
- Never set properties from code-behind; use data binding
- Never hard-code colors or strings

### 6. Register in Dependency Injection

**File location:** `App.xaml.cs`

```csharp
private static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Services
    services.AddSingleton<IFeatureService, FeatureService>();

    // ViewModels
    services.AddTransient<FeatureViewModel>();

    return services.BuildServiceProvider();
}
```

### 7. Add Navigation (if it's a new page)

If this is a top-level page, add it to `NavigationView` in `MainWindow.xaml`:

```xml
<NavigationViewItem
    Content="Features"
    Icon="Library"
    Tag="FeaturePage" />
```

And update `MainWindow.xaml.cs` or your navigation service to handle the route.

### 8. Build & Test

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# Build
dotnet build -c Debug -p:Platform=$Platform

# Test (create unit tests for ViewModel and Service)
cd Tests/IntVue.Tests
dotnet test -c Debug -p:Platform=$Platform

# Run the app
cd ..
dotnet run -c Debug -p:Platform=$Platform
```

### 9. Write Unit Tests

**File location:** `Tests/IntVue.Tests/ViewModels/FeatureViewModelTests.cs`

```csharp
[TestClass]
public class FeatureViewModelTests
{
    [TestMethod]
    public async Task LoadItemsAsync_OnSuccess_PopulatesItems()
    {
        // Arrange
        var mockService = new Mock<IFeatureService>();
        var items = new List<FeatureModel> { new() { Id = "1", Name = "Test" } };
        mockService.Setup(s => s.GetFeaturesAsync()).ReturnsAsync(items);
        var viewModel = new FeatureViewModel(mockService.Object);

        // Act
        await viewModel.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, viewModel.Items.Count);
        Assert.AreEqual("Test", viewModel.Items[0].Name);
    }

    [TestMethod]
    public async Task LoadItemsAsync_WhenServiceThrows_SetsStatusMessage()
    {
        // Arrange
        var mockService = new Mock<IFeatureService>();
        mockService.Setup(s => s.GetFeaturesAsync()).ThrowsAsync(new Exception("Network error"));
        var viewModel = new FeatureViewModel(mockService.Object);

        // Act
        await viewModel.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.IsTrue(viewModel.StatusMessage.Contains("Failed"));
    }
}
```

See `ViewModels/CLAUDE.md` and `.github/instructions/testing.instructions.md` for detailed testing patterns.

### 10. Verify in the App

1. Build and run: `dotnet run -c Debug -p:Platform=$Platform`
2. Navigate to the new page
3. Test all user interactions (load, delete, refresh)
4. Test error scenarios (network error, permission denied)
5. Verify UI renders correctly in light, dark, and high-contrast themes
6. Test keyboard navigation (Tab, Enter, Escape)
7. Test with Narrator (screen reader) to verify accessibility

---

## Checklist

- [ ] Model created (`Models/FeatureModel.cs`)
- [ ] Service interface created (`Services/IFeatureService.cs`)
- [ ] Service implementation created (`Services/FeatureService.cs`)
- [ ] ViewModel created (`ViewModels/FeatureViewModel.cs`)
- [ ] View/Page created (`Views/FeaturePage.xaml`)
- [ ] Service registered in `App.xaml.cs`
- [ ] Navigation added (if new page)
- [ ] Unit tests written (>80% coverage)
- [ ] App builds without errors
- [ ] All tests pass
- [ ] Feature works in the running app
- [ ] UI tested in light, dark, high-contrast themes
- [ ] Keyboard navigation verified
- [ ] Screen reader (Narrator) accessibility tested

---

## References

- **MVVM Pattern:** `./CLAUDE.md` → High-Level Architecture
- **ViewModel Details:** `ViewModels/CLAUDE.md`
- **Service Details:** `Services/CLAUDE.md`
- **View/XAML Details:** `Views/CLAUDE.md`
- **Testing:** `.github/instructions/testing.instructions.md`
- **Design Principles:** `.github/instructions/design-principles.instructions.md`
