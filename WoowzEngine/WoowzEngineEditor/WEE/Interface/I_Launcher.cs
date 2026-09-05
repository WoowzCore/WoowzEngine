using System.Numerics;
using ImGuiNET;
using NativeFileDialogSharp;
using WEEO;
using WLO.Interface;

namespace WEE_Interface;

public static class I_Launcher{
    private static string __NewProjectName      = "New Project";
    private const  string __ConfigFileExtension = "wee_config";
    
    public static void Update(){
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));

        ImGUI GUI = WEE.Interface.ImGUI;

        GUI.Window("Загрузчик", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse, () => {
            if(!string.IsNullOrEmpty(WEE.Prefs.LastConfigPath) && File.Exists(WEE.Prefs.LastConfigPath)){
                ImGui.TextColored(new Vector4(0.4f, 1, 0.4f, 1), "Последний проект:");
                if(ImGui.Button($"{Path.GetFileName(WEE.Prefs.LastConfigPath)}###RecentProject", new Vector2(-1, 40))){
                    WEE.Interface.ConfigPath = WEE.Prefs.LastConfigPath;
                    WEE.Interface.Config = EditorConfig.Load(WEE.Interface.ConfigPath);
                    OnProjectLoaded();
                }
                ImGui.Separator();
            }
            
            ImGui.Text("Добро пожаловать в WoowzEngineEditor");
            ImGui.Separator();
            ImGui.Spacing();
            
            ImGui.Text("Создать новый проект");
            ImGui.InputText("Название", ref __NewProjectName, 64);
            if(ImGui.Button("Создать и выбрать папку", new Vector2(-1, 30))){
                CreateNewProject();        
            }
            
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            ImGui.Text("Открыть существующий");
            if(ImGui.Button("Выбрать файл конфига", new Vector2(-1, 30))){
                OpenExistingProject();
            }
        });
    }
    
    private static void CreateNewProject(){
        DialogResult? Result = Dialog.FileSave(__ConfigFileExtension);
        if(Result.IsOk){
            string __ConfigPath = Result.Path;

            if(!__ConfigPath.EndsWith($".{__ConfigFileExtension}", StringComparison.OrdinalIgnoreCase)){
                __ConfigPath += $".{__ConfigFileExtension}";
            }

            WEE.Interface.Config = new EditorConfig(){
                Name = __NewProjectName
            };
            
            WEE.Interface.Config.Save(__ConfigPath);
            
            OnProjectLoaded();    
        }
    }

    private static void OpenExistingProject(){
        DialogResult? Result = Dialog.FileOpen(__ConfigFileExtension);
        if(Result.IsOk){
            WEE.Interface.ConfigPath = Result.Path;
            WEE.Interface.Config = EditorConfig.Load(WEE.Interface.ConfigPath);
            OnProjectLoaded();
        }
    }

    private static void OnProjectLoaded(){
        WEE.Interface.__IsProjectLoaded = true;
        WEE.Prefs.LastConfigPath = WEE.Interface.ConfigPath;
        WEE.Prefs.Save();
    }
}