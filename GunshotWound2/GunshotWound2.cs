using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
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

namespace GunshotWound2
{
    public sealed class GunshotWound2 : Script
    {
        public static string LastSystem = "Nothing";

        public const float MinimalRangeForWoundedPeds = 0;
        public const float AddingToRemovingMultiplier = 2;

        public static readonly Random Random = new Random();

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
            GunshotWoundInit();
        }

        private void GunshotWoundInit()
        {
            Function.Call(Hash._SET_CAM_EFFECT, 0);
            _ecsWorld = new EcsWorld();

            _mainConfig = EcsFilterSingle<MainConfig>.Create(_ecsWorld);
            _localeConfig = EcsFilterSingle<LocaleConfig>.Create(_ecsWorld);
            _gswWorld = EcsFilterSingle<GswWorld>.Create(_ecsWorld);
            _gswWorld.GswPeds = new Dictionary<Ped, int>();

            (_configLoaded, _configReason) = MainConfig.TryToLoadFromXml(_mainConfig);
            (_localizationLoaded, _localizationReason) = LocaleConfig.TryToLoadLocalization(_localeConfig, _mainConfig.Language);

            _everyFrameSystems = new EcsSystems(_ecsWorld);
            _commonSystems = new MultiTickEcsSystems(_ecsWorld, MultiTickEcsSystems.RestrictionModes.MILLISECONDS, 10);

            if (_mainConfig.NpcConfig.AddingPedRange > MinimalRangeForWoundedPeds)
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
                    _everyFrameSystems.Add(new AdrenalineSystem());
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

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyCode == _mainConfig.HelmetKey)
            {
                _ecsWorld.CreateEntityWith<AddHelmetToPlayerEvent>();
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.CheckKey)
            {
                CheckPlayer();
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.HealKey)
            {
                HealPlayer();
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.BandageKey)
            {
                ApplyBandageToPlayer();
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.IncreaseRangeKey)
            {
                ChangeRange(5);
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.ReduceRangeKey)
            {
                ChangeRange(-5);
                return;
            }

            if (eventArgs.KeyCode == _mainConfig.PauseKey)
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
            IsInitCheck();

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
                Notification.Show($"Last system is {LastSystem}");
#endif
            }
        }

        private void IsInitCheck()
        {
            if (_isInit || _ticks++ != 400) return;

            var translationAuthor = _localeConfig.LocalizationAuthor ?? "GSW2-community";

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

        private void GunshotWoundTick()
        {
            if (_isPaused) return;

            if (_mainConfig.NpcConfig.AddingPedRange > MinimalRangeForWoundedPeds)
            {
                Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 0.01f);
            }

            if (_mainConfig.PlayerConfig.WoundedPlayerEnabled)
            {
                Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
            }

            _everyFrameSystems.Run();
            _commonSystems.Run();

#if DEBUG
            Screen.ShowSubtitle($"ActiveEntities: {_ecsWorld.GetStats().ActiveEntities}\n" +
                                $"Peds in GSW: {_gswWorld.GswPeds.Count}");
#endif
        }

        private void ChangeRange(float value)
        {
            if (_mainConfig.NpcConfig.AddingPedRange + value < MinimalRangeForWoundedPeds) return;

            _mainConfig.NpcConfig.AddingPedRange += value;
            _mainConfig.NpcConfig.RemovePedRange = _mainConfig.NpcConfig.AddingPedRange * AddingToRemovingMultiplier;

            if (_mainConfig.NpcConfig.AddingPedRange <= MinimalRangeForWoundedPeds)
            {
                Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player, 1f);
            }

            SendMessage($"{_localeConfig.AddingRange}: {_mainConfig.NpcConfig.AddingPedRange.ToString("F0")}\n" +
                        $"{_localeConfig.RemovingRange}: {_mainConfig.NpcConfig.RemovePedRange.ToString("F0")}");
        }

        private void CheckPlayer()
        {
            var playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
        }

        private void HealPlayer()
        {
            var playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<InstantHealEvent>().Entity = playerEntity;
        }

        private void ApplyBandageToPlayer()
        {
            var playerEntity = _mainConfig.PlayerConfig.PlayerEntity;
            if (playerEntity < 0) return;

            _ecsWorld.CreateEntityWith<ApplyBandageEvent>().Entity = playerEntity;
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
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
                .Add(new BaseWeaponHitSystem())
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
    }
}