using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPaintMVVM.Tool
{
    public interface ITool
    {
        string Name { get; }
        string Icon { get; }

        Color Brush { get; set; }

        Color FillBrush { get; set; }

        int Thickness { get; set; }

        int IdIndex { get; set; }
        ITool Clone();
        Control Draw(Color brush, Color fillBrush, int thickness);
        void StartCorner(double x, double y);
        void EndCorner(double x, double y);
    }
}
