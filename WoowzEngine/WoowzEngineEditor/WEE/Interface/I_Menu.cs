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
        ImGUI GUI = WEE.D.ImGUI;

        GUI.MainMenuBar(() => {
            GUI.Menu("Файл", () => {
                if(ImGui.MenuItem("Новая сцена")){
                    CloseScene();
                    WEE.D.Selected.Scene = new Scene{ DoUpdate = false, DoEngineUpdate = true };
                }
                
                ImGui.Separator();

                if(ImGui.MenuItem("Открыть")){
                    OpenScene();
                }
                
                if(ImGui.MenuItem("Сохранить", "", false, WEE.D.Selected.Scene != null)){
                    SaveScene();
                }
                
                if(ImGui.MenuItem("Сохранить как", "", false, WEE.D.Selected.Scene != null)){
                    SaveSceneAs();
                }
                
                ImGui.Separator();
                
                if(ImGui.MenuItem("Закрыть сцену", "", false, WEE.D.Selected.Scene != null)){ CloseScene(); }
                if(ImGui.MenuItem("Выйти", "")){ WEE.D.Window.Close(); }
                
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

            if(WEE.D.Selected.Scene != null){
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.SetNextItemWidth(300);
                ImGui.InputText("##SceneNameInput", ref WEE.D.Selected.Scene.Name, 128);
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Название сцены"); }
                
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();
            }
            
            string MenuText = $"E-FPS: {WEE.D.Time.Engine.DTI.FPS:F1}";
            System.Numerics.Vector2 TextSize = ImGui.CalcTextSize(MenuText);
            ImGui.SameLine(ImGui.GetWindowWidth() - TextSize.X - 10);
            ImGui.TextDisabled(MenuText);
            if(ImGui.IsItemHovered()){ ImGui.SetTooltip("FPS стороны редактора"); }
        });
    }
    
    private static void CloseScene(){
        WEE.D.Selected.Scene?.Clear(true);
        WEE.D.Selected.Entity = null;
        WEE.D.Selected.Scene = null;

        __SceneFilePath = null!;
        
        I_View.BackgroundColor     = new Color4B(200, 200, 200);
        WEE.D.View.Camera.Position = new Vector3F();
        WEE.D.View.Camera.Rotation = new Vector3F();
        I_View.Is2DView             = false;
        WEE.D.View.CameraSpeed      = 1;
        WEE.D.View.Camera.Far      = 1000;
    }

    private static void SaveSceneAs(){
        if(WEE.D.Selected.Scene == null){ return; }
        
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
        if(WEE.D.Selected.Scene == null){ return; }
        if(string.IsNullOrEmpty(__SceneFilePath)){
            SaveSceneAs();
        }else{
            __SaveScene(__SceneFilePath);   
        }
    }
    
    private static void __SaveScene(string Path) {
        if(WEE.D.Selected.Scene == null){ return; }
        try{
            Scene.EditorInfo EditorInfo = WEE.D.Selected.Scene.__EditorInfo ?? new Scene.EditorInfo();

            EditorInfo.BackgroundColor   =  I_View.BackgroundColor;
            EditorInfo.CameraPosition    =  WEE.D.View.Camera.Position;
            EditorInfo.CameraRotation    =  WEE.D.View.Camera.Rotation;
            EditorInfo.CameraPerspective = !I_View.Is2DView;
            EditorInfo.CameraSpeed       =  WEE.D.View.CameraSpeed;
            EditorInfo.CameraFar         =  WEE.D.View.Camera.Far;
            EditorInfo.LastSaveTime      =  DateTime.Now.Ticks;
                
            if(EditorInfo.CreationTime == 0){ EditorInfo.CreationTime = DateTime.Now.Ticks; }

            WEE.D.Selected.Scene.__EditorInfo = EditorInfo;
            
            string JSON = WEE.D.Selected.Scene.ToJSON();
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
            
            WEE.D.Selected.Scene = Scene.FromJSON(JSON);

            WEE.D.Selected.Scene.DoUpdate       = false;
            WEE.D.Selected.Scene.DoEngineUpdate = true;
            
            __SceneFilePath = Path;

            if(WEE.D.Selected.Scene.__EditorInfo.HasValue){
                I_View.BackgroundColor     =  WEE.D.Selected.Scene.__EditorInfo.Value.BackgroundColor;
                WEE.D.View.Camera.Position =  WEE.D.Selected.Scene.__EditorInfo.Value.CameraPosition;
                WEE.D.View.Camera.Rotation =  WEE.D.Selected.Scene.__EditorInfo.Value.CameraRotation;
                I_View.Is2DView            = !WEE.D.Selected.Scene.__EditorInfo.Value.CameraPerspective;
                WEE.D.View.CameraSpeed     =  WEE.D.Selected.Scene.__EditorInfo.Value.CameraSpeed;
                WEE.D.View.Camera.Far      =  WEE.D.Selected.Scene.__EditorInfo.Value.CameraFar;
                
                if(WEE.D.Selected.Scene.__EditorInfo.Value.CreationTime == 0){
                    WEE.D.Selected.Scene.__EditorInfo = WEE.D.Selected.Scene.__EditorInfo.Value with{ CreationTime = DateTime.Now.Ticks };
                }
            }
            
            WEE.Prefs.AddRecentScene(Path);
            WL.Logger.Info($"Сцена загружена: {Path}");
            
            WEE.Registry.RunMethods<WEE_OnSceneLoad>(true, WEE.D.Selected.Scene);
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