namespace GunshotWound2.Services {
    using Configs;
    using GTA;

    public sealed class ModelCheckerService {
        private int healthPackModelHash;

        public void Init(MainConfig mainConfig) {
            healthPackModelHash = (int)StringHash.AtStringHashUtf8(mainConfig.playerConfig.MedkitModel);
        }

        public bool IsMedkit(Model model) {
            return model.Hash == healthPackModelHash;
        }
    }
}