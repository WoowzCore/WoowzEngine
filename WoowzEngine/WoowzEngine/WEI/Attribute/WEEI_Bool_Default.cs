using System.Reflection;
using ImGuiNET;
using WLO.Interface;

namespace WEI_Attribute;

public class WEEI_Bool_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter, ImGUI GUI){
        bool Value = (bool)Getter()!;
        if(ImGui.Checkbox(Label, ref Value)){ Setter(Value); }
    }
}