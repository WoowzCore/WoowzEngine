using System.Runtime.InteropServices;
using WEE_Interface;
using WEEO;
using WLO;
using WLO.Math;
using WLO.Render;

namespace WEE;

public static class Render{
    public static void Start(){
        WE.Render.Start(WEE.Window.MainWindow.GetProcAddress, new WE.Render.StartParameters{
            DebugLogger = true,
            UseThisLogger = WL.Logger.CurrentLogger
        });
        
        RecreateSceneFrameBuffer(new Vector2I(800, 600));

        UB_Default = new UniformBlock<UniformBlock_Default>(WE.Render.API);
    }
    
    public static void Stop(){
        WE.Render.Stop();
    }
    
    // ----------------------------------------------------------------------

    public static GLView SceneView = null!;

    public static UniformBlock<UniformBlock_Default> UB_Default = null!;
    
    private static void RecreateSceneFrameBuffer(Vector2I Size){
        if(SceneView != null!){ SceneView.Destroy(); }
        SceneView = GLView.Create(WE.Render.API, Size, [
            GLView.LayerConfig.Color(),
            GLView.LayerConfig.Depth(true)
        ]);
    }
    
    public static void SceneRender(DeltaTimeInfo DTI){
        Vector2I TargetSize = I_View.SceneViewSize;

        if(TargetSize.X <= 0 || TargetSize.Y <= 0){ return; }

        if(TargetSize.X > 0 && TargetSize.Y > 0 && 
           (TargetSize.X != SceneView.TextureColor0!.Size.X || 
            TargetSize.Y != SceneView.TextureColor0!.Size.Y)){
            RecreateSceneFrameBuffer(TargetSize);
            
            WEE.Editor.ViewCamera.Aspect = TargetSize.Aspect;
        }
        
        WE.Render.API.Pool.SetView(SceneView);
        
        WE.Render.API.Pool.GetView().Viewport = TargetSize;
        
        WE.Render.API.FrameStart();
            WE.Render.API.Clear(I_View.BackgroundColor);
                
            WE.Render.API.Pool.SetDepthTest(true);
        
            WEE.Interface.CurrentScene?.Render(DTI, WEE.Editor.ViewCamera);
            
            WE.Render.Queue.Render();
            
        WE.Render.API.FrameStop();
    }
    
    public static void MainRender(DeltaTimeInfo DTI){
        try{
            UB_Default.Update(new UniformBlock_Default{
                ViewProjection = WEE.Editor.ViewCamera.GetProjectionMatrix() * WEE.Editor.ViewCamera.GetViewMatrix(),
                Time = WEE.Cycle.Render_Time
            });
            WE.Render.API.Pool.SetUniformBlock(UB_Default, 0);
            
            SceneRender(DTI);
            
            WE.Render.API.Pool.SetView(null);
            
            WE.Render.API.Pool.GetView().Viewport = WEE.Window.MainWindow.Size;
            
            WE.Render.API.FrameStart();
                WE.Render.API.Clear(new Color4B(50, 25, 25));
                
                WEE.Interface.Render();
                
            WE.Render.API.FrameStop();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном рендере!", e);
        }
    }
    
    // ----------------------------------------------------------------------
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UniformBlock_Default{
        public Matrix4F ViewProjection;
        public float    Time;
        
        private float __0; 
        private float __1;
        private float __2;
    }
}