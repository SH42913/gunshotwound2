I'm happy to present you **Gunshot Wound 2**.

## What is GSW?
The GunShot Wound 2 - mod targets for realism, that brings to GTAV the most realistic damage system you ever have seen in GTA.\
It simulates pain, bleedings, organ failures, working armor, etc. Do you wanna real firearms experience?\
Try the **GSW2**, young Luke!

**Most mods that changes health or armor will be incompatible with GSW.**
~~GSW2 can work wrong if you have installed mods that changes .meta files of the game, especially if common.rpf/data/pedhealth.meta was changed, GSW2 only works well with vanilla pedhealth.meta file!
Eg, GSW2 will be compatible with Ripplers Realism if you'll not apply pedhealth.meta from RR.~~

## How it works
For the first, the GSW will find every ped around the player(you can disable it in XML-config).\
When ped(or player) have got damage, the script finds damaged body part and weapon.\
If the upper/lower body is damaged, and the ped has armor, GSW2 checks chances to penetrate armor.\
If ped doesn't have armor or armor was penetrated, the script selects type of wound with help of the random using body part and weapon.\
Every new wound applies pain to the ped. If total pain is more than maximal pain ped can handle, ped falls to ragdoll and stay until pain decreases(pain overwhelming simulation). 
Pain decreases over time. Pain will make ped slower, change movement animation and reduce its accuracy. For player pain will disable and lock Special abilities and sprint.\
Most of wounds create bleeding. Every bleeding step by step decreases ped's health, but wounds heal by themselves over time. Player can bandage self to reduce time to heal bleeding.\
Any wound also can create critical damage, like broken arm bone, punctured lungs, severed artery, etc. It will create additional pain, bleeding, and other effects.\
If ped got full health in any way, all pain, every bleedings and crits will heal instantly. So if you can heal all your wounds - find a med-pack or use a trainer to heal yourself.\
_Currently, GSW2 doesn't support vehicle weapons._

## Other features
- **Support for switching peds!** You can freely switch between peds! You can feel all their wounds. It also means you can switch skins!
- **Bandages and SelfHealing!** You can apply a bandage for $15 to the wound using K by default, but some bleedings(eg, internal bleedings) can't be bandaged. If you have no bleeding, your health will slowly increase with time. It's the only way to survive in this dangerous world!
- **Motorbike Helmets!** Helmets can save your head from hits and bullets with some chance. You can get a helmet for $40 with helmet-hotkey, by default H (changeable in XML-config).
- **Translations!** The GSW2 supports translations to other languages. Just add your translation to [GSW2Localization-table](https://docs.google.com/spreadsheets/d/1TY0nSEJMDmypkYrcVUBlMG3HIAEW075dCOtxXgW5UJ0/edit) and your translation will appear in next release!
- **Death from pain shock!** If ped's pain is more than 300%, ped will die. Be carefully!

## How to install?
1) Install Microsoft .NET Framework 4.8 or later
2) Install latest versions of ScriptHookV and ScriptHookVDotNet
3) Download and unpack archive
4) Put **ENTIRE GSW2 folder** to your **scripts** folder
5) Set **GSW2Config.xml** as what you want
6) Launch GTAV and try not to die quickly :)

## Default Hotkeys
- _L_ - Check yourself
- _J_ - Get/Remove Helmet
- _K_ - Apply Bandage
- _PageUp/PageDown_ - Increase/Reduce WoundedPed Range
- _End_ - Pause/Unpause GSW
You can change hotkeys in GSW2Config.xml!

## Recommended Mods
- <a href="https://www.gta5-mods.com/scripts/forced-first-person-aim"><b>Forced First Person Aim</b></a> if you like classical GTA third-person view, but also you like aiming from first-person. It's a great experience!
- <a href="https://gta5-mods.com/scripts/gun-recoil"><b>Gun Recoil</b></a> and <a href="https://www.gta5-mods.com/scripts/manual-reload"><b>Realistic Reload</b></a> to make firearm gameplay realistic and challenging.
- <a href="https://gta5-mods.com/scripts/better-weapon-ragdoll"><b>Better Weapon Ragdoll</b></a> to loosing your weapon from car-accident.
- <a href="https://ru.gta5-mods.com/misc/bass-dragon-s-euphoria-overhaul-w-i-p-v1-0"><b>Bass Dragon's ERO</b></a> or <a href="https://www.gta5-mods.com/misc/rageuphoria"><b>RAGEuphoria</b></a> for more realistic ped's reaction.
- <a href="https://www.gta5-mods.com/weapons/perui"><b>Baka's Weapons Sounds</b></a> to get best gun sound mod!
- <a href="https://www.gta5-mods.com/misc/realistic-blood-and-decals"><b>Realistic Blood and Decals</b></a> to get realistic wound decals.
- <a href="https://www.gta5-mods.com/scripts/pickups"><b>Pickups</b></a> with Auto Weapon Pickup = 0 in Pickups.ini.

## Donations
You can [Buy Me A Coffee](https://www.buymeacoffee.com/SH42913) :)