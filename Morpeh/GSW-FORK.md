# Morpeh: GunshotWound2 fork

This document lists every difference between the vendored Morpeh tree and [upstream](https://github.com/scellecs/morpeh), so you can re-apply them after refreshing sources.

## Baseline

| Item | Value |
|------|-------|
| Initial import | commit `4ed5cc0` (“Added Morpeh ECS”, 2023-07-25) |
| Location | `Morpeh/` at repo root (not a submodule) |
| Build | standalone `Morpeh.csproj`, .NET Framework **4.8.1**, `AllowUnsafeBlocks` |
| Upstream (approx.) | **~2023.1.x** layout (`Morpeh/Core/*.cs`); current upstream ships as `Scellecs.Morpeh` with a different file layout |

Morpeh sources target Unity (`UnityEngine`, IL2CPP attributes) but are built as a normal class library with the mod. Logging is wired in `GunshotWound2` via `MLogger.SetInstance(new GSWMorpehLogger(...))` — **not** a Morpeh patch; no change needed on upgrade.

## Change summary

| # | Commit | File(s) | What |
|---|--------|---------|------|
| — | `4ed5cc0` | `Morpeh/Morpeh.csproj` | .NET 4.8.1 project, explicit `Compile` list |
| 1 | `43c9c7c` | `Stash.cs`, `EntityExtensions.cs` | `AddOrGet` / `AddOrGetComponent` API |
| 2 | `3dd0736` | `Entity.cs` | Public `Entity.world` field |
| 3 | `5e53e54` | `WorldExtensions.cs` | Restrict type warmup to `GunshotWound2` assembly |
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

**File:** `Morpeh/Core/Stash.cs`  
Insert after `Add(Entity entity)` (before `Add(Entity entity, out bool exist)`):

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref T AddOrGet(Entity entity) {
    world.ThreadSafetyCheck();

#if MORPEH_DEBUG
    if (entity.IsNullOrDisposed()) {
        throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity.entityId.id}");
    }
#endif
    if (this.components.Add(entity.entityId.id, default, out var slotIndex)) {
        entity.AddTransfer(this.typeId);
        return ref this.components.data[slotIndex];
    }

    return ref this.components.GetValueRefByKey(entity.entityId.id);
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

## Patch 3 — warmup scoped to the mod assembly

**File:** `Morpeh/Core/WorldExtensions.cs`, `Warmup()`, loop over `AppDomain.CurrentDomain.GetAssemblies()`:

```csharp
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
    // HACK: Collect components only for GunshotWound2 assembly
    if (assembly.GetName().Name != "GunshotWound2") {
        continue;
    }

    foreach (var type in assembly.GetTypes()) {
        // ...
```

**Why:** with multiple .NET script mods in the same app domain, scanning every assembly caused crashes. If you rename the mod assembly, update the name in this check.

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
