using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using TEdit.ViewModel;

namespace TEdit.Editor.Tools
{
    public abstract class BaseTool : ObservableObject, ITool
    {
        protected bool _isActive;
        protected WriteableBitmap _preview;
        protected WorldViewModel _wvm;
        private double _previewScale = 1;

        protected BaseTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);
        }

        #region ITool Members

        public string Name { get; protected set; }

        public string Title => Properties.Language.ResourceManager.GetString($"tool_{Name.ToLower()}_title") ?? Name;

        public virtual ToolType ToolType { get; protected set; }

        public virtual BitmapImage Icon { get; protected set; }

        public virtual bool IsActive
        {
            get { return _isActive; }
            set { Set(nameof(IsActive), ref _isActive, value); }
        }

        public virtual void MouseDown(TileMouseState e)
        {
        }

        public virtual void MouseMove(TileMouseState e)
        {
        }

        public virtual void MouseUp(TileMouseState e)
        {
        }

        public virtual void MouseWheel(TileMouseState e)
        {
        }

        public double PreviewScale
        {
            get { return _previewScale; }
            protected set { _previewScale = value; }
        }

        public virtual WriteableBitmap PreviewTool()
        {
            return _preview;
        }

        public virtual bool PreviewIsTexture
        {
            get { return false; }
        }

        #endregion
    }
}