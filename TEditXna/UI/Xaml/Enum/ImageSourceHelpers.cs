// Source: http://www.codeproject.com/KB/WPF/EnumItemList.aspx
// License: The Code Project Open License (CPOL) 1.02
// License URL: http://www.codeproject.com/info/cpol10.aspx
using System;
using System.Windows.Media;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace TEdit.UI.Xaml.Enum
{
    static class ImageSourceHelpers
    {
        public static ImageSource CreateFromBitmap(Bitmap bmp)
        {
            var hBitmap = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        internal static ImageSource CreateFromIcon(Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
