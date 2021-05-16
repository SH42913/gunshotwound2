using GTA;
using GTA.Native;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class SpecialAbilityLockSystem : IEcsRunSystem
    {
        private readonly EcsFilter<ChangeSpecialAbilityEvent> _events = null;

        public void Run()
        {
            for (var i = 0; i < _events.EntitiesCount; i++)
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

            _events.CleanFilter();
        }
    }
}