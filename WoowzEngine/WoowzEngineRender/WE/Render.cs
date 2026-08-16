using WLI;
using WLO;
using WLO.Render.Hardware;

namespace WE;

public static class Render{
    public static bool IsStarted{ get; private set; }

    public static Logger? CurrentLogger = WL.Logger.CurrentLogger;

    public static uint LogType_Initialization = (uint)Logger.Type.Info;

    private static void Log(uint Type, object Message){
        CurrentLogger?.PrefixPush("WER");
        CurrentLogger?.Log(Type, Message);
        CurrentLogger?.PrefixPop();
    }
    
    public struct StartParameters{
        public Logger? UseThisLogger;
        public bool?   DebugLogger;
    }
    
    public static void Start(Func<string, IntPtr> ProcessLoader, StartParameters? Parameters = null){
        try{
            if(IsStarted){ throw new ExceptionWL("Рендер уже запущен!"); }

            StartParameters Parameters__ = new StartParameters{
                UseThisLogger = Parameters.HasValue && Parameters.Value.UseThisLogger != null ? Parameters.Value.UseThisLogger : WL.Logger.CurrentLogger,
                DebugLogger = Parameters?.DebugLogger
            };

            CurrentLogger = Parameters__.UseThisLogger;
            
            Log(LogType_Initialization, "Запуск WoowzEngineRender...");

            API = new OpenGL(ProcessLoader, new OpenGL.StartParameters{
                DebugLogger = Parameters__.DebugLogger
            }, true);
            
            Log(LogType_Initialization, "WoowzEngineRender запущен!");
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске WoowzEngineRender!", e);
        }
    }

    public static void Stop(){
        try{
            if(!IsStarted){ throw new ExceptionWL("Рендер не был даже запущен!"); }
            
            Log(LogType_Initialization, "Остановка WoowzEngineRender...");

            API.Stop();
            
            Log(LogType_Initialization, "WoowzEngineRender остановлен!");
            
            IsStarted = false;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при остановке WoowzEngineRender!", e);
        }
    }

    public static OpenGL API{ get; private set; } = null!;
}