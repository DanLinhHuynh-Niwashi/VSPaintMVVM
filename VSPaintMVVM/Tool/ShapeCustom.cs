using Avalonia;
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


        virtual public PointCustom BoxCenter()
        {
            PointCustom boxCenter = new PointCustom();

            boxCenter.x = (boxStart.x + boxEnd.x) / 2;
            boxCenter.y = (boxStart.y + boxEnd.y) / 2;
            return boxCenter;
        }

        virtual public ShapeCustom Copy()
        {
            ShapeCustom temp = new ShapeCustom()
            {
                BoxStart = boxStart, BoxEnd = boxEnd,
            };

            return temp;
        }
    }
}
