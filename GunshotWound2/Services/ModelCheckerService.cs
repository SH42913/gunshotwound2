namespace GunshotWound2.Services {
    using Configs;
    using GTA;

    public sealed class ModelCheckerService {
        private readonly int michaelModelHash;
        private readonly int franklinModelHash;
        private readonly int trevorModelHash;

        private int healthPackModelHash;

        public ModelCheckerService() {
            michaelModelHash = GetHashByName("player_zero");
            franklinModelHash = GetHashByName("player_one");
            trevorModelHash = GetHashByName("player_two");
        }

        public void Init(MainConfig mainConfig) {
            healthPackModelHash = GetHashByName(mainConfig.playerConfig.MedkitModel);
        }

        public bool IsMedkit(Model model) {
            return model.Hash == healthPackModelHash;
        }

        public bool IsMainChar(Model model) {
            return model.Hash == michaelModelHash || model.Hash == franklinModelHash || model.Hash == trevorModelHash;
        }

        private static int GetHashByName(string modelName) {
            return (int)StringHash.AtStringHashUtf8(modelName);
        }
    }
}