using System;
using GTA;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class HelmetRequestSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        
        private EcsFilter<HelmetRequestComponent> _requests;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(HelmetRequestSystem);
            
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
                    player.Character.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, Random.Next(0, 20));
                }
                else
                {
                    var message = _ecsWorld.CreateEntityWith<NotificationComponent>();
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