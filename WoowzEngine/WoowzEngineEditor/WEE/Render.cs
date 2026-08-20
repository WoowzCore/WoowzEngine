using WEEO;
using WLO.Math;

namespace WEE;

public static class Render{
    public static void Start(){
        WE.Render.Start(WEE.Window.MainWindow.GetProcAddress, new WE.Render.StartParameters{
            DebugLogger = true,
            UseThisLogger = WL.Logger.CurrentLogger
        });
    }
    
    public static void Stop(){
        WE.Render.Stop();
    }

    public static void MainRender(){
        try{
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