using WEI.Editor;

namespace WEO;

public struct Asset<T> : WLI.Packable where T : class{
    [WEESave] public string Key;
    
    public int __ID = -1;

    public  bool       UseCache;
    private readonly T __Cache;

    public Asset(string Key){
        this.Key = Key;
        __ID = WE.Asset.GetID(Key);
        UseCache = false;
        __Cache = null!;
    }

    public Asset(int ID){
        Key = WE.Asset.GetKey(ID);
        __ID = ID;
        UseCache = false;
        __Cache = null!;
    }

    public Asset(T Cache){
        Key = "";
        UseCache = true;
        __Cache = Cache;
    }
    
    public T? Resolve(){
        if(UseCache){ return __Cache; }

        if(string.IsNullOrEmpty(Key)){ return WE.Asset.Resolve<T>(-1); }

        if(__ID == -1 /*|| WE.Asset.GetKey(__ID) != Key*/){ __ID = WE.Asset.GetID(Key); }
        
        return WE.Asset.Resolve<T>(__ID);
    }

    public Dictionary<string, object?> __Pack(){
        Dictionary<string, object?> Data = new Dictionary<string, object?>();
        if(UseCache){ Data["UseCache"] = true; }else{ Data["Key"] = Key ?? ""; }

        return Data;
    }
    
    public void __Unpack(Dictionary<string, object?> Data){
        if(WL.Packer.Get<bool>(Data, "UseCache", false)){
            UseCache = true;
        }else{
            UseCache = false;
            
            Key = WL.Packer.Get<string>(Data, "Key", "")!;
            if(string.IsNullOrEmpty(Key)){
                __ID = -1;
            }else{
                __ID = WE.Asset.GetID(Key);
            }
        }
    }

    public override string ToString() => $"Asset({(!UseCache ? (string.IsNullOrEmpty(Key) ? "Пустой" : $"\"{Key}\" ({__ID})") : (__Cache != null ? $"Связан с [{__Cache}]" : "Не связан"))})";
}