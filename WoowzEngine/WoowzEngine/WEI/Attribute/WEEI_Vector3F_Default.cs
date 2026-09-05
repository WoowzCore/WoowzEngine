using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Vector3F_Default : WEEI_InspectorProperty{
    public float Speed = 0.1f;
    
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        Vector3F Value = (Vector3F)Getter()!;
        Vector3 SystemValue = Value;
        if(ImGui.DragFloat3(Label, ref SystemValue, Speed)){ Setter((Vector3F)SystemValue); }
    }
}