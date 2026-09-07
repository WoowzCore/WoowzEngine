using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;
using WEI_Attribute;
using WEO;
using WLO.Interface;
using WLO.Math;
using WLO.Render;

namespace WEE_Interface;

public static class I_View{
    private static bool __Is2DView = false;
    public static bool Is2DView{
        get => __Is2DView;
        set{
            __Is2DView = value;
            WEE.D.View.Camera.IsOrthographic = __Is2DView;
        }
    }
    
    public static Color4B BackgroundColor = new Color4B(200, 200, 200);
    
    // ----------------------------------------------------------------------
    
    private static readonly PixelAttribute PA_Default = new PixelAttribute("Default", 4, FramebufferAttachment.ColorAttachment0, InternalFormat.Rgba8);
    private static readonly PixelAttribute PA_Picking = new PixelAttribute("Picking", 4, FramebufferAttachment.ColorAttachment0, InternalFormat.Rgba8);

    private static PixelAttribute? SelectedAttachment = PA_Default;

    public static (string Name, string Asset)? SelectedEffect;

    private static Dictionary<string, string> AvailableEffects = [];
    
    public static void RefreshEffects(){
        AvailableEffects.Clear();

        if(WEE.Registry.HasMethods<WEE_OnDebugEffects>()){
            Dictionary<string, string>? Result = WEE.Registry.RunFirstDelegate<WEE_OnDebugEffects, Func<Dictionary<string, string>>>(false) as Dictionary<string, string>;
            AvailableEffects = Result!;
        }
    }
    
    // ----------------------------------------------------------------------
    
    public static void Update(){
        if(!WEE.Interface.WindowViewActive){ return; }

        ImGUI GUI = WEE.D.ImGUI;
        
        GLView? CameraLayout = null;
        List<PixelAttribute> SupportedPA = [];
        if(WEE.Registry.HasMethods<WEE_OnCameraPixelLayout>()){
            CameraLayout = WEE.Registry.RunFirstDelegate<WEE_OnCameraPixelLayout, Func<GLView>>(false) as GLView;

            if(CameraLayout != null){
                foreach(PixelAttribute PA in CameraLayout.Layout.Attributes){
                    if(PA.IsTexture){ SupportedPA.Add(PA); }
                }
            }
        }

        GUI.Window("Просмотр###View", ref WEE.Interface.WindowViewActive, () => {
            WEE.D.View.IsFocus = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 0));
            GUI.Child("SceneToolbar", new Vector2(0, 35), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoScrollbar, () => {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5); 
                ImGui.Indent(5);

                ImGui.Text($"({WEE.D.View.Size.W}x{WEE.D.View.Size.H}), R-FPS: {WEE.D.Time.Render.DTI.FPS:F1}");

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
                Vector3 CameraPosition = new Vector3(WEE.D.View.Camera.Position.X, WEE.D.View.Camera.Position.Y, WEE.D.View.Camera.Position.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraPosition", ref CameraPosition, 0.1f, 0, 0, "%g")){
                    WEE.D.View.Camera.Position = new Vector3F(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Пов.:");
                ImGui.SameLine();
                Vector3 CameraRotation = new Vector3(WEE.D.View.Camera.Rotation.X, WEE.D.View.Camera.Rotation.Y, WEE.D.View.Camera.Rotation.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CameraRotation", ref CameraRotation, 0.1f, 0, 0, "%g")){
                    WEE.D.View.Camera.Rotation = new Vector3F(CameraRotation.X, CameraRotation.Y, CameraRotation.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                ImGui.TextDisabled("Скор.:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50);
                ImGui.DragFloat("##CameraSpeed", ref WEE.D.View.CameraSpeed, 0.1f, 0.001f, 1000, "%g");
                
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                ImGui.TextDisabled("Far.:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50);
                ImGui.DragFloat("##CameraFar", ref WEE.D.View.Camera.Far, 0.1f, 0.001f, 100000, "%g");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                if(ImGui.Button("Сброс")){
                    WEE.D.View.Camera.Position = WEE.D.View.Camera.Rotation = new Vector3F();
                    WEE.D.View.CameraSpeed = 1;
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Сбросить настройки камеры на дефолтные значения"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                Vector3 BackgroundColor__ = new Vector3(BackgroundColor.R * WL.Math.Inverse255, BackgroundColor.G * WL.Math.Inverse255, BackgroundColor.B * WL.Math.Inverse255);
                if(ImGui.ColorEdit3("##BackgroundColor", ref BackgroundColor__, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel)){
                    BackgroundColor = new Color4B((byte)(BackgroundColor__.X * 255), (byte)(BackgroundColor__.Y * 255), (byte)(BackgroundColor__.Z * 255));
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Цвет заднего фона"); }
                
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
                
                if(ImGui.Button(SelectedAttachment != null ? SelectedAttachment.Value.Name : SelectedEffect!.Value.Name)){
                    ImGui.OpenPopup("GBufferMenu");
                }

                GUI.Popup("GBufferMenu", () => {
                    GUI.Menu("Канал", () => {
                        void MenuItem(PixelAttribute PA){ if(ImGui.MenuItem(PA.Name, "", SelectedAttachment == PA)){ SelectedAttachment = PA; SelectedEffect = null; } }
                    
                        MenuItem(PA_Default);
                        foreach(PixelAttribute PA in SupportedPA){ MenuItem(PA); }
                        MenuItem(PA_Picking);
                    });

                    GUI.Menu("Эффекты", () => {
                        foreach(KeyValuePair<string, string> Effect in AvailableEffects){
                            if(ImGui.MenuItem(Effect.Key, "", SelectedEffect?.Asset == Effect.Value)){ SelectedEffect = (Effect.Key, Effect.Value); SelectedAttachment = null; }
                        }
                    });
                });
            });
            ImGui.PopStyleVar();

            Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            __SceneViewport.X = System.Math.Max(1, __SceneViewport.X);
            __SceneViewport.Y = System.Math.Max(1, __SceneViewport.Y);
            WEE.D.View.Size = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);

            WEE.D.View.IsMouseOver = false;
            
            if(WEE.D.Selected.Scene != null){
                if(WEE.Registry.HasMethods<WEE_OnRenderView>()){
                    if(WEE.Render.SceneView != null!){
                        PixelAttribute PA = SelectedAttachment ?? PA_Default;
                        
                        uint TextureID;

                        if(PA == PA_Default){
                            TextureID = WEE.Render.SceneView.TextureColor0!.ID;
                        }else if(PA == PA_Picking){
                            TextureID = WEE.Render.PickingView.TextureColor0!.ID;
                        }else{
                            TextureID = CameraLayout!.GetTexture(PA.Attachment)!.ID;
                        }
            
                        ImGui.Image((IntPtr)TextureID, __SceneViewport, new Vector2(0, 1), new Vector2(1, 0));

                        WEE.D.View.IsMouseOver = ImGui.IsItemHovered(ImGuiHoveredFlags.None);
                        
                        Vector2 ImagePositionMin = ImGui.GetItemRectMin();
                        WEE.D.View.LocalMousePosition = new Vector2I(
                            (int)(WEE.D.Input.M.Position.X - ImagePositionMin.X),
                            (int)(WEE.D.Input.M.Position.Y - ImagePositionMin.Y)
                        );
                    }else{
                        // todo, Я ВСЁ ЕЩЁ МЕГАТРОН ДЕЛАЯ ПОВТОРЫ
                        string WarningText = "WEE.Render.SceneView равен null!";
                        Vector2 TextSize = ImGui.CalcTextSize(WarningText);
                    
                        ImGui.SetCursorPos(new Vector2(
                            ImGui.GetCursorPosX() + (__SceneViewport.X - TextSize.X) * 0.5f,
                            ImGui.GetCursorPosY() + (__SceneViewport.Y - TextSize.Y) * 0.5f
                        ));
                        ImGui.TextColored(new Vector4(1, 0.4f, 0, 1), WarningText);
                    }
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
        });
    }
    
    public static void ClickToView(){
        if(WEE.D.View.LocalMousePosition.X < 0 || WEE.D.View.LocalMousePosition.Y < 0 || WEE.D.View.LocalMousePosition.X > WEE.D.View.Size.X || WEE.D.View.LocalMousePosition.Y > WEE.D.View.Size.Y){ return; }

        // flip y
        Vector2I PickPosition = new Vector2I(WEE.D.View.LocalMousePosition.X, WEE.D.View.Size.Y - WEE.D.View.LocalMousePosition.Y);
        
        Color4B Color = WEE.Render.PickingView.GetRect(new Rect2I(PickPosition, new Vector2I(1, 1)))[0];
        uint ID = Color.ToUInt();
        
        WEE.D.Selected.Entity = ID != 0 ? Entity.GetFromID(ID) : null;
    }
}