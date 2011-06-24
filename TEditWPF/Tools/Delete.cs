using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.RenderWorld;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 3)]
    public class Delete : ToolBase
    {
        public Delete()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/pencil.png"));
            _Name = "Delete";
            _Type = ToolType.Brush;
            IsActive = false;
        }

        #region Properties
        private string _Name;
        public override string Name
        {
            get { return _Name; }
        }

        private ToolType _Type;
        public override ToolType Type
        {
            get { return _Type; }
        }

        private BitmapImage _Image;
        public override BitmapImage Image
        {
            get { return _Image; }
        }

        private bool _IsActive;
        public override bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    this.RaisePropertyChanged("IsActive");
                }
            }
        }
        #endregion

        [Import]
        private SelectionArea _selection;

        [Import("World", typeof(World))]
        private World _world;


        [Import]
        private WorldRenderer renderer;

        private PointInt32 start;
        private bool isLeftDown = false;
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
        public override bool PreviewTool(TileMouseEventArgs e) { return false; }

        private void EraseLine(TileMouseEventArgs e)
        {
            foreach (var p in WorldRenderer.DrawLine(start, e.Tile))
            {
                _world.Tiles[p.X, p.Y].IsActive = false;
                renderer.UpdateWorldImage(p);
            }
            start = e.Tile;
        }
    }
}
