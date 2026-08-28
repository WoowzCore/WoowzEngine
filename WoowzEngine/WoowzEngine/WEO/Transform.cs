using WLO.Math;

namespace WEO;

public class Transform : WLI.Packable{
    public Vector3F Position = new Vector3F(0, 0, 0);
    public Vector3F Rotation = new Vector3F(0, 0, 0);
    public Vector3F Scale    = new Vector3F(1, 1, 1);
    
    public Transform? Parent;
    
    public bool IsDirty = true;
    
    public Matrix4F GetLocalMatrix() => Matrix4F.CreateTranslation(Position) *
                                        Matrix4F.CreateRotationX(Rotation.X) *
                                        Matrix4F.CreateRotationY(Rotation.Y) *
                                        Matrix4F.CreateRotationZ(Rotation.Z) *
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