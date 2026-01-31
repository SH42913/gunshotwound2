**Gunshot Wound 2 (GSW2)** is a realism‑focused damage system for single‑player GTA V.

It simulates **pain, bleeding, trauma, armor penetration and medical treatment** for both the player and NPCs, turning every firefight into a tactical survival scenario instead of a simple health bar race.

---

## What is GSW2?

GSW2 replaces the vanilla GTA V health model with a **wound‑based simulation**:

- **Per‑body‑part damage** – shots to the head, chest, arms, legs and organs are handled differently.
- **Pain system** – pain slows you down, affects accuracy, movement and can knock you into ragdoll.
- **Bleeding and bandaging** – wounds bleed over time; you can bandage or bleed out.
- **Trauma effects** – broken bones, punctured lungs, severed arteries, etc. with unique gameplay consequences.
- **Working armor & helmets** – armor and helmets can stop or reduce damage based on configurable chances.
- **Status & shock** – high pain and severe trauma can leave you barely functional until you treat your wounds or take painkillers.

If you want **grounded, punishing gunplay** that rewards good tactics and medical management, this mod is for you.

---

## Installation

1. **Install prerequisites**
   - Install **Script Hook V**.
   - Install the **nightly Script Hook V .NET** build.
   - Confirm GTAV launches and ScriptHookVDotNet scripts run.
2. **Download GSW2**
   - Grab the latest GSW2 release archive.
3. **Extract the mod**
   - Unpack the archive.
4. **Copy the mod folder**
   - Place the **entire `GSW2` folder** into your GTA V `scripts` folder.  
     If you don’t have a `scripts` folder yet, create one in your GTAV root directory.
5. **Configure the mod**
   - Open the `GunshotWound2.*.xml` files (for example `GunshotWound2.KeyBinds.xml`) with a text editor.
   - For a detailed explanation of every config file and how they interact, see `CONTENT_README.md` in the same folder.
6. **Launch the game**
   - Start GTAV single‑player.
   - If installed correctly, GSW2 will initialize automatically in the background.
   - You'll see greeting notification when GSW2 will ready to play.

If the mod doesn’t seem to start, double‑check ScriptHookVDotNet is installed (nightly), and verify there are no startup errors in your ScriptHookVDotNet logs.

---

## Gameplay overview

At a high level, here is what GSW2 does in game:

- Tracks **all nearby peds**.
- Intercepts **damage events** for the player and NPCs.
- Determines **hit location + weapon**, then rolls for **armor / helmet interaction**.
- Creates **wounds** that apply:
  - Pain (movement penalties, aim penalties, ragdoll, special ability lockout).
  - Bleeding (gradual health loss until healed or stabilized).
  - Trauma (internal damage, organ failures, critical states).
- Wounds **heal over time**, especially if bandaged, but severe traumas may still be lethal.
- When a ped is fully healed (by med‑packs, trainers, etc.), all active pain, bleedings and traumas are cleared.

Medical Treatment:
- **Bandages:** Essential for stabilization. Applying a bandage reduces the bleeding intensity by 50% and halves the remaining time required for the wound to heal completely.
- **Painkillers:** High-tier combat utility. They increase the speed of pain recovery, dampen the impact of incoming pain, and provide a critical window where you can **temporarily ignore unconsciousness caused by pain**, keeping you in the fight longer.

Currently, GSW2 focuses on **ped and player weapon damage** and **does not handle vehicle weapons**.

---

## Default controls

These are the **default** hotkeys (they are defined in `GunshotWound2.KeyBinds.xml` and can be changed there):

- **L** – Check yourself  
  - **Shift + L** – Check the closest ped.
- **K** – Bandage yourself  
  - **Shift + K** – Bandage the closest ped.
- **J** – Use painkillers on yourself  
  - **Shift + J** – Use painkillers on the closest ped.
- **Alt + L** – Get / remove helmet.
- **Delete** – Kill yourself (for when you are stuck in an unrecoverable state).
- **End** – Pause / unpause GSW2.

If a hotkey conflicts with another mod, change it directly in `GunshotWound2.KeyBinds.xml`.

---

## Recommended mods

These are optional but pair well with GSW2’s gameplay:

- **Forced First Person Aim** – Keep classic third‑person movement while aiming in first‑person.
- **Gun Recoil** and **Realistic Reload** – More grounded weapon handling and reload behavior.
- **Better Weapon Ragdoll** – Better reactions to crashes and impacts.
- **Euphoria overhauls** (e.g. Bass Dragon’s ERO, RAGEuphoria) – More realistic reactions to being shot.
- **Realistic Blood and Decals** – More believable wound visuals.
- **Pickups** (with `Auto Weapon Pickup = 0`) – Works well with GSW2’s `CanDropWeapon` option in player's config.
- **LSPDFR** – If you want to experience GSW2 from the perspective of law enforcement.

Always read each mod’s documentation and configure them to avoid overlapping health / injury systems.

---

## Debug build / DEBUG version

- **Who should use it**: The DEBUG build is intended for developers and users who are debugging issues or working on GSW2 itself. Regular players should stick to the normal release build.
- **What it does**: DEBUG builds enable extra logging, on‑screen diagnostic messages and some additional validation checks. This helps track down configuration problems or unexpected behavior, but may slightly reduce performance and clutter the screen / logs.
- **Where to get it**: If a DEBUG package is provided in a release, it will be clearly marked in the download name (for example `GSW2_DEBUG.zip`) or in the release notes. Otherwise, you can compile it yourself from source using the `DEBUG` configuration in the Visual Studio solution.
- **How to use it**: Install the DEBUG build in the same way as the normal release.

---

## Support and community

- **Bug reports** – Use the [issues section at GitHub](https://github.com/SH42913/gunshotwound2/issues).
- **Feedback** - Use the [official GSW2 Discord](https://discord.gg/NSsw7cYhUR).
- **Translations** – Contribute to the [localization table](https://docs.google.com/spreadsheets/d/1TY0nSEJMDmypkYrcVUBlMG3HIAEW075dCOtxXgW5UJ0/edit) and your language may be added in a future release.

---

## Donations

If you enjoy GSW2 and want to support further development:

- Monthly support: [Patreon](https://patreon.com/SH42913) or [Boosty](https://boosty.to/sh42913)
- One‑time support: [Boosty one‑time donation](https://boosty.to/sh42913/donate) or [BuyMeACoffee](https://buymeacoffee.com/sh42913)

<details>
  <summary>All GSW supporters</summary>

- KamilStoch
- Jerry Schell
- Curtis Lobley
- FlyingIceWizard
- kuact
- wu huang
- volc
- Levi
- 明 黎
- Crese1924
- Byron Xu
- Brock
- WhyisMako
- realsubo2
</details>

Thank you for helping keep the project alive.

---

## Credits

- **ECS / architecture**
  - [Morpeh](https://github.com/scellecs/morpeh) – embedded ECS framework.
  - Libraries and utilities by [Leopotam](https://leopotam.com/).
- **Utilities**
  - [Weighted Randomizer](https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp)
- **Inspiration**
  - [Extreme Difficulty Health Realism](https://www.gta5-mods.com/scripts/sob-s-extreme-difficulty-health-realism) by sob (ancestor of GSW).
  - [Injuries](https://www.gta5-mods.com/scripts/injuries) by Zuthara.
  - `More Gore` by IAmJFry.

## Special thanks
- Pleb Masters team for their awesome [Game Data site](https://forge.plebmasters.de/)

And thanks to everyone in the community who tests, reports bugs, suggests features and contributes translations.