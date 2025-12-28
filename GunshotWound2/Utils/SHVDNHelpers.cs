namespace GunshotWound2.Utils {
    using GTA;
    using GTA.Native;

    public static class SHVDNHelpers {
        public static bool UseParticleFxAsset(ParticleEffectAsset asset) {
            asset.Request();
            if (!asset.IsLoaded) {
                return false;
            }

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, asset.AssetName);
            return true;
        }
    }
}