using WLO;
using WLO.Math;
using WLO.Render.Hardware;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

Vulkan Render = new WLO.Render.Hardware.Vulkan();
FrameBuffer Frame = new FrameBuffer(Window.Size);
Render.Viewport = Window.Size;

Render.Start();

int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(60, ref DTI)){
        i++;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
        
        Render.FrameStart();
        
        Render.Clear(new Color4B((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256)));
        
        Render.FrameStop();
    
        Render.DrawFrameBuffer(Frame);
    
        Window.Present(Frame);
    }
}

Render.Stop();

Window.Close();
WL.GLFW.Stop();