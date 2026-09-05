using System.Diagnostics;
using ImGuiNET;
using NativeFileDialogSharp;
using WEI_Attribute;
using WEO;
using WLO.Interface;
using WLO.Math;

namespace WEE_Interface;

public static class I_Menu{
    public static string __SceneFilePath       = null!;
    private const string __SceneFileExtension  = "we_scene";
    private const string __PrefabFileExtension = "we_prefab";
    
    public static void Update(){
        ImGUI GUI = WEE.Interface.ImGUI;

        GUI.MainMenuBar(() => {
            GUI.Menu("Файл", () => {
                if(ImGui.MenuItem("Новая сцена")){
                    CloseScene();
                    WEE.Interface.CurrentScene = new Scene();
                    WEE.Interface.CurrentScene.DoUpdate = false;
                    WEE.Interface.CurrentScene.DoEngineUpdate = true;
                }
                
                ImGui.Separator();

                if(ImGui.MenuItem("Открыть")){
                    OpenScene();
                }
                
                if(ImGui.MenuItem("Сохранить", "", false, WEE.Interface.CurrentScene != null)){
                    SaveScene();
                }
                
                if(ImGui.MenuItem("Сохранить как", "", false, WEE.Interface.CurrentScene != null)){
                    SaveSceneAs();
                }
                
                ImGui.Separator();
                
                if(ImGui.MenuItem("Закрыть сцену", "", false, WEE.Interface.CurrentScene != null)){ CloseScene(); }
                if(ImGui.MenuItem("Выйти", "")){ WEE.Window.MainWindow.Close(); }
                
                ImGui.Separator();

                foreach(string ScenePath in WEE.Prefs.RecentScenes.ToList()){
                    if(ImGui.MenuItem(Path.GetFileName(ScenePath))){
                        __LoadScene(ScenePath);
                    }
                    if(ImGui.IsItemHovered()){ ImGui.SetTooltip(ScenePath); }
                }
            });

            GUI.Menu("Редактировать", false, () => {
                if(ImGui.MenuItem("Отменить")){}
                if(ImGui.MenuItem("Вернуть")){}
            });

            GUI.Menu("Окно", () => {
                ImGui.MenuItem("Просмотр сцены", "", ref WEE.Interface.WindowViewActive);
                ImGui.MenuItem("Просмотр", "", ref WEE.Interface.WindowInspectorActive);
                ImGui.MenuItem("Иерархия", "", ref WEE.Interface.WindowHierarchyActive);
                ImGui.MenuItem("Ресурсы", "", ref WEE.Interface.WindowAssetsActive);
                ImGui.MenuItem("Консоль", "", ref WEE.Interface.WindowConsoleActive);
                ImGui.MenuItem("Конфиг", "", ref WEE.Interface.WindowConfigActive);
                
                ImGui.Separator();

                ImGui.MenuItem("ImGUI Demo", "", ref WEE.Interface.WindowImGUIDemoActive);
            });

            GUI.Menu("Остальное", () => {
                if(ImGui.MenuItem("Открыть GitHub...")){ Process.Start(new ProcessStartInfo("https://github.com/WoowzCore/WoowzEngine"){ UseShellExecute = true }); }
            });

            if(WEE.Interface.CurrentScene != null){
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.SetNextItemWidth(300);
                ImGui.InputText("##SceneNameInput", ref WEE.Interface.CurrentScene.Name, 128);
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Название сцены"); }
                
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
            }
            
            string MenuText = $"E-FPS: {WEE.Cycle.Engine_DTI.FPS:F1}";
            System.Numerics.Vector2 TextSize = ImGui.CalcTextSize(MenuText);
            ImGui.SameLine(ImGui.GetWindowWidth() - TextSize.X - 10);
            ImGui.TextDisabled(MenuText);
            if(ImGui.IsItemHovered()){ ImGui.SetTooltip("FPS стороны редактора"); }
        });
    }
    
    private static void CloseScene(){
        WEE.Interface.CurrentScene?.Clear(true);
        WEE.Interface.CurrentEntity = null;
        WEE.Interface.CurrentScene = null;

        __SceneFilePath = null!;
        
        I_View.BackgroundColor         = new Color4B(200, 200, 200);
        WEE.Editor.ViewCamera.Position = new Vector3F();
        WEE.Editor.ViewCamera.Rotation = new Vector3F();
        I_View.Is2DView                = false;
        WEE.Editor.CameraSpeed         = 1;
        WEE.Editor.ViewCamera.Far      = 1000;
    }

    private static void SaveSceneAs(){
        if(WEE.Interface.CurrentScene == null){ return; }
        
        DialogResult? Result = Dialog.FileSave(__SceneFileExtension);

        if(Result.IsOk){
            string Path = Result.Path;
            if(!Path.EndsWith($".{__SceneFileExtension}", StringComparison.OrdinalIgnoreCase)){ Path += $".{__SceneFileExtension}"; }
            
            __SaveScene(Path);
        }
    }

    private static void OpenScene(){
        DialogResult? Result = Dialog.FileOpen(__SceneFileExtension);

        if(Result.IsOk){
            __LoadScene(Result.Path);
        }
    }

    private static void SaveScene(){
        if(WEE.Interface.CurrentScene == null){ return; }
        if(string.IsNullOrEmpty(__SceneFilePath)){
            SaveSceneAs();
        }else{
            __SaveScene(__SceneFilePath);   
        }
    }
    
    private static void __SaveScene(string Path) {
        if(WEE.Interface.CurrentScene == null){ return; }
        try{
            Scene.EditorInfo EditorInfo = WEE.Interface.CurrentScene.__EditorInfo ?? new Scene.EditorInfo();

            EditorInfo.BackgroundColor   =  I_View.BackgroundColor;
            EditorInfo.CameraPosition    =  WEE.Editor.ViewCamera.Position;
            EditorInfo.CameraRotation    =  WEE.Editor.ViewCamera.Rotation;
            EditorInfo.CameraPerspective = !I_View.Is2DView;
            EditorInfo.CameraSpeed       =  WEE.Editor.CameraSpeed;
            EditorInfo.CameraFar         =  WEE.Editor.ViewCamera.Far;
            EditorInfo.LastSaveTime      =  DateTime.Now.Ticks;
                
            if(EditorInfo.CreationTime == 0){ EditorInfo.CreationTime = DateTime.Now.Ticks; }

            WEE.Interface.CurrentScene.__EditorInfo = EditorInfo;
            
            string JSON = WEE.Interface.CurrentScene.ToJSON();
            File.WriteAllText(Path, JSON);
            __SceneFilePath = Path;
            WL.Logger.Info($"Сцена сохранена: {Path}");
        }catch (Exception e){
            WL.Logger.Error($"Ошибка сохранения:", e);
        }
    }

    private static void __LoadScene(string Path){
        try{
            if(!File.Exists(Path)){ return; }
            string JSON = File.ReadAllText(Path);
            
            CloseScene();
            
            WEE.Interface.CurrentScene = Scene.FromJSON(JSON);

            WEE.Interface.CurrentScene.DoUpdate       = false;
            WEE.Interface.CurrentScene.DoEngineUpdate = true;
            
            __SceneFilePath = Path;

            if(WEE.Interface.CurrentScene.__EditorInfo.HasValue){
                I_View.BackgroundColor         =  WEE.Interface.CurrentScene.__EditorInfo.Value.BackgroundColor;
                WEE.Editor.ViewCamera.Position =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraPosition;
                WEE.Editor.ViewCamera.Rotation =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraRotation;
                I_View.Is2DView                = !WEE.Interface.CurrentScene.__EditorInfo.Value.CameraPerspective;
                WEE.Editor.CameraSpeed         =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraSpeed;
                WEE.Editor.ViewCamera.Far      =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraFar;
                
                if(WEE.Interface.CurrentScene.__EditorInfo.Value.CreationTime == 0){
                    WEE.Interface.CurrentScene.__EditorInfo = WEE.Interface.CurrentScene.__EditorInfo.Value with{ CreationTime = DateTime.Now.Ticks };
                }
            }
            
            WEE.Prefs.AddRecentScene(Path);
            WL.Logger.Info($"Сцена загружена: {Path}");
            
            WEE.Registry.RunMethods<WEE_OnSceneLoad>(true, WEE.Interface.CurrentScene);
        }catch(Exception e){
            WL.Logger.Error($"Ошибка загрузки:", e);
        }
    }

    public static void SaveEntityAsPrefab(Entity Entity){
        DialogResult? Result = NativeFileDialogSharp.Dialog.FileSave(__PrefabFileExtension);
        if(Result.IsOk){
            string Path = Result.Path;
            if(!Path.EndsWith($".{__PrefabFileExtension}")){ Path += $".{__PrefabFileExtension}"; }

            File.WriteAllText(Path, Prefab.FromEntity(Entity).ToJSON());
            
            WL.Logger.Info($"Prefab создан: {Path}");
        }
    }
}