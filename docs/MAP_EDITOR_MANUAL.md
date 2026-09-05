# Fronts of War map editor — opening guide

The repository includes one-click launchers, but not a standalone `.exe` yet.
This machine has Godot 4.7.2 Mono installed but does not have the matching
Windows export templates required to build a distributable executable.

## Fastest method

1. Open `E:\AI Projects\Games\Tower Defense` in File Explorer.
2. Double-click `Launch-MapEditor.cmd`.
3. Keep the console window open while the editor is running. It shows useful
   startup or file-operation errors.

The command launcher calls `Launch-MapEditor.ps1`, finds this checkout's
`godot-project` folder, verifies that Godot is the .NET/Mono edition, and
starts the developer-only `--map-editor` route.

## Launch from PowerShell

From any working directory, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "E:\AI Projects\Games\Tower Defense\Launch-MapEditor.ps1"
```

To force a window instead of the project's fullscreen default:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "E:\AI Projects\Games\Tower Defense\Launch-MapEditor.ps1" --windowed --resolution 1920x1080
```

The launcher currently finds the installed Mono build at:

```text
E:\Godot\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe
```

If Godot moves, pass its path explicitly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "E:\AI Projects\Games\Tower Defense\Launch-MapEditor.ps1" `
  -GodotMono "D:\Godot\Godot_v4.7.2-stable_mono_win64_console.exe"
```

## Current editor workflow

Use the **File** menu in the upper-left corner:

- **New map** creates an untitled draft.
- **Open…** loads a canonical `.tres` map from `assets/data/maps`.
- **Save** updates the current file.
- **Save As…** chooses a repository path and filename.
- **Close map** closes the document.

New, Open, and Close display Save/Discard/Cancel choices when the current map
has unsaved changes. Closing the application window does the same. Saves
validate the resource first and replace the old file only after the complete
temporary resource has been written.

Phases 2–15 provide document creation, opening, saving, schema checks, dirty
state, map rendering, catalog-backed placement, selection, transforms,
undo/redo, clipboard operations, inspector edits, validation, publishing,
planner conversion, and runtime preview. The board uses catalog sprites when
available and clear placeholders when an entry has no preview asset.

While a map is open, click an object on the board or in the hierarchy. Hold
Shift/Ctrl to add to the selection. Use the toolbar or keyboard shortcuts:

- `Ctrl+Z` / `Ctrl+Y` undo and redo.
- `Delete` removes the selection.
- `Ctrl+D` duplicates the selection one tile away.
- `Ctrl+C` / `Ctrl+V` copy and paste selected authored objects.
- Choose **Move** and drag a selected object, or use the arrow keys for one
  tile at a time. `Q` and `E` rotate the selection left or right by 90 degrees.
- Mouse wheel zooms around the cursor; middle mouse pans.

## Troubleshooting

- **“A .NET-enabled Godot executable was not found”** — install the Mono/.NET
  edition of Godot 4.7.2, then use `-GodotMono` as shown above.
- **“is not a .NET-enabled Godot build”** — the standard Godot executable
  cannot load this C# project; select the Mono download instead.
- **PowerShell script execution is disabled** — use the provided `.cmd`
  launcher, or include `-ExecutionPolicy Bypass` in the PowerShell command.
- **The normal game opens** — launch through one of the repository-root
  launchers. Opening `project.godot` and pressing Play does not add the
  required `--map-editor` flag.
- **A `.tres` file will not open** — it must be a current `MapDefinition`
  resource. Corrupt, unrelated, missing-version, and future-version resources
  are rejected with an explanatory dialog.

Release/player builds deliberately ignore `--map-editor`, and the player
export excludes the editor. Use a Debug/Developer build when export templates
become available.

## Catalog placement, validation, and runtime preview

The current workbench also includes catalog search and placement. Search the
ASSET LIBRARY, click an entry, then click an empty board cell. The placement is
tile-snapped, undoable, and stored as a catalog AssetId; press Escape to cancel
placement. PUBLISH blocks missing markers, duplicate IDs, invalid terrain,
missing catalog entries, and unapproved production art.

TEST MAP saves the current document and launches the real mission scene with
--map-id <id>. The runtime resolves that ID through MapLoader, installs authored
paths and pads, and resolves authored art slots by catalog ID. The checked-in
editor_smoke_fixture map is a small runtime fixture for this handoff.
