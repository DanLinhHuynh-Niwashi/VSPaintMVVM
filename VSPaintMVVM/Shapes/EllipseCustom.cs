using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class EllipseCustom : ShapeCustom, ITool
    {
        public static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/ellipse.png";
        public string Name => "Ellipse";

        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public int Thickness { get; set; }

        public int IdIndex { get { return idIndex; } set { idIndex = value; } }
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

            temp.Brush = Brush;
            temp.FillBrush = FillBrush;
            return temp;
        }

        public override void CopyFrom(ShapeCustom shape)
        {
            base.CopyFrom(shape);
            Thickness = ((ITool)shape).Thickness;
            Brush = ((ITool)shape).Brush;
            FillBrush = ((ITool)shape).FillBrush;
        }

        public Control Draw(Color brush, Color fillBrush, int thickness)
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
                StrokeThickness = thickness,
                Fill = new SolidColorBrush(fillBrush),
                Stroke = new SolidColorBrush(brush),
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
