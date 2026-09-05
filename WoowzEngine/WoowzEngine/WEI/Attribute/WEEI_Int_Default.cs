using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_Int_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        int Value = (int)Getter()!;
        if(ImGui.DragInt(Label, ref Value)){ Setter(Value); }
    }
}