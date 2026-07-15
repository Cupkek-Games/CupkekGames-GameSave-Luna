# CupkekGames GameSave — Luna Bridge

Luna UI bindings for [CupkekGames.GameSave](https://github.com/Cupkek-Games/CupkekGames-GameSave). Generic abstract bases that wire a slot-based save manager into a Luna UI panel — subclass them in your game with concrete `TSaveData` / `TSaveMetadata` types.

## What's inside

**Runtime** (`CupkekGames.GameSave.Luna.asmdef`)

- `MainMenuView<TSaveData, TSaveMetadata>` — `UIViewComponent` base wiring Continue/Load/NewGame/Credits/Settings/Quit buttons against the last-save metadata; subclass and override `GetSaveManager()` + the `OnButton*Clicked` handlers.
- `GameSaveViewList<TSaveData, TSaveMetadata>` — `MonoBehaviour` base for a slot list with auto/manual filters, load/overwrite/delete actions, and confirmation prompts; pairs with the concrete `GameSaveView` on the same GameObject. Overwrite/delete confirms push the serialized `_confirmDest` nav destination (a global node-bound `ChoicePopupController`, parameterized per call with `ChoicePopupArgs` and awaited via `PushAsync<int>`, index 0 = confirm). `_confirmDest` is required — the view logs an error and refuses overwrite/delete when it is unset.
- `GameSaveView` — `UIViewComponent` shell with a Return button that fades the view out and destroys it.
- `GameSaveViewEntry<TSaveMetadata>` — per-row binding helper used by `GameSaveViewList`'s `ListView`.
- `AutoSaveView` — `UIViewComponent` driven by `GameSaveEvents.AutosaveStart` / `AutosaveComplete`; shows a `RadialLoading` indicator on autosave.
- `VersionLabelController` — populates a `Label#VersionLabel` with `Application.version`.

These were sample-only code in `Luna/Samples~/GameFull/Scripts/UI.WithGameSave/` until Luna v2.0.3. Promoting them out of the sample so real games can depend on them without import-on-demand fragility.

## Dependencies

- `com.cupkekgames.gamesave` (UPM)
- `com.cupkekgames.luna` (UPM)
- `com.cupkekgames.data` (UPM)
- `com.unity.inputsystem` (optional — gates `UNITY_INPUT` define for InputAction-driven actions on the save list)
