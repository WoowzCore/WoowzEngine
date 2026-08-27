using System.Reflection;
using WEI.Editor;

namespace WEE;

public static class Registry{
    public static List<Type> AvailableComponents{ get; } = [];
    private static readonly HashSet<Assembly> ScannedAssemblies = [];
    
    public static void ScanAssembly(Assembly Assembly) {
        if(ScannedAssemblies.Contains(Assembly)){
            WL.Logger.Debug("Уже отсканированная сборка: " + Assembly.FullName);
            return;
        }
        
        WL.Logger.Debug("Сканирую сборку: " + Assembly.FullName);
        ScannedAssemblies.Add(Assembly);
        
        IEnumerable<Type> ComponentTypes = Assembly.GetTypes().Where(T => typeof(WEI.Component).IsAssignableFrom(T) && T is{ IsAbstract: false, IsClass: true });

        foreach(Type Type in ComponentTypes){
            if(!AvailableComponents.Contains(Type)){
                AvailableComponents.Add(Type);
                WL.Logger.Info($"Зарегистрирован компонент: {Type.FullName} из {Assembly.GetName().Name}");
            }
        }
    }

    public static void RunMethods<T>(params object[] Args) where T : Attribute{
        WL.Logger.Debug($"Поиск и выполнение методов с атрибутом [{typeof(T).Name}]...");


        foreach(Assembly Assembly in ScannedAssemblies){
            foreach(Type Type in Assembly.GetTypes()){
                MethodInfo[] Methods = Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach(MethodInfo Method in Methods){
                    if(Method.GetCustomAttribute<T>() != null){
                        try{
                            ParameterInfo[] Parameters = Method.GetParameters();
                            
                            if(Parameters.Length == Args.Length){
                                WL.Logger.Debug($"Вызов [{typeof(T).Name}]: {Type.Name}.{Method.Name}");
                                Method.Invoke(null, Args);
                            }else{
                                WL.Logger.Warn($"todo, Метод {Type.Name}.{Method.Name} помечен [{typeof(T).Name}], но имеет параметры и не может быть вызван.");
                            }
                        }catch(Exception e){
                            e = e.InnerException ?? e;
                            WL.Logger.Error($"todo, Ошибка при выполнении [{typeof(T).Name}] в {Type.Name}.{Method.Name}: {e.Message} {e.StackTrace}");
                        }
                    }
                }
            }
        }
    }
    
    public static void ResetAndReload(Assembly? GameAssembly = null){
        WL.Logger.Info("Обновление компонентов...");
        
        WE.Asset.Clear();
        
        ScannedAssemblies.Clear();
        AvailableComponents.Clear();

        Assembly[] LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach(Assembly Assembly in LoadedAssemblies){
            ScanAssembly(Assembly);
        }

        if(GameAssembly != null){
            ScanAssembly(GameAssembly);
        }
        
        RunMethods<WEERunOnInit>();
    }
}