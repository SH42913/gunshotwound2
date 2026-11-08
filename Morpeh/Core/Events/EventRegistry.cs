namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using Collections;

    internal class EventRegistry {
        internal readonly Dictionary<Type, EventBase> RegisteredEvents = new Dictionary<Type, EventBase>();

        internal FastList<EventBase> DispatchedEvents = new FastList<EventBase>();
        internal FastList<EventBase> ExecutingEvents = new FastList<EventBase>();
    }
}