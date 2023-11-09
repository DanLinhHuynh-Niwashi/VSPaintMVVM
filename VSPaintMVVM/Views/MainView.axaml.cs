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


            foreach (var shape in chosenList)
            {
                var element = shape as ITool;
                element.Brush = currentColor;
            }
            Redraw();

    }

    //Brush size slider
    private void BrushSizeSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        currentThickness = (int)BrushSlider.Value;


            foreach (var shape in chosenList)
            {
                var element = shape as ITool;
                element.Thickness = currentThickness;
            }
            Redraw();


    }

    bool preIsDrawing, preIsSelecting, preIsErasing;
    private void TransF_Move_Checked(object sender, RoutedEventArgs e)
    {
        
        if ((bool)TransFB.IsChecked)
        {
            preIsDrawing = isDrawing; preIsSelecting = isSelecting; preIsErasing = isErasing;
            isDrawing = false;
            isSelecting = false;
            isErasing = false;
            isTransforming = true;
        }
        else
        {
            isDrawing = preIsDrawing;
            isSelecting = preIsSelecting;
            isErasing = preIsErasing;
            isTransforming = false;
        }
        Redraw();
    }


    //selection + deletion tool
    private void Tool_Checked(object sender, RoutedEventArgs e)
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
            else if ((bool)PenB.IsChecked)
            {
                isErasing = false;
                isSelecting = false;
                isTransforming = false;
                
            }
        TransFB.IsChecked = false;
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

    Tuple<AnchorPoint, int, AnchorPoint> chosenAPoint;
    Point startingPos;
    bool isMoving = false;
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
                    foreach (var apoint in shape.ShowingAPoints)
                    {
                        if (apoint.isHovering(pos) != 0)
                        {
                            int i = shape.ShowingAPoints.IndexOf(apoint);
                            isMoving = false;
                            chosenAPoint = new Tuple<AnchorPoint, int, AnchorPoint>(shape.APoints[i], apoint.isHovering(pos), shape.APoints[i].Copy());
                            break;
                        }
                        else
                        {
                            isMoving = true;
                        }
                            
                    }
                }
            }
            else
            {
                isMoving = true;
            }

            startingPos = pos;
        }    
        
    }

    private void Resizing(AnchorPoint chosenAPoint, ShapeCustom shape, Point pos, AnchorPoint originalPoint)
    {
        var left = Math.Min(shape.BoxStart.x, shape.BoxEnd.x);
        var top = Math.Min(shape.BoxStart.y, shape.BoxEnd.y);

        var right = Math.Max(shape.BoxStart.x, shape.BoxEnd.x);
        var bottom = Math.Max(shape.BoxStart.y, shape.BoxEnd.y);

        var width = right - left;
        var height = bottom - top;

        

        var angle = shape.Angle;
        var a = angle * Math.PI / 180.0;
        float cosa = (float)Math.Cos(-a);
        float sina = (float)Math.Sin(-a);

        double centerX = shape.BoxCenter().x;
        double centerY = shape.BoxCenter().y;

        AnchorPoint finPos = new AnchorPoint();
        finPos.x = (pos.X - centerX) * cosa - (pos.Y - centerY) * sina + centerX;
        finPos.y = (pos.X - centerX) * sina + (pos.Y - centerY) * cosa + centerY;
      
        AnchorPoint stopPoint = new AnchorPoint();
        switch (chosenAPoint.type)
        {
            case "tl":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "br")
                    {
                        stopPoint = stop;
                        break;
                    }    
                }
                
                break;

            case "tr":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "bl")
                    {
                        stopPoint = stop;
                        break;
                    }
                }

                break;
            case "bl":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tr")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break;

            case "br":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tl")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break;

            case "tc":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "bc")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break;
            case "bc":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tc")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break;

            case "lc":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "rc")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break; 
            case "rc":
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "lc")
                    {
                        stopPoint = stop;
                        break;
                    }
                }
                break;
        }

        
        AnchorPoint newPoint = new AnchorPoint();
        switch (chosenAPoint.type)
        {
            case "tl":
                if (finPos.x >= right - currentThickness|| finPos.y >= bottom - currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == left)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                if (shape.BoxStart.y == top)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "br")
                    {
                        newPoint = stop;
                        break;
                    }
                }


                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;

            case "tr":
                if (finPos.x <= left + currentThickness || finPos.y >= bottom - currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == right)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                if (shape.BoxStart.y == top)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "bl")
                    {
                        newPoint = stop;
                        break;
                    }
                }

                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;

            case "bl":
                if (finPos.x >= right - currentThickness || finPos.y <= top + currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == left)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                if (shape.BoxStart.y == bottom)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tr")
                    {
                        newPoint = stop;
                        break;
                    }
                }
        

                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;
     

            case "br":
                if(finPos.x <= left + currentThickness || finPos.y <= top + currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == right)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                if (shape.BoxStart.y == bottom)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tl")
                    {
                        newPoint = stop;
                        break;
                    }
                }
         

                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;


            case "tc":
                if (finPos.y >= bottom - currentThickness)
                {
                    break;
                }

                if (shape.BoxStart.y == top)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "bc")
                    {
                        newPoint = stop;
                        break;
                    }
                }
                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;

            case "bc":
                if (finPos.y <= top + currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.y == bottom)
                    shape.BoxStart.y = finPos.y;
                else shape.BoxEnd.y = finPos.y;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "tc")
                    {
                        newPoint = stop;
                        break;
                    }
                }
                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
               
                break;

            case "lc":
                if (finPos.x >= right - currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == left)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "rc")
                    {
                        newPoint = stop;
                        break;
                    }
                }

                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;
            case "rc":
                if (finPos.x <= left + currentThickness)
                {
                    break;
                }
                if (shape.BoxStart.x == right)
                    shape.BoxStart.x = finPos.x;
                else shape.BoxEnd.x = finPos.x;

                shape.drawAnchorPoint();
                foreach (var stop in shape.ShowingAPoints)
                {
                    if (stop.type == "lc")
                    {
                        newPoint = stop;
                        break;
                    }
                }
                Moving(shape, new Point(newPoint.x, newPoint.y), new Point(stopPoint.x, stopPoint.y));
                break;
        }

        startingPos = new Point(pos.X, pos.Y);
        Redraw();
    }

    private void Moving(ShapeCustom shape, Point oriPos, Point afterPos)
    {
        double offsetX = oriPos.X - afterPos.X;
        double offsetY = oriPos.Y - afterPos.Y;

        shape.BoxEnd.x -= offsetX;
        shape.BoxStart.x -= offsetX;
        shape.BoxEnd.y -= offsetY;
        shape.BoxStart.y -= offsetY;

        Redraw();
    }

    private void Rotating(AnchorPoint chosenAPoint, ShapeCustom shape, Point pos, AnchorPoint originalPoint)
    {
        double rotatedAngle = Calculate_Angle(shape.BoxCenter(), pos, originalPoint);
        shape.Angle = rotatedAngle;
        Redraw();
    }

    private double Calculate_Angle (PointCustom center, Point endPoint, AnchorPoint originalPoint) 
    {
        double result = Math.Atan2(endPoint.Y - center.y, endPoint.X - center.x) 
            - Math.Atan2(originalPoint.y - center.y, originalPoint.x - center.x);
        return result * (180/Math.PI);
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
                    if (chosenAPoint.Item2 == 1) Resizing(chosenAPoint.Item1, shape, pos, chosenAPoint.Item3);
                    else if (chosenAPoint.Item2 == 2)
                    {
                        Rotating(chosenAPoint.Item1, shape, pos, chosenAPoint.Item3);
                    }
                }
            }
            else
            {
                foreach (var shape in chosenList)
                {
                    if (isMoving)
                    {
                        Moving(shape, startingPos, pos);
                        startingPos = pos;
                    }    
                        

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
            isMoving = false;
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