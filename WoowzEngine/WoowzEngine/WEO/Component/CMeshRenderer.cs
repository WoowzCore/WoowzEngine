using WEI;
using WEI.Editor;
using WEO.Processor;
using WLI.GPU;
using WLO.Math;

namespace WEO_Component;

public class CMeshRenderer : RenderComponent{
    public Mesh?    Mesh = null!;
    public Program? Program = null!;

    [WEESave] private Color4B Color = new Color4B(255, 255, 255);

    [WEESave] private bool Active = true;
    
    public override void OnRender(){
        if(Mesh == null || Program == null || !Active){ return; }
        
        Program.SetUniformM4F(PRender.TODO_Uniform_ViewProjection, PRender.ViewProjection);
        Program.SetUniformM4F(PRender.TODO_Uniform_ModelProjection, Owner.Transform.GetWorldMatrix());
        Program.SetUniformV3F(PRender.TODO_Uniform_Color, new Vector3F(Color.R / 255f, Color.G / 255f, Color.B / 255f));
        
        WE.Render.API.Draw(Mesh, Program);
    }
}