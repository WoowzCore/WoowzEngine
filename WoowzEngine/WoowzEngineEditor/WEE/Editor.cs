using WEE_Interface;
using WEO;
using WLI_Input;
using WLO.Math;

namespace WEE;

public static class Editor{
    public static Camera ViewCamera = new Camera();

    public static float CameraSpeed       = 1   ;
    public static float CameraSensitivity = 0.5f;
    
    public static void UpdateSceneView(){
        UpdateCamera();
    }

    public static void UpdateCamera(){
        if(WEE.Interface.CurrentScene == null){ return; }

        if(I_View.FocusSceneView){
            UpdateCameraControls();    
        }
    }
    
    public static void UpdateCameraControls(){
        float DT = (float)WEE.Cycle.Render_DT;
        WLI_Input.Keyboard Keyboard = WEE.Window.MainWindow.Keyboard;
        WLI_Input.Mouse    Mouse    = WEE.Window.MainWindow.Mouse;

        if(Mouse.IsButtonDown(Mouse.Button.Right)){
            Vector2I MouseDelta = Mouse.Delta;

            ViewCamera.Rotation.Y += MouseDelta.X * CameraSensitivity * DT;
            ViewCamera.Rotation.X += MouseDelta.Y * CameraSensitivity * DT;
        }

        float RotationSpeed = 2 * DT; 

        if(Keyboard.IsKeyDown(Keyboard.Key.Left )){ ViewCamera.Rotation.Y -= RotationSpeed; }
        if(Keyboard.IsKeyDown(Keyboard.Key.Right)){ ViewCamera.Rotation.Y += RotationSpeed; }
        if(Keyboard.IsKeyDown(Keyboard.Key.Up   )){ ViewCamera.Rotation.X -= RotationSpeed; }
        if(Keyboard.IsKeyDown(Keyboard.Key.Down )){ ViewCamera.Rotation.X += RotationSpeed; }
        
        Vector3F MoveDirection = new Vector3F();

        if(Keyboard.IsKeyDown(Keyboard.Key.W)){ MoveDirection += ViewCamera.Forward; }
        if(Keyboard.IsKeyDown(Keyboard.Key.S)){ MoveDirection -= ViewCamera.Forward; }
        if(Keyboard.IsKeyDown(Keyboard.Key.D)){ MoveDirection += ViewCamera.Right; }
        if(Keyboard.IsKeyDown(Keyboard.Key.A)){ MoveDirection -= ViewCamera.Right; }
        
        if(Keyboard.IsKeyDown(Keyboard.Key.Space   )){ MoveDirection += new Vector3F(0, 1, 0); }
        if(Keyboard.IsKeyDown(Keyboard.Key.ControlL)){ MoveDirection -= new Vector3F(0, 1, 0); }

        float CameraSpeed__ = CameraSpeed;

        if(Keyboard.IsKeyDown(Keyboard.Key.ShiftL)){ CameraSpeed__ = 5; }

        if(MoveDirection.Length > 0){
            ViewCamera.Position += MoveDirection.Normalized * CameraSpeed__ * 5 * DT;
        }
    }
}