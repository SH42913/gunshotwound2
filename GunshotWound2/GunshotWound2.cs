using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using GTA;
using GTA.Native;
using GunshotWound2.Components;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using GunshotWound2.Systems;
using GunshotWound2.Systems.DamageSystems;
using GunshotWound2.Systems.EffectSystems;
using GunshotWound2.Systems.HitSystems;
using GunshotWound2.Systems.HitSystems.WeaponHitSystems;
using GunshotWound2.Systems.NpcSystems;
using GunshotWound2.Systems.PlayerSystems;
using GunshotWound2.Systems.UiSystems;
using GunshotWound2.Systems.WoundSystems;
using GunshotWound2.Systems.WoundSystems.CriticalWoundSystems;
using GunshotWound2.Systems.WoundSystems.PainStatesSystem;
using Leopotam.Ecs;

namespace GunshotWound2
{
    public class GunshotWound2 : Script
    {
        private EcsWorld _ecsWorld;
        private EcsSystems _updateSystems;
        
        private bool _warningMessageWasShown;
        private MainConfig _mainConfig;
        private Stopwatch _debugStopwatch;
        public static string LastSystem = "Nothing";
        
        
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
                _ecsWorld.CreateEntityWith<HelmetRequestComponent>();
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
                    ReduceRange(5);
                    break;
                case Keys.PageUp:
                    IncreaseRange(5);
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
            LoadConfigsFromXml();
            
            _updateSystems = new EcsSystems(_ecsWorld);

            LastSystem = "AddNpcSystem";
            if (_mainConfig.NpcConfig.AddingPedRange > 1f)
            {
                _updateSystems
                    .Add(new NpcSystem());
            }

            LastSystem = "AddPlayerSystem";
            if (_mainConfig.PlayerConfig.WoundedPlayerEnabled)
            {
                _updateSystems
                    .Add(new PlayerSystem());
            }

            LastSystem = "AddAdrenalineSystem";
            if (_mainConfig.PlayerConfig.AdrenalineSlowMotion)
            {
                _updateSystems
                    .Add(new AdrenalineSystem());
            }

            LastSystem = "AddOtherSystems";
            _updateSystems
                .Add(new InstantHealSystem())
                .AddHitSystems()
                .AddDamageSystems()
                .AddWoundSystems()
                .AddPainSystems()
                .Add(new HitCleanSystem())
                .Add(new HelmetRequestSystem())
                .Add(new DebugInfoSystem())
                .Add(new CheckSystem())
                .Add(new NotificationSystem())
                .Add(new ArmorSystem())
                .Add(new RagdollSystem())
                .Add(new SwitchAnimationSystem());
            
            LastSystem = "OnInit";
            _updateSystems.Initialize();
            
            Tick += OnTick;
            KeyUp += OnKeyUp;
            
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.00001f);
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
            
            LastSystem = "Stopwatch";
            _debugStopwatch = new Stopwatch();
        }

        private void GunshotWoundTick()
        {
            _debugStopwatch.Restart();
            _updateSystems.Run();
            _debugStopwatch.Stop();
            
            CheckTime(_debugStopwatch.ElapsedMilliseconds);

#if DEBUG
            string debugSubtitles = $"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}/{_mainConfig.TicksToRefresh}\n" +
                                    $"Peds: {_ecsWorld.GetFilter<EcsFilter<WoundedPedComponent>>().EntitiesCount}\n" +
                                    $"Ms: {_debugStopwatch.ElapsedMilliseconds}";
            UI.ShowSubtitle(debugSubtitles);
#endif
        }

        private void LoadConfigsFromXml()
        {
            _mainConfig.TicksToRefresh = 30;
            _mainConfig.CheckKey = null;
            _mainConfig.HelmetKey = null;
            _mainConfig.HealKey = null;

            _mainConfig.PlayerConfig = new PlayerConfig
            {
                WoundedPlayerEnabled = true,
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
                MinimalStartHealth = 50,
                MaximalStartHealth = 100,
                MaximalBleedStopSpeed = 0.001f,
                MaximalPain = 80,
                MaximalPainRecoverSpeed = 1f
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
                PainDeviation = 0.2f
            };

            bool configInGsw = new FileInfo("scripts/GSW2/GSW2Config.xml").Exists;
            bool config = new FileInfo("scripts/GSW2Config.xml").Exists;
            
            if(!configInGsw && !config)
            {
                UI.Notify("GSW2 can't find config file, was loaded default values");
                UI.ShowSubtitle("GSW2 can't find config file, was loaded default values");
                return;
            }
            
            var doc = configInGsw
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

                var wyd = false;//bool.Parse(playerNode.Element("WYDCompatible").Value);
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

                var minimalHealth = npcNode.Element("MinimalStartHealth").Value;
                _mainConfig.NpcConfig.MinimalStartHealth = int.Parse(minimalHealth);

                var maximalHealth = npcNode.Element("MaximalStartHealth").Value;
                _mainConfig.NpcConfig.MinimalStartHealth = int.Parse(maximalHealth);

                var maximalPain = float.Parse(npcNode.Element("MaximalPain").Value, CultureInfo.InvariantCulture);
                _mainConfig.NpcConfig.MaximalPain = maximalPain;

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
        
        private void CheckTime(long timeInMs)
        {
            if (_mainConfig.TicksToRefresh < 90 && timeInMs > 40)
            {
                _mainConfig.TicksToRefresh++;
            }
            else if(_mainConfig.TicksToRefresh > 3 && timeInMs < 20)
            {
                _mainConfig.TicksToRefresh--;
                _warningMessageWasShown = false;
            }
            else if(_mainConfig.TicksToRefresh == 90)
            {
                if (_warningMessageWasShown) return;
                
                SendMessage("Performance drop!\nYou'd better to reduce adding range with help PageDown-button.\n" +
                            "You also can turn off WoundedPeds at all, " +
                            "just change StartWoundedPedRange to 0 in GSW2Config.xml", NotifyLevels.WARNING);
                _warningMessageWasShown = true;
            }
        }

        private void ReduceRange(float value)
        {
            if(_mainConfig.NpcConfig.AddingPedRange <= value) return;
            _mainConfig.NpcConfig.AddingPedRange -= value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * 2f;
            
            SendMessage($"Adding Range: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"Removing Range: {_mainConfig.NpcConfig.RemovePedRange}");
        }

        private void IncreaseRange(float value)
        {
            _mainConfig.NpcConfig.AddingPedRange += value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * 2f;
            
            SendMessage($"Adding Range: {_mainConfig.NpcConfig.AddingPedRange}\n" +
                        $"Removing Range: {_mainConfig.NpcConfig.RemovePedRange}");
        }
        
        

        private void CheckPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<CheckPedComponent>().PedEntity = playerEntity;
        }

        private void HealPlayer()
        {
            int playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if(playerEntity < 0) return;
            
            _ecsWorld.CreateEntityWith<InstantHealComponent>().PedEntity = playerEntity;
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
        
    internal static class Extensions
    {
        public static EcsSystems AddHitSystems(this EcsSystems systems)
        {
            return systems
                .Add(new LightImpactHitSystem())
                .Add(new CuttingHitSystem())
                .Add(new HeavyImpactHitSystem())
                .Add(new SmallCaliberHitSystem())
                .Add(new ShotgunHitSystem())
                .Add(new MediumCaliberHitSystem())
                .Add(new HighCaliberHitSystem())
                .Add(new ExplosionHitSystem());
        }
        
        public static EcsSystems AddDamageSystems(this EcsSystems systems)
        {
            return systems
                .Add(new BodyHitSystem())
                .Add(new LightImpactDamageSystem())
                .Add(new CuttingDamageSystem())
                .Add(new HeavyImpactDamageSystem())
                .Add(new SmallCaliberDamageSystem())
                .Add(new ShotgunDamageSystem())
                .Add(new MediumCaliberDamageSystem())
                .Add(new HighCaliberDamageSystem())
                .Add(new ExplosionDamageSystem());
        }
        
        public static EcsSystems AddWoundSystems(this EcsSystems systems)
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

        public static EcsSystems AddPainSystems(this EcsSystems systems)
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