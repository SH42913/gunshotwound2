using GTA;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class PlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<PlayerConfig> _config;
        private EcsFilter<WoundedPedComponent, PlayerComponent> _playerComponents;

        public void Initialize()
        {
            if(!_config.Data.WoundedPlayerEnabled) return;

            var ped = Game.Player.Character;
            
            var entity = _ecsWorld.CreateEntity();
            _ecsWorld.AddComponent<PlayerComponent>(entity);
            
            var woundPed = _ecsWorld.AddComponent<WoundedPedComponent>(entity);
            woundPed.ThisPed = ped;
            woundPed.Armor = ped.Armor;
            woundPed.IsPlayer = true;
            woundPed.IsMale = ped.Gender == Gender.Male;
            woundPed.Health = _config.Data.MaximalHealth;
            woundPed.MaximalPain = _config.Data.MaximalPain;
            woundPed.ThisPed.Health = (int) woundPed.Health;

            _config.Data.PlayerEntity = entity;
        }
        
        public void Run()
        {
            for (int i = 0; i < _playerComponents.EntitiesCount; i++)
            {
                _playerComponents.Components1[i].ThisPed = Game.Player.Character;
            }
        }

        public void Destroy()
        {}
    }
}