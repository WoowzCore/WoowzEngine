using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using WLO;
using WLO.GPU;
using WLO.Math;
using WLO.Render;

namespace WEE_Interface;

public static class I_Assets{
    public static void Update(){
        if(!WEE.Interface.WindowAssetsActive){ return; }

        if(ImGui.Begin("Ресурсы###Assets", ref WEE.Interface.WindowAssetsActive)){
            List<Type> AssetTypes = WE.Asset.ExplicitTypes.OrderBy(T => T.Name).ToList();

            if(AssetTypes.Count == 0){
                ImGui.TextDisabled("Нет зарегистрированных ресурсов");
            }else{
                foreach(Type Type in AssetTypes){
                    ImGui.PushID(Type.FullName); 
                    
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

                        for(int i = 0; i < Keys.Count; i++){
                            string Key = Keys[i];
                            int ID = WE.Asset.GetID(Key);

                            ImGui.PushID(Key);

                                ImGui.BeginGroup();
                                    Vector2 CursorPosition = ImGui.GetCursorScreenPos();

                                    bool IsVisible = ImGui.IsRectVisible(new Vector2(IconSize, IconSize));
                                    ImGui.InvisibleButton("##button", new Vector2(IconSize, IconSize));

                                    if(ImGui.BeginDragDropSource()){
                                        IntPtr Ptr = Marshal.StringToHGlobalAnsi(Key);
                                        ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)Key.Length + 1);
                                        Marshal.FreeHGlobal(Ptr);
                                    
                                        ImGui.Text($"Перетаскивание ассета: {Key}");
                                        ImGui.EndDragDropSource();
                                    }
                                
                                    bool IsHovered = ImGui.IsItemHovered();
                                    bool IsActive  = ImGui.IsItemActive();
                                    
                                    uint Column = ImGui.GetColorU32(IsActive ? ImGuiCol.HeaderActive : IsHovered ? ImGuiCol.HeaderHovered : ImGuiCol.FrameBg);

                                    ImGui.GetWindowDrawList().AddRectFilled(CursorPosition, new Vector2(CursorPosition.X + IconSize, CursorPosition.Y + IconSize), Column, 4.0f);

                                    RenderPreview(Key, CursorPosition, IconSize, IsVisible, ID);
                                    
                                    WEE.Interface.RenderTextScrolling($"[{ID}] {Key}", IconSize, IsHovered);
                                ImGui.EndGroup();

                                float CurrentItemRightEdge = ImGui.GetItemRectMax().X;
                                float NextItemRightEdge = CurrentItemRightEdge + Padding + IconSize;

                                if(i + 1 < Keys.Count && NextItemRightEdge < WindowVisibleRightEdge){
                                    ImGui.SameLine();
                                }

                            ImGui.PopID();
                        }

                        ImGui.Spacing();
                        
                    ImGui.PopID();
                }
            }
        } ImGui.End();

        __PreviewRotation += WEE.Cycle.Render_DT / 2;
    }

    private static readonly Dictionary<int, GLTexture2D> __PreviewTextures = [];
    private static          GLView?                      __SharedPreviewView;
    private static          float                        __PreviewRotation = 0;

    private static void RenderAsset(object Asset){
        Matrix4F View = Matrix4F.CreateLookAt(new Vector3F(0, 0, 5), new Vector3F(0, 0, 0), new Vector3F(0, 1, 0));
        Matrix4F Projection = Matrix4F.CreatePerspective(45, 1, 0.1f, 100);
        
        WEE.Render.UB_Default.Update(new WEE.Render.UniformBlock_Default {
            ViewProjection = Projection * View,
            Time = (float)ImGui.GetTime()
        });
        
        WE.Render.API.Pool.SetUniformBlock(WEE.Render.UB_Default, 0, true);

        Matrix4F CalculateModel(GLMesh Mesh){
            Bounds Bounds = Mesh.Bounds;
            
            Matrix4F Translation = Matrix4F.CreateTranslation(-Bounds.Center);

            float MaxDimension = System.Math.Max(Bounds.Size.X, System.Math.Max(Bounds.Size.Y, Bounds.Size.Z));
            float ScaleFactor = (MaxDimension > 0.0001f) ? (3 / MaxDimension) : 1;
            Matrix4F Scale = Matrix4F.CreateScale(new Vector3F(ScaleFactor, ScaleFactor, ScaleFactor));

            Matrix4F Rotation = Matrix4F.CreateRotationY(__PreviewRotation);

            return Rotation * Scale * Translation;
        }
        
        GLTexture2D? TestTexture = WE.Asset.Resolve<GLTexture2D>(WE.Asset.GetID("Texture/Test"));
        WE.Render.API.Pool.SetTexture2D(TestTexture);
        
        if(Asset is GLMesh Mesh){
            GLProgram? TestProgram = WE.Asset.Resolve<GLProgram>(WE.Asset.GetID("Shader/Default"));
            if(TestProgram != null){
                TestProgram.SetUniform(UniformValue.CreateM4F(0, CalculateModel(Mesh)));
                TestProgram.SetUniform(UniformValue.CreateV3F(1, new Vector3F(1, 1, 1)));
                WE.Render.API.Draw(Mesh, TestProgram);
            }
        }else if(Asset is GLProgram Program){
            GLMesh? TestMesh = WE.Asset.Resolve<GLMesh>(WE.Asset.GetID("Mesh/Sphere"));
            if(TestMesh != null){
                Program.SetUniform(UniformValue.CreateM4F(0, CalculateModel(TestMesh)));
                Program.SetUniform(UniformValue.CreateV3F(1, new Vector3F(1, 1, 1)));
                WE.Render.API.Draw(TestMesh, Program);
            }
        }
    }
    
    private static void UpdateAssetPreview(object Asset, int ID){
        const int Size = 128;

        if(!__PreviewTextures.TryGetValue(ID, out GLTexture2D? Texture)){
            Texture = WE.Render.API.CreateTexture2D(new Vector2I(Size, Size));
            __PreviewTextures[ID] = Texture;
        }

        if(__SharedPreviewView == null){
            __SharedPreviewView = GLView.Create(WE.Render.API, new Vector2I(Size, Size),
                GLView.LayerConfig.Color(),
                GLView.LayerConfig.Depth()
            );
        }

        GLView OldView = WE.Render.API.Pool.GetView();
        WE.Render.API.Pool.SetView(__SharedPreviewView);

        __SharedPreviewView.SetTexture(Texture);
        
        WE.Render.API.FrameStart();
        
            WE.Render.API.Clear(Color4B.Transparent);

            bool OldDepthTest = WE.Render.API.Pool.GetDepthTest();
            bool OldScissor   = WE.Render.API.Pool.GetScissorTest();
            bool OldCullFace  = WE.Render.API.Pool.GetCullFace();
            
            WE.Render.API.Pool.SetDepthTest(true);
            WE.Render.API.Pool.SetScissorTest(false);
            WE.Render.API.Pool.SetCullFace(false);
            
            RenderAsset(Asset);
            
            WE.Render.API.Pool.SetDepthTest(OldDepthTest);
            WE.Render.API.Pool.SetScissorTest(OldScissor);
            WE.Render.API.Pool.SetCullFace(OldCullFace);
            
        WE.Render.API.FrameStop();
        
        WE.Render.API.Pool.SetView(OldView);
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
            UpdateAssetPreview(Asset, ID);

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
            string Label = Asset?.GetType().Name ?? "Null";
            Vector2 TextSize = ImGui.CalcTextSize(Label);
            DrawList.AddText(CursorPosition + (new Vector2(IconSize) - TextSize) * 0.5f, ImGui.GetColorU32(ImGuiCol.TextDisabled), Label);
        }
    }
}