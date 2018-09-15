using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PedSystems
{
    [EcsInject]
    public class ArmorSystem : IEcsRunSystem
    {
        private EcsFilter<WoundedPedComponent> _peds;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(ArmorSystem);
#endif
            
            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                var ped = _peds.Components1[i];
                if(ped.Armor == 0) continue;
                if (ped.Armor < 0)
                {
                    ped.Armor = 0;
                }

                ped.ThisPed.Armor = ped.Armor;
            }
        }
    }
}