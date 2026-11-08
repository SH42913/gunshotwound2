namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;

    internal class RequestRegistry {
        internal readonly Dictionary<Type, RequestBase> RegisteredRequests = new Dictionary<Type, RequestBase>();
    }
}