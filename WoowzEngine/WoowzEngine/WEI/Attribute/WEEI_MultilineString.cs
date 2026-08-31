namespace WEI_Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class WEEI_MultilineString : Attribute{
    public int Height;
    public WEEI_MultilineString(int Height = 100){ this.Height = Height; }
}