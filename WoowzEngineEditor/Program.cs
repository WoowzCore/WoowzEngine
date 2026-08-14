using System.Numerics;
using ImGuiNET;
using WLO;
using WLO.Interface;
using WLO.Math;
using WLO.Render.Hardware;

WLO.Window.GLFW Window = new WLO.Window.GLFW(new Vector2I(800, 600), "WOOWZ ENGINE EDITOR");

Vulkan Render = new WLO.Render.Hardware.Vulkan();
FrameBuffer Frame = new FrameBuffer(Window.Size);
Render.Viewport = Window.Size;

Render.Start();



ImGui.CreateContext();
var IO = ImGui.GetIO();
IO.DisplaySize = new Vector2(800, 600);

IO.Fonts.AddFontDefault();
IO.Fonts.Build();

int i = 0;
DeltaTimeInfo? DTI = null;
while(!Window.IsClosed){
    Window.PollEvents();
    
    if(WL.Thread.LimitByFPS(60, ref DTI)){
        i++;
        Window.Title = "NEW TITLE " + i + " | FPS: " + DTI.Value.FPS;
        
        
        IO.DeltaTime = (float)DTI.Value.DT;
        ImGui.NewFrame();
        
        ImGui.Begin("Woowz Editor");
        ImGui.Text("vulkan test proreorwer");
        if(ImGui.Button("click syka")){ WL.Logger.Info("hello!!!"); }
        ImGui.End();
        
        ImGui.Render();
        
        
        
        Render.FrameStart();
        Render.RENDER_BEGIN(new Color4B((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256)));
        
        
        Render.RENDER_END();
        Render.FrameStop();
    
        Render.DrawFrameBuffer(Frame);
    
        Window.Present(Frame);
    }
}

Render.Stop();

Window.Close();
WL.GLFW.Stop();