using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using Avalonia;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class LineCustom : ShapeCustom, ITool
    {
        static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/line.png";
        public string Name => "Line";

        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            LineCustom line = new LineCustom();

            line.id = next_Possible_ID();
            return line;
        }


        public override LineCustom Copy()
        {
            LineCustom temp = new LineCustom();
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

            var line = new Line()
            {

                StartPoint = new Point(0, 0),
                EndPoint = new Point(boxEnd.x - boxStart.x, boxEnd.y - boxStart.y),
                StrokeThickness = thickness,
                Stroke = brush,
            };

            Canvas.SetLeft(line, boxStart.x);
            Canvas.SetTop(line, boxStart.y);

            RotateTransform transform = new RotateTransform(angle);

            line.RenderTransform = transform;

            return line;
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
