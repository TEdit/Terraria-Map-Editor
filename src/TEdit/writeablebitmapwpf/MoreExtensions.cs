using System.IO;

namespace System.Windows.Media.Imaging;

public static class MoreExtensions
{
    public static void SavePng(this WriteableBitmap wbmp, string filename)
    {
        var fileInfo = new FileInfo(filename);
        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }
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