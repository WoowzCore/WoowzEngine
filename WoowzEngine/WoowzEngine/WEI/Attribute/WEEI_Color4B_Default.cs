using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Interface;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Color4B_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter, ImGUI GUI){
        Color4B Value = (Color4B)Getter()!;

        Vector4 SystemValue = new Vector4(
            Value.R * WL.Math.Inverse255,
            Value.G * WL.Math.Inverse255,
            Value.B * WL.Math.Inverse255,
            Value.A * WL.Math.Inverse255
        );

        if(ImGui.ColorEdit4(Label, ref SystemValue)){
            Setter(new Color4B(
                (byte)(SystemValue.X * 255),
                (byte)(SystemValue.Y * 255),
                (byte)(SystemValue.Z * 255),
                (byte)(SystemValue.W * 255)
            ));
        }
    }
}