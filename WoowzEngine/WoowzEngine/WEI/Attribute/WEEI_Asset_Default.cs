using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_Asset_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        object? Value = Getter();
        Type MemberType = Member is FieldInfo f ? f.FieldType : ((PropertyInfo)Member).PropertyType;
        Type AssetTargetType = MemberType.GetGenericArguments()[0];

        if(Value == null){ return; }

        string CurrentKey = (string)(MemberType.GetField("Key")!.GetValue(Value) ?? "");
        
        ImGui.BeginGroup();
            float TotalW = ImGui.CalcItemWidth();
            float FrameH = ImGui.GetFrameHeight();
            float ButtonW = ImGui.CalcTextSize("...").X + ImGui.GetStyle().FramePadding.X * 2;
            float Spacing = ImGui.GetStyle().ItemSpacing.X;
            float InputW = TotalW - ButtonW - Spacing;
            
            ImGui.SetNextItemWidth(InputW);
            string TempKey = CurrentKey;
            
            if(ImGui.InputText($"##in_{Label}", ref TempKey, 256)){
                Setter(MemberType.GetConstructor([typeof(string)])!.Invoke([TempKey]));
            }

            if(!string.IsNullOrEmpty(CurrentKey) && ImGui.BeginDragDropSource()){
                IntPtr Ptr = Marshal.StringToHGlobalAnsi(CurrentKey);
                ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)CurrentKey.Length + 1);
                Marshal.FreeHGlobal(Ptr);
                ImGui.Text($"Передать ассет: {CurrentKey}");
                ImGui.EndDragDropSource();
            }

            if(ImGui.BeginDragDropTarget()){
                unsafe{
                    ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ASSET_KEY");
                    if (Payload.NativePtr != null) {
                        string DroppedKey = Marshal.PtrToStringAnsi(Payload.Data)!;
                        if (WE.Asset.GetKeysForType(AssetTargetType).Contains(DroppedKey)) {
                            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                                Setter(MemberType.GetConstructor([typeof(string)])!.Invoke([DroppedKey]));
                            ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(new Vector4(0.2f, 1, 0.2f, 1)), 4.0f);
                        } else {
                            ImGui.SetTooltip($"Недопустимый тип! Ожидается: {AssetTargetType.Name}");
                            ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(new Vector4(1, 0.2f, 0.2f, 1)), 4);
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
            
            ImGui.SameLine(0, Spacing);
            if(ImGui.Button("...", new Vector2(ButtonW, FrameH))){ ImGui.OpenPopup("AssetPicker"); }
            
            ImGui.SameLine(0, Spacing);
            ImGui.Text(Label);
        ImGui.EndGroup();

        if(ImGui.IsItemHovered()){ ImGui.SetTooltip($"Тип ассета: {AssetTargetType.Name}\nПуть: {(string.IsNullOrEmpty(CurrentKey) ? "Не задан" : CurrentKey)}"); }

        if(ImGui.BeginPopup("AssetPicker")){
            foreach(string Key in WE.Asset.GetKeysForType(AssetTargetType).OrderBy(K => K)){
                if(ImGui.Selectable(Key, Key == CurrentKey)){
                    Setter(MemberType.GetConstructor([typeof(string)])!.Invoke([Key]));
                }
            }
            ImGui.EndPopup();
        }
    }
}