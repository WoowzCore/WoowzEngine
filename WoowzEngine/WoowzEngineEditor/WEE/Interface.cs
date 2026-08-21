using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using WEO_Component;
using WEO;
using WLO;
using WLO.Interface;
using WLO.Math;

namespace WEE;

public static class Interface{
    public static void Start(){
        ImGUI = new GLImGUI(WE.Render.API, true);

        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        ImGUI.IO.ConfigWindowsMoveFromTitleBarOnly = true;
        
        Start_Console();
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------

    public static GLImGUI ImGUI{ get; private set; } = null!;

    // ----------------------------------------------------------------------

    private static bool __ShowSceneView = true;
    private static bool __ShowInspector = true;
    private static bool __ShowHierarchy = true;
    private static bool __ShowAssets    = true;
    private static bool __ShowConsole   = true;

    private static bool __ShowImGUIDemo = false;
    
    private static void Update_Menu(){
        if(ImGui.BeginMainMenuBar()){
            if(ImGui.BeginMenu("Файл")){
                if(ImGui.MenuItem("Выйти", "Alt+F4")){ WEE.Window.MainWindow.Close(); }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Редактировать")){
                if(ImGui.MenuItem("Отменить", "Ctrl+Z")){  }
                if(ImGui.MenuItem("Вернуть", "Ctrl+Y")){  }
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Окно")){
                ImGui.MenuItem("Просмотр сцены", "", ref __ShowSceneView);
                ImGui.MenuItem("Просмотр", "", ref __ShowInspector);
                ImGui.MenuItem("Иерархия", "", ref __ShowHierarchy);
                ImGui.MenuItem("Ресурсы", "", ref __ShowAssets);
                ImGui.MenuItem("Консоль", "", ref __ShowConsole);
                
                ImGui.Separator();

                ImGui.MenuItem("ImGUI Demo", "", ref __ShowImGUIDemo);
                
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Помощь")){
                if(ImGui.MenuItem("Открыть GitHub...")){ Process.Start(new ProcessStartInfo("https://github.com/WoowzCore/WoowzEngine"){ UseShellExecute = true }); }
                ImGui.EndMenu();
            }

            string MenuText = $"E-FPS: {WEE.Cycle.Engine_DTI.FPS:F1}";
            System.Numerics.Vector2 TextSize = ImGui.CalcTextSize(MenuText);
            ImGui.SameLine(ImGui.GetWindowWidth() - TextSize.X - 10);
            ImGui.TextDisabled(MenuText);
            
            ImGui.EndMainMenuBar();
        }
    }
    
    // ----------------------------------------------------------------------
    
    public static bool FocusSceneView{ get; private set; }

    public static Vector2I SceneViewSize{ get; private set; }
    
    private static void Update_SceneView(){
        if(!__ShowSceneView){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin($"Просмотр сцены ({SceneViewSize.W}x{SceneViewSize.H}), R-FPS: {WEE.Cycle.Render_DTI.FPS:F1}###SceneView", ref __ShowSceneView);

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            
            System.Numerics.Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            SceneViewSize = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);
            
            ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ResultTexture!.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
        
        ImGui.End();
    }

    // ----------------------------------------------------------------------
    
    private static void Update_Inspector(){
        if(!__ShowInspector){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Просмотр", ref __ShowInspector);
            if(SelectedEntity == null){
                ImGui.TextDisabled("Выберите объект в иерархии...");
            }else{
                string Name = SelectedEntity.Name;
                if(ImGui.InputText("Название", ref Name, 100)){
                    SelectedEntity.Name = Name;
                }
                
                ImGui.Separator();

                if(ImGui.CollapsingHeader("Положение", ImGuiTreeNodeFlags.DefaultOpen)){
                    System.Numerics.Vector3 Position = new Vector3(SelectedEntity.Transform.Position.X, SelectedEntity.Transform.Position.Y, SelectedEntity.Transform.Position.Z);

                    if(ImGui.DragFloat3("Позиция", ref Position, 0.1f)){
                        SelectedEntity.Transform.Position = new Vector3F(Position.X, Position.Y, Position.Z);
                        SelectedEntity.SetTransformDirty();
                    }
                    
                    System.Numerics.Vector3 Scale = new Vector3(SelectedEntity.Transform.Scale.X, SelectedEntity.Transform.Scale.Y, SelectedEntity.Transform.Scale.Z);

                    if(ImGui.DragFloat3("Размер", ref Scale, 0.1f)){
                        SelectedEntity.Transform.Scale = new Vector3F(Scale.X, Scale.Y, Scale.Z);
                        SelectedEntity.SetTransformDirty();
                    }
                    
                    System.Numerics.Vector3 Rotation = new Vector3(SelectedEntity.Transform.Rotation.X, SelectedEntity.Transform.Rotation.Y, SelectedEntity.Transform.Rotation.Z);

                    if(ImGui.DragFloat3("Поворот", ref Rotation, 0.1f)){
                        SelectedEntity.Transform.Rotation = new Vector3F(Rotation.X, Rotation.Y, Rotation.Z);
                        SelectedEntity.SetTransformDirty();
                    }
                }
                
                ImGui.Separator();
            }
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------

    public static Entity? SelectedEntity = null!;
    
    private static void Update_Hierarchy(){
        if(!__ShowHierarchy){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Иерархия###Hierarchy", ref __ShowHierarchy);

            if(WEE.Render.ActiveScene == null){
                ImGui.Text("Нет активной сцены");
                ImGui.End();
                return;
            }

            List<Entity> AllEntities = WEE.Render.ActiveScene.AllEntity.ToList();
            ImGui.TextDisabled($"Всего: {AllEntities.Count}, Корней: {WEE.Render.ActiveScene.Roots.Count()}");

            ImGui.BeginChild("HierarchyList");
            
                foreach(Entity Entity in WEE.Render.ActiveScene.Roots){
                    if(Entity.Node.Parent == null){
                        DrawEntityNode(Entity);
                    }
                }

                if(ImGui.IsMouseDown(0) && ImGui.IsWindowHovered()){
                    SelectedEntity = null;
                }

                if(ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems)){
                    if(ImGui.MenuItem("Создать пустой Entity")){
                        Entity NewEntity = new Entity();
                        CMeshRenderer __CMESHRENDERER = NewEntity.AddComponent<CMeshRenderer>();
                        __CMESHRENDERER.Mesh = WEE.Render.__MESH;
                        __CMESHRENDERER.Program = WEE.Render.__PROGRAM;
                        WEE.Render.ActiveScene.Add(NewEntity);
                        SelectedEntity = NewEntity;
                    }
                    ImGui.EndPopup();
                }
                
            ImGui.EndChild();
        
        ImGui.End();
    }

    private static void DrawEntityNode(Entity Entity){
        ImGuiTreeNodeFlags Flags = (SelectedEntity == Entity ? ImGuiTreeNodeFlags.Selected : 0);

        Flags |= ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        if(Entity.Node.Children.Count == 0){
            Flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        bool Opened = ImGui.TreeNodeEx($@"{Entity.Name}###{Entity.GetHashCode()}", Flags);

        if(ImGui.IsItemClicked()){
            SelectedEntity = Entity;
        }

        if(Opened && Entity.Node.Children.Count > 0){
            foreach(HierarchyNode<Entity> ChildNode in Entity.Node.Children){
                DrawEntityNode(ChildNode.Owner);
            }
            ImGui.TreePop();
        }
    }
    
    // ----------------------------------------------------------------------
    
    private static void Update_Assets(){
        if(!__ShowAssets){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Ресурсы", ref __ShowAssets);
            ImGui.Text("FILES");
        ImGui.End();
    }

    // ----------------------------------------------------------------------

    private struct __LogEntry{
        public string  Message;
        public Color4B Color;
    }

    private static readonly List<__LogEntry> __ConsoleEntries = [];
    private const           int              __MaxLogs        = 500;
    private static          bool             __ScrollToBottom = true;
    
    private static void Start_Console(){
        WL.Logger.CurrentLogger!.OnLog += (Type, Message) => {
            __ConsoleEntries.Add(new __LogEntry{ Message = Message, Color = new Color4B(255, 255, 255)});
            if(__ConsoleEntries.Count > __MaxLogs){ __ConsoleEntries.RemoveAt(0); }
            __ScrollToBottom = true;
        };
    }
    
    private static void Update_Console(){
        if(!__ShowConsole){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Консоль", ref __ShowConsole);

            if(ImGui.Button("Очистить")){ __ConsoleEntries.Clear(); }
            ImGui.SameLine();
            if(ImGui.Button("Тестовое сообщение")){ WL.Logger.Debug("Тестовое сообщение"); }
            ImGui.SameLine();
            ImGui.Text($"Кол-во: {__ConsoleEntries.Count}");
            
            ImGui.Separator();

            float FooterHeightToReverse = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0, -FooterHeightToReverse), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

                foreach(__LogEntry Entry in __ConsoleEntries){
                    ImGui.TextColored(new System.Numerics.Vector4(Entry.Color.R, Entry.Color.G, Entry.Color.B, Entry.Color.A), Entry.Message);
                }

                if(__ScrollToBottom){
                    ImGui.SetScrollHereY(1);
                    __ScrollToBottom = false;
                }
            
            ImGui.EndChild();
            
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);

        Update_Menu();
        
        ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        
        Update_SceneView();
        
        Update_Inspector();

        Update_Hierarchy();
        
        Update_Assets();
        
        Update_Console();

        if(__ShowImGUIDemo){ ImGui.ShowDemoWindow(ref __ShowImGUIDemo); }

        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}