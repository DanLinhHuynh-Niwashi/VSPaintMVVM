using VSPaintMVVM.Views;

namespace VSPaintMVVM.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public static double canvasW = 0;
        public static double canvasH = 0;
        public static bool isOpeningFile = false;

    }
}