using WEI;
using WEI.Editor;
using WEO;
using WEO.Processor;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using WLO.Render;

namespace WEO_Component;

public class CMeshRenderer : RenderComponent{
    [WEESave] public Asset<Mesh   > Mesh   ;
    [WEESave] public Asset<Program> Program;

    [WEESave] public Color4B Color = new Color4B(255, 255, 255);
    [WEESave] public bool Active = true;
    
    public override void OnRender(Vector3F CameraPosition){
        if(!Active){ return; }

        GLMesh?    Mesh__    = Mesh   .Resolve() as GLMesh;
        GLProgram? Program__ = Program.Resolve() as GLProgram;
        
        if(Mesh__ == null || Program__ == null){ return; }
        
        WE.Render.Queue.Submit(new GLRenderQueue.Command{
            DistanceToCamera = (ActualPosition - CameraPosition).Length,
            IsTransparent =  IsTransparent,
            
            Mesh = Mesh__,
            Program = Program__,
            
            Uniforms = [
                UniformValue.CreateM4F(0, PRender.ViewProjection),
                UniformValue.CreateM4F(1, Owner.Transform.GetWorldMatrix()),
                UniformValue.CreateV3F(2, new Vector3F(Color.R / 255f, Color.G / 255f, Color.B / 255f))
            ]
        });
    }
}