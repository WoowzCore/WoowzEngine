using WLO.Math;

namespace WEEO;

public class EditorConfig : WLI.Packable{
    public string Name        = "New Project";
    public string GameDLLPath = "";

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Name"       ] = Name,
        ["GameDLLPath"] = GameDLLPath
    };
    
    public void __Unpack(Dictionary<string, object?> Data){
        Name        = WL.Packer.Get<string>(Data, "Name", this.Name)!;
        GameDLLPath = WL.Packer.Get<string>(Data, "GameDLLPath", this.GameDLLPath)!;
    }
    
    // ----------------------------------------------------------------------

    public void Save(string Path){
        try{
            File.WriteAllText(Path, WL.String.ToJSON(WL.Packer.Pack(this)));
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при сохранении конфига TODO", e);
        }
    }

    public static EditorConfig Load(string Path){
        try{
            Dictionary<string, object?> Data = (Dictionary<string, object?>)WL.String.FromJSON(File.ReadAllText(Path))!;
            EditorConfig Config = new EditorConfig();
            WL.Packer.Unpack(Config, Data);
            return Config;
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при загрузке конфига TODO", e);
        }
    }
}