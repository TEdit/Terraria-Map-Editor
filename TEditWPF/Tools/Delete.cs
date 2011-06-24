using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.RenderWorld;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export(typeof (ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 3)]
    public class Delete : ToolBase
    {
        [Import] private ToolProperties _properties;
        [Import] private SelectionArea _selection;

        [Import("World", typeof (World))] private World _world;
        private bool isLeftDown;


        [Import] private WorldRenderer renderer;

        private PointInt32 start;

        public Delete()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/pencil.png"));
            _Name = "Delete";
            _Type = ToolType.Brush;
            IsActive = false;
        }

        #region Properties

        private readonly BitmapImage _Image;
        private readonly string _Name;

        private readonly ToolType _Type;
        private bool _IsActive;

        public override string Name
        {
            get { return _Name; }
        }

        public override ToolType Type
        {
            get { return _Type; }
        }

        public override BitmapImage Image
        {
            get { return _Image; }
        }

        public override bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isLeftDown = true;
                start = e.Tile;
            }
            return true;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                EraseLine(e);
            }
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            if (_selection.Rectangle.Contains(e.Tile) && isLeftDown)
            {
                for (int x = _selection.Rectangle.X; x < _selection.Rectangle.GetRight(); x++)
                {
                    for (int y = _selection.Rectangle.Y; y < _selection.Rectangle.GetBottom(); y++)
                    {
                        _world.Tiles[x, y].IsActive = false;
                    }
                }
                renderer.UpdateWorldImage(_selection.Rectangle);
            }
            else
            {
                if (isLeftDown)
                    EraseLine(e);
            }
            isLeftDown = false;
            return true;
        }

        public override WriteableBitmap PreviewTool()
        {
            return new WriteableBitmap(
                _properties.Size.Width,
                _properties.Size.Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }

        private void EraseLine(TileMouseEventArgs e)
        {
            foreach (PointInt32 p in WorldRenderer.DrawLine(start, e.Tile))
            {
                _world.Tiles[p.X, p.Y].IsActive = false;
                renderer.UpdateWorldImage(p);
            }
            start = e.Tile;
        }
    }
}