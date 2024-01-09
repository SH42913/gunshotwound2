namespace GunshotWound2.PedsFeature {
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedsFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new NpcDetectSystem(sharedData));
            systemsGroup.AddSystem(new ConvertPedSystem(sharedData));
            systemsGroup.AddSystem(new RagdollSystem(sharedData));
            systemsGroup.AddSystem(new PedMovementSystem(sharedData));
            systemsGroup.AddSystem(new RemoveConvertedPedSystem(sharedData));

            MainConfig mainConfig = sharedData.mainConfig;
            InputListener inputListener = sharedData.inputListener;
            inputListener.RegisterHotkey(mainConfig.IncreaseRangeKey, () => ChangeRange(sharedData, 5f));
            inputListener.RegisterHotkey(mainConfig.ReduceRangeKey, () => ChangeRange(sharedData, -5f));

#if DEBUG
            inputListener.RegisterHotkey(System.Windows.Forms.Keys.N, () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                    if (convertedPed.thisPed.IsRagdoll) {
                        convertedPed.ResetRagdoll();
                    } else {
                        convertedPed.RequestPermanentRagdoll();
                    }
                }
            });
#endif
        }

        private static void ChangeRange(SharedData sharedData, float value) {
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            if (npcConfig.AddingPedRange + value < MainConfig.MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
                return;
            }

            npcConfig.AddingPedRange += value;
            npcConfig.RemovePedRange = npcConfig.AddingPedRange * MainConfig.ADDING_TO_REMOVING_MULTIPLIER;

            LocaleConfig localeConfig = sharedData.localeConfig;
            sharedData.notifier.info.AddMessage($"{localeConfig.AddingRange}: {npcConfig.AddingPedRange.ToString("F0")}");
            sharedData.notifier.info.AddMessage($"{localeConfig.RemovingRange}: {npcConfig.RemovePedRange.ToString("F0")}");
        }
    }
}