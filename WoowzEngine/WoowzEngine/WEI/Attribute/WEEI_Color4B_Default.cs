using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Color4B_Default : WEEI_InspectorProperty{
    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        Color4B Value = (Color4B)Getter()!;

        // TODO
        const float Div = 1f / 255;
        
        Vector4 SystemValue = new Vector4(
            Value.R * Div,
            Value.G * Div,
            Value.B * Div,
            Value.A * Div
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