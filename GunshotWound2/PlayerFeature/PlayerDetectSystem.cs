namespace GunshotWound2.PlayerFeature {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

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

            // TODO WYD
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
    }
}