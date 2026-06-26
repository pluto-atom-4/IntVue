# Setup Symlinks for Unified Configuration

This document explains how to create symlinks that enable both Claude Code and Cursor to use the same rule files from the `.ai/rules/` directory.

---

## Why Symlinks?

The `.ai/rules/` directory is the **single source of truth**. Symlinks allow:
- `.claude/rules/` → `.ai/rules/` (Claude Code reads from `.ai/rules/`)
- `.cursor/rules/` → `.ai/rules/` (Cursor reads from `.ai/rules/`)
- Both tools stay in sync; edits in one location update both

---

## Setup Instructions

### Windows (PowerShell, requires admin)

Run PowerShell as Administrator and execute:

```powershell
# Create symlink: .claude/rules → .ai/rules
New-Item -ItemType SymbolicLink -Path .\.claude\rules -Target ..\.ai\rules -Force

# Create symlink: .cursor/rules → .ai/rules
New-Item -ItemType SymbolicLink -Path .\.cursor\rules -Target ..\.ai\rules -Force
```

**Verify:**
```powershell
dir .claude, .cursor | grep rules
```

You should see `rules → ..\.ai\rules` for both directories.

---

### macOS & Linux (bash)

```bash
mkdir -p .ai/rules .claude .cursor

ln -sf ../.ai/rules .claude/rules
ln -sf ../.ai/rules .cursor/rules
```

**Verify:**
```bash
ls -la .claude/rules .cursor/rules
```

You should see symlinks pointing to `../.ai/rules`.

---

## Troubleshooting

**Error: "Access Denied" or "You don't have permission"**
- Windows: Run PowerShell as Administrator. If you're not an admin, contact your IT department or use NTFS junctions instead (less recommended).
- macOS/Linux: Check you have write permissions in the project root.

**Error: "target doesn't exist"**
- Ensure `.ai/rules/` directory exists: `mkdir -p .ai/rules`

**Symlink not working in Claude Code or Cursor**
- Verify the symlink was created correctly (see Verify steps above)
- Restart Claude Code or Cursor if they were open during symlink creation
- Check that the symlink points to the correct location (relative path)

---

## What Goes in `.ai/rules/`?

Initially, `.ai/rules/` is empty. As the project evolves, it will contain:

- Project-wide rule files (copied or symlinked from `.github/instructions/`)
- Custom project-specific rules
- Shared configuration files

For now, agents will reference `.github/instructions/` directly via the Rules Router in `./CLAUDE.md`.

---

## Cross-Platform Testing

After creating symlinks, verify they work on your platform:

1. Create a test file in `.ai/rules/`: `echo "test" > .ai/rules/test.txt`
2. Check it appears in both locations:
   - Windows: `type .claude\rules\test.txt` (should show "test")
   - macOS/Linux: `cat .claude/rules/test.txt` (should show "test")
3. Clean up: `rm .ai/rules/test.txt`

---

## CI/CD Considerations

If your CI/CD pipeline runs on multiple platforms (Windows, macOS, Linux), add symlink setup to your pipeline:

```yaml
# Example GitHub Actions
- name: Setup unified AI configuration
  run: |
    mkdir -p .ai/rules .claude .cursor
    ln -sf ../.ai/rules .claude/rules
    ln -sf ../.ai/rules .cursor/rules
  shell: bash
```

This ensures all developers and CI agents use the same rule files.
