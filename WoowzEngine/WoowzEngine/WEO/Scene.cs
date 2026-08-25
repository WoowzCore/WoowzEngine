using WEO.Processor;
using WLO;
using WLO.Math;

namespace WEO;

public class Scene : WLI.Packable{
    public string Name = "New Scene";

    public EditorInfo? __EditorInfo;
    
    private readonly HashSet    <Entity> __Registry = [];
    public           IEnumerable<Entity> AllEntity => __Registry;

    public IEnumerable<Entity> Roots => __Registry.Where(E => E.Node.Parent == null);

    public bool Add(Entity Entity){
        if(!__Registry.Add(Entity)){ return false; }

        Entity.Node.OnChildAdded += OnChildAddedToEntity;
        Entity.Node.OnParentChanged += OnEntityParentChanged;

        foreach(HierarchyNode<Entity> Child in Entity.Node.Children.ToList()){
            Add(Child.Owner);
        }

        return true;
    }

    public bool Remove(Entity Entity){
        if(!__Registry.Contains(Entity)){ return false; }

        __Registry.Remove(Entity);

        Entity.Node.OnChildAdded -= OnChildAddedToEntity;
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

    public void Clear(){ foreach(Entity Entity in __Registry.ToList()){ Remove(Entity); } }

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
    
    public void Render(Camera Camera, int Uniform_ViewProjection, int Uniform_ModelProjection, int Uniform_Color){
        PRender.Render(this, Camera, Uniform_ViewProjection, Uniform_ModelProjection, Uniform_Color);
    }

    public struct EditorInfo : WLI.Packable{
        public Color4B  BackgroundColor;
        public Vector3F CameraPosition;
        public Vector3F CameraRotation;
        public bool     CameraPerspective;

        public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
            ["BackgroundColor"  ] = BackgroundColor,
            ["CameraPosition"   ] = CameraPosition,
            ["CameraRotation"   ] = CameraRotation,
            ["CameraPerspective"] = CameraPerspective
        };
        
        public void __Unpack(Dictionary<string, object?> Data){
            BackgroundColor   = WL.Packer.Get<Color4B >(Data, "BackgroundColor", BackgroundColor);
            CameraPosition    = WL.Packer.Get<Vector3F>(Data, "CameraPosition", CameraPosition);
            CameraRotation    = WL.Packer.Get<Vector3F>(Data, "CameraRotation", CameraRotation);
            CameraPerspective = WL.Packer.Get<bool    >(Data, "CameraPerspective", CameraPerspective);
        }
    }
}