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
Any wound also may create critical damage, like broken arm bone, punctured lungs, severed artery, etc. It will create additional pain, bleeding, and other effects.\
When ped got full health in any way - all pain, every bleeding and crits will heal instantly. So, if you can heal all your wounds - find a med-pack or use a trainer to heal yourself. \
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

- <a href="https://www.gta5-mods.com/scripts/forced-first-person-aim"><b>Forced First Person Aim</b></a> if you like
  classical GTA third-person view, but also you like aiming from first-person. It's a great experience!
- <a href="https://www.gta5-mods.com/scripts/gun-recoil"><b>Gun Recoil</b></a>
  and <a href="https://www.gta5-mods.com/scripts/manual-reload"><b>Realistic Reload</b></a> to make firearm gameplay
  realistic and challenging.
- <a href="https://www.gta5-mods.com/scripts/better-weapon-ragdoll"><b>Better Weapon Ragdoll</b></a> to loosing your
  weapon from car-accident.
- <a href="https://www.gta5-mods.com/misc/bass-dragon-s-euphoria-overhaul-w-i-p-v1-0"><b>Bass Dragon's ERO</b></a>
  or <a href="https://www.gta5-mods.com/misc/rageuphoria"><b>RAGEuphoria</b></a> for more realistic ped's reaction.
- <a href="https://www.gta5-mods.com/weapons/perui"><b>Baka's Weapons Sounds</b></a> to get best gun sound mod!
- <a href="https://www.gta5-mods.com/misc/realistic-blood-and-decals"><b>Realistic Blood and Decals</b></a> to get
  realistic wound decals.
- <a href="https://www.gta5-mods.com/scripts/pickups"><b>Pickups</b></a> with Auto Weapon Pickup = 0 in Pickups.ini.
  Using Pickups, you may enable CanDropWeapon option in GSW2Config.xml.

## Donations

You can [Buy Me A Coffee](https://www.buymeacoffee.com/SH42913) :)

## Credits

- [Morpeh](https://github.com/scellecs/morpeh)
- [Weighted Randomizer](https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp)
- Some libraries by [Leopotam](https://leopotam.com/)