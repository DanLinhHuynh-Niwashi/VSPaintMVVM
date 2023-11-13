using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class RectangleCustom:ShapeCustom, ITool
    {
        static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/rectangle.png";
        public string Name => "Rectangle";

        public SolidColorBrush Brush { get; set; }

        public SolidColorBrush FillBrush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            RectangleCustom rect = new RectangleCustom();

            rect.id = next_Possible_ID();
            return rect;
        }

        
        public override RectangleCustom Copy()
        {
            RectangleCustom temp = new RectangleCustom();
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

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Fill = fillBrush,
                Stroke = brush,
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(angle);

            rect.RenderTransform = transform;

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
