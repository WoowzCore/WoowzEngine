using WLI.GPU;
using WLO;
using WLO.Math;

namespace WEO;

public class GameObject{
    public string Name;

    public readonly HierarchyNode<GameObject> Node;
    public readonly Transform                 Transform;

    public GameObject(string Name = "New GameObject"){
        this.Name = Name;
        Node = new HierarchyNode<GameObject>(this);
        Transform = new Transform();

        Node.OnParentChanged += (Self, OldParent, NewParent) => {
            Transform.Parent = NewParent?.Owner.Transform;
            Transform.IsDirty = true;
        };
    }
}