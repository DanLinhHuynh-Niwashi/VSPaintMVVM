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
using Avalonia.Visuals;
using System;
using Avalonia.Interactivity;
using System.Linq;

using System.Timers;

using Avalonia.Media.Imaging;

using Avalonia.Platform;

using System.Reflection;
using Avalonia.Media.Immutable;

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

    private Stack<ActionCustom> shapeUndoStack = new Stack<ActionCustom>();
    private Stack<ActionCustom> shapeRedoStack = new Stack<ActionCustom>();

    private static int currentThickness;
    private ITool drawingShape = null;
    private string selectedShapeName;

    private static SolidColorBrush currentColor = new SolidColorBrush();
    private static SolidColorBrush currentFill = new SolidColorBrush();

    ShapeCollection shapeCollection = new ShapeCollection();
    List<Button> toolCollection = new List<Button>();
    public MainView()
    {
        InitializeComponent();
        canvas.Height = 600; canvas.Width = 600;
        canvasContainer.Height = canvas.Height + 2000; canvasContainer.Width = canvas.Width + 2000;
        CreateTools();

        strokeCB.IsChecked = true;
        bool found = false;
        foreach (var tool in toolCollection)
        {
            if (tool.Background != null)
            {
                SolidColorBrush buttonColor = tool.Background as SolidColorBrush;
                if (buttonColor.Color == Colors.Violet)
                {
                    selectedShapeName = tool.Name;
                    found = true;
                    break;
                }
            }
        }
        if (found==false && toolCollection.Count>0)
        {
            toolCollection[0].Background = new SolidColorBrush(Colors.Violet);
            selectedShapeName = toolCollection[0].Name;
        }

    }
    private void CreateTools()
    {
        foreach (var shape in shapeCollection.prototypes)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Bitmap btm = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/" + shape.Value.Icon)));
            Image shapeImage = new Image()
            {
                Source = btm
            };

            Button shapeButton = new Button()
            {
                Name = shape.Value.Name,
                Width = 40,
                Height = 40,
                Content = shapeImage,
                Background = new SolidColorBrush(Colors.White),

            };
            shapeButton.Click += itemChoose;
            toolCollection.Add(shapeButton);
            shapeListContainer.Children.Add(shapeButton);
        }
    }
    private void DisableDrawing()
    {
        foreach (var tool in toolCollection)
        {
            tool.IsEnabled = false;
        }
    }
    private void EnableDrawing()
    {
        foreach (var tool in toolCollection)
        {
            tool.IsEnabled = true;
        }
    }
    private void itemChoose(object sender, RoutedEventArgs e)
    {
        foreach (var tool in toolCollection)
        {
            tool.Background = new SolidColorBrush(Colors.White);
            if (sender as Button == tool)
            {
                tool.Background = new SolidColorBrush(Colors.Violet);
                selectedShapeName = tool.Name.ToString();
            }
        }
    }

    //Color slider
    private void CB_Clicked(object sender, RoutedEventArgs e)
    {
        if ((bool)fillCB.IsChecked && fillColor.Fill != null)
        {
            ImmutableSolidColorBrush fill = (ImmutableSolidColorBrush)fillColor.Fill;
            BText.Text = fill.Color.B.ToString();
            RText.Text =fill.Color.R.ToString();
            GText.Text = fill.Color.G.ToString();
        }
        else if ((bool)strokeCB.IsChecked && strokeColor.Fill != null)
        {
            ImmutableSolidColorBrush stroke = (ImmutableSolidColorBrush)strokeColor.Fill;
            BText.Text = stroke.Color.B.ToString();
            RText.Text = stroke.Color.R.ToString();
            GText.Text = stroke.Color.G.ToString();
        }
    }
    private void Transparent_Click(object sender, RoutedEventArgs e)
    {
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB, 0);
        }
        else
        {
            ColorChange(strokeCB, 0);
        }
    }
    private void BlueSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        BText.Text = ((int)BSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB, 255);
        }
        else
        {
            ColorChange(strokeCB, 255);
        }    
    }
    private void RedSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        RText.Text = ((int)RSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB, 255);
        }
        else
        {
            ColorChange(strokeCB, 255);
        }
    }
    private void GreenSlider_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        GText.Text = ((int)GSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB, 255);
        }
        else
        {
            ColorChange(strokeCB, 255);
        }
    }

    //Color textbox
    private void BlueTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double blueValue;
        if (double.TryParse(BText.Text, out blueValue))
        {
            if (blueValue < 0) blueValue = 0;
            if (blueValue > 255) blueValue = 255;
            BSlider.Value = blueValue;
        }
    }
    private void RedTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double redValue;
        if (double.TryParse(RText.Text, out redValue))
        {
            if (redValue < 0) redValue = 0;
            if (redValue > 255) redValue = 255;
            RSlider.Value = redValue;
        }
    }
    private void GreenTextBox_Change(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        double greenValue;
        if (double.TryParse(GText.Text, out greenValue))
        {
            if (greenValue < 0) greenValue = 0;
            if (greenValue > 255) greenValue = 255;
            GSlider.Value = greenValue;
        }
    }


    bool isChanging = false;
    System.Timers.Timer timer;
    int timeCounting = 0;

    private void ColorChange(object sender, byte a)
    {

        byte r = (byte)RSlider.Value;
        byte g = (byte)GSlider.Value;
        byte b = (byte)BSlider.Value;
        Color tempC = new Color(a, r, g, b);
        SolidColorBrush prevColor = new SolidColorBrush();
        if (sender == fillCB)
        {
            prevColor = currentFill;
            fillColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        }
        else
        {
            prevColor = currentColor;
            strokeColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        }
        
        if (prevColor.Color == tempC) return;

        ChangedStart();

        if (sender == fillCB)
        {
            currentFill = new SolidColorBrush(tempC);
            fillColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        }
        else
        {
            currentColor = new SolidColorBrush(tempC);
            strokeColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        }

        foreach (var shape in chosenList)
        {
            var element = shape as ITool;
            element.FillBrush = currentFill;
            element.Brush = currentColor;
        }
        Redraw();

    }

    //Brush size slider
    private void BrushSizeSlider_Changed(object sender, AvaloniaPropertyChangedEventArgs e)
    {

        if (currentThickness == (int)BrushSlider.Value) return;
        ChangedStart();
        currentThickness = (int)BrushSlider.Value;
        BrushTextBox.Text = ((int)BrushSlider.Value).ToString();
        foreach (var shape in chosenList)
        {
            var element = shape as ITool;
            element.Thickness = currentThickness;
        }
        Redraw();


    }
    //Brush size textbox
    private void BrushSizeTextBox_Changed(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        int newThickness;
        if (Int32.TryParse(BrushTextBox.Text, out newThickness))
        {
            if (newThickness < 1) newThickness = 1;
            if (newThickness > 100) newThickness = 100;
            BrushSlider.Value = newThickness;

        }

    }
    private void ChangedStart()
    {
        if (isChanging == false)
        {
            currentAction = new ActionCustom();
            timer = new System.Timers.Timer();
            timeCounting = 0;
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler(TimerTick);
            timer.Enabled = true;
            isChanging = true;

            foreach (var shape in chosenList)
            {
                int i = shapeList.IndexOf((ITool)shape);
                currentAction.beforeShape.Add((ITool)shape.Copy());
                currentAction.pos.Add(i);
                currentAction.ids.Add(shape.ID);
            }
        }

    }
    private void TimerTick(object source, ElapsedEventArgs e)
    {
        timeCounting++;
        if (timeCounting == 3 && isChanging == true)
        {
            isChanging = false;
            foreach (var shape in chosenList)
            {
                int i = shapeList.IndexOf((ITool)shape);

                currentAction.afterShape.Add((ITool)shape.Copy());
                currentAction.posA.Add(i);
            }
            if (currentAction!=null && (currentAction.afterShape.Count > 0 || currentAction.beforeShape.Count > 0))
            {
                shapeUndoStack.Push(currentAction);
                shapeRedoStack.Clear();
            }
            timer.Enabled = true;
        }
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

        if (isDrawing == false)
            DisableDrawing();
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
            DisableDrawing();
        }
        else if ((bool)EraserB.IsChecked)
        {
            isDrawing = false;
            isSelecting = false;
            isErasing = true;
            isTransforming = false;
            DisableDrawing();
        }
        else if ((bool)PenB.IsChecked)
        {
            isErasing = false;
            isSelecting = false;
            isTransforming = false;
            EnableDrawing();
            

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

    List<ShapeCustom> copyShapeList = new List<ShapeCustom>();
    private void Copy_Clicked(object sender, RoutedEventArgs e)
    {
        copyShapeList.Clear();
        foreach (var shape in chosenList)
        {
            copyShapeList.Add(shape);
        }
    }

    private void Paste_Clicked(object sender, RoutedEventArgs e)
    {
        ITool newShape;
        currentAction = new ActionCustom();
        chosenList.Clear();
        foreach(var shape in copyShapeList)
        {
            newShape = shapeCollection.Create(((ITool)shape).Name);
            ((ShapeCustom)newShape).CopyFrom(shape);
            Moving((ShapeCustom)newShape, new Point(0, 0), new Point(30, 30));
            shapeList.Add(newShape);
            chosenList.Add((ShapeCustom)newShape);

            int j = shapeList.IndexOf(newShape);
            currentAction.afterShape.Add(newShape);
            currentAction.posA.Add(j);
            currentAction.ids.Add(shape.ID);
        }

        if (currentAction.afterShape.Count > 0)
        {
            shapeRedoStack.Clear();
            shapeUndoStack.Push(currentAction);
        }
            
        Redraw();
    }
    private void Undo_Clicked(object sender, RoutedEventArgs e)
    {
        if (shapeUndoStack.Count == 0) return;
        TransFB.IsChecked = false;
        isTransforming = false;
        ActionCustom temp = shapeUndoStack.Pop();

        while (temp == null && shapeUndoStack.Count > 0)
            temp = shapeUndoStack.Pop();

        if (temp == null) return;
        shapeRedoStack.Push(temp);

        temp.ctrlZ(shapeList);
        
        Redraw();
    }

    private void Redo_Clicked(object sender, RoutedEventArgs e)
    {

        if (shapeRedoStack.Count == 0) return;
        TransFB.IsChecked = false;
        isTransforming = false;
        ActionCustom temp = shapeRedoStack.Pop();

        while (temp == null && shapeRedoStack.Count > 0)
            temp = shapeRedoStack.Pop();
        shapeUndoStack.Push(temp);

        
        temp.ctrlY(shapeList);
        
        Redraw();
    }

    Tuple<AnchorPoint, int, AnchorPoint> chosenAPoint;
    ActionCustom currentAction;
    Point startingPos;
    bool isMoving = false;
    private void canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(canvas);
        shapeRedoStack.Clear();
        currentAction = new ActionCustom();
        
        if (!isSelecting && !isErasing && !isTransforming)
        {
            isDrawing = true;
            // new ready to draw shape
            drawingShape = shapeCollection.Create(selectedShapeName);
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
                    int j = shapeList.IndexOf((ITool)shape);
                    currentAction.beforeShape.Add((ITool)shape.Copy());
                    currentAction.pos.Add(j);
                    currentAction.ids.Add(shape.ID);
                }
            }
            else if (chosenList.Count > 0)
            {
                foreach (var shape in chosenList)
                {

                    int i = shapeList.IndexOf((ITool)shape);
                    currentAction.beforeShape.Add((ITool)shape.Copy());
                    currentAction.pos.Add(i);
                    currentAction.ids.Add(shape.ID);
                }

                isMoving = true;
            }

            startingPos = pos;
        }   
        else if (isErasing)
        {
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    int i = shapeList.IndexOf(shape);
                    currentAction.beforeShape.Add(shape);
                    currentAction.pos.Add(i);
                    currentAction.ids.Add(element.ID);
                    break;
                }
            }
            
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
                    }    
                }
                if (isMoving)
                    startingPos = pos;
            }
               
        }    
        else if (isDrawing)
        {
            chosenList.Clear();
            

            drawingShape.EndCorner(pos.X, pos.Y);

            Redraw();

            canvas.Children.Add(drawingShape.Draw(currentColor, currentFill, currentThickness));
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

            shapeUndoStack.Push(currentAction);
            Redraw();
        }
        
        else if (isTransforming)
        {
            chosenAPoint = null;
            isMoving = false;
            foreach (var shape in chosenList)
            {
                int i = shapeList.IndexOf((ITool)(shape));

                currentAction.afterShape.Add((ITool)shape.Copy());
                currentAction.posA.Add(i);
            }
            shapeUndoStack.Push(currentAction);
        }    
        else
        {
            Point pos = e.GetPosition(canvas);
            drawingShape.EndCorner(pos.X, pos.Y);

            // add to list
            drawingShape.Brush = currentColor;
            drawingShape.Thickness = currentThickness;
            drawingShape.FillBrush = currentFill;
            shapeList.Add(drawingShape);

            currentAction.ids.Add(((ShapeCustom)drawingShape).ID);
            int i = shapeList.IndexOf(drawingShape);
            currentAction.afterShape.Add(drawingShape);
            currentAction.posA.Add(i);

            shapeUndoStack.Push(currentAction);
            

            Redraw();
            isDrawing = false;
        } 
        currentAction = null;
    }

    private void Redraw()
    {
        canvas.Children.Clear();

        foreach (var shape in shapeList)
        {
            var element = shape.Draw(shape.Brush, shape.FillBrush, shape.Thickness);
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