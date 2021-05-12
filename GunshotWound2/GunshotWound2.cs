﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using GTA;
using GTA.Native;
using GTA.UI;
using GunshotWound2.Configs;
using GunshotWound2.Crits;
using GunshotWound2.Damage;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.Healing;
using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using GunshotWound2.Pain;
using GunshotWound2.Player;
using GunshotWound2.Utils;
using GunshotWound2.World;
using Leopotam.Ecs;
using Screen = GTA.UI.Screen;

namespace GunshotWound2
{
    public sealed class GunshotWound2 : Script
    {
        public static string LastSystem = "Nothing";

        public const float MINIMAL_RANGE_FOR_WOUNDED_PEDS = 0;
        private const float ADDING_TO_REMOVING_MULTIPLIER = 2;
        private bool _isPaused;

        private EcsWorld _ecsWorld;
        private EcsSystems _everyFrameSystems;
        private MultiTickEcsSystems _commonSystems;

        private MainConfig _mainConfig;
        private LocaleConfig _localeConfig;
        private GswWorld _gswWorld;

        private bool _isInit;
        private bool _configLoaded;
        private string _configReason;
        private bool _localizationLoaded;
        private string _localizationReason;
        private bool _exceptionInRuntime;
        private int _ticks;

        public GunshotWound2()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            Function.Call(Hash._SET_CAM_EFFECT, 0);
            GunshotWoundInit();
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            if (_mainConfig.HelmetKey != null && eventArgs.KeyCode == _mainConfig.HelmetKey)
            {
                _ecsWorld.CreateEntityWith<AddHelmetToPlayerEvent>();
                return;
            }

            if (_mainConfig.CheckKey != null && eventArgs.KeyCode == _mainConfig.CheckKey)
            {
                CheckPlayer();
                return;
            }

            if (_mainConfig.HealKey != null && eventArgs.KeyCode == _mainConfig.HealKey)
            {
                HealPlayer();
                return;
            }

            if (_mainConfig.BandageKey != null && eventArgs.KeyCode == _mainConfig.BandageKey)
            {
                ApplyBandageToPlayer();
                return;
            }

            if (_mainConfig.IncreaseRangeKey != null && eventArgs.KeyCode == _mainConfig.IncreaseRangeKey)
            {
                IncreaseRange(5);
                return;
            }

            if (_mainConfig.ReduceRangeKey != null && eventArgs.KeyCode == _mainConfig.ReduceRangeKey)
            {
                ReduceRange(5);
                return;
            }

            if (_mainConfig.PauseKey != null && eventArgs.KeyCode == _mainConfig.PauseKey)
            {
                _isPaused = !_isPaused;
                Notification.Show(_isPaused
                    ? $"~r~{_localeConfig.GswIsPaused}"
                    : $"~g~{_localeConfig.GswIsWorking}");
                return;
            }
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            if (!_isInit && _ticks++ == 400)
            {
                string translationAuthor = string.IsNullOrEmpty(_localeConfig.LocalizationAuthor)
                    ? "GSW2-community"
                    : _localeConfig.LocalizationAuthor;
                Notification.Show(!_exceptionInRuntime
                    ? $"{_localeConfig.ThanksForUsing}\n~g~GunShot Wound ~r~2~s~\nby SH42913\nTranslated by {translationAuthor}"
                    : $"~r~{_localeConfig.GswStopped}");

                if (!_configLoaded)
                {
                    Notification.Show("GSW2 couldn't load config, default config was loaded.\n" +
                              $"You need to check {_configReason}");
                }

                if (!_localizationLoaded)
                {
                    Notification.Show("GSW2 couldn't load localization, default localization was loaded.\n" +
                              "You need to check or change localization\n" +
                              "Possible reason: ~r~" + _localizationReason);
                }

                _isInit = true;
            }

            if (_exceptionInRuntime) return;

            try
            {
                GunshotWoundTick();
            }
            catch (Exception exception)
            {
                Notification.Show(_localeConfig.GswStopped);
                Notification.Show($"~r~GSW2 error in runtime:{exception}");
                _exceptionInRuntime = true;
#if DEBUG
                Notification.Show("Last system is " + LastSystem);
#endif
            }
        }

        private void GunshotWoundInit()
        {
            _ecsWorld = new EcsWorld();

            _mainConfig = EcsFilterSingle<MainConfig>.Create(_ecsWorld);
            _localeConfig = EcsFilterSingle<LocaleConfig>.Create(_ecsWorld);
            _gswWorld = EcsFilterSingle<GswWorld>.Create(_ecsWorld);
            _gswWorld.GswPeds = new Dictionary<Ped, int>();

            try
            {
                TryToLoadConfigsFromXml();
                _configLoaded = true;
            }
            catch (Exception e)
            {
                MainConfig.LoadDefaultValues(_mainConfig);
                _configLoaded = false;

#if DEBUG
                Notification.Show(e.ToString());
#endif
            }

            try
            {
                TryToLoadLocalization();
                _localizationLoaded = true;
            }
            catch (Exception e)
            {
                LocaleConfig.FillWithDefaultValues(_localeConfig);
                _localizationReason = e.Message;
                _localizationLoaded = false;

#if DEBUG
                Notification.Show(e.ToString());
#endif
            }

            _everyFrameSystems = new EcsSystems(_ecsWorld);
            _commonSystems = new MultiTickEcsSystems(_ecsWorld, MultiTickEcsSystems.RestrictionModes.MILLISECONDS, 10);

            if (_mainConfig.NpcConfig.AddingPedRange > MINIMAL_RANGE_FOR_WOUNDED_PEDS)
            {
                _commonSystems
                    .Add(new NpcFindSystem())
                    .Add(new ConvertPedToNpcGswPedSystem())
                    .Add(new RemoveWoundedPedSystem());
            }

            if (_mainConfig.PlayerConfig.WoundedPlayerEnabled)
            {
                _everyFrameSystems
                    .Add(new PlayerSystem())
                    .Add(new SpecialAbilityLockSystem());

                if (_mainConfig.PlayerConfig.MaximalSlowMo < 1f)
                {
                    _everyFrameSystems
                        .Add(new AdrenalineSystem());
                }
            }

            _everyFrameSystems
                .Add(new InstantHealSystem())
                .Add(new HelmetRequestSystem())
                .Add(new RagdollSystem())
                .Add(new SwitchAnimationSystem())
                .Add(new DebugInfoSystem())
                .Add(new CameraShakeSystem())
                .Add(new FlashSystem())
                .Add(new PainRecoverySystem())
                .Add(new BleedingSystem())
                .Add(new BandageSystem())
                .Add(new SelfHealingSystem());

            _commonSystems
                .Add(new ArmorSystem())
                .AddHitDetectSystems()
                .AddDamageProcessingSystems()
                .AddWoundSystems()
                .AddPainStateSystems()
                .Add(new CheckSystem())
                .Add(new NotificationSystem());

            _everyFrameSystems.Initialize();
            _commonSystems.Initialize();

            Tick += OnTick;
            KeyUp += OnKeyUp;

            _isPaused = false;
        }

        private void GunshotWoundTick()
        {
            if (_isPaused) return;

            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.01f);
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);

            _everyFrameSystems.Run();
            _commonSystems.Run();

#if DEBUG
            string debugSubtitles = $"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}\n" +
                                    $"Peds in GSW: {_gswWorld.GswPeds.Count}";
            Screen.ShowSubtitle(debugSubtitles);
#endif
        }

        private void TryToLoadConfigsFromXml()
        {
            MainConfig.LoadDefaultValues(_mainConfig);

            bool configInGswFolder = new FileInfo("scripts/GSW2/GSW2Config.xml").Exists;
            bool config = new FileInfo("scripts/GSW2Config.xml").Exists;

            if (!configInGswFolder && !config)
            {
                _configReason = "Config doesn't exist";
                throw new Exception("Config doesn't exist");
            }

            var doc = configInGswFolder
                ? XDocument.Load("scripts/GSW2/GSW2Config.xml").Root
                : XDocument.Load("scripts/GSW2Config.xml").Root;

            _configReason = "Hotkeys section";
            var buttonNode = doc.Element("Hotkeys");
            if (buttonNode != null)
            {
                _mainConfig.HelmetKey = buttonNode.GetKey("GetHelmetKey");
                _mainConfig.CheckKey = buttonNode.GetKey("CheckKey");
                _mainConfig.HealKey = buttonNode.GetKey("HealKey");
                _mainConfig.IncreaseRangeKey = buttonNode.GetKey("IncreaseRangeKey");
                _mainConfig.ReduceRangeKey = buttonNode.GetKey("ReduceRangeKey");
                _mainConfig.BandageKey = buttonNode.GetKey("BandageKey");
                _mainConfig.PauseKey = buttonNode.GetKey("PauseKey");
            }

            _configReason = "Player section";
            var playerNode = doc.Element("Player");
            if (playerNode != null)
            {
                _mainConfig.PlayerConfig.WoundedPlayerEnabled = playerNode.Element("GSWPlayerEnabled").GetBool();

                _mainConfig.PlayerConfig.MinimalHealth = playerNode.Element("MinimalHealth").GetInt();
                _mainConfig.PlayerConfig.MaximalHealth = _mainConfig.PlayerConfig.MinimalHealth + 99;

                _mainConfig.PlayerConfig.MaximalPain = playerNode.Element("MaximalPain").GetFloat();
                _mainConfig.PlayerConfig.PainRecoverSpeed = playerNode.Element("PainRecoverySpeed").GetFloat();
                _mainConfig.PlayerConfig.BleedHealingSpeed = playerNode.Element("BleedHealSpeed").GetFloat() / 1000f;
                _mainConfig.PlayerConfig.PoliceCanForgetYou = playerNode.Element("PoliceCanForget").GetBool();
                _mainConfig.PlayerConfig.CanDropWeapon = playerNode.Element("CanDropWeapon").GetBool();
                _mainConfig.PlayerConfig.MaximalSlowMo = playerNode.Element("MaximalSlowMo").GetFloat();

                var animationNode = playerNode.Element("Animations");
                _mainConfig.PlayerConfig.NoPainAnim = animationNode.Attribute("NoPain").Value;
                _mainConfig.PlayerConfig.MildPainAnim = animationNode.Attribute("MildPain").Value;
                _mainConfig.PlayerConfig.AvgPainAnim = animationNode.Attribute("AvgPain").Value;
                _mainConfig.PlayerConfig.IntensePainAnim = animationNode.Attribute("IntensePain").Value;
            }

            _configReason = "Peds section";
            var npcNode = doc.Element("Peds");
            if (npcNode != null)
            {
                _mainConfig.NpcConfig.AddingPedRange = npcNode.Element("GSWScanRange").GetFloat();
                _mainConfig.NpcConfig.RemovePedRange =
                    _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

                _mainConfig.NpcConfig.ShowEnemyCriticalMessages = npcNode.Element("CriticalMessages").GetBool();
                _mainConfig.NpcConfig.ScanOnlyDamaged = npcNode.Element("ScanOnlyDamaged").GetBool();

                var targetsNode = npcNode.Element("Targets");
                bool all = targetsNode.GetBool("ALL");
                GswTargets targets = 0;
                if (all)
                {
                    _mainConfig.NpcConfig.Targets = GswTargets.ALL;
                }
                else
                {
                    if (targetsNode.GetBool("COMPANION"))
                    {
                        targets = targets | GswTargets.COMPANION;
                    }

                    if (targetsNode.GetBool("DISLIKE"))
                    {
                        targets = targets | GswTargets.DISLIKE;
                    }

                    if (targetsNode.GetBool("HATE"))
                    {
                        targets = targets | GswTargets.HATE;
                    }

                    if (targetsNode.GetBool("LIKE"))
                    {
                        targets = targets | GswTargets.LIKE;
                    }

                    if (targetsNode.GetBool("NEUTRAL"))
                    {
                        targets = targets | GswTargets.NEUTRAL;
                    }

                    if (targetsNode.GetBool("PEDESTRIAN"))
                    {
                        targets = targets | GswTargets.PEDESTRIAN;
                    }

                    if (targetsNode.GetBool("RESPECT"))
                    {
                        targets = targets | GswTargets.RESPECT;
                    }

                    _mainConfig.NpcConfig.Targets = targets;
                }

                var healthNode = npcNode.Element("PedHealth");
                _mainConfig.NpcConfig.MinStartHealth = healthNode.GetInt("Min");
                _mainConfig.NpcConfig.MaxStartHealth = healthNode.GetInt("Max");

                var painNode = npcNode.Element("MaximalPain");
                _mainConfig.NpcConfig.LowerMaximalPain = painNode.GetFloat("Min");
                _mainConfig.NpcConfig.UpperMaximalPain = painNode.GetFloat("Max");

                var accuracyNode = npcNode.Element("Accuracy");
                _mainConfig.NpcConfig.MinAccuracy = accuracyNode.GetInt("Min");
                _mainConfig.NpcConfig.MaxAccuracy = accuracyNode.GetInt("Max");

                var rateNode = npcNode.Element("ShootRate");
                _mainConfig.NpcConfig.MinShootRate = rateNode.GetInt("Min");
                _mainConfig.NpcConfig.MaxShootRate = rateNode.GetInt("Max");

                _mainConfig.NpcConfig.MaximalPainRecoverSpeed = npcNode.Element("PainRecoverySpeed").GetFloat();
                _mainConfig.NpcConfig.MaximalBleedStopSpeed = npcNode.Element("BleedHealSpeed").GetFloat() / 1000f;

                var animationNode = npcNode.Element("Animations");
                _mainConfig.NpcConfig.NoPainAnim = animationNode.Attribute("NoPain").Value;
                _mainConfig.NpcConfig.MildPainAnim = animationNode.Attribute("MildPain").Value;
                _mainConfig.NpcConfig.AvgPainAnim = animationNode.Attribute("AvgPain").Value;
                _mainConfig.NpcConfig.IntensePainAnim = animationNode.Attribute("IntensePain").Value;
            }

            _configReason = "Notifications section";
            var noteNode = doc.Element("Notifications");
            if (noteNode != null)
            {
                _mainConfig.Language = noteNode.Element("Language").Attribute("Value").Value;
                _mainConfig.CommonMessages = noteNode.Element("Common").GetBool();
                _mainConfig.WarningMessages = noteNode.Element("Warning").GetBool();
                _mainConfig.AlertMessages = noteNode.Element("Alert").GetBool();
                _mainConfig.EmergencyMessages = noteNode.Element("Emergency").GetBool();
            }

            _configReason = "Wounds section";
            var woundsNode = doc.Element("Wounds");
            if (woundsNode != null)
            {
                _mainConfig.WoundConfig.MoveRateOnFullPain = woundsNode.Element("MoveRateOnFullPain").GetFloat();
                _mainConfig.WoundConfig.RealisticNervesDamage = woundsNode.Element("RealisticNervesDamage").GetBool();
                _mainConfig.WoundConfig.DamageMultiplier = woundsNode.Element("OverallDamageMult").GetFloat();
                _mainConfig.WoundConfig.DamageDeviation = woundsNode.Element("DamageDeviation").GetFloat();
                _mainConfig.WoundConfig.PainMultiplier = woundsNode.Element("OverallPainMult").GetFloat();
                _mainConfig.WoundConfig.PainDeviation = woundsNode.Element("PainDeviation").GetFloat();
                _mainConfig.WoundConfig.BleedingMultiplier = woundsNode.Element("OverallBleedingMult").GetFloat();
                _mainConfig.WoundConfig.BleedingDeviation = woundsNode.Element("BleedingDeviation").GetFloat();
                _mainConfig.WoundConfig.RagdollOnPainfulWound = woundsNode.Element("RagdollOnPainfulWound").GetBool();
                _mainConfig.WoundConfig.PainfulWoundValue = woundsNode.Element("PainfulWoundValue").GetFloat();
                _mainConfig.WoundConfig.MinimalChanceForArmorSave =
                    woundsNode.Element("MinimalChanceForArmorSave").GetFloat();
                _mainConfig.WoundConfig.ApplyBandageTime = woundsNode.Element("ApplyBandageTime").GetFloat();
                _mainConfig.WoundConfig.BandageCost = woundsNode.Element("BandageCost").GetInt();
                _mainConfig.WoundConfig.SelfHealingRate = woundsNode.Element("SelfHealingRate").GetFloat();
            }

            _configReason = "Weapons section";
            var weaponNode = doc.Element("Weapons");
            if (weaponNode != null)
            {
                var dictionary = new Dictionary<string, float?[]>();

                foreach (XElement element in weaponNode.Elements())
                {
                    var mults = new float?[5];

                    var damageString = element.Attribute("DamageMult");
                    mults[0] = damageString != null
                        ? (float?) float.Parse(damageString.Value, CultureInfo.InvariantCulture)
                        : null;

                    var bleedingString = element.Attribute("BleedingMult");
                    mults[1] = bleedingString != null
                        ? (float?) float.Parse(bleedingString.Value, CultureInfo.InvariantCulture)
                        : null;

                    var painString = element.Attribute("PainMult");
                    mults[2] = painString != null
                        ? (float?) float.Parse(painString.Value, CultureInfo.InvariantCulture)
                        : null;

                    var critString = element.Attribute("CritChance");
                    mults[3] = critString != null
                        ? (float?) float.Parse(critString.Value, CultureInfo.InvariantCulture)
                        : null;

                    var armorString = element.Attribute("ArmorDamage");
                    mults[4] = armorString != null
                        ? (float?) float.Parse(armorString.Value, CultureInfo.InvariantCulture)
                        : null;

                    dictionary.Add(element.Name.LocalName, mults);
                }

                _mainConfig.WoundConfig.DamageSystemConfigs = dictionary;
            }

#if DEBUG
            Notification.Show($"{_mainConfig}");
#endif
        }

        private void TryToLoadLocalization()
        {
            bool configInGswFolder = new FileInfo("scripts/GSW2/GSW2Localization.csv").Exists;
            bool config = new FileInfo("scripts/GSW2Localization.csv").Exists;
            if (!configInGswFolder && !config)
            {
                throw new Exception("Localization doesn't exist");
            }

            var doc = configInGswFolder
                ? new FileInfo("scripts/GSW2/GSW2Localization.csv")
                : new FileInfo("scripts/GSW2Localization.csv");

            var manager = new LocalizationManager(doc.OpenRead());
            manager.SetLanguage(_mainConfig.Language);

            _localeConfig.HelmetSavedYourHead = manager.GetWord("HelmetSavedYourHead");
            _localeConfig.ArmorSavedYourChest = manager.GetWord("ArmorSavedYourChest");
            _localeConfig.ArmorSavedYourLowerBody = manager.GetWord("ArmorSavedYourLowerBody");
            _localeConfig.ArmorPenetrated = manager.GetWord("ArmorPenetrated");

            _localeConfig.BodyPartHead = manager.GetWord("BodyPartHead");
            _localeConfig.BodyPartNeck = manager.GetWord("BodyPartNeck");
            _localeConfig.BodyPartChest = manager.GetWord("BodyPartChest");
            _localeConfig.BodyPartLowerBody = manager.GetWord("BodyPartLowerBody");
            _localeConfig.BodyPartArm = manager.GetWord("BodyPartArm");
            _localeConfig.BodyPartLeg = manager.GetWord("BodyPartLeg");

            _localeConfig.GrazeWound = manager.GetWord("GrazeWound");

            _localeConfig.GrazeGswOn = manager.GetWord("GrazeGswOn");
            _localeConfig.FleshGswOn = manager.GetWord("FleshGswOn");
            _localeConfig.PenetratingGswOn = manager.GetWord("PenetratingGswOn");
            _localeConfig.PerforatingGswOn = manager.GetWord("PerforatingGswOn");
            _localeConfig.AvulsiveGswOn = manager.GetWord("AvulsiveGswOn");

            _localeConfig.HeavyBrainDamage = manager.GetWord("HeavyBrainDamage");
            _localeConfig.BulletFlyThroughYourHead = manager.GetWord("BulletFlyThroughYourHead");
            _localeConfig.BulletTornApartYourBrain = manager.GetWord("BulletTornApartYourBrain");

            _localeConfig.LightBruise = manager.GetWord("LightBruise");
            _localeConfig.LightBruiseOn = manager.GetWord("LightBruiseOn");
            _localeConfig.MediumBruiseOn = manager.GetWord("MediumBruiseOn");
            _localeConfig.HeavyBruiseOn = manager.GetWord("HeavyBruiseOn");
            _localeConfig.AbrazionWoundOn = manager.GetWord("AbrazionWoundOn");
            _localeConfig.WindedFromImpact = manager.GetWord("WindedFromImpact");

            _localeConfig.IncisionWoundOn = manager.GetWord("IncisionWoundOn");
            _localeConfig.LacerationWoundOn = manager.GetWord("LacerationWoundOn");
            _localeConfig.StabWoundOn = manager.GetWord("StabWoundOn");

            _localeConfig.BodyBlown = manager.GetWord("BodyBlown");
            _localeConfig.HeadBlown = manager.GetWord("HeadBlown");
            _localeConfig.NeckBlown = manager.GetWord("NeckBlown");
            _localeConfig.ChestBlown = manager.GetWord("ChestBlown");
            _localeConfig.LowerBodyBlown = manager.GetWord("LowerBodyBlown");
            _localeConfig.ArmBlown = manager.GetWord("ArmBlown");
            _localeConfig.LegBlown = manager.GetWord("LegBlown");

            _localeConfig.Blackout = manager.GetWord("Blackout");
            _localeConfig.BleedingInHead = manager.GetWord("BleedingInHead");
            _localeConfig.TraumaticBrainInjury = manager.GetWord("TraumaticBrainInjury");
            _localeConfig.BrokenNeck = manager.GetWord("BrokenNeck");

            _localeConfig.Health = manager.GetWord("Health");
            _localeConfig.YouAreDead = manager.GetWord("YouAreDead");
            _localeConfig.Pain = manager.GetWord("Pain");

            _localeConfig.ArmorLooksGreat = manager.GetWord("ArmorLooksGreat");
            _localeConfig.ScratchesOnArmor = manager.GetWord("ScratchesOnArmor");
            _localeConfig.DentsOnArmor = manager.GetWord("DentsOnArmor");
            _localeConfig.ArmorLooksAwful = manager.GetWord("ArmorLooksAwful");

            _localeConfig.Crits = manager.GetWord("Crits");
            _localeConfig.NervesCrit = manager.GetWord("NervesCrit");
            _localeConfig.HeartCrit = manager.GetWord("HeartCrit");
            _localeConfig.LungsCrit = manager.GetWord("LungsCrit");
            _localeConfig.StomachCrit = manager.GetWord("StomachCrit");
            _localeConfig.GutsCrit = manager.GetWord("GutsCrit");
            _localeConfig.ArmsCrit = manager.GetWord("ArmsCrit");
            _localeConfig.LegsCrit = manager.GetWord("LegsCrit");

            _localeConfig.Wounds = manager.GetWord("Wounds");

            _localeConfig.DontHaveMoneyForHelmet = manager.GetWord("DontHaveMoneyForHelmet");

            _localeConfig.InternalBleeding = manager.GetWord("InternalBleeding");
            _localeConfig.SeveredArtery = manager.GetWord("SeveredArtery");
            _localeConfig.SeveredArteryMessage = manager.GetWord("SeveredArteryMessage");

            _localeConfig.PlayerNervesCritMessage = manager.GetWord("PlayerNervesCritMessage");
            _localeConfig.ManNervesCritMessage = manager.GetWord("ManNervesCritMessage");
            _localeConfig.WomanNervesCritMessage = manager.GetWord("WomanNervesCritMessage");

            _localeConfig.PlayerHeartCritMessage = manager.GetWord("PlayerHeartCritMessage");
            _localeConfig.ManHeartCritMessage = manager.GetWord("ManHeartCritMessage");
            _localeConfig.WomanHeartCritMessage = manager.GetWord("WomanHeartCritMessage");

            _localeConfig.PlayerLungsCritMessage = manager.GetWord("PlayerLungsCritMessage");
            _localeConfig.ManLungsCritMessage = manager.GetWord("ManLungsCritMessage");
            _localeConfig.WomanLungsCritMessage = manager.GetWord("WomanLungsCritMessage");

            _localeConfig.PlayerStomachCritMessage = manager.GetWord("PlayerStomachCritMessage");
            _localeConfig.ManStomachCritMessage = manager.GetWord("ManStomachCritMessage");
            _localeConfig.WomanStomachCritMessage = manager.GetWord("WomanStomachCritMessage");

            _localeConfig.PlayerGutsCritMessage = manager.GetWord("PlayerGutsCritMessage");
            _localeConfig.ManGutsCritMessage = manager.GetWord("ManGutsCritMessage");
            _localeConfig.WomanGutsCritMessage = manager.GetWord("WomanGutsCritMessage");

            _localeConfig.PlayerArmsCritMessage = manager.GetWord("PlayerArmsCritMessage");
            _localeConfig.ManArmsCritMessage = manager.GetWord("ManArmsCritMessage");
            _localeConfig.WomanArmsCritMessage = manager.GetWord("WomanArmsCritMessage");

            _localeConfig.PlayerLegsCritMessage = manager.GetWord("PlayerLegsCritMessage");
            _localeConfig.ManLegsCritMessage = manager.GetWord("ManLegsCritMessage");
            _localeConfig.WomanLegsCritMessage = manager.GetWord("WomanLegsCritMessage");

            _localeConfig.UnbearablePainMessage = manager.GetWord("UnbearablePainMessage");

            _localeConfig.AddingRange = manager.GetWord("AddingRange");
            _localeConfig.RemovingRange = manager.GetWord("RemovingRange");

            _localeConfig.ThanksForUsing = manager.GetWord("ThanksForUsing");
            _localeConfig.GswStopped = manager.GetWord("GswStopped");
            _localeConfig.GswIsPaused = manager.GetWord("GswIsPaused");
            _localeConfig.GswIsWorking = manager.GetWord("GswIsWorking");

            _localeConfig.AlreadyBandaging = manager.GetWord("AlreadyBandaging");
            _localeConfig.DontHaveMoneyForBandage = manager.GetWord("DontHaveMoneyForBandage");
            _localeConfig.YouTryToBandage = manager.GetWord("YouTryToBandage");
            _localeConfig.BandageFailed = manager.GetWord("BandageFailed");
            _localeConfig.BandageSuccess = manager.GetWord("BandageSuccess");

            _localeConfig.ArmorDestroyed = manager.GetWord("ArmorDestroyed");

            _localeConfig.LocalizationAuthor = manager.GetWord("TranslationAuthor");
        }

        private void ReduceRange(float value)
        {
            if (_mainConfig.NpcConfig.AddingPedRange - value < MINIMAL_RANGE_FOR_WOUNDED_PEDS) return;

            _mainConfig.NpcConfig.AddingPedRange -= value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
        }

        private void IncreaseRange(float value)
        {
            if (_mainConfig.NpcConfig.AddingPedRange < MINIMAL_RANGE_FOR_WOUNDED_PEDS) return;

            _mainConfig.NpcConfig.AddingPedRange += value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
        }


        private void CheckPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
        }

        private void HealPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<InstantHealEvent>().Entity = playerEntity;
        }

        private void ApplyBandageToPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<ApplyBandageEvent>().Entity = playerEntity;
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }

    internal static class Extensions
    {
        public static MultiTickEcsSystems AddHitDetectSystems(this MultiTickEcsSystems systems)
        {
            return systems
                .Add(new HitDetectSystem())
                .Add(new LightImpactHitSystem())
                .Add(new CuttingHitSystem())
                .Add(new HeavyImpactHitSystem())
                .Add(new SmallCaliberHitSystem())
                .Add(new ShotgunHitSystem())
                .Add(new MediumCaliberHitSystem())
                .Add(new HighCaliberHitSystem())
                .Add(new ExplosionHitSystem())
                .Add(new BodyHitSystem())
                .Add(new HitCleanSystem());
        }

        public static MultiTickEcsSystems AddDamageProcessingSystems(this MultiTickEcsSystems systems)
        {
            return systems
                .Add(new SmallCaliberDamageSystem())
                .Add(new ShotgunDamageSystem())
                .Add(new LightImpactDamageSystem())
                .Add(new HeavyImpactDamageSystem())
                .Add(new MediumCaliberDamageSystem())
                .Add(new HighCaliberDamageSystem())
                .Add(new CuttingDamageSystem())
                .Add(new ExplosionDamageSystem());
        }

        public static MultiTickEcsSystems AddWoundSystems(this MultiTickEcsSystems systems)
        {
            return systems
                .Add(new WoundSystem())
                .Add(new HeartCriticalSystem())
                .Add(new LungsCriticalSystem())
                .Add(new NervesCriticalSystem())
                .Add(new ArmsCriticalSystem())
                .Add(new LegsCriticalSystem())
                .Add(new GutsCriticalSystem())
                .Add(new StomachCriticalSystem());
        }

        public static MultiTickEcsSystems AddPainStateSystems(this MultiTickEcsSystems systems)
        {
            return systems
                .Add(new IncreasePainSystem())
                .Add(new NoPainStateSystem())
                .Add(new MildPainStateSystem())
                .Add(new AveragePainStateSystem())
                .Add(new IntensePainStateSystem())
                .Add(new UnbearablePainStateSystem())
                .Add(new DeadlyPainStateSystem());
        }


        public static float NextFloat(this Random rand, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("min must be less than max");
            }

            return (float) rand.NextDouble() * (max - min) + min;
        }

        public static void RemoveAllEntities(this EcsFilter filter)
        {
            var world = filter.GetWorld();
            for (var i = 0; i < filter.EntitiesCount; i++)
            {
                world.RemoveEntity(filter.Entities[i]);
            }
        }

        public static bool GetBool(this XElement node, string attributeName = "Value")
        {
            string value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return !string.IsNullOrEmpty(value) && bool.Parse(value);
        }

        public static int GetInt(this XElement node, string attributeName = "Value")
        {
            string value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return int.Parse(value);
        }

        public static float GetFloat(this XElement node, string attributeName = "Value")
        {
            string value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public static Keys? GetKey(this XElement node, string name)
        {
            if (string.IsNullOrEmpty(node?.Element(name)?.Value)) return null;

            return (Keys) int.Parse(node.Element(name).Value);
        }
    }
}