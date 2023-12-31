﻿using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using Avalonia;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class StarCustom : ShapeCustom, ITool
    {
        public static int idIndex = 0;
        public string Icon => "Assets/Icons/ToolIcon/star.png";
        public string Name => "Star";

        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public int Thickness { get; set; }

        public int IdIndex { get { return idIndex; } set { idIndex = value; } }
        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            StarCustom star = new StarCustom();

            star.id = next_Possible_ID();
            return star;
        }


        public override StarCustom Copy()
        {
            StarCustom temp = new StarCustom();
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


            var star = new Polygon()
            {
                Points =
                {
                    new Point(width/2,0),
                    new Point(width*4/5, height),
                    new Point (0, height*2/5),
                    new Point (width, height*2/5),
                    new Point(width/5,height),

                    
                },
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Fill = new SolidColorBrush(fillBrush),
                Stroke = new SolidColorBrush(brush),
            };

            Canvas.SetLeft(star, left);
            Canvas.SetTop(star, top);

            RotateTransform transform = new RotateTransform(angle);

            star.RenderTransform = transform;

            return star;
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
