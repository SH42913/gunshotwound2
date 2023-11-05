namespace GunshotWound2.Player {
    using System;
    using GTA;
    using GTA.Native;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class PlayerDetectSystem : ISystem {
        private readonly SharedData sharedData;
        private Stash<ConvertedPed> pedStash;
        private Filter newPeds;

        public Scellecs.Morpeh.World World { get; set; }

        public PlayerDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            pedStash = World.GetStash<ConvertedPed>();
            newPeds = World.Filter.With<ConvertedPed>().With<JustConvertedEvent>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            MarkNewPedsAsPlayer();
            RemoveMarkerIfDifferentPed();
            FindNewPedIfNeed();

            // PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
            // foreach (int index in players) {
            //     var woundedPed = players.Get1(index);
            //     var playerEntity = players.GetEntity(index);
            //     Player player = Game.Player;
            //     Ped playerPed = player.Character;
            //     if (!woundedPed.ThisPed.Position.Equals(playerPed.Position)) {
            //         SwitchGswPed(playerPed, woundedPed, playerEntity);
            //     }
            //
            //     var pedHealth = woundedPed.PedHealth;
            //     if (pedHealth > playerConfig.MaximalHealth) {
            //         ecsWorld.ScheduleEventWithTarget<InstantHealEvent>(playerEntity);
            //     } else if (pedHealth < playerConfig.MinimalHealth && !woundedPed.IsDead) {
            //         woundedPed.Health = pedHealth;
            //         woundedPed.IsDead = true;
            //         player.WantedLevel = 0;
            //         player.IgnoredByEveryone = false;
            //         Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
            //
            //         var ragdollEvent = ecsWorld.CreateEntityWith<SetPedToRagdollEvent>();
            //         ragdollEvent.RagdollState = RagdollStates.PERMANENT;
            //         ragdollEvent.Entity = playerEntity;
            //
            //         ecsWorld.RemoveComponent<PainComponent>(players.Entities[index], true);
            //         ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
            //     }
            // }
        }

        private void MarkNewPedsAsPlayer() {
            foreach (Scellecs.Morpeh.Entity entity in newPeds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.thisPed.IsPlayer) {
                    SetPlayer(entity, ref convertedPed);
                }
            }
        }

        private void RemoveMarkerIfDifferentPed() {
            if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity entity)) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);

                // !woundedPed.ThisPed.Position.Equals(playerPed.Position)
                if (!convertedPed.thisPed.Exists() || !Game.Player.Character.Equals(convertedPed.thisPed)) {
                    UnSetPlayer(entity, ref convertedPed);
                }
            }
        }

        private void SetPlayer(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.isPlayer = true;
            entity.SetMarker<JustConvertedEvent>();
            sharedData.playerEntity = entity;
#if DEBUG
            sharedData.logger.WriteInfo($"Ped {convertedPed.name} is new player");
#endif
        }

        private void UnSetPlayer(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.isPlayer = false;
            entity.SetMarker<JustConvertedEvent>();
            sharedData.playerEntity = null;
#if DEBUG
            sharedData.logger.WriteInfo($"Ped {convertedPed.name} is not player anymore");
#endif
        }

        private void FindNewPedIfNeed() {
            if (!sharedData.mainConfig.PlayerConfig.WoundedPlayerEnabled || sharedData.TryGetPlayer(out _)) {
                return;
            }

            Ped playerPed = Game.Player.Character;
            if (sharedData.worldService.TryGetConverted(playerPed, out Scellecs.Morpeh.Entity entity)) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                SetPlayer(entity, ref convertedPed);
            } else if (playerPed.IsAlive) {
                sharedData.worldService.EnqueueToConvert(playerPed);
#if DEBUG
                sharedData.logger.WriteInfo($"New Player Ped {playerPed.Handle.ToString()} will be created!");
#endif
            }
        }

        // private void CreateGswPlayer(Ped ped, PlayerConfig playerConfig) {
        //     Scellecs.Morpeh.Entity entity = World.CreateEntity();
        //     entity.AddComponent<IsPlayerMarker>();
        //     entity.AddComponent<JustConvertedMarker>();
        //
        //     ref ConvertedPed convertedPed = ref entity.AddComponent<ConvertedPed>();
        //     convertedPed.thisPed = ped;
        //     convertedPed.isPlayer = true;

        // ref WoundedPedComponent woundedPed = ref entity.Get<WoundedPedComponent>();
        // woundedPed.PainState = PainStates.DEADLY;
        //
        // woundedPed.IsPlayer = true;
        // woundedPed.IsMale = ped.Gender == Gender.Male;
        // woundedPed.ThisPed = ped;
        // woundedPed.IsDead = false;
        //
        // woundedPed.Armor = ped.Armor;
        // woundedPed.Health = playerConfig.MaximalHealth;
        // woundedPed.PedHealth = woundedPed.Health;
        // woundedPed.PedMaxHealth = woundedPed.Health;
        //
        // woundedPed.MaximalPain = playerConfig.MaximalPain;
        // woundedPed.PainRecoverSpeed = playerConfig.PainRecoverSpeed;
        // woundedPed.StopBleedingAmount = playerConfig.BleedHealingSpeed;
        //
        // woundedPed.Crits = 0;
        // woundedPed.BleedingCount = 0;
        // woundedPed.MostDangerBleedingEntity = null;
        //
        // woundedPed.ThisPed.CanWrithe = false;
        // woundedPed.ThisPed.DiesOnLowHealth = false;
        // woundedPed.ThisPed.CanWearHelmet = true;
        // woundedPed.ThisPed.CanSufferCriticalHits = false;

        // ecsWorld.ScheduleEventWithTarget<NoPainChangeStateEvent>(entity);
        //     sharedData.worldService.AddConverted(ped, entity);
        //     sharedData.playerEntity = entity;
        // }

        //         private void SwitchGswPed(Ped ped, WoundedPedComponent oldPed, EcsEntity oldEntity) {
        //             var playerConfig = _config.Data.PlayerConfig;
        //
        //             oldPed.IsPlayer = false;
        //             oldPed.Health -= playerConfig.MinimalHealth;
        //             oldPed.PedHealth = oldPed.Health;
        //             oldPed.PedMaxHealth = 100;
        //
        //             ecsWorld.RemoveComponent<IsPlayerMarker>(oldEntity);
        //             ecsWorld.AddComponent<IsNpcMarker>(oldEntity);
        // #if DEBUG
        //             oldPed.ThisPed.AddBlip();
        // #endif
        //
        //             if (_world.Data.GswPeds.TryGetValue(ped, out var newEntity)) {
        //                 var newPed = ecsWorld.GetComponent<WoundedPedComponent>(newEntity);
        //                 newPed.IsPlayer = true;
        //                 newPed.IsDead = false;
        //                 newPed.Health += playerConfig.MinimalHealth;
        //                 newPed.PedHealth = newPed.Health;
        //                 newPed.PedMaxHealth = playerConfig.MaximalHealth;
        //
        //                 newPed.MaximalPain = playerConfig.MaximalPain;
        //                 newPed.PainRecoverSpeed = playerConfig.PainRecoverSpeed;
        //                 newPed.StopBleedingAmount = playerConfig.BleedHealingSpeed;
        //
        //                 ecsWorld.RemoveComponent<IsNpcMarker>(newEntity);
        //                 ecsWorld.AddComponent<IsPlayerMarker>(newEntity);
        //                 UpdatePainState(newEntity, newPed.PainState);
        //
        //                 playerConfig.PlayerEntity = newEntity;
        // #if DEBUG
        //                 sharedData.logger.WriteInfo($"Switched to {newEntity}");
        //                 newPed.ThisPed.AttachedBlip.Delete();
        // #endif
        //             } else {
        //                 CreateGswPlayer(ped);
        //             }
        //         }
        //
        //         private void UpdatePainState(int entity, PainStates state) {
        //             BaseChangePainStateEvent evt;
        //
        //             switch (state) {
        //                 case PainStates.NONE:
        //                     evt = ecsWorld.AddComponent<NoPainChangeStateEvent>(entity);
        //                     break;
        //                 case PainStates.MILD:
        //                     evt = ecsWorld.AddComponent<MildPainChangeStateEvent>(entity);
        //                     break;
        //                 case PainStates.AVERAGE:
        //                     evt = ecsWorld.AddComponent<AveragePainChangeStateEvent>(entity);
        //                     break;
        //                 case PainStates.INTENSE:
        //                     evt = ecsWorld.AddComponent<IntensePainChangeStateEvent>(entity);
        //                     break;
        //                 case PainStates.UNBEARABLE:
        //                     evt = ecsWorld.AddComponent<UnbearablePainChangeStateEvent>(entity);
        //                     break;
        //                 case PainStates.DEADLY:
        //                     evt = ecsWorld.AddComponent<DeadlyPainChangeStateEvent>(entity);
        //                     break;
        //                 default: throw new ArgumentOutOfRangeException();
        //             }
        //
        //             evt.ForceUpdate = true;
        //             evt.Entity = entity;
        //         }
    }
}