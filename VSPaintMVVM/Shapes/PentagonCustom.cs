using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using Avalonia;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class PentagonCustom : ShapeCustom, ITool
    {
        public static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/pentagon.png";
        public string Name => "Pentagon";

        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public int Thickness { get; set; }

        public int IdIndex { get { return idIndex; } set { idIndex = value; } }
        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            PentagonCustom pentagon = new PentagonCustom();

            pentagon.id = next_Possible_ID();
            return pentagon;
        }


        public override PentagonCustom Copy()
        {
            PentagonCustom temp = new PentagonCustom();
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


            var pentagon = new Polygon()
            {
                Points =
                {
                    new Point(width/2,0),
                    new Point (width, height*2/5),
                    new Point(width*4/5, height),
                    new Point(width/5,height),
                    new Point (0, height*2/5)
                },
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Fill = new SolidColorBrush(fillBrush),
                Stroke = new SolidColorBrush(brush),
            };

            Canvas.SetLeft(pentagon, left);
            Canvas.SetTop(pentagon, top);

            RotateTransform transform = new RotateTransform(angle);

            pentagon.RenderTransform = transform;

            return pentagon;
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
