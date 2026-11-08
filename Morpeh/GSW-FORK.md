# Morpeh: GunshotWound2 fork

This document lists every difference between the vendored Morpeh tree and [upstream](https://github.com/scellecs/morpeh), so you can re-apply them after refreshing sources.

## Baseline

| Item | Value |
|------|-------|
| Vendored version | **[Morpeh 2023.1.1](https://github.com/scellecs/morpeh)** (`Scellecs.Morpeh` package layout under `Morpeh/Core/`) |
| Events add-on | **[Morpeh.Events 1.2.0](https://github.com/codewriter-packages/Morpeh.Events)** (`Morpeh/Core/Events/`, merged into same assembly) |
| Initial import | commit `4ed5cc0` (“Added Morpeh ECS”, 2023-07-25); upgraded in-repo to **2023.1.1** |
| Location | `Morpeh/` at repo root (not a submodule) |
| Build | standalone `Morpeh.csproj`, .NET Framework **4.8.1**, `AllowUnsafeBlocks` (default C# for this project; no `Collections/Raw` or `Native/` in compile list) |
| Newer upstream | **2024+** uses `Core/Stashes/Stash.cs` and other breaking layout/API changes |

Morpeh sources target Unity (`UnityEngine`, IL2CPP attributes) but are built as a normal class library with the mod. Logging is wired in `GunshotWound2` via `MLogger.SetInstance(new GSWMorpehLogger(...))` — **not** a Morpeh patch; no change needed on upgrade.

## Removed from vendored tree (GSW)

Not shipped in this repo (removed when trimming Morpeh for SHVDN):

| Path | Reason |
|------|--------|
| `Collections/Raw/*` | Optional native containers; unused by core or GSW |
| `Native/*` | Burst/Jobs API (`#if MORPEH_BURST`); requires Unity Burst |

`Aspect.cs` / `AspectExtensions.cs` are **kept** because `WorldExtensions.GetAspectFactory` references them (mod does not use aspects).

Unity `.meta` files are not kept in `Morpeh/`.

`Morpeh.Events` Unity package debris (`package.json`, `Tests/`, `Packages/ScriptTemplates.unitypackage`, `asmref`, `Assembly.cs` for tests) is not vendored.

### Morpeh.Events (non-Unity)

| Item | Detail |
|------|--------|
| Registration | `EventWorldPlugin` static ctor calls `AddWorldPlugin`; `WorldExtensions.Initialize` runs `RuntimeHelpers.RunClassConstructor(typeof(EventWorldPlugin))` when `!MORPEH_UNITY` (no `RuntimeInitializeOnLoadMethod` stubs in `Morpeh.cs`) |
| `Request` DEBUG checks | `UnityEngine.Time.frameCount` stub in `Morpeh.cs`; incremented at the start of each `World.Update` when `!MORPEH_UNITY` |
| Tick pipeline | GunshotWound2 registers `commonSystems` via `AddSystemsGroup`, then `ecsWorld.Update(0)` in the script ctor (while the group is still empty) so groups leave `newSystemsGroups` before gameplay; frame loop uses `ecsWorld.Update` / `LateUpdate` / `CleanupUpdate` (plugin groups, including `ProcessEventsSystem`, run after ordered world groups in each phase) |

---

## Change summary

| # | Commit | File(s) | What |
|---|--------|---------|------|
| — | `4ed5cc0` | `Morpeh/Morpeh.csproj` | .NET 4.8.1 project, explicit `Compile` list |
| 1 | `43c9c7c` | `Stash.cs`, `EntityExtensions.cs` | `AddOrGet` / `AddOrGetComponent` API |
| 2 | `3dd0736` | `Entity.cs` | Public `Entity.world` field |
| 3 | `5e53e54` | `WorldExtensions.cs` | *(obsolete in 2023.1.1)* assembly-scoped warmup — upstream removed `AppDomain` scan |
| 3b | — | `Filter.cs` | Implicit `FilterBuilder` → `Filter` (GSW: old `World.Filter.With<…>()` assignment style) |
| 4 | `e83bcee` | `Morpeh.csproj` | `MORPEH_DEBUG` in Debug configuration |
| 5 | `b9cc45d` | `Stash.cs` | Richer DEBUG messages; silent path in `AddOrGet` |
| 6 | `66dd545` | `SystemsGroupExtensions.cs` | Re-`throw` after logging in systems |
| 7 | `9439015` | `Morpeh.csproj` | Release without PDB (`DebugType=none`) |
| 8 | `8effcb0` | `*.meta` | Removed Unity `.meta`; stale `Content` entries may remain in `.csproj` — remove manually |

## GunshotWound2 dependencies

After upgrading Morpeh, confirm patches **1** and **2** are still required:

- `AddOrGet` / `AddOrGetComponent`: `HealthInitSystem`, `PainInitSystem`, `PlayerSpeedSystem`, `WoundSystem`, `TraumaSystem` (stash).
- `entity.world`: `Bleeding.cs`, `HealthFeature.cs`, `TraumaFeature.cs`.

---

## Integration: `Morpeh/Morpeh.csproj`

Upstream is a Unity package (asmdef). This repo uses a classic csproj. When replacing sources, **keep** (or recreate) the project file:

**Debug:**

```xml
<DefineConstants>DEBUG;TRACE;MORPEH_DEBUG</DefineConstants>
```

Without `MORPEH_DEBUG` in Debug, Morpeh validation and `TryCatch*` wrappers in systems are not compiled in (see patch 6).

**Release** (patch 7):

```xml
<DebugSymbols>false</DebugSymbols>
<DebugType>none</DebugType>
```

---

## Patch 1 — `AddOrGet` and `AddOrGetComponent`

**File:** `Morpeh/Core/Stash.cs` (2023.1.1: `TryAddData`, `map`, `data`, `AddTransfer(typeId, offset)`)  
Insert after `Add(Entity entity)` (before `Add(Entity entity, out bool exist)`):

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref T AddOrGet(Entity entity) {
    world.ThreadSafetyCheck();

#if MORPEH_DEBUG
    if (entity.IsNullOrDisposed()) {
        throw new Exception($"[MORPEH] You are trying Add on null or disposed entity");
    }

    var previousCapacity = this.map.capacity;
#endif
    if (this.TryAddData(entity.entityId.id, default, out var slotIndex)) {
        entity.AddTransfer(this.typeId, this.offset);
#if MORPEH_DEBUG
        if (previousCapacity != this.map.capacity) {
            world.newMetrics.stashResizes++;
        }
#endif
        return ref this.data[slotIndex];
    }

    if (this.map.TryGetIndex(entity.entityId.id, out var dataIndex)) {
        return ref this.data[dataIndex];
    }

    return ref this.empty;
}
```

**Behavior:** adds the component if missing; if it already exists, returns `ref` to the existing instance **without** logging (unlike `Add`).

**File:** `Morpeh/Core/EntityExtensions.cs`  
After `AddComponent<T>(this Entity entity)`:

```csharp
#if MORPEH_LEGACY
[Obsolete("[MORPEH] Use Stash.AddOrGet() instead.")]
#endif
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static ref T AddOrGetComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
    if (entity.IsNullOrDisposed()) {
        throw new Exception("[MORPEH] You are trying AddComponent on null or disposed entity");
    }
#endif

    return ref entity.world.GetStash<T>().AddOrGet(entity);
}
```

Upstream 2024+ does not ship `AddOrGet` — port this patch manually onto the new `Stash` (`Core/Stashes/Stash.cs`, different internals).

---

## Patch 2 — public `Entity.world`

**File:** `Morpeh/Core/Entity.cs`

```diff
-        internal World world;
+        public World world;
```

Required by mod code (`entity.world.GetStash`, `entity.world.CreateEntity()`).

---

## Patch 3b — implicit filter build (GSW)

**File:** `Morpeh/Core/Filter.cs`, inside `FilterBuilder`:

```csharp
public static implicit operator Filter(FilterBuilder builder) => builder.Build();
```

Morpeh 2023.1.1 returns `FilterBuilder` from `With` / `Without`; GunshotWound2 still assigns chains directly to `Filter`. Do not extend filters from an existing `Filter` instance (`filter.With<…>()` was removed upstream) — rebuild from `World.Filter` instead.

---

## Patch 5 — `Stash` error messages

**File:** `Morpeh/Core/Stash.cs`

1. In all DEBUG messages for “component that already exists”, include the component type:

   ```csharp
   $"You're trying to add on entity {entity.entityId.id} a component({typeof(T).FullName}) that already exists! Use Get or Set instead!"
   ```

   Applies to `Add` overloads (ref return, `Add(entity, in T value)`, etc.) — **not** `AddOrGet` (duplicate logging there was removed in `b9cc45d`).

2. In `AddOrGet`, when the component already exists, do **not** call `MLogger.LogError` — only `GetValueRefByKey`.

---

## Patch 6 — re-throw exceptions in systems

**File:** `Morpeh/Core/SystemsGroupExtensions.cs`

In `TryCatchUpdate` (`catch` block):

```csharp
catch (Exception exception) {
    systemsGroup.SystemThrowException(system, exception);
    throw;
}
```

In `TryCatchAwake` (`catch` block):

```csharp
catch (Exception exception) {
    MLogger.LogError($"Can not initialize {initializer.GetType()}");
    MLogger.LogException(exception);
    throw;
}
```

**Note:** `TryCatchUpdate` / `TryCatchAwake` are marked `[Conditional("MORPEH_DEBUG")]`. Re-throw applies to **Debug** Morpeh builds (`MORPEH_DEBUG` in csproj). In **Release**, `ForwardUpdate` runs without try/catch — exceptions behave like a direct `OnUpdate` call.

---

## Re-application order

1. Back up `Morpeh/Morpeh.csproj` (or keep a diff of integration settings).
2. Replace the `Morpeh/Core/` tree (and `Properties/` if needed) with the chosen upstream version.
3. Restore the csproj: TFM 4.8.1, `Compile` list, `MORPEH_DEBUG`, Release `DebugType=none`, remove `Content` items for missing `.meta` files.
4. Apply patches in order **2 → 3 → 1 → 5 → 6** (numbers from the summary table).
5. Build the solution and smoke-test in-game.
6. When moving to Morpeh 2024+, reconcile `Stash`, `Entity`, and `SystemsGroupExtensions` APIs — paths and signatures may differ.

## Quick recovery from git

If the baseline import in history is still `4ed5cc0`:

```powershell
cd C:\GitSources\gunshotwound2
git format-patch 4ed5cc0^..HEAD -- Morpeh/ -o patches/morpeh
```

After dropping in a fresh upstream tree, apply only the hunks that match the patches above (not every commit may apply cleanly).

## Upgrading to newer upstream

- Tags: `2023.1.1`, `2024.1.1`, … — `git fetch --tags` in a clone of [scellecs/morpeh](https://github.com/scellecs/morpeh).
- Incremental upgrade: start from a 2023-era tag, then step toward your target version.
- Jump to 2024+: copy `Scellecs.Morpeh/`, regenerate `Morpeh.csproj`, rewrite patches 1 and 5 for `Core/Stashes/Stash.cs`.
