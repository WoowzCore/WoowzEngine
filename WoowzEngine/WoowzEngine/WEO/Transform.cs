using WLO.Math;

namespace WEO;

public class Transform : WLI.Packable{
    public event Action<Transform>? OnChanged;
    public event Action<Transform, Vector3F>? OnChangedPosition;
    public event Action<Transform, Vector3F>? OnChangedRotation;
    public event Action<Transform, Vector3F>? OnChangedScale;
    
    private Vector3F __Position = new Vector3F(0, 0, 0);
    private Vector3F __Rotation = new Vector3F(0, 0, 0);
    private Vector3F __Scale    = new Vector3F(1, 1, 1);

    public Vector3F Position{
        get => __Position;
        set{
            if(__Position == value){ return; }
            __Position = value;
            IsDirty = true;
            
            OnChanged?.Invoke(this);
            OnChangedPosition?.Invoke(this, value);
        }
    }
    public Vector3F Rotation{
        get => __Rotation;
        set{
            if(__Rotation == value){ return; }
            __Rotation = value;
            IsDirty = true;
            
            OnChanged?.Invoke(this);
            OnChangedRotation?.Invoke(this, value);
        }
    }
    public Vector3F Scale{
        get => __Scale;
        set{
            if(__Scale == value){ return; }
            __Scale = value;
            IsDirty = true;
            
            OnChanged?.Invoke(this);
            OnChangedScale?.Invoke(this, value);
        }
    }

    //todo
    public Vector3F WorldPosition => GetWorldMatrix().Translation;
    
    public Transform? Parent;
    
    public bool IsDirty = true;
    
    public Matrix4F GetLocalMatrix() => Matrix4F.CreateTranslation(Position) *
                                        Matrix4F.CreateRotation(Rotation) *
                                        Matrix4F.CreateScale(Scale);

    private Matrix4F __WorldMatrix = Matrix4F.Identity;
    public Matrix4F GetWorldMatrix(){
        if(IsDirty){
            if(Parent == null){
                __WorldMatrix = GetLocalMatrix();
            }else{
                __WorldMatrix = Parent.GetWorldMatrix() * GetLocalMatrix();
            }

            IsDirty = false;
        }

        return __WorldMatrix;
    }
    
    // ----------------------------------------------------------------------

    public void SetFrom(Transform Other){
        Position = Other.Position;
        Rotation = Other.Rotation;
        Scale    = Other.Scale;
    }
    
    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Position"] = Position,
        ["Rotation"] = Rotation,
        ["Scale"   ] = Scale
    };
    
    public void __Unpack(Dictionary<string, object?> Data){
        Position = WL.Packer.Get(Data, "Position", new Vector3F());
        Rotation = WL.Packer.Get(Data, "Rotation", new Vector3F());
        Scale    = WL.Packer.Get(Data, "Scale"   , new Vector3F());

        IsDirty = true;
    }
}