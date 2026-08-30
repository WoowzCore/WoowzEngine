using System.Reflection;
using WEI.Editor;
using WEO;

namespace WEE;

public static class Registry{
    public static List<Type> AvailableComponents{ get; } = [];
    private static readonly HashSet<Assembly> ScannedAssemblies = [];

    private static readonly Dictionary<Type, List<MethodInfo>> __CachedMethods = [];
    
    public static void ScanAssembly(Assembly Assembly) {
        if(ScannedAssemblies.Contains(Assembly)){
            WL.Logger.Debug("Уже отсканированная сборка: " + Assembly.FullName);
            return;
        }
        
        WL.Logger.Debug("Сканирую сборку: " + Assembly.FullName);
        ScannedAssemblies.Add(Assembly);

        foreach(Type Type in Assembly.GetTypes()){
            if(typeof(WEI.Component).IsAssignableFrom(Type) && !Type.IsAbstract && Type.IsClass){
                if(!AvailableComponents.Contains(Type)){
                    AvailableComponents.Add(Type);
                    WL.Logger.Info($"Зарегистрирован компонент: {Type.FullName} из {Assembly.GetName().Name}");
                }
            }

            MethodInfo[] Methods = Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach(MethodInfo Method in Methods){
                IEnumerable<Attribute> Attributes = Method.GetCustomAttributes();
                foreach(Attribute Attribute in Attributes){
                    Type AttributeType = Attribute.GetType();

                    if(!__CachedMethods.TryGetValue(AttributeType, out List<MethodInfo>? List)){
                        List = [];
                        __CachedMethods[AttributeType] = List;
                    }
                    List.Add(Method);
                }
            }
        }
    }

    public static void RunMethods<T>(bool Notify, params object[] Args) where T : Attribute{
        if(Notify){ WL.Logger.Debug($"Поиск и выполнение методов с атрибутом [{typeof(T).Name}]..."); }

        Type TargetAttribute = typeof(T);
        
        if(!__CachedMethods.TryGetValue(TargetAttribute, out List<MethodInfo>? Methods)){ return; }

        foreach(MethodInfo Method in Methods){
            try{
                ParameterInfo[] Parameters = Method.GetParameters();
                if(Parameters.Length == Args.Length){
                    Method.Invoke(null, Args);
                }else{
                    WL.Logger.Warn($"Метод {Method.DeclaringType?.Name}.{Method.Name} имеет неверное количество параметров для [{TargetAttribute.Name}]");
                }
            }catch(Exception e){
                e = e.InnerException ?? e;
                WL.Logger.Error($"Ошибка в {Method.Name} [{TargetAttribute.Name}]:\n{e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static List<TD> GetDelegates<T, TD>() where TD : Delegate{
        List<TD> Result = [];
        if(__CachedMethods.TryGetValue(typeof(T), out List<MethodInfo>? Methods)){
            foreach(MethodInfo Method in Methods){
                try{
                    Result.Add((TD)Delegate.CreateDelegate(typeof(TD), Method));
                }catch{
                    WL.Logger.Error($"Не удалось создать делегат для {Method.Name}!");
                }
            }
        }
        return Result;
    }

    public static TD? GetFirstDelegate<T, TD>() where TD : Delegate{
        if(__CachedMethods.TryGetValue(typeof(T), out List<MethodInfo>? Methods) && Methods.Count > 0){
            try{
                return (TD)Delegate.CreateDelegate(typeof(TD), Methods[0]);
            }catch{
                WL.Logger.Error($"Не удалось создать делегат для {Methods[0].Name}!");
            }
        }
        return null;
    }

    public static object? RunFirstDelegate<T, TD>(bool Notify, params object[] Args) where TD : Delegate{
        TD? Delegate = GetFirstDelegate<T, TD>();
        if(Delegate != null){
            try{
                return Delegate.DynamicInvoke(Args);
            }catch(Exception e){
                e = e is TargetInvocationException ? (e.InnerException ?? e) : e;
                WL.Logger.Error($"Ошибка при выполнении делегата [{typeof(T).Name}]:\n{e.Message}\n{e.StackTrace}");
            }
        }else{
            WL.Logger.Warn($"Не найден делегат [{typeof(T).Name}] для вызова!");
        }

        return null;
    }
    
    public static void ResetAndReload(Assembly? GameAssembly = null){
        WL.Logger.Info("Обновление компонентов...");
        
        WE.Asset.Clear();
        
        ScannedAssemblies.Clear();
        AvailableComponents.Clear();
        __CachedMethods.Clear();
        
        foreach(Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies()){ ScanAssembly(Assembly); }

        if(GameAssembly != null){ ScanAssembly(GameAssembly); }
        
        RunMethods<WEERunOnInit>(true, WEE.Render.API);
    }
}