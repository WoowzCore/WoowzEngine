using WEI;
using WEO_Component;
using WLO;

namespace WEO;

public class Entity : WLI.Serializable, WLI.Hierarchical<Entity>{
    public string Name;

    public HierarchyNode<Entity> Node{ get; }
    public readonly Transform    Transform;

    private readonly List<Component> __Components = [];
    
    public Entity(string Name = "New Entity"){
        this.Name = Name;
        Node = new HierarchyNode<Entity>(this);
        Transform = new Transform();

        Node.ChildFactory = (Data) => {
            Entity Entity = new Entity();
            Entity.Deserialize(Data);
            return Entity;
        };

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

    public bool RemoveComponent(Component Component){
        if(__Components.Remove(Component)){
            Component.Owner = null!;
            return true;
        }
        return false;
    }

    public IEnumerable<Component> GetAllComponents() => __Components;
    
    public void SetTransformDirty(){
        if(Transform.IsDirty){ return; }

        Transform.IsDirty = true;

        foreach(HierarchyNode<Entity> Child in Node.Children.ToList()){
            Child.Owner.SetTransformDirty();
        }
    }

    public void Destroy(){
        foreach(HierarchyNode<Entity> Child in Node.Children.ToList()){
            Child.Owner.Destroy();
        }
        
        Node.SetParent(null);
        
        __Components.Clear();
    }

    public Dictionary<string, object> Serialize() => new Dictionary<string, object>(){
        ["Name"] = Name,
        ["Transform"] = Transform.Serialize(),
        ["Components"] = __Components.Select(C => C.Serialize()).ToList(),
        ["Hierarchy"] = Node.Serialize()
    };
    
    public void Deserialize(Dictionary<string, object> Data){
        if(Data.TryGetValue("Name", out object? V_Name__) && V_Name__ is string V_Name){
            Name = V_Name;
        }
        
        if(Data.TryGetValue("Transform", out object? V_Transform__) && V_Transform__ is Dictionary<string, object> V_Transform){
            Transform.Deserialize(V_Transform);
        }
        
        if(Data.TryGetValue("Components", out object? V_Components__) && V_Components__ is IEnumerable<object> V_Components){
            __Components.Clear();
            foreach(object V_Component__ in V_Components){
                if(V_Component__ is Dictionary<string, object> V_Component){
                    if(WL.Serializer.Deserialize(V_Component) is Component Component){
                        Component.Owner = this;
                        __Components.Add(Component);
                    }
                }
            }
        }
        
        if(Data.TryGetValue("Hierarchy", out object? V_Hierarchy__) && V_Hierarchy__ is Dictionary<string, object> V_Hierarchy){
            Node.Deserialize(V_Hierarchy);
        }
    }
}