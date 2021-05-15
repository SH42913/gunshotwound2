﻿using System;
using GTA;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using GunshotWound2.World;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class PlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<WoundedPedComponent, PlayerMarkComponent> _players = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<GswWorld> _world = null;

        public void Initialize()
        {
            if (!_config.Data.PlayerConfig.WoundedPlayerEnabled) return;

            CreateGswPlayer(Game.Player.Character);
        }

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(PlayerSystem);
#endif
            if (!_config.Data.PlayerConfig.WoundedPlayerEnabled) return;

            for (var i = 0; i < _players.EntitiesCount; i++)
            {
                var woundedPed = _players.Components1[i];
                var playerEntity = _players.Entities[i];

                if (!woundedPed.ThisPed.Position.Equals(Game.Player.Character.Position))
                {
                    SwitchGswPed(Game.Player.Character, woundedPed, playerEntity);
                }

                if (woundedPed.ThisPed.Health > _config.Data.PlayerConfig.MaximalHealth)
                {
                    _ecsWorld.CreateEntityWith<InstantHealEvent>().Entity = playerEntity;
                }
                else if (woundedPed.ThisPed.Health < _config.Data.PlayerConfig.MinimalHealth && !woundedPed.IsDead)
                {
                    woundedPed.Health = woundedPed.ThisPed.Health;
                    woundedPed.IsDead = true;
                    Game.Player.WantedLevel = 0;
                    Game.Player.IgnoredByEveryone = false;
                    Function.Call(Hash.ANIMPOSTFX_STOP_ALL);

                    var ragdollEvent = _ecsWorld.CreateEntityWith<SetPedToRagdollEvent>();
                    ragdollEvent.RagdollState = RagdollStates.PERMANENT;
                    ragdollEvent.Entity = playerEntity;

                    _ecsWorld.RemoveComponent<PainComponent>(_players.Entities[i], true);
                    _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
                }
            }
        }

        private void CreateGswPlayer(Ped ped)
        {
            var entity = _ecsWorld.CreateEntityWith(out PlayerMarkComponent _, out WoundedPedComponent woundedPed);
            woundedPed.PainState = PainStates.DEADLY;

            woundedPed.IsPlayer = true;
            woundedPed.IsMale = ped.Gender == Gender.Male;
            woundedPed.ThisPed = ped;
            woundedPed.IsDead = false;

            woundedPed.Armor = ped.Armor;
            woundedPed.ThisPed.MaxHealth = _config.Data.PlayerConfig.MaximalHealth + 1;

            woundedPed.Health = _config.Data.PlayerConfig.MaximalHealth;
            woundedPed.ThisPed.Health = (int) woundedPed.Health;

            woundedPed.MaximalPain = _config.Data.PlayerConfig.MaximalPain;
            woundedPed.PainRecoverSpeed = _config.Data.PlayerConfig.PainRecoverSpeed;
            woundedPed.StopBleedingAmount = _config.Data.PlayerConfig.BleedHealingSpeed;

            woundedPed.Crits = 0;
            woundedPed.BleedingCount = 0;
            woundedPed.MostDangerBleedingEntity = null;

            woundedPed.ThisPed.CanWrithe = false;
            woundedPed.ThisPed.DiesOnLowHealth = false;
            woundedPed.ThisPed.CanWearHelmet = true;
            woundedPed.ThisPed.CanSufferCriticalHits = false;

            FindDeadlyWound();

            _ecsWorld.CreateEntityWith<NoPainChangeStateEvent>().Entity = entity;
            _world.Data.GswPeds.Add(ped, entity);

            _config.Data.PlayerConfig.PlayerEntity = entity;
#if DEBUG
            SendMessage($"Create new entity {entity}, " +
                        $"GSW health {woundedPed.Health}, " +
                        $"real health {woundedPed.ThisPed.Health}/{woundedPed.ThisPed.MaxHealth}", entity,
                NotifyLevels.DEBUG);
#endif
        }

        private void SwitchGswPed(Ped ped, WoundedPedComponent oldPed, int oldEntity)
        {
            oldPed.IsPlayer = false;
            oldPed.Health = oldPed.Health - _config.Data.PlayerConfig.MinimalHealth;
            oldPed.ThisPed.MaxHealth = _config.Data.NpcConfig.MaxStartHealth;
            oldPed.ThisPed.Health = (int) oldPed.Health;

            _ecsWorld.RemoveComponent<PlayerMarkComponent>(oldEntity);
            _ecsWorld.AddComponent<NpcMarkComponent>(oldEntity);
#if DEBUG
            oldPed.ThisPed.AddBlip();
#endif

            if (_world.Data.GswPeds.ContainsKey(ped))
            {
                var newEntity = _world.Data.GswPeds[ped];

                var newPed = _ecsWorld.GetComponent<WoundedPedComponent>(newEntity);
                newPed.IsPlayer = true;
                newPed.IsDead = false;
                newPed.Health = _config.Data.PlayerConfig.MinimalHealth + newPed.Health;
                newPed.ThisPed.MaxHealth = _config.Data.PlayerConfig.MaximalHealth + 1;
                newPed.ThisPed.Health = (int) newPed.Health;

                newPed.MaximalPain = _config.Data.PlayerConfig.MaximalPain;
                newPed.PainRecoverSpeed = _config.Data.PlayerConfig.PainRecoverSpeed;
                newPed.StopBleedingAmount = _config.Data.PlayerConfig.BleedHealingSpeed;

                _ecsWorld.RemoveComponent<NpcMarkComponent>(newEntity);
                _ecsWorld.AddComponent<PlayerMarkComponent>(newEntity);

                _config.Data.PlayerConfig.PlayerEntity = newEntity;
#if DEBUG
                SendMessage($"Switched to {newEntity}", newEntity, NotifyLevels.DEBUG);
                newPed.ThisPed.AttachedBlip.Delete();
#endif
            }
            else
            {
                CreateGswPlayer(ped);
            }
        }

        private void FindDeadlyWound()
        {
            var totalHealth = _config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth;
            var critical = (float) Math.Sqrt(totalHealth * _config.Data.PlayerConfig.BleedHealingSpeed);
            _config.Data.WoundConfig.EmergencyBleedingLevel = critical;
        }

        public void Destroy()
        {
        }

        private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
            if (_config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}