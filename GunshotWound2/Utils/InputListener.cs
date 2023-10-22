namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public sealed class InputListener {
        private readonly List<Hotkey> hotkeys = new();

        public void RegisterHotkey(Keys? key, Action action) {
            if (key.HasValue) {
                hotkeys.Add(new Hotkey(key.Value, action));
            }
        }

        public void ConsumeKeyUp(Keys keyCode) {
            foreach (Hotkey hotkey in hotkeys) {
                hotkey.Consume(keyCode);
            }
        }

        private readonly struct Hotkey {
            private readonly Keys key;
            private readonly Action action;

            public Hotkey(Keys key, Action action) {
                this.key = key;
                this.action = action;
            }

            public void Consume(Keys keyCode) {
                if (key == keyCode) {
                    action.Invoke();
                }
            }
        }
    }
}