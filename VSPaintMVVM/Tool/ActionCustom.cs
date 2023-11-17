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

        public virtual void ctrlZ(List<ITool> shapeList)
        {
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
                        ShapeCustom? temp = shape as ShapeCustom;
                        if (temp == null) return;
                        temp.CopyFrom((ShapeCustom)beforeShape[a]);
                        if (pos[ids.IndexOf(i)] != posA[ids.IndexOf(i)])
                        {
                            Swap(shapeList, pos[ids.IndexOf(i)], posA[ids.IndexOf(i)]);
                            j++;
                        }    
                        
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
                    shapeList.Add(null);
                }

                foreach (var drawedShape in afterShape)
                {
                    int position = posA[afterShape.IndexOf(drawedShape)];
                    shapeList.Insert(position, drawedShape);
                }

                foreach (var shape in shapeList.ToList())
                {
                    if (shape == null)
                        shapeList.Remove(shape);
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
                        ShapeCustom? temp = shape as ShapeCustom;
                        if (temp == null) return;
                        temp.CopyFrom((ShapeCustom)afterShape[a]);
                        if (pos[ids.IndexOf(i)] != posA[ids.IndexOf(i)])
                        {
                            Swap(shapeList, pos[ids.IndexOf(i)], posA[ids.IndexOf(i)]);
                            j++;
                        }

                    }
                }
            }


            
            return;
        }

        private void Swap(List<ITool> list, int index1, int index2)
        {
            ITool temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
    }
}
