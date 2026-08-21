using WLO;
using WLO.Math;

namespace WEO;

public class Scene{
    public string Name = "New Scene";

    private readonly HashSet<GameObject> __Registry = [];
    public IEnumerable<GameObject> AllGameObjects => __Registry;

    public bool Add(GameObject GO){
        if(__Registry.Contains(GO)){ return false; }

        __Registry.Add(GO);

        GO.Node.OnParentChanged += OnGameObjectParentChanged;

        foreach(HierarchyNode<GameObject> Child in GO.Node.Children){
            Add(Child.Owner);
        }

        return true;
    }

    public bool Remove(GameObject GO){
        if(!__Registry.Contains(GO)){ return false; }

        __Registry.Remove(GO);

        GO.Node.OnParentChanged -= OnGameObjectParentChanged;
        
        foreach(HierarchyNode<GameObject> Child in GO.Node.Children){
            Remove(Child.Owner);
        }
        
        return true;
    }

    private void OnGameObjectParentChanged(HierarchyNode<GameObject> Self, HierarchyNode<GameObject>? OldParent, HierarchyNode<GameObject>? NewParent){
        
    }
    
    public void Render(Camera Camera, int Uniform_ViewProjection, int Uniform_ModelProjection){
        Matrix4F ViewProjection = Camera.GetProjectionMatrix() * Camera.GetViewMatrix();

        
    }
}