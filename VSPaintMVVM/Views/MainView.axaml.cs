using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using VSPaintMVVM.Tool;
using VSPaintMVVM.Shapes;
using OpenTK.Input;
using Avalonia.Input;
using Avalonia.Media;
using System;
using Avalonia.Interactivity;

namespace VSPaintMVVM.Views;

public partial class MainView : UserControl
{

    private bool isDrawing = false;
    private bool isSelecting = false;
    private List<ITool> shapeList = new List<ITool>();
    private Stack<ITool> shapeUndoStack = new Stack<ITool>();
    private Stack<ITool> shapeRedoStack = new Stack<ITool>();

    private static int currentThickness = 3;
    private ITool drawingShape = null;
    private string selectedShapeName = "Rectangle";

    private static SolidColorBrush currentColor = new SolidColorBrush(Colors.Black);
    ShapeCollection shapeCollection = new ShapeCollection();
    public MainView()
    {
        InitializeComponent();
        canvas.Height = 700; canvas.Width = 700;
        canvasContainer.Height = canvas.Height + 2000; canvasContainer.Width = canvas.Width + 2000;
        drawingShape = shapeCollection.Create(selectedShapeName);
    }
    class ShapeCollection
    {
        Dictionary<string, ITool> prototypes;
        public ShapeCollection()
        {
            prototypes = new Dictionary<string, ITool>();

            ITool rect = new RectangleCustom();

            prototypes.Add(rect.Name, rect);
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
    
    //Color slider
    private void BlueSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        BText.Text = ((int)(BSlider.Value / 100 * 255)).ToString();
        ColorChange();
    }
    private void RedSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        RText.Text = ((int)(RSlider.Value / 100 * 255)).ToString();
        ColorChange();
    }
    private void GreenSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        GText.Text = ((int)(GSlider.Value / 100 * 255)).ToString();
        ColorChange();
    }

    private void ColorChange()
    {
        byte r = (byte)(RSlider.Value / 100 * 255);
        byte g = (byte)(GSlider.Value / 100 * 255);
        byte b = (byte)(BSlider.Value / 100 * 255);
        Color tempC = new Color(255, r, g, b);
        strokeColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        currentColor = new SolidColorBrush(tempC);
    }

    //Brush size slider
    private void BrushSizeSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        currentThickness = (int)(brushSlider.Value);
    }

    //selection tool
    private void Tool_Checked(object sender, RoutedEventArgs e)
    {
        if ((bool)SelB.IsChecked)
        {
            isSelecting = true;
        }    
        else
        {
            isSelecting = false;
        }
    }
    private void canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        isDrawing = true;
        Point pos = e.GetPosition(canvas);

        drawingShape.StartCorner(pos.X, pos.Y);
        
    }

    private void canvas_PointerMoved(object sender, PointerEventArgs e)
    {
        if (isDrawing)
        {
            Point pos = e.GetPosition(canvas);

            drawingShape.EndCorner(pos.X, pos.Y);

            Redraw();

            canvas.Children.Add(drawingShape.Draw(currentColor, currentThickness));
        }
    }

    private void canvas_PointerReleased(object sender, PointerReleasedEventArgs e)
    {

        Point pos = e.GetPosition(canvas);
        drawingShape.EndCorner(pos.X, pos.Y);

        // add to list
        drawingShape.Brush = currentColor;
        drawingShape.Thickness = currentThickness;
        shapeList.Add(drawingShape);
        shapeUndoStack.Push(drawingShape);

        // new ready to draw shape
        drawingShape = shapeCollection.Create(selectedShapeName);

        Redraw();
        isDrawing=false;
    }

    private void Redraw()
    {
        canvas.Children.Clear();

        foreach (var shape in shapeList)
        {
            var element = shape.Draw(shape.Brush, shape.Thickness);
            canvas.Children.Add(element);
        }
    }


}