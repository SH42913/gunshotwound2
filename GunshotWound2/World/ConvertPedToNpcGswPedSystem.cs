using System;
using GTA;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using Leopotam.Ecs;

namespace GunshotWound2.World
{
    [EcsInject]
    public sealed class ConvertPedToNpcGswPedSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<ConvertPedToNpcGswPedEvent> _requests;

        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<GswWorld> _world;

        private static readonly Random Random = new Random();

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(ConvertPedToNpcGswPedSystem);
#endif

            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                ProcessRequest(_requests.Components1[i]);
                _requests.Components1[i].PedsToAdd = null;
                _ecsWorld.RemoveEntity(_requests.Entities[i]);
            }
        }

        private void ProcessRequest(ConvertPedToNpcGswPedEvent request)
        {
            while (request.PedsToAdd.Count > 0)
            {
                Ped pedToConvert = request.PedsToAdd.Dequeue();

                int entity = _ecsWorld.CreateEntityWith(out NpcMarkComponent _, out WoundedPedComponent woundedPed);

                woundedPed.ThisPed = pedToConvert;
                woundedPed.IsMale = pedToConvert.Gender == Gender.Male;
                woundedPed.IsDead = false;
                woundedPed.IsPlayer = false;
                var newHealth = Random.Next(
                    _config.Data.NpcConfig.MinStartHealth,
                    _config.Data.NpcConfig.MaxStartHealth);
                woundedPed.Health = newHealth;
                woundedPed.Armor = pedToConvert.Armor;
                woundedPed.ThisPed.MaxHealth = newHealth;

                woundedPed.ThisPed.CanWrithe = false;
                woundedPed.ThisPed.CanWearHelmet = true;
                woundedPed.ThisPed.AlwaysDiesOnLowHealth = false;
                woundedPed.ThisPed.CanSufferCriticalHits = true;
                woundedPed.ThisPed.CanSwitchWeapons = true;
                woundedPed.ThisPed.CanBeShotInVehicle = true;
                if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, pedToConvert))
                {
                }

                woundedPed.StopBleedingAmount = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalBleedStopSpeed / 2,
                    _config.Data.NpcConfig.MaximalBleedStopSpeed);

                if (_config.Data.NpcConfig.MinAccuracy > 0 && _config.Data.NpcConfig.MaxAccuracy > 0)
                {
                    pedToConvert.Accuracy = Random.Next(_config.Data.NpcConfig.MinAccuracy,
                        _config.Data.NpcConfig.MaxAccuracy + 1);
                }

                if (_config.Data.NpcConfig.MinShootRate > 0 && _config.Data.NpcConfig.MaxShootRate > 0)
                {
                    pedToConvert.ShootRate = Random.Next(_config.Data.NpcConfig.MinShootRate,
                        _config.Data.NpcConfig.MaxShootRate);
                }

                woundedPed.DefaultAccuracy = pedToConvert.Accuracy;

                woundedPed.MaximalPain = Random.NextFloat(
                    _config.Data.NpcConfig.LowerMaximalPain,
                    _config.Data.NpcConfig.UpperMaximalPain);
                woundedPed.PainRecoverSpeed = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed / 2,
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed);

                woundedPed.Crits = 0;
                woundedPed.BleedingCount = 0;
                woundedPed.MostDangerBleedingEntity = null;

                _ecsWorld.CreateEntityWith<NoPainChangeStateEvent>().Entity = entity;

                _world.Data.GswPeds.Add(pedToConvert, entity);

#if DEBUG
                pedToConvert.AddBlip();
#endif
            }
        }
    }
}