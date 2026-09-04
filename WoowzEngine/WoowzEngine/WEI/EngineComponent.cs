using WLO;
using WLO.Math;

namespace WEI;

public interface EngineComponent{
    public virtual void OnEngineUpdate(DeltaTimeInfo DTI, bool Selected){}
    public virtual void OnEngineRender(DeltaTimeInfo DTI, Vector3F CameraPosition, bool Selected){}
}