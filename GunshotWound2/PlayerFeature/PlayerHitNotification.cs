namespace GunshotWound2.PlayerFeature {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh;

    [Serializable]
    public struct PlayerHitNotification : IComponent {
        public readonly struct Entry {
            public readonly string weaponDesc;
            public readonly string bodyPart;
            public readonly string wound;
            public readonly bool hasTrauma;

            public Entry(string weaponDesc, string bodyPart, string wound, bool hasTrauma) {
                this.weaponDesc = weaponDesc;
                this.bodyPart = bodyPart;
                this.wound = wound;
                this.hasTrauma = hasTrauma;
            }
        }

        public Queue<Entry> entries;
    }
}