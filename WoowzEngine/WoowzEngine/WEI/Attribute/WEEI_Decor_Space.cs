using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Decor_Space : WEEI_InspectorDecorator{
    public float HeightPixels;

    public WEEI_Decor_Space(float HeightPixels = 8) => this.HeightPixels = HeightPixels;

    public override void Draw(string Label, object Target, MemberInfo Member){
        ImGui.Dummy(new Vector2(0, HeightPixels));
    }
}