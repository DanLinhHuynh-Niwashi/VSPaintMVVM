using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;
using VSPaintMVVM.Tool;
using VSPaintMVVM.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using Avalonia.Interactivity;
using System.Linq;
using System.Timers;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Newtonsoft.Json;
using System.Reflection;
using Avalonia.Media.Immutable;
using System.IO;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using VSPaintMVVM.ViewModels;
namespace VSPaintMVVM.Views;

public partial class MainView : UserControl
{

    private bool isDrawing = false;
    private bool isSelecting = false;
    private bool isErasing = false;
    private bool isTransforming = false;
    public bool isFileSaved = true;

    string filePath = "";

    private List<ITool> shapeList = new List<ITool>();
    private List<ShapeCustom> chosenList = new List<ShapeCustom>();

    private Stack<ActionCustom> shapeUndoStack = new Stack<ActionCustom>();
    private Stack<ActionCustom> shapeRedoStack = new Stack<ActionCustom>();

    private int currentThickness;
    private ITool? drawingShape = null;
    private string selectedShapeName;

    private Color currentColor = new Color();
    private Color currentFill = new Color();

    ShapeCollection shapeCollection = new ShapeCollection();
    List<Button> toolCollection = new List<Button>();

    public MainView()
    {
        InitializeComponent();
        CreateTools();

        canvas.Width = 0; canvas.Height = 0;
        canvasContainer.Width = canvas.Width+2000; canvasContainer.Height = canvas.Height + 2000;

        strokeCB.IsChecked = true;
        ASlider.Value = 255;
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

        PreviewRedraw();
    }

    
    KeyGesture Undogesture = new KeyGesture(Avalonia.Input.Key.Z, Avalonia.Input.KeyModifiers.Control);
    KeyGesture Redogesture = new KeyGesture(Avalonia.Input.Key.Y, Avalonia.Input.KeyModifiers.Control);
    KeyGesture Transformgesture = new KeyGesture(Avalonia.Input.Key.T, Avalonia.Input.KeyModifiers.Control);

    KeyGesture UndogestureMeta = new KeyGesture(Avalonia.Input.Key.Z, Avalonia.Input.KeyModifiers.Meta);
    KeyGesture RedogestureMeta = new KeyGesture(Avalonia.Input.Key.Y, Avalonia.Input.KeyModifiers.Meta);
    KeyGesture TransformgestureMeta = new KeyGesture(Avalonia.Input.Key.T, Avalonia.Input.KeyModifiers.Meta);

    bool isShiftPressing = false;

    protected override async void OnPointerEntered(PointerEventArgs e)
    {

        InitializeCanvas();

        if (filePath == "") crnPath.Content = "Untitled";
        else crnPath.Content = filePath;

        if (isFileSaved) saveState.Content = "Saved";
        else saveState.Content = "Unsaved";
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (filePath == "") crnPath.Content = "Untitled";
        else crnPath.Content = filePath;

        if (isFileSaved) saveState.Content = "Saved";
        else saveState.Content = "Unsaved";

        base.OnPointerMoved(e);
    }

    
    protected async Task InitializeCanvas()
    {
        if (canvas.Width < 200 || canvas.Height < 200)
        {
            await New(true);
        }
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        KeyGesture gesture = new KeyGesture(e.Key, e.KeyModifiers);
        if (CopyM.InputGesture == gesture)
        {
            Copy_Clicked(new object(), new RoutedEventArgs());
        }
        else if (PasteM.InputGesture == gesture)
        {
            Paste_Clicked(new object(), new RoutedEventArgs());
        }
        else if (Transformgesture == gesture || TransformgestureMeta == gesture)
        {
            TransFB.IsChecked = true;
            TransF_Move_Checked(new object(), new RoutedEventArgs());
        }
        else if (SelAllM.InputGesture == gesture)
        {
            SelectAll_Clicked(new object(), new RoutedEventArgs());
        }
        else if (DeSelM.InputGesture == gesture)
        {
            DeselectAll_Clicked(new object(), new RoutedEventArgs());
        }
        else if (OpenM.InputGesture == gesture)
        {
            Open_Click(new object(), new RoutedEventArgs());
        }
        else if (SaveM.InputGesture == gesture)
        {
            Save_Click(new object(), new RoutedEventArgs());
        }
        else if (SaveAsM.InputGesture == gesture)
        {
            SaveAs_Click(new object(), new RoutedEventArgs());
        }
        else if (NewM.InputGesture == gesture)
        {
            New_Click(new object(), new RoutedEventArgs());
        }
        else if (Undogesture == gesture || UndogestureMeta == gesture)
        {
            Undo_Clicked(new object(), new RoutedEventArgs());
        }
        else if (Redogesture == gesture || RedogestureMeta == gesture)
        {
            Redo_Clicked(new object(), new RoutedEventArgs());
        }
        else
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.B:
                    PenB.IsChecked = true;
                    Tool_Checked(new object(), new RoutedEventArgs());
                    break;
                case Avalonia.Input.Key.E:
                    EraserB.IsChecked = true;
                    Tool_Checked(new object(), new RoutedEventArgs());
                    break;
                case Avalonia.Input.Key.S:
                    SelB.IsChecked = true;
                    Tool_Checked(new object(), new RoutedEventArgs());
                    break;
                case Avalonia.Input.Key.Delete:
                    DeleteAll();
                    break;
                case Avalonia.Input.Key.LeftShift:
                case Avalonia.Input.Key.RightShift:
                    isShiftPressing = true;
                    break;
            }
        }    

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Avalonia.Input.Key.LeftShift:
            case Avalonia.Input.Key.RightShift:
                isShiftPressing = false;
                break;
        }
    }
    private void DeleteAll()
    {
        currentAction = new ActionCustom();
        foreach (var shape in chosenList)
        {
            int i = shapeList.IndexOf((ITool)shape);
            currentAction.beforeShape.Add((ITool)shape);
            currentAction.pos.Add(i);
            currentAction.ids.Add(shape.ID);

            shapeList.Remove((ITool)shape);
        }
        chosenList.Clear();
        Redraw();
        shapeUndoStack.Push(currentAction);
        isFileSaved = false;
        
    }
    private void CreateTools()
    {
        foreach (var shape in shapeCollection.prototypes)
        {
            if (shape.Value.Name == new ImageImportCustom().Name) continue;

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
    private void itemChoose(object? sender, RoutedEventArgs e)
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
        if (selectedShapeName!="" && selectedShapeName!=null)
        {
            PreviewRedraw();
        }    
        
    }

    //Color slider
    private void CB_Clicked(object? sender, RoutedEventArgs e)
    {
        if ((bool)fillCB.IsChecked && fillColor.Fill != null)
        {
            ImmutableSolidColorBrush fill = (ImmutableSolidColorBrush)fillColor.Fill;
            BText.Text = fill.Color.B.ToString();
            RText.Text =fill.Color.R.ToString();
            GText.Text = fill.Color.G.ToString();
            AText.Text = fill.Color.A.ToString();
        }
        else if ((bool)strokeCB.IsChecked && strokeColor.Fill != null)
        {
            ImmutableSolidColorBrush stroke = (ImmutableSolidColorBrush)strokeColor.Fill;
            BText.Text = stroke.Color.B.ToString();
            RText.Text = stroke.Color.R.ToString();
            GText.Text = stroke.Color.G.ToString();
            AText.Text = stroke.Color.A.ToString();
        }
    }
    private void Transparent_Click(object? sender, RoutedEventArgs e)
    {
        if ((bool)fillCB.IsChecked)
        {
            AText.Text = "0";
        }
        else
        {
            AText.Text = "0";
        }
    }

    private void OpacitySlider_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        AText.Text = ((int)ASlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB);
        }
        else
        {
            ColorChange(strokeCB);
        }
    }
    private void BlueSlider_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        BText.Text = ((int)BSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB);
        }
        else
        {
            ColorChange(strokeCB);
        }    
    }
    private void RedSlider_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        RText.Text = ((int)RSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB);
        }
        else
        {
            ColorChange(strokeCB);
        }
    }
    private void GreenSlider_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        GText.Text = ((int)GSlider.Value).ToString();
        if ((bool)fillCB.IsChecked)
        {
            ColorChange(fillCB);
        }
        else
        {
            ColorChange(strokeCB);
        }
    }

    //Color textbox
    private void OpacityTextBox_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        double opacityValue;
        if (double.TryParse(AText.Text, out opacityValue))
        {
            if (opacityValue < 0) opacityValue = 0;
            if (opacityValue > 255) opacityValue = 255;
            ASlider.Value = opacityValue;
        }
    }

    private void BlueTextBox_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        double blueValue;
        if (double.TryParse(BText.Text, out blueValue))
        {
            if (blueValue < 0) blueValue = 0;
            if (blueValue > 255) blueValue = 255;
            BSlider.Value = blueValue;
        }
    }
    private void RedTextBox_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        double redValue;
        if (double.TryParse(RText.Text, out redValue))
        {
            if (redValue < 0) redValue = 0;
            if (redValue > 255) redValue = 255;
            RSlider.Value = redValue;
        }
    }
    private void GreenTextBox_Change(object? sender, AvaloniaPropertyChangedEventArgs e)
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

    private void ColorChange(object sender)
    {

        byte r = (byte)RSlider.Value;
        byte g = (byte)GSlider.Value;
        byte b = (byte)BSlider.Value;
        byte a = (byte)ASlider.Value;
        Color tempC = new Color(a, r, g, b);
        Color prevColor = new Color();

        if (sender == null)
            return;
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
        
        if (prevColor == tempC) return;

        ChangedStart();

        if (sender == fillCB)
        {
            currentFill = tempC;
            fillColor.Fill = new SolidColorBrush(tempC).ToImmutable();
        }
        else
        {
            currentColor = tempC;
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
    private void BrushSizeSlider_Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
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
    private void BrushSizeTextBox_Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
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
            isChanging = true;
            timer = new System.Timers.Timer();
            timeCounting = 0;
            timer.Interval = 1000;
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler(TimerTick);
            timer.Enabled = true;
            

            foreach (var shape in chosenList)
            {
                int i = shapeList.IndexOf((ITool)shape);
                currentAction.beforeShape.Add((ITool)shape.Copy());
                currentAction.pos.Add(i);
                currentAction.ids.Add(shape.ID);
            }
        }
        else
        {
            timeCounting = 0;
        }
    }
    private void TimerTick(object? source, ElapsedEventArgs e)
    {
        bool haveOtherThanImage = false;
        timeCounting++;
        if (timeCounting == 3 && isChanging == true)
        {
            isChanging = false;
            foreach (var shape in chosenList)
            {
                int i = shapeList.IndexOf((ITool)shape);
                if (currentAction!=null)
                {
                    currentAction.afterShape.Add((ITool)shape.Copy());
                    currentAction.posA.Add(i);
                }    
                
                if(((ITool)shape).Name != new ImageImportCustom().Name)
                {
                    haveOtherThanImage = true;
                }    
                    
            }
            if (currentAction!=null && (currentAction.afterShape.Count > 0 || currentAction.beforeShape.Count > 0) && haveOtherThanImage)
            {
                shapeUndoStack.Push(currentAction);
                isFileSaved = false;
                shapeRedoStack.Clear();
            }
            timer.Enabled = true;
        }
    } 
    

    bool preIsDrawing, preIsSelecting, preIsErasing;
    private void TransF_Move_Checked(object? sender, RoutedEventArgs e)
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
        else EnableDrawing();
        Redraw();
    }


    //selection + deletion tool
    private void Tool_Checked(object? sender, RoutedEventArgs e)
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

    private void SelectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        foreach (var shape in Enumerable.Reverse(shapeList))
        {
            ShapeCustom element = shape as ShapeCustom;
            if (!chosenList.Contains(element))
                chosenList.Add(element);
        }

        isTransforming = false;
        TransFB.IsChecked = false;
        TransF_Move_Checked(new object(), new RoutedEventArgs());
        Redraw() ;
    }

    private void DeselectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        foreach (var shape in Enumerable.Reverse(chosenList))
        {
                chosenList.Remove(shape);
        }
        TransFB.IsChecked = false;
        isTransforming = false;
        TransF_Move_Checked(new object(), new RoutedEventArgs());
        Redraw();
    }

    List<ShapeCustom> copyShapeList = new List<ShapeCustom>();
    private void Copy_Clicked(object? sender, RoutedEventArgs e)
    {
        copyShapeList.Clear();
        foreach (var shape in chosenList)
        {
            copyShapeList.Add(shape);
        }
    }

    private void Paste_Clicked(object? sender, RoutedEventArgs e)
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
            isFileSaved = false;
        }
            
        Redraw();
    }
    private void Undo_Clicked(object? sender, RoutedEventArgs e)
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

        isFileSaved = false;
        Redraw();
    }

    private void Redo_Clicked(object? sender, RoutedEventArgs e)
    {

        if (shapeRedoStack.Count == 0) return;
        TransFB.IsChecked = false;
        isTransforming = false;
        ActionCustom temp = shapeRedoStack.Pop();

        while (temp == null && shapeRedoStack.Count > 0)
            temp = shapeRedoStack.Pop();
        shapeUndoStack.Push(temp);

        
        temp.ctrlY(shapeList);

        isFileSaved = false;
        Redraw();
    }

    //Push up & down layer
    private void DownLevel_Clicked(object? sender, RoutedEventArgs e)
    {
        if (chosenList.Count == 0) return;
        if (chosenList.Count > 1) return;
        currentAction = new ActionCustom();
        
        for (int i = 0; i < shapeList.Count; i++)
        {
            if (shapeList[i] == chosenList[0] && i != 0)
            {
                shapeRedoStack.Clear();
                currentAction.beforeShape.Add((ITool)((ShapeCustom)shapeList[i]).Copy());
                currentAction.afterShape.Add((ITool)((ShapeCustom)shapeList[i]).Copy());
                currentAction.pos.Add(i);
                currentAction.ids.Add(((ShapeCustom)shapeList[i]).ID);

                Swap(shapeList, i, i - 1);
                
                currentAction.posA.Add(i - 1);

                shapeUndoStack.Push(currentAction);
                isFileSaved = false;
                Redraw();
                return;
            }
        }
    }

    private void UpLevel_Clicked(object? sender, RoutedEventArgs e)
    {
        if (chosenList.Count == 0) return;
        if (chosenList.Count > 1) return;
        currentAction = new ActionCustom();
        for (int i = 0; i < shapeList.Count; i++)
        {
            if (shapeList[i] == chosenList[0] && i != shapeList.Count - 1)
            {
                shapeRedoStack.Clear();
                currentAction.beforeShape.Add((ITool)((ShapeCustom)shapeList[i]).Copy());
                currentAction.afterShape.Add((ITool)((ShapeCustom)shapeList[i]).Copy());
                currentAction.pos.Add(i);
                currentAction.ids.Add(((ShapeCustom)shapeList[i]).ID);

                Swap(shapeList, i + 1, i);
                currentAction.posA.Add(i + 1);

                shapeUndoStack.Push(currentAction);
                isFileSaved = false;
                Redraw() ;
                return;
            }
        }
    }
    private void Swap(List<ITool> list, int index1, int index2)
    {
        ITool temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }



    Tuple<AnchorPoint, int, AnchorPoint> chosenAPoint;
    ActionCustom currentAction;
    Point startingPos;
    bool isMoving = false;
    private void canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
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

    }

    private void Moving(ShapeCustom shape, Point oriPos, Point afterPos)
    {
        double offsetX = oriPos.X - afterPos.X;
        double offsetY = oriPos.Y - afterPos.Y;

        shape.BoxEnd.x -= offsetX;
        shape.BoxStart.x -= offsetX;
        shape.BoxEnd.y -= offsetY;
        shape.BoxStart.y -= offsetY;


    }

    private void Rotating(AnchorPoint chosenAPoint, ShapeCustom shape, Point pos, AnchorPoint originalPoint)
    {
        double rotatedAngle = Calculate_Angle(shape.BoxCenter(), pos, originalPoint);
        shape.Angle = rotatedAngle;
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
        pointerPos.Content = "Pointer: " + (int)pos.X + ", " + (int)pos.Y;

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
            Redraw();

        }    
        else if (isDrawing)
        {
            chosenList.Clear();
            

            drawingShape.EndCorner(pos.X, pos.Y);

            Redraw();

            canvas.Children.Add(drawingShape.Draw(currentColor, currentFill, currentThickness));
        }
    }

    private void canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (isSelecting)
        {
            Point pos = e.GetPosition(canvas);
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    if (chosenList.Contains(element))
                        chosenList.Remove(element);
                    else if (isShiftPressing)
                        chosenList.Add(element);
                    else
                    {
                        chosenList.Clear();
                        chosenList.Add(element);
                    }
                    break;
                }
            }
            Redraw();
        }

        else if (currentAction == null)
            return;
        else if (isErasing)
        {
            Point pos = e.GetPosition(canvas);
            foreach (var shape in Enumerable.Reverse(shapeList))
            {
                ShapeCustom element = shape as ShapeCustom;
                if (element.isHovering(pos))
                {
                    shapeList.Remove(shape);
                    if (chosenList.Contains(element))
                        chosenList.Remove(element);
                    break;
                }
            }

            shapeUndoStack.Push(currentAction);
            isFileSaved = false;
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
            isFileSaved = false;
        }
        else
        {
            Point pos = e.GetPosition(canvas);

            if (drawingShape == null) { return; }
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
            isFileSaved = false;

            Redraw();
            isDrawing = false;
        } 
        currentAction = null;
    }

    private void PreviewRedraw()
    {
        previewPanel.Children.Clear();
        if (selectedShapeName!=null)
        {
            ITool previewShape = shapeCollection.getShape(selectedShapeName);

            if (previewShape.Name == "Line")
            {
                previewShape.StartCorner(10, 50);
                previewShape.EndCorner(180, 50);
            }    
            else
            {
                previewShape.StartCorner(10, 10);
                previewShape.EndCorner(180, 90);
            }    
            

            previewPanel.Children.Add(previewShape.Draw(currentColor, currentFill, currentThickness));
        }
    }

    private void ShapeListBoxRedraw()
    {
        shapeListBox.Items.Clear();
        foreach (var shape in Enumerable.Reverse(shapeList))
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = ((ShapeCustom)shape).ID;
            if (chosenList.Contains(((ShapeCustom)shape)))
            {
                item.IsSelected = true;
            }
            shapeListBox.Items.Add(item);
        }
    }

    private void CanvasRedraw()
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


    public async void New_Click(object? sender, RoutedEventArgs e)
    {
        await New();
    }

    bool taskEnded = true;
    private async Task New(bool isInitialize = false)
    {
        NewCanvasDialog dialog = new NewCanvasDialog();
        if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow == null) return;

            if (taskEnded)
            {
                await dialog.ShowDialog(desktop.MainWindow);
                taskEnded = false;
            }
            else return;

            if (dialog.choosenState == 0)
            {   
                if (isInitialize)
                {
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Alert", "There is no canvas in the current workspace, please create a canvas or the application will be closed.",
                    ButtonEnum.OkCancel);

                    var result = await box.ShowWindowDialogAsync(desktop.MainWindow);
                    taskEnded = true;
                    if (result == ButtonResult.Cancel)
                        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();

                }    
            }
            else if (dialog.choosenState == 2)
            {
                if (!isFileSaved)
                {
                    if (taskEnded == false) { return; }
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Save current file", "Unsaved change detected. Do you want to save the current progress?",
                    ButtonEnum.YesNo);

                    var result = await box.ShowAsync();
                    taskEnded = true;
                    if (result == ButtonResult.Yes)
                        await Save();
                }
                shapeCollection.reset();
                shapeList.Clear();
                chosenList.Clear();
                shapeRedoStack.Clear();
                shapeUndoStack.Clear();

                Redraw();
                canvas.Width = dialog.w; canvas.Height = dialog.h;
                canvasContainer.Width = canvas.Width + 2000;
                canvasContainer.Height = canvas.Height + 2000;

                isFileSaved = true;
                filePath = "";
            }

            else if (dialog.choosenState == 1)
            {
                await Open();
                taskEnded = true;
            }
        }
    }
    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        await Save();
    }

    private async void SaveAs_Click(object? sender, RoutedEventArgs e)
    {
        await Save(true);
    }
    public async Task Save (bool isSaveAs = false)
    {
        if (filePath == "" || isSaveAs)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save File",
                FileTypeChoices = new[] { new FilePickerFileType("JSON") { 
                    Patterns = new[] { "*.json"}, AppleUniformTypeIdentifiers = new[]{"public.json"} } }

            });

            if (file is not null)
            {
                // Open writing stream from the file.
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);
                if (!isSaveAs) filePath = file.TryGetLocalPath() ?? string.Empty;
                SaveasJson(streamWriter);
            }
        }    
        else
        {
            StreamWriter writing = new StreamWriter(filePath);
            SaveasJson(writing);
        }    
        
    }

    private async void Open_Click(object? sender, RoutedEventArgs args)
    {
        await Open();
    }

    private async Task Open()
    {
        if (!isFileSaved)
        {
            var box = MessageBoxManager
            .GetMessageBoxStandard("Save current file", "Unsaved change detected. Do you want to save the current progress?",
                ButtonEnum.YesNo);

            var result = await box.ShowAsync();

            if (result == ButtonResult.Yes)
                await Save();

        }


        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("JSON") {
                    Patterns = new[] { "*.json"}, AppleUniformTypeIdentifiers = new[]{"public.json"} } },

        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            filePath = files[0].TryGetLocalPath() ?? string.Empty;
            OpenFile(streamReader);
        }
        else
        { MainWindowViewModel.isOpeningFile = false;
            InitializeCanvas();
           
        }    
    }
        private async void OpenFile(StreamReader FileStream)
    {

        string jsonString = FileStream.ReadToEnd();

        var options = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,

        };

        if (jsonString != null && jsonString!="")
        {
            Dictionary<string, int> pre = shapeCollection.reset();
            try
            {
                dynamic record = JsonConvert.DeserializeObject(jsonString, options);

                if (record.list == null || record.cv == null) 
                    throw new Exception();
                List<ITool> temp = record.list;
                double tempW = record.cv[0];
                double tempH = record.cv[1];

                shapeList = temp;
                canvas.Width = tempW;
                canvas.Height = tempH;

                shapeRedoStack.Clear();
                shapeUndoStack.Clear();
            }
            catch (Exception e)
            {
                var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Can not open this file",
                ButtonEnum.Ok);

                var result = await box.ShowAsync();

                shapeCollection.restore(pre);
                FileStream.Close();

                MainWindowViewModel.isOpeningFile = false;
                return;
            }
            
        }

        else
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Can not open this file",
                ButtonEnum.Ok);

            var result = await box.ShowAsync();
            FileStream.Close();

            MainWindowViewModel.isOpeningFile = false;
            return;
        }    

        Redraw();

        isFileSaved = true;
        FileStream.Close();
    }

    private void SaveasJson(StreamWriter FileStream)
    {
        dynamic record = new
        {
            list = shapeList,
            cv = new Double[]
            {
                canvas.Width, canvas.Height
            }
        };
        var options = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All

        };

        string jsonString = JsonConvert.SerializeObject(record, Formatting.Indented, options);
        FileStream.Write(jsonString);
        FileStream.Close();
        isFileSaved = true;
    }

    private async void Import_Click(object? sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import File",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageJpg, FilePickerFileTypes.ImagePng },

        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            await using var stream = await files[0].OpenReadAsync();
            ImportFile(stream);
        }
    }

    private async void Export_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export File",
            FileTypeChoices = new[] { FilePickerFileTypes.ImageJpg, FilePickerFileTypes.ImagePng }

        });

        if (file is not null)
        {
            // Open writing stream from the file.
            await using var stream = await file.OpenWriteAsync();
            ExportToPNG(stream);
        }
    }
    private void ExportToPNG(Stream FileStream)
    {
        Canvas mainView = new Canvas();
        mainView.Width = canvas.Width;
        mainView.Height = canvas.Height;
        mainView.Background = canvas.Background;

        foreach (var shape in shapeList) 
        {
            mainView.Children.Add(shape.Draw(shape.Brush, shape.FillBrush, shape.Thickness));
        }

        mainView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        mainView.Arrange(new Rect(mainView.DesiredSize));
        mainView.UpdateLayout();

        int w = (int)mainView.DesiredSize.Width;
        int h = (int)mainView.DesiredSize.Height;
        var rendersize = new PixelSize(w, h);
        RenderTargetBitmap bitmap = new RenderTargetBitmap(rendersize);
        bitmap.Render(mainView);
        bitmap.Save(FileStream);
    }

    private void ImportFile(Stream FileStream)
    {
        shapeRedoStack.Clear();
        currentAction = new ActionCustom();
        ImageImportCustom image = new ImageImportCustom();

        ITool imageFile = image.Clone();

        Bitmap btm = new Bitmap(FileStream);
        imageFile.StartCorner(0, 0);

        double w = btm.Size.Width;
        double h = btm.Size.Height;

        if (w > canvas.Width)
        {
            w = canvas.Width;
            h = h * (w / btm.Size.Width);
        }
        else if (h > canvas.Height)
        {
            h = canvas.Height;
            w = w * (h / btm.Size.Height);
        }

        imageFile.EndCorner(w, h);

        ImageImportCustom crnImage = imageFile as ImageImportCustom;

        using (MemoryStream stream = new MemoryStream())
        {
            btm.Save(stream);
            crnImage.Bitmap = stream.ToArray();
        }


        shapeList.Add(crnImage);

        int j = shapeList.IndexOf(imageFile);
        currentAction.afterShape.Add(imageFile);
        currentAction.posA.Add(j);
        currentAction.ids.Add(crnImage.ID);

        shapeUndoStack.Push(currentAction);
        isFileSaved = false;
        Redraw();

    }
    private void Redraw()
    {
        
        PreviewRedraw();
        CanvasRedraw();
        ShapeListBoxRedraw();
    }
}