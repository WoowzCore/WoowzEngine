using WLO.Math;

namespace WEE;

public static class Window{
    public static void Start(){
        MainWindow = WE.Window.CreateWindow();
    }
    
    public static void Stop(){
        if(MainWindow != null!){ MainWindow.Close(); }
    }

    public static void ConnectEvents(){
        MainWindow.Mouse.OnMove += (Position, Delta) => WEE.Interface.ImGUI.MousePosition(Position);
        MainWindow.Mouse.OnScroll += Delta => WEE.Interface.ImGUI.MouseScroll(Delta);
        MainWindow.Mouse.OnButton += (Button, Down) => WEE.Interface.ImGUI.MouseButton(Button, Down);

        MainWindow.Keyboard.OnKey += (Key, Down) => WEE.Interface.ImGUI.KeyboardKey(Key, Down);
        MainWindow.Keyboard.OnChar += Char => WEE.Interface.ImGUI.KeyboardChar(Char);
    }
    
    // ----------------------------------------------------------------------
    
    public static WLO.Window.GLFW MainWindow;
}