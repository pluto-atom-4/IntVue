# Copilot Agent Mode — Pattern Rules (Detailed Reference)

Reference guide for Copilot Agent Mode. Compact rules in `.github/copilot-instructions.md`; detailed examples here.

---

## XAML Binding & Styling

**DO:**
- Use `x:Bind` (strongly-typed, compile-time checked)
- Use `{ThemeResource ...Brush}` for colors (light/dark/high-contrast support)
- Use `{ThemeResource ...}` for spacing (8px grid)
- Use semantic tokens: `TextFillColorPrimaryBrush`, `ControlStrongFillColorDefaultBrush`
- Set `AutomationProperties.Name` on interactive controls
- Use `CornerRadius` from scale (2, 8, 16)

**DO NOT:**
- Use `{Binding}` (WinForms-style, no type checking)
- Hard-code colors, spacing, or fonts
- Omit automation properties
- Use arbitrary values

**Example (DO):**
```xaml
<StackPanel Spacing="8" Padding="12">
    <TextBlock Text="Recording" FontSize="14" 
        Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
    <Button Content="Start" AutomationProperties.Name="Start recording"
        Style="{ThemeResource AccentButtonStyle}" />
</StackPanel>
```

---

## C# ViewModels (MVVM + DI)

**DO:**
- Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Use `[ObservableProperty]` for properties
- Use `RelayCommand` for user actions
- Depend on Services via constructor (DI)
- Validate input at entry points

**DO NOT:**
- Mix business logic in ViewModel
- Use auto-properties without INotifyPropertyChanged
- Hard-code service instantiation
- Hold state across views
- Use `Task.Run` without `ConfigureAwait(false)`

**Example (DO):**
```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IRecordingService _recordingService;

    [ObservableProperty]
    private bool isRecording;

    public MainViewModel(IRecordingService recordingService)
    {
        _recordingService = recordingService;
    }

    [RelayCommand]
    private async Task StartRecording()
    {
        IsRecording = await _recordingService.StartAsync();
    }
}
```

---

## C# Services (Stateless, Boundary Validation)

**DO:**
- Validate all inputs at boundaries
- Use dependency injection (constructor only)
- Return meaningful errors with context
- Dispose resources properly
- Write async with `ConfigureAwait(false)`

**DO NOT:**
- Hold state across calls
- Throw bare exceptions
- Catch `Exception` broadly
- Use `Task.Result` (deadlock risk)
- Dispose resources lazily

**Example (DO):**
```csharp
public class RecordingService : IRecordingService
{
    public async Task<bool> StartAsync()
    {
        if (string.IsNullOrEmpty(_outputPath))
            throw new InvalidOperationException("Output path not set");

        try
        {
            await _mediaCapture.InitializeAsync();
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Camera access denied: {ex.Message}");
            return false;
        }
    }

    public void Dispose() => _mediaCapture?.Dispose();
}
```

---

## C# Tests (MSTest + AAA)

**DO:**
- Use MSTest + Moq
- Follow AAA: Arrange → Act → Assert
- Name: `MethodName_Scenario_ExpectedResult`
- Target ≥80% coverage on ViewModels/Services
- Use `async Task` for async tests
- Mock all Service dependencies

**DO NOT:**
- Use generic names ("Test1", "DoTest")
- Mix concerns (one test = one behavior)
- Use `[TestInitialize]` for complex setup
- Assert on exact error text
- Skip coverage

**Example (DO):**
```csharp
[TestMethod]
public async Task LoadItems_OnSuccess_PopulatesCollection()
{
    // Arrange
    var mockService = new Mock<IItemService>();
    mockService.Setup(s => s.LoadAsync()).ReturnsAsync(new[] { "Item1", "Item2" });
    var viewModel = new MainViewModel(mockService.Object);

    // Act
    await viewModel.LoadItemsCommand.ExecuteAsync(null);

    // Assert
    Assert.AreEqual(2, viewModel.Items.Count);
}
```

---

## Git Commits & PR References

**DO:**
- Reference issues: `git commit -m "feat: Add feature (Closes #123)"`
- Use conventional commits: `feat:`, `fix:`, `refactor:`, `test:`, `style:`, `docs:`, `chore:`
- Keep subject ≤50 chars
- Add body if "why" isn't obvious

**DO NOT:**
- Commit without issue reference
- Use generic subjects
- Commit secrets or certificates
- Force-push to `main`

---

## Test Commands

**Before commit:**
```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
gitleaks detect --source . -v
```

**Run one test class:**
```powershell
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"
```

---

## Cross-References

- Color tokens: `.claude/rules/design-colors.rules.md`
- Spacing scale: `.claude/rules/design-spacing.rules.md`
- Typography: `.claude/rules/design-typography.rules.md`
- Components: `.claude/rules/design-components.rules.md`
- Accessibility: `.github/instructions/accessibility.instructions.md`
- Testing framework: `.github/instructions/testing.instructions.md`
- MVVM patterns: `.github/instructions/winui-best-practices.instructions.md`

