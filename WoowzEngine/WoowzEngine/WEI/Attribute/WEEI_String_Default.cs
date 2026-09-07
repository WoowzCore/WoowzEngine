using System.Reflection;
using ImGuiNET;
using WLO.Interface;

namespace WEI_Attribute;

public class WEEI_String_Default : WEEI_InspectorProperty{
    public uint MaxLength = 512;
    
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter, ImGUI GUI){
        string Value = (string)(Getter() ?? "");
        if(ImGui.InputText(Label, ref Value, MaxLength)){ Setter(Value); }
        if(ImGui.IsItemHovered()){ ImGui.SetTooltip($"{Value.Length}/{MaxLength}"); }
    }
}