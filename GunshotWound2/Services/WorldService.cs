namespace GunshotWound2.Services {
    using System.Collections.Generic;
    using GTA;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public sealed class WorldService {
        public bool forceRefreshRequest;

        private readonly Dictionary<Ped, EcsEntity> convertedPeds;
        private readonly Queue<Ped> pedsToConvert;

        public WorldService(int startCapacity) {
            convertedPeds = new Dictionary<Ped, EcsEntity>(startCapacity, new PedComparer());
            pedsToConvert = new Queue<Ped>(startCapacity);
            forceRefreshRequest = true;
        }

        public void AddConverted(Ped ped, EcsEntity entity) {
            convertedPeds.Add(ped, entity);
        }

        public void RemoveConverted(Ped ped) {
            convertedPeds.Remove(ped);
        }

        public bool IsConverted(Ped ped) {
            return convertedPeds.ContainsKey(ped);
        }

        public bool TryGetConverted(Ped ped, out EcsEntity entity) {
            return convertedPeds.TryGetValue(ped, out entity);
        }

        public Dictionary<Ped, EcsEntity>.KeyCollection GetConvertedCollection() {
            return convertedPeds.Keys;
        }

        public void EnqueueToConvert(Ped ped) {
            pedsToConvert.Enqueue(ped);
        }

        public bool TryGetToConvert(out Ped ped) {
            if (pedsToConvert.Count > 0) {
                ped = pedsToConvert.Dequeue();
                return true;
            } else {
                ped = null;
                return false;
            }
        }

        public override string ToString() {
            return $"GSW-world:\nConverted:{convertedPeds.Count.ToString()}\nToConvert:{pedsToConvert.Count.ToString()}";
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