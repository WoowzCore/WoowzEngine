using WEI;
using WEI.Editor;
using WEO;
using WEO.Processor;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;

namespace WEO_Component;

public class CMeshRenderer : RenderComponent{
    [WEESave] public Asset<Mesh   > Mesh   ;
    [WEESave] public Asset<Program> Program;

    [WEESave] private Color4B Color = new Color4B(255, 255, 255);
    [WEESave] private bool Active = true;
    
    public override void OnRender(){
        if(!Active){ return; }

        GLMesh?    Mesh__    = Mesh   .Resolve() as GLMesh;
        GLProgram? Program__ = Program.Resolve() as GLProgram;
        
        if(Mesh__ == null || Program__ == null){ return; }
        
        //TODO, надо сделать какую-то runtime кеширование uniforms, или что-то такоееееее я хзззз, что-бы такой хунёй не маятся
        
        Program__.SetUniformM4F(Program__.GetUniform("uViewProjection"), PRender.ViewProjection);
        Program__.SetUniformM4F(Program__.GetUniform("uModelProjection"), Owner.Transform.GetWorldMatrix());
        Program__.SetUniformV3F(Program__.GetUniform("uColor"), new Vector3F(Color.R / 255f, Color.G / 255f, Color.B / 255f));
        
        WE.Render.API.Draw(Mesh__, Program__);
    }
}