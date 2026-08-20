using WLO.Math;

namespace WEO;

public class Scene{
    public          string           Name        = "New Scene";
    public readonly List<GameObject> GameObjects = [];

    public void Render(Camera Camera, int Uniform_ViewProjection, int Uniform_ModelProjection){
        Matrix4F ViewProjection = Camera.GetProjectionMatrix() * Camera.GetViewMatrix();

        foreach(GameObject GameObject in GameObjects){
            GameObject.Render(ViewProjection, Uniform_ViewProjection, Uniform_ModelProjection);
        }
    }
}