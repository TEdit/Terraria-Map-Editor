using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Imaging;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldImage : ObservableObject
    {
        private WriteableBitmap _Image;
        private WriteableBitmap _Rendered;

        public WriteableBitmap Image
        {
            get { return _Image; }
            set { SetProperty(ref _Image, ref value, "Image"); }
        }

        public WriteableBitmap Rendered
        {
            get { return _Rendered; }
            set { SetProperty(ref _Rendered, ref value, "Rendered"); }
        }

    }
}