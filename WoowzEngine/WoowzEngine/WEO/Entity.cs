using WEI;
using WLO;

namespace WEO;

public class Entity : WLI.Packable, WLI.Hierarchical<Entity>{
    public string Name = "New Entity";

    public HierarchyNode<Entity> Node{ get; }
    public readonly Transform    Transform;
    
    public Scene? Scene{ get; internal set; }

    private static          uint                     __NextID = 1;
    private static readonly Dictionary<uint, Entity> __IDMap  = [];
    public uint ID{ get; internal set; }

    public static Entity? GetFromID(uint ID) => __IDMap.TryGetValue(ID, out Entity? Entity) ? Entity : null;

    public static void DestroyAllEntities(){
        foreach(KeyValuePair<uint, Entity> KVP in __IDMap){
            KVP.Value.Destroy();
        }
        __IDMap.Clear();
        __NextID = 1;
    }
    
    private readonly List<Component> __Components = [];

    public Entity(){
        ID = __NextID++;
        __IDMap[ID] = this;
        
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
        
        Scene?.RegisterComponent(Component);
        
        return Component;
    }
    
    public T? GetComponent<T>() where T : Component{
        return __Components.OfType<T>().FirstOrDefault();
    }

    public bool RemoveComponent(Component Component){
        if(__Components.Remove(Component)){
            Scene?.UnregisterComponent(Component);
            
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
        foreach(HierarchyNode<Entity> Child in Node.Children.ToList()){ Child.Owner.Destroy(); }

        Scene?.Remove(this);
        
        Node.SetParent(null);

        foreach(Component Component in __Components){
            Component.Owner = null!;
        }
        
        __Components.Clear();
        
        Scene = null;

        __IDMap.Remove(ID);
        ID = 0;
    }

    // ----------------------------------------------------------------------

    public void SetFrom(Entity Other){
        Transform.SetFrom(Other.Transform);
        
        __Components.Clear();
        foreach(Component OtherComponent in Other.__Components){
            Component NewComponent = (Component)Activator.CreateInstance(OtherComponent.GetType())!;
            NewComponent.Owner = this;

            WL.Packer.Unpack(NewComponent, WL.Packer.Pack(OtherComponent) as Dictionary<string, object>);
            
            __Components.Add(NewComponent);
        }

        foreach(HierarchyNode<Entity> Children in Other.Node.Children){
            Entity DuplicateChild = Children.Owner.Duplicate();
            DuplicateChild.Node.SetParent(Node);
        }
    }
    
    public Entity Duplicate(){
        Entity Duplicate = new Entity(Name);

        Duplicate.SetFrom(this);
        
        return Duplicate;
    }
    
    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Name"      ] = Name,
        ["Transform" ] = Transform,
        ["Components"] = __Components,
        ["Hierarchy" ] = Node,
        ["ID"        ] = ID
    };
    
    public void __Unpack(Dictionary<string, object?> Data){
        uint SavedID = WL.Packer.Get(Data, "ID", 0U);

        if(SavedID != 0){
            __IDMap.Remove(ID);
            
            // todo, сменять везде ссылки тогда тоже
            if(__IDMap.ContainsKey(SavedID)){
                ID = __NextID++;
            }else{
                ID = SavedID;
            }
            
            __IDMap[ID] = this;
            
            if(ID >= __NextID){ __NextID = ID + 1; }
        }
        
        Name = WL.Packer.Get(Data, "Name", Name)!;
        
        Dictionary<string, object?>? TransformData = WL.Packer.Get<Dictionary<string, object?>>(Data, "Transform", Raw: true);
        if(TransformData != null){ WL.Packer.Unpack(Transform, TransformData); }
        
        List<Component>? ComponentsList = WL.Packer.Get<List<Component>>(Data, "Components");
        if(ComponentsList != null){
            if(Scene != null){
                foreach(Component C in __Components){
                    Scene.UnregisterComponent(C);
                }
            }
            
            __Components.Clear();
            foreach(var Component in ComponentsList){
                if(Component != null!){
                    Component.Owner = this;
                    __Components.Add(Component);
                    Scene?.RegisterComponent(Component);
                }
            }
        }

        Dictionary<string, object?>? HierarchyData = WL.Packer.Get<Dictionary<string, object?>>(Data, "Hierarchy", Raw: true);
        if(HierarchyData != null){ WL.Packer.Unpack(Node, HierarchyData); }
    }
}