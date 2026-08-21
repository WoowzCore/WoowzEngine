using WEI;
using WLO;

namespace WEO;

public class Entity{
    public string Name;

    public readonly HierarchyNode<Entity> Node;
    public readonly Transform             Transform;

    private readonly List<Component> __Components = [];
    
    public Entity(string Name = "New Entity"){
        this.Name = Name;
        Node = new HierarchyNode<Entity>(this);
        Transform = new Transform();

        Node.OnParentChanged += (Self, OldParent, NewParent) => {
            Transform.Parent = NewParent?.Owner.Transform;
            Transform.IsDirty = true;
        };
    }

    public T AddComponent<T>() where T : Component, new(){
        T Component = new T{ Owner = this };
        __Components.Add(Component);
        return Component;
    }

    public T? GetComponent<T>() where T : Component{
        return __Components.OfType<T>().FirstOrDefault();
    }

    public void SetTransformDirty(){
        if(Transform.IsDirty){ return; }

        Transform.IsDirty = true;

        foreach(HierarchyNode<Entity> Child in Node.Children){
            Child.Owner.SetTransformDirty();
        }
    }
}