# Repository Guidelines

## Project Structure & Module Organization

This repository is a Unity 2D project, built with Unity `6000.3.10f1` and URP. Game code lives in `Assets/Scripts/`; scenes are in `Assets/Scenes/`; prefabs are in `Assets/PreFabs/`. Art, sprites, animations, and audio are organized under `Assets/Arts`, `Assets/Sprites`, `Assets/Animations`, and `Assets/Audio`. Unity settings and packages are managed in `ProjectSettings/` and `Packages/`. No dedicated test directory or assembly definitions are currently present.

## Build, Test, and Development

Open the project with Unity `6000.3.10f1` (Unity Hub is suitable), then run scenes from the Editor. Use **File > Build Profiles** to configure and build a player; check `ProjectSettings/EditorBuildSettings.asset` for enabled scenes. Run available checks from **Window > General > Test Runner**. The Test Framework package is installed, but no repository test suite was found, so do not assume tests exist. Avoid committing generated `Library/`, `Temp/`, or `Logs/` content.

## Coding Style & Naming

Use C# with four-space indentation and one type per `.cs` file; keep the filename aligned with its main type (for example, `Player.cs` and `Player`). Unity components generally use PascalCase type and public method names, with serialized scene references exposed through Inspector fields. Preserve existing Spanish identifiers and UI text when changing related code. Keep scene references and prefab assignments consistent in the Unity Inspector, and retain Unity `.meta` files when adding or moving assets.

## Testing Guidelines

For gameplay changes, open the affected scene in the Editor and verify the relevant flow, including save/continue behavior where applicable. Check the Console for compile errors and missing serialized references. Add automated tests under an `Assets/Tests/` directory only when introducing a test suite, and document how to run them.

## Commits & Pull Requests

Git history was unavailable in this checkout, so no repository-specific commit convention could be confirmed. Use concise imperative commit subjects, such as `Add pause handling to custom level`. Pull requests should describe the behavior change, list affected scenes or assets, include reproduction and verification steps, and attach screenshots or a short recording for visible UI/gameplay changes. Call out any PlayerPrefs or Supabase configuration impact.

## Configuration & Secrets

Do not commit credentials or private backend keys. Review Supabase and Unity Services configuration before sharing builds, and keep machine-specific settings and generated Unity files out of commits.
