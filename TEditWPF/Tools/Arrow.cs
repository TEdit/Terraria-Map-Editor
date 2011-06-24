using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;

namespace TEditWPF.Tools
{
    [Export(typeof (ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 1)]
    public class Arrow : ToolBase
    {
        [Import] private ToolProperties _properties;

        public Arrow()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/cursor.png"));
            _Name = "Arrow";
            _Type = ToolType.Arrow;
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
            return false;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            return false;
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
    }
}