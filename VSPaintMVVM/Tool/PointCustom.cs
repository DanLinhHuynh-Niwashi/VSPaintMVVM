using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPaintMVVM.Tool
{
    public class PointCustom
    {
        public double x { get; set; }
        public double y { get; set; }

        public PointCustom(double x = 0, double y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public PointCustom Copy()
        {
            PointCustom temp = new PointCustom();
            temp.x = x;
            temp.y = y;
            return temp;
        }


        public void StartCorner(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public void EndCorner(double x, double y)
        {
            this.x = x;
            this.y = y;
        }


        public Control Draw(Color brush, Color fillbrush, int thickness)
        {
            Line line = new Line()
            {
                StartPoint = new Avalonia.Point(x, y),
                EndPoint = new Avalonia.Point(x, y),
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(brush),
            };

            return line;
        }
    }
}
