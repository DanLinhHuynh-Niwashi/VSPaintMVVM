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
        static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/pentagon.png";
        public string Name => "Pentagon";

        public SolidColorBrush Brush { get; set; }

        public SolidColorBrush FillBrush { get; set; }
        public int Thickness { get; set; }

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
                Fill = fillBrush,
                Stroke = brush,
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
