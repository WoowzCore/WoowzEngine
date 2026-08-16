using WE;
using WLI_Input;
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

uniform mat4 uViewProjection;

out vec4 vColor;

void main() {
    gl_Position = uViewProjection * vec4(aPos, 0.0, 1.0);
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
int Uniform_ViewProjection = Program.GetUniform("uViewProjection");

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


Vector3F Camera_Position = new Vector3F(0, 0, 3);
Vector3F Camera_Rotation = new Vector3F(0, 0, 0);

float Camera_Move_Speed = 3;
float Camera_Rotate_Speed = 2;

float Time = 0;
int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(120, ref DTI)){
        float DT = (float)DTI.Value.DT;
        
        i++;
        Time += DT;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;

        void CameraControlls(){
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.Up))    Camera_Rotation -= new Vector3F(Camera_Rotate_Speed * DT, 0, 0);
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.Down))  Camera_Rotation += new Vector3F(Camera_Rotate_Speed * DT, 0, 0);
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.Left))  Camera_Rotation -= new Vector3F(0, Camera_Rotate_Speed * DT, 0);
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.Right)) Camera_Rotation += new Vector3F(0, Camera_Rotate_Speed * DT, 0);
            
            float pitch = Camera_Rotation.X;
            float yaw   = Camera_Rotation.Y;
            
            Vector3F forward = new Vector3F(
                (float)Math.Sin(yaw) * (float)Math.Cos(pitch),
                -(float)Math.Sin(pitch),
                -(float)Math.Cos(yaw) * (float)Math.Cos(pitch)
            );
            
            Vector3F right = new Vector3F(
                (float)Math.Cos(yaw),
                0,
                (float)Math.Sin(yaw)
            );
            
            float speed = Camera_Move_Speed * DT;
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.W)) Camera_Position += forward * speed;
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.S)) Camera_Position -= forward * speed;
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.A)) Camera_Position -= right * speed;
            if(Window.Keyboard.IsKeyDown(Keyboard.Key.D)) Camera_Position += right * speed;
        }
        CameraControlls();
        
        WE.Render.API.CRenderView.Viewport = Window.Size;
        
        WE.Render.API.FrameStart();
        
        WE.Render.API.Clear(new Color4B(200, 200, 200));


        Matrix4F Projection = Matrix4F.CreatePerspective(1, Window.Aspect, 0.1f, 100f);
        
        Matrix4F View = Matrix4F.CreateRotationX(Camera_Rotation.X) *
                        Matrix4F.CreateRotationY(Camera_Rotation.Y) *
                        Matrix4F.CreateTranslation(-Camera_Position.X, -Camera_Position.Y, -Camera_Position.Z);
        

        Program.SetUniformM4F(Uniform_ViewProjection, Projection * View);

        Program.SetUniformF(Uniform_Time, Time);
        WE.Render.API.Draw(Triangle, Program);
        
        WE.Render.API.FrameStop();
        
        Window.SwapBuffers();
    }
}

WE.Render.Stop();

Window.Close();
WL.GLFW.Stop();    