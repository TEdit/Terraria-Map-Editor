using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;

namespace TerrariaMapEditor.Classes
{
    public class ChestOptions
    {
        // Add future options later!
        // Kind of feel like this is over-kill right now...
        private static Boolean presetsLoaded = false;

        private static Boolean _jumpToChest;
        public static Boolean jumpToChest
        {
            get
            {
                if (!presetsLoaded)
                {
                    loadPresets();
                }
                return _jumpToChest;
            }
            set
            {
                _jumpToChest = value;
            }
        }

        private static Keys _keyPressRequiredForEditingChest = Keys.Shift;
        public static Keys keyPressRequired
        {
            get
            {
                if (!presetsLoaded)
                {
                    loadPresets();
                }
                return _keyPressRequiredForEditingChest;
            }
            set
            {
                _keyPressRequiredForEditingChest = value;
            }
        }

        public static String getKeyPressRequired()
        {
            return getStringFromKey(keyPressRequired);
        }

        public static void setKeyPressRequired(String k)
        {
            switch (k)
            {
                case("Control"):
                    keyPressRequired = Keys.Control;
                    break;
                case("Shift"):
                    keyPressRequired = Keys.Shift;
                    break;
                default:
                    keyPressRequired = Keys.Alt;
                    break;
            }
        }

        public static String getStringFromKey(Keys key)
        {
            switch (key)
            {
                case(Keys.Alt):
                    return "Alt";
                case(Keys.Control):
                    return "Control";
                case(Keys.Shift):
                    return "Shift";
            }
            return "Alt";
        }

        public static Boolean keyMatchesRequiredForChestEditing(System.Windows.Forms.Keys keyPressed)
        {
            if (keyPressRequired == Keys.Alt)
            {
                return (keyPressed == Keys.Alt || keyPressed.ToString() == "Control, Alt"); //Special case: Right alt is considered both control and alt
            }
            else
            {
                return keyPressed == keyPressRequired;
            }
        }

        public static void loadPresets()
        {
            try
            {
                string jumper = Properties.Settings.Default.isChecked;
                if (string.IsNullOrEmpty(jumper))
                    jumpToChest = false;
                else
                    jumpToChest = jumper.ToLower().Equals("true");
                keyPressRequired = Properties.Settings.Default.keyRequired;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            presetsLoaded = true;
        }

        public static void save()
        {
            Properties.Settings.Default.isChecked = jumpToChest.ToString();
            Properties.Settings.Default.keyRequired = keyPressRequired;
            Properties.Settings.Default.Save();
        }
    }
}
