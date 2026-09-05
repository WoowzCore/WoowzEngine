using WEEO;
using WEI_Attribute;
using WEO;
using WLO;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;

namespace WEE;

public static class Main{
    public static void Start(string[] Args){
        try{
            WE.Engine.Start();
            WE.Engine.IsEditor = true;

            Pipeline = new Pipeline();

            DebugRotationDeep();
            
            WEE.Window.Start();
            WEE.Control.Start();
            WEE.Render.Start();
            WEE.Interface.Start();
            
            WEE.Window.ConnectEvents();
            WEE.Cycle.Start();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при запуске Main!", e);
        }
    }
    
    public static void DebugRotationDeep() {
    WL.Logger.Debug("=== НАЧАЛО ГЛУБОКОГО ТЕСТА МАТЕМАТИКИ ВРАЩЕНИЙ ===");

    // Тестовые углы (специально разные и не кратные 90 градусам)
    Vector3F testEuler = new Vector3F(0.4f, 0.8f, -0.2f); 

    // --- ТЕСТ 1: Проверка отдельных осей ---
    // Если этот тест упадет, значит ошибка в самих методах CreateRotationX/Y/Z или ToMatrix4F
    CheckAxis("X (Pitch)", testEuler.X, Matrix4F.CreateRotationPitch(testEuler.X), QuaternionF.FromEuler(new Vector3F(testEuler.X, 0, 0)));
    CheckAxis("Y (Yaw)",   testEuler.Y, Matrix4F.CreateRotationYaw(testEuler.Y),   QuaternionF.FromEuler(new Vector3F(0, testEuler.Y, 0)));
    CheckAxis("Z (Roll)",  testEuler.Z, Matrix4F.CreateRotationRoll(testEuler.Z),  QuaternionF.FromEuler(new Vector3F(0, 0, testEuler.Z)));

    // --- ТЕСТ 2: Проверка цепочки перемножения ---
    // Сравниваем прямую матрицу и матрицу через Кватернион
    Matrix4F matDirect = Matrix4F.CreateRotationPitch(testEuler.X) * 
                         Matrix4F.CreateRotationYaw(testEuler.Y) * 
                         Matrix4F.CreateRotationRoll(testEuler.Z);

    QuaternionF quat = QuaternionF.FromEuler(testEuler);
    Matrix4F matFromQuat = quat.ToMatrix4F();

    WL.Logger.Debug("\n--- СРАВНЕНИЕ МАТРИЦ (Порядок X*Y*Z) ---");
    PrintMatrixDiff(matDirect, matFromQuat);

    // --- ТЕСТ 3: Обратимость Эйлеров ---
    Vector3F restored = quat.ToEuler();
    WL.Logger.Debug($"\n--- ТЕСТ ОБРАТИМОСТИ ---");
    WL.Logger.Debug($"Original: {testEuler}");
    WL.Logger.Debug($"Restored: {restored}");
    WL.Logger.Debug($"Difference: {testEuler - restored}");

    WL.Logger.Debug("=== КОНЕЦ ТЕСТА ===");
}

private static void CheckAxis(string name, float angle, Matrix4F mat, QuaternionF q) {
    Matrix4F qMat = q.ToMatrix4F();
    float diff = 0;
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            diff += MathF.Abs(mat[i, j] - qMat[i, j]);

    if (diff > 0.01f) {
        WL.Logger.Error($"[ОСЬ {name}] ОШИБКА: Матрица и Кватернион не совпадают! Разница: {diff}");
    } else {
        WL.Logger.Debug($"[ОСЬ {name}] OK");
    }
}

private static void PrintMatrixDiff(Matrix4F a, Matrix4F b) {
    for (int row = 0; row < 3; row++) {
        string lineA = $"| {a[row, 0]:F3} {a[row, 1]:F3} {a[row, 2]:F3} |";
        string lineB = $"| {b[row, 0]:F3} {b[row, 1]:F3} {b[row, 2]:F3} |";
        WL.Logger.Debug($"{lineA}  vs  {lineB}");
    }
}

    public static int Stop(){
        try{
            WEE.Interface.Stop();
            WEE.Render.Stop();
            WEE.Window.Stop();
            
            WE.Engine.Stop();

            return 0;
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка при остановке Main!", e);
        }
    }

    public static Pipeline Pipeline = null!;

    public static void RefreshPipeline(){
        Pipeline.Clear();

        // ----------------------------------------------------------------------
        
        // Вызывается при обновлении сцены
        Stage<Action<DeltaTimeInfo, Scene?>> S_SceneUpdate = Pipeline.GetOrCreate<Action<DeltaTimeInfo, Scene?>>("SceneUpdate");
        S_SceneUpdate.Add("Default", (DTI, Scene) => {
            if(Scene != null){
                Scene.UpdateEngine(DTI);
            }
        });
        
        // Вызывается при рендере сцены
        Stage<Action<DeltaTimeInfo, Scene?, Vector2I, GLView?, GLView?, OpenGL, Camera, Color4B, double, string>> S_SceneRender = Pipeline.GetOrCreate<Action<DeltaTimeInfo, Scene?, Vector2I, GLView?, GLView?, OpenGL, Camera, Color4B, double, string>>("SceneRender");
        S_SceneRender.Add("Default", (DTI, Scene, ViewSize, SceneView, PickingView, Render, Camera, BackgroundColor, Time, Effect) => {
            if(WEE.Registry.HasMethods<WEE_OnRenderView>()){ WEE.Render.ViewRender(DTI, Scene, ViewSize, SceneView, PickingView, Render, Camera, BackgroundColor, Time, Effect); }
        });
        
        // ----------------------------------------------------------------------
        
        if(WEE.Registry.HasMethods<WEE_OnPipeline>()){
            WEE.Registry.RunFirstDelegate<WEE_OnPipeline, Action<Pipeline>>(true, Pipeline);
        }
    }
}