using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

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

        public static void loadPresets()
        {
            try
            {
                string jumper = Properties.Settings.Default.isChecked;
                if (string.IsNullOrEmpty(jumper))
                {
                    jumpToChest = false;
                }
                else
                {
                    jumpToChest = jumper.ToLower().Equals("true");
                }
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
            Properties.Settings.Default.Save();
        }
    }
}
