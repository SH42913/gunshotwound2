namespace GunshotWound2.PedsFeature {
    using GTA;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class FacialIdleAnimSystem : ILateSystem {
        private readonly SharedData sharedData;

        public EcsWorld World { get; set; }

        private Filter peds;
        private Stash<ConvertedPed> pedStash;
        private bool lastCutsceneState;

        private CrClipDictionary maleDict;
        private CrClipDictionary femaleDict;

        public FacialIdleAnimSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            maleDict = new CrClipDictionary("facials@gen_male@base");
            maleDict.Request();

            femaleDict = new CrClipDictionary("facials@gen_female@base");
            femaleDict.Request();
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            bool currentCutsceneState = Game.IsCutsceneActive;
            if (currentCutsceneState != lastCutsceneState) {
                lastCutsceneState = currentCutsceneState;
#if DEBUG
                sharedData.logger.WriteInfo($"Cutscene state changed:{lastCutsceneState}");
#endif

                if (lastCutsceneState) {
                    ForceCleanFacialIdleAnimForAll();
                }
            }

            if (!lastCutsceneState) {
                UpdatePeds();
            }
        }

        private void UpdatePeds() {
            foreach (EcsEntity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.facialIdleAnimApplied) {
                    continue;
                }

                if (!string.IsNullOrEmpty(convertedPed.facialIdleAnim)) {
                    CrClipDictionary dict = convertedPed.isMale ? maleDict : femaleDict;
                    if (dict.IsLoaded) {
                        PedEffects.SetFacialIdleAnim(convertedPed.thisPed, dict.Name, convertedPed.facialIdleAnim);
#if DEBUG
                        sharedData.logger.WriteInfo($"Applied {convertedPed.facialIdleAnim} facialAnim to {convertedPed.name}");
#endif
                        convertedPed.facialIdleAnimApplied = true;
                    }
                } else {
                    PedEffects.CleanFacialIdleAnim(convertedPed.thisPed);
#if DEBUG
                    sharedData.logger.WriteInfo($"Reset facialAnim of {convertedPed.name}");
#endif
                    convertedPed.facialIdleAnimApplied = true;
                }
            }
        }

        public void Dispose() {
            ForceCleanFacialIdleAnimForAll();
            maleDict.MarkAsNoLongerNeeded();
            femaleDict.MarkAsNoLongerNeeded();
        }

        private void ForceCleanFacialIdleAnimForAll() {
            foreach (EcsEntity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                PedEffects.CleanFacialIdleAnim(convertedPed.thisPed);
                convertedPed.facialIdleAnimApplied = false;
            }
        }
    }
}