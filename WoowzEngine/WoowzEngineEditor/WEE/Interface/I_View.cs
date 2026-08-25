using System.Numerics;
using ImGuiNET;
using WLO.Math;

namespace WEE_Interface;

public static class I_View{
    public static bool FocusSceneView{ get; private set; }

    public static Vector2I SceneViewSize{ get; private set; }

    public static bool Is2DView = false;

    public static Color4B BackgroundColor = new Color4B(200, 200, 200);
    
    public static void Update(){
        if(!WEE.Interface.WindowViewActive){ return; }

        if(ImGui.Begin("Просмотр сцены###View", ref WEE.Interface.WindowViewActive)){

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

            if(ImGui.BeginChild("SceneToolbar", new Vector2(0, 30), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoScrollbar)){
                ImGui.SameLine();

                ImGui.Text($"({SceneViewSize.W}x{SceneViewSize.H}), R-FPS: {WEE.Cycle.Render_DTI.FPS:F1}");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                string ModeText = Is2DView ? "2D" : "3D";

                if(ImGui.Button(ModeText, new Vector2(100, 20))){
                    Is2DView = !Is2DView;

                    WEE.Editor.SceneViewCamera.IsOrthographic = Is2DView;
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Переключить перспективу камеры"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Позиция:");
                ImGui.SameLine();
                Vector3 CameraPosition = new Vector3(WEE.Editor.SceneViewCamera.Position.X, WEE.Editor.SceneViewCamera.Position.Y, WEE.Editor.SceneViewCamera.Position.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CamPos", ref CameraPosition, 0.1f)){
                    WEE.Editor.SceneViewCamera.Position = new Vector3F(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Поворот:");
                ImGui.SameLine();
                Vector3 CameraRotation = new Vector3(WEE.Editor.SceneViewCamera.Rotation.X, WEE.Editor.SceneViewCamera.Rotation.Y, WEE.Editor.SceneViewCamera.Rotation.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CamRos", ref CameraRotation, 0.1f)){
                    WEE.Editor.SceneViewCamera.Rotation = new Vector3F(CameraRotation.X, CameraRotation.Y, CameraRotation.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                if(ImGui.Button("Сброс")){
                    WEE.Editor.SceneViewCamera.Position = WEE.Editor.SceneViewCamera.Rotation = new Vector3F();
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Сбросить позицию и поворот камеры на дефолтные значения"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                Vector3 BackgroundColor__ = new Vector3(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);
                if(ImGui.ColorEdit3("##Background", ref BackgroundColor__, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel)){
                    BackgroundColor = new Color4B((byte)(BackgroundColor__.X * 255), (byte)(BackgroundColor__.Y * 255), (byte)(BackgroundColor__.Z * 255));
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Цвет заднего фона"); }
            } ImGui.EndChild();

            Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            __SceneViewport.X = System.Math.Max(1, __SceneViewport.X);
            __SceneViewport.Y = System.Math.Max(1, __SceneViewport.Y);
            SceneViewSize = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);

            if(WEE.Interface.CurrentScene != null){
                ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ResultTexture!.ID, __SceneViewport, new Vector2(0, 1), new Vector2(1, 0));
            }
        } ImGui.End();
    }
}