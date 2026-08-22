using System.Reflection;
using WEI.Editor;
using WEO;

namespace WEI;

public abstract class Component : WLI.Serializable{
    public Entity Owner{ get; internal set; } = null!;

    public virtual Dictionary<string, object> Serialize(){
        Dictionary<string, object> Data = new Dictionary<string, object>(){
            [WL.Serializer.__Type] = GetType().AssemblyQualifiedName ?? GetType().FullName ?? "Unknown"
        };

        foreach(FieldInfo Field in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WEESave>() == null){ continue; }

            object? Value = Field.GetValue(this);
            if(Value != null){
                Data[Field.Name] = WL.Serializer.Serialize(Value);
            }
        }
        
        return Data;
    }

    public virtual void Deserialize(Dictionary<string, object> Data){
        foreach(FieldInfo Field in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WEESave>() == null){ continue; }
            
            if(Data.TryGetValue(Field.Name, out object? Value) && Value != null!){
                object? FinalValue = null;

                if(Value is Dictionary<string, object> Dictionary){
                    FinalValue = WL.Serializer.Deserialize(Dictionary, Field.FieldType);
                }else{
                    FinalValue = Value;
                }

                if(FinalValue != null){
                    try{
                        Type TargetType = Field.FieldType;
                        Type ActualType = Nullable.GetUnderlyingType(TargetType) ?? TargetType;

                        if(ActualType.IsInstanceOfType(FinalValue)){
                            Field.SetValue(this, FinalValue);
                        }else if(ActualType.IsEnum){
                            Field.SetValue(this, Enum.Parse(ActualType, FinalValue.ToString()!));   
                        }else{
                            Field.SetValue(this, Convert.ChangeType(FinalValue, ActualType));
                        }
                    }catch(Exception e){
                        WL.Logger.Error($"todo, ошибка привязки поля {Field.Name}: {e.Message + "\n" + e.StackTrace}");
                    }
                }
            }
        }
    }
}