using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using VSPaintMVVM.ViewModels;

namespace VSPaintMVVM.Views
{
    public partial class NewCanvasDialog : Window
    {
        public int choosenState = 0;
        public double h, w;
        public NewCanvasDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            choosenState = 1;
            MainWindowViewModel.isOpeningFile = true;
            this.Close();
        }

        private async void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(NewCanvasHeightTBox.Text, out h))
                {
                    if (h > 10000 || h < 200) throw new Exception() { HResult = 0 };
                    MainWindowViewModel.canvasH = h;
                }
                else throw new Exception();

                if (double.TryParse(NewCanvasWidthTBox.Text, out w))
                {
                    if (w > 10000 || w < 200) throw new Exception() { HResult = 1 };
                    MainWindowViewModel.canvasW = w;
                }
                else throw new Exception();

                choosenState = 2;
                this.Close();
            }
            catch (Exception ex)
            {
                string message = "";
                if (ex.HResult == 0) { message = "Height must under 10000px and over 200px"; }
                else if (ex.HResult == 1) { message = "Width must under 10000px and over 200px"; }
                else { message = "Inappropriate value inputs."; }

                var box = MessageBoxManager
                .GetMessageBoxStandard("Error", message,
                ButtonEnum.Ok);

                await box.ShowAsync();


            }

            
        }

        private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            choosenState = 0;
            this.Close();
        }
    }
}
