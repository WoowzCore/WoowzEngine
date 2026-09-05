using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Float_Range : WEEI_InspectorProperty{
    public float Min, Max;
    public WEEI_Float_Range(float Min, float Max){ this.Min = Min; this.Max = Max; }

    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        float Value = (float)Getter()!;
        if(ImGui.SliderFloat(Label, ref Value, Min, Max)){ Setter(Value); }
    }
}