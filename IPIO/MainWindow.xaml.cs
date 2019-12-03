using IPIO.Core.Algorithms;
using IPIO.Core.Interfaces;
using IPIO.Extensions;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IPIO
{
    public partial class MainWindow : Window
    {
        private IStringEmbeddingAlgorithm _algorithm;
        private Bitmap _loadedImage = null;
        private Bitmap _modifiedImage = null;
        private PerformState _performAction = PerformState.ENCODE;
        private const string IMG_FILE_FILTERS = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
        private const string SAVE_IMG_FILE_FILTERS = "Image files (*.png)|*.png";

        public MainWindow()
        {
            InitializeComponent();
            InitElementsState();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //create alg factory
            _algorithm = new HaarWaveletAlgorithm();
        }

        private void InitElementsState()
        {
            ChangePerformAction(PerformState.ENCODE);
        }

        private async void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = IMG_FILE_FILTERS
            };

            if (dialog.ShowDialog() == true)
            {
                await PerformWithProgressBar(async () =>
                {
                    var bm = await Task.Run(() => (Bitmap)Bitmap.FromFile(dialog.FileName));
                    ImageBefore.Source = bm.ToImage();
                    _loadedImage = bm;
                });

                ChangeEnableStatePerformActionButton(true);
            }
        }

        private void EncodeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePerformAction(PerformState.ENCODE);
            Label_EditText.Text = "Encode  text:";
        }

        private void DecodeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePerformAction(PerformState.DECODE);
            Label_EditText.Text = "Decoded text:";
        }

        private async void PerformActionButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformWithProgressBar(async () =>
            {
                switch (_performAction)
                {
                    case PerformState.ENCODE: await EncodeImage(); break;
                    case PerformState.DECODE: await DecodeImage(); break;
                }
            });
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearWindow();
        }

        private async Task EncodeImage()
        {
            var input = InputText.Text;
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("You need to type some text!");
                return;
            }

            var modifiedImageBitmap = await _algorithm.EmbedAsync(_loadedImage, input);
            _modifiedImage = modifiedImageBitmap;
            ImageAfter.Source = modifiedImageBitmap.ToImage();
            ChangeEnableStateSaveButton(true);
        }

        private async Task DecodeImage()
        {
            var msg = await _algorithm.RetrieveAsync(_loadedImage);
            InputText.Text = msg;
            MessageBox.Show("Decoded text is in the box!", "Decoded");
            ChangeEnableStatePerformActionButton(false);
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
                MessageBox.Show("Successfully saved!", "Saved");
                ClearWindow();
            }

        }

        private void ChangePerformAction(PerformState newState)
        {
            _performAction = newState;
            if (!string.IsNullOrEmpty(InputText.Text)) InputText.Text = "";
            if (newState == PerformState.ENCODE)
            {
                EncodeRadioButton.IsChecked = true;
                DecodeRadioButton.IsChecked = false;
                PerformActionButton.Content = "Encode";
                InputText.IsEnabled = true;

            }
            if (newState == PerformState.DECODE)
            {
                EncodeRadioButton.IsChecked = false;
                DecodeRadioButton.IsChecked = true;
                PerformActionButton.Content = "Decode";
                InputText.IsEnabled = false;
            }
        }
        private bool handle = true;
        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (handle) Handle();
            handle = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handle = !cmb.IsDropDownOpen;
            Handle();
        }

        private void Handle()
        {
            switch (AlhorithmChooser.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last())
            {
                case "LSB":
                    _algorithm = new LsbAlgorithm();
                    ClearWindow();
                    break;
                case "DCT":
                    _algorithm = new DctAlgorithm();
                    ClearWindow();
                    break;
                case "HaarWavelet":
                    _algorithm = new HaarWaveletAlgorithm();
                    ClearWindow();
                    break;
                default: _algorithm = new LsbAlgorithm(); break;
            }
        }

        private void ChangeEnableStateSaveButton(bool enable) => SaveButton.IsEnabled = enable;

        private void ChangeEnableStatePerformActionButton(bool enable) => PerformActionButton.IsEnabled = enable;

        private async Task PerformWithProgressBar(Func<Task> action)
        {
            InitProgressBar();
            await action();
            HideProgressBar();
        }

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
            ImageBefore.Source = null;
            ImageAfter.Source = null;
            _loadedImage = null;
            _modifiedImage = null;
            ChangeEnableStateSaveButton(false);
            ChangeEnableStatePerformActionButton(false);
            InputText.Text = "";
        }

    }
}
enum PerformState
{
    ENCODE,
    DECODE
}
