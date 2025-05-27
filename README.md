I'm happy to present you the **Gunshot Wound 2**.

## What is GSW?
The GunShot Wound 2 - mod targets for realism, that brings to GTAV the most realistic damage system you ever have seen in GTA.\
It simulates pain, bleeding, organ failures, working armor, etc. Do you want to experience real firearms damage?\
Try the **GSW2**, young Luke!

**Most mods that change health or armor may be incompatible with GSW.**
~~GSW2 can work wrong if you have installed mods that changes .meta files of the game, especially if common.rpf/data/pedhealth.meta was changed, GSW2 only works well with vanilla pedhealth.meta file!
Eg, GSW2 will be compatible with Ripplers Realism if you'll not apply pedhealth.meta from RR.~~

## How it works
For the first, the GSW will find every ped around the player(you can disable it in XML-config).\
When ped(or player) has got damage, the script finds damaged body part and weapon.\
If the upper/lower body is damaged, and the ped has armor, GSW2 checks chances to penetrate armor.\
If ped doesn't have armor or armor was penetrated, the script selects type of wound with the help of the random using body part and weapon.\
Every new wound applies pain to the ped. If the total pain is more than maximal pain ped can handle, ped falls to ragdoll and stay until pain decreases(pain overwhelming simulation). 
Pain will make ped slower, change movement animation and reduce its accuracy. For the player, pain will disable and lock Special abilities and sprint. Pain decreases over time.\
Most of the wounds create bleeding. Every bleeding step by step decreases ped's health, but wounds heal by themselves over time. Player can bandage self to reduce time to heal bleeding.\
Any wound also may create trauma, like broken arm bone, punctured lungs, severed artery, etc. It will create additional pain, bleeding, and other effects.\
When ped got full health in any way - all pain, every bleeding and trauma will heal instantly. So, if you can heal all your wounds - find a med-pack or use a trainer to heal yourself. \
_Currently, GSW2 doesn't support vehicle weapons._

## Additional features
- **Support for switching peds!** You can freely switch between peds! You can feel all their wounds. It also means you
  can switch skins!
- **Bandages and SelfHealing!** You can apply a bandage to the wound, but some bleeding(eg, internal bleeding) can't be
  bandaged. If you have no bleeding, your health will slowly increase with time.
- **Helmets!** Helmets can save your head from hits and bullets with some chance. You can get a motorbike helmet using
  GetHelmet-hotkey.
- **Translations!** The GSW2 supports translations to other languages. Just add your translation
  to [GSW2Localization-table](https://docs.google.com/spreadsheets/d/1TY0nSEJMDmypkYrcVUBlMG3HIAEW075dCOtxXgW5UJ0/edit),
  and your translation will appear in next release!
- **Death from pain shock!** If your pain is more than 300%, you will die. Be carefully!

## How to install?
1) Install Microsoft .NET Framework 4.8 or later
2) Install the latest version of Script Hook V
3) Install [nightly version of Script Hook V .NET](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)
4) Download GSW2 and unpack archive
5) Put **ENTIRE GSW2 folder**(with GSW2Config.xml) to your **scripts** folder
6) Set **GSW2Config.xml** as what you want
7) Launch GTAV and try not to die quickly :)

## Default Hotkeys

- _L_ - Check yourself (+ Shift to check the closest ped)
- _K_ - Bandage yourself (+ Shift to bandage the closest ped)
- _J_ - Get/Remove Helmet
- _Delete_ - Kill yourself if you're stuck in a deadly situation
- _PageUp/PageDown_ - Increase/Decrease scan range
- _End_ - Pause/Unpause GSW
  You can change hotkeys in _GSW2Config.xml_!

## Cheat-codes

- _GSW_HEAL_ - Will instantly heal player
- _GSW_KILL_PLAYER_ - Will instantly kill player
- _GSW_TEST_PED_ - Will create ped for tests

## Recommended Mods

- [Forced First Person Aim](https://www.gta5-mods.com/scripts/forced-first-person-aim)
  if you like classical GTA third-person view, but also you like aiming from first-person view. It's a great experience!
- [Gun Recoil](https://www.gta5-mods.com/scripts/gun-recoil)
  and [Realistic Reload](https://www.gta5-mods.com/scripts/manual-reload) to make firearm gameplay realistic and challenging
- [Better Weapon Ragdoll](https://www.gta5-mods.com/scripts/better-weapon-ragdoll) to loosing your weapons from car-accident
- [Bass Dragon's ERO](https://www.gta5-mods.com/misc/bass-dragon-s-euphoria-overhaul-w-i-p-v1-0)
  or [RAGEuphoria](https://www.gta5-mods.com/misc/rageuphoria) for more realistic ped's reaction.
- [Baka's Weapons Sounds](https://www.gta5-mods.com/weapons/perui) to get best gun sound mod!
- [Realistic Blood and Decals](https://www.gta5-mods.com/misc/realistic-blood-and-decals) to get realistic wound decals
- [Pickups](https://www.gta5-mods.com/scripts/pickups) with Auto Weapon Pickup = 0 in Pickups.ini. 
  Using Pickups, you may enable CanDropWeapon option in GSW2Config.xml.
- [LSPDFR](https://www.lcpdfr.com/) if you want to change side and play as police officer

## Compatibility
### GSW2 will never be compatible with:
- **Injuries** by Zuthara, 'cause GSW2 is doing same things
- **Crawl Injury** by jedijosh920, 'cause GSW2 is doing same things

### GSW2 is compatible with some adjustments:
- **More Gore** by IAmJFry.\
  Required to set `PedHealthToPlayInjuredRagdoll`, `HealthLossRate` and `PlayerHealthLossRate` values to 0 in _More Gore Settings.xml_.

### GSW2 will be compatible in future updates(if it's possible) with:
- **Watch Your Death** by IAmJFry
- **GTA Online Respawn** by MrFoxsteil
- **Los Santos Red** by Greskrendtregk
- **Rebalance Dispatch Enhanced** by RDE Team
You may request for compatibility with other mods in [#compatibility-issues](https://discordapp.com/channels/1263842381321732147/1338065003185442857) channel in discord.

## Donations

You can use a subscription at [Patreon](https://patreon.com/SH42913) or [Boosty](https://boosty.to/sh42913)
Also you can make [one-time donate at Boosty](https://boosty.to/sh42913/donate).

## Credits

- [Morpeh](https://github.com/scellecs/morpeh)
- [Weighted Randomizer](https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp)
- Some libraries by [Leopotam](https://leopotam.com/)

## Sources of inspiration

- [Extreme Difficulty Health Realism](https://www.gta5-mods.com/scripts/sob-s-extreme-difficulty-health-realism) by sob, the ancestor of GSW
- [Crawl Injury](https://www.gta5-mods.com/scripts/crawl-injury) by jedijosh920
- [Injuries](https://www.gta5-mods.com/scripts/injuries) by Zuthara
- More Gore by IAmJFry