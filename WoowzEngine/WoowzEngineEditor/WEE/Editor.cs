using WEE_Interface;
using WEO;
using WLI_Input;
using WLO.Math;
using WLO.Window;

namespace WEE;

public static class Editor{
    public static void UpdateCamera(){
        if(WEE.D.Selected.Scene == null){ return; }

        if(WEE.D.View.IsFocus || WEE.D.View.IsMouseOver){
            UpdateCameraControls();    
        }
    }
    
    public static void UpdateCameraControls(){
        float DT = WEE.D.Time.Render.DTF;
        
        GLFW.GLFW_Mouse    M = WEE.D.Input.M;
        GLFW.GLFW_Keyboard K = WEE.D.Input.K;

        if(M.IsButtonDown(Mouse.Button.Right)){
            Vector2I MouseDelta = M.Delta;

            WEE.D.View.Camera.Rotation.Y += MouseDelta.X * WEE.D.View.CameraSensitivity * DT;
            WEE.D.View.Camera.Rotation.X += MouseDelta.Y * WEE.D.View.CameraSensitivity * DT;
        }

        float RotationSpeed = 2 * DT; 

        if(K.IsKeyDown(Keyboard.Key.Left )){ WEE.D.View.Camera.Rotation.Y -= RotationSpeed; }
        if(K.IsKeyDown(Keyboard.Key.Right)){ WEE.D.View.Camera.Rotation.Y += RotationSpeed; }
        if(K.IsKeyDown(Keyboard.Key.Up   )){ WEE.D.View.Camera.Rotation.X -= RotationSpeed; }
        if(K.IsKeyDown(Keyboard.Key.Down )){ WEE.D.View.Camera.Rotation.X += RotationSpeed; }
        
        Vector3F MoveDirection = new Vector3F();

        if(K.IsKeyDown(Keyboard.Key.W)){ MoveDirection += WEE.D.View.Camera.Forward; }
        if(K.IsKeyDown(Keyboard.Key.S)){ MoveDirection -= WEE.D.View.Camera.Forward; }
        if(K.IsKeyDown(Keyboard.Key.D)){ MoveDirection += WEE.D.View.Camera.Right; }
        if(K.IsKeyDown(Keyboard.Key.A)){ MoveDirection -= WEE.D.View.Camera.Right; }
        
        if(K.IsKeyDown(Keyboard.Key.Space   )){ MoveDirection += new Vector3F(0, 1, 0); }
        if(K.IsKeyDown(Keyboard.Key.ControlL)){ MoveDirection -= new Vector3F(0, 1, 0); }

        float CameraSpeed__ = WEE.D.View.CameraSpeed;

        if(K.IsKeyDown(Keyboard.Key.ShiftL)){ CameraSpeed__ *= 5; }

        if(MoveDirection.Length > 0){
            WEE.D.View.Camera.Position += MoveDirection.Normalized * CameraSpeed__ * 5 * DT;
        }
    }
}