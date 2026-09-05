using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_Decor_Separator : WEEI_InspectorDecorator{
    public override void Draw(string Label, object Target, MemberInfo Member){
        ImGui.Separator();
    }
}