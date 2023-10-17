using Avalonia;
using Avalonia.Collections;
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
    public class ShapeCustom
    {
        protected PointCustom boxStart = new PointCustom();
        protected PointCustom boxEnd = new PointCustom();
        protected bool isChosen;
        public PointCustom BoxStart
        {
            get { return boxStart; }
            set { boxStart = value; }
        }

        public PointCustom BoxEnd
        {
            get { return boxEnd; }
            set { boxEnd = value; }
        }

        public bool IsChosen
        {
            get => isChosen;
            set => isChosen = value;
        }    

        virtual public PointCustom BoxCenter()
        {
            PointCustom boxCenter = new PointCustom();

            boxCenter.x = (boxStart.x + boxEnd.x) / 2;
            boxCenter.y = (boxStart.y + boxEnd.y) / 2;
            return boxCenter;
        }

        virtual public bool isHovering (Point pos)
        {
            double x1, x2, y1, y2;
            x1 = BoxStart.x;
            x2 = BoxEnd.x;
            y1 = BoxStart.y;
            y2 = BoxEnd.y;
            if (BoxEnd.x > BoxStart.x)
            {
                x1 = BoxEnd.x;
                x2 = BoxStart.x;
            }
            if (BoxEnd.y > BoxStart.y)
            {
                y1 = BoxEnd.y;
                y2 = BoxStart.y;
            }
            return pos.X >= x2 && pos.X <= x1 && pos.Y >= y2 && pos.Y <= y1; 
        }

        virtual public Control drawChosenLine()
        {
            var left = Math.Min(boxStart.x, boxEnd.x) - 2;
            var top = Math.Min(boxStart.y, boxEnd.y) - 2;

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            var width = right - left;
            var height = bottom - top;

            var rect = new Rectangle()
            {
                Width = width+2,
                Height = height+2,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.BlueViolet),
                StrokeDashArray = new AvaloniaList<double> { 5,5,5,5}
              
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            return rect;
        }
        virtual public ShapeCustom Copy()
        {
            ShapeCustom temp = new ShapeCustom()
            {
                BoxStart = boxStart,
                BoxEnd = boxEnd,
            };

            return temp;
        }
    }
}
