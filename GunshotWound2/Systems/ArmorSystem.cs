using GunshotWound2.Components.WoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems
{
    [EcsInject]
    public class ArmorSystem : IEcsRunSystem
    {
        private EcsFilter<WoundedPedComponent> _peds;
        
        public void Run()
        {
            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                var ped = _peds.Components1[i];
                if(ped.Armor == 0) continue;
                if (ped.Armor < 0)
                {
                    ped.Armor = 0;
                    ped.ThisPed.Armor = 0;
                    continue;
                }

                ped.ThisPed.Armor = ped.Armor;
            }
        }
    }
}