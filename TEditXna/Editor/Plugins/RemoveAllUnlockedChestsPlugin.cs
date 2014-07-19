using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class RemoveAllUnlockedChestsPlugin : BasePlugin
    {
        public RemoveAllUnlockedChestsPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Remove All Unlocked Chests";
        }

        private short[] _lockedChestUs = new short[] { 72, 144, 828, 864, 900, 936, 972 };

        private bool isLocked(short u)
        {
            foreach (short s in _lockedChestUs)
            {
                if (u == s || u == s + 18) return true;
            }

            return false;
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
                {
                    if (_wvm.CurrentWorld.Tiles[x, y].Type == 21 && !isLocked(_wvm.CurrentWorld.Tiles[x, y].U))
                    {
                        _wvm.UndoManager.SaveTile(x, y);
                        _wvm.CurrentWorld.Tiles[x, y].Type = 0;
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false;
                        _wvm.UpdateRenderPixel(x, y);
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }
    }
}
