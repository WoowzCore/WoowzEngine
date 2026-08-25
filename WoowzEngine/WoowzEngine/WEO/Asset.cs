using WEI.Editor;

namespace WEO;

public struct Asset<T> : WLI.Serializable where T : class{
    [WEESave] public string Key;
    
    public int __ID = -1;

    public  bool Linked;
    private T    __Cache;
    
    public Asset(string Key){
        this.Key = Key;
        __ID = WE.Asset.GetID(Key);
        Linked = true;
        __Cache = null!;
    }

    public Asset(int ID){
        Key = WE.Asset.GetKey(ID);
        __ID = ID;
        Linked = true;
        __Cache = null!;
    }

    public Asset(T NotLinked){
        Key = "";
        Linked = false;
        __Cache = NotLinked;
    }
    
    public T? Resolve(){
        if(!Linked){ return __Cache; }

        // le govno, тут надо добавить проверку ещё по ключу, но я боюсь что сожрёт оптимизацию
        if(__ID == -1 && !string.IsNullOrEmpty(Key)){ __ID = WE.Asset.GetID(Key); }
        return WE.Asset.Resolve<T>(__ID);
    }

    public Dictionary<string, object> Serialize(){
        Dictionary<string, object> Data = new Dictionary<string, object>();
        if(Linked){ Data["Key"] = Key; }
        else{ Data["NotLinked"] = true; }

        return Data;
    }
    
    public void Deserialize(Dictionary<string, object> Data){
        if(WL.Serializer.Get<bool>(Data, "NotLinked")){
            // пустота
        }else{
            Linked = true;
            Key = WL.Serializer.Get<string>(Data, "Key", "")!;
            if(!string.IsNullOrEmpty(Key)){ __ID = WE.Asset.GetID(Key); }
        }
    }

    public override string ToString() => $"Asset({(Linked ? (string.IsNullOrEmpty(Key) ? "Пустой" : $"\"{Key}\" ({__ID})") : (__Cache != null ? $"Связан с [{__Cache}]" : "Не связан"))})";
}