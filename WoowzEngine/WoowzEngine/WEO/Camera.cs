using WLO.Math;

namespace WEO;

public class Camera{
    public Vector3F Position = new Vector3F(0, 0, 3);
    public Vector3F Rotation = new Vector3F(0, 0, 0);

    public float FOV    = 60;
    public float Aspect = 1.6f;
    public float Near   = 0.1f;
    public float Far    = 1000;

    public Matrix4F GetViewMatrix() => Matrix4F.CreateRotationX(Rotation.X) *
                                       Matrix4F.CreateRotationY(Rotation.Y) *
                                       Matrix4F.CreateRotationZ(Rotation.Z) *
                                       Matrix4F.CreateTranslation(Position.Negative);

    public Matrix4F GetProjectionMatrix() => Matrix4F.CreatePerspective(FOV * (float)(System.Math.PI / 180 /* todo */), Aspect, Near, Far);
}