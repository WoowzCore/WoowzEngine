using System.Reflection;
using WEI.Editor;
using WEO;
using WLO;

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

    private bool __IsStarted = false;
    public void __Update(DeltaTimeInfo DTI){
        if(!__IsStarted){ OnStart(); __IsStarted = true; }
        OnUpdate(DTI);
    }
    
    // ----------------------------------------------------------------------

    public virtual void OnAdd(){}

    public virtual void OnDestroy(){}

    public virtual void OnStart(){}

    public virtual void OnUpdate(DeltaTimeInfo DTI){}
    
    public virtual void OnEngineUpdate(DeltaTimeInfo DTI){}
}