using System.Numerics;
using ImGuiNET;
using WLO.Interface;
using WLO.Math;

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

    public static GLImGUI ImGUI;

    public static Vector2I SceneViewport;

    private static void StartUpdate(){
        
    }
    
    private static bool __FirstTime = true;
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.DT, WEE.Window.MainWindow.Size);

        ImGui.DockSpaceOverViewport();
        
        if(__FirstTime){ StartUpdate(); __FirstTime = false; }
        
        ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin("Scene View (800x600)");

            System.Numerics.Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            SceneViewport = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);
            
            ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
        
        ImGui.End();

        ImGui.SetNextWindowSize(new Vector2(200, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin("Inspector");
            ImGui.Text("hi, welcome here!");
        ImGui.End();
        
        ImGui.SetNextWindowSize(new Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Explorer");
            ImGui.Text("FILES");
        ImGui.End();
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}