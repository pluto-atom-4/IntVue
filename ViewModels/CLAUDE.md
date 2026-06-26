---
description: MVVM patterns, ObservableProperty, RelayCommand, async command patterns, and ViewModel testing
applyTo: ViewModels/**/*.cs
---

# ViewModels — Scoped Guidance

ViewModels manage UI state and commands. They act as the data binding layer between Views and Services. ViewModels must contain no UI references and no business logic — that belongs in Services.

---

## ViewModel Responsibility

**What ViewModels do:**
- Hold **UI state** (properties that the View binds to)
- Expose **commands** that the View binds to buttons/interactions
- **Transform data** from Services into a form suitable for UI display
- **Orchestrate navigation** and multi-step workflows via dependency injection

**What ViewModels must NOT do:**
- **No business logic:** That belongs in Services
- **No UI references:** Never reference Views, Windows, or `DispatcherQueue` directly
- **No direct file I/O:** Use Services for media capture, file operations, etc.
- **No hard-coded data:** Use Services to fetch data; ViewModel only displays it

---

## ViewModel Base Class

Use **`CommunityToolkit.Mvvm`** for boilerplate-free ViewModels:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IMediaCaptureService _mediaCaptureService;

    // UI state properties
    [ObservableProperty]
    private string recordingTime = "00:00:00";

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public RecordingViewModel(IMediaCaptureService mediaCaptureService)
    {
        _mediaCaptureService = mediaCaptureService;
    }

    // Async command
    [RelayCommand]
    private async Task StartRecordingAsync()
    {
        try
        {
            IsRecording = true;
            StatusMessage = "Recording...";
            await _mediaCaptureService.StartRecordingAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsRecording = false;
        }
    }

    // Sync command
    [RelayCommand]
    private void StopRecording()
    {
        IsRecording = false;
        StatusMessage = "Recording stopped.";
    }
}
```

**Key patterns:**
- Inherit from `ObservableObject` (provides `INotifyPropertyChanged`)
- Use `[ObservableProperty]` attribute for properties (auto-generates change notification)
- Use `[RelayCommand]` attribute for commands (auto-generates `ICommand` implementation)
- Constructor-inject services (never resolve via `App.Services.GetService()`)
- Keep methods short; extract complex logic into services

---

## Async Command Patterns

- **Always use `async Task`** for command handlers, never sync methods that do I/O
- **Never block on async operations** (no `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- **Handle exceptions gracefully**: catch, log, and update UI state (StatusMessage, ErrorState)
- **Set loading state**: Use `IsLoading` property to disable UI during async operations

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        
        // Call async service method
        var data = await _dataService.GetDataAsync();
        
        // Update UI state
        Items.Clear();
        foreach (var item in data)
        {
            Items.Add(item);
        }
    }
    catch (Exception ex)
    {
        ErrorMessage = "Failed to load data.";
        // Log the full exception internally, but only show user-friendly message in UI
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## ObservableProperty Patterns

- **Auto-property pattern:** Use `[ObservableProperty]` with private backing field for type safety
- **Boolean flags for UI state:** `IsLoading`, `IsRecording`, `HasError`, etc.
- **Default values:** Initialize to sensible defaults (empty string, zero, false)

```csharp
// GOOD: Auto-property with default value
[ObservableProperty]
private string title = string.Empty;

[ObservableProperty]
private int itemCount = 0;

[ObservableProperty]
private bool isLoading;  // Defaults to false

// GOOD: Validation or computed logic (if needed, keep it brief)
[ObservableProperty]
private string userName = string.Empty;
partial void OnUserNameChanged(string value)
{
    // Validate or update related properties
    IsValidName = !string.IsNullOrEmpty(value);
}
```

---

## Dependency Injection

- **Constructor injection:** All services passed via constructor, never resolved via `App.Services`
- **Interface-based:** Depend on `IMyService`, not concrete `MyService` class
- **Testability:** Design so services can be easily mocked in unit tests

```csharp
public partial class MyViewModel : ObservableObject
{
    private readonly IMediaCaptureService _mediaService;
    private readonly INavigationService _navigationService;

    // GOOD: Constructor injection
    public MyViewModel(IMediaCaptureService mediaService, INavigationService navigationService)
    {
        _mediaService = mediaService;
        _navigationService = navigationService;
    }

    // AVOID: Resolving from service locator
    // private IMediaCaptureService _mediaService = App.Services.GetService<IMediaCaptureService>();
}
```

---

## Testing ViewModels

- **Mock all service dependencies** using Moq
- **Test both happy path and error paths** (success, exception, timeout, etc.)
- **Use AAA pattern:** Arrange (setup mocks) → Act (call command) → Assert (verify state)
- **Test naming:** `MethodName_Scenario_ExpectedResult` (e.g., `StartRecordingAsync_WhenDeviceUnavailable_SetsErrorMessage`)

```csharp
[TestClass]
public class RecordingViewModelTests
{
    [TestMethod]
    public async Task StartRecordingAsync_OnSuccess_SetsIsRecordingTrue()
    {
        // Arrange
        var mockMediaService = new Mock<IMediaCaptureService>();
        mockMediaService.Setup(s => s.StartRecordingAsync()).Returns(Task.CompletedTask);
        var viewModel = new RecordingViewModel(mockMediaService.Object);

        // Act
        await viewModel.StartRecordingCommand.ExecuteAsync(null);

        // Assert
        Assert.IsTrue(viewModel.IsRecording);
        Assert.AreEqual("Recording...", viewModel.StatusMessage);
    }

    [TestMethod]
    public async Task StartRecordingAsync_WhenServiceThrows_SetsErrorMessage()
    {
        // Arrange
        var mockMediaService = new Mock<IMediaCaptureService>();
        mockMediaService.Setup(s => s.StartRecordingAsync())
            .ThrowsAsync(new InvalidOperationException("Device unavailable"));
        var viewModel = new RecordingViewModel(mockMediaService.Object);

        // Act
        await viewModel.StartRecordingCommand.ExecuteAsync(null);

        // Assert
        Assert.IsFalse(viewModel.IsRecording);
        Assert.IsTrue(viewModel.StatusMessage.Contains("Error"));
    }
}
```

See `.github/instructions/testing.instructions.md` for full testing guidelines.

---

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| ViewModel calls business logic directly (no Services) | Extract logic to Service; inject into ViewModel |
| ViewModel blocks UI thread with `.Result` or `.Wait()` | Use `async/await`; never block async operations |
| ViewModel references View or Window | Remove the reference; pass data via properties/commands |
| Command handler is synchronous but does I/O | Make it `async Task` and use `await` |
| Properties don't raise `INotifyPropertyChanged` | Use `[ObservableProperty]` attribute (auto-generates) |
| ViewModel has >200 lines | Split into multiple ViewModels; extract logic to Services |
| Test fails intermittently due to shared state | Ensure each test is independent; reset mocks between tests |

---

## References

| File | When to consult |
|---|---|
| `winui-best-practices.instructions.md` | MVVM architecture, ViewModel responsibility, DI |
| `testing.instructions.md` | Unit testing, Moq mocking, AAA pattern, test naming |
| `performance.instructions.md` | Async patterns, blocking calls, threading constraints |
| `design-principles.instructions.md` | ViewModel design, SRP (one reason to change), DIP (depend on abstractions) |

---

## Quick Checklist

- [ ] ViewModel inherits from `ObservableObject` (CommunityToolkit.Mvvm)
- [ ] All properties use `[ObservableProperty]` attribute
- [ ] All commands use `[RelayCommand]` attribute
- [ ] All async methods use `async Task` or `async Task<T>` (never sync I/O)
- [ ] No blocking calls (no `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- [ ] All service dependencies constructor-injected
- [ ] No references to Views, Windows, or DispatcherQueue
- [ ] Business logic delegated to Services (ViewModel only transforms/displays data)
- [ ] Error handling: exceptions caught and user-friendly messages set in UI state
- [ ] Loading state managed (`IsLoading` flag set during async operations)
- [ ] ViewModel has >80% test coverage
- [ ] All tests are independent (no shared state between tests)
- [ ] Command tests use mocked services (Moq)
