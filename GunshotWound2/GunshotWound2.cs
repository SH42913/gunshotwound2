using System;
using System.Diagnostics;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using GunshotWound2.Systems.HitSystems;
using GunshotWound2.Systems.HitSystems.BodyDamageSystems;
using GunshotWound2.Systems.HitSystems.WeaponDamageSystems;
using GunshotWound2.Systems.HitSystems.WeaponHitSystems;
using GunshotWound2.Systems.NpcSystems;
using GunshotWound2.Systems.PlayerSystems;
using GunshotWound2.Systems.UiSystems;
using GunshotWound2.Systems.WoundSystems;
using GunshotWound2.Systems.WoundSystems.CriticalWoundSystems;
using LeopotamGroup.Ecs;

namespace GunshotWound2
{
    public class GunshotWound2 : Script
    {
        private EcsWorld _ecsWorld;
        private EcsSystems _updateSystems;
        private MainConfig _mainConfig;
        private PlayerConfig _playerConfig;
        private NpcConfig _npcConfig;
        private WoundConfig _woundConfig;
        private Stopwatch _debugStopwatch;
        public static string LastSystem = "Nothing";
        private bool _messageWasShown;
        
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
                case Keys.T:
                    Test();
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
            _playerConfig = EcsFilterSingle<PlayerConfig>.Create(_ecsWorld);
            _npcConfig = EcsFilterSingle<NpcConfig>.Create(_ecsWorld);
            _woundConfig = EcsFilterSingle<WoundConfig>.Create(_ecsWorld);
            LoadConfigsFromXml();
            
            _updateSystems = new EcsSystems(_ecsWorld);
            
            if (_npcConfig.AddingPedRange > 1f)
            {
                _updateSystems
                    .Add(new NpcSystem());
            }

            if (_playerConfig.WoundedPlayerEnabled)
            {
                _updateSystems
                    .Add(new PlayerSystem());
            }
            
            _updateSystems
                .AddHitSystems()
                .AddDamageSystems()
                .AddWoundSystems()
                .Add(new HitCleanSystem())
                .Add(new HelmetRequestSystem())
                .Add(new CheckPedSystem())
                .Add(new NotificationSystem());
            
            _updateSystems.Initialize();
            
            Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.1f);
            Tick += OnTick;
            KeyUp += OnKeyUp;
            
            _debugStopwatch = new Stopwatch();
        }

        private void LoadConfigsFromXml()
        {
            _mainConfig.Debug = false;
            _mainConfig.TicksToRefresh = 30;

            _playerConfig.WoundedPlayerEnabled = true;
            _playerConfig.MoneyForHelmet = 30;
            _playerConfig.MaximalHealth = 100;
            _playerConfig.MinimalHealth = 0;
            _playerConfig.MaximalPain = 100;

            _npcConfig.AddingPedRange = 5f;
            _npcConfig.RemovePedRange = _npcConfig.AddingPedRange * 2f;
            _npcConfig.ShowEnemyCriticalMessages = true;

            _woundConfig.StopBleedingAmount = 0.005f;
            _woundConfig.MoveRateOnNervesDamage = 0.8f;
            
            if(!_mainConfig.Debug) return;
            UI.Notify("Configs:\n" +
                      $"{_mainConfig}\n" +
                      $"{_playerConfig}\n" +
                      $"{_npcConfig}\n" +
                      $"{_woundConfig}\n");
        }

        private void GunshotWoundTick()
        {
            _debugStopwatch.Restart();
            _updateSystems.Run();
            _debugStopwatch.Stop();
            
            if (_mainConfig.TicksToRefresh < 90 &&_debugStopwatch.ElapsedMilliseconds > 40)
            {
                _mainConfig.TicksToRefresh++;
            }
            else if(_mainConfig.TicksToRefresh > 2 && _debugStopwatch.ElapsedMilliseconds < 20)
            {
                _mainConfig.TicksToRefresh--;
                _messageWasShown = false;
            }
            else if(_mainConfig.TicksToRefresh == 90)
            {
                if(!_messageWasShown) 
                {
                    SendMessage("You better to reduce adding range " +
                            "or turn off WoundedPeds!", NotifyLevels.WARNING);
                    _messageWasShown = true;
                }           
            }

            if (!_mainConfig.Debug) return;
            string debugSubtitles = $"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}/{_mainConfig.TicksToRefresh}\n" +
                                    $"Peds: {_ecsWorld.GetFilter<EcsFilter<WoundedPedComponent>>().EntitiesCount}\n" +
                                    $"Ms: {_debugStopwatch.ElapsedMilliseconds}";
            UI.ShowSubtitle(debugSubtitles);
        }

        private void ReduceRange(float value)
        {
            if(_npcConfig.AddingPedRange <= value) return;
            _npcConfig.AddingPedRange -= value;
            _npcConfig.RemovePedRange = _npcConfig.AddingPedRange * 2f;
            
            SendMessage($"Adding Range: {_npcConfig.AddingPedRange}\n" +
                        $"Removing Range: {_npcConfig.RemovePedRange}");
        }

        private void IncreaseRange(float value)
        {
            _npcConfig.AddingPedRange += value;
            _npcConfig.RemovePedRange = _npcConfig.AddingPedRange * 2f;
            
            SendMessage($"Adding Range: {_npcConfig.AddingPedRange}\n" +
                        $"Removing Range: {_npcConfig.RemovePedRange}");
        }

        private void Test()
        {
            _ecsWorld.CreateEntityWith<CheckPedComponent>().PedEntity = _playerConfig.PlayerEntity;
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
                .Add(new OtherHitSystem())
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
                .Add(new BodyDamageSystem())
                .Add(new OtherDamageSystem())
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
                .Add(new InstantDamageSystem())
                .Add(new BleedingSystem())
                .Add(new PainSystem())
                .Add(new ArmsCriticalSystem())
                .Add(new LegsCriticalSystem())
                .Add(new HeartCriticalSystem())
                .Add(new LungsCriticalSystem())
                .Add(new NervesCriticalSystem())
                .Add(new GutsCriticalSystem())
                .Add(new StomachCriticalSystem());
        }
    }
}