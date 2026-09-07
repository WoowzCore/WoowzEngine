using WEE_Interface;
using WLI_Input;

namespace WEE;

public static class Window{
    public static void Start(){
        WEE.D.Window = WE.Window.CreateWindow();
        WEE.D.Window.TODO_UseDarkMode();
        UpdateTitle();
    }
    
    public static void Stop(){
        if(WEE.D.Window != null!){ WEE.D.Window.Close(); }
    }

    public static void ConnectEvents(){
        WEE.D.Window.Mouse.OnMove   += (Position, Delta) => WEE.D.ImGUI.MousePosition(Position);
        WEE.D.Window.Mouse.OnScroll += Delta => WEE.D.ImGUI.MouseScroll(Delta);
        WEE.D.Window.Mouse.OnButton += (Button, Down) => {
            WEE.D.ImGUI.MouseButton(Button, Down);
            
            if(WEE.D.View.IsMouseOver && Button == Mouse.Button.Left && Down){
                I_View.ClickToView();
            }
        };

        WEE.D.Window.Keyboard.OnKey  += (Key, Down) => WEE.D.ImGUI.KeyboardKey(Key, Down);
        WEE.D.Window.Keyboard.OnChar += Char => WEE.D.ImGUI.KeyboardChar(Char);
    }

    public static void UpdateTitle(){
        string Title = "WoowzEngineEditor";

        if(!WEE.Interface.__IsProjectLoaded){
            Title += " - Добро пожаловать!";
        }else{
            Title += $" | {WEE.Interface.Config!.Name}";
            
            if(WEE.D.Selected.Scene == null){
                Title += " - Не выбрана сцена";
            }else{
                Title += $" - {(I_Menu.__SceneFilePath == null! ? "Не указано куда сохранять." : I_Menu.__SceneFilePath)}";
            }
        }
        
        WEE.D.Window.Title = Title;
    }
}