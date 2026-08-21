using WLO.Math;

namespace WEO;

public class Transform{
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
}