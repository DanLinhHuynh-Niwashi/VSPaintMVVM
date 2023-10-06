using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace VSPaintMVVM.Views
{
    public partial class SkiaCanvas : UserControl
    {
        public int Width = 400;
        public int Height = 400;
        public SkiaCanvas()
        {
            InitializeComponent();


            /*Initialized += SkiaCanvas_Initialized;

            renderingLogic = new RenderingLogic();
            renderingLogic.RenderCall += (canvas) => RenderSkia?.Invoke(canvas);*/
        }

    }
}
