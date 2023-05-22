using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Messanger.Client.Helpers;
internal class ImageHelper
{
    public byte[] BitmapSourceToByteArray(BitmapSource bitmap)
    {
        JpegBitmapEncoder encoder = new()
        {
            //encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            QualityLevel = 100
        };
        // byte[] bit = new byte[0];
        using MemoryStream stream = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);
        var bit = stream.ToArray();
        stream.Close();
        return bit;
    }
    public BitmapSource BitmapToBitmapSource(Bitmap bitmap)
    {
        System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

        BitmapSource bitmapSource = BitmapSource.Create(
            bitmapData.Width, bitmapData.Height,
            bitmap.HorizontalResolution, bitmap.VerticalResolution,
            PixelFormats.Bgr24, null,
            bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

        bitmap.UnlockBits(bitmapData);

        return bitmapSource;
    }
    public BitmapSource Base64StringToBitmapSource(string source)
    {
        return BitmapToBitmapSource(new Bitmap(new MemoryStream(Convert.FromBase64String(source))));
    }
    public string BitmapSourceToBase64String(BitmapSource bitmapSource)
    {
        return Convert.ToBase64String(BitmapSourceToByteArray(bitmapSource));
    }

    internal BitmapSource BinaryToBitmapSource(byte[] imageBytes)
    {
        return BitmapToBitmapSource(new Bitmap(new MemoryStream(imageBytes)));
    }
}
