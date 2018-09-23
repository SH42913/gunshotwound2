using GTA;
using GunshotWound2.Components.MarkComponents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.NpcSystems
{
    [EcsInject]
    public class RemoveWoundedPedSystem : IEcsRunSystem
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
                if(_npcs.Components1[pedIndex].IsPlayer) return;
                
                var ped = _npcs.Components1[pedIndex].ThisPed;
                if(ped.IsAlive && !OutRemoveRange(ped)) continue;

                _npcs.Components1[pedIndex].ThisPed = null;
                _ecsWorld.RemoveEntity(_npcs.Entities[pedIndex]);
                _world.Data.GswPeds.Remove(ped);

#if DEBUG
                ped.CurrentBlip.Remove();
#endif
            }
        }

        private bool OutRemoveRange(Ped ped)
        {
            var removeRange = _config.Data.NpcConfig.RemovePedRange;
            return World.GetDistance(Game.Player.Character.Position, ped.Position) > removeRange;
        }
    }
}