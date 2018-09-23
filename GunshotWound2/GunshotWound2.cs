using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.HealingEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using GunshotWound2.Systems.DamageProcessingSystems;
using GunshotWound2.Systems.EffectSystems;
using GunshotWound2.Systems.GuiSystems;
using GunshotWound2.Systems.HealingSystems;
using GunshotWound2.Systems.HitSystems;
using GunshotWound2.Systems.HitSystems.WeaponHitSystems;
using GunshotWound2.Systems.NpcSystems;
using GunshotWound2.Systems.PedSystems;
using GunshotWound2.Systems.PlayerSystems;
using GunshotWound2.Systems.WoundSystems;
using GunshotWound2.Systems.WoundSystems.CriticalWoundSystems;
using GunshotWound2.Systems.WoundSystems.PainStatesSystems;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2
{
    public class GunshotWound2 : Script
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

        private bool _isInited;
        private bool _configLoaded;
        private string _configReason;
        private bool _localizationLoaded;
        private string _localizationReason;
        private bool _exceptionInRuntime;
        private int _ticks;
        
        public GunshotWound2()
        {
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
                UI.Notify(_isPaused 
                    ? "~r~GSW2 is paused" 
                    : "~g~GSW2 is working");
                return;
            }
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            if (!_isInited && _ticks++ == 400)
            {
                string translationAuthor = string.IsNullOrEmpty(_localeConfig.LocalizationAuthor) 
                    ? "GSW2-community" 
                    : _localeConfig.LocalizationAuthor;
                UI.Notify(!_exceptionInRuntime
                    ? $"{_localeConfig.ThanksForUsing}\n~g~GunShot Wound ~r~2~s~\nby SH42913\nTranslated by {translationAuthor}"
                    : $"~r~{_localeConfig.GswStopped}");

                if (!_configLoaded)
                {
                    UI.Notify("GSW2 couldn't load config, default config was loaded.\n" +
                              $"You need to check {_configReason}");
                }

                if (!_localizationLoaded)
                {
                    UI.Notify("GSW2 couldn't load localization, default localization was loaded.\n" +
                              "You need to check or change localization\n" +
                              "Possible reason: ~r~" + _localizationReason);
                }
                
                _isInited = true;
            }
            
            if(_exceptionInRuntime) return;
            
            try
            {
                GunshotWoundTick();
            }
            catch (Exception exception)
            {
                UI.Notify("~r~GSW2 error in runtime:\n" +
                          $"{exception.Message}");
                UI.Notify(_localeConfig.GswStopped);
                _exceptionInRuntime = true;
#if DEBUG
                UI.Notify("Last system is " + LastSystem);
#endif
            }
        }

        private void GunshotWoundInit()
        {
            _ecsWorld = new EcsWorld();

            _mainConfig = EcsFilterSingle<MainConfig>.Create(_ecsWorld);
            _localeConfig = EcsFilterSingle<LocaleConfig>.Create(_ecsWorld);
            _gswWorld = EcsFilterSingle<GswWorld>.Create(_ecsWorld);
            _gswWorld.GswPeds = new HashSet<Ped>();

            try
            {
                TryToLoadConfigsFromXml();
                _configLoaded = true;
            }
            catch (Exception e)
            {
                LoadDefaultConfigs();
                _configLoaded = false;

#if DEBUG
                UI.Notify(e.ToString());
#endif
            }

            try
            {
                TryToLoadLocalization();
                _localizationLoaded = true;
            }
            catch (Exception e)
            {
                LoadDefaultLocalization();
                _localizationReason = e.Message;
                _localizationLoaded = false;

#if DEBUG
                UI.Notify(e.ToString());
#endif
            }
            
            _everyFrameSystems = new EcsSystems(_ecsWorld);
            _commonSystems = new MultiTickEcsSystems(_ecsWorld, MultiTickEcsSystems.RestrictionModes.MILLISECONDS, 10);

            if (_mainConfig.NpcConfig.AddingPedRange > MINIMAL_RANGE_FOR_WOUNDED_PEDS)
            {
                _commonSystems
                    .Add(new NpcFindSystem())
                    .Add(new ConvertNpcToWoundedPedSystem())
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
                .Add(new CheckSystem())
                .Add(new NotificationSystem())
                
                .Add(new CameraShakeSystem())
                .Add(new FlashSystem())
                
                .Add(new PainRecoverySystem())
                .Add(new BleedingSystem())
                .Add(new SelfHealingSystem());
            
            _commonSystems
                .Add(new ArmorSystem())
                .Add(new BandageSystem())
                .AddHitDetectSystems()
                .AddDamageProcessingSystems()
                .AddWoundSystems()
                .AddPainStateSystems();
            
            _everyFrameSystems.Initialize();
            _commonSystems.Initialize();
            
            Tick += OnTick;
            KeyUp += OnKeyUp;
            
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.0001f);
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);

            _isPaused = false;
        }

        private void GunshotWoundTick()
        {
            if(_isPaused) return;
            
            _everyFrameSystems.Run();
            _commonSystems.Run();

#if DEBUG
            string debugSubtitles = $"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}\n" +
                                    $"Peds in GSW: {_gswWorld.GswPeds.Count}";
            UI.ShowSubtitle(debugSubtitles);
#endif
        }

        private void LoadDefaultConfigs()
        {
            _mainConfig.Language = "EN";
            
            _mainConfig.HelmetKey = Keys.J;
            _mainConfig.BandageKey = Keys.K;
            _mainConfig.CheckKey = Keys.L;
            _mainConfig.IncreaseRangeKey = Keys.PageUp;
            _mainConfig.ReduceRangeKey = Keys.PageDown;
            _mainConfig.PauseKey = Keys.End;
            _mainConfig.HealKey = null;

            _mainConfig.PlayerConfig = new PlayerConfig
            {
                WoundedPlayerEnabled = true,
                CanDropWeapon = true,
                MoneyForHelmet = 40,
                MaximalHealth = 99,
                MinimalHealth = 0,
                MaximalPain = 100,
                PainRecoverSpeed = 1.5f,
                BleedHealingSpeed = 0.001f,
                PlayerEntity = -1,
                MaximalSlowMo = 0.5f,
                PoliceCanForgetYou = true,
                NoPainAnim = "move_m@generic",
                MildPainAnim = "move_m@gangster@a",
                AvgPainAnim = "move_m@drunk@moderatedrunk",
                IntensePainAnim = "move_m@drunk@verydrunk"
            };

            _mainConfig.NpcConfig = new NpcConfig
            {
                AddingPedRange = 50f,
                RemovePedRange = 100f,
                ShowEnemyCriticalMessages = true,
                MinStartHealth = 50,
                MaxStartHealth = 100,
                MaximalBleedStopSpeed = 0.001f,
                LowerMaximalPain = 50,
                UpperMaximalPain = 80,
                MaximalPainRecoverSpeed = 1f,
                UpperBoundForFindInMs = 10,
                MinAccuracy = 10,
                MaxAccuracy = 50,
                MinShootRate = 10,
                MaxShootRate = 50,
                Targets = GswTargets.ALL,
                ScanOnlyDamaged = false
            };

            _mainConfig.WoundConfig = new WoundConfig
            {
                MoveRateOnNervesDamage = 0.7f,
                MoveRateOnFullPain = 0.8f,
                EmergencyBleedingLevel = 1.5f,
                RealisticNervesDamage = true,
                DamageMultiplier = 1,
                BleedingMultiplier = 1,
                PainMultiplier = 1,
                DamageDeviation = 0.2f,
                BleedingDeviation = 0.2f,
                PainDeviation = 0.2f,
                RagdollOnPainfulWound = true,
                PainfulWoundValue = 50,
                MinimalChanceForArmorSave = 0.6f,
                BandageCost = 15
            };
        }

        private void TryToLoadConfigsFromXml()
        {
            LoadDefaultConfigs();
            
            bool configInGswFolder = new FileInfo("scripts/GSW2/GSW2Config.xml").Exists;
            bool config = new FileInfo("scripts/GSW2Config.xml").Exists;
            
            if(!configInGswFolder && !config)
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
                _mainConfig.PlayerConfig.BleedHealingSpeed = playerNode.Element("BleedHealSpeed").GetFloat();
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
                _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

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
                _mainConfig.NpcConfig.MaximalBleedStopSpeed = npcNode.Element("BleedHealSpeed").GetFloat();

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
                _mainConfig.WoundConfig.MinimalChanceForArmorSave = woundsNode.Element("MinimalChanceForArmorSave").GetFloat();
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
            UI.Notify($"{_mainConfig}");
#endif
        }

        private void LoadDefaultLocalization()
        {
            _localeConfig.HelmetSavedYourHead = "Helmet saved your head";
            _localeConfig.ArmorSavedYourChest = "Armor saved your chest";
            _localeConfig.ArmorSavedYourLowerBody = "Armor saved your lower body";
            _localeConfig.ArmorPenetrated = "Your armor was penetrated";
            
            _localeConfig.BodyPartHead = "head";
            _localeConfig.BodyPartNeck = "neck";
            _localeConfig.BodyPartChest = "chest";
            _localeConfig.BodyPartLowerBody = "lower body";
            _localeConfig.BodyPartArm = "arm";
            _localeConfig.BodyPartLeg = "leg";
            
            _localeConfig.GrazeWound = "Graze wound";
            
            _localeConfig.GrazeGswOn = "Graze GSW on";
            _localeConfig.FleshGswOn = "Flesh GSW on";
            _localeConfig.PenetratingGswOn = "Penetrating GSW on";
            _localeConfig.PerforatingGswOn = "Perforating GSW on";
            _localeConfig.AvulsiveGswOn = "Avulsive GSW on";

            _localeConfig.EarFlyAway = "Part of ear sails off away";
            _localeConfig.HeavyBrainDamage = "Heavy brain damage";
            _localeConfig.BulletFlyThroughYourHead = "Bullet fly through your head";
            _localeConfig.BulletTornApartYourBrain = "Bullet torn apart your brain";

            _localeConfig.LightBruise = "Light bruise";
            _localeConfig.LightBruiseOn = "Light bruise on";
            _localeConfig.MediumBruiseOn = "Medium bruise on";
            _localeConfig.HeavyBruiseOn = "Heavy bruise on";
            _localeConfig.AbrazionWoundOn = "Abrazion wound on";
            _localeConfig.WindedFromImpact = "Winded from impact";

            _localeConfig.IncisionWoundOn = "Incision wound on";
            _localeConfig.LacerationWoundOn = "Laceration wound on";
            _localeConfig.StabWoundOn = "Stab wound on";

            _localeConfig.BodyBlown = "Body blown";
            _localeConfig.HeadBlown = "Head blown";
            _localeConfig.NeckBlown = "Neck blown";
            _localeConfig.ChestBlown = "Chest blown";
            _localeConfig.LowerBodyBlown = "Lower body blown";
            _localeConfig.ArmBlown = "Arm blown";
            _localeConfig.LegBlown = "Leg blown";

            _localeConfig.Blackout = "Blackout possible";
            _localeConfig.BleedingInHead = "Bleeding in the head";
            _localeConfig.TraumaticBrainInjury = "Traumatic brain injury";
            _localeConfig.BrokenNeck = "Broken neck";

            _localeConfig.Health = "Health";
            _localeConfig.YouAreDead = "You are dead!";
            _localeConfig.Pain = "Pain";

            _localeConfig.ArmorLooksGreat = "Your armor looks great";
            _localeConfig.ScratchesOnArmor = "Your armor has some scratches";
            _localeConfig.DentsOnArmor = "Your armor has large dents";
            _localeConfig.ArmorLooksAwful = "Your armor looks awful";

            _localeConfig.Crits = "Critical damaged";
            _localeConfig.NervesCrit = "nerves";
            _localeConfig.HeartCrit = "heart";
            _localeConfig.LungsCrit = "lungs";
            _localeConfig.StomachCrit = "stomach";
            _localeConfig.GutsCrit = "guts";
            _localeConfig.ArmsCrit = "arms";
            _localeConfig.LegsCrit = "legs";

            _localeConfig.Wounds = "Wounds";
            _localeConfig.HaveNoWounds = "You have no wounds";

            _localeConfig.DontHaveMoneyForHelmet = "You don't have enough money to buy helmet";

            _localeConfig.InternalBleeding = "Internal bleeding";
            _localeConfig.SeveredArtery = "Severed artery";
            _localeConfig.SeveredArteryMessage = "Artery was severed!";

            _localeConfig.PlayerNervesCritMessage = "You feel you can't control your arms and legs anymore";
            _localeConfig.ManNervesCritMessage = "He looks like his spine was damaged";
            _localeConfig.WomanNervesCritMessage = "She looks like her spine was damaged";

            _localeConfig.PlayerHeartCritMessage = "You feel awful pain in your chest";
            _localeConfig.ManHeartCritMessage = "He coughs up blood";
            _localeConfig.WomanHeartCritMessage = "She coughs up blood";

            _localeConfig.PlayerLungsCritMessage = "It's very hard for you to breathe";
            _localeConfig.ManLungsCritMessage = "He coughs up blood";
            _localeConfig.WomanLungsCritMessage = "She coughs up blood";

            _localeConfig.PlayerStomachCritMessage = "You feel yourself very sick";
            _localeConfig.ManStomachCritMessage = "He looks very sick";
            _localeConfig.WomanStomachCritMessage = "She looks very sick";

            _localeConfig.PlayerGutsCritMessage = "You feel yourself very sick";
            _localeConfig.ManGutsCritMessage = "He looks very sick";
            _localeConfig.WomanGutsCritMessage = "She looks very sick";

            _localeConfig.PlayerArmsCritMessage = "It's looks like arm bone was broken";
            _localeConfig.ManArmsCritMessage = "His arm looks very bad";
            _localeConfig.WomanArmsCritMessage = "Her arm looks very bad";

            _localeConfig.PlayerLegsCritMessage = "It's looks like leg bone was broken";
            _localeConfig.ManLegsCritMessage = "His leg looks very bad";
            _localeConfig.WomanLegsCritMessage = "Her leg looks very bad";

            _localeConfig.UnbearablePainMessage = "You got a pain shock! You lose consciousness!";

            _localeConfig.AddingRange = "Adding range";
            _localeConfig.RemovingRange = "Removing range";

            _localeConfig.ThanksForUsing = "Thanks for using";
            _localeConfig.GswStopped = "GSW2 stopped, sorry :(";

            _localeConfig.LocalizationAuthor = "~r~SH42913";
        }

        private void TryToLoadLocalization()
        {
            bool configInGswFolder = new FileInfo("scripts/GSW2/GSW2Localization.csv").Exists;
            bool config = new FileInfo("scripts/GSW2Localization.csv").Exists;
            if(!configInGswFolder && !config)
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

            _localeConfig.EarFlyAway = manager.GetWord("EarFlyAway");
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
            _localeConfig.HaveNoWounds = manager.GetWord("HaveNoWounds");

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

            _localeConfig.LocalizationAuthor = manager.GetWord("TranslationAuthor");
        }

        private void ReduceRange(float value)
        {
            if(_mainConfig.NpcConfig.AddingPedRange - value < MINIMAL_RANGE_FOR_WOUNDED_PEDS) return;
            
            _mainConfig.NpcConfig.AddingPedRange -= value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;
            
            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
        }

        private void IncreaseRange(float value)
        {
            if(_mainConfig.NpcConfig.AddingPedRange < MINIMAL_RANGE_FOR_WOUNDED_PEDS) return;
            
            _mainConfig.NpcConfig.AddingPedRange += value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;
            
            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
        }
        
        

        private void CheckPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
        }

        private void HealPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<InstantHealEvent>().Entity = playerEntity;
        }

        private void ApplyBandageToPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
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
            return (float)rand.NextDouble() * (max - min) + min;
        }

        public static void RemoveAllEntities(this EcsFilter filter)
        {
            var world = filter.GetWorld();
            for (var i = 0; i < filter.EntitiesCount; i++) {
                world.RemoveEntity (filter.Entities[i]);
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