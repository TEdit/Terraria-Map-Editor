using TEditXna.ViewModel;
using System.Linq;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Plugins
{
    public sealed class SandSettlePlugin : BasePlugin
    {
        public SandSettlePlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Settle Sand";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;
            int[] _tileSand = {53, 112, 234, 116};

            for (int y = _wvm.CurrentWorld.TilesHigh - 1; y > 0; y--)
            {
                for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
                {
                    var curTile = _wvm.CurrentWorld.Tiles[x, y];
                    if (_tileSand.Contains(curTile.Type))
                    {
                        // check if tile below current tile is empty and move sand to there if it is.
                        int shiftAmmount = 1;
                        while (shiftAmmount + y < _wvm.CurrentWorld.TilesHigh && !_wvm.CurrentWorld.Tiles[x, y + shiftAmmount].IsActive)
                            shiftAmmount++;
                        shiftAmmount--;

                        if (shiftAmmount > 0)
                        {
                            var belowTile = _wvm.CurrentWorld.Tiles[x, y + shiftAmmount];
                            if (!belowTile.IsActive)
                            {
                                _wvm.UndoManager.SaveTile(x, y + shiftAmmount);
                                _wvm.UndoManager.SaveTile(x, y);
                                belowTile.IsActive = true;
                                belowTile.Type = curTile.Type;
                                curTile.IsActive = false;
                                BlendRules.ResetUVCache(_wvm, x, y, 1, 1 + shiftAmmount);
                                _wvm.UpdateRenderPixel(x, y);
                                _wvm.UpdateRenderPixel(x, y + shiftAmmount);
                            }
                        }
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }
    }
}