using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using VSPaintMVVM.ViewModels;
using VSPaintMVVM.Views;

namespace VSPaintMVVM
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                // Create the splash screen
                var splashScreenVM = new SplashScreenViewModel();
                var splashScreen = new SplashScreen
                {
                    DataContext = splashScreenVM
                };

                // Set as the (temporary) main window.
                // By default, the application lifetime will shtut down the application when the main
                // window is closed (unless ShutdownMode is set to something else). Later on we will
                // swap out MainWindow for the "true" MainWindow before closing the splash screen.
                // I see that this type of MainWindow swapping is used when switching themes in
                // Avalonias ControlCatalog source, so presumably it's OK to do this.
                desktop.MainWindow = splashScreen;

                splashScreen.Show();

                try
                {
                    await Task.Delay(4000);
                }
                catch (TaskCanceledException)
                {
                    // User cancelled somewhere along the line. We could clean up here if needed.
                    // Then, close the splash screen and return to shut down the application.
                    // (If we don't close the splash screen, the app will remain running since
                    // it is the MainWindow.)
                    splashScreen.Close();
                    return;
                }

                // Create the main window, and swap it in for the real main window
                var mainWin = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };


                desktop.MainWindow = mainWin;

                mainWin.Show();

                splashScreen.Close();


                
            }

            base.OnFrameworkInitializationCompleted();
        }

        
    }
}