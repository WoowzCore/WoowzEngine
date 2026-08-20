using WLI.GPU;
using WLO.Math;

namespace WEO;

public class GameObject{
    public string Name = "New GameObject";
    public readonly Transform Transform = new Transform();

    // todo, это временно, потом переедет в класс рендера
    public Mesh?    Mesh;
    public Program? Program;

    public void Render(Matrix4F ViewProjection, int Uniform_ViewProjection, int Uniform_ModelProjection){
        if(Mesh == null || Program == null){ return; }
        
        Program.SetUniformM4F(Uniform_ViewProjection, ViewProjection);
        Program.SetUniformM4F(Uniform_ModelProjection, Transform.GetModelMatrix());
        
        WE.Render.API.Draw(Mesh, Program);
    }
}