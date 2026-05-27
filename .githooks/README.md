Repository Git hooks

This repository includes a pre-commit hook under `.githooks/pre-commit` that invokes `scripts/pre-commit.ps1`.

What the hook does (by default):
- Runs `dotnet format --verify-no-changes` (fails if formatting needed)
- Runs `dotnet build -c Debug -p:Platform=<detected>` (build + analyzers)
- Runs `dotnet test -c Debug -p:Platform=<detected>` (optional; can be commented out in the script)

How to enable the hooks (one-time per clone):

Open a PowerShell terminal at the repository root and run:

    pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\install-githooks.ps1

Or if you don't have pwsh, use Windows PowerShell:

    powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\install-githooks.ps1

This sets `git config core.hooksPath .githooks` for the current repository so Git will run the supplied hooks. You can undo by running:

    git config --unset core.hooksPath

Notes:
- `dotnet format` may need to be installed globally: `dotnet tool install -g dotnet-format` if your SDK doesn't already provide it.
- Pre-commit hooks run locally — they are intended to fail fast and prevent commits that violate formatting, build, or test expectations.
- If you prefer a faster pre-commit, edit `scripts/pre-commit.ps1` to skip tests or only run a subset of checks.

Automatic setup (recommended)
--------------------------------
To streamline onboarding, a repository setup script is provided: `scripts/setup-repo.ps1`.
Run it once after cloning to configure hooks and verify common tooling:

    pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup-repo.ps1

What `setup-repo.ps1` does:
- Sets `git config core.hooksPath .githooks` (same as `install-githooks.ps1`).
- Verifies `dotnet` is available and runs `dotnet restore`.
- Checks whether `dotnet-format` is installed globally and offers to install it if missing.
- Runs an initial `dotnet build` to surface any immediate build/analyzer issues.

The setup script is interactive by default but idempotent — safe to re-run.

