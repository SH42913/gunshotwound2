## GSW2 Configuration Guide

This document explains **all end‑user configuration files** for Gunshot Wound 2 (GSW2): what each file controls, when you should edit it, and how to do so safely.

All config files live in the `GSW2` / `GunshotWound2` folder inside your GTA V `scripts` directory once the mod is installed.

- **XML configs (gameplay & UI):**
  - `GunshotWound2.Player.xml`
  - `GunshotWound2.Peds.xml`
  - `GunshotWound2.Armor.xml`
  - `GunshotWound2.BodyParts.xml`
  - `GunshotWound2.Weapons.xml`
  - `GunshotWound2.Wounds.xml`
  - `GunshotWound2.Traumas.xml`
  - `GunshotWound2.Inventory.xml`
  - `GunshotWound2.Notifications.xml`
  - `GunshotWound2.KeyBinds.xml`
- **Localization / text:**
  - `GunshotWound2.csv`

Always **back up files before editing** (copy them somewhere else), especially `Weapons`, `Wounds` and `Traumas` which strongly affect balance.

---

## General editing tips

- **Editor**: Use a text editor that preserves UTF‑8 and XML (e.g. Notepad++, VS Code).
- **Booleans**: Values like `TRUE` / `FALSE` are not case‑sensitive.
- **Numbers**: Use a dot for decimals, e.g. `0.5`, not `0,5`.
- **Keys must match**: `Key=""` attributes are referenced across multiple files. Never change a `Key` unless you also update every reference to it.
- **Hashes & weapon names**: Weapon config uses GTA weapon names (e.g. `PISTOL`, `MICROSMG`). If you add a weapon from another mod, use the correct hash/name from that mod’s documentation.

If GSW2 stops working after an edit, restore your backup and re‑apply changes in smaller steps.

---

## `GunshotWound2.Player.xml` – player behavior and difficulty

**Purpose**: Controls how **the player** is treated by the wound system – death rules, self‑healing, pain, screen FX, and special options like slow‑motion.

**Key options (examples):**
- `UseVanillaHealthSystem`:  
  - `FALSE` – use GSW2’s wound system (recommended).  
  - `TRUE` – fall back to GTA’s default health for the **player**.
- `PoliceCanForget`: Whether cops can lose aggro when you fall unconscious.
- `PedsWillIgnoreUnconsciousPlayer`: Whether enemies stop attacking when you’re knocked out.
- `CanDropWeapon`: If `TRUE`, you may drop your weapon on arm injuries or unconsciousness.
- `HeadshotIsInstantDeath`: Make headshots kill the player immediately instead of over time.
- `RealisticSpineDamage`: Controls whether spine hits cause permanent ragdoll or just limb damage.
- `PainSlowMo`: Enables **adrenaline slow‑motion** based on pain level.
  - Negative value (default `-1`) disables this feature.
  - Positive value like `0.7` means time never slows below 70% normal speed.
- `UseScreenEffects`: Enable/disable PostFX (screen effects) for immersion.
- `PainShockThreshold`: Pain value at which you fall unconscious.
- `PainRecoverySpeed`, `BleedHealSpeed`, `SelfHealingRate`: Speeds for pain relief, natural wound closure and passive healing.
- `HelmetCost`: Money required to buy a helmet via the hotkey (see KeyBinds).
- `RampagePainMult`: Pain multiplier while in GTA’s Rampage mode.

**Typical tweaks:**
- Make the game **less punishing**:
  - Slightly increase `SelfHealingRate` and `BleedHealSpeed`.
  - Increase `PainShockThreshold` so you can withstand more pain before blacking out.
  - Set `UseScreenEffects` to `FALSE` if the visuals are too strong.
- Make the game **more hardcore**:
  - Set `HeadshotIsInstantDeath` to `TRUE`.
  - Lower `PainShockThreshold`.
  - Reduce `SelfHealingRate` and `BleedHealSpeed`.

After editing, save the file and restart GTA V to ensure changes are applied cleanly.

---

## `GunshotWound2.Peds.xml` – NPC behavior and difficulty

**Purpose**: Controls how **other peds/NPCs** are simulated: who is affected by GSW2, how quickly they die or recover, and how they move when injured.

**Key options (examples):**
- `UseVanillaHealthSystem`:  
  - `FALSE` – peds use GSW2’s wound system (recommended).  
  - `TRUE` – revert peds back to vanilla GTA health.
- `HeadshotIsInstantDeath`: Same idea as the player option, but for NPCs.
- `ShowFullHealthInfo`: Show full wound/health panel when checking peds (`TRUE`) or a simplified version (`FALSE`).
- `DontActivateRagdollFromBulletImpact`: If `TRUE`, bullets won’t trigger extra ragdoll from impacts (may override Euphoria mods).
- `RealisticSpineDamage`: Determines permanent ragdoll behavior when spine is hit.
- `ClosestPedRange`: Distance (in meters) used for “closest ped” actions (bandage, check, etc.).
- `Targets`: Relationship flags (`COMPANION`, `RESPECT`, `LIKE`, `PEDESTRIAN`, `NEUTRAL`, `DISLIKE`, `HATE`) controlling which peds GSW2 affects.
- `PainShockThreshold`: Range of pain threshold for peds.
- `CustomHealth`, `CustomAccuracy`, `CustomShootRate`:
  - Set `Min`/`Max` to `-1` to keep GTA’s default values.
  - Otherwise, peds get random values within the configured range.
- `SelfHealingRate`, `PainRecoverySpeed`, `BleedHealSpeed`: Same concepts as player, but for NPCs.
- `MaleStatusMoveSets`, `FemaleStatusMoveSets`: GTA walking animations for warning/distressed/critical states (semicolon‑separated lists).

**Typical tweaks:**
- **Performance‑oriented / lighter:** Set `UseVanillaHealthSystem` to `TRUE` while leaving the player on GSW2.
- **Increased realism for shootouts:**
  - Set `CustomHealth` to something like `Min="50" Max="100"`.
  - Lower `CustomAccuracy` and `CustomShootRate` so NPCs aren’t perfect marksmen.
- **Who is affected:** Toggle `COMPANION` and `RESPECT` to decide if friendly units (e.g. LSPDFR partners) should be fully simulated.

---

## `GunshotWound2.Armor.xml` – armor levels and helmets

**Purpose**: Defines **armor classes, coverage and helmet props** used by GSW2 when calculating armor saves and trauma.

**Key options:**
- `MinimalChanceForArmorSave`: Base chance that armor can stop a hit as long as there is at least 1 armor point.
  - Higher values = armor saves more often at low armor levels.
  - At 100 armor points, armor always saves regardless.
- `HelmetPropIndexes`: List of GTA **prop indices** that count as helmets.
  - You can add indices here to make custom helmet models work with GSW2.
- `<Levels>`:
  - Each `<Level>` defines an armor class:
    - `Key`: Internal ID (`ClassI`, `ClassII`, etc.) referenced from weapons.
    - `LocKey`: Localization key (links to `GunshotWound2.csv`).
    - `Parts`: Semicolon‑separated body parts covered (`Chest;Abdomen;LeftShoulder;...`).
    - `MaxValue`: Max armor durability for this class.
    - `ColorPrefix`: GTA color code used in notifications (e.g. `~r~` for red).
    - `TraumaPadEfficiency`: How much armor reduces blunt trauma when it does not fully stop the hit.

**Typical tweaks:**
- Increase `MaxValue` for higher survivability (e.g. heavier armor).
- Adjust `TraumaPadEfficiency` if you want armor to reduce more or less trauma.
- Add new armor classes by copying an existing `<Level>` and changing `Key`, `LocKey`, `Parts`, `MaxValue`.
  - Remember to reference the new class from `GunshotWound2.Weapons.xml` (e.g. `SafeArmorLevel="ClassVHeavy"`).

---

## `GunshotWound2.BodyParts.xml` – hitboxes and trauma tables

**Purpose**: Defines **body regions**, which GTA bones they include, how much damage/bleeding/pain they take, and which traumas are likely for each type of impact.

**Key concepts:**
- `<BodyPart>` attributes:
  - `Key`: Internal ID (`Head`, `Neck`, `Chest`, `Abdomen`, `Pelvis`, etc.).
  - `LocKey`: For in‑game text (links to `GunshotWound2.csv`).
  - `Bones`: Semicolon‑separated GTA bone names forming the hit area.
  - `TangentialWoundThreshold`: How “grazing” a hit must be to count as tangential.
  - `DamageMult`, `BleedMult`, `PainMult`: Multipliers applied to all wounds in this part.
  - `TraumaChance`: Base chance to apply trauma when a wound is created.
  - `IgnoreWeaponTraumaChanceForPenetratingWounds`: If `TRUE`, penetrating wounds always use body‑part trauma weights instead of weapon trauma chance.
- `<BluntTraumas>` / `<PenetratingTraumas>`:
  - Lists of `<Trauma Key="..." Weight="..."/>` that reference `GunshotWound2.Traumas.xml`.
  - `Weight` controls randomness – higher weights = more likely trauma.

**Typical tweaks:**
- Increase `PainMult` or `BleedMult` for specific parts to emphasize their importance (e.g. more deadly pelvis shots).
- Adjust trauma weights to make certain injuries more or less common.
- Advanced use: Add new `BodyPart` entries for custom hit groups (requires careful bone selection).

---

## `GunshotWound2.Weapons.xml` – weapon categories and damage models

**Purpose**: Maps **GTA weapons** to realistic ballistic profiles: damage, bleeding, pain, armor interaction and wound types.

**Top‑level options:**
- `SpecialStunDamage`:
  - `Enabled`: If `TRUE`, stun damage (like Taser) always causes unconsciousness based on `StunPainPercent`.
  - `StunPainPercent`: Pain applied relative to max pain (e.g. `1.15` = 115%).
- `CleanLastDamageFromPed`:
  - Compatibility flag for other damage‑tracking mods (`MoreGore`, etc.).
  - Leave `TRUE` unless another mod requires it off.
- `<IgnoreSet>`:
  - Groups of non‑standard damage sources the mod **ignores** (explosions, gas, sharks, etc.).
  - Each child (`Explosives`, `Projectiles`, `Launchers`, etc.) has a `Hashes` attribute with semicolon‑separated names/hashes.

**Weapon entries:**
- Each weapon profile is an element like `<Firearm9x19_HP>`, `<Firearm12GA_00Buck>`, etc., with attributes:
  - `ShortDesc`: In‑game short text for ammo type.
  - `DamageMult`, `BleedMult`, `PainMult`: Scaling for all wounds generated by that ammo.
  - `ChanceToCauseTrauma`: Base chance to apply a trauma from `GunshotWound2.Traumas.xml`.
  - `SafeArmorLevel`: Highest armor level that can reliably stop this ammo (must match a `Key` in `GunshotWound2.Armor.xml`).
  - `ArmorDamage`: How much armor durability is removed on hit.
  - `HelmetSafeChance`: Probability that a helmet saves the head.
- Child elements:
  - Weapon mappings like `<Pistols Hashes="PISTOL;SNSPISTOL;..."/>`, `<SMGs .../>`, `<Shotguns .../>`, `<Rifles .../>`, etc.
  - `<TakedownWound Key="..."/>`: Wound applied when a “takedown” hit happens.
  - `<TangentialWound Key="..."/>`: Wound for grazing hits.
  - `<Wounds>` list:
    - `<Wound Key="LaceratingGsw" Weight="50"/>` etc., referencing keys in `GunshotWound2.Wounds.xml`.

**Adding support for a new weapon (from another mod):**
1. Identify the weapon’s hash/name (from the weapon mod).
2. Decide which existing ammo profile best matches (e.g. another 9x19 pistol).
3. Add the weapon’s hash to the appropriate group:
   - Example: add `MYMOD_PISTOL` into an existing `<Pistols Hashes="...;MYMOD_PISTOL;"/>`.
4. If the ammo is unique, duplicate a similar element, change:
   - Element name (`<FirearmMyCustomAmmo>`).
   - `ShortDesc`, `DamageMult`, `BleedMult`, `PainMult`, `ChanceToCauseTrauma`, `SafeArmorLevel`, `ArmorDamage`, `HelmetSafeChance`.
   - Wound weights if desired.
5. Keep all keys used in `Wounds` and `TakedownWound` synchronized with `GunshotWound2.Wounds.xml`.

---

## `GunshotWound2.Wounds.xml` – wound types and global multipliers

**Purpose**: Defines **base wound templates** (bruises, cuts, gunshot types) and global multipliers that apply to every wound.

**Global settings:**
- `<GlobalMultipliers DamageMult="" BleedMult="" PainMult=""/>`:
  - Multiply all wound damage/bleed/pain values.
  - Example: `BleedMult="2.0"` doubles bleeding from all wounds.
- `<GlobalDeviations Damage="" Bleed="" Pain=""/>`:
  - Adds random variation ±X% to each new wound.
  - Example: `Damage="0.2"` => ±20% randomization.
- `<Takedown .../>`:
  - Defines wound behavior for heavy melee takedowns (ragdoll duration and multipliers).
- `UseCustomUnconsciousBehaviour`:
  - When `TRUE`, peds use varied unconscious animations instead of simple ragdoll.
- `DelayedPainPercent`, `DelayedPainSpeed`:
  - Control how much pain is delayed (adrenaline) and how fast it returns.
- `PainfulWoundPercent`, `RagdollOnPainfulWound`:
  - Determine when a wound is considered painful enough to trigger short ragdoll.

**Wound templates:**
- Each `<Wound>` defines:
  - `Key`: Internal ID (used by body parts and weapons).
  - `LocKey`: Text key for notifications.
  - `IsBlunt`: Whether it is blunt trauma (vs cutting/penetrating).
  - `CanCauseTrauma`: If `TRUE`, can trigger traumas from `Traumas.xml`.
  - `Damage`, `Pain`, `Bleed`: Base values before multipliers.

Here you can globally **rebalance** how dangerous certain wound types are (e.g. increase bleeding from `AvulsiveGsw`, reduce pain from bruises, etc.).

---

## `GunshotWound2.Traumas.xml` – specific internal injuries

**Purpose**: Defines **named traumas** (fractures, organ damage, arterial bleeds) and their damage/pain/bleeding behavior.

**Trauma entries:**
- Each `<Trauma>` includes:
  - `Key`: Internal ID used by `BodyParts.xml` (`Trauma Key="..." Weight="..."`).
  - `LocKey`: Text key referencing `GunshotWound2.csv`.
  - `Damage`, `Pain`, `Bleed`: Per‑second (or per‑tick) effects.
  - Optional `Effect`: High‑level effect category (`Head`, `Spine`, `Heart`, `Lungs`, `Abdomen`, `Legs`, `Arms`, etc.).
  - Optional `EffectMessage`: Whether a dedicated message is shown (`TRUE`/`FALSE`).
  - Optional `PainRateWhenMoving`, `PainRateWhenRunning`, `PainRateWhenAiming`: Additional pain gained based on activity.

**Usage:**
- `BodyParts.xml` references these trauma keys under `<BluntTraumas>` and `<PenetratingTraumas>`.
- `Weapons.xml` can also indirectly cause trauma via `ChanceToCauseTrauma` and `CanCauseTrauma` on wounds.

You can:
- Increase or decrease lethality of specific injuries (e.g. `HeartInjury`, `GreatVesselBleed`).
- Tune how much certain traumas hurt while moving vs standing still.

Avoid renaming `Key` values unless you update every reference in `BodyParts.xml`.

---

## `GunshotWound2.Inventory.xml` – medical items and loadouts

**Purpose**: Controls **how medical items are given and consumed**, and how painkillers behave.

**Key options:**
- `<BlipsToMedkits>`:
  - `Value`: Enable/disable map blips to medkits.
  - `RefreshTime`: How often the game scans for medkits around the player.
  - `ModelName`: GTA model name used to identify medkits.
- `DontSpendMedicalItemsInEmergencyVehicles`:
  - When `TRUE`, using bandages/painkillers inside emergency vehicles does not consume them (convenient for LSPDFR).
- `<Painkillers Rate="" Duration=""/>`:
  - `Rate`: Pain points removed per second.
  - `Duration`: How long the effect lasts (seconds).
- `<DefaultLoadout>`:
  - Items all characters receive when they gain an inventory (`Bandages`, `Painkillers`, etc.).
- `<MedkitLoadout>`:
  - Items you gain when you pick up a medkit.
- `<EmergencyVehicleLoadout>`:
  - Items granted the first time you enter an emergency vehicle.

You can customize **how generous** the system is with medical supplies and how strong painkillers feel.

---

## `GunshotWound2.Notifications.xml` – language and UI messages

**Purpose**: Configures **language**, helper tips and which notification categories are shown.

**Key options:**
- `<Language Value="EN"/>`:
  - Supported codes: `EN,RU,DE,FR,PL,KR,PT-BR,SPA,SWE,JP,ZH-HK,ZN-CN,FA` (and potential future updates).
  - This both picks a language and must match a column in `GunshotWound2.csv`.
- `<HelpTips Enabled="" TipDurationInSec="" MinIntervalInSec="" MaxIntervalInSec=""/>`:
  - Toggle contextual helper tips and how often they appear.
- `<HitNotification Enabled="TRUE"/>`: Show hit markers (body part + wound).
- `<Info Value="TRUE"/>`: Info notifications like “Totally healed”, “Picked up items”, etc.
- `<OtherPeds Value="TRUE"/>`: Messages about other peds (e.g. “He coughs up blood”).
- `<Wounds Value="TRUE"/>`: Player’s wound notifications.
- `<Critical Value="TRUE"/>`: Important messages about serious injuries, armor penetration, etc.

If you want a **minimal HUD/UI**, you can set some of these flags to `FALSE` while keeping the simulation intact.

---

## `GunshotWound2.KeyBinds.xml` – hotkeys

**Purpose**: Defines all in‑game **keyboard shortcuts** used by GSW2.

**How it works:**
- Each entry corresponds to an action:
  - `<CheckSelfKey>`, `<CheckClosestKey>`, `<BandagesSelfKey>`, `<BandagesClosestKey>`,
    `<PainkillersSelfKey>`, `<PainkillersClosestKey>`, `<DeathKey>`, `<GetHelmetKey>`, `<PauseKey>`.
- Attributes:
  - `KeyCode`: Keyboard key (e.g. `L`, `K`, `J`, `Delete`, etc.).
  - `Modifiers`: Optional `Shift`, `Alt`, `Control` (can be combined with `;` if needed).

**Examples (defaults):**
- `CheckSelfKey` – `L`
- `CheckClosestKey` – `L` + `Shift`
- `BandagesSelfKey` – `K`
- `BandagesClosestKey` – `K` + `Shift`
- `PainkillersSelfKey` – `J`
- `PainkillersClosestKey` – `J` + `Shift`
- `GetHelmetKey` – `L` + `Alt`
- `DeathKey` – `Delete`
- `PauseKey` – `End`

To **disable** a key, you can remove its `KeyCode` or the whole element (see comment in the file).  
Always make sure new bindings do not conflict with other mods that also use ScriptHookVDotNet hotkeys.

---

## `GunshotWound2.csv` – localization table

**Purpose**: Stores **all texts and translations** used by GSW2: notification messages, body part names, armor level descriptions, trauma/wound texts, etc.

**Structure:**
- First row defines columns: `KEY,EN,RU,DE,FR,PL,KR,PT-BR,SPA,SWE,JP,ZH-HK,ZN-CN,FA,FI`.
- Every subsequent row has:
  - `KEY`: Symbolic name used by XML files (e.g. `ArmorClassI`, `HelmetProtectedYou`, `BodyParts.Head`).
  - One cell per language with the translated text.

**How it connects to XML:**
- XML configs use `LocKey` attributes (e.g. `LocKey="ArmorClassI"`, `LocKey="Wounds.Gsw.Grazing"`).
- `GunshotWound2.Notifications.xml` selects which language column to display via `<Language Value="..."/>`.

**Editing recommendations:**
- You can:
  - Fix typos or phrasing in your language.
  - Add translations to languages you speak.
  - Create your own language column (e.g. `IT`) and switch `Language` to that code (if you add it consistently).
- Do **not**:
  - Change the `KEY` column unless you update all `LocKey`/`LocText` references in XML.
  - Remove rows that are still referenced from configs.

For major translation contributions, you can also use the shared spreadsheet referenced in `README.md`.

---

## Safe workflow for customizing GSW2

1. **Back up** all XML and the CSV before making any changes.
2. Change **one area at a time**:
   - Difficulty → `Player`, `Peds`, `Wounds`, `Traumas`.
   - HUD / messages → `Notifications`, `KeyBinds`, `GunshotWound2.csv`.
   - Ballistics / armor → `Weapons`, `Armor`, `BodyParts`.
   - Medical items → `Inventory`.
3. **Test in‑game**:
   - Load a save, fire a few weapons, observe wounds, traumas, and notifications.
   - If something feels off, revert or adjust gradually.
4. If the mod fails to start or behaves strangely:
   - Restore the last known‑good backup.
   - Re‑apply your edits in smaller steps to find the problematic change.

With these files you can completely tune GSW2’s realism, difficulty, and presentation without recompiling any code.

