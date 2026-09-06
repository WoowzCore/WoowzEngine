using WLO.Math;

namespace WEO;

public class Transform : WLI.Packable{
    public event Action<Transform>? OnChanged;
    public event Action<Transform, Vector3F>? OnChangedPosition;
    public event Action<Transform, Vector3F>? OnChangedRotation;
    public event Action<Transform, Vector3F>? OnChangedScale;

    public bool InheritPosition = true;
    public bool InheritRotation = true;
    public bool InheritScale    = true;
    
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

    public Vector3F WorldPosition => GetWorldMatrix().Position;
    public Vector3F WorldRotation => GetWorldMatrix().Rotation;
    public Vector3F WorldScale    => GetWorldMatrix().Scale;
    
    public Transform? Parent;

    public bool IsDirty{ get; private set; } = true;
    public void SetDirty(){
        if(IsDirty){ return; } IsDirty = true;
        OnChanged?.Invoke(this);
    }
    
    public Matrix4F GetLocalMatrix() => Matrix4F.CreatePosition(Position) *
                                        Matrix4F.CreateRotation(Rotation) *
                                        Matrix4F.CreateScale(Scale);

    private Matrix4F __WorldMatrix = Matrix4F.Identity;
    public Matrix4F GetWorldMatrix(){
        bool ParentDirty = Parent != null && Parent.IsDirty;
        
        if(IsDirty || ParentDirty){
            if(Parent == null){
                __WorldMatrix = GetLocalMatrix();
            }else{
                Matrix4F ParentWorld = Parent.GetWorldMatrix();

                if(InheritPosition && InheritRotation && InheritScale){
                    __WorldMatrix = ParentWorld * GetLocalMatrix();
                }else{
                    Vector3F ParentPosition = InheritPosition ? ParentWorld.Position : Vector3F.Zero;
                    Vector3F ParentRotation = InheritRotation ? ParentWorld.Rotation : Vector3F.Zero;
                    Vector3F ParentScale    = InheritScale    ? ParentWorld.Scale    : Vector3F.One;
                    
                    Matrix4F FilteredParent = Matrix4F.CreatePosition(ParentPosition) *
                                              Matrix4F.CreateRotation(ParentRotation) *
                                              Matrix4F.CreateScale(ParentScale);
                    
                    __WorldMatrix = FilteredParent * GetLocalMatrix();
                }
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
        ["Scale"   ] = Scale,
        
        ["InheritPosition"] = InheritPosition,
        ["InheritRotation"] = InheritRotation,
        ["InheritScale"]    = InheritScale
    };
    
    public void __Unpack(Dictionary<string, object?> Data){
        Position = WL.Packer.Get(Data, "Position", new Vector3F());
        Rotation = WL.Packer.Get(Data, "Rotation", new Vector3F());
        Scale    = WL.Packer.Get(Data, "Scale"   , new Vector3F());

        InheritPosition = WL.Packer.Get(Data, "InheritPosition", true);
        InheritRotation = WL.Packer.Get(Data, "InheritRotation", true);
        InheritScale    = WL.Packer.Get(Data, "InheritScale"   , true);
        
        SetDirty();
    }
}