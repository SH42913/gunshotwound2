using GTA;
using GTA.Native;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class PainRecoverySystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent> _peds;
        private EcsFilterSingle<MainConfig> _config;
        private uint _ticks;
        private float _lastTime;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(PainRecoverySystem);
            
            var ticksToRefresh = _config.Data.TicksToRefresh;
            _lastTime += Game.LastFrameTime;
            if(++_ticks % ticksToRefresh != 0) return;
            
            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                var woundedPed = _peds.Components1[i];
                if(woundedPed.PainMeter <= 0) continue;
                
                woundedPed.PainMeter -= woundedPed.PainRecoverSpeed * _lastTime;
                var painPercent = woundedPed.PainMeter / woundedPed.MaximalPain;
                var backPercent = painPercent > 1
                    ? 0
                    : 1 - painPercent;
                if (woundedPed.PainMeter < 0) woundedPed.PainMeter = 0;

                if (painPercent > 1 && !woundedPed.ThisPed.IsRagdoll && !woundedPed.GivesInToPain)
                {
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, -1, -1, 0, 0, 0, 0);
                    woundedPed.GivesInToPain = true;
                    SendDebug($"Send this ped to ragdoll with {painPercent}");
                    if (woundedPed.IsPlayer)
                    {
                        Game.Player.IgnoredByEveryone = true;
                        if (_config.Data.PlayerConfig.PoliceCanForgetYou) Game.Player.WantedLevel = 0;
                        if (!woundedPed.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED) && !woundedPed.IsDead)
                        {
                            SendMessage("You can't take this pain anymore!\n" +
                                        "You lose consciousness!", NotifyLevels.WARNING);
                        }
                    }
                    continue;
                }

                if (woundedPed.GivesInToPain)
                {
                    if (painPercent < 0.75f && 
                        !(_config.Data.WoundConfig.RealisticNervesDamage &&
                          woundedPed.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)))    
                    {
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 1, 1, 1, 0, 0, 0);
                        woundedPed.GivesInToPain = false;
                        SendDebug("Recover this ped from ragdoll");
                        if(woundedPed.ThisPed.IsPlayer) Game.Player.IgnoredByEveryone = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (!woundedPed.DamagedParts.HasFlag(DamageTypes.LEGS_DAMAGED))
                {
                    var moveRate = _config.Data.WoundConfig.MoveRateOnFullPain +
                                   (1 - _config.Data.WoundConfig.MoveRateOnFullPain) * backPercent;
                    Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
                }

                if (woundedPed.DamagedParts.HasFlag(DamageTypes.ARMS_DAMAGED)) continue;
                
                if (woundedPed.IsPlayer)
                {
                    Function.Call(Hash._SET_CAM_EFFECT, painPercent > 0.5f 
                        ? 2 
                        : 0);
                }
                else
                {
                    woundedPed.ThisPed.Accuracy = (int) (backPercent * woundedPed.DefaultAccuracy);
                }
            }

            _lastTime = 0;
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
        
        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}