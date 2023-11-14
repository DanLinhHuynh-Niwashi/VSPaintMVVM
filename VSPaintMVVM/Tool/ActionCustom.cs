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
            //drawAction
            if (beforeShape.Count == 0)
            {
                foreach (var drawedShape in afterShape)
                {
                    shapeList.Remove(drawedShape);
                }
                return;
            }

            //deleteAction
            if (afterShape.Count == 0)
            {
                int postPosition = 0;
                foreach (var deletedShape in beforeShape)
                {
                    shapeList.Add(null);
                }

                foreach (var deletedShape in beforeShape)
                {
                    int position = pos[beforeShape.IndexOf(deletedShape)];
                    shapeList.Insert(position, deletedShape);
                }

                foreach (var shape in shapeList.ToList())
                {
                    if (shape == null)
                        shapeList.Remove(shape);
                }
            }

            for (int j = 0; j < shapeList.Count; j++)
            {
                var shape = shapeList[j];
                foreach (var i in ids)
                {
                    if (((ShapeCustom)shape).ID == i)
                    {
                        int a = ids.IndexOf(i);
                        found = true;
                        ShapeCustom temp = shape as ShapeCustom;
                        temp.CopyFrom((ShapeCustom)beforeShape[a]);
                    }
                }
            }
           
            return;
        }
        public virtual void ctrlY(List<ITool> shapeList) 
        {
            //deleteAction
            if (afterShape.Count == 0)
            {
                foreach (var deletedShape in beforeShape)
                {
                    shapeList.Remove(deletedShape);
                }
                return;
            }

            //drawAction
            if (beforeShape.Count == 0)
            {
                foreach (var drawedShape in afterShape)
                {
                    shapeList.Insert(posA[afterShape.IndexOf(drawedShape)], drawedShape);
                }
                return;
            }
            for (int j = 0; j < shapeList.Count; j++)
            {
                var shape = shapeList[j];
                foreach (var i in ids)
                {
                    if (((ShapeCustom)shape).ID == i)
                    {
                        

                        int a = ids.IndexOf(i);
                        ShapeCustom temp = shape as ShapeCustom;
                        temp.CopyFrom((ShapeCustom)afterShape[a]);
                    }
                }
            }


            
            return;
        }
    }
}
