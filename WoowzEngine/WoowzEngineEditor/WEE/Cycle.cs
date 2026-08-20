using WEEO;
using WLO;

namespace WEE;

public static class Cycle{
    public static void Start(){
        while(!WEE.Window.MainWindow.IsClosed){
            CycleFast();
        }
    }
    
    private static DeltaTimeInfo? __Render_DTI = null;
    private static DeltaTimeInfo? __Engine_DTI = null;
    public static void CycleFast(){
        try{
            WEE.Window.MainWindow.PollEvents();
            
            if(WL.Thread.LimitByFPS(30, ref __Engine_DTI)){
                Engine_DTI = __Engine_DTI!.Value;
                CycleEngine();
            }
            
            if(WL.Thread.LimitByFPS(120, ref __Render_DTI)){
                Render_DTI = __Render_DTI!.Value;
                CycleRender();
            }
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном цикле!", e);
        }
    }

    public static void CycleEngine(){
        try{
            
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в ENGINE цикле!", e);
        }
    }
    
    public static void CycleRender(){
        try{
            WEE.Editor.UpdateSceneView();
            
            WEE.Window.MainWindow.PollEvents2();

            Render_DT = (float)Render_DTI.DT;

            Render_Time += Render_DT;

            WEE.Window.MainWindow.Title = $"TEST WINDOW E-FPS: {Engine_DTI.FPS:F1}, R-FPS: {Render_DTI.FPS:F1}";
            
            WEE.Interface.Update();
            
            WEE.Render.MainRender();
            
            WEE.Window.MainWindow.SwapBuffers();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в RENDER цикле!", e);
        }
    }
    
    // ----------------------------------------------------------------------

    public static DeltaTimeInfo Render_DTI;
    public static DeltaTimeInfo Engine_DTI;
    
    public static float         Render_DT;
    
    public static float         Render_Time;
}