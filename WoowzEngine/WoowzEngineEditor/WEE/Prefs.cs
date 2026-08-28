namespace WEE;

public static class Prefs{
    public static string       LastConfigPath = "";
    public static List<string> RecentScenes   = [];
    private const int          MaxRecentCount = 10;

    private static string PrefsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".wee_prefs");

    public static Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["LastConfigPath"] = LastConfigPath,
        ["RecentScenes"  ] = RecentScenes
    };

    public static void __Unpack(Dictionary<string, object?> Data){
        LastConfigPath = WL.Packer.Get(Data, "LastConfigPath", LastConfigPath)!;
        RecentScenes   = WL.Packer.Get<List<string>>(Data, "RecentScenes", [])!;
    }

    public static void AddRecentScene(string Path){
        if(string.IsNullOrEmpty(Path)){ return; }
        RecentScenes.Remove(Path);
        RecentScenes.Insert(0, Path);
        if(RecentScenes.Count > MaxRecentCount){ RecentScenes.RemoveAt(RecentScenes.Count - 1); }
        Save();
    }

    public static void Save(){
        try{
            File.WriteAllText(PrefsPath, WL.String.ToJSON(__Pack()));
        }catch(Exception e){
            WL.Logger.Warn("ERROR TODO... " + e.Message);
        }
    }

    public static void Load(){
        if(!File.Exists(PrefsPath)){ return; }
        try{
            __Unpack((Dictionary<string, object?>)WL.String.FromJSON(File.ReadAllText(PrefsPath))!);
        }catch(Exception e){
            WL.Logger.Warn("ERROR TODO 2... " + e.Message);
        }
    }
}