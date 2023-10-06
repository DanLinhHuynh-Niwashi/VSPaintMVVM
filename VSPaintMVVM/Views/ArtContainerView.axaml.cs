using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using VSPaintMVVM.ViewModels;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using System.Diagnostics;

namespace VSPaintMVVM.Views;

public partial class ArtContainerView : UserControl
{
    private readonly ZoomBorder? _zoomBorder;
    public ArtContainerView()
    {
        InitializeComponent();

        _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
        if (_zoomBorder != null)
        {
            _zoomBorder.KeyDown += ZoomBorder_KeyDown;

            _zoomBorder.ZoomChanged += ZoomBorder_ZoomChanged;
        }


    }
    private void ZoomBorder_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F:
                _zoomBorder?.Fill();
                break;
            case Key.U:
                _zoomBorder?.Uniform();
                break;
            case Key.R:
                _zoomBorder?.ResetMatrix();
                break;
            case Key.T:
                _zoomBorder?.ToggleStretchMode();
                _zoomBorder?.AutoFit();
                break;
        }
    }

    private void ZoomBorder_ZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        Debug.WriteLine($"[ZoomChanged] {e.ZoomX} {e.ZoomY} {e.OffsetX} {e.OffsetY}");
    }
}