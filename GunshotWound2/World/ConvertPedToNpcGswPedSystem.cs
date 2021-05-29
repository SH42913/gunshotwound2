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
            var npcConfig = _config.Data.NpcConfig;

            while (request.PedsToAdd.Count > 0)
            {
                var pedToConvert = request.PedsToAdd.Dequeue();

                var entity = _ecsWorld.CreateEntityWith(out NpcMarkComponent _, out WoundedPedComponent woundedPed);

                woundedPed.ThisPed = pedToConvert;
                woundedPed.IsMale = pedToConvert.Gender == Gender.Male;
                woundedPed.IsDead = false;
                woundedPed.IsPlayer = false;
                woundedPed.Armor = pedToConvert.Armor;
                woundedPed.Health = npcConfig.GetRandomHealth();
                woundedPed.PedHealth = woundedPed.Health;
                woundedPed.PedMaxHealth = woundedPed.Health;

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
                    npcConfig.MaximalBleedStopSpeed / 2,
                    npcConfig.MaximalBleedStopSpeed);

                if (npcConfig.MinAccuracy > 0 && npcConfig.MaxAccuracy > 0)
                {
                    pedToConvert.Accuracy = GunshotWound2.Random.Next(npcConfig.MinAccuracy, npcConfig.MaxAccuracy + 1);
                }

                if (npcConfig.MinShootRate > 0 && npcConfig.MaxShootRate > 0)
                {
                    pedToConvert.ShootRate = GunshotWound2.Random.Next(npcConfig.MinShootRate, npcConfig.MaxShootRate);
                }

                woundedPed.DefaultAccuracy = pedToConvert.Accuracy;

                woundedPed.MaximalPain = GunshotWound2.Random.NextFloat(
                    npcConfig.LowerMaximalPain,
                    npcConfig.UpperMaximalPain);
                woundedPed.PainRecoverSpeed = GunshotWound2.Random.NextFloat(
                    npcConfig.MaximalPainRecoverSpeed / 2,
                    npcConfig.MaximalPainRecoverSpeed);

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