using System.Numerics;
using ImGuiNET;
using WEI_Attribute;
using WEO;
using WLO.Math;

namespace WEE_Interface;

public static class I_View{
    public static bool FocusSceneView{ get; private set; }

    public static Vector2I SceneViewSize{ get; private set; }

    public static Vector2I ViewMousePosition;

    private static bool __Is2DView = false;
    public static bool Is2DView{
        get => __Is2DView;
        set{
            __Is2DView = value;
            WEE.Editor.ViewCamera.IsOrthographic = __Is2DView;
        }
    }

    public enum ShowWhatType{
        Scene, Depth, Picking
    }
    public static ShowWhatType ShowWhat = ShowWhatType.Scene;

    public static Color4B BackgroundColor = new Color4B(200, 200, 200);
    
    public static void Update(){
        if(!WEE.Interface.WindowViewActive){ return; }

        if(ImGui.Begin("Просмотр###View", ref WEE.Interface.WindowViewActive)){

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 0)); 
            if(ImGui.BeginChild("SceneToolbar", new Vector2(0, 35), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoScrollbar)){
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5); 
                ImGui.Indent(5);

                ImGui.Text($"({SceneViewSize.W}x{SceneViewSize.H}), R-FPS: {WEE.Cycle.Render_DTI.FPS:F1}");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                string ModeText = Is2DView ? "2D" : "3D";

                if(ImGui.Button(ModeText, new Vector2(50, 20))){ Is2DView = !Is2DView; }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Переключить перспективу камеры"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Поз.:");
                ImGui.SameLine();
                Vector3 CameraPosition = new Vector3(WEE.Editor.ViewCamera.Position.X, WEE.Editor.ViewCamera.Position.Y, WEE.Editor.ViewCamera.Position.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraPosition", ref CameraPosition, 0.1f, 0, 0, "%g")){
                    WEE.Editor.ViewCamera.Position = new Vector3F(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Пов.:");
                ImGui.SameLine();
                Vector3 CameraRotation = new Vector3(WEE.Editor.ViewCamera.Rotation.X, WEE.Editor.ViewCamera.Rotation.Y, WEE.Editor.ViewCamera.Rotation.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraRotation", ref CameraRotation, 0.1f, 0, 0, "%g")){
                    WEE.Editor.ViewCamera.Rotation = new Vector3F(CameraRotation.X, CameraRotation.Y, CameraRotation.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                ImGui.TextDisabled("Скор.:");
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
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Сбросить настройки камеры на дефолтные значения"); }

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

                ImGui.SetNextItemWidth(100);
                if(ImGui.BeginCombo("##ShowWhat", ShowWhat switch{
                    ShowWhatType.Scene => "Сцена",
                    ShowWhatType.Depth => "Глубина",
                    ShowWhatType.Picking => "ID"
                })){
                    if(ImGui.Selectable("Сцена", ShowWhat == ShowWhatType.Scene)){ ShowWhat = ShowWhatType.Scene; }
                    if(ImGui.Selectable("Глубина", ShowWhat == ShowWhatType.Depth)){ ShowWhat = ShowWhatType.Depth; }
                    if(ImGui.Selectable("ID", ShowWhat == ShowWhatType.Picking)){ ShowWhat = ShowWhatType.Picking; }
                    
                    ImGui.EndCombo();
                }
            } ImGui.EndChild();
            ImGui.PopStyleVar();

            Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            __SceneViewport.X = System.Math.Max(1, __SceneViewport.X);
            __SceneViewport.Y = System.Math.Max(1, __SceneViewport.Y);
            SceneViewSize = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);

            if(WEE.Interface.CurrentScene != null){
                if(WEE.Registry.HasMethods<WEE_OnViewRender>()){
                    uint TextureID = ShowWhat switch{
                        ShowWhatType.Scene => WEE.Render.SceneView.TextureColor0!.ID,
                        ShowWhatType.Depth => WEE.Render.SceneView.TextureDepth!.ID,
                        ShowWhatType.Picking => WEE.Render.PickingView.TextureColor0!.ID
                    };
                
                    ImGui.Image((IntPtr)TextureID, __SceneViewport, new Vector2(0, 1), new Vector2(1, 0));
                    Vector2 ImagePositionMin = ImGui.GetItemRectMin();
                    ViewMousePosition = new Vector2I(
                        (int)(WEE.Control.MousePosition.X - ImagePositionMin.X),
                        (int)(WEE.Control.MousePosition.Y - ImagePositionMin.Y)
                    );
                }else{
                    string WarningText = "Укажите метод рендера сцены через атрибут [WEE_OnViewRender]!";
                    Vector2 TextSize = ImGui.CalcTextSize(WarningText);
                    
                    ImGui.SetCursorPos(new Vector2(
                        ImGui.GetCursorPosX() + (__SceneViewport.X - TextSize.X) * 0.5f,
                        ImGui.GetCursorPosY() + (__SceneViewport.Y - TextSize.Y) * 0.5f
                    ));
                    ImGui.TextColored(new Vector4(1, 0.4f, 0, 1), WarningText);
                }
            }else{
                // todo, Я НАХУЙ МЕГАТРОН, ДЕЛАЯ ПОВТОРЫ КОДА НАХУЙ Я МЕГАТРОН ДЕЛАЯ ПОВТОРЫ КОДА ПОВТОРЫ КОДА ПОВТОРЫ КОДА ПОВТОРИТЕ ПОЖАЛУЙСТА НЕ РАССЛЫШАЛ, ВЫ СКАЗАЛИ ПОВТОРЫ КОДА?
                
                string WarningText = "Откройте сцену для рендера сцены";
                Vector2 TextSize = ImGui.CalcTextSize(WarningText);
                    
                ImGui.SetCursorPos(new Vector2(
                    ImGui.GetCursorPosX() + (__SceneViewport.X - TextSize.X) * 0.5f,
                    ImGui.GetCursorPosY() + (__SceneViewport.Y - TextSize.Y) * 0.5f
                ));
                ImGui.TextColored(new Vector4(1, 0.4f, 0, 1), WarningText);
            }
        } ImGui.End();
    }
    
    public static void ClickToView(){
        if(ViewMousePosition.X < 0 || ViewMousePosition.Y < 0 || ViewMousePosition.X > SceneViewSize.X || ViewMousePosition.Y > SceneViewSize.Y){ return; }

        // flip y
        Vector2I PickPosition = new Vector2I(ViewMousePosition.X, SceneViewSize.Y - ViewMousePosition.Y);
        
        Color4B Color = WEE.Render.PickingView.GetRect(new Rect2I(PickPosition, new Vector2I(1, 1)))[0];
        uint ID = Color.ToUInt();
        
        WEE.Interface.CurrentEntity = ID != 0 ? Entity.GetFromID(ID) : null;
    }
}