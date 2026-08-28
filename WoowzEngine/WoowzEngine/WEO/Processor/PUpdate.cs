using WEI;
using WLO;
using WLO.Math;

namespace WEO.Processor;

public static class PUpdate{
    public static void Update(Scene Scene, DeltaTimeInfo DTI){
        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<Component>()?.OnUpdate(DTI);
        }
    }
    
    public static void UpdateEngine(Scene Scene, DeltaTimeInfo DTI){
        foreach(Entity E in Scene.AllEntity){
            E.GetComponent<Component>()?.OnEngineUpdate(DTI);
        }
    }
}