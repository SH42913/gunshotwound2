namespace GunshotWound2.Utils {
    using Scellecs.Morpeh;

    public static class CustomEcsExtensions {
        public static void SetMarker<T>(this Entity entity)
                where T : struct, IComponent {
            entity.SetComponent(default(T));
        }
    }
}