using System.Numerics;
using System.Reflection;
using ImGuiNET;
using NativeFileDialogSharp;
using WEO;

namespace WEE_Interface;

public static class I_Config{
    public static void Update(){
        if(!WEE.Interface.WindowConfigActive || WEE.Interface.Config == null){ return; }
        
        ImGui.SetNextWindowSize(new Vector2(450, 250), ImGuiCond.FirstUseEver);
        if(ImGui.Begin("Конфиг###Config", ref WEE.Interface.WindowConfigActive)){
            ImGui.TextDisabled($"Путь к конфигу: {WEE.Interface.ConfigPath}");
            
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            ImGui.TextDisabled("Информация об сцене");

            if(WEE.Interface.CurrentScene == null){
                ImGui.TextDisabled("Сцена не открыта");
            }else{
                if(!WEE.Interface.CurrentScene.__EditorInfo.HasValue){
                    ImGui.TextDisabled("У сцены нет информации");
                }else{
                    Scene.EditorInfo EditorInfo = WEE.Interface.CurrentScene.__EditorInfo!.Value;
                
                    ImGui.TextDisabled($"Создана: {new DateTime(EditorInfo.CreationTime):dd.MM.yyyy HH:mm:ss}");
                    ImGui.TextDisabled($"Сохранён: {(EditorInfo.LastSaveTime > 0 ? new DateTime(EditorInfo.LastSaveTime).ToString("dd.MM.yyyy HH:mm:ss") : "Ни разу не сохранялась")}");
                }
            }
            
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.InputText("Название проекта", ref WEE.Interface.Config.Name, 64);

            ImGui.Text("Путь к Game DLL");
            float __ButtonW = 35;
            float __Spacing = ImGui.GetStyle().ItemSpacing.X;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - __ButtonW - __Spacing);
            ImGui.InputText("##GameDLLPath", ref WEE.Interface.Config.GameDLLPath, 256);
            ImGui.SameLine();
            if(ImGui.Button("...###SelectGameDLL")){
                DialogResult? Result = Dialog.FileOpen("dll");
                if(Result.IsOk){
                    WEE.Interface.Config.GameDLLPath = Result.Path;

                    try{
                        Assembly GameAssembly = Assembly.LoadFrom(WEE.Interface.Config.GameDLLPath);
                        WEE.Registry.ResetAndReload(GameAssembly);
                    }catch(Exception e){
                        WL.Logger.Error($"ошибка при загрузке dll, todo {e.Message}\n{e.StackTrace}");
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            if(ImGui.Button("Сохранить", new Vector2(-1, 30))){
                WEE.Interface.Config.Save(WEE.Interface.ConfigPath);
                WL.Logger.Info("Конфиг сохранён!");
            }
        } ImGui.End();
    }
}