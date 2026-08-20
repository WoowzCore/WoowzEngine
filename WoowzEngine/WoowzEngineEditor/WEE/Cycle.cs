using WEEO;
using WLO;

namespace WEE;

public static class Cycle{
    public static void Start(){
        while(!WEE.Window.MainWindow.IsClosed){
            CycleFast();
        }
    }
    
    private static DeltaTimeInfo? __DTI = null;
    public static void CycleFast(){
        try{
            WEE.Window.MainWindow.PollEvents();
            
            if(WL.Thread.LimitByFPS(120, ref __DTI)){
                DTI = __DTI!.Value;
                CycleFixed();
            }
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном цикле!", e);
        }
    }

    public static void CycleFixed(){
        try{
            WEE.Window.MainWindow.PollEvents2();

            DT = (float)DTI.DT;

            TotalFrames++;
            Time += DT;

            WEE.Window.MainWindow.Title = $"TEST WINDOW {TotalFrames} | FPS: {DTI.FPS}";
            
            WEE.Interface.Update();
            
            WEE.Render.MainRender();
            
            WEE.Window.MainWindow.SwapBuffers();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в зафиксированном цикле!", e);
        }
    }
    
    // ----------------------------------------------------------------------

    public static DeltaTimeInfo DTI;
    public static float         DT;
    
    public static int           TotalFrames;
    public static float         Time;
}