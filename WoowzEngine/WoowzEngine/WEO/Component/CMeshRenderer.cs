using WEI;
using WEO.Processor;
using WLI.GPU;

namespace WEO_Component;

public class CMeshRenderer : RenderComponent{
    public Mesh?    Mesh = null!;
    public Program? Program = null!;
    
    public override void OnRender(){
        if(Mesh == null || Program == null){ return; }
        
        Program.SetUniformM4F(PRender.TODO_Uniform_ViewProjection, PRender.ViewProjection);
        Program.SetUniformM4F(PRender.TODO_Uniform_ModelProjection, Owner.Transform.GetWorldMatrix());
        
        WE.Render.API.Draw(Mesh, Program);
    }
}