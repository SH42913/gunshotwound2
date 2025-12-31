namespace GunshotWound2.PlayerFeature {
    using System;
    using System.Text;
    using Configs;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerHitNotificationSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly StringBuilder stringBuilder;

        private Filter hits;
        private int postHandle;

        public EcsWorld World { get; set; }

        public PlayerHitNotificationSystem(SharedData sharedData) {
            this.sharedData = sharedData;
            stringBuilder = new StringBuilder();
        }

        public void OnAwake() {
            hits = World.Filter.With<PlayerHitNotification>();
        }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.mainConfig.HitNotificationEnabled) {
                return;
            }

            foreach (EcsEntity entity in hits) {
                ref PlayerHitNotification hitNotification = ref entity.GetComponent<PlayerHitNotification>();
                if (hitNotification.entries == null || hitNotification.entries.Count < 1) {
                    continue;
                }

                int count = hitNotification.entries.Count;
                LocaleConfig localeConfig = sharedData.localeConfig;
                while (count > 0) {
                    PlayerHitNotification.Entry entry = hitNotification.entries.Dequeue();
                    count--;

                    Notifier.Color.COMMON.AppendTo(stringBuilder);
                    if (string.IsNullOrEmpty(entry.weaponDesc)) {
                        stringBuilder.AppendFormat(localeConfig.HitNotificationDefault, entry.bodyPart);
                    } else {
                        stringBuilder.AppendFormat(localeConfig.HitNotificationWithWeapon, entry.weaponDesc, entry.bodyPart);
                    }

                    stringBuilder.AppendEndOfLine();

                    Notifier.Color.YELLOW.AppendTo(stringBuilder);
                    stringBuilder.Append(entry.wound);
                    if (entry.hasTrauma) {
                        Notifier.Color.RED.AppendTo(stringBuilder);
                        stringBuilder.AppendSpace();
                        stringBuilder.AppendFormat(localeConfig.HitNotificationTrauma);
                    }

                    if (count > 0) {
                        stringBuilder.AppendEndOfLine();
                    }
                }

                postHandle = sharedData.notifier.ReplaceOne(stringBuilder.ToString(), blinking: false, postHandle);
                stringBuilder.Clear();
            }
        }

        void IDisposable.Dispose() { }
    }
}