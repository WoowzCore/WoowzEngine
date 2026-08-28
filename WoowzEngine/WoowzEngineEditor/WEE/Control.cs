using WEE_Interface;
using WEO;
using WLI_Input;
using WLO.Math;

namespace WEE;

public static class Control{
    public static Vector2I MousePosition;

    public static void Start(){
        WEE.Window.MainWindow.Mouse.OnButton += (Button, Down) => {
            if(I_View.FocusSceneView && Button == Mouse.Button.Left && Down){
                I_View.ClickToView();
            }
        };
    }
    
    public static void Update(){
        MousePosition = WEE.Window.MainWindow.Mouse.Position;
    }
}