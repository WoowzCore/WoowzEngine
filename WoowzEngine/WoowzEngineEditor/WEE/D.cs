using WEO;
using WLO;
using WLO.Interface;
using WLO.Math;
using WLO.Window;

namespace WEE;

public static class D{
    public static class Time{
        public static class Engine{
            public static uint          MaxFPS = 30;
            public static DeltaTimeInfo DTI;
            
            public static double DT  => DTI.DT;
            public static float  DTF => (float)DT;
        }
        
        public static class Render{
            public static uint          MaxFPS = 120;
            public static DeltaTimeInfo DTI;

            public static double DT  => DTI.DT;
            public static float  DTF => (float)DT;

            public static double Elapsed;
            public static float  ElapsedF => (float)Elapsed;

            public static float EngineAlpha;
        }
    }

    public static class Input{
        public static GLFW.GLFW_Mouse    M => WEE.D.Window.Mouse;
        public static GLFW.GLFW_Keyboard K => WEE.D.Window.Keyboard;
    }
    
    public static GLFW Window = null!;
    
    public static GLImGUI ImGUI = null!;
    
    public static class Selected{
        public static Scene?  Scene;
        public static Entity? Entity;
    }
    
    public static class View{
        public static Camera Camera = new Camera();

        public static float CameraSpeed       = 1   ;
        public static float CameraSensitivity = 0.5f;

        public static bool IsFocus;

        public static bool IsMouseOver;

        public static Vector2I Size;

        public static Vector2I LocalMousePosition;
    }
}