using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;

namespace VSPaintMVVM.Tool
{
    public class AnchorPoint:ITool
    {
        public bool rotable { get; set; }
        public double angle { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public string type { get; set; }
        public string Icon { get; }

        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public string Name => "AnchorPoint";
        public int Thickness { get; set; }

        public int IdIndex { get; set; }
        public ITool Clone()
        {
            return new AnchorPoint();
        }

        public int isHovering(Point pos)
        {
            Point sample = new Point(x, y);
            if (Point.Distance(sample, pos) <= 10)
            {
                return 1;
            }

            if (rotable == true && Point.Distance(sample, pos) <= 20)
            {
                return 2;
            }    
            
            return 0;
        }
        public AnchorPoint(double x = 0, double y = 0, bool rotable = false, string type = "")
        {
            this.x = x;
            this.y = y;
            this.rotable = rotable;
            this.type = type;
        }
        public AnchorPoint Copy()
        {
            AnchorPoint temp = new AnchorPoint();
            temp.x = x;
            temp.y = y;
            temp.rotable = rotable;
            temp.type = type;
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
            var left = x - 3;
            var top = y - 3;

            var width = 6;
            var height = 6;

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(brush),
                Fill = new SolidColorBrush(fillbrush)
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(angle);

            rect.RenderTransform = transform;

            return rect;
        }
    }
}
