using GTA;
using GunshotWound2.Configs;
using GunshotWound2.GUI;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class HelmetRequestSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<LocaleConfig> _locale = null;
        private readonly EcsFilter<AddHelmetToPlayerEvent> _requests = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(HelmetRequestSystem);
#endif

            if (_requests.EntitiesCount <= 0) return;

            var player = Game.Player;
            var ped = player.Character;
            if (ped.IsWearingHelmet)
            {
                ped.RemoveHelmet(false);
            }
            else if (!ped.IsRagdoll)
            {
                if (player.Money > _config.Data.PlayerConfig.MoneyForHelmet)
                {
                    player.Money -= _config.Data.PlayerConfig.MoneyForHelmet;
                    ped.GiveHelmet(false, Helmet.RegularMotorcycleHelmet, GunshotWound2.Random.Next(0, 15));
                }
                else
                {
                    var message = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
                    message.StringToShow = _locale.Data.DontHaveMoneyForHelmet;
                    message.Level = NotifyLevels.COMMON;
                }
            }

            _requests.CleanFilter();
        }
    }
}