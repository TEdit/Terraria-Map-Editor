using System.Windows.Input;
using System.Collections.Generic;
using System;

namespace TEdit;

public class KeyboardShortcuts
{
    private Dictionary<KeyCombo, string> KeyCommands = new Dictionary<KeyCombo, string>();

    public void Add(string command, Key key, ModifierKeys modifier = ModifierKeys.None)
    {
        KeyCommands.Add(new KeyCombo(key, modifier), command.ToLowerInvariant());
    }

    public string Get(KeyEventArgs e) => Get(e.Key, e.KeyboardDevice.Modifiers);

    public string Get(Key key, ModifierKeys modifer = ModifierKeys.None)
    {
        if (KeyCommands.TryGetValue(new KeyCombo(key, modifer), out string command))
        {
            return command;
        }

        return null;
    }

    class KeyCombo : IEquatable<KeyCombo>
    {
        public KeyCombo(Key key, ModifierKeys modifier)
        {
            Key = key;
            Modifier = modifier;
        }

        public Key Key { get; }
        public ModifierKeys Modifier { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as KeyCombo);
        }

        public bool Equals(KeyCombo other)
        {
            return other != null &&
                   Key == other.Key &&
                   Modifier == other.Modifier;
        }

        public override int GetHashCode()
        {
            int hashCode = 572187996;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Modifier.GetHashCode();
            return hashCode;
        }
    }
}
