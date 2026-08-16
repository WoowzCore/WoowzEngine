using WE;
using WLI_Render;
using WLO;
using WLO.GPU;
using WLO.Math;
using WoowzLib.Render.WLO;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

WE.Render.Start(Window.GetProcAddress, new Render.StartParameters{
    DebugLogger = true
});


// language=GLSL
string VertexSource = @"
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec4 aColor;

out vec4 vColor;

void main() {
    gl_Position = vec4(aPos, 0.0, 1.0);
    vColor = aColor;
}";

// language=GLSL
string FragmentSource = @"
#version 330 core
in vec4 vColor;
out vec4 FragColor;

void main() {
    FragColor = vColor;
}";


GLShader Shader = (GLShader)WE.Render.API.CreateShader(VertexSource, FragmentSource);

Vertex[] Vertices = [
    new Vertex(new Vector2F(-0.5f, -0.5f), new Color4B(255, 0, 0, 255)), // Красный (лево-низ)
    new Vertex(new Vector2F(0.5f, -0.5f), new Color4B(0, 255, 0, 255)),  // Зеленый (право-низ)
    new Vertex(new Vector2F(0.0f, 0.5f), new Color4B(0, 0, 255, 255))   // Синий (верх)
];

GLMesh Triangle = (GLMesh)WE.Render.API.CreateMesh(Vertices);





int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(6000, ref DTI)){
        i++;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
        
        WE.Render.API.FrameStart();

        WE.Render.API.CurrentRenderView.Viewport = Window.Size;
        
        RenderContext Context = WE.Render.API.CurrentRenderView.Context;
        
        Context.Clear(new Color4B((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255)));

        Context.CurrentShader = Shader;
        Triangle.Draw(Context);
        
        WE.Render.API.FrameStop();
        
        Window.SwapBuffers();
    }
}

WE.Render.Stop();

Window.Close();
WL.GLFW.Stop();    