using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Imaging;
using TEditWPF.Common;

namespace TEditWPF.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldImage : ObservableObject
    {
        private WriteableBitmap _Image;

        private WriteableBitmap _Overlay;

        public WriteableBitmap Image
        {
            get { return _Image; }
            set
            {
                if (_Image != value)
                {
                    _Image = value;
                    RaisePropertyChanged("Image");
                }
            }
        }

        public WriteableBitmap Overlay
        {
            get { return _Overlay; }
            set
            {
                if (_Overlay != value)
                {
                    _Overlay = value;
                    RaisePropertyChanged("Overlay");
                }
            }
        }

        public void ResetOverlay()
        {
            if (_Image != null)
            {
                _Overlay = new WriteableBitmap(_Image.PixelWidth, _Image.PixelHeight, _Image.DpiX, _Image.DpiY,
                                               _Image.Format, _Image.Palette);

                int stride = _Overlay.BackBufferStride;
                int numpixelbytes = _Overlay.PixelHeight*_Overlay.PixelWidth*_Overlay.Format.BitsPerPixel/8;
                var pixels = new byte[numpixelbytes];

                //for (int x = 0; x < _Overlay.PixelWidth; x++)
                //{
                //    for (int y = 0; y < _Overlay.PixelHeight; y++)
                //    {
                //        pixels[x * 4 + y * stride] = (byte)0;
                //        pixels[x * 4 + y * stride + 1] = (byte)0;
                //        pixels[x * 4 + y * stride + 2] = (byte)0;
                //        pixels[x * 4 + y * stride + 3] = (byte)0;
                //    }
                //}

                _Image.CopyPixels(pixels, stride, 0);
                _Overlay.WritePixels(new Int32Rect(0, 0, _Overlay.PixelWidth, _Overlay.PixelHeight), pixels,
                                     _Overlay.PixelWidth*_Overlay.Format.BitsPerPixel/8, 0);
            }
            RaisePropertyChanged("Overlay");
        }
    }
}