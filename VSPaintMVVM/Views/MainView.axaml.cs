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
using System.Linq;

namespace VSPaintMVVM.Views;

public partial class MainView : UserControl
{

    private bool isDrawing = false;
    private bool isSelecting = false;
    private bool isErasing = false;
    private bool isTransforming = false;
    private bool isChangingFromText = false;

    private List<ITool> shapeList = new List<ITool>();
    private List<ShapeCustom> chosenList = new List<ShapeCustom>();

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
        if (isChangingFromText)
            isChangingFromText = false;
        else
            BText.Text = ((int)BSlider.Value).ToString();
        ColorChange();
    }
    private void RedSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (isChangingFromText)
            isChangingFromText = false;
        else
            RText.Text = ((int)RSlider.Value).ToString();
        ColorChange();
    }
    private void GreenSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (isChangingFromText)
            isChangingFromText = false;
        else
            GText.Text = ((int)GSlider.Value).ToString();
        ColorChange();
    }

    //Color textbox
    private void BlueTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double blueValue;
        if (double.TryParse(BText.Text, out blueValue))
        {
            isChangingFromText = true;
            BSlider.Value = blueValue;
            ColorChange();
        }
    }
    private void RedTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double redValue;
        if (double.TryParse(RText.Text, out redValue))
        {
            isChangingFromText = true;
            RSlider.Value = redValue;
            ColorChange();
        }
    }
    private void GreenTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double greenValue;
        if (double.TryParse(GText.Text, out greenValue))
        {
            isChangingFromText = true;
            GSlider.Value = greenValue;
            ColorChange();
        }
    }

    private void ColorChange()
    {
        byte r = (byte)RSlider.Value;
        byte g = (byte)GSlider.Value;
        byte b = (byte)BSlider.Value;
        Color tempC = new Color(255, r, g, b);
        strokeColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        currentColor = new SolidColorBrush(tempC);

        if (isSelecting)
        {
            foreach (var shape in chosenList)
            {
                var element = shape as ITool;
                element.Brush = currentColor;
            }
            Redraw();
        }
    }

    //Brush size slider
    private void BrushSizeSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        currentThickness = (int)BrushSlider.Value;
    }

    //drawingtool
    private void DrawTool_Checked(object sender, RoutedEventArgs e)
    {
        isErasing = false;
        isSelecting = false;
        isTransforming = false;
        TransFB.IsChecked = false;
    }


    //selection + deletion tool
    private void Tool_Checked(object sender, RoutedEventArgs e)
    {
        if ((bool)TransFB.IsChecked)
        {
            isDrawing = false;
            isSelecting = false;
            isErasing = false;
            isTransforming = true;
            Redraw();
            return;
        }
        else
        {
            if ((bool)SelB.IsChecked)
            {
                isDrawing = false;
                isSelecting = true;
                isErasing = false;
                isTransforming = false;
            }
            else if ((bool)EraserB.IsChecked)
            {
                isDrawing = false;
                isSelecting = false;
                isErasing = true;
                isTransforming = false;
            }
        }
    }

    private void SelectAll_Clicked(object sender, RoutedEventArgs e)
    {
        foreach (var shape in Enumerable.Reverse(shapeList))
        {
            ShapeCustom element = shape as ShapeCustom;
            if (!chosenList.Contains(element))
                chosenList.Add(element);
        }
        
        Redraw() ;
    }

    private void DeselectAll_Clicked(object sender, RoutedEventArgs e)
    {
        foreach (var shape in Enumerable.Reverse(chosenList))
        {
                chosenList.Remove(shape);
        }
        TransFB.IsChecked = false;
        isTransforming = false;
        Redraw();
    }

    AnchorPoint chosenAPoint;
    private void canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(canvas);
        if (!isSelecting && !isErasing && !isTransforming)
        {
            isDrawing = true;
            drawingShape.StartCorner(pos.X, pos.Y);
        } 
        else if (isTransforming)
        {
            if (chosenList.Count == 1)
            {
                foreach (var shape in chosenList)
                {
                    foreach (var apoint in shape.APoints)
                    {
                        if (apoint.isHovering(pos) == 1)
                        {
                            chosenAPoint = apoint;
                        }
                    }
                }
            }
        }    
        
    }

    private void Resizing(AnchorPoint chosenAPoint, ShapeCustom shape, Point pos)
    {
        var left = Math.Min(shape.BoxStart.x, shape.BoxEnd.x);
        var top = Math.Min(shape.BoxStart.y, shape.BoxEnd.y);

        var right = Math.Max(shape.BoxStart.x, shape.BoxEnd.x);
        var bottom = Math.Max(shape.BoxStart.y, shape.BoxEnd.y);
        switch (chosenAPoint.type)
        {
            case "tl":
                if (pos.X > right || pos.Y > bottom)
                    break;
                if (shape.BoxStart.x == left)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                if (shape.BoxStart.y == top)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.x = pos.X;
                chosenAPoint.y = pos.Y;
                break;

            case "tr":
                if (pos.X < left || pos.Y > bottom)
                    break;
                if (shape.BoxStart.x == right)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                if (shape.BoxStart.y == top)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.x = pos.X;
                chosenAPoint.y = pos.Y;
                break;
            case "bl":
                if (pos.X > right || pos.Y < top)
                    break;
                if (shape.BoxStart.x == left)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                if (shape.BoxStart.y == bottom)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.x = pos.X;
                chosenAPoint.y = pos.Y;
                break;

            case "br":
                if (pos.X < left || pos.Y < top)
                    break;
                if (shape.BoxStart.x == right)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                if (shape.BoxStart.y == bottom)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.x = pos.X;
                chosenAPoint.y = pos.Y;
                break;

            case "tc":
                if (pos.Y > bottom)
                    break;

                if (shape.BoxStart.y == top)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.y = pos.Y;
                break;
            case "bc":
                if (pos.Y < top)
                    break;

                if (shape.BoxStart.y == bottom)
                {
                    shape.BoxStart.y = pos.Y;
                }
                else
                {
                    shape.BoxEnd.y = pos.Y;
                }

                chosenAPoint.y = pos.Y;
                break;

            case "lc":
                if (pos.X > right)
                    break;

                if (shape.BoxStart.x == left)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                chosenAPoint.x = pos.X;
                break;
            case "rc":
                if (pos.X < left)
                    break;

                if (shape.BoxStart.x == right)
                {
                    shape.BoxStart.x = pos.X;
                }
                else
                {
                    shape.BoxEnd.x = pos.X;
                }

                chosenAPoint.x = pos.X;
                break;
        }

        Redraw();
    }
    private void canvas_PointerMoved(object sender, PointerEventArgs e)
    {

        Point pos = e.GetPosition(canvas);

        if (isSelecting || isErasing)
        {
            Redraw();
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    if (!chosenList.Contains(element))
                        canvas.Children.Add(((ShapeCustom)shape).drawChosenLine());
                    break;
                }
                
            }
        }
        else if (isTransforming)
        {

            if (chosenList.Count == 1 && chosenAPoint!=null)
            {
                foreach (var shape in chosenList)
                {
                    Resizing(chosenAPoint, shape, pos);
                }
            }    
               
        }    
        else if (isDrawing)
        {
            chosenList.Clear();
            

            drawingShape.EndCorner(pos.X, pos.Y);

            Redraw();

            canvas.Children.Add(drawingShape.Draw(currentColor, currentThickness));
        }
    }

    private void canvas_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if(isSelecting)
        {
            Point pos = e.GetPosition(canvas);
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    if (chosenList.Contains(element))
                        chosenList.Remove(element);
                    else
                        chosenList.Add(element);
                    break;
                }
            }
            Redraw();
        }

        else if (isErasing)
        {
            Point pos = e.GetPosition(canvas);
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    shapeList.Remove(shape);
                    break;
                }
            }
            Redraw();
        }
        
        else if (isTransforming)
        {
            chosenAPoint = null;
        }    
        else
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
            isDrawing = false;
        }    
    }

    private void Redraw()
    {
        canvas.Children.Clear();

        foreach (var shape in shapeList)
        {
            var element = shape.Draw(shape.Brush, shape.Thickness);
            canvas.Children.Add(element);
            if (chosenList.Contains(((ShapeCustom)shape)))
            {
                canvas.Children.Add(((ShapeCustom)shape).drawChosenLine());
            }
            if (isTransforming)
            {
                if (chosenList.Count != 1)
                {
                    continue;
                }
                else
                {
                    foreach (var aPoint in ((ShapeCustom)chosenList[0]).drawAnchorPoint())
                    {
                        canvas.Children.Add(aPoint);
                    }
                }
            }
        }
    }


}