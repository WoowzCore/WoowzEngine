using WEO.Processor;
using WLO;
using WLO.Math;

namespace WEO;

public class Scene{
    public string Name = "New Scene";

    private readonly HashSet    <Entity> __Registry = [];
    public           IEnumerable<Entity> AllEntity => __Registry;

    public IEnumerable<Entity> Roots => __Registry.Where(E => E.Node.Parent == null);

    public bool Add(Entity E){
        if(__Registry.Contains(E)){ return false; }

        __Registry.Add(E);

        E.Node.OnChildAdded += OnChildAddedToEntity;
        E.Node.OnParentChanged += OnEntityParentChanged;

        foreach(HierarchyNode<Entity> Child in E.Node.Children){
            Add(Child.Owner);
        }

        return true;
    }

    public bool Remove(Entity E){
        if(!__Registry.Contains(E)){ return false; }

        __Registry.Remove(E);

        E.Node.OnChildAdded -= OnChildAddedToEntity;
        E.Node.OnParentChanged -= OnEntityParentChanged;
        
        foreach(HierarchyNode<Entity> Child in E.Node.Children){
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
    
    public void Render(Camera Camera, int Uniform_ViewProjection, int Uniform_ModelProjection){
        PRender.Render(this, Camera, Uniform_ViewProjection, Uniform_ModelProjection);
    }
}