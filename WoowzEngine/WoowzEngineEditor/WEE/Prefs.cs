namespace WEE;

public static class Prefs{
    public static string       LastConfigPath = "";
    public static List<string> RecentScenes   = [];
    private const int          MaxRecentCount = 10;

    private static string PrefsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".weeprefs");

    public static Dictionary<string, object> Serialize() => new Dictionary<string, object>(){
        ["LastConfigPath"] = LastConfigPath,
        ["RecentScenes"] = RecentScenes.Cast<object>().ToList()
    };

    public static void Deserialize(Dictionary<string, object> Data){
        if(Data.TryGetValue("LastConfigPath", out object? V_LastConfigPath__)){ LastConfigPath = V_LastConfigPath__.ToString()!; }
        if(Data.TryGetValue("RecentScenes", out object? V_RecentScenes__) && V_RecentScenes__ is List<object> V_RecentScenes) {
            RecentScenes = V_RecentScenes.Select(x => x.ToString()!).ToList();
        }
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
            File.WriteAllText(PrefsPath, WL.Serializer.ToJson(Serialize()));   
        }catch(Exception e){
            WL.Logger.Warn("ERROR TODO... " + e.Message);
        }
    }

    public static void Load(){
        if(!File.Exists(PrefsPath)){ return; }
        try{
            Deserialize(WL.Serializer.FromJson(File.ReadAllText(PrefsPath)));
        }catch(Exception e){
            WL.Logger.Warn("ERROR TODO 2... " + e.Message);
        }
    }
}