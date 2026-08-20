using WEEO;
using WLO.Math;
using WLO.Render;

namespace WEE;

public static class Render{
    public static void Start(){
        WE.Render.Start(WEE.Window.MainWindow.GetProcAddress, new WE.Render.StartParameters{
            DebugLogger = true,
            UseThisLogger = WL.Logger.CurrentLogger
        });
        
        SceneFramebuffer = GLRenderView.Create(WE.Render.API, new Vector2I(800, 600));
    }
    
    public static void Stop(){
        WE.Render.Stop();
    }

    // ----------------------------------------------------------------------

    public static GLRenderView SceneFramebuffer = null!;

    public static void SceneRender(){
        WE.Render.API.CRenderView = SceneFramebuffer;
        
        WE.Render.API.CRenderView.Viewport = WEE.Interface.SceneViewport;
        
        WE.Render.API.FrameStart();
            
            WE.Render.API.Clear(new Color4B((byte)Random.Shared.Next(0, 255), (byte)Random.Shared.Next(0, 255), (byte)Random.Shared.Next(0, 255)));
                
        WE.Render.API.FrameStop();
    }
    
    public static void MainRender(){
        try{
            SceneRender();
            
            WE.Render.API.CRenderView = null!;
            
            WE.Render.API.CRenderView.Viewport = WEE.Window.MainWindow.Size;
            
            WE.Render.API.FrameStart();
            
                WE.Render.API.Clear(new Color4B(200, 200, 200));
            
                WEE.Interface.Render();
                
            WE.Render.API.FrameStop();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном рендере!", e);
        }
    }
}