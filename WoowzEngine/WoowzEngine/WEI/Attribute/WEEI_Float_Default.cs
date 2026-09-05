using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_Float_Default : WEEI_InspectorProperty{
    public float Speed = 0.1f;
    
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        float Value = (float)Getter()!;
        if(ImGui.DragFloat(Label, ref Value, Speed)){ Setter(Value); }
    }
}