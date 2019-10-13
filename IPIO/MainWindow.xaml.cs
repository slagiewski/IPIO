using IPIO.Core.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IPIO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _fileName;
        private byte[] _imageBytes;
        public Bitmap _bitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private BitmapSource ToImage(byte[] array)
        {

            return (BitmapSource)new ImageSourceConverter().ConvertFrom(array);
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";
            if (dialog.ShowDialog() == true)
            {
                _fileName = dialog.FileName;
                _imageBytes = await File.ReadAllBytesAsync(dialog.FileName);
                ImageBefore.Source = ToImage(_imageBytes);
                var bm = (Bitmap)Bitmap.FromFile(dialog.FileName);
                //ImageAfter.Source = ToImage(
                var (before, after)= Core.Extensions.BitmapExtensions.ForEachPixel(bm, pixel =>
                {
                    //return new Pixel((byte)(pixel.R / 2), (byte)(pixel.G / 2), (byte)(pixel.B / 2));
                    return new Pixel(pixel.R, pixel.G, pixel.B);
                });
                //    );

                ImageAfter.Source = BitmapToImageSource(after);

            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
