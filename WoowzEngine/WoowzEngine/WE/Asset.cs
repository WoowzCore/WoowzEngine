using WEO;
using WLI.GPU;

namespace WE;

public static class Asset{
    private static readonly Dictionary<string, int>   __KeyToID   = [];
    private static readonly List<string>              __IDToKey   = [];
    private static readonly Dictionary<int, Provider> __Providers = [];

    private static readonly Dictionary<Type, List<string>> __TypeRegistry = [];
    
    public static IReadOnlyList<string> AllKeys => __IDToKey;

    public static IEnumerable<Type> RegisteredTypes => __TypeRegistry.Keys;
    
    public static void Clear(){
        __KeyToID.Clear();
        __IDToKey.Clear();
        __Providers.Clear();
        __TypeRegistry.Clear();
    }

    public static IReadOnlyList<string> GetKeysForType(Type TargetType){
        if (__TypeRegistry.TryGetValue(TargetType, out List<string>? Keys)) {
            return Keys;
        }
        return [];
    }
    
    public static int GetID(string? Key){
        if(string.IsNullOrEmpty(Key)){ return -1; }

        if(__KeyToID.TryGetValue(Key, out int ID)){ return ID; }

        return -1;
    }

    public static string GetKey(int ID){
        if(ID >= 0 && ID < __IDToKey.Count){ return __IDToKey[ID]; }
        return string.Empty;
    }
    
    public static WEO.Asset<T> Register<T>(string Key, Func<T> Factory, bool IsDynamic = false) where T : class{
        if(string.IsNullOrEmpty(Key)){ throw new ExceptionWE("todo, ключ не может быть пустой"); }
        if(GetID(Key) != -1){ throw new ExceptionWE($"todo, такой id [{Key}] уже есть"); }

        int ID = __IDToKey.Count;
        __KeyToID[Key] = ID;
        __IDToKey.Add(Key);
        
        __Providers[ID] = new Provider(Factory, IsDynamic);

        #region Запись типа

            Type? CurrentType = typeof(T);
        
            void AddKeyToType(Type Type, string Key){
                if(!__TypeRegistry.TryGetValue(Type, out List<string>? List)){
                    List = [];
                    __TypeRegistry[Type] = List;
                }

                if(!List.Contains(Key)){ List.Add(Key); }
            }
            
            while(CurrentType != null){
                AddKeyToType(CurrentType, Key);

                CurrentType = CurrentType.BaseType;
            }

            CurrentType = typeof(T);
            
            foreach(Type Interface in CurrentType.GetInterfaces()){
                AddKeyToType(Interface, Key);
            }

        #endregion
        
        WL.Logger.Info($"Зарегистрирован ресурс: {Key} [{ID}]");
        
        return new Asset<T>(ID);
    }
    
    public static T? Resolve<T>(int ID) where T : class{
        if(__Providers.TryGetValue(ID, out Provider? Provider)){
            return Provider.Get() as T;
        }
        return null;
    }
    
    public class Provider{
        public  Func<object> Factory;
        public  bool         IsDynamic;
        private object?      __Cache;

        public Provider(Func<object> Factory, bool IsDynamic){
            this.Factory = Factory;
            this.IsDynamic = IsDynamic;
        }

        public bool ClearCache(){
            if(__Cache == null){ return false; }
            __Cache = null;
            return true;
        }

        public object Get(){
            if(IsDynamic){ return Factory(); }
            return __Cache ??= Factory();
        }
    }
}