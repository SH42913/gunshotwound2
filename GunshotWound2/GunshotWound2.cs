using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using GunshotWound2.Systems.DamageProcessingSystems;
using GunshotWound2.Systems.EffectSystems;
using GunshotWound2.Systems.GuiSystems;
using GunshotWound2.Systems.HitSystems;
using GunshotWound2.Systems.HitSystems.WeaponHitSystems;
using GunshotWound2.Systems.NpcSystems;
using GunshotWound2.Systems.PedSystems;
using GunshotWound2.Systems.PlayerSystems;
using GunshotWound2.Systems.WoundSystems;
using GunshotWound2.Systems.WoundSystems.CriticalWoundSystems;
using GunshotWound2.Systems.WoundSystems.PainStatesSystem;
using Leopotam.Ecs;

namespace GunshotWound2
{
    public class GunshotWound2 : Script
    {
        public static string LastSystem = "Nothing";
        
        public const float MINIMAL_RANGE_FOR_WOUNDED_PEDS = 5;
        private const float ADDING_TO_REMOVING_MULTIPLIER = 2;
        
        
        private EcsWorld _ecsWorld;
        private EcsSystems _everyFrameSystems;
        private MultiTickEcsSystems _commonSystems;
        
        private bool _warningMessageWasShown;
        private MainConfig _mainConfig;
        private LocaleConfig _localeConfig;
        private Stopwatch _mainCycleStopwatch;
        
        public GunshotWound2()
        {
            try
            {
                GunshotWoundInit();
            }
            catch (Exception exception)
            {
                UI.Notify("~r~Error on initialization: \n" +
                          $"In {LastSystem}\n" +
                          $"{exception.Message}");
            }
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
            
            switch (eventArgs.KeyCode)
            {
                case Keys.PageDown:
                    ReduceRange(MINIMAL_RANGE_FOR_WOUNDED_PEDS);
                    break;
                case Keys.PageUp:
                    IncreaseRange(MINIMAL_RANGE_FOR_WOUNDED_PEDS);
                    break;
            }
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                GunshotWoundTick();
            }
            catch (Exception exception)
            {
                UI.Notify("~r~Error on tick: \n" +
                          $"In {LastSystem}\n" +
                          $"{exception.Message}");
            }
        }

        private void GunshotWoundInit()
        {
            _ecsWorld = new EcsWorld();

            _mainConfig = EcsFilterSingle<MainConfig>.Create(_ecsWorld);
            _localeConfig = EcsFilterSingle<LocaleConfig>.Create(_ecsWorld);
            
            TryToLoadConfigsFromXml();
            LoadDefaultLocalization();
            
            _everyFrameSystems = new EcsSystems(_ecsWorld);
            _commonSystems = new MultiTickEcsSystems(_ecsWorld, MultiTickEcsSystems.RestrictionModes.MILLISECONDS, 16);

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
                    .Add(new PlayerSystem());
            }

            if (_mainConfig.PlayerConfig.AdrenalineSlowMotion)
            {
                _everyFrameSystems
                    .Add(new AdrenalineSystem());
            }

            _everyFrameSystems
                .Add(new InstantHealSystem())
                .Add(new HelmetRequestSystem())
                .Add(new DebugInfoSystem())
                .Add(new CheckSystem())
                .Add(new NotificationSystem());
            
            _commonSystems
                .Add(new ArmorSystem())
                .AddHitSystems()
                .AddDamageSystems()
                .AddWoundSystems()
                .AddPainSystems()
                .Add(new RagdollSystem())
                .Add(new SwitchAnimationSystem());
            
            _everyFrameSystems.Initialize();
            _commonSystems.Initialize();
            
            Tick += OnTick;
            KeyUp += OnKeyUp;
            
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.00001f);
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
            
            _mainCycleStopwatch = new Stopwatch();
        }

        private void GunshotWoundTick()
        {
            _mainCycleStopwatch.Restart();
            
            _everyFrameSystems.Run();
            _commonSystems.Run();
            
            _mainCycleStopwatch.Stop();
            CheckTime(_mainCycleStopwatch.ElapsedMilliseconds);

#if DEBUG
            string debugSubtitles = $"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}\n" +
                                    $"Peds: {_ecsWorld.GetFilter<EcsFilter<WoundedPedComponent>>().EntitiesCount}\n" +
                                    $"Ms: {_mainCycleStopwatch.ElapsedMilliseconds}";
            UI.ShowSubtitle(debugSubtitles);
#endif
        }

        private void LoadDefaultConfigs()
        {
            _mainConfig.CheckKey = null;
            _mainConfig.HelmetKey = null;
            _mainConfig.HealKey = null;

            _mainConfig.PlayerConfig = new PlayerConfig
            {
                WoundedPlayerEnabled = true,
                CanDropWeapon = true,
                MoneyForHelmet = 30,
                MaximalHealth = 99,
                MinimalHealth = 0,
                MaximalPain = 100,
                PainRecoverSpeed = 2f,
                BleedHealingSpeed = 0.001f,
                PlayerEntity = -1,
                AdrenalineSlowMotion = false,
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
                LowerStartHealth = 50,
                UpperStartHealth = 100,
                MaximalBleedStopSpeed = 0.001f,
                LowerMaximalPain = 50,
                UpperMaximalPain = 80,
                MaximalPainRecoverSpeed = 1f,
                UpperBoundForFindInMs = 10
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
                PainfulWoundValue = 50
            };
        }

        private void TryToLoadConfigsFromXml()
        {
            LoadDefaultConfigs();

            bool configInGswFolder = new FileInfo("scripts/GSW2/GSW2Config.xml").Exists;
            bool config = new FileInfo("scripts/GSW2Config.xml").Exists;
            
            if(!configInGswFolder && !config)
            {
                UI.Notify("GSW2 can't find config file, was loaded default values");
                UI.ShowSubtitle("GSW2 can't find config file, was loaded default values");
                return;
            }
            
            var doc = configInGswFolder
                ? XDocument.Load("scripts/GSW2/GSW2Config.xml").Root
                : XDocument.Load("scripts/GSW2Config.xml").Root;
            
            LastSystem = "Hotkeys";
            var buttonNode = doc.Element("Hotkeys");
            if (buttonNode != null)
            {
                var helmetString = buttonNode.Element("GetHelmetKey").Value;
                if(!string.IsNullOrEmpty(helmetString)) _mainConfig.HelmetKey = (Keys) int.Parse(helmetString);
                
                var checkString = buttonNode.Element("CheckKey").Value;
                if(!string.IsNullOrEmpty(checkString)) _mainConfig.CheckKey = (Keys) int.Parse(checkString);
                
                var healString = buttonNode.Element("HealKey").Value;
                if(!string.IsNullOrEmpty(healString)) _mainConfig.HealKey = (Keys) int.Parse(healString);
            }

            LastSystem = "Player";
            var playerNode = doc.Element("Player");
            if (playerNode != null)
            {
                var woundedPlayerEnabled = playerNode.Element("WoundedPlayerEnabled").Value;
                _mainConfig.PlayerConfig.WoundedPlayerEnabled = bool.Parse(woundedPlayerEnabled);

                var wyd = true;//bool.Parse(playerNode.Element("WYDCompatible").Value);
                if (!wyd)
                {
                    _mainConfig.PlayerConfig.MinimalHealth = 0;
                    _mainConfig.PlayerConfig.MaximalHealth = 99;
                }
                else
                {
                    _mainConfig.PlayerConfig.MinimalHealth = 1700;
                    _mainConfig.PlayerConfig.MaximalHealth = 1799;
                }

                var maximalPain = float.Parse(playerNode.Element("MaximalPain").Value, CultureInfo.InvariantCulture);
                _mainConfig.PlayerConfig.MaximalPain = maximalPain;

                var painSpd = float.Parse(playerNode.Element("PainRecoverySpeed").Value, CultureInfo.InvariantCulture);
                _mainConfig.PlayerConfig.PainRecoverSpeed = painSpd;

                var bleedSpd = float.Parse(playerNode.Element("BleedHealSpeed").Value, CultureInfo.InvariantCulture);
                _mainConfig.PlayerConfig.BleedHealingSpeed = bleedSpd;

                var policeForget = bool.Parse(playerNode.Element("PoliceCanForget").Value);
                _mainConfig.PlayerConfig.PoliceCanForgetYou = policeForget;

                var canDropWeapon = bool.Parse(playerNode.Element("CanDropWeapon").Value);
                _mainConfig.PlayerConfig.CanDropWeapon = canDropWeapon;

                var animationNode = playerNode.Element("Animations");
                _mainConfig.PlayerConfig.NoPainAnim = animationNode.Attribute("NoPain").Value;
                _mainConfig.PlayerConfig.MildPainAnim = animationNode.Attribute("MildPain").Value;
                _mainConfig.PlayerConfig.AvgPainAnim = animationNode.Attribute("AvgPain").Value;
                _mainConfig.PlayerConfig.IntensePainAnim = animationNode.Attribute("IntensePain").Value;
            }
            
            LastSystem = "Peds";
            var npcNode = doc.Element("Peds");
            if (npcNode != null)
            {
                var woundedPedsRange = npcNode.Element("StartWoundedPedRange").Value;
                _mainConfig.NpcConfig.AddingPedRange = float.Parse(woundedPedsRange, CultureInfo.InvariantCulture);

                var healthNode = npcNode.Element("StartHealth");
                if (healthNode != null)
                {
                    var minimalHealth = healthNode.Attribute("Lower").Value;
                    _mainConfig.NpcConfig.LowerStartHealth = int.Parse(minimalHealth);

                    var maximalHealth = healthNode.Attribute("Upper").Value;
                    _mainConfig.NpcConfig.UpperStartHealth = int.Parse(maximalHealth);
                }

                var painNode = npcNode.Element("MaximalPain");
                if (painNode != null)
                {
                    var minimalPain = float.Parse(painNode.Attribute("Lower").Value, CultureInfo.InvariantCulture);
                    _mainConfig.NpcConfig.LowerMaximalPain = minimalPain;
                
                    var maximalPain = float.Parse(painNode.Attribute("Upper").Value, CultureInfo.InvariantCulture);
                    _mainConfig.NpcConfig.UpperMaximalPain = maximalPain;
                }

                var painSpd = float.Parse(npcNode.Element("PainRecoverySpeed").Value, CultureInfo.InvariantCulture);
                _mainConfig.NpcConfig.MaximalPainRecoverSpeed = painSpd;

                var bleedSpd = float.Parse(npcNode.Element("BleedHealSpeed").Value, CultureInfo.InvariantCulture);
                _mainConfig.NpcConfig.MaximalBleedStopSpeed = bleedSpd;

                var messages = bool.Parse(npcNode.Element("CriticalMessages").Value);
                _mainConfig.NpcConfig.ShowEnemyCriticalMessages = messages;

                var animationNode = npcNode.Element("Animations");
                _mainConfig.NpcConfig.NoPainAnim = animationNode.Attribute("NoPain").Value;
                _mainConfig.NpcConfig.MildPainAnim = animationNode.Attribute("MildPain").Value;
                _mainConfig.NpcConfig.AvgPainAnim = animationNode.Attribute("AvgPain").Value;
                _mainConfig.NpcConfig.IntensePainAnim = animationNode.Attribute("IntensePain").Value;
            }

            LastSystem = "Wounds";
            var woundsNode = doc.Element("Wounds");
            if (woundsNode != null)
            {
                var moveRate = woundsNode.Element("MoveRateOnFullPain").Value;
                _mainConfig.WoundConfig.MoveRateOnFullPain = float.Parse(moveRate, CultureInfo.InvariantCulture);

                var nervesDamage = woundsNode.Element("RealisticNervesDamage").Value;
                _mainConfig.WoundConfig.RealisticNervesDamage = bool.Parse(nervesDamage);

                var damageMult = woundsNode.Element("OverallDamageMult").Value;
                _mainConfig.WoundConfig.DamageMultiplier = float.Parse(damageMult, CultureInfo.InvariantCulture);

                var damageDev = woundsNode.Element("DamageDeviation").Value;
                _mainConfig.WoundConfig.DamageDeviation = float.Parse(damageDev, CultureInfo.InvariantCulture);

                var painMult = woundsNode.Element("OverallPainMult").Value;
                _mainConfig.WoundConfig.PainMultiplier = float.Parse(painMult, CultureInfo.InvariantCulture);

                var painDev = woundsNode.Element("PainDeviation").Value;
                _mainConfig.WoundConfig.PainDeviation = float.Parse(painDev, CultureInfo.InvariantCulture);

                var bleedMult = woundsNode.Element("OverallBleedingMult").Value;
                _mainConfig.WoundConfig.BleedingMultiplier = float.Parse(bleedMult, CultureInfo.InvariantCulture);

                var bleedDev = woundsNode.Element("BleedingDeviation").Value;
                _mainConfig.WoundConfig.BleedingDeviation = float.Parse(bleedDev, CultureInfo.InvariantCulture);

                var ragdollOnPain = woundsNode.Element("RagdollOnPainfulWound").Value;
                _mainConfig.WoundConfig.RagdollOnPainfulWound = bool.Parse(ragdollOnPain);
            }

            LastSystem = "Notifications";
            var noteNode = doc.Element("Notifications");
            if (noteNode != null)
            {
                var commonString = noteNode.Element("Common").Value;
                _mainConfig.CommonMessages = bool.Parse(commonString);
                
                var warningString = noteNode.Element("Warning").Value;
                _mainConfig.WarningMessages = bool.Parse(warningString);
                
                var alertString = noteNode.Element("Alert").Value;
                _mainConfig.AlertMessages = bool.Parse(alertString);
                
                var emergencyString = noteNode.Element("Emergency").Value;
                _mainConfig.EmergencyMessages = bool.Parse(emergencyString);
            }

            LastSystem = "Weapons";
            var weaponNode = doc.Element("Weapons");
            if (weaponNode != null)
            {
                var dictionary = new Dictionary<string, float?[]>();

                foreach (XElement element in weaponNode.Elements())
                {
                    var mults = new float?[4];

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
            _localeConfig.SeveredArtery = "Artery severed";
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

            _localeConfig.PlayerArmsCritMessage = "You feel awful pain in your arm.\nIt's looks like bone was broken.";
            _localeConfig.ManArmsCritMessage = "His arm looks very bad";
            _localeConfig.WomanArmsCritMessage = "Her arm looks very bad";

            _localeConfig.PlayerLegsCritMessage = "You feel awful pain in your leg.\nIt's looks like bone was broken.";
            _localeConfig.ManLegsCritMessage = "His leg looks very bad";
            _localeConfig.WomanLegsCritMessage = "Her leg looks very bad";

            _localeConfig.UnbearablePainMessage = "You can't take this pain anymore!\nYou lose consciousness!";

            _localeConfig.AddingRange = "Adding range";
            _localeConfig.RemovingRange = "Removing range";
            
            _localeConfig.PerfomanceDropMessage = "Performance drop!\n" +
                                                  "You'd better to reduce adding range with help PageDown-button.\n" +
                                                  "You also can turn off WoundedPed at all in GSW2Config.xml";
        }
        
        private void CheckTime(long timeInMs)
        {
            if (timeInMs <= 40) return;
            if (_warningMessageWasShown) return;
                
            SendMessage(_localeConfig.PerfomanceDropMessage, NotifyLevels.WARNING);
            SendMessage("TooMuchTime in " + LastSystem, NotifyLevels.DEBUG);
            _warningMessageWasShown = true;
        }

        private void ReduceRange(float value)
        {
            if(_mainConfig.NpcConfig.AddingPedRange <= value) return;
            
            _mainConfig.NpcConfig.AddingPedRange -= value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;
            
            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
            _warningMessageWasShown = false;
        }

        private void IncreaseRange(float value)
        {
            _mainConfig.NpcConfig.AddingPedRange += value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;
            
            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange}");
            _warningMessageWasShown = false;
        }
        
        

        private void CheckPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().PedEntity = playerEntity;
        }

        private void HealPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<InstantHealEvent>().PedEntity = playerEntity;
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
        public static MultiTickEcsSystems AddHitSystems(this MultiTickEcsSystems systems)
        {
            return systems
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
        
        public static MultiTickEcsSystems AddDamageSystems(this MultiTickEcsSystems systems)
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
                .Add(new StomachCriticalSystem())
                .Add(new BleedingSystem());
        }

        public static MultiTickEcsSystems AddPainSystems(this MultiTickEcsSystems systems)
        {
            return systems
                .Add(new PainSystem())
                .Add(new PainRecoverySystem())
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
    }
}