namespace GunshotWound2.World {
    using System.Collections.Generic;
    using GTA;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public sealed class GswWorld {
        public readonly Dictionary<Ped, EcsEntity> gswPeds;
        public readonly Queue<Ped> pedsToConvert;
        public bool forceUpdateRequest;

        public GswWorld(int startCapacity) {
            gswPeds = new Dictionary<Ped, EcsEntity>(startCapacity, new PedComparer());
            pedsToConvert = new Queue<Ped>(startCapacity);
            forceUpdateRequest = true;
        }

        private sealed class PedComparer : IEqualityComparer<Ped> {
            public bool Equals(Ped x, Ped y) {
                if (ReferenceEquals(x, y)) {
                    return true;
                }

                if (ReferenceEquals(x, null)) {
                    return false;
                }

                if (ReferenceEquals(y, null)) {
                    return false;
                }

                if (x.GetType() != y.GetType()) {
                    return false;
                }

                return x.Equals(y);
            }

            public int GetHashCode(Ped obj) {
                return obj.GetHashCode();
            }
        }
    }
}