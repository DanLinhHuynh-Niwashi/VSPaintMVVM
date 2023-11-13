using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class EllipseCustom : ShapeCustom, ITool
    {
        static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/ellipse.png";
        public string Name => "Ellipse";

        public SolidColorBrush Brush { get; set; }

        public SolidColorBrush FillBrush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            EllipseCustom ellipse = new EllipseCustom();

            ellipse.id = next_Possible_ID();
            return ellipse;
        }


        public override EllipseCustom Copy()
        {
            EllipseCustom temp = new EllipseCustom();
            temp.BoxStart = boxStart.Copy();
            temp.BoxEnd = boxEnd.Copy();
            temp.Angle = angle;

            temp.Thickness = Thickness;

            if (Brush != null)
                temp.Brush = Brush;
            if (FillBrush != null)
                temp.FillBrush = FillBrush;
            return temp;
        }

        public override void CopyFrom(ShapeCustom shape)
        {
            base.CopyFrom(shape);
            Thickness = ((ITool)shape).Thickness;
            if (((ITool)shape).Brush != null)
                Brush = ((ITool)shape).Brush;
            if (((ITool)shape).FillBrush != null)
                FillBrush = ((ITool)shape).FillBrush;
        }

        public Control Draw(SolidColorBrush brush, SolidColorBrush fillBrush, int thickness)
        {
            var left = Math.Min(boxStart.x, boxEnd.x);
            var top = Math.Min(boxStart.y, boxEnd.y);

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            var width = right - left;
            var height = bottom - top;

            var ellipse = new Ellipse()
            {
                Width = width,
                Height = height,
                Fill = fillBrush,
                StrokeThickness = thickness,
                Stroke = brush,
            };

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);

            RotateTransform transform = new RotateTransform(angle);

            ellipse.RenderTransform = transform;

            return ellipse;
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
