using GTA;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using Leopotam.Ecs;

namespace GunshotWound2.World
{
    [EcsInject]
    public sealed class RemoveWoundedPedSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent, NpcMarkComponent> _npcs;

        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<GswWorld> _world;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(RemoveWoundedPedSystem);
#endif

            RemovePeds();
        }

        private void RemovePeds()
        {
            for (int pedIndex = 0; pedIndex < _npcs.EntitiesCount; pedIndex++)
            {
                WoundedPedComponent woundedPed = _npcs.Components1[pedIndex];
                if (woundedPed.IsPlayer) return;

                var ped = woundedPed.ThisPed;
                if (ped.IsAlive && !OutRemoveRange(ped)) continue;

                _world.Data.GswPeds.Remove(ped);
                woundedPed.ThisPed = null;
                _ecsWorld.RemoveComponent<PainComponent>(_npcs.Entities[pedIndex], true);
                _ecsWorld.RemoveEntity(_npcs.Entities[pedIndex]);

#if DEBUG
                ped.CurrentBlip.Remove();
#endif
            }
        }

        private bool OutRemoveRange(Ped ped)
        {
            var removeRange = _config.Data.NpcConfig.RemovePedRange;
            return GTA.World.GetDistance(Game.Player.Character.Position, ped.Position) > removeRange;
        }
    }
}