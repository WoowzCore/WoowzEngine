namespace WEI.Editor;

[AttributeUsage(AttributeTargets.Field)]
public class WEEMultilineString : Attribute{
    public int Height;
    public WEEMultilineString(int Height = 100){ this.Height = Height; }
}