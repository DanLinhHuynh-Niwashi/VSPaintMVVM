# VSPaint
VSPaint is a simple vector drawing application available on Window and Linux!  

**Creators:** VSPain Group, University of Information Technology, Vietnam National University of Ho Chi Minh City
- Huỳnh Lê Đan Linh (Student ID: 22520759)
- Huỳnh Trần Khánh Quỳnh (Student ID: 22521230)    

**Language:** C#  
**Framework:** Net6.0  
**UI Framework:** [AvaloniaUI](https://avaloniaui.net/)

Visit download website [here](https://q190504.github.io/VSPaint-Website/).

## Basic Usage
See detail functions and shortcuts from our [reference page](https://q190504.github.io/VSPaint-Website/reference.html).

### Drawing and Erasing
- Supporting shapes: **Line, Rectangle, Circle, Triangle, Pentagon, Star**. Switching via the Tool Box
- User can change the Fill color and Stroke color of the shape via a color slider
- User can change the Brush size via a brush size slider
- User can choose and erase any drawed shape by hovering and clicking on the shape with **Erase tool (E)**

### Selecting and Modifying shapes
- User can select a single shape via **Selecting tool (S)** hold **Shift** for selecting multiple shapes.
- User can only select the shape on top most layer if the tool is hovering on multiple intersecting shapes.
- Modifying: **Resizing** and **Rotating** (available when there's only *one* shape chosed), **Moving**.

### Copy and Paste
- Selected shapes can be copied and paste into the current canvas via buttons, or by pressing **Ctrl + C (Copy)**, **Ctrl + V (Paste)**.

### Layering
- **Shapes are placed in different layers.**
- Layer structure is displayed on the left panel of the working screen.
- Shapes on higher layers can overlay shapes on lower layers.
- User can change a shape's layer position via layer switching buttons.

### Undo and Redo
- Every of your actions are manage by Undo stack and Redo stack.
- Undo and Redo's datas will remain until the current canvas is replaced by a new one/ is closed.
- User can **Undo** their action via button on UI, or press **Ctrl + Z**.
- Unser can **Redo** their action via button on UI, or press **Ctrl + Y**.
- If any action is performed, the Redo stack will be cleared.

### New, Save, Open project
- User can create a new canvas via **File > New**, or by pressing **Ctrl + N**.
- The current project's state (with layered shapes and canvas's size) can be saved as a **(*.json)** file via **File > Save**, or by pressing **Ctrl + S**.
- The **(*.json)** file can be opened anytime via **File > Open**, or by pressing **Ctrl + O**.
- Use **File > Save As...**, or press **Ctrl + Shift + S** if you want to save the project with a different name.

### Import and Export
- User can Import an *image* (*.jpeg, *.png) into the canvas as a shape *(modifiable)* via **File > Import**, or by pressing **Ctrl + I**.
- User can Export the *current drawing canvas* as an *image* (*.jpeg, *.png) via **File > Export**, or by pressing **Ctrl + E**.

## Installing on Linux
There're some few steps to be taken before you can launch VSPaint on Linux!  
- After extracting the .zip file, you will get a VSPaint folder
- Run `chmod +x VSPaintMVVM` to make the file executable
- You can now double-click on the VSPaintMVVM to lauch the application

## License

VSPaint is licenced under the [MIT licence](LICENSE.md).

## NuGet Packages
- [MessageBox.Avalonia](https://github.com/AvaloniaCommunity/MessageBox.Avalonia)  
- [NetSparkleUpdater](https://github.com/NetSparkleUpdater/NetSparkle)  
- [Avalonia.Controls.PanAndZoom](https://github.com/wieslawsoltes/PanAndZoom)  
