using Leopotam.Ecs;

namespace GunshotWound2.Utils {
    public static class EcsExtensions {
        public static void CleanFilter(this EcsFilter filter) {
            foreach (int index in filter) {
                filter.GetEntity(index).Destroy();
            }
        }

        public static void ScheduleEventWithTarget<T>(this EcsWorld world, EcsEntity target)
                where T : struct {
            EcsEntity eventEntity = world.NewEntity();
            eventEntity.Get<TargetEntity>().target = target;
            eventEntity.Get<T>();
        }

        public static void ScheduleEventWithTarget<T>(this EcsWorld world, EcsEntity target, T evt)
                where T : struct {
            EcsEntity eventEntity = world.NewEntity();
            eventEntity.Get<TargetEntity>().target = target;
            eventEntity.Replace(evt);
        }

        public static void SetMarker<T>(this EcsEntity entity)
                where T : struct {
            entity.Get<T>();
        }
    }
}