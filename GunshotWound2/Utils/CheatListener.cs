namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using GTA;

    public sealed class CheatListener {
        private readonly struct Cheat {
            public readonly string key;
            public readonly Action action;

            public Cheat(string key, Action action) {
                this.key = key;
                this.action = action;
            }
        }

        private readonly List<Cheat> cheats = new List<Cheat>();
        private readonly ILogger logger;

        public CheatListener(ILogger logger) {
            this.logger = logger;
        }

        public void Register(string cheat, Action action) {
            cheats.Add(new Cheat(cheat, action));
        }

        public void Check() {
            foreach (Cheat cheat in cheats) {
                if (Game.WasCheatStringJustEntered(cheat.key)) {
#if DEBUG
                    logger.WriteInfo($"Just activated cheat {cheat.key}");
#endif
                    cheat.action?.Invoke();
                }
            }
        }
    }
}