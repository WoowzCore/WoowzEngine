using WEEO;
using WEI_Attribute;
using WEO;
using WLO;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;

namespace WEE;

public static class Main{
    public static void Start(string[] Args){
        try{
            WE.Engine.Start();

            Pipeline = new Pipeline();
            
            WEE.Window.Start();
            WEE.Control.Start();
            WEE.Render.Start();
            WEE.Interface.Start();
            
            WEE.Window.ConnectEvents();
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

    public static Pipeline Pipeline = null!;

    public static void RefreshPipeline(){
        Pipeline.Clear();

        // ----------------------------------------------------------------------
        
        // Вызывается при обновлении сцены
        Stage<Action<DeltaTimeInfo, Scene?>> S_SceneUpdate = Pipeline.GetOrCreate<Action<DeltaTimeInfo, Scene?>>("SceneUpdate");
        S_SceneUpdate.Add("Default", (DTI, Scene) => {
            if(Scene != null){
                Scene.UpdateEngine(DTI);
            }
        });
        
        // Вызывается при рендере сцены
        Stage<Action<DeltaTimeInfo, Scene?, Vector2I, GLView?, GLView?, OpenGL, Camera, Color4B, double, string>> S_SceneRender = Pipeline.GetOrCreate<Action<DeltaTimeInfo, Scene?, Vector2I, GLView?, GLView?, OpenGL, Camera, Color4B, double, string>>("SceneRender");
        S_SceneRender.Add("Default", (DTI, Scene, ViewSize, SceneView, PickingView, Render, Camera, BackgroundColor, Time, Effect) => {
            if(WEE.Registry.HasMethods<WEE_OnRenderView>()){ WEE.Render.ViewRender(DTI, Scene, ViewSize, SceneView, PickingView, Render, Camera, BackgroundColor, Time, Effect); }
        });
        
        // ----------------------------------------------------------------------
        
        if(WEE.Registry.HasMethods<WEE_OnPipeline>()){
            WEE.Registry.RunFirstDelegate<WEE_OnPipeline, Action<Pipeline>>(true, Pipeline);
        }
    }
}