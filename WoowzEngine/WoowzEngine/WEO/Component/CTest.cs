using WEI;
using WEI.Editor;
using WEO;
using WLI.GPU;
using WLO;
using WLO.GPU;
using WLO.Math;
using WLO.Render;

namespace WEO_Component;

public class CTest : RenderComponent{
    [WEESave] public Asset<Mesh   > Mesh   ;
    [WEESave] public Asset<Program> Program;
    
    public override void OnRender(DeltaTimeInfo DTI, Vector3F CameraPosition){
        GLMesh?      Mesh__    = Mesh   .Resolve() as GLMesh;
        GLProgram?   Program__ = Program.Resolve() as GLProgram;
        
        if(Mesh__ == null || Program__ == null){ return; }
        
        WE.Render.Queue.Submit(new GLRenderQueue.Command{
            DistanceToCamera = (ActualPosition - CameraPosition).Length,
            IsTransparent =  IsTransparent,
            
            Mesh      = Mesh__,
            Program   = Program__,
            
            Uniforms = [
                UniformValue.CreateM4F(0, Owner.Transform.GetWorldMatrix()),
                UniformValue.CreateV3F(1, new Vector3F(Random.Shared.Next(0, 255) / 255f, Random.Shared.Next(0, 255) / 255f, Random.Shared.Next(0, 255) / 255f))
            ]
        });
    }

    public override void OnEngineUpdate(DeltaTimeInfo DTI){
        Owner.Transform.Scale = new Vector3F(Random.Shared.NextSingle() + 0.5f, Random.Shared.NextSingle() + 0.5f, Random.Shared.NextSingle() + 0.5f);
        Owner.Transform.IsDirty = true;
    }
    
    public override void OnUpdate(DeltaTimeInfo DTI){
        Owner.Transform.Rotation = new Vector3F(Random.Shared.NextSingle() * MathF.PI, Random.Shared.NextSingle() * MathF.PI, Random.Shared.NextSingle() * MathF.PI);
        Owner.Transform.IsDirty = true;
    }
}