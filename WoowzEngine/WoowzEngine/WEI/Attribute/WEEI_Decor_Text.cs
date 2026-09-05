using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Math;

namespace WEI_Attribute;

public class WEEI_Decor_Text : WEEI_InspectorDecorator{
    public string  Text;

    public byte R, G, B, A;

    public WEEI_Decor_Text(string Text, byte R = 255, byte G = 255, byte B = 255, byte A = 255){ this.Text = Text; this.R = R; this.G = G; this.B = B; this.A = A; }

    public override void Draw(string Label, object Target, MemberInfo Member){
        // TODO
        const float Div = 1f / 255;
        
        ImGui.TextColored(new Vector4(R * Div, G * Div, B * Div, A * Div), Text);
    }
}