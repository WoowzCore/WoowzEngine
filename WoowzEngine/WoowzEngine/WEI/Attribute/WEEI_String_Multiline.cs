using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace WEI_Attribute;

public class WEEI_String_Multiline : WEEI_InspectorProperty{
    public uint MaxLength = 4096;
    public int  Height;

    public WEEI_String_Multiline(int Height = 100){ this.Height = Height; }

    public override void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter){
        string Value = (string)(Getter() ?? "");
        ImGui.Text(Label);
        if(ImGui.InputTextMultiline($"##{Label}", ref Value, MaxLength, new Vector2(-1, Height))){ Setter(Value); }
    }
}