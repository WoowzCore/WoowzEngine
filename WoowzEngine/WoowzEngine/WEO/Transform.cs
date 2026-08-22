using WLO.Math;

namespace WEO;

public class Transform : WLI.Serializable{
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
    
    public Dictionary<string, object> Serialize() => new Dictionary<string, object>{
        ["Position"] = Position.Serialize(),
        ["Rotation"] = Rotation.Serialize(),
        ["Scale"   ] = Scale.Serialize()
    };
    
    public void Deserialize(Dictionary<string, object> Data){
        if(Data.TryGetValue("Position", out object? Position__) && Position__ is Dictionary<string, object> PositionD__){
            Position.Deserialize(PositionD__);
        }
        
        if(Data.TryGetValue("Rotation", out object? Rotation__) && Rotation__ is Dictionary<string, object> RotationD__){
            Rotation.Deserialize(RotationD__);
        }
        
        if(Data.TryGetValue("Scale", out object? Scale__) && Scale__ is Dictionary<string, object> ScaleD__){
            Scale.Deserialize(ScaleD__);
        }

        IsDirty = true;
    }
}