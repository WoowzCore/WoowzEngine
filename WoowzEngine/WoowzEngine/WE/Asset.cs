using WEO;

namespace WE;

public struct Asset{
    private static readonly Dictionary<string, int>   __KeyToID   = [];
    private static readonly List<string>              __IDToKey   = [];
    private static readonly Dictionary<int, Provider> __Providers = [];
    
    private static readonly Dictionary<Type, List<string>> __TypeRegistry = [];
    private static readonly Dictionary<Type, List<string>> __ExplicitTypeRegistry = [];

    private static readonly Dictionary<Type, int> __Fallbacks = [];
    
    public static IReadOnlyList<string> AllKeys => __IDToKey;
    public static IEnumerable<Type> RegisteredTypes => __TypeRegistry.Keys;
    public static IEnumerable<Type> ExplicitTypes => __ExplicitTypeRegistry.Keys;
    
    public static void Clear(){
        __KeyToID.Clear();
        __IDToKey.Clear();
        __Providers.Clear();
        __TypeRegistry.Clear();
        __ExplicitTypeRegistry.Clear();
        __Fallbacks.Clear();
    }

    public static void SetNotFound<T>(string FallbackKey) where T : class => __Fallbacks[typeof(T)] = GetID(FallbackKey);
    
    public static IReadOnlyList<string> GetKeysForType(Type TargetType){
        if (__TypeRegistry.TryGetValue(TargetType, out List<string>? Keys)) {
            return Keys;
        }
        return [];
    }
    
    public static IReadOnlyList<string> GetKeysForExplicitType(Type TargetType){
        if (__ExplicitTypeRegistry.TryGetValue(TargetType, out List<string>? Keys)) {
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

            Type ExplicitType = typeof(T);
        

            if(!__ExplicitTypeRegistry.TryGetValue(ExplicitType, out List<string>? ExplicitList)){
                ExplicitList = [];
                __ExplicitTypeRegistry[ExplicitType] = ExplicitList;
            }
            ExplicitList.Add(Key);
            
            void AddKeyToType(Type Type, string Key){
                if(!__TypeRegistry.TryGetValue(Type, out List<string>? List)){
                    List = [];
                    __TypeRegistry[Type] = List;
                }

                if(!List.Contains(Key)){ List.Add(Key); }
            }
            
            Type? CurrentType = ExplicitType;
            while(CurrentType != null){
                AddKeyToType(CurrentType, Key);

                CurrentType = CurrentType.BaseType;
            }
            
            foreach(Type Interface in ExplicitType.GetInterfaces()){
                AddKeyToType(Interface, Key);
            }

        #endregion
        
        WL.Logger.Info($"Зарегистрирован ресурс: {Key} [{ID}]");
        
        return new Asset<T>(ID);
    }
    
    public static T? Resolve<T>(int ID) where T : class{
        if(ID >= 0 && __Providers.TryGetValue(ID, out Provider? Provider)){
            if(Provider.Get() is T Object){ return Object; }
        }

        if(__Fallbacks.TryGetValue(typeof(T), out int Fallback)){
            if(Fallback != -1 && __Providers.TryGetValue(Fallback, out Provider? FallbackProvider)){
                if(FallbackProvider.Get() is T FallbackObject){ return FallbackObject; }
            }

            WL.Logger.Warn($"todo, не указан fallback для ресурсов типа [{typeof(T).Name}]!");
        }
        
        return null;
    }

    public static void __Start(){
        WL.Loader.UpdateRegister(
            typeof(WLO.Loader.OBJ),
            typeof(WLO.Loader.FBX),
            typeof(WLO.Loader.PNG),
            typeof(WLO.Loader.JPG),
            typeof(WLO.Loader.BMP),
            typeof(WLO.Loader.TGA)
        );
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