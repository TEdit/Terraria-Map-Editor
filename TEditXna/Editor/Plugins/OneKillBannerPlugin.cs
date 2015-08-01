using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TEditXna.ViewModel;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Plugins
{
    class OneKillBannerPlugin : BasePlugin
    {
        public OneKillBannerPlugin(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Name = "One Kill Banner";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
            {
                return;
            }
            if (MessageBox.Show("Increase kill counts to nearest 49/99.", "Kill Counts",
                        MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }
            foreach (int idx in World.TallyNames.Keys)
            {
                int currentNum = _wvm.CurrentWorld.KilledMobs[idx];
                int increasedNum = ((currentNum / 50) + 1) * 50 - 1;
                _wvm.CurrentWorld.KilledMobs[idx] = increasedNum;
            }
        }
    }
}
