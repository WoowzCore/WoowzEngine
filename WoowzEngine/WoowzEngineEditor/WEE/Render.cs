using System.Runtime.InteropServices;
using WEE_Interface;
using WEEO;
using WEI_Attribute;
using WEO;
using WLO;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;

namespace WEE;

public static class Render{
    public static void Start(){
        API = new OpenGL(WEE.Window.MainWindow.GetProcAddress, new OpenGL.StartParameters{
            DebugLogger = true,
            UseThisLogger = WL.Logger.CurrentLogger
        }, true);
        
        RecreateSceneFrameBuffer(new Vector2I(800, 600));
    }
    
    public static void Stop(){
        API.Stop();
    }
    
    // ----------------------------------------------------------------------

    public static OpenGL API = null!;
    
    public static GLView SceneView   = null!;
    public static GLView PickingView = null!;
    
    private static void RecreateSceneFrameBuffer(Vector2I Size){
        if(SceneView != null!){ SceneView.Destroy(); }
        SceneView = GLView.Create(API, Size, [
            GLView.LayerConfig.Color(),
            GLView.LayerConfig.Depth(true)
        ]);
        
        if(PickingView != null!){ PickingView.Destroy(); }
        PickingView = GLView.Create(API, Size, [
            GLView.LayerConfig.Color(),
            GLView.LayerConfig.Depth()
        ]);
    }

    public static void ViewRender(DeltaTimeInfo DTI){
        Vector2I TargetSize = I_View.SceneViewSize;

        if(TargetSize.X <= 0 || TargetSize.Y <= 0){ return; }

        if(TargetSize.X > 0 && TargetSize.Y > 0 && 
           (TargetSize.X != SceneView.TextureColor0!.Size.X || 
            TargetSize.Y != SceneView.TextureColor0!.Size.Y)){
            RecreateSceneFrameBuffer(TargetSize);
            
            WEE.Editor.ViewCamera.Aspect = TargetSize.Aspect;
        }
        
        API.Pool.SetView(SceneView);
        
        API.Pool.GetView().Viewport = TargetSize;
        
        API.FrameStart();
            if(WEE.Interface.CurrentScene != null){
                WEE.Registry.RunFirstDelegate<WEE_OnViewRender, Action<OpenGL, Scene, Camera, Vector2I, Color4B, DeltaTimeInfo, float, bool>>(true,
                    WEE.Render.API,
                    WEE.Interface.CurrentScene,
                    WEE.Editor.ViewCamera,
                    TargetSize,
                    I_View.BackgroundColor,
                    DTI,
                    WEE.Cycle.Render_Time,
                    false
                );
            }
        API.FrameStop();
        
        
        API.Pool.SetView(PickingView);
        
        API.Pool.GetView().Viewport = TargetSize;
        
        API.FrameStart();
            if(WEE.Interface.CurrentScene != null){
                WEE.Registry.RunFirstDelegate<WEE_OnViewRender, Action<OpenGL, Scene, Camera, Vector2I, Color4B, DeltaTimeInfo, float, bool>>(true,
                    WEE.Render.API,
                    WEE.Interface.CurrentScene,
                    WEE.Editor.ViewCamera,
                    TargetSize,
                    Color4B.Black,
                    DTI,
                    WEE.Cycle.Render_Time,
                    true
                );
            }
        API.FrameStop();
    }
    
    public static void MainRender(DeltaTimeInfo DTI){
        try{
            if(WEE.Registry.HasMethods<WEE_OnViewRender>()){ ViewRender(DTI); }
            
            API.Pool.SetView(null);
            
            API.Pool.GetView().Viewport = WEE.Window.MainWindow.Size;
            
            API.FrameStart();
                API.Clear(new Color4B(50, 25, 25));
                
                WEE.Interface.Render();
                
            API.FrameStop();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном рендере!", e);
        }
    }
}