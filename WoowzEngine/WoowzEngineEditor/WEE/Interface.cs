using WLO.Interface;

namespace WEE;

public static class Interface{
    public static void Start(){
        ImGUI = new GLImGUI(WE.Render.API, true);
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------

    public static GLImGUI ImGUI;

    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.DT, WEE.Window.MainWindow.Size);
        
            ImGuiNET.ImGui.ShowDemoWindow();
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}