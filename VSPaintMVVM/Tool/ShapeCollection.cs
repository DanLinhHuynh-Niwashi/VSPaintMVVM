using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSPaintMVVM.Shapes;

namespace VSPaintMVVM.Tool
{
    public class ShapeCollection
    {
        public Dictionary<string, ITool> prototypes;
        public ShapeCollection()
        {
            prototypes = new Dictionary<string, ITool>();

            ITool rect = new RectangleCustom();
            ITool ellipse = new EllipseCustom();
            ITool triangle = new TriangleCustom();
            ITool pentagon = new PentagonCustom();
            ITool line = new LineCustom();
            prototypes.Add(line.Name, line);
            prototypes.Add(rect.Name, rect);
            prototypes.Add(ellipse.Name, ellipse);
            prototypes.Add(triangle.Name, triangle);
            prototypes.Add(pentagon.Name, pentagon);
            
        }

        public ITool Create(string id)
        {
            if (prototypes[id] is ITool)
            {
                return prototypes[id].Clone();
            }
            return null;
        }
    }
}
