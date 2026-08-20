using ImGuiNET;
using WLO.Interface;

namespace WEE;

public static class Interface{
    public static void Start(){
        ImGUI = new GLImGUI(WE.Render.API, true);

        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------

    public static GLImGUI ImGUI{ get; private set; } = null!;

    public static bool FocusSceneView{ get; private set; }

    private static void Update_SceneView(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin("Scene View (800x600)");

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            
            System.Numerics.Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
                
            ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
        
        ImGui.End();
    }

    private static void Update_Inspector(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Inspector");
         ImGui.Text("hi, welcome here!");
        ImGui.End();
    }
    
    private static void Update_Hierarchy(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Hierarchy");
            ImGui.Text("Hierarchy");
        ImGui.End();
    }
    
    private static void Update_Assets(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Assets");
            ImGui.Text("FILES");
        ImGui.End();
    }
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);

        ImGui.DockSpaceOverViewport();
        
        Update_SceneView();
        
        Update_Inspector();

        Update_Hierarchy();
        
        Update_Assets();
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}