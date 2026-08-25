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

            float FooterHeightToReverse = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

            if(__ScrollToBottom){ ImGui.SetKeyboardFocusHere(); }
            
            unsafe{
                ImGui.InputTextMultiline("##FulLog", ref __FullLogBuffer, (uint)__FullLogBuffer.Length + 1, new Vector2(-1, -FooterHeightToReverse), ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.CallbackAlways, ConsoleCallback);

                if(__ScrollToBottom && ImGui.IsItemActive()){
                    __ScrollToBottom = false;
                }
            }
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