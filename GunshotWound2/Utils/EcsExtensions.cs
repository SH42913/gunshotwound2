namespace GunshotWound2.Utils {
    using Scellecs.Morpeh;

    public static class CustomEcsExtensions {
        public static void SetMarker<T>(this EcsEntity entity)
                where T : struct, IComponent {
            entity.SetComponent(default(T));
        }
    }
}