using System.Reflection;
using WEI_Attribute;
using WEO;
using WLO;

namespace WEI;

public abstract class Component : WLI.Packable{
    public Entity Owner{ get; internal set; } = null!;

    public static class Template{
        public static readonly Dictionary<Type, object> __Templates = [];
        public static object GetDefault(Type Type){
            if(!__Templates.TryGetValue(Type, out var Template)){
                Template = Activator.CreateInstance(Type)!;
                __Templates[Type] = Template;
            }

            return Template;
        }
    }
    
    [WE_Save][WEEI_Hide] public HashSet<string> OverridesValues = [];
    
    public virtual Dictionary<string, object?> __Pack(){
        Dictionary<string, object?> Data = new Dictionary<string, object?>();
        Type Type = GetType();

        foreach(string MemberName in OverridesValues){
            FieldInfo? Field = Type.GetField(MemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(Field != null){
                Data[MemberName] = Field.GetValue(this);
                continue;
            }

            PropertyInfo? Property = Type.GetProperty(MemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(Property != null){
                Data[MemberName] = Property.GetValue(this);
            }
        }
        
        return Data;
    }

    public virtual void __Unpack(Dictionary<string, object?> Data){
        Type Type = GetType();
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        OverridesValues.Clear();
        
        foreach(KeyValuePair<string, object?> KVP in Data){
            if(KVP.Key == "OverridesValues"){ continue; }

            FieldInfo? Field = Type.GetField(KVP.Key, Flags);
            if(Field != null && Field.GetCustomAttribute<WE_Save>() != null){
                Field.SetValue(this, WL.Packer.Unpack(KVP.Value, Field.FieldType));
                OverridesValues.Add(KVP.Key);
                continue;
            }
        
            PropertyInfo? Property = Type.GetProperty(KVP.Key, Flags);
            if(Property != null && Property.GetCustomAttribute<WE_Save>() != null){
                Property.SetValue(this, WL.Packer.Unpack(KVP.Value, Property.PropertyType));
                OverridesValues.Add(KVP.Key);
            }
        }
    }

    private bool __IsStarted = false;
    
    public void __FixedUpdate(DeltaTimeInfo DTI){
        if(!__IsStarted){ OnStart(); __IsStarted = true; }
        OnFixedUpdate(DTI);
    }
    
    public void __Update(DeltaTimeInfo DTI){
        if(!__IsStarted){ OnStart(); __IsStarted = true; }
        OnUpdate(DTI);
    }
    
    // ----------------------------------------------------------------------

    public virtual void OnAdd(){}

    public virtual void OnRemove(){}

    public virtual void OnStart(){}

    public virtual void OnFixedUpdate(DeltaTimeInfo DTI){}
    
    public virtual void OnUpdate(DeltaTimeInfo DTI){}
}