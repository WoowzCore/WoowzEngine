using WLI;
using WLO;

namespace WE;

public static class Render{
    private const string Prefix = "WERender";

    public static bool IsStarted{ get; private set; }

    public static Logger? CurrentLogger = WL.Logger.CurrentLogger;

    public static uint LogType_Initialization = (uint)Logger.Type.Info;

    private static void Log(uint Type, object Message){
        CurrentLogger?.PrefixPush(Prefix);
        CurrentLogger?.Log(Type, Message);
        CurrentLogger?.PrefixPop();
    }
    
    public struct StartProperties{
        public Logger? UseThisLogger;
    }
    
    public static void Start(StartProperties? Properties = null){
        try{
            if(IsStarted){ throw new ExceptionWL("Рендер уже запущен!"); }

            StartProperties Properties__ = Properties ?? new StartProperties();

            CurrentLogger = Properties__.UseThisLogger ?? WL.Logger.CurrentLogger;
            
            Log(LogType_Initialization, $"Запуск WoowzEngineRender...\nПараметры: {Properties}");
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при запуске WoowzEngineRender!\nWE.Render.Start()", e);
        }
    }

    public static void Stop(){
        try{
            if(!IsStarted){ throw new ExceptionWL("Рендер не был даже запущен!"); }
            
            Log(LogType_Initialization, "Остановка WoowzEngineRender!");
            
            IsStarted = false;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при остановке WoowzEngineRender!\nWE.Render.Stop()", e);
        }
    }
}