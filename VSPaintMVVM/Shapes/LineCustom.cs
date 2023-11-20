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
        public static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/line.png";
        public string Name => "Line";

        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public int Thickness { get; set; }

        public int IdIndex { get { return idIndex; } set { idIndex = value; } }
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

            var line = new Line()
            {

                StartPoint = new Point(0, 0),
                EndPoint = new Point(boxEnd.x - boxStart.x, boxEnd.y - boxStart.y),
                Width = width,
                Height=height,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(brush),
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
