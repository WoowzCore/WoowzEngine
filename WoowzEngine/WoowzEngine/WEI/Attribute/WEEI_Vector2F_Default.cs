using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Vector2F_Default : WEEI_InspectorProperty{
    public float Speed = 0.1f;
    
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        Vector2F Value = (Vector2F)Getter()!;
        Vector2 SystemValue = Value;
        if(ImGui.DragFloat2(Label, ref SystemValue, Speed)){ Setter((Vector2F)SystemValue); }
    }
}