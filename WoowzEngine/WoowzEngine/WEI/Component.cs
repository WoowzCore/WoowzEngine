using WEO;

namespace WEI;

public abstract class Component{
    public Entity Owner{ get; internal set; } = null!;
}