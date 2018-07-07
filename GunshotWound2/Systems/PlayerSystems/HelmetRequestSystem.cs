using System;
using GTA;
using GunshotWoundEcs.Components.PlayerComponents;
using GunshotWoundEcs.Components.UiComponents;
using GunshotWoundEcs.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.PlayerSystems
{
    [EcsInject]
    public class HelmetRequestSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<PlayerConfig> _config;
        private EcsFilter<HelmetRequestComponent> _requests;
        private static readonly Random _random = new Random();
        
        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(HelmetRequestSystem);
            if(_requests.EntitiesCount == 0) return;

            var player = Game.Player;
            if (player.Character.IsWearingHelmet)
            {
                player.Character.RemoveHelmet(false);
            }
            else
            {
                if (player.Money > _config.Data.MoneyForHelmet)
                {
                    player.Money -= _config.Data.MoneyForHelmet;
                    player.Character.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, _random.Next(0, 20));
                }
                else
                {
                    var message = _ecsWorld
                        .CreateEntityWith<NotificationComponent>();
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