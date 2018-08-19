using System;
using GTA;
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
        private EcsFilter<ConvertPedsToWoundedPedsEvent> _requests;
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

        private void ProcessRequest(ConvertPedsToWoundedPedsEvent request)
        {
            for (int i = 0; i < request.PedsInRange.Length; i++)
            {
                Ped pedToConvert = request.PedsInRange[i];
                
                NpcMarkComponent npcComponent;
                WoundedPedComponent woundedPed;
                _ecsWorld.CreateEntityWith(out npcComponent, out woundedPed);
                
                woundedPed.ThisPed = pedToConvert;

                var newHealth = Random.Next(
                    _config.Data.NpcConfig.LowerStartHealth,
                    _config.Data.NpcConfig.UpperStartHealth);
                woundedPed.Health = newHealth;
                woundedPed.Armor = pedToConvert.Armor;
                woundedPed.ThisPed.MaxHealth = newHealth;
                woundedPed.ThisPed.Health = newHealth;
                woundedPed.ThisPed.CanWrithe = false;
                woundedPed.ThisPed.AlwaysDiesOnLowHealth = false;

                woundedPed.StopBleedingAmount = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalBleedStopSpeed/2,
                    _config.Data.NpcConfig.MaximalBleedStopSpeed);
                woundedPed.DefaultAccuracy = pedToConvert.Accuracy;
                
                woundedPed.HeShe = pedToConvert.Gender == Gender.Male
                    ? "He"
                    : "She";
                woundedPed.HisHer = pedToConvert.Gender == Gender.Male
                    ? "His"
                    : "Her";
                
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
    }
}