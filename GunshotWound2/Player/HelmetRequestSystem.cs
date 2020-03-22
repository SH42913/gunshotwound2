using System;
using GTA;
using GunshotWound2.Configs;
using GunshotWound2.GUI;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public class HelmetRequestSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<LocaleConfig> _locale;

        private EcsFilter<AddHelmetToPlayerEvent> _requests;

        private static readonly Random Random = new Random();

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(HelmetRequestSystem);
#endif

            if (_requests.EntitiesCount <= 0) return;

            var player = Game.Player;
            if (player.Character.IsWearingHelmet)
            {
                player.Character.RemoveHelmet(false);
            }
            else
            {
                if (player.Money > _config.Data.PlayerConfig.MoneyForHelmet)
                {
                    player.Money -= _config.Data.PlayerConfig.MoneyForHelmet;
                    player.Character.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, Random.Next(0, 15));
                }
                else
                {
                    var message = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
                    message.StringToShow = _locale.Data.DontHaveMoneyForHelmet;
                    message.Level = NotifyLevels.COMMON;
                }
            }

            _requests.RemoveAllEntities();
        }
    }
}