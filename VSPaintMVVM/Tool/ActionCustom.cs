using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPaintMVVM.Tool
{
    public class ActionCustom
    {
        public List<ITool> beforeShape = new List<ITool>();
        public List<ITool> afterShape = new List<ITool>();
        public double beforeVar;
        public double afterVar;
        public List<string> ids = new List<string>();

        public virtual double ctrlZ(List<ITool> shapeList, int index)
        {
            bool found = false;
            for (int j = 0; j < shapeList.Count; j++)
            {
                var shape = shapeList[j];
                foreach (var i in ids)
                {
                    if (((ShapeCustom)shape).ID == i)
                    {
                        //drawAction
                        if (beforeShape.Count == 0)
                        {
                            shapeList.Remove(shape);
                            return -1;
                        }

                        int a = ids.IndexOf(i);
                        found = true;
                        ShapeCustom temp = shape as ShapeCustom;
                        temp.BoxStart = ((ShapeCustom)beforeShape[a]).BoxStart;
                        temp.BoxEnd = ((ShapeCustom)beforeShape[a]).BoxEnd;
                        temp.Angle = ((ShapeCustom)beforeShape[a]).Angle;
                    }
                }
            }
               

            //deleteAction
            if (found==false)
            {
                foreach (var deletedShape in beforeShape)
                {
                    shapeList.Add(deletedShape);
                }
            }    
            return beforeVar;
        }
        public virtual double ctrlY(ITool shape, string id) 
        {

            return afterVar;
        }
    }
}
