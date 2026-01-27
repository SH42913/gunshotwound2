## GSW2 – Technical overview (for developers)

### Tech stack

- **Language / runtime**: C# targeting **.NET Framework 4.8** (ScriptHookVDotNet-compatible)
- **Game**: Grand Theft Auto V, single-player
- **Scripting bridge**: `ScriptHookV` + `ScriptHookVDotNet` (nightly builds only)
- **ECS framework**: Embedded fork of `Morpeh` ECS (included in this repo)

### Repository / folder layout (summary)

- `GunshotWound2/` – main mod source code (systems, components, config handling, UI).
- `Morpeh/` – embedded ECS library source.
- `HookDLLs/` – ScriptHookVDotNet binaries used at runtime.
- `README.md` – player-facing description, install guide, hotkeys.
- `TECHNICAL_README.md` – this technical overview for developers.

### Solution / projects

- **`GunshotWound2`**  
  Main game script project.
  - Contains gameplay logic for wounds, bleeding, pain, armor, trauma, UI and config handling.
  - Compiles to a `.dll` loaded by ScriptHookVDotNet inside GTAV.

- **`Morpeh`**  
  ECS support library.
  - Included as a separate project in the same solution.
  - Provides ECS primitives (`World`, `Entity`, `Component`, `System` etc.) used by `GunshotWound2`.

- **`HookDLLs`**  
  Runtime hook binaries (not source).
  - `ScriptHookVDotNet.asi`
  - `ScriptHookVDotNet3.dll`

### Project overview

- **Script entry point & ECS world**
  - `GunshotWound2.GunshotWound2` is the ScriptHookVDotNet entry point. It:
    - Creates a single Morpeh `World` and a shared `SystemsGroup` for all gameplay systems.
    - Owns `SharedData`, which aggregates configs, services (`WorldService`, `CameraService`, `PedStateService`, `UIService`, `ModelCheckerService`), logging, input listeners, notifier and per-frame timing.
    - Subscribes to `Tick` / `KeyUp` and drives the ECS loop: `Update` → `LateUpdate` → `CleanupUpdate` every frame.
  - On first usable frame it loads and validates XML configs, loads localization, applies settings (hotkeys, notifications, language), then registers all feature modules.

- **Feature modules (per concern)**
  - Each gameplay area is a `*Feature` static class with a `Create` method that wires concrete systems into the shared `SystemsGroup`:
    - `PedsFeature` – discovery, conversion and lifecycle of NPC peds into ECS entities (`ConvertPedSystem`, `NpcDetectSystem`, `PedMovementSystem`, `RagdollSystem`, etc.).
    - `PlayerFeature` – player-specific systems such as detection, camera, mission tracking, speed tracking, death reporting and contextual help.
    - `HitDetection` – converts GTA damage callbacks into structured hit data (`HitDetectSystem`, `WeaponHitSystem`, `MultiBulletHitSystem`, `BodyHitSystem`, `HitCleanSystem`).
    - `WoundFeature` – interprets hit data into wound entities, using configs + `ArmorChecker` and `WoundData`.
    - `TraumaFeature` – long‑term internal damage (`TraumaSystem` and per‑body‑part `*TraumaEffect` classes).
    - `HealthFeature` – health, bleeding, self‑healing and blood FX (`HealthChangeSystem`, `BleedingSystem`, `BleedingFxSystem`, `BloodPoolFxSystem`, `SelfHealingSystem`, `TotalHealCheckSystem`).
    - `PainFeature` – pain accumulation and effects (`PainGeneratingSystem`, `PainChangeSystem`, `PainkillersItem` / `PainkillersEffect`).
    - `StatusFeature` – high‑level ped states (warning / distressed / critical / unconscious) and corresponding visuals.
    - `InventoryFeature` – inventory model and item flow (`Inventory`, `ItemTemplate`, `AddItemSystem`, `InventoryUseSystem`, `UseItemRequest`, `AddItemRequest`).
  - Most systems depend only on `SharedData` and ECS components, which keeps the game-facing GTA API usage localized in services and helpers.

- **Runtime data flow**
  - GTA events (player / ped damage, pickups, movement, etc.) are read via ScriptHookVDotNet and `WorldService`, and mapped to ECS entities:
    - `PedsFeature` and `WorldService` maintain a mapping from GTA `Ped` to ECS entity and a queue of peds to convert.
    - `HitDetection` builds `PedHitData` from raw GTA events.
  - The main pipeline is:
    1. **Hit detection**: Systems in `HitDetection` read GTA damage data into `PedHitData` and related components.
    2. **Wound creation**: `WoundFeature` chooses wound definitions (per body part, weapon, armor) from XML configs and attaches wound / bleeding / trauma requests to the affected ped entity.
    3. **Trauma, health and pain**: `TraumaFeature`, `HealthFeature` and `PainFeature` update components each frame to drain health, start / stop bleeding, accumulate pain and check thresholds for shock, unconsciousness or death.
    4. **Status & behavior**: `StatusFeature`, `PedsFeature` and `PlayerFeature` translate component state into animation changes, movement penalties, ragdoll, visual effects and UI notifications.

- **Config‑driven behavior**
  - All core behaviors are driven by XML configs under `GunshotWound2/Configs` and their `.xml` counterparts at the project root:
    - `BodyPartConfig`, `WeaponConfig`, `ArmorConfig`, `WoundConfig`, `TraumaConfig`, `PedsConfig`, `PlayerConfig`, `InventoryConfig`, `LocaleConfig`, etc.
  - `MainConfig` orchestrates loading / validation and applies global options (difficulty, language, UI policy, pause key, debug settings) to services and systems.
  - This makes wound tables, trauma effects, item availability and ped behavior tunable without recompiling.

- **Services, utils and cross‑cutting concerns**
  - `WorldService`, `PedStateService`, `CameraService`, `UIService`, `ModelCheckerService` hide direct GTA and ScriptHookVDotNet calls behind focused, testable APIs.
  - Utility classes (`GTAHelpers`, `SHVDNHelpers`, `RaycastDebugDrawer`, `MathHelpers`, `RandomExtensions`, `Localization*`, etc.) provide shared helpers for geometry, localization, debugging and logging.
  - `SharedData` is the central dependency carrier; systems receive only what they need via this object plus ECS components, which keeps system constructors thin and consistent.
  - Error handling is centralized in `GunshotWound2.HandleRuntimeException`, with an on‑disk log, on‑screen notifications and optional profiling support (`ProfilerSample`, `GSW_PROFILING`).

