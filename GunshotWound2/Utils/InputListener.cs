namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public sealed class InputListener {
        private readonly List<Hotkey> hotkeys = new();

        public void RegisterHotkey(Keys? key, Action action, Keys modifiers = default) {
            if (key.HasValue) {
                RegisterHotkey(new Scheme(key.Value, modifiers), action);
            }
        }

        public void RegisterHotkey(Scheme scheme, Action action) {
            if (scheme.IsValid) {
                hotkeys.Add(new Hotkey(scheme, action));
            }
        }

        public void ConsumeKeyUp(KeyEventArgs keyEventArgs) {
            foreach (Hotkey hotkey in hotkeys) {
                hotkey.Consume(keyEventArgs);
            }
        }

        public readonly struct Scheme {
            private readonly Keys key;
            private readonly Keys modifiers;
            public readonly string description;

            public bool IsValid => key != Keys.None;
            public bool HasModifiers => modifiers != Keys.None;

            public Scheme(Keys key, Keys modifiers) {
                this.key = key;
                this.modifiers = modifiers;
                description = HasModifiers ? $"{modifiers.ToString()}+{key.ToString()}" : key.ToString();
            }

            public bool IsPressed(KeyEventArgs keyEventArgs) {
                return keyEventArgs.KeyCode == key && keyEventArgs.Modifiers == modifiers;
            }
        }

        private readonly struct Hotkey {
            private readonly Scheme scheme;
            private readonly Action action;

            public Hotkey(Scheme scheme, Action action) {
                this.scheme = scheme;
                this.action = action;
            }

            public void Consume(KeyEventArgs keyEventArgs) {
                if (scheme.IsPressed(keyEventArgs)) {
                    action.Invoke();
                }
            }
        }
    }
}