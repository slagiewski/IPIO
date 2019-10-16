using IPIO.Core.Algorithms;
using IPIO.Extensions;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;

namespace IPIO
{
    public partial class MainWindow : Window
    {
        private IWatermarkingAlgorithm _algorithm;
        private Bitmap _modifiedImage = null;
        private const string IMG_FILE_FILTERS = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
        private const string SAVE_IMG_FILE_FILTERS = "Image files (*.png)|*.png";
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //create alg factory
            _algorithm = new DimAlgorithm();
        }

        private async void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = IMG_FILE_FILTERS
            };

            if (dialog.ShowDialog() == true)
            {
                InitProgressBar();

                var bm = await Task.Run(() => (Bitmap)Bitmap.FromFile(dialog.FileName));
                ImageBefore.Source = bm.ToImage();

                var modifiedImageBitmap = await Task.Run(() => _algorithm.Run(bm));
                _modifiedImage = modifiedImageBitmap;
                ImageAfter.Source = modifiedImageBitmap.ToImage();

                HideProgressBar();
                ChangeEnableStateSaveButton(true);
            }
        }
        private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_modifiedImage == null) return;
            var dialog = new SaveFileDialog
            {
                Filter = SAVE_IMG_FILE_FILTERS
            };
            if (dialog.ShowDialog() == true)
            {
                await Task.Run(() => _modifiedImage.Save(dialog.FileName, ImageFormat.Png));
                ClearWindow();
            }

        }

        private void ChangeEnableStateSaveButton(bool enable) => SaveButton.IsEnabled = enable;
        private void HideProgressBar() => ProgressBar.Visibility = Visibility.Hidden;

        private void InitProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Height = 50;
            ProgressBar.Width = 200;
            ProgressBar.IsIndeterminate = true;
        }

        private void ClearWindow()
        {
            ImageAfter.Source = null;
            ChangeEnableStateSaveButton(false);
        }
    }
}
