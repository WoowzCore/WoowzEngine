using WLO.Math;

namespace WE;

public static class Window{
    public static void __Start(){
        WL.GLFW.Start();
    }

    public static void __Stop(){
        WL.GLFW.Stop();
    }
    
    public static WLO.Window.GLFW CreateWindow(){
        return new WLO.Window.GLFW(new Vector2I(800, 600), "todo window name", true);
    }
}