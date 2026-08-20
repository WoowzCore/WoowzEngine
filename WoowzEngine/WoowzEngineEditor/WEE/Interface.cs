using System.Diagnostics;
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
        
        Start_Console();
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------

    public static GLImGUI ImGUI{ get; private set; } = null!;

    // ----------------------------------------------------------------------

    private static void Update_Menu(){
        if(ImGui.BeginMainMenuBar()){
            if(ImGui.BeginMenu("File")){
                if(ImGui.MenuItem("Exit", "Alt+F4")){ WEE.Window.MainWindow.Close(); }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Edit")){
                if(ImGui.MenuItem("Undo", "Ctrl+Z")){  }
                if(ImGui.MenuItem("Redo", "Ctrl+Y")){  }
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Window")){
                ImGui.MenuItem("Scene View", "", true);
                ImGui.MenuItem("Inspector", "", true);
                ImGui.MenuItem("Hierarchy", "", true);
                ImGui.MenuItem("Assets", "", true);
                ImGui.MenuItem("Console", "", true);
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Help")){
                if(ImGui.MenuItem("Open GitHub...")){ Process.Start(new ProcessStartInfo("https://github.com/WoowzCore/WoowzEngine"){ UseShellExecute = true }); }
                ImGui.EndMenu();
            }

            string MenuText = "hello world!";
            Vector2 TextSize = ImGui.CalcTextSize(MenuText);
            ImGui.SameLine(ImGui.GetWindowWidth() - TextSize.X - 10);
            ImGui.TextDisabled(MenuText);
            
            ImGui.EndMainMenuBar();
        }
    }
    
    // ----------------------------------------------------------------------
    
    public static bool FocusSceneView{ get; private set; }

    private static void Update_SceneView(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin("Scene View (800x600)");

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            
            System.Numerics.Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
                
            ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
        
        ImGui.End();
    }

    // ----------------------------------------------------------------------
    
    private static void Update_Inspector(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Inspector");
         ImGui.Text("hi, welcome here!");
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------
    
    private static void Update_Hierarchy(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Hierarchy");
            ImGui.Text("Hierarchy");
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------
    
    private static void Update_Assets(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Assets");
            ImGui.Text("FILES");
        ImGui.End();
    }

    // ----------------------------------------------------------------------

    private struct __LogEntry{
        public string  Message;
        public Color4B Color;
    }

    private static readonly List<__LogEntry> __ConsoleEntries = [];
    private const           int              __MaxLogs        = 500;
    private static          bool             __ScrollToBottom = true;
    
    private static void Start_Console(){
        WL.Logger.CurrentLogger!.OnLog += (Type, Message) => {
            __ConsoleEntries.Add(new __LogEntry{ Message = Message, Color = new Color4B(255, 255, 255)});
            if(__ConsoleEntries.Count > __MaxLogs){ __ConsoleEntries.RemoveAt(0); }
            __ScrollToBottom = true;
        };
    }
    
    private static void Update_Console(){
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Console");

            if(ImGui.Button("Clear")){ __ConsoleEntries.Clear(); }
            ImGui.SameLine();
            if(ImGui.Button("Test Message")){ WL.Logger.Debug("TEST MESSAGE"); }
            ImGui.SameLine();
            ImGui.Text($"Logs: {__ConsoleEntries.Count}");
            
            ImGui.Separator();

            float FooterHeightToReverse = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0, -FooterHeightToReverse), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

                foreach(__LogEntry Entry in __ConsoleEntries){
                    ImGui.TextColored(new System.Numerics.Vector4(Entry.Color.R, Entry.Color.G, Entry.Color.B, Entry.Color.A), Entry.Message);
                }

                if(__ScrollToBottom){
                    ImGui.SetScrollHereY(1);
                    __ScrollToBottom = false;
                }
            
            ImGui.EndChild();
            
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);

        Update_Menu();
        
        ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        
        Update_SceneView();
        
        Update_Inspector();

        Update_Hierarchy();
        
        Update_Assets();
        
        Update_Console();
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}