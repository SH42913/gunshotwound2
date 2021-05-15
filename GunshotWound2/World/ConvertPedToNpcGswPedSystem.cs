using GTA;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.World
{
    [EcsInject]
    public sealed class ConvertPedToNpcGswPedSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<ConvertPedToNpcGswPedEvent> _requests = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<GswWorld> _world = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(ConvertPedToNpcGswPedSystem);
#endif

            for (var i = 0; i < _requests.EntitiesCount; i++)
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
                var pedToConvert = request.PedsToAdd.Dequeue();

                var entity = _ecsWorld.CreateEntityWith(out NpcMarkComponent _, out WoundedPedComponent woundedPed);

                woundedPed.ThisPed = pedToConvert;
                woundedPed.IsMale = pedToConvert.Gender == Gender.Male;
                woundedPed.IsDead = false;
                woundedPed.IsPlayer = false;
                var newHealth = GunshotWound2.Random.Next(
                    _config.Data.NpcConfig.MinStartHealth,
                    _config.Data.NpcConfig.MaxStartHealth);
                woundedPed.Health = newHealth + 100;
                woundedPed.Armor = pedToConvert.Armor;
                woundedPed.ThisPed.MaxHealth = newHealth;

                woundedPed.ThisPed.CanWrithe = false;
                woundedPed.ThisPed.CanWearHelmet = true;
                woundedPed.ThisPed.DiesOnLowHealth = false;
                woundedPed.ThisPed.CanSufferCriticalHits = true;
                woundedPed.ThisPed.CanSwitchWeapons = true;
                woundedPed.ThisPed.CanBeShotInVehicle = true;
                if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, pedToConvert))
                {
                }

                woundedPed.StopBleedingAmount = GunshotWound2.Random.NextFloat(
                    _config.Data.NpcConfig.MaximalBleedStopSpeed / 2,
                    _config.Data.NpcConfig.MaximalBleedStopSpeed);

                if (_config.Data.NpcConfig.MinAccuracy > 0 && _config.Data.NpcConfig.MaxAccuracy > 0)
                {
                    pedToConvert.Accuracy = GunshotWound2.Random.Next(_config.Data.NpcConfig.MinAccuracy,
                        _config.Data.NpcConfig.MaxAccuracy + 1);
                }

                if (_config.Data.NpcConfig.MinShootRate > 0 && _config.Data.NpcConfig.MaxShootRate > 0)
                {
                    pedToConvert.ShootRate = GunshotWound2.Random.Next(_config.Data.NpcConfig.MinShootRate,
                        _config.Data.NpcConfig.MaxShootRate);
                }

                woundedPed.DefaultAccuracy = pedToConvert.Accuracy;

                woundedPed.MaximalPain = GunshotWound2.Random.NextFloat(
                    _config.Data.NpcConfig.LowerMaximalPain,
                    _config.Data.NpcConfig.UpperMaximalPain);
                woundedPed.PainRecoverSpeed = GunshotWound2.Random.NextFloat(
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