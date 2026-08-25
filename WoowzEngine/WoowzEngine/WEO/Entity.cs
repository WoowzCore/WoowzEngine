using WEI;
using WEO_Component;
using WLO;

namespace WEO;

public class Entity : WLI.Packable, WLI.Hierarchical<Entity>{
    public string Name = "New Entity";

    public HierarchyNode<Entity> Node{ get; }
    public readonly Transform    Transform;
    
    public Scene? Scene{ get; internal set; }

    private readonly List<Component> __Components = [];

    public Entity(){
        Node = new HierarchyNode<Entity>(this);
        Transform = new Transform();

        Node.ChildFactory = Data => {
            Entity Entity = new Entity();
            WL.Packer.Unpack(Entity, Data!);
            return Entity;
        };

        Node.OnParentChanged += (Self, OldParent, NewParent) => {
            Transform.Parent = NewParent?.Owner.Transform;
            Transform.IsDirty = true;
        };
    }
    
    public Entity(string Name) : this(){
        this.Name = Name;
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

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Name"      ] = Name,
        ["Transform" ] = Transform,
        ["Components"] = __Components,
        ["Hierarchy" ] = Node
    };
    
    public void __Unpack(Dictionary<string, object?> Data){
        Name = WL.Packer.Get(Data, "Name", Name)!;
        
        Dictionary<string, object?>? TransformData = WL.Packer.Get<Dictionary<string, object?>>(Data, "Transform", Raw: true);
        if(TransformData != null){ WL.Packer.Unpack(Transform, TransformData); }
        
        List<Component>? ComponentsList =  WL.Packer.Get<List<Component>>(Data, "Components");
        if(ComponentsList != null){
            __Components.Clear();
            foreach(var Component in ComponentsList){
                if(Component != null!){
                    Component.Owner = this;
                    __Components.Add(Component);
                }
            }
        }

        Dictionary<string, object?>? HierarchyData =  WL.Packer.Get<Dictionary<string, object?>>(Data, "Hierarchy", Raw: true);
        if(HierarchyData != null){ WL.Packer.Unpack(Node, HierarchyData); }
    }
}