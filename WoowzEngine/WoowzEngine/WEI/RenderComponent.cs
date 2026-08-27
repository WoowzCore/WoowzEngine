using WLO.Math;

namespace WEI;

public abstract class RenderComponent : Component{
    public Vector3F ActualPosition = new Vector3F();
    public bool     IsTransparent  = false;
    
    public abstract void OnRender(Vector3F CameraPosition);
}