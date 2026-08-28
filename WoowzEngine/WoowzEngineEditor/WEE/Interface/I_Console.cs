using System.Numerics;
using ImGuiNET;

namespace WEE_Interface;

public static class I_Console{
    private static string __FullLogBuffer  = "";
    private static bool   __ScrollToBottom = false;
    
    public static void Update(){
        if(!WEE.Interface.WindowConsoleActive){ return; }

        if(ImGui.Begin("Консоль###Console", ref WEE.Interface.WindowConsoleActive)){

            if(ImGui.Button("Очистить")){ __FullLogBuffer = ""; }
            ImGui.SameLine();
            if(ImGui.Button("Тестовое сообщение")){ WL.Logger.Debug("Тестовое сообщение"); }
            
            ImGui.Separator();

            if(ImGui.BeginChild("ScrollingRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar)){
                ImGui.TextUnformatted(__FullLogBuffer);

                if(__ScrollToBottom){
                    ImGui.SetScrollHereY(1);
                    __ScrollToBottom = false;
                }
            }
            ImGui.EndChild();
        } ImGui.End();
    }

    public static void Start(){
        WL.Logger.CurrentLogger!.OnLog += (Type, Message) => {
            __FullLogBuffer += Message + "\n";
            if(__FullLogBuffer.Length > 100000){ __FullLogBuffer = __FullLogBuffer.Substring(__FullLogBuffer.Length - 50000); }
            __ScrollToBottom = true;
        };
    }
    
    private static unsafe int ConsoleCallback(ImGuiInputTextCallbackData* Data){
        if(__ScrollToBottom){
            Data -> CursorPos = Data -> BufTextLen;
            Data -> SelectionStart = Data -> BufTextLen;
            Data -> SelectionEnd = Data -> BufTextLen;
        }
        return 0;
    }
}