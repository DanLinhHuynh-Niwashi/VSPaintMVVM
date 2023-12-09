using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.ComponentModel;
using NetSparkleUpdater;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NetSparkleUpdater.UI.Avalonia;
using System.Threading.Tasks;

namespace VSPaintMVVM.Views
{
    public partial class MainWindow : Window
    {
        private SparkleUpdater _sparkle;
        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            // set icon in project properties!
            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _sparkle = new SparkleUpdater("https://q190504.github.io/VSPaint-Website/file/Window/WindowVSPaintappcast.xml", new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Unsafe))
                    {
                        UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(),
                        RelaunchAfterUpdate = false,
                        ShowsUIOnMainThread = true,
                        UserInteractionMode = NetSparkleUpdater.Enums.UserInteractionMode.DownloadAndInstall
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _sparkle = new SparkleUpdater("https://q190504.github.io/VSPaint-Website/file/Linux/LinuxVSPaintappcast.xml", new Ed25519Checker(NetSparkleUpdater.Enums.SecurityMode.Unsafe))
                    {
                        UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(),
                        RelaunchAfterUpdate = false,
                        ShowsUIOnMainThread = true,
                        UserInteractionMode = NetSparkleUpdater.Enums.UserInteractionMode.DownloadAndInstall

                    };
                    // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)

                }

                bool isClosing = false;
                _sparkle.PreparingToExitAsync += (async (x, cancellable) =>
                {

                        var box = MessageBoxManager
                    .GetMessageBoxStandard("New Update Detected", "The application will be closed for an update",
                    ButtonEnum.YesNo);

                        var result = await box.ShowAsync();

                        if (result == ButtonResult.Yes)
                            cancellable.Cancel = false;
                        else
                            cancellable.Cancel = true;
                    
                    
                });

                _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
                _sparkle.StartLoop(true, true);


            }


            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }


        public bool savingRequest = true;

    
        public async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            
            if (mainView != null)
            {
                if (mainView.isFileSaved == false && savingRequest) 
                {
                    e.Cancel = true;
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Save current file", "Unsaved change detected. Do you want to save the current progress?",
                    ButtonEnum.YesNo);

                    var result = await box.ShowAsync();

                    if (result == ButtonResult.Yes)
                        await mainView.Save();
                    else if (result == ButtonResult.No)
                        savingRequest = false;
                    else
                        e.Cancel = false;
                }
            }

            if (e.Cancel == true)
            {
                this.Close();
            }    
        }
    }
}