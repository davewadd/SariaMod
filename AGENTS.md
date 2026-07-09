# Repository Guidelines

## Project Structure

SariaMod is a C# tModLoader mod targeting .NET 6. Core hooks and state live in the repository root, including `SariaMod.cs`, `FairyPlayer.cs`, `FairyGlobalNPC.cs`, `FairyProjectile.cs`, and `SariaModSystem.cs`. Feature code is grouped under `Items/` by form or system; `Buffs/`, `Dusts/`, `Gores/`, `Sounds/`, `MusicChanges/`, `Tiles/`, and `Effects/` contain assets and supporting content. Multiplayer code is under `Netcode/` and `SariaMod/Netcode/`. GitHub workflows and issue templates are in `.github/`.

There is no automated test project. `TestItemRecipes.cs` and `Items/Strange/TestingStaff.cs` are debug/game content, not test suites.

## Working Rules

Work directly on the `main` branch; do not create or switch branches for repository changes. Never push to a remote unless the user explicitly asks for it. Treat the repository as the source of truth: before making an important change, inspect the relevant implementation, callers, configuration, assets, logs, and recent history. Prefer tracing behavior through code over guessing from filenames or asking the user questions that the repository can answer. Ask only when the required intent, authority, or external information cannot be established locally.

## Build and Development Commands

```powershell
dotnet restore SariaMod.csproj
dotnet build SariaMod.csproj
```

Restore dependencies before the first build; the project imports `..\tModLoader.targets`. The CI compile check runs `dotnet build SariaMod.csproj --no-restore` after preparing those targets. For behavior verification, launch the installed tModLoader client and load the mod in-game. Review `debugsaria*.txt` when diagnosing runtime issues.

## Coding Style and Naming

Use four-space indentation and preserve the surrounding style in large existing files. Use PascalCase for types and methods, camelCase for private fields and locals, and descriptive names matching the existing `Fairy*` and `Saria*` conventions. No formatter or linter is configured; `.editorconfig` disables analyzer diagnostics.

Preserve gameplay behavior, visual output, numeric values, conditions, draw order, and public/protected APIs unless the change explicitly requests otherwise. When editing the body-mask system, apply every mask type to every supported body part.

## Testing and Pull Requests

Test changes manually in tModLoader, covering the affected form, item, hook, or multiplayer path. Include reproduction steps, expected/actual behavior, and screenshots or relevant log excerpts for visual or runtime changes.

Commit subjects are short and imperative, commonly using `fix:`, `feat:`, or `refactor:` prefixes. Pull requests should summarize the change, list affected areas, report the build command used, and describe manual in-game checks. Keep secrets out of the repository; publishing credentials belong in GitHub Actions secrets. `AGENTS.md` is local guidance and must remain uncommitted.
