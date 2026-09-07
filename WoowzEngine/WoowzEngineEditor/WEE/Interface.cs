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
    
    // ----------------------------------------------------------------------
    
    public static void Start(){
        WEE.D.ImGUI = new GLImGUI(WEE.Render.API, true);

        WEE.D.ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        WEE.D.ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        WEE.D.ImGUI.IO.ConfigWindowsMoveFromTitleBarOnly = true;
        
        ImGuiStylePtr Style = ImGui.GetStyle();
        RangeAccessor<Vector4> Colors = Style.Colors;

        Vector4 BgDeep    = new Vector4(0.12f, 0.12f, 0.12f, 1);
        Vector4 BgMid     = new Vector4(0.18f, 0.18f, 0.18f, 1);
        Vector4 BgLight   = new Vector4(0.24f, 0.24f, 0.24f, 1);
        Vector4 MainRed   = new Vector4(0.70f, 0.00f, 0.00f, 1);
        Vector4 HoverRed  = new Vector4(0.85f, 0.10f, 0.10f, 1);
        Vector4 ActiveRed = new Vector4(1.00f, 0.00f, 0.00f, 1);
        Vector4 DarkRed   = new Vector4(0.40f, 0.00f, 0.00f, 1);

        Colors[(int)ImGuiCol.WindowBg]               = BgMid;
        Colors[(int)ImGuiCol.ChildBg]                = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        Colors[(int)ImGuiCol.PopupBg]                = BgDeep;
        Colors[(int)ImGuiCol.Border]                 = new Vector4(0.30f, 0.30f, 0.30f, 0.50f);
        Colors[(int)ImGuiCol.Text]                   = new Vector4(0.95f, 0.95f, 0.95f, 1.00f);
        Colors[(int)ImGuiCol.TextDisabled]           = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);

        Colors[(int)ImGuiCol.TitleBg]                = new Vector4(0.25f, 0.05f, 0.05f, 1.00f);
        Colors[(int)ImGuiCol.TitleBgActive]          = DarkRed;
        Colors[(int)ImGuiCol.TitleBgCollapsed]       = new Vector4(0.15f, 0.00f, 0.00f, 0.51f);
        Colors[(int)ImGuiCol.MenuBarBg]              = BgDeep;

        Colors[(int)ImGuiCol.Button]                 = MainRed;
        Colors[(int)ImGuiCol.ButtonHovered]          = HoverRed;
        Colors[(int)ImGuiCol.ButtonActive]           = ActiveRed;

        Colors[(int)ImGuiCol.CheckMark]              = ActiveRed;
        Colors[(int)ImGuiCol.SliderGrab]             = MainRed;
        Colors[(int)ImGuiCol.SliderGrabActive]       = HoverRed;

        Colors[(int)ImGuiCol.FrameBg]                = BgDeep;
        Colors[(int)ImGuiCol.FrameBgHovered]         = BgLight;
        Colors[(int)ImGuiCol.FrameBgActive]          = new Vector4(0.40f, 0.10f, 0.10f, 0.40f);

        Colors[(int)ImGuiCol.Tab]                    = BgDeep;
        Colors[(int)ImGuiCol.TabHovered]             = HoverRed;
        Colors[(int)ImGuiCol.TabSelected]            = MainRed;
        Colors[(int)ImGuiCol.TabSelectedOverline]    = ActiveRed;
        Colors[(int)ImGuiCol.TabDimmed]              = BgDeep;
        Colors[(int)ImGuiCol.TabDimmedSelected]      = new Vector4(0.30f, 0.10f, 0.10f, 1.00f);

        Colors[(int)ImGuiCol.Header]                 = new Vector4(0.50f, 0.00f, 0.00f, 0.35f);
        Colors[(int)ImGuiCol.HeaderHovered]          = MainRed;
        Colors[(int)ImGuiCol.HeaderActive]           = ActiveRed;

        Colors[(int)ImGuiCol.Separator]              = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);
        Colors[(int)ImGuiCol.SeparatorHovered]       = MainRed;
        Colors[(int)ImGuiCol.SeparatorActive]        = ActiveRed;

        Colors[(int)ImGuiCol.ScrollbarBg]            = BgDeep;
        Colors[(int)ImGuiCol.ScrollbarGrab]          = BgLight;
        Colors[(int)ImGuiCol.ScrollbarGrabHovered]   = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
        Colors[(int)ImGuiCol.ScrollbarGrabActive]    = MainRed;

        Colors[(int)ImGuiCol.DockingPreview]         = MainRed;
        Colors[(int)ImGuiCol.DockingEmptyBg]         = BgDeep;
        Colors[(int)ImGuiCol.TextSelectedBg]         = new Vector4(0.70f, 0.00f, 0.00f, 0.35f);
        Colors[(int)ImGuiCol.NavWindowingHighlight]  = MainRed;

        Style.WindowRounding    = 0;
        Style.FrameRounding     = 4;
        Style.PopupRounding     = 0;
        Style.GrabRounding      = 4;
        Style.TabRounding       = 0;
        
        Style.WindowBorderSize  = 1;
        Style.FrameBorderSize   = 0;
        
        WEE.Prefs.Load();
        
        I_Console.Start();
    }
    
    public static void Stop(){
        if(WEE.D.ImGUI != null!){ WEE.D.ImGUI.Stop(); WEE.D.ImGUI = null!; }
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
        WEE.D.ImGUI.Build((float)WEE.D.Time.Render.DTI.DT, WEE.D.Window.Size, () => {
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
        });
    }

    public static void Draw() => WEE.D.ImGUI.Draw();
}