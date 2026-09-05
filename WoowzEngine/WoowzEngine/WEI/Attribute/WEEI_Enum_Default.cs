using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_Enum_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        object? Value = Getter();
        if(Value == null){ return; }

        Type EnumType = Value.GetType();
        string[] Names = Enum.GetNames(EnumType);
        string CurrentName = Value.ToString()!;

        if(ImGui.BeginCombo(Label, CurrentName)){
            foreach(string Name in Names){
                bool IsSelected = CurrentName == Name;
                if(ImGui.Selectable(Name, IsSelected)){
                    Setter(Enum.Parse(EnumType, Name));
                }
                if(IsSelected){ ImGui.SetItemDefaultFocus(); }
            }
            ImGui.EndCombo();
        }
    }
}