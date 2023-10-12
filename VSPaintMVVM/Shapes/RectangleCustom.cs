using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSPaintMVVM.Tool;

namespace VSPaintMVVM.Shapes
{
    public class RectangleCustom:ShapeCustom, ITool
    {
        public string Icon => "Assets/Icons/RectTIcon.png";
        public string Name => "Rectangle";

        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            return new RectangleCustom();
        }

        public override RectangleCustom Copy()
        {
            RectangleCustom temp = new RectangleCustom();
            temp.BoxStart = boxStart.Copy();
            temp.BoxEnd = boxEnd.Copy();

            temp.Thickness = Thickness;

            if (Brush != null)
                temp.Brush = Brush;

            return temp;
        }
        public Control Draw (SolidColorBrush brush, int thickness)
        {
            var left = boxStart.x;
            var top = boxStart.y;

            var right = boxEnd.x;
            var bottom = boxEnd.y;

            var width = right - left;
            var height = bottom - top;

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Stroke = brush,
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            return rect;
        }
        public void StartCorner(double x, double y)
        {
            boxStart = new PointCustom();
            boxStart.y = y;
            boxStart.x = x;
        }
        public void EndCorner(double x, double y)
        {
            boxEnd = new PointCustom();
            boxEnd.y = y;
            boxEnd.x = x;
        }
    }
}
