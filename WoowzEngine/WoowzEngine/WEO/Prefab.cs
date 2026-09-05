namespace WEO;

public class Prefab : WLI.Packable{
    public Dictionary<string, object?> EntityData = [];
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["EntityData"] = EntityData
    };
    public void __Unpack(Dictionary<string, object?> Data){
        EntityData = WL.Packer.Get(Data, "EntityData", new Dictionary<string, object?>(), Raw: true)!;
    }
    
    // ----------------------------------------------------------------------

    public static Prefab FromEntity(Entity Entity){
        Dictionary<string, object?> Data = WL.Packer.Pack(Entity) as Dictionary<string, object?> ?? [];

        Data.Remove("Transform");
        
        void CleanData(Dictionary<string, object?> Data){
            Data.Remove("ID");
            Data.Remove("SourcePrefab");

            if(Data.TryGetValue("Hierarchy", out object? Hierarchy__) && Hierarchy__ is Dictionary<string, object?> Hierarchy){
                if(Hierarchy.TryGetValue("Children", out object? Children__) && Children__ is List<object> Children){
                    foreach(object Child__ in Children){
                        if(Child__ is Dictionary<string, object?> Child){
                            CleanData(Child);
                        }
                    }
                }
            }
        }
        CleanData(Data);

        return new Prefab{ EntityData = Data };
    }

    public string ToJSON() => WL.String.ToJSON(WL.Packer.Pack(this));
    
    public static Prefab FromJSON(string JSON){
        Dictionary<string, object?>? Data = WL.String.FromJSON(JSON) as Dictionary<string, object?>;
        
        Prefab Result = new Prefab();
        if(Data != null){
            WL.Packer.Unpack(Result, Data);
        }
        
        return Result;
    }
}