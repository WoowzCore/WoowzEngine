using System.Reflection;

namespace WEI_Attribute;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class WEEI_InspectorProperty : Attribute{
    public abstract void Draw(string Label, object Target, MemberInfo Member, Func<object?> Getter, Action<object?> Setter);
}