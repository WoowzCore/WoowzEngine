using WEI;
using WEI.Editor;
using WEO;
using WEO.Processor;
using WLI.GPU;
using WLO.Math;

namespace WEO_Component;

public class CMeshRenderer : RenderComponent{
    [WEESave] public Asset<Mesh   > Mesh   ;
    [WEESave] public Asset<Program> Program;

    [WEESave] private Color4B Color = new Color4B(255, 255, 255);
    [WEESave] private bool Active = true;
    
    public override void OnRender(){
        if(!Active){ return; }

        Mesh?    Mesh__    = Mesh   .Resolve();
        Program? Program__ = Program.Resolve();
        
        if(Mesh__ == null || Program__ == null){ return; }
        
        Program__.SetUniformM4F(PRender.TODO_Uniform_ViewProjection, PRender.ViewProjection);
        Program__.SetUniformM4F(PRender.TODO_Uniform_ModelProjection, Owner.Transform.GetWorldMatrix());
        Program__.SetUniformV3F(PRender.TODO_Uniform_Color, new Vector3F(Color.R / 255f, Color.G / 255f, Color.B / 255f));
        
        WE.Render.API.Draw(Mesh__, Program__);
    }
}