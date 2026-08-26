using System.Diagnostics;
using ImGuiNET;
using NativeFileDialogSharp;
using WEO;
using WLO.Math;

namespace WEE_Interface;

public static class I_Menu{
    public static string __SceneFilePath      = null!;
    private const string __SceneFileExtension = "weescene";
    
    public static void Update(){
        if(ImGui.BeginMainMenuBar()){
            if(ImGui.BeginMenu("Файл")){
                if(ImGui.MenuItem("Новая сцена")){
                    CloseScene();
                    WEE.Interface.CurrentScene = new Scene();
                }
                
                ImGui.Separator();

                if(ImGui.MenuItem("Открыть", "Ctrl+O")){
                    OpenScene();
                }
                
                if(ImGui.MenuItem("Сохранить", "Ctrl+S", false, WEE.Interface.CurrentScene != null)){
                    SaveScene();
                }
                
                if(ImGui.MenuItem("Сохранить как", "Ctrl+Shift+S", false, WEE.Interface.CurrentScene != null)){
                    SaveSceneAs();
                }
                
                ImGui.Separator();
                
                if(ImGui.MenuItem("Закрыть сцену", "", false, WEE.Interface.CurrentScene != null)){ CloseScene(); }
                if(ImGui.MenuItem("Выйти", "Alt+F4")){ WEE.Window.MainWindow.Close(); }
                
                ImGui.Separator();

                foreach(string ScenePath in WEE.Prefs.RecentScenes.ToList()){
                    if(ImGui.MenuItem(Path.GetFileName(ScenePath))){
                        __LoadScene(ScenePath);
                    }
                    if(ImGui.IsItemHovered()){ ImGui.SetTooltip(ScenePath); }
                }
                
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Редактировать", false)){
                if(ImGui.MenuItem("Отменить", "Ctrl+Z")){  }
                if(ImGui.MenuItem("Вернуть", "Ctrl+Y")){  }
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Окно")){
                ImGui.MenuItem("Просмотр сцены", "", ref WEE.Interface.WindowViewActive);
                ImGui.MenuItem("Просмотр", "", ref WEE.Interface.WindowInspectorActive);
                ImGui.MenuItem("Иерархия", "", ref WEE.Interface.WindowHierarchyActive);
                ImGui.MenuItem("Ресурсы", "", ref WEE.Interface.WindowAssetsActive);
                ImGui.MenuItem("Консоль", "", ref WEE.Interface.WindowConsoleActive);
                ImGui.MenuItem("Конфиг", "", ref WEE.Interface.WindowConfigActive);
                
                ImGui.Separator();

                ImGui.MenuItem("ImGUI Demo", "", ref WEE.Interface.WindowImGUIDemoActive);
                
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Помощь")){
                if(ImGui.MenuItem("Открыть GitHub...")){ Process.Start(new ProcessStartInfo("https://github.com/WoowzCore/WoowzEngine"){ UseShellExecute = true }); }
                ImGui.EndMenu();
            }

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
            ImGui.EndMainMenuBar();
        }
    }
    
    private static void CloseScene(){
        WEE.Interface.CurrentScene?.Clear();
        WEE.Interface.CurrentEntity = null;
        WEE.Interface.CurrentScene = null;

        I_View.BackgroundColor         = new Color4B(200, 200, 200);
        WEE.Editor.ViewCamera.Position = new Vector3F();
        WEE.Editor.ViewCamera.Rotation = new Vector3F();
        I_View.Is2DView                = false;
        WEE.Editor.CameraSpeed         = 1;
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
            WEE.Interface.CurrentScene.__EditorInfo = new Scene.EditorInfo{
                BackgroundColor = I_View.BackgroundColor,
                CameraPosition = WEE.Editor.ViewCamera.Position,
                CameraRotation = WEE.Editor.ViewCamera.Rotation,
                CameraPerspective = !I_View.Is2DView,
                CameraSpeed = WEE.Editor.CameraSpeed
            };
            
            string JSON = WEE.Interface.CurrentScene.SaveToJSON();
            File.WriteAllText(Path, JSON);
            __SceneFilePath = Path;
            WL.Logger.Info($"Сцена сохранена: {Path}");
        }catch (Exception e){
            WL.Logger.Error($"Ошибка сохранения: {e.Message}");
        }
    }

    private static void __LoadScene(string Path){
        try{
            if(!File.Exists(Path)){ return; }
            string JSON = File.ReadAllText(Path);
            
            CloseScene();
            
            WEE.Interface.CurrentScene = Scene.LoadFromJSON(JSON);
            __SceneFilePath = Path;

            if(WEE.Interface.CurrentScene.__EditorInfo.HasValue){
                I_View.BackgroundColor         =  WEE.Interface.CurrentScene.__EditorInfo.Value.BackgroundColor;
                WEE.Editor.ViewCamera.Position =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraPosition;
                WEE.Editor.ViewCamera.Rotation =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraRotation;
                I_View.Is2DView                = !WEE.Interface.CurrentScene.__EditorInfo.Value.CameraPerspective;
                WEE.Editor.CameraSpeed         =  WEE.Interface.CurrentScene.__EditorInfo.Value.CameraSpeed;
            }
            
            WEE.Prefs.AddRecentScene(Path);
            WL.Logger.Info($"Сцена загружена: {Path}");
        }catch(Exception e){
            WL.Logger.Error($"Ошибка загрузки: {e.Message + "\n" + e.StackTrace}");
        }
    }
}