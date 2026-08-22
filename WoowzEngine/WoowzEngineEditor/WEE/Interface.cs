using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using NativeFileDialogSharp;
using WEEO;
using WEI;
using WEO_Component;
using WEO;
using WLO;
using WLO.Interface;
using WLO.Math;
using WoowzLib.Interface.ImGUI;

namespace WEE;

// todo, NativeFileDialogSharp

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

    public static Scene?  ActiveScene;
    
    private static bool __ShowSceneView = true;
    private static bool __ShowInspector = true;
    private static bool __ShowHierarchy = true;
    private static bool __ShowAssets    = true;
    private static bool __ShowConsole   = true;

    private static bool __ShowImGUIDemo = false;

    public static string __SceneFilePath = null!;
    private const  string __SceneFileExtension = "weescene";
    
    private static void Update_Menu(){
        if(ImGui.BeginMainMenuBar()){
            if(ImGui.BeginMenu("Файл")){
                if(ImGui.MenuItem("Новая сцена")){
                    ActiveScene?.Clear();
                    SelectedEntity = null;
                    ActiveScene = new Scene();
                }
                
                ImGui.Separator();

                if(ImGui.MenuItem("Открыть", "Ctrl+O")){
                    OpenScene();
                }
                
                if(ImGui.MenuItem("Сохранить", "Ctrl+S")){
                    SaveScene();
                }
                
                if(ImGui.MenuItem("Сохранить как", "Ctrl+Shift+S")){
                    SaveSceneAs();
                }
                
                ImGui.Separator();
                
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

    private static void SaveSceneAs(){
        DialogResult? Result = Dialog.FileSave(__SceneFileExtension);

        if(Result.IsOk){
            string Path = Result.Path;
            if(!Path.EndsWith($".{__SceneFileExtension}")){ Path += $".{__SceneFileExtension}"; }
            
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
        if(string.IsNullOrEmpty(__SceneFilePath)){
            SaveSceneAs();
        }else{
            __SaveScene(__SceneFilePath);   
        }
    }
    
    private static void __SaveScene(string Path) {
        if(ActiveScene == null){ return; }
        try{
            string JSON = ActiveScene.SaveToJSON();
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
            ActiveScene?.Clear();
            ActiveScene = Scene.LoadFromJSON(JSON);

            // MEGA TODO!
            if(ActiveScene != null!){
                foreach(var Entity in ActiveScene.AllEntity.ToList()){
                    var __MESHRENDERERS = Entity.GetAllComponents().OfType<CMeshRenderer>().ToList();
                    
                    foreach(var Renderer in __MESHRENDERERS){
                        Renderer.Mesh = WEE.Render.__MESH;
                        Renderer.Program = WEE.Render.__PROGRAM;
                    }
                }
            }
            
            __SceneFilePath = Path;
            SelectedEntity = null;
            WL.Logger.Info($"Сцена загружена: {Path}");
        }catch (Exception e){
            WL.Logger.Error($"Ошибка загрузки: {e.Message}");
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

            if(ActiveScene != null){
                ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ResultTexture!.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
            }
            
        ImGui.End();
    }

    // ----------------------------------------------------------------------
    
    private static void Update_Inspector(){
        if(!__ShowInspector){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        if(ImGui.Begin("Просмотр###Inspector", ref __ShowInspector)){
            try{
                if(SelectedEntity == null){
                    ImGui.TextDisabled("Выберите объект в иерархии...");
                }
                else{
                    string Name = SelectedEntity.Name;
                    if(ImGui.InputText("Название", ref Name, 100)){
                        SelectedEntity.Name = Name;
                    }

                    ImGui.Separator();

                    if(ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen)){
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

                    foreach(Component Component in SelectedEntity.GetAllComponents().ToList()){
                        bool ComponentOpen = ImGui.CollapsingHeader(Component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen);

                        if(ImGui.BeginPopupContextItem($"{Component.GetHashCode()}")){
                            if(ImGui.MenuItem("Удалить компонент")){
                                SelectedEntity.RemoveComponent(Component);
                            }

                            ImGui.EndPopup();
                        }

                        if(ComponentOpen){
                            DrawComponentFields(Component);
                        }
                    }

                    ImGui.Separator();

                    if(ImGui.Button("Добавить компонент", new Vector2(-1, 0))){
                        ImGui.OpenPopup("AddComponentPopup");
                    }

                    if(ImGui.BeginPopup("AddComponentPopup")){
                        if(ImGui.MenuItem("CMeshRenderer")){
                            CMeshRenderer __CMESHREDNDERER = SelectedEntity.AddComponent<CMeshRenderer>();
                            __CMESHREDNDERER.Mesh = WEE.Render.__MESH;
                            __CMESHREDNDERER.Program = WEE.Render.__PROGRAM;
                        }

                        ImGui.EndPopup();
                    }
                }
            }finally{
                ImGui.End();
            }
        }
    }

    private static void DrawComponentFields(WEI.Component Component){
        var fields = Component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach(var field in fields){
            if(field.GetCustomAttribute<WEI.Save>() == null) continue;

            string label = field.Name;
            object value = field.GetValue(Component);

            if(field.FieldType == typeof(float)){
                float val = (float)value;
                if(ImGui.DragFloat(label, ref val, 0.1f)) field.SetValue(Component, val);
            }
            
            else if(field.FieldType == typeof(int)){
                int val = (int)value;
                if(ImGui.DragInt(label, ref val)) field.SetValue(Component, val);
            }
            
            else if(field.FieldType == typeof(bool)){
                bool val = (bool)value;
                if(ImGui.Checkbox(label, ref val)) field.SetValue(Component, val);
            }
            
            else if(field.FieldType == typeof(string)){
                string val = (string)value ?? "";
                if(ImGui.InputText(label, ref val, 200)) field.SetValue(Component, val);
            }
            
            else if(field.FieldType == typeof(Vector3F)){
                Vector3F v = (Vector3F)value;
                System.Numerics.Vector3 sysV = new(v.X, v.Y, v.Z);
                if(ImGui.DragFloat3(label, ref sysV, 0.1f)){
                    field.SetValue(Component, new Vector3F(sysV.X, sysV.Y, sysV.Z));
                }
            }
            
            else if(field.FieldType == typeof(Color4B)){
                Color4B c = (Color4B)value;
                System.Numerics.Vector4 sysC = new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
                if(ImGui.ColorEdit4(label, ref sysC)){
                    field.SetValue(Component, new Color4B((byte)(sysC.X * 255), (byte)(sysC.Y * 255), (byte)(sysC.Z * 255), (byte)(sysC.W * 255)));
                }
            }
        }
    }
    
    // ----------------------------------------------------------------------

    public static Entity? SelectedEntity = null!;
    
    private static Entity? __DraggedEntity = null;
    
    private static void Update_Hierarchy(){
        if(!__ShowHierarchy){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Иерархия###Hierarchy", ref __ShowHierarchy);

            if(ActiveScene == null){
                ImGui.Text("Нет активной сцены");
                ImGui.End();
                return;
            }

            List<Entity> AllEntities = ActiveScene.AllEntity.ToList();
            ImGui.TextDisabled($"Всего: {AllEntities.Count}, Корней: {ActiveScene.Roots.Count()}");

            ImGui.BeginChild("HierarchyList");
            
                foreach(Entity Entity in ActiveScene.Roots.ToList()){
                    if(Entity.Node.Parent == null){
                        DrawEntityNode(Entity);
                    }
                }

                if(ImGui.IsMouseDown(0) && ImGui.IsWindowHovered()){
                    SelectedEntity = null;
                }

                if(ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems)){
                    if(ImGui.MenuItem("Создать Entity")){
                        Entity NewEntity = new Entity();
                        ActiveScene.Add(NewEntity);
                        SelectedEntity = NewEntity;
                    }
                    ImGui.EndPopup();
                }

                if(ImGui.BeginDragDropTarget()){
                    unsafe{
                        ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ENTITY_HIERARCHY");
                        if(Payload.NativePtr != null && __DraggedEntity != null){
                            __DraggedEntity.Node.SetParent(null);
                        }
                    }
                    ImGui.EndDragDropTarget();
                }
                
            ImGui.EndChild();
        
        ImGui.End();
    }

    private static void DrawEntityNode(Entity Entity){
        bool IsLeaf = Entity.Node.Children.Count == 0;
        
        ImGuiTreeNodeFlags Flags = (SelectedEntity == Entity ? ImGuiTreeNodeFlags.Selected : 0);
        Flags |= ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        if(IsLeaf){
            Flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        bool Opened = ImGui.TreeNodeEx($@"{Entity.Name}###{Entity.GetHashCode()}", Flags);

        if(ImGui.IsItemClicked()){ SelectedEntity = Entity; }

        if(ImGui.BeginDragDropSource()){
            __DraggedEntity = Entity;
            ImGui.SetDragDropPayload("ENTITY_HIERARCHY", IntPtr.Zero, 0);
            ImGui.Text($"Перенос: {Entity.Name}");
            ImGui.EndDragDropSource();
        }

        if(ImGui.BeginDragDropTarget()){
            unsafe{
                ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ENTITY_HIERARCHY");
                if(Payload.NativePtr != null){
                    if(__DraggedEntity != null && __DraggedEntity != Entity){
                        if(!Entity.Node.IsDescendantOf(__DraggedEntity.Node)){
                            __DraggedEntity.Node.SetParent(Entity.Node);
                        }
                    }
                }
            }
            ImGui.EndDragDropTarget();
        }

        if(ImGui.BeginPopupContextItem($"EntityContext_{Entity.GetHashCode()}")){
            if(ImGui.MenuItem("Создать Entity")){
                Entity NewEntity = new Entity();
                NewEntity.Node.SetParent(Entity.Node);
                SelectedEntity = NewEntity;
            }
            if(ImGui.MenuItem("Удалить")){ Entity.Destroy(); }
            ImGui.EndPopup();
        }
        
        if(Opened && !IsLeaf){
            try{
                foreach(HierarchyNode<Entity> ChildNode in Entity.Node.Children.ToList()){
                    DrawEntityNode(ChildNode.Owner);
                }
            }finally{
                ImGui.TreePop();
            }
        }
    }
    
    // ----------------------------------------------------------------------
    
    private static void Update_Assets(){
        if(!__ShowAssets){ return; }

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("Ресурсы###Assets", ref __ShowAssets);
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
        ImGui.Begin("Консоль###Console", ref __ShowConsole);

            if(ImGui.Button("Очистить")){ __ConsoleEntries.Clear(); }
            ImGui.SameLine();
            if(ImGui.Button("Тестовое сообщение")){ WL.Logger.Debug("Тестовое сообщение"); }
            ImGui.SameLine();
            ImGui.Text($"Кол-во: {__ConsoleEntries.Count}");
            
            ImGui.Separator();

            float FooterHeightToReverse = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0, -FooterHeightToReverse), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

                foreach(__LogEntry Entry in __ConsoleEntries){
                    ImGui.TextColored(new System.Numerics.Vector4(Entry.Color.R / 255f, Entry.Color.G / 255f, Entry.Color.B / 255f, Entry.Color.A / 255f), Entry.Message);
                }

                if(__ScrollToBottom){
                    ImGui.SetScrollHereY(1);
                    __ScrollToBottom = false;
                }
            
            ImGui.EndChild();
            
        ImGui.End();
    }
    
    // ----------------------------------------------------------------------

    private static bool __FirstFrame = true;
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);
        
            Update_Menu();

            uint DockSpaceID = ImGui.GetID("MainDockSpace");
            ImGui.DockSpaceOverViewport(DockSpaceID, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

            if(__FirstFrame){
                __FirstFrame = false;

                ImGuiDockBuilder.igDockBuilderRemoveNode(DockSpaceID); 
                ImGuiDockBuilder.igDockBuilderAddNode(DockSpaceID, ImGuiDockNodeFlags.None);
                ImGuiDockBuilder.igDockBuilderSetNodeSize(DockSpaceID, ImGui.GetMainViewport().Size);

                ImGuiDockBuilder.igDockBuilderSplitNode(DockSpaceID, ImGuiDir.Right, 0.15f, out uint dockid_right, out uint dockid_left);

                ImGuiDockBuilder.igDockBuilderSplitNode(dockid_left, ImGuiDir.Up, 0.75f, out uint dockid_up, out uint dockid_down);

                ImGuiDockBuilder.igDockBuilderSplitNode(dockid_down, ImGuiDir.Right, 0.15f, out uint dockid_down_right, out uint dockid_down_left);

                ImGuiDockBuilder.igDockBuilderDockWindow("###SceneView", dockid_up);
                
                ImGuiDockBuilder.igDockBuilderDockWindow("###Inspector", dockid_right);
                
                ImGuiDockBuilder.igDockBuilderDockWindow("###Hierarchy", dockid_down_right);
                
                ImGuiDockBuilder.igDockBuilderDockWindow("###Assets", dockid_down_left);
                ImGuiDockBuilder.igDockBuilderDockWindow("###Console", dockid_down_left);
                
                ImGuiDockBuilder.igDockBuilderFinish(DockSpaceID);
            }
            
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