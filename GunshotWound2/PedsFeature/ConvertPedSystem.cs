﻿namespace GunshotWound2.PedsFeature {
    using System;
    using GTA;
    using Scellecs.Morpeh;

    public sealed class ConvertPedSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter justConverted;

        public Scellecs.Morpeh.World World { get; set; }

        public ConvertPedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            justConverted = World.Filter.With<JustConvertedEvent>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in justConverted) {
                entity.RemoveComponent<JustConvertedEvent>();
            }

            WorldService worldService = sharedData.worldService;
            while (worldService.TryGetToConvert(out Ped pedToConvert)) {
                Scellecs.Morpeh.Entity entity = World.CreateEntity();

                ref ConvertedPed convertedPed = ref entity.AddComponent<ConvertedPed>();
                convertedPed.name = $"P{pedToConvert.Handle.ToString()}";
                convertedPed.thisPed = pedToConvert;
                convertedPed.isMale = pedToConvert.Gender == Gender.Male;
                convertedPed.defaultAccuracy = pedToConvert.Accuracy;
                PedEffects.SetRagdollBlockingFlags(pedToConvert, 1);
                //TODO Or maybe PED_FLAG_DONT_ACTIVATE_RAGDOLL_FROM_BULLET_IMPACT	107?

                worldService.AddConverted(pedToConvert, entity);
                entity.AddComponent<JustConvertedEvent>();

#if DEBUG
                convertedPed.customBlip = pedToConvert.AddBlip();
                convertedPed.customBlip.Scale = 0.3f;
#endif
            }
        }
    }
}