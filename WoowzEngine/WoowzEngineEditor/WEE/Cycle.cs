using WEEO;
using WLO;

namespace WEE;

public static class Cycle{
    public static uint MaxEngineFPS = 30;
    public static uint MaxRenderFPS = 120;
    
    private static DeltaTimeInfo? __Engine_DTI;
    private static DeltaTimeInfo? __Render_DTI;
    public static void Start(){
        while(!WEE.Window.MainWindow.IsClosed){
            try{
                try{
                    SharedCycle();
                }catch(Exception e){
                    WL.Logger.Error("Ошибка в SHARED цикле!", e);   
                }
                
                if(WL.Thread.LimitByFPS(MaxEngineFPS, ref __Engine_DTI)){
                    Engine_DTI = __Engine_DTI!.Value;
                    try{
                        EngineCycle(Engine_DTI);
                    }catch(Exception e){
                        WL.Logger.Error("Ошибка в ENGINE цикле!", e);   
                    }
                }
            
                if(WL.Thread.LimitByFPS(MaxRenderFPS, ref __Render_DTI)){
                    Render_DTI = __Render_DTI!.Value;
                    try{
                        RenderCycle(Render_DTI);
                    }catch(Exception e){
                        WL.Logger.Error("Ошибка в RENDER цикле!", e);
                    }
                }
            }catch(Exception e){
                throw new ExceptionWEE("Ошибка в цикле!", e);
            }
        }
    }

    public static void SharedCycle(){
        WEE.Window.MainWindow.PollEvents();
    }
    
    public static void EngineCycle(DeltaTimeInfo DTI){
        WEE.Window.UpdateTitle();

        WEE.Main.Pipeline.Run("SceneUpdate", DTI, WEE.Interface.CurrentScene);
    }
    
    public static void RenderCycle(DeltaTimeInfo DTI){
        WEE.Control.Update();
        WEE.Editor.UpdateSceneView();
        
        WEE.Window.MainWindow.PollEvents2();

        Render_Time += Render_DTI.DT;
        
        WEE.Interface.Update();
        
        WEE.Render.MainRender(DTI);
        
        WEE.Window.MainWindow.SwapBuffers();
    }
    
    // ----------------------------------------------------------------------

    public static DeltaTimeInfo Render_DTI;
    public static DeltaTimeInfo Engine_DTI;
    
    public static double Render_Time;
}