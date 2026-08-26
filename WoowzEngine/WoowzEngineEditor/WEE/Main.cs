using WEEO;

namespace WEE;

public static class Main{
    public static void Start(string[] Args){
        try{
            WE.Engine.Start();
            
            WEE.Window.Start();
            WEE.Render.Start();
            WEE.Interface.Start();
            
            WEE.Window.ConnectEvents();
            WEE.Render.__CREATEDEFAULTS();
            WEE.Cycle.Start();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при запуске Main!", e);
        }
    }

    public static int Stop(){
        try{
            WEE.Interface.Stop();
            WEE.Render.Stop();
            WEE.Window.Stop();
            
            WE.Engine.Stop();

            return 0;
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при остановке Main!", e);
        }
    }
}