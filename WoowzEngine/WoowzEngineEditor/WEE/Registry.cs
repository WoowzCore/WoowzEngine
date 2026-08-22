using System.Reflection;

namespace WEE;

public static class Registry{
    public static List<Type> AvailableComponents{ get; } = [];
    
    public static void ScanAssembly(Assembly Assembly) {
        IEnumerable<Type> ComponentTypes = Assembly.GetTypes().Where(T => typeof(WEI.Component).IsAssignableFrom(T) && T is{ IsAbstract: false, IsClass: true });

        foreach(Type Type in ComponentTypes){
            if(!AvailableComponents.Contains(Type)){
                AvailableComponents.Add(Type);
                WL.Logger.Info($"Зарегистрирован компонент: {Type.FullName} из {Assembly.GetName().Name}");
            }
        }
    }

    public static void ResetAndReload(Assembly? GameAssembly = null){
        WL.Logger.Info("Обновление компонентов...");
        
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