using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using WLO;
using WLO.Math;
using WLO.Render.Hardware;
using WoowzLib.Render.WLO;
using Buffer = Silk.NET.Vulkan.Buffer;

static unsafe class Program{
    public static unsafe void Main(string[] Args){
        WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

        Vulkan Render = new WLO.Render.Hardware.Vulkan();
        FrameBuffer Frame = new FrameBuffer(Window.Size);
        Render.Viewport = Window.Size;

        Render.Start();


        // language=GLSL
        string VCode = @"#version 450
layout(location = 0) in vec2 inPos;
layout(location = 1) in vec4 inColor;
layout(location = 0) out vec4 outColor;
void main() {
    gl_Position = vec4(inPos, 0.0, 1.0);
    outColor = inColor;
}";

        // language=GLSL
        string FCode = @"#version 450
layout(location = 0) in vec4 outColor;
layout(location = 0) out vec4 fColor;
void main() {
    fColor = outColor;
}";

        byte[] VBytes = Render.CompileShader(VCode, "shader.vert", ShaderKind.VertexShader);
        byte[] FBytes = Render.CompileShader(FCode, "shader.frag", ShaderKind.FragmentShader);
        
        ShaderModule VShader = Render.CreateShaderModule(VBytes);
        ShaderModule FShader = Render.CreateShaderModule(FBytes);

        (Pipeline Pipeline, PipelineLayout Layout) = Render.CreateGraphicsPipeline(VShader, FShader);

        Vertex[] Triangle = [
            new Vertex(new Vector2F(0, -0.5f), new Color4B(255, 0, 0)),
            new Vertex(new Vector2F(0.5f, 0.5f), new Color4B(0, 255, 0)),
            new Vertex(new Vector2F(-0.5f, 0.5f), new Color4B(0, 0, 255))
        ];
        Render.InternalCreateMesh(Triangle, out Buffer __TriangleBuffer, out DeviceMemory __TriangleMemory);

        int i = 0;
        DeltaTimeInfo? DTI = null;
        while(!Window.IsClosed){
            Window.PollEvents();
    
            if(WL.Thread.LimitByFPS(60, ref DTI)){
                i++;
                Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
        
                Render.FrameStart();
        
                    Render.Clear(new Color4B((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256)));
        
                    Render.API.CmdBindPipeline(Render.CommandBuffer, PipelineBindPoint.Graphics, Pipeline);
                    ulong Offset = 0;
                    Render.API.CmdBindVertexBuffers(Render.CommandBuffer, 0, 1, &__TriangleBuffer, &Offset);
                    Render.API.CmdDraw(Render.CommandBuffer, 3, 1, 0, 0);
        
                Render.FrameStop();
    
                Render.DrawFrameBuffer(Frame);
    
                Window.Present(Frame);
            }
        }

        Render.Stop();

        Window.Close();
        WL.GLFW.Stop();    
    }
}