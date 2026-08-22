using WEO.Processor;
using WLO;
using WLO.Math;

namespace WEO;

public class Scene{
    public string Name = "New Scene";

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

    public string SaveToJSON(){
        return WL.Serializer.ToJson(new Dictionary<string, object>(){
            ["Name"] = Name,
            ["Entities"] = Roots.Select(E => E.Serialize()).ToList()
        });
    }

    public static Scene LoadFromJSON(string JSON){
        Dictionary<string, object> Data = WL.Serializer.FromJson(JSON);
        Scene Result = new Scene();
        
        if(Data.TryGetValue("Name", out object? V_Name__) && V_Name__ is string V_Name){
            Result.Name = V_Name;
        }
        
        if(Data.TryGetValue("Entities", out object? V_Entities__) && V_Entities__ is IEnumerable<object> V_Entities){
            foreach(object V_Entity__ in V_Entities){
                if(V_Entity__ is Dictionary<string, object> V_Entity){
                    Entity Root = new Entity();
                    Root.Deserialize(V_Entity);
                    Result.Add(Root);
                }
            }
        }
        
        return Result;
    }
    
    // ----------------------------------------------------------------------
    
    public void Render(Camera Camera, int Uniform_ViewProjection, int Uniform_ModelProjection, int Uniform_Color){
        PRender.Render(this, Camera, Uniform_ViewProjection, Uniform_ModelProjection, Uniform_Color);
    }
}