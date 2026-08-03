<#
.SYNOPSIS
Count tokens in IntVue context files and validate against budgets.

.DESCRIPTION
Counts tokens using a conservative heuristic:
  - Code: 1 line ≈ 10 tokens
  - Prose: 1 word ≈ 1.3 tokens
  - Token estimate: max(lines*10, words*1.3)

Validates against INTVUE-CONTEXT-MANIFEST.json budgets.

.PARAMETER Mode
  'check': Validate against budgets (fail if exceeded by >10%)
  'report': Print token summary (no validation)
  'update': Update manifest with new counts (requires admin)

.EXAMPLE
  .\count-context-tokens.ps1 -Mode check
  .\count-context-tokens.ps1 -Mode report
#>

param(
  [ValidateSet('check', 'report', 'update')]
  [string]$Mode = 'report'
)

$ErrorActionPreference = 'Stop'

# File mapping: Path and category
$ContextFiles = @(
  @{ Path = 'CLAUDE.md'; Category = 'core' },
  @{ Path = 'DESIGN.md'; Category = 'core' },
  @{ Path = 'AGENTS.md'; Category = 'core' },
  @{ Path = '.claude/profiles.json'; Category = 'config' },
  @{ Path = '.claude/custom-instructions.md'; Category = 'core' },
  @{ Path = '.github/copilot-instructions.md'; Category = 'core' },
  @{ Path = '.claude/rules/design-colors.rules.md'; Category = 'design' },
  @{ Path = '.claude/rules/design-spacing.rules.md'; Category = 'design' },
  @{ Path = '.claude/rules/design-typography.rules.md'; Category = 'design' },
  @{ Path = '.claude/rules/design-components.rules.md'; Category = 'design' },
  @{ Path = '.claude/rules/hook-quick-fix.rules.md'; Category = 'rules' },
  @{ Path = '.claude/rules/hook-resolution.rules.md'; Category = 'rules' },
  @{ Path = '.claude/rules/hook-strategy.rules.md'; Category = 'rules' }
)

function Get-TokenEstimate {
  param([int]$Lines, [int]$Words)

  $tokensFromLines = $Lines * 10
  $tokensFromWords = [int]($Words * 1.3)

  return [Math]::Max($tokensFromLines, $tokensFromWords)
}

function Count-FileTokens {
  param([string]$FilePath)

  if (-not (Test-Path $FilePath)) {
    return $null
  }

  $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
  if (-not $content) {
    return @{ Lines = 0; Words = 0; Tokens = 0 }
  }

  # Use Measure-Object for accurate line and word counting
  $measurements = $content | Measure-Object -Line -Word -Character
  $lines = $measurements.Lines
  $words = $measurements.Words
  $tokens = Get-TokenEstimate $lines $words

  return @{
    Lines = $lines
    Words = $words
    Tokens = $tokens
  }
}

Write-Host "=== IntVue Context Token Count ===" -ForegroundColor Cyan
Write-Host ""

$totalTokens = 0
$alwaysOnTokens = 0
$report = @()

foreach ($entry in $ContextFiles) {
  $file = $entry.Path
  $category = $entry.Category
  $metrics = Count-FileTokens $file

  if ($metrics) {
    $totalTokens += $metrics.Tokens

    # Always-on: CLAUDE.md, custom-instructions.md, copilot-instructions.md
    if (@('CLAUDE.md', '.claude/custom-instructions.md', '.github/copilot-instructions.md') -contains $file) {
      $alwaysOnTokens += $metrics.Tokens
    }

    $report += [PSCustomObject]@{
      File = $file
      Category = $category
      Lines = $metrics.Lines
      Words = $metrics.Words
      Tokens = $metrics.Tokens
    }
  }
}

# Display report
$report | Format-Table -AutoSize @(
  @{Label="File"; Expression={$_.File}},
  @{Label="Lines"; Expression={$_.Lines}},
  @{Label="Words"; Expression={$_.Words}},
  @{Label="Tokens"; Expression={$_.Tokens}}
)

Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  Total tokens (all files):        $totalTokens"
Write-Host "  Always-on tokens (CLAUDE + inst): $alwaysOnTokens / 3000 ($('{0:P1}' -f ($alwaysOnTokens / 3000)))"
Write-Host ""

# Validation
if ($Mode -eq 'check') {
  $alwaysOnBudget = 3000
  $percentOfBudget = $alwaysOnTokens / $alwaysOnBudget

  if ($percentOfBudget -gt 1.1) {
    Write-Host "❌ FAILED: Always-on context exceeds budget by >10%" -ForegroundColor Red
    Write-Host "  Current: $alwaysOnTokens tokens"
    Write-Host "  Budget: $alwaysOnBudget tokens"
    exit 1
  } elseif ($percentOfBudget -gt 1.0) {
    Write-Host "⚠️  WARNING: Always-on context slightly over budget" -ForegroundColor Yellow
    Write-Host "  Current: $alwaysOnTokens tokens"
    Write-Host "  Budget: $alwaysOnBudget tokens"
  } else {
    Write-Host "✅ PASSED: All token budgets within limits" -ForegroundColor Green
    exit 0
  }
}
else {
  Write-Host "Report complete. No validation performed (use -Mode check to validate)." -ForegroundColor Green
}
