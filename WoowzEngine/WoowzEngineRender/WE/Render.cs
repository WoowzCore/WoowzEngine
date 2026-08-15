using WLI;
using WLO;
using WLO.Render.Hardware;

namespace WE;

public static class Render{
    private const string Prefix = "WER";

    public static bool IsStarted{ get; private set; }

    public static Logger? CurrentLogger = WL.Logger.CurrentLogger;

    public static uint LogType_Initialization = (uint)Logger.Type.Info;

    private static void Log(uint Type, object Message){
        CurrentLogger?.PrefixPush(Prefix);
        CurrentLogger?.Log(Type, Message);
        CurrentLogger?.PrefixPop();
    }
    
    public struct StartProperties{
        public Logger?              UseThisLogger; // todo, remake, look vulkan reference
        public Func<string, IntPtr> ProcLoader;
    }
    
    public static void Start(StartProperties? Properties = null){
        try{
            if(IsStarted){ throw new ExceptionWL("Рендер уже запущен!"); }

            StartProperties Properties__ = Properties ?? new StartProperties();

            CurrentLogger = Properties__.UseThisLogger ?? WL.Logger.CurrentLogger;
            
            Log(LogType_Initialization, $"Запуск WoowzEngineRender...\nПараметры: {Properties}");

            __API = new OpenGL(Properties__.ProcLoader!);
            
            __API.Start();
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске WoowzEngineRender!", e);
        }
    }

    public static void Stop(){
        try{
            if(!IsStarted){ throw new ExceptionWL("Рендер не был даже запущен!"); }
            
            Log(LogType_Initialization, "Остановка WoowzEngineRender...");

            __API.Stop();
            
            IsStarted = false;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при остановке WoowzEngineRender!", e);
        }
    }

    public  static OpenGL API => __API;
    private static OpenGL        __API;
}