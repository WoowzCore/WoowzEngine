using WEI;
using WLO;

namespace WEO;

public class Entity : WLI.Packable, WLI.Hierarchical<Entity>{
    public string Name = "New Entity";

    public HierarchyNode<Entity> Node{ get; }
    public readonly Transform    Transform;
    
    public Scene? Scene{ get; internal set; }

    // ----------------------------------------------------------------------

    private Asset<Prefab>? __SourcePrefab;
    public Asset<Prefab>? SourcePrefab{
        get => __SourcePrefab;
        set{
            __SourcePrefab = value;
            UpdatePrefabStatus();
        }
    }
    
    public Entity? PrefabRoot{ get; private set; }
    public bool IsPartOfPrefab => PrefabRoot != null;

    private void UpdatePrefabStatus(){
        Entity? NewRoot = null;

        if(__SourcePrefab.HasValue){
            NewRoot = this;
        }else{
            NewRoot = Node.Parent?.Owner.PrefabRoot;
        }

        if(PrefabRoot != NewRoot){
            PrefabRoot = NewRoot;

            foreach(HierarchyNode<Entity> Child in Node.Children){
                Child.Owner.UpdatePrefabStatus();
            }
        }
    }

    public void SyncFromPrefab(){
        if(!SourcePrefab.HasValue){ return; }
        Prefab? Prefab = SourcePrefab.Value.Resolve();
        if(Prefab == null){ return; }

        // todo, я уверен можно проще
        
        string OldName = Name;
        Dictionary<string, object?>? OldTransformData = WL.Packer.Pack(Transform) as Dictionary<string, object?>;
        HierarchyNode<Entity>? OldParent = Node.Parent;

        DestroyChildrens();
        RemoveAllComponents();

        WL.Packer.Unpack(this, Prefab.EntityData);

        Name = OldName;
        if(OldTransformData != null){
            WL.Packer.Unpack(Transform, OldTransformData);
        }
        Node.SetParent(OldParent);
        
        UpdatePrefabStatus();
    }
    
    // ----------------------------------------------------------------------
    
    private static          uint                     __NextID = 1;
    private static readonly Dictionary<uint, Entity> __IDMap  = [];
    public uint ID{ get; internal set; }
    
    public static Entity? GetFromID(uint ID) => __IDMap.TryGetValue(ID, out Entity? Entity) ? Entity : null;

    // ----------------------------------------------------------------------
    
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

        Transform.OnChanged += (T) => SetTransformDirty();
        
        Node.ChildFactory = Data => {
            Entity Entity = new Entity();
            WL.Packer.Unpack(Entity, Data!);
            return Entity;
        };

        Node.OnParentChanged += (Self, OldParent, NewParent) => {
            Transform.Parent = NewParent?.Owner.Transform;
            SetTransformDirty();
            UpdatePrefabStatus();
        };
    }
    
    public Entity(string Name) : this(){
        this.Name = Name;
    }

    public T AddComponent<T>() where T : Component, new(){
        T Component = new T{ Owner = this };
        __Components.Add(Component);
        
        Scene?.RegisterComponent(Component);
        
        Component.OnAdd();
        
        return Component;
    }
    
    public T? GetComponent<T>() where T : Component{
        return __Components.OfType<T>().FirstOrDefault();
    }

    public bool RemoveComponent(Component Component){
        if(__Components.Remove(Component)){
            Component.OnRemove();
            
            Scene?.UnregisterComponent(Component);
            
            Component.Owner = null!;
            return true;
        }
        return false;
    }

    public IEnumerable<Component> GetAllComponents() => __Components;

    public IEnumerable<T> GetComponents<T>() where T : Component => __Components.OfType<T>();
    
    public void SetTransformDirty(){
        Transform.IsDirty = true;

        foreach(HierarchyNode<Entity> Child in Node.Children){
            if(!Child.Owner.Transform.IsDirty){ Child.Owner.SetTransformDirty(); }
        }
    }

    public void DestroyChildrens(){
        foreach(HierarchyNode<Entity> Child in Node.Children.ToList()){
            Child.Owner.Destroy();
        }
    }

    public void RemoveAllComponents(){
        foreach(Component Component in GetAllComponents().ToList()){
            RemoveComponent(Component);
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
        SourcePrefab = Other.SourcePrefab;
        Transform.SetFrom(Other.Transform);

        if(SourcePrefab != null){
            Prefab? Prefab = SourcePrefab.Value.Resolve();
            if(Prefab != null){
                WL.Packer.Unpack(this, Prefab.EntityData);
            }
        }else{
            List<Component> OldComponents = __Components.ToList();
            foreach(Component Component in OldComponents){ RemoveComponent(Component); }
        
            foreach(Component OtherComponent in Other.__Components){
                Component NewComponent = (Component)Activator.CreateInstance(OtherComponent.GetType())!;
                NewComponent.Owner = this;

                WL.Packer.Unpack(NewComponent, WL.Packer.Pack(OtherComponent) as Dictionary<string, object>);
            
                __Components.Add(NewComponent);
                Scene?.RegisterComponent(NewComponent);
            
                NewComponent.OnAdd();
            }

            foreach(HierarchyNode<Entity> Children in Other.Node.Children){
                Entity DuplicateChild = Children.Owner.Duplicate();
                DuplicateChild.Node.SetParent(Node);
            }
        }
        
        UpdatePrefabStatus();
    }
    
    public Entity Duplicate(){
        Entity Duplicate = new Entity(Name);

        Duplicate.SetFrom(this);
        
        return Duplicate;
    }

    public static Entity FromPrefab(Asset<Prefab> PrefabAsset){
        Prefab? Prefab = PrefabAsset.Resolve();
        if(Prefab == null){ return null!; }

        Entity Entity = new Entity();

        WL.Packer.Unpack(Entity, Prefab.EntityData);
        Entity.SourcePrefab = PrefabAsset;

        return Entity;
    }
    
    // ----------------------------------------------------------------------

    public Dictionary<string, object?> __Pack(){
        Dictionary<string, object?> Data = [];

        Data["ID"] = ID;
        Data["Name"] = Name;
        Data["Transform"] = Transform;

        if(SourcePrefab.HasValue){
            Data["SourcePrefab"] = SourcePrefab;
        }else{
            Data["Components"] = __Components;
            Data["Hierarchy"] = Node;
        }
        
        return Data;
    }
    
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

        if(Data.ContainsKey("SourcePrefab")){
            SourcePrefab = WL.Packer.Get<Asset<Prefab>?>(Data, "SourcePrefab");
            Prefab? Prefab = SourcePrefab?.Resolve();
            if(Prefab != null){
                WL.Packer.Unpack(this, Prefab.EntityData);
            }
        }
        
        Name = WL.Packer.Get(Data, "Name", Name)!;
        
        Dictionary<string, object?>? TransformData = WL.Packer.Get<Dictionary<string, object?>>(Data, "Transform", Raw: true);
        if(TransformData != null){ WL.Packer.Unpack(Transform, TransformData); }

        if(Data.ContainsKey("Components")){
            List<Component>? ComponentsList = WL.Packer.Get<List<Component>>(Data, "Components");
            if(ComponentsList != null){
                foreach(Component Component in __Components.ToList()){ RemoveComponent(Component); }

                foreach(Component? Component in ComponentsList){
                    if(Component != null!){
                        Component.Owner = this;
                        __Components.Add(Component);
                        Scene?.RegisterComponent(Component);
                        Component.OnAdd();
                    }
                }
            }
        }

        if(Data.ContainsKey("Hierarchy")){
            Dictionary<string, object?>? HierarchyData = WL.Packer.Get<Dictionary<string, object?>>(Data, "Hierarchy", Raw: true);
            if(HierarchyData != null){ WL.Packer.Unpack(Node, HierarchyData); }
        }
        
        UpdatePrefabStatus();
    }
}