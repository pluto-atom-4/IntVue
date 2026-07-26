# Copilot Rules — Detailed Examples

Reference file for [.github/copilot-instructions.md](../copilot-instructions.md). Contains full YAML configurations and code examples.

---

## XAML Files (`**/*.xaml`)

```yaml
---
applyTo: "**/*.xaml"
category: "ui"
references:
  - .github/instructions/accessibility.instructions.md
  - .github/instructions/performance.instructions.md
  - .claude/rules/design-components.rules.md
---
```

### XAML Examples

**Correct x:Bind usage:**
```xaml
<TextBlock Text="{x:Bind ViewModel.RecordingTime, Mode=OneWay}" />
<Button IsEnabled="{x:Bind ViewModel.CanRecord, Mode=OneWay}" />
```

**Correct color usage:**
```xaml
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
<Border Background="{ThemeResource SolidBackgroundFillColorBaseBrush}" />
```

---

## C# ViewModels (`ViewModels/**/*.cs`)

```yaml
---
applyTo: "ViewModels/**/*.cs"
category: "mvvm"
references:
  - .github/instructions/winui-best-practices.instructions.md
  - .github/instructions/code-quality.instructions.md
---
```

### ViewModel Example

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string recordingStatus = "Ready";

    [RelayCommand]
    public async Task StartRecordingAsync()
    {
        recordingStatus = "Recording...";
        // Implementation
    }
}
```

---

## Services (`Services/**/*.cs`)

```yaml
---
applyTo: "Services/**/*.cs"
category: "business-logic"
references:
  - .github/instructions/security.instructions.md
  - .github/instructions/performance.instructions.md
---
```

### Service Example

```csharp
public class RecordingService : IDisposable
{
    public async Task<bool> StartRecordingAsync(string filePath)
    {
        // Validate input at boundary
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        // Async operation
        return await InitializeRecorderAsync(filePath);
    }
}
```

---

## Test Files (`**/*Tests.cs`)

```yaml
---
applyTo: "**/*Tests.cs, **/*Test.cs"
category: "testing"
references:
  - .github/instructions/testing.instructions.md
---
```

### Test Example (Arrange-Act-Assert)

```csharp
[TestMethod]
public void StartRecording_WithValidPath_ReturnsTrue()
{
    // Arrange
    var service = new RecordingService();
    string filePath = "/tmp/recording.wav";

    // Act
    var result = service.StartRecording(filePath);

    // Assert
    Assert.IsTrue(result);
    Assert.IsNotNull(service.CurrentRecording);
}
```

### Test Naming Convention

- `MethodName_Scenario_ExpectedResult`
- Example: `StartRecording_WithNullPath_ThrowsArgumentNullException`
- Example: `DeleteRecording_FileExists_ReturnsTrue`

---

## YAML Schema Template

```yaml
---
applyTo: "**/*.pattern"
category: "category-name"
references:
  - .github/instructions/instruction-file.md
  - .claude/rules/rule-file.md
---
```

**Fields:**
- `applyTo`: Glob pattern for matching files
- `category`: Rule category (ui, mvvm, business-logic, testing)
- `references`: Array of linked instruction files (see main file)

---

**Last Updated:** 2026-07-25
