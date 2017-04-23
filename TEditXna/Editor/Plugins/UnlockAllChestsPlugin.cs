using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class UnlockAllChestsPlugin : BasePlugin
    {
        public UnlockAllChestsPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Unlock All Chests";
        }

        private short[] _lockedChestUs = new short[] { 72, 144, 828, 864, 900, 936, 972 };
        private short[] _unlockedUs = new short[] { 36, 108, 648, 684, 720, 756, 792 };

        private bool isLocked(short u, out short unlockedU)
        {
            unlockedU = 0;
            for (int i = 0; i < _lockedChestUs.Length;i++ )
            {
                if (u == _lockedChestUs[i])
                {
                    unlockedU = _unlockedUs[i];
                    return true;
                }
                else if (u == _lockedChestUs[i] + 18)
                {
                    unlockedU = (short)(_unlockedUs[i] + 18);
                    return true;
                }
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
                    short unlockedU = 0;
                    if (_wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.Chest && isLocked(_wvm.CurrentWorld.Tiles[x, y].U, out unlockedU))
                    {
                        _wvm.UndoManager.SaveTile(x, y);
                        _wvm.CurrentWorld.Tiles[x, y].U = unlockedU;
                        _wvm.UpdateRenderPixel(x, y);
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }
    }
}
