using WEI;
using WLO;
using WLO.Math;

namespace WEO;

public class Scene : WLI.Packable{
    public string Name = "New Scene";

    public bool DoUpdate       = true;
    public bool DoEngineUpdate = false;
    public bool DoRender       = true;
    
    public EditorInfo? __EditorInfo;
    
    // ----------------------------------------------------------------------
    
    private readonly HashSet    <Entity> __Registry = [];
    public           IEnumerable<Entity> AllEntity => __Registry;

    public IEnumerable<Entity> Roots => __Registry.Where(E => E.Node.Parent == null);
    
    public bool Add(Entity Entity){
        if(!__Registry.Add(Entity)){ return false; }

        Entity.Scene = this;

        foreach(Component Component in Entity.GetAllComponents()){ RegisterComponent(Component); }
        
        Entity.Node.OnChildAdded    += OnChildAddedToEntity;
        Entity.Node.OnParentChanged += OnEntityParentChanged;

        foreach(HierarchyNode<Entity> Child in Entity.Node.Children.ToList()){
            Add(Child.Owner);
        }

        return true;
    }

    public bool Remove(Entity Entity){
        if(!__Registry.Contains(Entity)){ return false; }

        foreach(Component Component in Entity.GetAllComponents()){ UnregisterComponent(Component); }
        
        Entity.Scene = null;
        
        __Registry.Remove(Entity);

        Entity.Node.OnChildAdded    -= OnChildAddedToEntity;
        Entity.Node.OnParentChanged -= OnEntityParentChanged;
        
        foreach(HierarchyNode<Entity> Child in Entity.Node.Children.ToList()){
            Remove(Child.Owner);
        }
        
        return true;
    }

    private void OnChildAddedToEntity(HierarchyNode<Entity> Self, HierarchyNode<Entity> Child) {
        Add(Child.Owner);
    }
    
    private void OnEntityParentChanged(HierarchyNode<Entity> Self, HierarchyNode<Entity>? OldParent, HierarchyNode<Entity>? NewParent){
        if(NewParent != null){
            if(__Registry.Contains(NewParent.Owner)){
                Add(Self.Owner);
            }else{
                Remove(Self.Owner);
            }
        }
    }

    public void Clear(bool ClearAllEntities = false){
        foreach(Entity Entity in __Registry.ToList()){
            Remove(Entity);
        }

        if(ClearAllEntities){ Entity.DestroyAllEntities(); }
    }
    
    // ----------------------------------------------------------------------
    
    private readonly Dictionary<Type, HashSet<Component>> __Components = [];

    public IEnumerable<T> GetComponents<T>() where T : class{
        if(__Components.TryGetValue(typeof(T), out HashSet<Component>? Pool)){
            return Pool.Cast<T>().ToList();
        }

        return [];
    }

    internal void RegisterComponent(Component Component){
        Type? Type = Component.GetType();
        while(Type != null && Type != typeof(object)){
            if(!__Components.TryGetValue(Type, out HashSet<Component>? Pool)){
                Pool = new HashSet<Component>();
                __Components[Type] = Pool;
            }

            Pool!.Add(Component);
            Type = Type.BaseType;
        }
    }

    internal void UnregisterComponent(Component Component){
        Type? Type = Component.GetType();
        while(Type != null && Type != typeof(object)){
            if(__Components.TryGetValue(Type, out HashSet<Component>? Pool)){
                Pool.Remove(Component);
                if(Pool.Count == 0){ __Components.Remove(Type); }
            }
            Type = Type.BaseType;
        }
    }
    
    // ----------------------------------------------------------------------

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Name"      ] = Name,
        ["Entities"  ] = Roots.ToList(),
        ["EditorInfo"] = __EditorInfo
    };

    public void __Unpack(Dictionary<string, object?> Data){
        Clear();
        
        Name = WL.Packer.Get(Data, "Name", Name)!;

        __EditorInfo = WL.Packer.Get<EditorInfo?>(Data, "EditorInfo");

        List<Entity>? Entities = WL.Packer.Get<List<Entity>>(Data, "Entities");
        if(Entities != null){
            foreach(Entity Entity in Entities){ Add(Entity); }
        }
    }

    public string SaveToJSON() => WL.String.ToJSON(WL.Packer.Pack(this));

    public static Scene LoadFromJSON(string JSON){
        Dictionary<string, object?>? Data = WL.String.FromJSON(JSON) as Dictionary<string, object?>;
        
        Scene Result = new Scene();
        if(Data != null){
            WL.Packer.Unpack(Result, Data);
        }
        
        return Result;
    }
    
    // ----------------------------------------------------------------------

    public void Update(DeltaTimeInfo DTI){
        if(!DoUpdate){ return; }
        
        foreach(Component C in GetComponents<Component>()){
            C.OnUpdate(DTI);
        }
    }
    
    public void UpdateEngine(DeltaTimeInfo DTI){
        if(!DoEngineUpdate){ return; }

        foreach(Component C in GetComponents<Component>()){
            C.OnEngineUpdate(DTI);
        }
    }
    
    public void Render(DeltaTimeInfo DTI, Vector3F CameraPosition){
        if(!DoRender){ return; }
        
        foreach(RenderComponent C in GetComponents<RenderComponent>()){
            C.OnRender(DTI, CameraPosition);
        }
    }
    
    // ----------------------------------------------------------------------

    public struct EditorInfo : WLI.Packable{
        public Color4B  BackgroundColor;
        public Vector3F CameraPosition;
        public Vector3F CameraRotation;
        public bool     CameraPerspective;
        public float    CameraSpeed;
        public float    CameraFar;
        public long     CreationTime;
        public long     LastSaveTime;

        public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
            ["BackgroundColor"  ] = BackgroundColor,
            ["CameraPosition"   ] = CameraPosition,
            ["CameraRotation"   ] = CameraRotation,
            ["CameraPerspective"] = CameraPerspective,
            ["CameraSpeed"      ] = CameraSpeed,
            ["CameraFar"        ] = CameraFar,
            ["CreationTime"     ] = CreationTime,
            ["LastSaveTime"     ] = LastSaveTime
        };
        
        public void __Unpack(Dictionary<string, object?> Data){
            BackgroundColor   = WL.Packer.Get<Color4B >(Data, "BackgroundColor", BackgroundColor);
            CameraPosition    = WL.Packer.Get<Vector3F>(Data, "CameraPosition", CameraPosition);
            CameraRotation    = WL.Packer.Get<Vector3F>(Data, "CameraRotation", CameraRotation);
            CameraPerspective = WL.Packer.Get<bool    >(Data, "CameraPerspective", CameraPerspective);
            CameraSpeed       = WL.Packer.Get<float   >(Data, "CameraSpeed", CameraSpeed);
            CameraFar         = WL.Packer.Get<float   >(Data, "CameraFar", CameraFar);
            CreationTime      = WL.Packer.Get<long    >(Data, "CreationTime", CreationTime);
            LastSaveTime      = WL.Packer.Get<long    >(Data, "LastSaveTime", LastSaveTime);
        }
    }
}