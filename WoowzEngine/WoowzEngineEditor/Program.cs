using WE;
using WLI_Render;
using WLI.GPU;
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

uniform float uTime;

void main() {
    float pulse = (sin(uTime * 3.0) + 1.0) / 2.0 * 0.8 + 0.2;
    
    FragColor = vec4(vColor.rgb * pulse, vColor.a);
}";


GLShader VShader = (GLShader)WE.Render.API.CreateShader(WLI.GPU.Shader.Type.Vertex  , VertexSource  );
GLShader FShader = (GLShader)WE.Render.API.CreateShader(WLI.GPU.Shader.Type.Fragment, FragmentSource);

GLProgram Program = (GLProgram)WE.Render.API.CreateProgram(VShader, FShader);
int Uniform_Time = Program.GetUniform("uTime");

var Layout = new VertexLayout(
    new VertexAttribute("aPos", 2, VertexAttribute.AttributeType.Float),
    new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.Byte, true)    
);

Vertex[] Vertices = [
    new Vertex(new Vector2F(-0.5f, -0.5f), new Color4B(255, 0, 0, 255)),
    new Vertex(new Vector2F(0.5f, -0.5f), new Color4B(0, 255, 0, 255)),
    new Vertex(new Vector2F(0.0f, 0.5f), new Color4B(0, 0, 255, 255))
];

GLMesh Triangle = (GLMesh)WE.Render.API.CreateMesh(Layout, Vertices);




float Time = 0;
int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(120, ref DTI)){
        i++;
        Time += (float)DTI.Value.DT;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
        
        WE.Render.API.CRenderView.Viewport = Window.Size;
        
        WE.Render.API.FrameStart();
        
        WE.Render.API.Clear(new Color4B(200, 200, 200));

        Program.SetUniformF(Uniform_Time, Time);
        WE.Render.API.Draw(Triangle, Program);
        
        WE.Render.API.FrameStop();
        
        Window.SwapBuffers();
    }
}

WE.Render.Stop();

Window.Close();
WL.GLFW.Stop();    