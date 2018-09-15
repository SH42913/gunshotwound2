using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.PlayerEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class SpecialAbilityLockSystem : IEcsRunSystem
    {
        private EcsFilter<ChangeSpecialAbilityEvent> _events;
        
        public void Run()
        {
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                if (_events.Components1[i].Lock)
                {
                    Function.Call(Hash._0x6A09D0D590A47D13, Game.Player.Character.Model.Hash);
                }
                else
                {
                    Function.Call(Hash._0xD6A953C6D1492057, Game.Player.Character.Model.Hash);
                    Function.Call(Hash._0xF145F3BE2EFA9A3B, Game.Player.Character.Model.Hash);
                }
            }
            
            _events.RemoveAllEntities();
        }
    }
}