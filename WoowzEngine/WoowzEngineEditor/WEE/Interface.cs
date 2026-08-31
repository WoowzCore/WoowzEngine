using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WEE_Interface;
using WEEO;
using WEO;
using WLO.Interface;
using WoowzLib.Interface.ImGUI;

namespace WEE;

// todo, NativeFileDialogSharp

public static class Interface{
    public static GLImGUI ImGUI{ get; private set; } = null!;

    public static bool WindowViewActive      = true;
    public static bool WindowConfigActive    = false;
    public static bool WindowAssetsActive    = true;
    public static bool WindowHierarchyActive = true;
    public static bool WindowInspectorActive = true;
    public static bool WindowConsoleActive   = true;
    public static bool WindowImGUIDemoActive = false;
    
    public static bool __IsProjectLoaded = false;
    
    public static EditorConfig? Config     = null!;
    public static string        ConfigPath = "";
    
    public static Scene?  CurrentScene;
    
    public static Entity? CurrentEntity = null!;
    
    // ----------------------------------------------------------------------
    
    public static void Start(){
        ImGUI = new GLImGUI(WEE.Render.API, true);

        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        ImGUI.IO.ConfigWindowsMoveFromTitleBarOnly = true;
        
        ImGuiStylePtr Style = ImGui.GetStyle();
        RangeAccessor<Vector4> Colors = Style.Colors;

        Vector4 mainRed = new Vector4(0.70f, 0.00f, 0.00f, 1.00f);
        Vector4 hoverRed = new Vector4(0.85f, 0.10f, 0.10f, 1.00f);
        Vector4 activeRed = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        Vector4 darkRed = new Vector4(0.40f, 0.00f, 0.00f, 1.00f);

        Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.15f, 0.00f, 0.00f, 1.00f);
        Colors[(int)ImGuiCol.TitleBgActive] = darkRed;
        Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);

        Colors[(int)ImGuiCol.Button] = mainRed;
        Colors[(int)ImGuiCol.ButtonHovered] = hoverRed;
        Colors[(int)ImGuiCol.ButtonActive] = activeRed;

        Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.05f, 0.05f, 0.54f);
        Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.40f, 0.10f, 0.10f, 0.40f);
        Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.50f, 0.10f, 0.10f, 0.67f);

        Colors[(int)ImGuiCol.Tab] = darkRed;
        Colors[(int)ImGuiCol.TabHovered] = hoverRed;
        Colors[(int)ImGuiCol.TabSelected] = activeRed;
        Colors[(int)ImGuiCol.TabSelectedOverline] = activeRed;
        Colors[(int)ImGuiCol.TabDimmed] = darkRed;
        Colors[(int)ImGuiCol.TabDimmedSelected] = mainRed;

        Colors[(int)ImGuiCol.Header] = darkRed;
        Colors[(int)ImGuiCol.HeaderHovered] = mainRed;
        Colors[(int)ImGuiCol.HeaderActive] = hoverRed;

        Colors[(int)ImGuiCol.CheckMark] = activeRed;
        Colors[(int)ImGuiCol.SliderGrab] = mainRed;
        Colors[(int)ImGuiCol.SliderGrabActive] = hoverRed;
        Colors[(int)ImGuiCol.SeparatorHovered] = hoverRed;
        Colors[(int)ImGuiCol.SeparatorActive] = activeRed;
        Colors[(int)ImGuiCol.ResizeGrip] = darkRed;
        Colors[(int)ImGuiCol.ResizeGripHovered] = mainRed;
        Colors[(int)ImGuiCol.ResizeGripActive] = activeRed;
        Colors[(int)ImGuiCol.DockingPreview] = mainRed;
        Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.90f, 0.20f, 0.20f, 0.35f);

        Style.WindowRounding = 0;
        Style.FrameRounding = 12;
        Style.PopupRounding = 0;
        Style.GrabRounding = 12;
        Style.TabRounding = 0;

        Style.TabBorderSize = 1;
        
        WEE.Prefs.Load();
        
        I_Console.Start();
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------
    
    public static void RenderTextScrolling(string text, float maxWidth, bool isHovered){
        Vector2 pos = ImGui.GetCursorScreenPos();
        Vector2 textSize = ImGui.CalcTextSize(text);
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        Vector2 clipMin = pos;
        Vector2 clipMax = new Vector2(pos.X + maxWidth, pos.Y + ImGui.GetTextLineHeightWithSpacing());

        if (textSize.X <= maxWidth){
            drawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), text);
        }else{
            if (isHovered){
                float diff = textSize.X - maxWidth;
                float speed = 2.0f;
                float offset = (MathF.Sin((float)ImGui.GetTime() * speed) * 0.5f + 0.5f) * diff;

                drawList.PushClipRect(clipMin, clipMax, true);
                drawList.AddText(new Vector2(pos.X - offset, pos.Y), ImGui.GetColorU32(ImGuiCol.Text), text);
                drawList.PopClipRect();
            }else{
                string truncated = text;
                while (truncated.Length > 1 && ImGui.CalcTextSize(truncated + "...").X > maxWidth)
                {
                    truncated = truncated.Substring(0, truncated.Length - 1);
                }
                drawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.TextDisabled), truncated + "...");
            }
        }

        ImGui.Dummy(new Vector2(maxWidth, ImGui.GetTextLineHeightWithSpacing()));
    }
    
    // ----------------------------------------------------------------------
    
    private static bool __FirstFrame = true;
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);

            if(!__IsProjectLoaded){
                I_Launcher.Update(); 
            }else{
                I_Menu.Update();

                uint DockSpaceID = ImGui.GetID("MainDockSpace");
                ImGui.DockSpaceOverViewport(DockSpaceID, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                if(__FirstFrame){
                    __FirstFrame = false;

                    Assembly? GameAssembly = null;
                    try{
                        if(Config != null && !string.IsNullOrEmpty(Config.GameDLLPath)){ GameAssembly = Assembly.LoadFrom(Config.GameDLLPath); }
                    }catch(Exception e){
                        WL.Logger.Error($"ошибка при загрузке dll, todo 2", e);
                    }
                    WEE.Registry.ResetAndReload(GameAssembly);

                    ImGuiDockBuilder.igDockBuilderRemoveNode(DockSpaceID); 
                    ImGuiDockBuilder.igDockBuilderAddNode(DockSpaceID, ImGuiDockNodeFlags.None);
                    ImGuiDockBuilder.igDockBuilderSetNodeSize(DockSpaceID, ImGui.GetMainViewport().Size);

                    ImGuiDockBuilder.igDockBuilderSplitNode(DockSpaceID, ImGuiDir.Right, 0.25f, out uint dockid_right, out uint dockid_left);

                    ImGuiDockBuilder.igDockBuilderSplitNode(dockid_left, ImGuiDir.Up, 0.75f, out uint dockid_up, out uint dockid_down);

                    ImGuiDockBuilder.igDockBuilderSplitNode(dockid_down, ImGuiDir.Right, 0.15f, out uint dockid_down_right, out uint dockid_down_left);

                    ImGuiDockBuilder.igDockBuilderDockWindow("###View", dockid_up);
                    
                    ImGuiDockBuilder.igDockBuilderDockWindow("###Inspector", dockid_right);
                    
                    ImGuiDockBuilder.igDockBuilderDockWindow("###Hierarchy", dockid_down_right);
                    
                    ImGuiDockBuilder.igDockBuilderDockWindow("###Assets", dockid_down_left);
                    ImGuiDockBuilder.igDockBuilderDockWindow("###Console", dockid_down_left);
                    
                    ImGuiDockBuilder.igDockBuilderFinish(DockSpaceID);
                }
                
                I_View.Update();
                I_Inspector.Update();
                I_Hierarchy.Update();
                I_Assets.Update();
                I_Console.Update();
                I_Config.Update();

                if(WindowImGUIDemoActive){ ImGui.ShowDemoWindow(ref WindowImGUIDemoActive); }
            }
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}