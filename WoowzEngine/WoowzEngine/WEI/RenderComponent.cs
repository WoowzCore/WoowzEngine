using WLO;
using WLO.Math;

namespace WEI;

public abstract class RenderComponent : Component{
    public Vector3F ActualPosition = new Vector3F();
    public bool     IsTransparent  = false;
    
    // ----------------------------------------------------------------------
    
    public abstract void OnRender(DeltaTimeInfo DTI, Vector3F CameraPosition);
}