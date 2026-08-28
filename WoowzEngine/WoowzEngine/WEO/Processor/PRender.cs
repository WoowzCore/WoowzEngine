using WEI;
using WLO;
using WLO.Math;

namespace WEO.Processor;

public static class PRender{
    public static void Render(Scene Scene, DeltaTimeInfo DTI, Camera Camera){
        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<RenderComponent>()?.OnRender(DTI, Camera.Position);
        }
    }
}