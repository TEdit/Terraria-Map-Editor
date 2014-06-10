using System.IO;

namespace System.Windows.Media.Imaging
{
    public static class MoreExtensions
    {
        public static void SavePng(this WriteableBitmap wbmp, string filename)
        {
            using (var filestream = new FileStream(filename, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                BitmapFrame frame = BitmapFrame.Create(wbmp);
                encoder.Frames.Add(frame);
                encoder.Save(filestream);

                filestream.Close();
            }
        }
    }
}