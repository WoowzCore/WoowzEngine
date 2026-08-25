using System.Reflection;
using WEI.Editor;
using WEO;

namespace WEI;

public abstract class Component : WLI.Packable{
    public Entity Owner{ get; internal set; } = null!;

    public virtual Dictionary<string, object?> __Pack(){
        Dictionary<string, object?> Data = new Dictionary<string, object?>();

        foreach(FieldInfo Field in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WEESave>() == null){ continue; }

            Data[Field.Name] = Field.GetValue(this);
        }
        
        return Data;
    }

    public virtual void __Unpack(Dictionary<string, object?> Data){
        foreach(FieldInfo Field in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WEESave>() == null){ continue; }

            if(Data.TryGetValue(Field.Name, out object? Value)){
                Field.SetValue(this, WL.Packer.Unpack(Value, Field.FieldType));
            }
        }
    }
}