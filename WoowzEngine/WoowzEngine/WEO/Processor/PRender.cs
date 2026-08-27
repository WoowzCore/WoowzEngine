using WEI;
using WLO.Math;

namespace WEO.Processor;

public static class PRender{
    public static void Render(Scene Scene, Camera Camera){
        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<RenderComponent>()?.OnRender(Camera.Position);
        }
    }
}