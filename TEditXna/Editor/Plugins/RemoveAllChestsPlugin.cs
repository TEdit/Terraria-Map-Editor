using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class RemoveAllChestsPlugin : BasePlugin
    {
        public RemoveAllChestsPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Remove All Chests";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
                {
                    if (_wvm.CurrentWorld.Tiles[x, y].Type == 21)
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
