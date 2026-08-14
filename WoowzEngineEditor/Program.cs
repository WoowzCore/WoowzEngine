using WLO.Math;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

while(!Window.IsClosed){
    Window.PollEvents();
    
    
}

Window.Close();
WL.GLFW.Stop();