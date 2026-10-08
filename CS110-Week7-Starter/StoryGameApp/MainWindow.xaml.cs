using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StoryGameApp;

public partial class MainWindow : Window
{
    // Each Slide keeps its text, image and description together.
    private record Slide(string Text, string ImagePath, string Description);

    private readonly Slide[] slides =
    {
        new Slide("This is Maxwell The Cat.", "Assets/Images/01.png", "Maxwell is walking through the streets, as he is looking for something to eat."),
        new Slide("Finally! Some Pie.", "Assets/Images/02.png", "Wait a minute... OH NO! The pie is secretly a bomb. MAXWELL, WATCH OUT!!! =O "),
        new Slide("R.I.P MAXWELL THE CAT", "Assets/Images/03.png", "Our poor, innocent Maxwell had been erased by the devastating pie explosion. He may no longer be with us on this planet, yet Maxwell shall be living peacefully in another life. :'(")
    };

    private int currentSlide = 0; // Array positions start at zero.

    public MainWindow()
    {
        InitializeComponent(); // Build the named controls from XAML first.
        ShowSlide(currentSlide);
    }

    private void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Length) return;
        currentSlide = index;
        Slide slide = slides[currentSlide];
        StoryTextBlock.Text = slide.Text;
        ImageDescription.Text = slide.Description;
        SlideCounter.Text = $"Slide {currentSlide + 1} of {slides.Length}";
        BackButton.IsEnabled = currentSlide > 0;
        NextButton.IsEnabled = currentSlide < slides.Length - 1;

        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, slide.ImagePath);
            if (!File.Exists(path)) throw new FileNotFoundException("Image file was not found.", path);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            StoryImage.Source = image;
        }
        catch (Exception ex) when (ex is IOException || ex is NotSupportedException || ex is System.IO.FileFormatException)
        {
            StoryImage.Source = null;
        }
    }

    // --- Story Navigation Handlers ---

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(currentSlide + 1);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(currentSlide - 1);
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(0);
    }

    // --- Screen Capture Logic ---

    private BitmapSource? capturedImage;

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            capturedImage = CaptureService.CapturePrimaryScreen();
            CapturePreview.Source = capturedImage;
            SaveCaptureButton.IsEnabled = true;
            ClearCaptureButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Capture failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (capturedImage is null) return;
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG image|*.png",
            DefaultExt = ".png",
            AddExtension = true,
            FileName = "story-capture.png",
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(capturedImage));
            using var file = File.Create(dialog.FileName);
            encoder.Save(file);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Save failed. Choose a writable folder. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        capturedImage = null;
        CapturePreview.Source = null;
        SaveCaptureButton.IsEnabled = false;
        ClearCaptureButton.IsEnabled = false;
    }
}