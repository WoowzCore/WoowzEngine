using WLO.Math;

namespace WEO;

public class Transform{
    public Vector3F Position = new Vector3F(0, 0, 0);
    public Vector3F Rotation = new Vector3F(0, 0, 0);
    public Vector3F Scale    = new Vector3F(1, 1, 1);
    
    public Matrix4F GetModelMatrix() => Matrix4F.CreateScale(Scale) *
                                        Matrix4F.CreateRotationX(Rotation.X) *
                                        Matrix4F.CreateRotationY(Rotation.Y) *
                                        Matrix4F.CreateRotationZ(Rotation.Z) *
                                        Matrix4F.CreateTranslation(Position);
}