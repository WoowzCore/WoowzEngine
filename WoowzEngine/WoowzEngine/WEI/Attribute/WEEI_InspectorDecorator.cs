using System.Reflection;

namespace WEI_Attribute;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public abstract class WEEI_InspectorDecorator : Attribute{
    public abstract void Draw(string Label, object Target, MemberInfo Member);
}