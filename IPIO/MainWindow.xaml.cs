using IPIO.Core.Algorithms;
using IPIO.Core.Algorithms.Formulas;
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
        private Type _algorithmType;
        private Type _selectedFormulaType;

        private readonly Type[] _formulas = new[]
        {
            typeof(AbsExtendedBasicMultiplicationFormula),
            typeof(ExtendedBasicMultiplicationFormula),
            typeof(BasicMultiplicationFormula)
        };

        private double _alpha = 0.1;
        private Bitmap _loadedImage = null;
        private Bitmap _modifiedImage = null;
        private Bitmap _watermarkImage = null;
        private PerformState _performAction = PerformState.ENCODE;
        private const string IMG_FILE_FILTERS = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp";
        private const string SAVE_IMG_FILE_FILTERS = "Image files (*.png)|*.png";

        public MainWindow()
        {
            InitializeComponent();
            InitElementsState();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _selectedFormulaType = _formulas[2];
            _algorithmType = typeof(DctAlgorithm);
            AlphaTextBox.Text = "0.1";
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

        private async void ChooseWatermarkButton_Click(object sender, RoutedEventArgs e)
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
                    Watermark.Source = bm.ToImage();
                    _watermarkImage = bm;
                });

                ChangeEnableStatePerformActionButton(true);
            }
        }

        private void EncodeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePerformAction(PerformState.ENCODE);
        }

        private void DecodeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePerformAction(PerformState.DECODE);
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

        private bool ParseAlpha()
        {
            var valid = double.TryParse(AlphaTextBox.Text.Replace('.', ','), out var alpha) && alpha > 0 && alpha < 1;

            if (valid)
            {
                _alpha = alpha;
            }

            return valid;
        }

        private async Task EncodeImage()
        {
            if (_watermarkImage == null)
            {
                MessageBox.Show("You need to choose watermark!");
                return;
            }

            if (!ParseAlpha())
            {
                MessageBox.Show("Invalid alpha value!");
                return;
            }

            var algorithm = CreateSelectedAlgorithmInstance();
            var modifiedImageBitmap = await algorithm.EmbedAsync(_loadedImage, _watermarkImage);
            _modifiedImage = modifiedImageBitmap;
            ImageAfter.Source = modifiedImageBitmap.ToImage();
            ChangeEnableStateSaveButton(true);
        }

        private async Task DecodeImage()
        {
            if (_watermarkImage == null)
            {
                MessageBox.Show("You need to choose watermark!");
                return;
            }

            if (!ParseAlpha())
            {
                MessageBox.Show("Invalid alpha value!");
            }

            var algorithm = CreateSelectedAlgorithmInstance();
            var msg = await algorithm.RetrieveAsync(_loadedImage, _watermarkImage, 64 * 64);
            _modifiedImage = msg;
            ImageAfter.Source = msg.ToImage();
            MessageBox.Show("Done!");
            ChangeEnableStatePerformActionButton(true);
            ChangeEnableStateSaveButton(true);
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

            if (newState == PerformState.ENCODE)
            {
                EncodeRadioButton.IsChecked = true;
                DecodeRadioButton.IsChecked = false;
                PerformActionButton.Content = "Encode";
                ChooseWatermark.Content = "Choose watermark...";
                ChooseWatermark.FontSize = 15;
            }
            if (newState == PerformState.DECODE)
            {
                EncodeRadioButton.IsChecked = false;
                DecodeRadioButton.IsChecked = true;
                PerformActionButton.Content = "Decode";
                ChooseWatermark.Content = "Choose watermarked image...";
                ChooseWatermark.FontSize = 14;
            }
        }

        private bool handle = true;

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (handle) HandleAlgorithmChange();
            handle = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handle = !cmb.IsDropDownOpen;
            HandleAlgorithmChange();
        }

        private void HandleAlgorithmChange()
        {
            _algorithmType = (AlhorithmChooser.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last()) switch
            {
                "DCT" => typeof(DctAlgorithm),
                "FourierAlgorithm" => typeof(FourierAlgorithm),
                _ => typeof(DctAlgorithm),
            };
        }

        private void HandleFormulaChange()
        {
            var selectedFormula = FormulaChooser.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            _selectedFormulaType = _formulas
                .FirstOrDefault(t =>
                    t.GetProperty(nameof(IFormula.Name))
                    ?.GetValue(null, null) as string == selectedFormula
                );
        }

        private void FormulaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handle = !cmb.IsDropDownOpen;
            HandleFormulaChange();
        }

        private IWatermarkingAlgorithm CreateSelectedAlgorithmInstance() =>
            Activator.CreateInstance(
                _algorithmType,
                Activator.CreateInstance(
                    _selectedFormulaType,
                    _alpha)
                )
            as IWatermarkingAlgorithm;

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
            _watermarkImage = null;
            Watermark.Source = null;
        }
    }
}

internal enum PerformState
{
    ENCODE,
    DECODE
}