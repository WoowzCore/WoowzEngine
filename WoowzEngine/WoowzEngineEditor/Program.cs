using WLO;
using WLO.Math;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

WE.Render.Start();

int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(60, ref DTI)){
        i++;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
    }
}

WE.Render.Stop();

Window.Close();
WL.GLFW.Stop();    