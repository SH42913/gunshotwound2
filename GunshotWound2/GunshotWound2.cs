using System;
using System.Diagnostics;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GunshotWound2.Components;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using GunshotWound2.Systems;
using GunshotWound2.Systems.DamageSystems;
using GunshotWound2.Systems.HitSystems;
using GunshotWound2.Systems.HitSystems.WeaponHitSystems;
using GunshotWound2.Systems.NpcSystems;
using GunshotWound2.Systems.PlayerSystems;
using GunshotWound2.Systems.UiSystems;
using GunshotWound2.Systems.WoundSystems;
using GunshotWound2.Systems.WoundSystems.CriticalWoundSystems;
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
                UI.Notify("~r~Error on init: \n" +
                          $"In {LastSystem}\n" +
                          $"{exception.Message}");
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            switch (eventArgs.KeyCode)
            {
                case Keys.PageDown:
                    ReduceRange(5);
                    break;
                case Keys.PageUp:
                    IncreaseRange(5);
                    break;
                case Keys.H:
                    _ecsWorld.CreateEntityWith<HelmetRequestComponent>();
                    break;
                case Keys.I:
                    CheckPlayer();
                    break;
                case Keys.J:
                    HealPlayer();
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
            
            if (_mainConfig.NpcConfig.AddingPedRange > 1f)
            {
                _updateSystems
                    .Add(new NpcSystem());
            }

            if (_mainConfig.PlayerConfig.WoundedPlayerEnabled)
            {
                _updateSystems
                    .Add(new PlayerSystem());
            }
            
            _updateSystems
                .Add(new HealSystem())
                .AddHitSystems()
                .AddDamageSystems()
                .AddWoundSystems()
                .Add(new HitCleanSystem())
                .Add(new HelmetRequestSystem())
                .Add(new DebugInfoSystem())
                .Add(new CheckSystem())
                .Add(new NotificationSystem())
                .Add(new AdrenalineSystem())
                .Add(new ArmorSystem());
            
            _updateSystems.Initialize();
            
            Tick += OnTick;
            KeyUp += OnKeyUp;
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.001f);
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
            
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

            _mainConfig.PlayerConfig = new PlayerConfig
            {
                WoundedPlayerEnabled = true,
                MoneyForHelmet = 30,
                MaximalHealth = 99,
                MinimalHealth = 0,
                MaximalPain = 100,
                PainRecoverSpeed = 2f,
                BleedHealingSpeed = 0.001f,
                PlayerEntity = -1
            };

            _mainConfig.NpcConfig = new NpcConfig
            {
                AddingPedRange = 50f,
                RemovePedRange = 100f,
                ShowEnemyCriticalMessages = true,
                MaximalHealth = 100,
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
                
                SendMessage("Perfomance drop!\nYou better to reduce adding range " +
                            "or turn off WoundedPeds!", NotifyLevels.WARNING);
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
            
            _ecsWorld.CreateEntityWith<HealComponent>().PedEntity = playerEntity;
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
                .Add(new HighCaliberHitSystem());
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
                .Add(new HighCaliberDamageSystem());
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
                .Add(new BleedingSystem())
                .Add(new PainSystem())
                .Add(new PainRecoverySystem());
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