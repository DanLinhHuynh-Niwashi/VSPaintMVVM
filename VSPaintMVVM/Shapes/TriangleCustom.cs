using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using Avalonia;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class TriangleCustom : ShapeCustom, ITool
    {
        static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/triangle.png";
        public string Name => "Triangle";

        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            TriangleCustom triangle = new TriangleCustom();

            triangle.id = next_Possible_ID();
            return triangle;
        }


        public override TriangleCustom Copy()
        {
            TriangleCustom temp = new TriangleCustom();
            temp.BoxStart = boxStart.Copy();
            temp.BoxEnd = boxEnd.Copy();
            temp.Angle = angle;

            temp.Thickness = Thickness;

            if (Brush != null)
                temp.Brush = Brush;

            return temp;
        }

        public override void CopyFrom(ShapeCustom shape)
        {
            base.CopyFrom(shape);
            Thickness = ((ITool)shape).Thickness;
            if (((ITool)shape).Brush != null)
                Brush = ((ITool)shape).Brush;
        }

        public Control Draw(SolidColorBrush brush, int thickness)
        {
            var left = Math.Min(boxStart.x, boxEnd.x);
            var top = Math.Min(boxStart.y, boxEnd.y);

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            var width = right - left;
            var height = bottom - top;


            var triangle = new Polygon()
            {
                Points =
                {
                    new Point(width/2,0),
                    new Point(width,height),
                    new Point(0, height)
                },
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Stroke = brush,
            };

            Canvas.SetLeft(triangle, left);
            Canvas.SetTop(triangle, top);

            RotateTransform transform = new RotateTransform(angle);

            triangle.RenderTransform = transform;

            return triangle;
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
