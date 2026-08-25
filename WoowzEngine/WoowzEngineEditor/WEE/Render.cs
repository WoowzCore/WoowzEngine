using WEE_Interface;
using WEEO;
using WEO_Component;
using WEO;
using WLI_Render;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using WLO.Render;
using WoowzLib.Render.WLO;

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
        Vector2I TargetSize = I_View.SceneViewSize;

        if(TargetSize.X <= 0 || TargetSize.Y <= 0){ return; }

        if(TargetSize.X > 0 && TargetSize.Y > 0 && 
           (TargetSize.X != SceneFramebuffer.ResultTexture!.Size.X || 
            TargetSize.Y != SceneFramebuffer.ResultTexture!.Size.Y)){
            SceneFramebuffer.Destroy();
            SceneFramebuffer = GLRenderView.Create(WE.Render.API, TargetSize);
            
            WEE.Editor.SceneViewCamera.Aspect = TargetSize.Aspect;
        }
        
        WE.Render.API.CRenderView = SceneFramebuffer;
        
        WE.Render.API.CRenderView.Viewport = TargetSize;
        
        WE.Render.API.FrameStart();
            
            WE.Render.API.Clear(I_View.BackgroundColor);
                
            __RENDERTESTRENDER();
            
        WE.Render.API.FrameStop();
    }
    
    public static void MainRender(){
        try{
            SceneRender();
            
            WE.Render.API.CRenderView = null!;
            
            WE.Render.API.CRenderView.Viewport = WEE.Window.MainWindow.Size;
            
            WE.Render.API.FrameStart();
            
                WE.Render.API.Clear(new Color4B(50, 25, 25));
                
                WEE.Interface.Render();
                
            WE.Render.API.FrameStop();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в главном рендере!", e);
        }
    }
    
    // ----------------------------------------------------------------------
    // TODO, TEST RENDER
    
    public static GLProgram __PROGRAM;
    public static GLMesh    __MESH_TRIANGLE;
    public static GLMesh    __MESH_CUBE;

    private static int __UNIFORM_VPROJ;
    private static int __UNIFORM_MPROJ;
    private static int __UNIFORM_COLOR;
    
    public static void __STARTTESTRENDER(){
        __PROGRAM = (GLProgram)WE.Render.API.CreateProgram(
            // language=GLSL
            WE.Render.API.CreateShader(Shader.Type.Vertex, @"
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;

uniform mat4 uViewProjection;
uniform mat4 uModelProjection;

out vec4 vColor;

void main() {
    gl_Position = uViewProjection * uModelProjection * vec4(aPos, 1.0);
    vColor = aColor;
}"),
            // language=GLSL
            WE.Render.API.CreateShader(Shader.Type.Fragment, @"
#version 330 core
in vec4 vColor;
out vec4 FragColor;

uniform vec3 uColor;

void main() {
    FragColor = vec4(uColor.rgb, vColor.a);
}")
        );

        __UNIFORM_VPROJ = __PROGRAM.GetUniform("uViewProjection");
        __UNIFORM_MPROJ = __PROGRAM.GetUniform("uModelProjection");
        __UNIFORM_COLOR = __PROGRAM.GetUniform("uColor");
        
        __MESH_TRIANGLE = (GLMesh)WE.Render.API.CreateMesh(
            new VertexLayout(
                new VertexAttribute("aPos", 3, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.Byte, true)    
            ), [
                new Vertex(new Vector2F(-0.5f, -0.5f), new Color4B(255, 255, 255, 255)),
                new Vertex(new Vector2F(0.5f, -0.5f), new Color4B(255, 255, 255, 255)),
                new Vertex(new Vector2F(0.0f, 0.5f), new Color4B(255, 255, 255, 255))
            ]
        );

        Vertex[] GetCubeVertices(){
            Color4B w = new Color4B(255, 255, 255, 255);
            float s = 0.5f; // Половина размера (от -0.5 до 0.5)

            return [
                // Front face (Z+)
                new Vertex(new Vector3F(-s, -s,  s), w), new Vertex(new Vector3F( s, -s,  s), w), new Vertex(new Vector3F( s,  s,  s), w),
                new Vertex(new Vector3F( s,  s,  s), w), new Vertex(new Vector3F(-s,  s,  s), w), new Vertex(new Vector3F(-s, -s,  s), w),

                // Back face (Z-)
                new Vertex(new Vector3F(-s, -s, -s), w), new Vertex(new Vector3F(-s,  s, -s), w), new Vertex(new Vector3F( s,  s, -s), w),
                new Vertex(new Vector3F( s,  s, -s), w), new Vertex(new Vector3F( s, -s, -s), w), new Vertex(new Vector3F(-s, -s, -s), w),

                // Left face (X-)
                new Vertex(new Vector3F(-s,  s,  s), w), new Vertex(new Vector3F(-s,  s, -s), w), new Vertex(new Vector3F(-s, -s, -s), w),
                new Vertex(new Vector3F(-s, -s, -s), w), new Vertex(new Vector3F(-s, -s,  s), w), new Vertex(new Vector3F(-s,  s,  s), w),

                // Right face (X+)
                new Vertex(new Vector3F( s,  s,  s), w), new Vertex(new Vector3F( s, -s,  s), w), new Vertex(new Vector3F( s, -s, -s), w),
                new Vertex(new Vector3F( s, -s, -s), w), new Vertex(new Vector3F( s,  s, -s), w), new Vertex(new Vector3F( s,  s,  s), w),

                // Top face (Y+)
                new Vertex(new Vector3F(-s,  s, -s), w), new Vertex(new Vector3F(-s,  s,  s), w), new Vertex(new Vector3F( s,  s,  s), w),
                new Vertex(new Vector3F( s,  s,  s), w), new Vertex(new Vector3F( s,  s, -s), w), new Vertex(new Vector3F(-s,  s, -s), w),

                // Bottom face (Y-)
                new Vertex(new Vector3F(-s, -s, -s), w), new Vertex(new Vector3F( s, -s, -s), w), new Vertex(new Vector3F( s, -s,  s), w),
                new Vertex(new Vector3F( s, -s,  s), w), new Vertex(new Vector3F(-s, -s,  s), w), new Vertex(new Vector3F(-s, -s, -s), w)
            ];
        }
        
        __MESH_CUBE = (GLMesh)WE.Render.API.CreateMesh(
            new VertexLayout(
                new VertexAttribute("aPos", 3, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.Byte, true)
            ), GetCubeVertices()
        );

        WE.Asset.Register("Triangle", () => __MESH_TRIANGLE);
        WE.Asset.Register("Cube", () => __MESH_CUBE);
        WE.Asset.Register("DefaultShader", () => __PROGRAM);
    }

    public static void __RENDERTESTRENDER(){
        WE.Render.API.DepthTest = true;
        
        WEE.Interface.CurrentScene?.Render(WEE.Editor.SceneViewCamera, __UNIFORM_VPROJ, __UNIFORM_MPROJ, __UNIFORM_COLOR);
    }
}