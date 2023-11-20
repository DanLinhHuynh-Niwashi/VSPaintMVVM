using Avalonia;
using Avalonia.Controls;

namespace VSPaintMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.AttachDevTools();
        }
    }
}