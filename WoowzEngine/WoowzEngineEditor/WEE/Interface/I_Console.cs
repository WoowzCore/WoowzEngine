using System.Numerics;
using ImGuiNET;
using WLO.Interface;

namespace WEE_Interface;

public static class I_Console{
    private static string __FullLogBuffer  = "";
    private static bool   __ScrollToBottom = false;

    private static int  __UnreadCount = 0;
    private static bool __InFocused   = false;
    
    public static void Update(){
        if(!WEE.Interface.WindowConsoleActive){ return; }

        ImGUI GUI = WEE.Interface.ImGUI;
        
        string WindowTitle = (__UnreadCount > 0 ? $"Консоль [{__UnreadCount}]" : "Консоль") + "###Console";

        bool Showen = GUI.Window(WindowTitle, ref WEE.Interface.WindowConsoleActive, () => {
            if(ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows)){
                __UnreadCount = 0;
                __InFocused = true;
            }else{
                __InFocused = false;
            }
            
            if(ImGui.Button("Очистить")){ __FullLogBuffer = ""; __UnreadCount = 0; }
            ImGui.SameLine();
            if(ImGui.Button("Тестовое сообщение")){ WL.Logger.Debug("Тестовое сообщение"); }
            
            ImGui.Separator();

            GUI.Child("ScrollingRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar, () => {
                ImGui.TextUnformatted(__FullLogBuffer);

                if(__ScrollToBottom){
                    ImGui.SetScrollHereY(1);
                    __ScrollToBottom = false;
                }
            });
        });

        if(!Showen){ __InFocused = false; }
    }

    public static void Start(){
        WL.Logger.CurrentLogger!.OnLog += (Type, Message) => {
            __FullLogBuffer += Message + "\n";

            if(!__InFocused){
                __UnreadCount++;
            }
            
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