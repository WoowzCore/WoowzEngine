using WEO;
using WLI_Input;
using WLO.Math;

namespace WEE;

public static class Editor{
    public static Camera SceneViewCamera = new Camera();

    public static float CameraSpeed       = 1   ;
    public static float CameraSensitivity = 0.5f;
    
    public static void UpdateSceneView(){
        UpdateCamera();
    }

    public static void UpdateCamera(){
        if(WEE.Interface.ActiveScene == null){ return; }

        if(WEE.Interface.FocusSceneView){
            UpdateCameraControls();    
        }
    }
    
    public static void UpdateCameraControls(){
        float DT = (float)WEE.Cycle.Render_DT;
        WLI_Input.Keyboard Keyboard = WEE.Window.MainWindow.Keyboard;
        WLI_Input.Mouse    Mouse    = WEE.Window.MainWindow.Mouse;

        if(Mouse.IsButtonDown(Mouse.Button.Right)){
            Vector2I MouseDelta = Mouse.Delta;

            SceneViewCamera.Rotation.Y += MouseDelta.X * CameraSensitivity * DT;
            SceneViewCamera.Rotation.X += MouseDelta.Y * CameraSensitivity * DT;
        }

        Vector3F MoveDirection = new Vector3F();

        if(Keyboard.IsKeyDown(Keyboard.Key.W)){ MoveDirection += SceneViewCamera.Forward; }
        if(Keyboard.IsKeyDown(Keyboard.Key.S)){ MoveDirection -= SceneViewCamera.Forward; }
        if(Keyboard.IsKeyDown(Keyboard.Key.D)){ MoveDirection += SceneViewCamera.Right; }
        if(Keyboard.IsKeyDown(Keyboard.Key.A)){ MoveDirection -= SceneViewCamera.Right; }
        
        if(Keyboard.IsKeyDown(Keyboard.Key.Space   )){ MoveDirection += new Vector3F(0, 1, 0); }
        if(Keyboard.IsKeyDown(Keyboard.Key.ControlL)){ MoveDirection -= new Vector3F(0, 1, 0); }

        float CameraSpeed__ = CameraSpeed;

        if(Keyboard.IsKeyDown(Keyboard.Key.ShiftL)){ CameraSpeed__ = 5; }

        if(MoveDirection.Length > 0){
            SceneViewCamera.Position += MoveDirection.Normalized * CameraSpeed__ * 5 * DT;
        }
    }
}