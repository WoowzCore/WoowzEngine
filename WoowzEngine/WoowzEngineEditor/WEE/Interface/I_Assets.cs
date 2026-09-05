using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using WEI_Attribute;
using WLO;
using WLO.GPU;
using WLO.Interface;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;

namespace WEE_Interface;

public static class I_Assets{
    public static void Update(){
        if(!WEE.Interface.WindowAssetsActive){ return; }

        GLImGUI GUI = WEE.Interface.ImGUI;
        
        GUI.Window("Ресурсы###Assets", ref WEE.Interface.WindowAssetsActive, () => {
            List<Type> AssetTypes = WE.Asset.ExplicitTypes.OrderBy(T => T.Name).ToList();

            if(AssetTypes.Count == 0){
                ImGui.TextDisabled("Нет зарегистрированных ресурсов");
            }else{
                foreach(Type Type in AssetTypes){
                    GUI.CustomID(Type.FullName!, () => {
                        ImGui.Spacing();
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.8f, 0.4f, 1));
                            ImGui.Text(Type.Name.ToUpper());
                        ImGui.PopStyleColor();
                        ImGui.Separator();
                        ImGui.Spacing();

                        List<string> Keys = WE.Asset.GetKeysForType(Type).OrderBy(k => k).ToList();

                        const float IconSize = 128;
                        float Padding = ImGui.GetStyle().ItemSpacing.X;

                        float WindowVisibleRightEdge = ImGui.GetCursorScreenPos().X + ImGui.GetContentRegionAvail().X;

                        WEE.Registry.RunMethods<WEE_OnPreAllRenderPreview>(false, WEE.Render.API, WEE.Cycle.Render_DTI, WEE.Cycle.Render_Time);
                        
                        for(int i = 0; i < Keys.Count; i++){
                            string Key = Keys[i];
                            int ID = WE.Asset.GetID(Key);

                            GUI.CustomID(Key, () => {
                                GUI.Group(() => {
                                    Vector2 CursorPosition = ImGui.GetCursorScreenPos();

                                    bool IsVisible = ImGui.IsRectVisible(new Vector2(IconSize, IconSize));
                                    ImGui.InvisibleButton("##button", new Vector2(IconSize, IconSize));

                                    GUI.DragDropSource(() => {
                                        IntPtr Ptr = Marshal.StringToHGlobalAnsi(Key);
                                        ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)Key.Length + 1);
                                        Marshal.FreeHGlobal(Ptr);
                                    
                                        ImGui.Text($"Перетаскивание ассета: {Key}");
                                    });
                                
                                    bool IsHovered = ImGui.IsItemHovered();
                                    bool IsActive  = ImGui.IsItemActive();
                                    
                                    uint Column = ImGui.GetColorU32(IsActive ? ImGuiCol.HeaderActive : IsHovered ? ImGuiCol.HeaderHovered : ImGuiCol.FrameBg);

                                    ImGui.GetWindowDrawList().AddRectFilled(CursorPosition, new Vector2(CursorPosition.X + IconSize, CursorPosition.Y + IconSize), Column, 4.0f);

                                    RenderPreview(Key, CursorPosition, IconSize, IsVisible, ID);
                                    
                                    WEE.Interface.RenderTextScrolling($"[{ID}] {Key}", IconSize, IsHovered);
                                });

                                float CurrentItemRightEdge = ImGui.GetItemRectMax().X;
                                float NextItemRightEdge = CurrentItemRightEdge + Padding + IconSize;

                                if(i + 1 < Keys.Count && NextItemRightEdge < WindowVisibleRightEdge){
                                    ImGui.SameLine();
                                }
                            });
                        }

                        ImGui.Spacing();
                    });
                }
            }
        });
    }

    private static readonly Dictionary<int, GLTexture2D> __PreviewTextures = [];
    private static          GLView?                      __SharedPreviewView;
    
    private static void UpdateAssetPreview(object Asset, int ID, string Key){
        const int Size = 128;

        if(!__PreviewTextures.TryGetValue(ID, out GLTexture2D? Texture)){
            Texture = WEE.Render.API.CreateTexture2D(new Vector2I(Size, Size));
            __PreviewTextures[ID] = Texture;
        }

        if(__SharedPreviewView == null){
            __SharedPreviewView = GLView.Create(WEE.Render.API, new Vector2I(Size, Size), new PixelLayout(
                PixelAttribute.Color("Color", 4)
            ));
        }

        GLView OldView = WEE.Render.API.Pool.GetView();
        WEE.Render.API.Pool.SetView(__SharedPreviewView);

        __SharedPreviewView!.SetTexture(Texture);

        GLMesh?    TargetMesh    = null;
        GLProgram? TargetProgram = null;

        switch(Asset){
            case GLMesh AssetMesh__:
                TargetMesh = AssetMesh__;
                break;
            case GLProgram AssetProgram__:
                TargetProgram = AssetProgram__;
                break;
        }
        
        WEE.Render.API.Render(() => {
            WEE.Registry.RunFirstDelegate<WEE_OnRenderPreview, Action<OpenGL, GLMesh?, GLProgram?, DeltaTimeInfo, object, int, string>>(true,
                WEE.Render.API,
                TargetMesh,
                TargetProgram,
                WEE.Cycle.Render_DTI,
                Asset,
                ID,
                Key
            );
        });
        
        WEE.Render.API.Pool.SetView(OldView);
    }
    
    private static void RenderPreview(string Key, Vector2 CursorPosition, float IconSize, bool IsVisible, int ID){
        if(!IsVisible){ return; }

        ImDrawListPtr DrawList = ImGui.GetWindowDrawList();

        object? Asset = WE.Asset.Resolve<object>(ID);

        const float Margin = 4;
        Vector2 PMix = CursorPosition + new Vector2(Margin);
        Vector2 PMax = CursorPosition + new Vector2(IconSize - Margin);
        
        if(Asset is GLTexture2D Texture){
            DrawList.AddImage(
                (IntPtr)Texture.ID,
                PMix,
                PMax,
                new Vector2(0, 1),
                new Vector2(1, 0)
            );
        }else if(Asset is GLMesh || Asset is GLProgram){
            if(WEE.Registry.HasMethods<WEE_OnRenderPreview>()){
                UpdateAssetPreview(Asset, ID, Key);

                if(__PreviewTextures.TryGetValue(ID, out GLTexture2D? PreviewTexture)){
                    DrawList.AddImage(
                        (IntPtr)PreviewTexture.ID,
                        PMix,
                        PMax,
                        new Vector2(0, 1),
                        new Vector2(1, 0)
                    );
                }
            }else{
                string WarningText = "Нет метода\nрендера!";
                Vector2 TextSize = ImGui.CalcTextSize(WarningText);
                DrawList.AddText(CursorPosition + (new Vector2(IconSize) - TextSize) * 0.5f, ImGui.GetColorU32(new Vector4(1, 0.4f, 0.4f, 1)), WarningText);
            }
        }else{
            string Label = Asset?.GetType().Name ?? "Null";
            Vector2 TextSize = ImGui.CalcTextSize(Label);
            DrawList.AddText(CursorPosition + (new Vector2(IconSize) - TextSize) * 0.5f, ImGui.GetColorU32(ImGuiCol.TextDisabled), Label);
        }
    }
}