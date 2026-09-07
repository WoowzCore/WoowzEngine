using WEEO;
using WLO;

namespace WEE;

public static class Cycle{
    private static double Accumulator = 0;
    private static double PhysicStep;
    private static long   LastTicks;
    
    public static void Start(){
        LastTicks = System.Diagnostics.Stopwatch.GetTimestamp();

        DeltaTimeInfo? __Render_DTI = null;
        
        while(!WEE.D.Window.IsClosed){
            try{
                try{
                    SharedCycle();
                }catch(Exception e){
                    WL.Logger.Error("Ошибка в SHARED цикле!", e);   
                }

                PhysicStep = DeltaTimeInfo.FPSToDT(WEE.D.Time.Engine.MaxFPS);
                
                Accumulator += WL.Thread.GetRawDT(ref LastTicks);
                
                if(WL.Thread.NeedFixedUpdate(ref Accumulator, PhysicStep)){
                    WEE.D.Time.Engine.DTI = new DeltaTimeInfo(0, PhysicStep);
                    try{
                        EngineCycle();
                    }catch(Exception e){
                        WL.Logger.Error("Ошибка в ENGINE цикле!", e);   
                    }
                }

                WEE.D.Time.Render.EngineAlpha = (float)(Accumulator / PhysicStep);
                
                if(WL.Thread.LimitByFPS(WEE.D.Time.Render.MaxFPS, ref __Render_DTI)){
                    WEE.D.Time.Render.DTI = __Render_DTI!.Value;
                    try{
                        RenderCycle();
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
        WEE.D.Window.PollEvents();
    }
    
    public static void EngineCycle(){
        WEE.Window.UpdateTitle();

        WEE.Main.Pipeline.Run("SceneUpdate", WEE.D.Time.Engine.DTI, WEE.D.Selected.Scene);
    }
    
    public static void RenderCycle(){
        WEE.D.Time.Render.Elapsed += WEE.D.Time.Render.DT;
        
        WEE.Editor.UpdateCamera();
        
        WEE.Interface.Update();
        
        WEE.Render.MainRender(WEE.D.Time.Render.DTI);
        
        WEE.D.Window.SwapBuffers();
        
        WEE.D.Window.PollEvents2();
    }
}