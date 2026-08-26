using WEI;
using WLO.Math;

namespace WEO.Processor;

public static class PRender{
    public static Matrix4F ViewProjection;
    
    public static void Render(Scene Scene, Camera Camera){ // todo
        ViewProjection = Camera.GetProjectionMatrix() * Camera.GetViewMatrix();

        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<RenderComponent>()?.OnRender();
        }
    }
}