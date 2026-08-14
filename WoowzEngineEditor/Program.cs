using WLO.Math;
using WLO.Render.Hardware;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

Vulkan Render = new WLO.Render.Hardware.Vulkan();
Render.Start();

while(!Window.IsClosed){
    Window.PollEvents();
    
    
}

Render.Stop();

Window.Close();
WL.GLFW.Stop();