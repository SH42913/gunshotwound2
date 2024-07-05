namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public sealed class InputListener {
        private readonly List<Hotkey> hotkeys = new();

        public void RegisterHotkey(Keys? key, Action action, Keys modifiers = default) {
            if (key.HasValue) {
                hotkeys.Add(new Hotkey(key.Value, action, modifiers));
            }
        }

        public void ConsumeKeyUp(Keys pressedKey, Keys pressedModifier) {
            foreach (Hotkey hotkey in hotkeys) {
                hotkey.Consume(pressedKey, pressedModifier);
            }
        }

        private readonly struct Hotkey {
            private readonly Keys key;
            private readonly Action action;
            private readonly Keys modifiers;

            public Hotkey(Keys key, Action action, Keys modifiers) {
                this.key = key;
                this.action = action;
                this.modifiers = modifiers;
            }

            public void Consume(Keys pressedKey, Keys pressedModifier) {
                if (pressedKey == key && pressedModifier == modifiers) {
                    action.Invoke();
                }
            }
        }
    }
}