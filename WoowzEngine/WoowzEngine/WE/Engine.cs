using WEO;

namespace WE;

public struct Engine{
    public static void Start(){
        try{
            WL.Packer.SetFallback(typeof(WEI.Component), typeof(WEO.UnknownComponent));
            
            WE.Asset.__Start();
            WE.Window.__Start();
        }catch(Exception e){
            Stop();
            throw new ExceptionWE("Произошла ошибка при запуске WoowzEngine!", e);
        }
    }

    public static void Stop(){
        try{
            WE.Window.__Stop();
        }catch(Exception e){
            throw new ExceptionWE("Произошла ошибка при остановке WoowzEngine!", e);
        }
    }
}