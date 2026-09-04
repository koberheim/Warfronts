# tools/

## Run-HeadlessChecks.ps1

Runs every automated check the repo has, in one pass, headlessly:

1. `dotnet build FrontsOfWar.csproj --no-restore`.
2. Every GoDotTest suite under `godot-project/tests/*.cs` (discovered
   automatically by scanning for `class X : TestClass` — no suite list to
   keep in sync by hand as suites are added).
3. `--validate-data` — the Data Validator's headless CLI path (GDD §19
   prompt 45 / §15.6 item 4).
4. The canonical headless smoke run
   (`--headless --fixed-fps 60 --quit-after 5400`, see `docs/DECISIONS.md`
   D46), which must print zero `error`/`exception` lines and at least one
   `[kill]` line.

It prints a pass/fail table and exits non-zero if anything failed.

### Usage

```powershell
# from anywhere; defaults resolve relative to this script and to
# $env:GODOT_MONO / docs/DECISIONS.md D13's machine-local path
powershell -File tools/Run-HeadlessChecks.ps1

# override the Godot Mono binary or project path explicitly
powershell -File tools/Run-HeadlessChecks.ps1 `
    -GodotMono "D:\Godot\Godot_v4.7.2-stable_mono_win64_console.exe" `
    -ProjectPath "D:\some\other\checkout\godot-project"
```

Requires the **.NET-enabled ("Mono")** Godot 4.7.2 build — the standard
GDScript-only Godot binary cannot load this project's C# autoloads (see
`CLAUDE.md` §7).

### Wiring it as a pre-commit hook (manual — no hook is installed by this repo)

Git looks for `.git/hooks/pre-commit` as a plain executable script (no
extension, no shebang requirement on Windows as long as it's runnable by
your git-for-windows shell). To wire this script in manually:

1. Create `.git/hooks/pre-commit` with:

   ```sh
   #!/bin/sh
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(git rev-parse --show-toplevel)/tools/Run-HeadlessChecks.ps1"
   exit $?
   ```

2. Make sure it's executable (`chmod +x .git/hooks/pre-commit` under Git
   Bash — Windows itself ignores the bit, but Git for Windows' shell honors
   it).
3. Set `$env:GODOT_MONO` in your shell profile if your Godot Mono binary
   isn't at the `docs/DECISIONS.md` D13 default path.

This is a per-checkout, manual step — deliberately not automated by this
repo (installing hooks from a script a clone just downloaded is its own can
of worms), and the full check suite (headless smoke run included) takes
long enough that some contributors may prefer running it manually before
`git push` instead of on every commit.
