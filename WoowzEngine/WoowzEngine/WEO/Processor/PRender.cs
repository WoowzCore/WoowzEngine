using WEI;
using WLO.Math;

namespace WEO.Processor;

public static class PRender{
    public static int      TODO_Uniform_ViewProjection;
    public static int      TODO_Uniform_ModelProjection;
    public static int      TODO_Uniform_Color;
    public static Matrix4F ViewProjection;
    
    public static void Render(Scene Scene, Camera Camera, int TODO1, int TODO2, int TODO3){ // todo
        TODO_Uniform_ViewProjection = TODO1;
        TODO_Uniform_ModelProjection = TODO2;
        TODO_Uniform_Color = TODO3;
        
        ViewProjection = Camera.GetProjectionMatrix() * Camera.GetViewMatrix();

        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<RenderComponent>()?.OnRender();
        }
    }
}