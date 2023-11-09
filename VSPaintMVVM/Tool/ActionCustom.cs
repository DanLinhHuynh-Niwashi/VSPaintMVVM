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
        public List<string> ids = new List<string>();
        public List<int> pos = new List<int>();
        public List<int> posA = new List<int>();

        public void addStarter(ITool before = null, int befPos = -1, string ID = "")
        {
            if (before != null || ID == "" || befPos < 0) { }
            else
            {
                beforeShape.Add(before);
                ids.Add(ID);
                pos.Add(befPos);
            }
        }

        public void addAfter(ITool after = null, int afPos = -1)
        {
            if (after != null || afPos < 0) { }
            else
            {
                afterShape.Add(after);
                posA.Add(afPos);
            }
        }
        public virtual void ctrlZ(List<ITool> shapeList)
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
                            return;
                        }

                        int a = ids.IndexOf(i);
                        found = true;
                        ShapeCustom temp = shape as ShapeCustom;
                        temp.BoxStart = ((ShapeCustom)beforeShape[a]).BoxStart;
                        temp.BoxEnd = ((ShapeCustom)beforeShape[a]).BoxEnd;
                        temp.Angle = ((ShapeCustom)beforeShape[a]).Angle;
                        shape.Thickness = (beforeShape[a]).Thickness;
                        shape.Brush = (beforeShape[a]).Brush;
                    }
                }
            }
            //deleteAction
            if (found==false)
            {
                foreach (var deletedShape in beforeShape)
                {
                    shapeList.Insert(pos[beforeShape.IndexOf(deletedShape)], deletedShape);
                }
            }    
            return;
        }
        public virtual void ctrlY(List<ITool> shapeList) 
        {
            bool found = false;
            for (int j = 0; j < shapeList.Count; j++)
            {
                var shape = shapeList[j];
                foreach (var i in ids)
                {
                    if (((ShapeCustom)shape).ID == i)
                    {
                        //deleteAction
                        if (afterShape.Count == 0)
                        {
                            shapeList.Remove(shape);                            
                            return;
                        }

                        int a = ids.IndexOf(i);
                        found = true;
                        ShapeCustom temp = shape as ShapeCustom;
                        temp.BoxStart = ((ShapeCustom)afterShape[a]).BoxStart;
                        temp.BoxEnd = ((ShapeCustom)afterShape[a]).BoxEnd;
                        temp.Angle = ((ShapeCustom)afterShape[a]).Angle;
                        shape.Thickness = (afterShape[a]).Thickness;
                        shape.Brush = (afterShape[a]).Brush;
                    }
                }
            }


            //drawAction
            if (found == false)
            {
                foreach (var drawedShape in afterShape)
                {
                    shapeList.Insert(posA[afterShape.IndexOf(drawedShape)], drawedShape);
                }
            }
            return;
        }
    }
}
