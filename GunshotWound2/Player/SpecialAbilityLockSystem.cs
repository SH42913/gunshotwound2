using GTA;
using GTA.Native;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public class SpecialAbilityLockSystem : IEcsRunSystem
    {
        private EcsFilter<ChangeSpecialAbilityEvent> _events;

        public void Run()
        {
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                if (Function.Call<bool>(Hash.IS_SPECIAL_ABILITY_ACTIVE, Game.Player))
                {
                    Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE_FAST, Game.Player);
                }

                Function.Call(_events.Components1[i].Lock
                        ? Hash.SPECIAL_ABILITY_LOCK
                        : Hash.SPECIAL_ABILITY_UNLOCK,
                    Game.Player.Character.Model.Hash);
            }

            _events.RemoveAllEntities();
        }
    }
}