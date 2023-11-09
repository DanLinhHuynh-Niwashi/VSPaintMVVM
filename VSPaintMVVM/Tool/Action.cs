using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPaintMVVM.Tool
{
    public class Action
    {
        protected ITool beforeShape = null;
        protected ITool afterShape = null;
        protected double beforeVar;
        protected double afterVar;
        protected string id;

        public ITool BeforeShape
        { set {  beforeShape = value; } }
        public ITool AfterShape
        { set {  afterShape = value; } }
        public string Id
        { get { return id; }  set { id = value; } }

        public virtual double ctrlZ(ITool shape)
        { shape = beforeShape;
            return beforeVar;
        }
        public virtual double ctrlY(ITool shape) 
        { shape = afterShape; 
            return afterVar;
        }
    }
}
