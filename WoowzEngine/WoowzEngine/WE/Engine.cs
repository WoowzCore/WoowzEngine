using WEO;
using WLO;

namespace WE;

public struct Engine{
    public static void Start(){
        try{
            if(__Started){ throw new Exception("WoowzEngine и так был запущен!"); } __Started = true;

            WL.Core.EngineInfo = new ProjectInfo("WoowzEngine", Author: "Woowz11", License: "Look at Repo (WIP)"); //todo
            
            WL.Packer.SetFallback(typeof(WEI.Component), typeof(WEO.UnknownComponent));
            
            WE.Asset .__Start();
            WE.Window.__Start();
        }catch(Exception e){
            Stop();
            throw new ExceptionWE("Произошла ошибка при запуске WoowzEngine!", e);
        }
    }
    private static bool __Started;

    public static void Stop(){
        try{
            __Started = false;
            
            WE.Window.__Stop();
        }catch(Exception e){
            throw new ExceptionWE("Произошла ошибка при остановке WoowzEngine!", e);
        }
    }
}