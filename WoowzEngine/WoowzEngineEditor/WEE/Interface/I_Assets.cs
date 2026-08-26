using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

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

                        float windowVisibleRightEdge = ImGui.GetCursorScreenPos().X + ImGui.GetContentRegionAvail().X;

                        for(int i = 0; i < Keys.Count; i++){
                            string Key = Keys[i];
                            int ID = WE.Asset.GetID(Key);

                            ImGui.PushID(Key);

                                ImGui.BeginGroup();
                                    Vector2 CursorPosition = ImGui.GetCursorScreenPos();
                                    ImGui.InvisibleButton("##btn", new Vector2(IconSize, IconSize));

                                    if(ImGui.BeginDragDropSource()){
                                        IntPtr Ptr = Marshal.StringToHGlobalAnsi(Key);
                                        ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)Key.Length + 1);
                                        Marshal.FreeHGlobal(Ptr);
                                        
                                        ImGui.Text($"Перетаскивание ассета: {Key}");
                                        ImGui.EndDragDropSource();
                                    }
                                    
                                    bool IsHovered = ImGui.IsItemHovered();
                                    bool IsActive = ImGui.IsItemActive();

                                    uint Column = ImGui.GetColorU32(IsActive ? ImGuiCol.HeaderActive : IsHovered ? ImGuiCol.HeaderHovered : ImGuiCol.FrameBg);

                                    ImGui.GetWindowDrawList().AddRectFilled(CursorPosition, new Vector2(CursorPosition.X + IconSize, CursorPosition.Y + IconSize), Column, 4.0f);

                                    WEE.Interface.RenderTextScrolling($"[{ID}] {Key}", IconSize, IsHovered);
                                ImGui.EndGroup();

                                float CurrentItemRightEdge = ImGui.GetItemRectMax().X;
                                float NextItemRightEdge = CurrentItemRightEdge + Padding + IconSize;

                                if(i + 1 < Keys.Count && NextItemRightEdge < windowVisibleRightEdge){
                                    ImGui.SameLine();
                                }

                            ImGui.PopID();
                        }

                        ImGui.Spacing();
                        
                    ImGui.PopID();
                }
            }
        } ImGui.End();
    }
}