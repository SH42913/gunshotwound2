namespace Scellecs.Morpeh {
    public static class CustomEcsExtensions {
        // public static void CleanFilter(this EcsFilter filter) {
        //     foreach (int index in filter) {
        //         filter.GetEntity(index).Destroy();
        //     }
        // }
        //
        // public static void ScheduleEventWithTarget<T>(this EcsWorld world, EcsEntity target)
        //         where T : struct {
        //     EcsEntity eventEntity = world.NewEntity();
        //     eventEntity.Get<TargetEntity>().target = target;
        //     eventEntity.Get<T>();
        // }
        //
        // public static void ScheduleEventWithTarget<T>(this EcsWorld world, EcsEntity target, T evt)
        //         where T : struct {
        //     EcsEntity eventEntity = world.NewEntity();
        //     eventEntity.Get<TargetEntity>().target = target;
        //     eventEntity.Replace(evt);
        // }

        public static void SetMarker<T>(this Entity entity)
                where T : struct, IComponent {
            entity.SetComponent(default(T));
        }
    }
}