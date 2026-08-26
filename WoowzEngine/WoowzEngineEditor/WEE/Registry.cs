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
        
        RunInitMethods(Assembly);
    }

    private static void RunInitMethods(Assembly Assembly){
        foreach(Type Type in Assembly.GetTypes()){
            MethodInfo[] Methods = Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach(MethodInfo Method in Methods){
                if(Method.GetCustomAttribute<WEERunOnInit>() != null){
                    try{
                        if(Method.GetParameters().Length == 0){
                            WL.Logger.Debug($"Выполнение WEERunOnInit: {Type.Name}.{Method.Name}");
                            Method.Invoke(null, null);
                        }else{
                            WL.Logger.Warn($"todo, Метод {Type.Name}.{Method.Name} помечен [WEERunOnInit], но имеет параметры и не может быть вызван.");  
                        }
                    }catch(Exception e){
                        Exception ie = e.InnerException!;
                        WL.Logger.Error($"todo, Ошибка при выполнении [WEERunOnInit] в {Type.Name}.{Method.Name}: {e.Message} {e.StackTrace}\n{ie.Message} {ie.StackTrace}");   
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
    }
}