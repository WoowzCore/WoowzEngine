using WEE_Interface;
using WEEO;
using WEI.Editor;
using WLI_Render;
using WLI.GPU;
using WLO;
using WLO.GPU;
using WLO.Math;
using WLO.Render;

namespace WEE;

public static class Render{
    public static void Start(){
        WE.Render.Start(WEE.Window.MainWindow.GetProcAddress, new WE.Render.StartParameters{
            DebugLogger = true,
            UseThisLogger = WL.Logger.CurrentLogger
        });
        
        RecreateSceneFrameBuffer(new Vector2I(800, 600));
    }
    
    public static void Stop(){
        WE.Render.Stop();
    }
    
    // ----------------------------------------------------------------------

    public static GLRenderView SceneFramebuffer = null!;

    private static void RecreateSceneFrameBuffer(Vector2I Size){
        if(SceneFramebuffer != null!){ SceneFramebuffer.Destroy(); }
        SceneFramebuffer = GLRenderView.Create(WE.Render.API, Size, [
            GLRenderView.LayerConfig.Color(),
            GLRenderView.LayerConfig.Depth(true)
        ]);
    }
    
    public static void SceneRender(){
        Vector2I TargetSize = I_View.SceneViewSize;

        if(TargetSize.X <= 0 || TargetSize.Y <= 0){ return; }

        if(TargetSize.X > 0 && TargetSize.Y > 0 && 
           (TargetSize.X != SceneFramebuffer.TextureColor0!.Size.X || 
            TargetSize.Y != SceneFramebuffer.TextureColor0!.Size.Y)){
            RecreateSceneFrameBuffer(TargetSize);
            
            WEE.Editor.ViewCamera.Aspect = TargetSize.Aspect;
        }
        
        WE.Render.API.CRenderView = SceneFramebuffer;
        
        WE.Render.API.CRenderView.Viewport = TargetSize;
        
        WE.Render.API.FrameStart();
            
            WE.Render.API.Clear(I_View.BackgroundColor);
                
            WE.Render.API.DepthTest = true;
        
            WEE.Interface.CurrentScene?.Render(WEE.Editor.ViewCamera);
            
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

    [WEERunOnInit]
    public static void __CREATEDEFAULTS(){
        GLProgram __PROGRAM = (GLProgram)WE.Render.API.CreateProgram(
            // language=GLSL
            WE.Render.API.CreateShader(Shader.Type.Vertex, @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aUV;
layout (location = 3) in vec4 aColor;
layout (location = 4) in uint aID;

uniform mat4 uViewProjection;
uniform mat4 uModelProjection;

out vec3 vNormal;

void main() {
    gl_Position = uViewProjection * uModelProjection * vec4(aPosition, 1.0);
    vNormal = mat3(uModelProjection) * aNormal;
}"),
            // language=GLSL
            WE.Render.API.CreateShader(Shader.Type.Fragment, @"
#version 330 core
in vec3 vNormal;
out vec4 fColor;

uniform vec3 uColor;

void main() {
    vec3 LightDirection = normalize(vec3(0.5, 1, 0.3));
    vec3 Norm = normalize(vNormal);
    float Diff = max(dot(Norm, LightDirection), 0);
    float Ambient = 0.2;
    float Light = Ambient + Diff;
    fColor = vec4(uColor * Light, 1);
}")
        );

        VertexLayout VL = new VertexLayout(
            new VertexAttribute("aPosition", 3, VertexAttribute.AttributeType.Float),
            new VertexAttribute("aNormal", 3, VertexAttribute.AttributeType.Float),
            new VertexAttribute("aUV", 2, VertexAttribute.AttributeType.Float),
            new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.UByte, true),
            new VertexAttribute("aID", 1, VertexAttribute.AttributeType.UInt)
        );
        
        GeometryData TRIANGLE = WL.Geometry.CreateTriangle();
        GLMesh __MESH_TRIANGLE = (GLMesh)WE.Render.API.CreateMesh(
            VL, TRIANGLE.Vertices.ToArray(), TRIANGLE.Indices.ToArray()
        );
        
        GeometryData QUAD = WL.Geometry.CreateQuad();
        GLMesh __MESH_QUAD = (GLMesh)WE.Render.API.CreateMesh(
            VL, QUAD.Vertices.ToArray(), QUAD.Indices.ToArray()
        );
        
        GeometryData CUBE = WL.Geometry.CreateCube();
        GLMesh __MESH_CUBE = (GLMesh)WE.Render.API.CreateMesh(
            VL, CUBE.Vertices.ToArray(), CUBE.Indices.ToArray()
        );

        WE.Asset.Register("Triangle", () => __MESH_TRIANGLE);
        WE.Asset.Register("Quad", () => __MESH_QUAD);
        WE.Asset.Register("Cube", () => __MESH_CUBE);
        WE.Asset.Register("DefaultShader", () => __PROGRAM);
    }
}