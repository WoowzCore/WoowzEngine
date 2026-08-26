using System.Numerics;
using ImGuiNET;
using WLO.Math;

namespace WEE_Interface;

public static class I_View{
    public static bool FocusSceneView{ get; private set; }

    public static Vector2I SceneViewSize{ get; private set; }

    private static bool __Is2DView = false;
    public static bool Is2DView{
        get => __Is2DView;
        set{
            __Is2DView = value;
            WEE.Editor.ViewCamera.IsOrthographic = __Is2DView;
        }
    }

    public static bool ShowDepth = false;

    public static Color4B BackgroundColor = new Color4B(200, 200, 200);
    
    public static void Update(){
        if(!WEE.Interface.WindowViewActive){ return; }

        if(ImGui.Begin("Просмотр###View", ref WEE.Interface.WindowViewActive)){

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

            if(ImGui.BeginChild("SceneToolbar", new Vector2(0, 30), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoScrollbar)){
                ImGui.SameLine();

                ImGui.Text($"({SceneViewSize.W}x{SceneViewSize.H}), R-FPS: {WEE.Cycle.Render_DTI.FPS:F1}");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                string ModeText = Is2DView ? "2D" : "3D";

                if(ImGui.Button(ModeText, new Vector2(100, 20))){ Is2DView = !Is2DView; }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Переключить перспективу камеры"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Позиция:");
                ImGui.SameLine();
                Vector3 CameraPosition = new Vector3(WEE.Editor.ViewCamera.Position.X, WEE.Editor.ViewCamera.Position.Y, WEE.Editor.ViewCamera.Position.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraPosition", ref CameraPosition, 0.1f, 0, 0, "%g")){
                    WEE.Editor.ViewCamera.Position = new Vector3F(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Поворот:");
                ImGui.SameLine();
                Vector3 CameraRotation = new Vector3(WEE.Editor.ViewCamera.Rotation.X, WEE.Editor.ViewCamera.Rotation.Y, WEE.Editor.ViewCamera.Rotation.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraRotation", ref CameraRotation, 0.1f, 0, 0, "%g")){
                    WEE.Editor.ViewCamera.Rotation = new Vector3F(CameraRotation.X, CameraRotation.Y, CameraRotation.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                ImGui.TextDisabled("Скорость:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50);
                ImGui.DragFloat("##CameraSpeed", ref WEE.Editor.CameraSpeed, 0.1f, 0.001f, 1000, "%g");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                if(ImGui.Button("Сброс")){
                    WEE.Editor.ViewCamera.Position = WEE.Editor.ViewCamera.Rotation = new Vector3F();
                    WEE.Editor.CameraSpeed = 1;
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Сбросить позицию и поворот камеры на дефолтные значения"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                Vector3 BackgroundColor__ = new Vector3(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);
                if(ImGui.ColorEdit3("##BackgroundColor", ref BackgroundColor__, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel)){
                    BackgroundColor = new Color4B((byte)(BackgroundColor__.X * 255), (byte)(BackgroundColor__.Y * 255), (byte)(BackgroundColor__.Z * 255));
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Цвет заднего фона"); }
                
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                string FrameText = ShowDepth ? "Глубина" : "Цвет";

                if(ImGui.Button(FrameText, new Vector2(100, 20))){ ShowDepth = !ShowDepth; }
            } ImGui.EndChild();

            Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            __SceneViewport.X = System.Math.Max(1, __SceneViewport.X);
            __SceneViewport.Y = System.Math.Max(1, __SceneViewport.Y);
            SceneViewSize = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);

            if(WEE.Interface.CurrentScene != null){
                uint TextureID;

                if(ShowDepth){
                    TextureID = WEE.Render.SceneFramebuffer.TextureDepth!.ID;
                }else{
                    TextureID = WEE.Render.SceneFramebuffer.TextureColor0!.ID;
                }
                
                ImGui.Image((IntPtr)TextureID, __SceneViewport, new Vector2(0, 1), new Vector2(1, 0));
            }
        } ImGui.End();
    }
}