using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class PointTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview;

        private bool _isActive;

        public PointTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/point.png"));
            Name = "Point Picker";
            IsActive = false;
        }

        public string Name { get; private set; }

        public ToolType ToolType { get { return ToolType.Npc; } }

        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }

        public void MouseDown(TileMouseState e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var npc = _wvm.CurrentWorld.NPCs.FirstOrDefault(n=>n.Name == _wvm.SelectedPoint);

                if (npc != null)
                    npc.Home = e.Location;
                else
                {
                    if (string.Equals(_wvm.SelectedPoint, "Spawn", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _wvm.CurrentWorld.SpawnX = e.Location.X;
                        _wvm.CurrentWorld.SpawnY = e.Location.Y;
                    }
                    else if (string.Equals(_wvm.SelectedPoint, "Dungeon", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _wvm.CurrentWorld.DungeonX = e.Location.X;
                        _wvm.CurrentWorld.DungeonY = e.Location.Y;
                    }
                }
            }
        }

        public void MouseMove(TileMouseState e)
        {
        }

        public void MouseUp(TileMouseState e)
        {

        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public WriteableBitmap PreviewTool()
        {
            return _preview;
        }

        public bool PreviewIsTexture { get { return false; } }
    }
}