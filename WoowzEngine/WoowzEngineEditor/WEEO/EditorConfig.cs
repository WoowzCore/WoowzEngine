using WLO.Math;

namespace WEEO;

public class EditorConfig : WLI.Serializable{
    public string Name = "New Project";
    public string  GameDLLPath     = "";

    public Dictionary<string, object> Serialize() => new Dictionary<string, object>(){
        ["Name"] = Name,
        ["GameDLLPath"] = GameDLLPath
    };
    
    public void Deserialize(Dictionary<string, object> Data){
        if(Data.TryGetValue("Name", out object? V_Name__)){ Name = V_Name__.ToString()!; }
        
        if(Data.TryGetValue("GameDLLPath", out object? V_GameDLLPath__)){ GameDLLPath = V_GameDLLPath__.ToString()!; }
    }
    
    // ----------------------------------------------------------------------

    public void Save(string Path){
        try{
            File.WriteAllText(Path, WL.Serializer.ToJson(Serialize()));
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при сохранении конфига TODO", e);
        }
    }

    public static EditorConfig Load(string Path){
        try{
            Dictionary<string, object> Data = WL.Serializer.FromJson(File.ReadAllText(Path));
            EditorConfig Config = new EditorConfig();
            Config.Deserialize(Data);
            return Config;
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при загрузке конфига TODO", e);
        }
    }
}