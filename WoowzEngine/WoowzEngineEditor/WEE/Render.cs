using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
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
    }
    
    public static void Stop(){
        API.Stop();
    }
    
    // ----------------------------------------------------------------------

    public static OpenGL API = null!;
    
    public static GLView SceneView   = null!;
    public static GLView PickingView = null!;
    
    private static void RecreateSceneFrameBuffer(Vector2I Size){
        if(SceneView   != null!){ SceneView  .Destroy(); }
        if(PickingView != null!){ PickingView.Destroy(); }

        SceneView = GLView.Create(API, Size, new PixelLayout(
            PixelAttribute.Color("Color", 4)
        ));
        
        PickingView = GLView.Create(API, Size, new PixelLayout(
            PixelAttribute.Color("Color", 4),
            PixelAttribute.Depth()
        ));
    }

    public static void ViewRender(
        DeltaTimeInfo DTI,
        Scene? Scene,
        Vector2I ViewSize,
        GLView? SceneView,
        GLView? PickingView,
        OpenGL Render,
        Camera Camera,
        Color4B BackgroundColor,
        double Time,
        string Effect
    ){
        Vector2I TargetSize = ViewSize;

        if(TargetSize.X <= 0 || TargetSize.Y <= 0){ return; }

        if(SceneView == null! || PickingView == null! || (TargetSize.X > 0 && TargetSize.Y > 0 && 
           (TargetSize.X != SceneView.TextureColor0!.Size.X || 
            TargetSize.Y != SceneView.TextureColor0!.Size.Y))){
            RecreateSceneFrameBuffer(TargetSize);
            
            Camera.Aspect = TargetSize.Aspect;
        }
        
        if(SceneView == null! || PickingView == null!){ return; }
        
        if(Scene != null){
            WEE.Registry.RunFirstDelegate<WEE_OnRenderView, Action<OpenGL, GLView, Scene, Camera, Vector2I, Color4B, DeltaTimeInfo, double, string?, bool>>(true,
                Render,
                SceneView,
                Scene,
                Camera,
                TargetSize,
                BackgroundColor,
                DTI,
                Time,
                Effect,
                false
            );
        }
        
        if(Scene != null){
            WEE.Registry.RunFirstDelegate<WEE_OnRenderView, Action<OpenGL, GLView, Scene, Camera, Vector2I, Color4B, DeltaTimeInfo, double, string?, bool>>(true,
                Render,
                PickingView,
                Scene,
                Camera,
                TargetSize,
                Color4B.Black,
                DTI,
                Time,
                Effect,
                true
            );
        }
    }
    
    public static void MainRender(DeltaTimeInfo DTI){
        try{
            WEE.Main.Pipeline.Run("SceneRender",
                DTI,
                WEE.Interface.CurrentScene,
                I_View.SceneViewSize,
                SceneView,
                PickingView,
                WEE.Render.API,
                WEE.Editor.ViewCamera,
                I_View.BackgroundColor,
                WEE.Cycle.Render_Time,
                I_View.SelectedEffect?.Asset
            );
            
            API.Pool.SetView(null);
            
            API.Pool.GetView().Viewport = WEE.Window.MainWindow.Size;
            
            API.Render(() => {
                API.Clear(new Color4B(50, 25, 25));
                
                WEE.Interface.Render();
            });
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном рендере!", e);
        }
    }
}