using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPaintMVVM.Tool
{
    public class ShapeCustom
    {
        
        protected PointCustom boxStart = new PointCustom();
        protected PointCustom boxEnd = new PointCustom();
        protected double angle = 0;
        protected string id;
        protected PriorityQueue<string, int> posible_id_list = new PriorityQueue<string, int>();

        protected List<AnchorPoint> apoints;
        protected List<AnchorPoint> showingApoints;
        virtual public void CreateAnchorPoints()
        {
            var left = Math.Min(boxStart.x, boxEnd.x);
            var top = Math.Min(boxStart.y, boxEnd.y);

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            apoints = new List<AnchorPoint>()
            {
                new AnchorPoint(left, top, true, "tl"),
                new AnchorPoint(right, top, true, "tr"),
                new AnchorPoint(left, bottom , true, "bl"),
                new AnchorPoint(right, bottom, true, "br"),
                new AnchorPoint(BoxCenter().x, BoxCenter().y, false, ""),
                new AnchorPoint(BoxCenter().x, top, false, "tc"),
                new AnchorPoint(BoxCenter().x, bottom, false, "bc"),   
                new AnchorPoint(left, BoxCenter().y, false, "lc"),
                new AnchorPoint(right, BoxCenter().y, false, "rc"),
            };

            showingApoints = new List<AnchorPoint>();
        }

        public string next_Possible_ID()
        {
            return posible_id_list.Dequeue();
        }

        public PointCustom BoxStart
        {
            get { return boxStart; }
            set { boxStart = value; }
        }
        public List<AnchorPoint> APoints
        {
            get { return apoints; }
        }

        public List<AnchorPoint> ShowingAPoints
        {
            get { return showingApoints; }
        }

        public PointCustom BoxEnd
        {
            get { return boxEnd; }
            set { boxEnd = value; }
        }

        public string ID
        {
            get => id;
            set => id = value;
        }

        public double Angle
        {
            get => angle;
            set => angle = value;
        }

        virtual public PointCustom BoxCenter()
        {
            PointCustom boxCenter = new PointCustom();

            boxCenter.x = (boxStart.x + boxEnd.x) / 2;
            boxCenter.y = (boxStart.y + boxEnd.y) / 2;
            return boxCenter;
        }

        virtual public bool isHovering (Avalonia.Point pos)
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

        public List<Control> drawAnchorPoint()
        {
            CreateAnchorPoints();
            List<Control> anchor = new List<Control>();
            showingApoints.Clear();

            foreach (AnchorPoint aPoint in apoints)
            {
                var a = angle * Math.PI / 180.0;
                float cosa = (float)Math.Cos(a);
                float sina = (float)Math.Sin(a);

                double centerX = BoxCenter().x;
                double centerY = BoxCenter().y;

                AnchorPoint newPoint = aPoint.Copy();
                newPoint.x = (aPoint.x - centerX) * cosa - (aPoint.y - centerY) * sina + centerX;
                newPoint.y = (aPoint.x - centerX) * sina + (aPoint.y - centerY) * cosa + centerY;
                newPoint.angle = angle;
                showingApoints.Add(newPoint);
                anchor.Add(newPoint.Draw(new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.White), 1));
            }    
            
            return anchor;
        }

        virtual public Control drawChosenLine()
        {
            var left = Math.Min(boxStart.x, boxEnd.x) - 2;
            var top = Math.Min(boxStart.y, boxEnd.y) - 2;

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            var width = right - left;
            var height = bottom - top;

            var rect = new Avalonia.Controls.Shapes.Rectangle()
            {
                Width = width+1,
                Height = height+1,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.BlueViolet),
                StrokeDashArray = new AvaloniaList<double> { 5,5,5,5}
              
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(angle);

            rect.RenderTransform = transform;

            return rect;
        }
        virtual public ShapeCustom Copy()
        {
            ShapeCustom temp = new ShapeCustom()
            {
                BoxStart = boxStart,
                BoxEnd = boxEnd,
                Angle = angle,
            };

            return temp;
        }

        virtual public void CopyFrom(ShapeCustom shape)
        {
            BoxStart = shape.boxStart.Copy();
            BoxEnd = shape.boxEnd.Copy();
            Angle = shape.angle;
        }
    }
}
