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
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<WoundedPedComponent, NpcMarkComponent> _npcs = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<GswWorld> _world = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(RemoveWoundedPedSystem);
#endif

            RemovePeds();
        }

        private void RemovePeds()
        {
            for (var pedIndex = 0; pedIndex < _npcs.EntitiesCount; pedIndex++)
            {
                var woundedPed = _npcs.Components1[pedIndex];
                if (woundedPed.IsPlayer) return;

                var ped = woundedPed.ThisPed;
                if (ped.IsAlive && !OutRemoveRange(ped)) continue;

                _world.Data.GswPeds.Remove(ped);
                woundedPed.ThisPed = null;
                _ecsWorld.RemoveComponent<PainComponent>(_npcs.Entities[pedIndex], true);
                _ecsWorld.RemoveEntity(_npcs.Entities[pedIndex]);

#if DEBUG
                ped.AttachedBlip.Delete();
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