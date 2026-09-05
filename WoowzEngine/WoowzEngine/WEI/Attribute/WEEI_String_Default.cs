using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_String_Default : WEEI_InspectorProperty{
    public uint MaxLength = 512;
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        string Value = (string)(Getter() ?? "");
        if(ImGui.InputText(Label, ref Value, MaxLength)){ Setter(Value); }
    }
}