using System.Numerics;
using System.Reflection;
using ImGuiNET;
using WLO.Interface;

namespace WEI_Attribute;

public class WEEI_Decor_Text : WEEI_InspectorDecorator{
    public string  Text;

    public byte R, G, B, A;

    public WEEI_Decor_Text(string Text, byte R = 255, byte G = 255, byte B = 255, byte A = 255){ this.Text = Text; this.R = R; this.G = G; this.B = B; this.A = A; }

    public override void Draw(string Label, object Target, MemberInfo Member, ImGUI GUI){
        ImGui.TextColored(new Vector4(R * WL.Math.Inverse255, G * WL.Math.Inverse255, B * WL.Math.Inverse255, A * WL.Math.Inverse255), Text);
    }
}