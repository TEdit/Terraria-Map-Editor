using System;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 2)]
    public class Selection : ToolBase
    {
        public Selection()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/shape_square.png"));
            _Name = "Selection";
            _Type = ToolType.Selection;
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

        PointInt32 startselection;
        public override bool PressTool(PointInt32 location)
        {
            startselection = location;
            return true;
        }
        public override bool MoveTool(PointInt32 location)
        {
            _selection.SetRectangle(startselection, location);
            return false;
        }
        public override bool ReleaseTool(PointInt32 location)
        {
            _selection.SetRectangle(startselection, location);
            return true;
        }
        public override bool PreviewTool(PointInt32 location) { return false; }




        //public bool PreviewTool(Point[] location, World world, WriteableBitmap viewPortRegion)
        //{
        //    return false;
        //}

        //public bool UseTool(Point[] location, World world)
        //{
        //    return false;
        //}
    }
}
