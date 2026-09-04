using System.Reflection;
using WEI_Attribute;
using WEO;
using WLO;

namespace WEI;

public abstract class Component : WLI.Packable{
    public Entity Owner{ get; internal set; } = null!;

    public virtual Dictionary<string, object?> __Pack(){
        Dictionary<string, object?> Data = new Dictionary<string, object?>();
        Type Type = GetType();
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WE_Save>() == null){ continue; }

            Data[Field.Name] = Field.GetValue(this);
        }
        
        foreach(PropertyInfo Property in Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Property.GetCustomAttribute<WE_Save>() == null || !Property.CanRead){ continue; }

            Data[Property.Name] = Property.GetValue(this);
        }
        
        return Data;
    }

    public virtual void __Unpack(Dictionary<string, object?> Data){
        Type Type = GetType();
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        foreach(KeyValuePair<string, object?> KVP in Data){
            FieldInfo? Field = Type.GetField(KVP.Key, Flags);
            if(Field != null){
                if(Field.GetCustomAttribute<WE_Save>() != null){
                    Field.SetValue(this, WL.Packer.Unpack(KVP.Value, Field.FieldType));
                }
                continue;
            }
        
            PropertyInfo? Property = Type.GetProperty(KVP.Key, Flags);
            if(Property != null){
                if(Property.GetCustomAttribute<WE_Save>() != null){
                    Property.SetValue(this, WL.Packer.Unpack(KVP.Value, Property.PropertyType));
                }
            }
        }
    }

    private bool __IsStarted = false;
    public void __Update(DeltaTimeInfo DTI){
        if(!__IsStarted){ OnStart(); __IsStarted = true; }
        OnUpdate(DTI);
    }
    
    // ----------------------------------------------------------------------

    public virtual void OnAdd(){}

    public virtual void OnRemove(){}

    public virtual void OnStart(){}

    public virtual void OnUpdate(DeltaTimeInfo DTI){}
}