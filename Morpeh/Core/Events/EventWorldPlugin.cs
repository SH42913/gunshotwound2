#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

namespace Scellecs.Morpeh {
    internal sealed class EventWorldPlugin : IWorldPlugin {
        static EventWorldPlugin() {
            WorldExtensions.AddWorldPlugin(new EventWorldPlugin());
        }

        public void Initialize(World world) {
            world.CodeWriterEventsRegistry = new EventRegistry();
            world.CodeWriterRequestsRegistry = new RequestRegistry();

            var eventSystemGroup = world.CreateSystemsGroup();
            eventSystemGroup.AddSystem(new ProcessEventsSystem(world.CodeWriterEventsRegistry));
            world.AddPluginSystemsGroup(eventSystemGroup);
        }

        public void Deinitialize(World world) { }
    }
}