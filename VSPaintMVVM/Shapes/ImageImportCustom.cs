using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using VSPaintMVVM.Tool;


namespace VSPaintMVVM.Shapes
{
    public class ImageImportCustom : ShapeCustom, ITool
    {
        public static int idIndex = 0;

        private Bitmap btm;
        public string Icon => "Assets/Icons/ToolIcon/rectangle.png";
        public string Name => "Image";

        public int IdIndex { get { return idIndex; } set { idIndex = value; } }
        public Bitmap Bitmap {
            get { return btm; }
            set { btm = value; } 
        }
        public Color Brush { get; set; }

        public Color FillBrush { get; set; }
        public int Thickness { get; set; }

        public ITool Clone()
        {
            idIndex = idIndex + 1;
            posible_id_list.Enqueue(Name + idIndex.ToString(), idIndex);
            ImageImportCustom img = new ImageImportCustom();

            img.id = next_Possible_ID();
            return img;
        }


        public override ImageImportCustom Copy()
        {
            ImageImportCustom temp = new ImageImportCustom();
            temp.BoxStart = boxStart.Copy();
            temp.BoxEnd = boxEnd.Copy();
            temp.Angle = angle;
            temp.Bitmap = btm;

            return temp;
        }

        public override void CopyFrom(ShapeCustom shape)
        {
            base.CopyFrom(shape);
            Bitmap = ((ImageImportCustom)shape).Bitmap;
        }

        public Control Draw(Color brush, Color fillBrush, int thickness)
        {
            var left = Math.Min(boxStart.x, boxEnd.x);
            var top = Math.Min(boxStart.y, boxEnd.y);

            var right = Math.Max(boxStart.x, boxEnd.x);
            var bottom = Math.Max(boxStart.y, boxEnd.y);

            var width = right - left;
            var height = bottom - top;

            

            var rect = new Image()
            {
                Width = width,
                Height = height,
                Stretch = Stretch.Fill,
                Source = btm,
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(angle);

            rect.RenderTransform = transform;

            return rect;
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
