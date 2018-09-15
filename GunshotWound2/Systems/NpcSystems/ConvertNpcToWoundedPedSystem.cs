using System;
using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.NpcEvents;
using GunshotWound2.Components.MarkComponents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.NpcSystems
{
    [EcsInject]
    public class ConvertNpcToWoundedPedSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<ConvertPedToWoundedPedEvent> _requests;
        
        private EcsFilterSingle<MainConfig> _config;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(ConvertNpcToWoundedPedSystem);
#endif

            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                ProcessRequest(_requests.Components1[i]);
                _requests.Components1[i].PedsInRange = null;
                _ecsWorld.RemoveEntity(_requests.Entities[i]);
            }
        }

        private void ProcessRequest(ConvertPedToWoundedPedEvent request)
        {
            for (int i = 0; i < request.PedsInRange.Length; i++)
            {
                Ped pedToConvert = request.PedsInRange[i];

                _ecsWorld.CreateEntityWith(out NpcMarkComponent _, out WoundedPedComponent woundedPed);
                
                woundedPed.ThisPed = pedToConvert;
                woundedPed.IsMale = pedToConvert.Gender == Gender.Male;

                var newHealth = Random.Next(
                    _config.Data.NpcConfig.MinStartHealth,
                    _config.Data.NpcConfig.MaxStartHealth);
                woundedPed.Health = newHealth;
                woundedPed.Armor = pedToConvert.Armor;
                woundedPed.ThisPed.MaxHealth = newHealth;
                woundedPed.ThisPed.Health = newHealth;
                woundedPed.ThisPed.CanWrithe = false;
                woundedPed.ThisPed.AlwaysDiesOnLowHealth = false;

                woundedPed.StopBleedingAmount = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalBleedStopSpeed/2,
                    _config.Data.NpcConfig.MaximalBleedStopSpeed);

                if (_config.Data.NpcConfig.MinAccuracy > 0 && _config.Data.NpcConfig.MaxAccuracy > 0)
                {
                    int old = pedToConvert.Accuracy;
                    pedToConvert.Accuracy = Random.Next(_config.Data.NpcConfig.MinAccuracy, _config.Data.NpcConfig.MaxAccuracy + 1);
                }
                if (_config.Data.NpcConfig.MinShootRate > 0 && _config.Data.NpcConfig.MaxShootRate > 0)
                {
                    pedToConvert.ShootRate = Random.Next(_config.Data.NpcConfig.MinShootRate, _config.Data.NpcConfig.MaxShootRate);
                }
                woundedPed.DefaultAccuracy = pedToConvert.Accuracy;
                
                woundedPed.PainMeter = 0;
                woundedPed.MaximalPain = Random.NextFloat(
                    _config.Data.NpcConfig.LowerMaximalPain,
                    _config.Data.NpcConfig.UpperMaximalPain);
                woundedPed.PainRecoverSpeed = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed/2,
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed);

#if DEBUG
                pedToConvert.AddBlip();       
#endif
            }
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}