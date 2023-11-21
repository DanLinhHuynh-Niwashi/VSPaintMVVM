using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.ComponentModel;

namespace VSPaintMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;
            this.AttachDevTools();
        }

        public bool savingRequest = true;
        public async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            
            if (mainView != null)
            {
                if (mainView.canvas.Children.Count > 0 && mainView.isFileSaved == false && savingRequest) 
                {
                    e.Cancel = true;
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Save current file", "Unsaved change detected. Do you want to save the current progress?",
                    ButtonEnum.YesNo);

                    var result = await box.ShowAsync();

                    if (result == ButtonResult.Yes)
                        await mainView.Save();
                    if (result == ButtonResult.No)
                        savingRequest = false;
                }
            }

            if (e.Cancel == true)
            {
                this.Close();
            }    
        }
    }
}