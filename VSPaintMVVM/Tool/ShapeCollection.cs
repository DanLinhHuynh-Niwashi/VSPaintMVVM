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
            ITool star = new StarCustom();
            ITool img = new ImageImportCustom();
            prototypes.Add(line.Name, line);
            prototypes.Add(rect.Name, rect);
            prototypes.Add(ellipse.Name, ellipse);
            prototypes.Add(triangle.Name, triangle);
            prototypes.Add(pentagon.Name, pentagon);
            prototypes.Add(star.Name, star);
            prototypes.Add(img.Name, img);
        }

        public ITool Create(string id)
        {
            if (prototypes[id] is ITool)
            {
                return prototypes[id].Clone();
            }
            return null;
        }

        public ITool getShape(string id)
        {
            if (prototypes[id] is ITool && id!= new ImageImportCustom().Name)
            {
                return prototypes[id];
            }
            return null;
        }

        public Dictionary<string, int> reset ()
        {
            Dictionary<string, int> pre = new Dictionary<string, int>();
            foreach (ITool proto in prototypes.Values)
            {
                pre.Add(proto.Name, proto.IdIndex);
                proto.IdIndex = 0;
                
            }

            return pre;
        }

        public void restore (Dictionary<string, int> pre)
        {
            foreach (string id in pre.Keys)
            {
                if (prototypes[id] is ITool)
                {
                    prototypes[id].IdIndex = pre[id];
                }
            }
        }
    }
}
