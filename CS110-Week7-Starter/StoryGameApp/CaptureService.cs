using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace StoryGameApp;

// Supplied bridge: Windows screen pixels -> an image WPF can display.
public static class CaptureService
{
    public static BitmapSource CapturePrimaryScreen()
    {
        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? throw new InvalidOperationException("No primary display is available.");
        var bounds = screen.Bounds;
        using var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        }
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze(); // Image no longer depends on the disposed stream.
        return image;
    }
}
