using System;
using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class HelmetRequestSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        
        private EcsFilter<AddHelmetToPlayerEvent> _requests;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(HelmetRequestSystem);
#endif
            
            if(_requests.EntitiesCount == 0) return;

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
                    message.StringToShow = "You don't have enough money to buy helmet";
                    message.Level = NotifyLevels.COMMON;
                }
            }

            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                _ecsWorld.RemoveEntity(_requests.Entities[i]);
            }
        }
    }
}