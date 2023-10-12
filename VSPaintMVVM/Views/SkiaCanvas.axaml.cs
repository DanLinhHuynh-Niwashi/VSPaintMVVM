using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using VSPaintMVVM.Tool;
using VSPaintMVVM.ViewModels;
using SkiaSharp;
using System;
using System.Reflection;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Skia;
using Avalonia.Platform;

namespace VSPaintMVVM.Views
{
    public partial class SkiaCanvas : UserControl
    {

        private SKPaint SKBrush;
        private WriteableBitmap RenderTarget;
        private SKSurface CurrentSurface;

        public static bool IsDrawing = false;
        public static bool IsErasing = false;
        private SKPoint? LastPoint = null;
        //Should do the drawing of the image in another thread potentially

        MainWindowViewModel currentVM = new MainWindowViewModel();
        public override void EndInit()
        {

            RenderTarget = new WriteableBitmap(new PixelSize(1000, 1000), new Vector(96, 96), PixelFormat.Rgba8888);

            using (var lockedBitmap = RenderTarget.Lock())
            {
                SKImageInfo info = new SKImageInfo(lockedBitmap.Size.Width, lockedBitmap.Size.Height, lockedBitmap.Format.ToSkColorType());

                CurrentSurface = SKSurface.Create(info, lockedBitmap.Address, lockedBitmap.RowBytes);
                CurrentSurface.Canvas.Clear(new SKColor(255, 255, 255));
            }

            PointerPressed += DrawingCanvas_PointerPressed;
            PointerMoved += DrawingCanvas_PointerMoved;

            PointerReleased += DrawingCanvas_PointerReleased;
            base.EndInit();
        }

        private int count = 0;
        private void DrawingCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            IsDrawing = false;
            e.Handled = true;
        }

        private void DrawingCanvas_PointerMoved(object sender, PointerEventArgs e)
        {
            if (IsDrawing)
            {
                DrawCurrentBrush(e);
                e.Handled = true;
            }
        }

        private SKPoint Interpolate(SKPoint p0, SKPoint p1, float t)
        {
            float omt = 1.0f - t;
            return new SKPoint(p0.X * omt + p1.X * t, p0.Y * omt + p1.Y * t);
        }

        
        private void DrawCurrentBrush(PointerEventArgs e)
        {
            SKPoint currentPoint = e.GetPosition(this).ToSKPoint();
            if (LastPoint == null)
            {
                CurrentSurface.Canvas.DrawCircle(currentPoint, 2, SKBrush);
            }
            else
            {
                float length = (LastPoint.Value - currentPoint).Length;
                float stepLength = 2.5f / length;
                for (float t = 0; t < 1.0f; t += stepLength)
                {
                    CurrentSurface.Canvas.DrawCircle(Interpolate(LastPoint.Value, currentPoint, t), 2, SKBrush);
                }
                CurrentSurface.Canvas.DrawCircle(currentPoint, 2, SKBrush);
            }

            LastPoint = currentPoint;
            InvalidateVisual();
            
        }

        private void DrawingCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            IsDrawing = true;
            LastPoint = null;
            DrawCurrentBrush(e);
            e.Handled = true;
        }

        public async Task<bool> SaveImage(string path)
        {
            try
            {
                using (var lockedBitmap = RenderTarget.Lock())
                using (FileStream fileStream = File.OpenWrite(path))
                {
                    SKImageInfo info = new SKImageInfo(lockedBitmap.Size.Width, lockedBitmap.Size.Height, lockedBitmap.Format.ToSkColorType());

                    await SKImage.FromPixels(info, lockedBitmap.Address, lockedBitmap.RowBytes)
                        .Encode(SKEncodedImageFormat.Png, 100)
                        .AsStream()
                        .CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public override void Render(DrawingContext context)
        {
            context.DrawImage(RenderTarget, 
                new Rect(0, 0, RenderTarget.PixelSize.Width, RenderTarget.PixelSize.Height)

                );
        }

    }
}
