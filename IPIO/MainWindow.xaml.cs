﻿using IPIO.Core.Algorithms;
using IPIO.Extensions;
using Microsoft.Win32;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace IPIO
{
    public partial class MainWindow : Window
    {
        private IWatermarkingAlgorithm _algorithm;
        private const string IMG_FILE_FILTERS = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

        public MainWindow()
        {
            InitializeComponent();
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
                ImageAfter.Source = modifiedImageBitmap.ToImage();
               
                HideProgressBar();
            }
        }

        private void HideProgressBar()
        {
            ProgressBar.Visibility = Visibility.Hidden;
        }

        private void InitProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Height = 30;
            ProgressBar.Width = 200;
            ProgressBar.IsIndeterminate = true;
        }
    }
}
