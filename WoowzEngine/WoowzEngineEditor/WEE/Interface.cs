using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using NativeFileDialogSharp;
using WEEO;
using WEI;
using WEI.Editor;
using WEO_Component;
using WEO;
using WLO;
using WLO.Interface;
using WLO.Math;
using WoowzLib.Interface.ImGUI;

namespace WEE;

// todo, NativeFileDialogSharp

// TODO, РАЗДЕЛИТЬ СКРИПТ ОТДЕЛЬНО КАЖДОЕ ОКОШКО В ОТДЕЛЬНЫЙ СКРИПТ (займусь этим сейчас наверное.....)

public static class Interface{
    public static void Start(){
        ImGUI = new GLImGUI(WE.Render.API, true);

        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGUI.IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        ImGUI.IO.ConfigWindowsMoveFromTitleBarOnly = true;
        
        ImGuiStylePtr Style = ImGui.GetStyle();
        RangeAccessor<Vector4> Colors = Style.Colors;

        Vector4 mainRed = new Vector4(0.70f, 0.00f, 0.00f, 1.00f);
        Vector4 hoverRed = new Vector4(0.85f, 0.10f, 0.10f, 1.00f);
        Vector4 activeRed = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        Vector4 darkRed = new Vector4(0.40f, 0.00f, 0.00f, 1.00f);

        Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.15f, 0.00f, 0.00f, 1.00f);
        Colors[(int)ImGuiCol.TitleBgActive] = darkRed;
        Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);

        Colors[(int)ImGuiCol.Button] = mainRed;
        Colors[(int)ImGuiCol.ButtonHovered] = hoverRed;
        Colors[(int)ImGuiCol.ButtonActive] = activeRed;

        Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.05f, 0.05f, 0.54f);
        Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.40f, 0.10f, 0.10f, 0.40f);
        Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.50f, 0.10f, 0.10f, 0.67f);

        Colors[(int)ImGuiCol.Tab] = darkRed;
        Colors[(int)ImGuiCol.TabHovered] = hoverRed;
        Colors[(int)ImGuiCol.TabSelected] = activeRed;
        Colors[(int)ImGuiCol.TabSelectedOverline] = activeRed;
        Colors[(int)ImGuiCol.TabDimmed] = darkRed;
        Colors[(int)ImGuiCol.TabDimmedSelected] = mainRed;

        Colors[(int)ImGuiCol.Header] = darkRed;
        Colors[(int)ImGuiCol.HeaderHovered] = mainRed;
        Colors[(int)ImGuiCol.HeaderActive] = hoverRed;

        Colors[(int)ImGuiCol.CheckMark] = activeRed;
        Colors[(int)ImGuiCol.SliderGrab] = mainRed;
        Colors[(int)ImGuiCol.SliderGrabActive] = hoverRed;
        Colors[(int)ImGuiCol.SeparatorHovered] = hoverRed;
        Colors[(int)ImGuiCol.SeparatorActive] = activeRed;
        Colors[(int)ImGuiCol.ResizeGrip] = darkRed;
        Colors[(int)ImGuiCol.ResizeGripHovered] = mainRed;
        Colors[(int)ImGuiCol.ResizeGripActive] = activeRed;
        Colors[(int)ImGuiCol.DockingPreview] = mainRed;
        Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.90f, 0.20f, 0.20f, 0.35f);

        Style.WindowRounding = 0;
        Style.FrameRounding = 12;
        Style.PopupRounding = 0;
        Style.GrabRounding = 12;
        Style.TabRounding = 0;

        Style.TabBorderSize = 1;
        
        WEE.Prefs.Load();
        
        Start_Console();
    }
    
    public static void Stop(){
        if(ImGUI != null!){ ImGUI.Stop(); }
    }
    
    // ----------------------------------------------------------------------

    public static GLImGUI ImGUI{ get; private set; } = null!;

    // ----------------------------------------------------------------------

    public static  EditorConfig? Config                = null!;
    private static string        __ConfigPath          = "";
    public static  bool          __IsProjectLoaded     = false;
    private static string        __NewProjectName      = "New Project";
    private const  string        __ConfigFileExtension = "weeconfig";
    
    public static void Update_Launcher(){
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));

        if(ImGui.Begin("Загрузчик", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)){
            if(!string.IsNullOrEmpty(Prefs.LastConfigPath) && File.Exists(Prefs.LastConfigPath)){
                ImGui.TextColored(new Vector4(0.4f, 1, 0.4f, 1), "Последний проект:");
                if(ImGui.Button($"{Path.GetFileName(Prefs.LastConfigPath)}###RecentProject", new Vector2(-1, 40))){
                    __ConfigPath = Prefs.LastConfigPath;
                    Config = EditorConfig.Load(__ConfigPath);
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
            
            ImGui.End();
        }
    }

    private static void CreateNewProject(){
        DialogResult? Result = Dialog.FileSave(__ConfigFileExtension);
        if(Result.IsOk){
            string __ConfigPath = Result.Path;

            if(!__ConfigPath.EndsWith($".{__ConfigFileExtension}", StringComparison.OrdinalIgnoreCase)){
                __ConfigPath += $".{__ConfigFileExtension}";
            }

            Config = new EditorConfig{
                Name = __NewProjectName
            };
            
            Config.Save(__ConfigPath);
            
            OnProjectLoaded();    
        }
    }

    private static void OpenExistingProject(){
        DialogResult? Result = Dialog.FileOpen(__ConfigFileExtension);
        if(Result.IsOk){
            __ConfigPath = Result.Path;
            Config = EditorConfig.Load(__ConfigPath);
            OnProjectLoaded();
        }
    }

    private static void OnProjectLoaded(){
        __IsProjectLoaded = true;
        Prefs.LastConfigPath = __ConfigPath;
        Prefs.Save();
    }
    
    // ----------------------------------------------------------------------

    public static Scene?  ActiveScene;
    
    private static bool __ShowSceneView = true;
    private static bool __ShowInspector = true;
    private static bool __ShowHierarchy = true;
    private static bool __ShowAssets    = true;
    private static bool __ShowConsole   = true;
    private static bool __ShowConfig    = false;

    private static bool __ShowImGUIDemo = false;

    public static string __SceneFilePath = null!;
    private const  string __SceneFileExtension = "weescene";

    private static void CloseScene(){
        ActiveScene?.Clear();
        SelectedEntity = null;
        ActiveScene = null;
    }
    
    private static void Update_Menu(){
        if(ImGui.BeginMainMenuBar()){
            if(ImGui.BeginMenu("Файл")){
                if(ImGui.MenuItem("Новая сцена")){
                    CloseScene();
                    ActiveScene = new Scene();
                }
                
                ImGui.Separator();

                if(ImGui.MenuItem("Открыть", "Ctrl+O")){
                    OpenScene();
                }
                
                if(ImGui.MenuItem("Сохранить", "Ctrl+S", false, ActiveScene != null)){
                    SaveScene();
                }
                
                if(ImGui.MenuItem("Сохранить как", "Ctrl+Shift+S", false, ActiveScene != null)){
                    SaveSceneAs();
                }
                
                ImGui.Separator();
                
                if(ImGui.MenuItem("Закрыть сцену", "", false, ActiveScene != null)){ CloseScene(); }
                if(ImGui.MenuItem("Выйти", "Alt+F4")){ WEE.Window.MainWindow.Close(); }
                
                ImGui.Separator();

                foreach(string ScenePath in Prefs.RecentScenes.ToList()){
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
                ImGui.MenuItem("Просмотр сцены", "", ref __ShowSceneView);
                ImGui.MenuItem("Просмотр", "", ref __ShowInspector);
                ImGui.MenuItem("Иерархия", "", ref __ShowHierarchy);
                ImGui.MenuItem("Ресурсы", "", ref __ShowAssets);
                ImGui.MenuItem("Консоль", "", ref __ShowConsole);
                ImGui.MenuItem("Конфиг", "", ref __ShowConfig);
                
                ImGui.Separator();

                ImGui.MenuItem("ImGUI Demo", "", ref __ShowImGUIDemo);
                
                ImGui.EndMenu();
            }
            
            if(ImGui.BeginMenu("Помощь")){
                if(ImGui.MenuItem("Открыть GitHub...")){ Process.Start(new ProcessStartInfo("https://github.com/WoowzCore/WoowzEngine"){ UseShellExecute = true }); }
                ImGui.EndMenu();
            }

            if(ActiveScene != null){
                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.SetNextItemWidth(300);
                ImGui.InputText("##SceneNameInput", ref ActiveScene.Name, 128);
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

    private static void SaveSceneAs(){
        if(ActiveScene == null){ return; }
        
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
        if(ActiveScene == null){ return; }
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
            __SceneFilePath = Path;
            SelectedEntity = null;
            
            Prefs.AddRecentScene(Path);
            WL.Logger.Info($"Сцена загружена: {Path}");
        }catch(Exception e){
            WL.Logger.Error($"Ошибка загрузки: {e.Message + "\n" + e.StackTrace}");
        }
    }
    
    // ----------------------------------------------------------------------
    
    public static bool FocusSceneView{ get; private set; }

    public static Vector2I SceneViewSize{ get; private set; }

    public static bool Is2DView = false;

    public static Color4B BackgroundColor = new Color4B(200, 200, 200);
    
    private static void Update_SceneView(){
        if(!__ShowSceneView){ return; }

        if(ImGui.Begin("Просмотр сцены###SceneView", ref __ShowSceneView)){

            FocusSceneView = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

            if(ImGui.BeginChild("SceneToolbar", new Vector2(0, 30), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoScrollbar)){
                ImGui.SameLine();

                ImGui.Text($"({SceneViewSize.W}x{SceneViewSize.H}), R-FPS: {WEE.Cycle.Render_DTI.FPS:F1}");

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                string ModeText = Is2DView ? "2D" : "3D";

                if(ImGui.Button(ModeText, new Vector2(100, 20))){
                    Is2DView = !Is2DView;

                    WEE.Editor.SceneViewCamera.IsOrthographic = Is2DView;
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Переключить перспективу камеры"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Позиция:");
                ImGui.SameLine();
                Vector3 CameraPosition = new Vector3(WEE.Editor.SceneViewCamera.Position.X, WEE.Editor.SceneViewCamera.Position.Y, WEE.Editor.SceneViewCamera.Position.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CamPos", ref CameraPosition, 0.1f)){
                    WEE.Editor.SceneViewCamera.Position = new Vector3F(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                ImGui.TextDisabled("Поворот:");
                ImGui.SameLine();
                Vector3 CameraRotation = new Vector3(WEE.Editor.SceneViewCamera.Rotation.X, WEE.Editor.SceneViewCamera.Rotation.Y, WEE.Editor.SceneViewCamera.Rotation.Z);
                ImGui.SetNextItemWidth(200);
                if(ImGui.DragFloat3("##CamRos", ref CameraRotation, 0.1f)){
                    WEE.Editor.SceneViewCamera.Rotation = new Vector3F(CameraRotation.X, CameraRotation.Y, CameraRotation.Z);
                }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                if(ImGui.Button("Сброс")){
                    WEE.Editor.SceneViewCamera.Position = WEE.Editor.SceneViewCamera.Rotation = new Vector3F();
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Сбросить позицию и поворот камеры на дефолтные значения"); }

                ImGui.SameLine();
                ImGui.TextDisabled("|");
                ImGui.SameLine();

                Vector3 BackgroundColor__ = new Vector3(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);
                if(ImGui.ColorEdit3("##Background", ref BackgroundColor__, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel)){
                    BackgroundColor = new Color4B((byte)(BackgroundColor__.X * 255), (byte)(BackgroundColor__.Y * 255), (byte)(BackgroundColor__.Z * 255));
                }
                if(ImGui.IsItemHovered()){ ImGui.SetTooltip("Цвет заднего фона"); }
            } ImGui.EndChild();

            System.Numerics.Vector2 __SceneViewport = ImGui.GetContentRegionAvail();
            __SceneViewport.X = System.Math.Max(1, __SceneViewport.X);
            __SceneViewport.Y = System.Math.Max(1, __SceneViewport.Y);
            SceneViewSize = new Vector2I((int)__SceneViewport.X, (int)__SceneViewport.Y);

            if(ActiveScene != null){
                ImGui.Image((IntPtr)WEE.Render.SceneFramebuffer.ResultTexture!.ID, __SceneViewport, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
            }
        } ImGui.End();
    }

    // ----------------------------------------------------------------------
    
    private static void Update_Inspector(){
        if(!__ShowInspector){ return; }

        if(ImGui.Begin("Просмотр###Inspector", ref __ShowInspector)){
            try{
                if(SelectedEntity == null){
                    ImGui.TextDisabled("Выберите объект в иерархии...");
                }else{
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

                        System.Numerics.Vector3 Rotation = new Vector3(SelectedEntity.Transform.Rotation.X, SelectedEntity.Transform.Rotation.Y, SelectedEntity.Transform.Rotation.Z);

                        if(ImGui.DragFloat3("Поворот", ref Rotation, 0.1f)){
                            SelectedEntity.Transform.Rotation = new Vector3F(Rotation.X, Rotation.Y, Rotation.Z);
                            SelectedEntity.SetTransformDirty();
                        }
                        
                        System.Numerics.Vector3 Scale = new Vector3(SelectedEntity.Transform.Scale.X, SelectedEntity.Transform.Scale.Y, SelectedEntity.Transform.Scale.Z);

                        if(ImGui.DragFloat3("Размер", ref Scale, 0.1f)){
                            SelectedEntity.Transform.Scale = new Vector3F(Scale.X, Scale.Y, Scale.Z);
                            SelectedEntity.SetTransformDirty();
                        }
                    }

                    ImGui.Separator();

                    foreach(Component Component in SelectedEntity.GetAllComponents().ToList()){
                        ImGui.PushID(Component.GetHashCode());
                        
                        bool ComponentOpen = ImGui.CollapsingHeader(Component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen);

                        if(ImGui.BeginPopupContextItem("ComponentSettings")){
                            if(ImGui.MenuItem("Удалить компонент")){
                                SelectedEntity.RemoveComponent(Component);
                            }

                            ImGui.EndPopup();
                        }

                        if(ComponentOpen){
                            DrawComponentFields(Component);
                        }
                        
                        ImGui.PopID();
                    }

                    ImGui.Separator();

                    if(ImGui.Button("Добавить компонент", new Vector2(-1, 0))){
                        ImGui.OpenPopup("AddComponentPopup");
                    }

                    if(ImGui.BeginPopup("AddComponentPopup")){
                        foreach(Type ComponentType in WEE.Registry.AvailableComponents){
                            if(ImGui.MenuItem(ComponentType.Name)){
                                MethodInfo Method = typeof(Entity).GetMethod("AddComponent")!;
                                MethodInfo Generic = Method.MakeGenericMethod(ComponentType);
                                Generic.Invoke(SelectedEntity, null);
                            }
                        }
                        
                        ImGui.EndPopup();
                    }
                }
            }catch(Exception e){
                WL.Logger.Warn("TODO INSPECTOR " + e.Message);
            }
        } ImGui.End();
    }

    private static void DrawComponentFields(WEI.Component Component){
        FieldInfo[] Fields = Component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach(FieldInfo Field in Fields){
            if(Field.GetCustomAttribute<WEESave>() == null){ continue; }

            ImGui.PushID(Field.Name);
            
            string Label = Field.Name;
            object Value = Field.GetValue(Component)!;

            Type FieldType = Field.FieldType;

            if(FieldType.IsGenericType && FieldType.GetGenericTypeDefinition() == typeof(WEO.Asset<>)){
                Type AssetTargetType = FieldType.GetGenericArguments()[0];

                string CurrentKey = (string)FieldType.GetField("Key")!.GetValue(Value)! ?? "";
                bool IsLinked = (bool)FieldType.GetField("Linked")!.GetValue(Value)!;
                
                ImGui.BeginGroup();

                    float AvailableWidth = ImGui.GetContentRegionAvail().X;
                    float LabelWidth = ImGui.CalcTextSize(Label).X + 20;
                    const float ButtonWidth = 35;
                    
                    ImGui.SetNextItemWidth(AvailableWidth - LabelWidth - ButtonWidth - ImGui.GetStyle().ItemSpacing.X * 2);

                    string TempKey = CurrentKey;
                    if(ImGui.InputText($"##in_{Label}", ref TempKey, 256)){
                        ConstructorInfo? Constructor = FieldType.GetConstructor([typeof(string)]);
                        Field.SetValue(Component, Constructor!.Invoke([TempKey]));
                    }

                    if(!string.IsNullOrEmpty(CurrentKey) && ImGui.BeginDragDropSource()){
                        IntPtr Ptr = Marshal.StringToHGlobalAnsi(CurrentKey);
                        ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)CurrentKey.Length + 1);
                        Marshal.FreeHGlobal(Ptr);

                        ImGui.Text($"Передать ассет: {CurrentKey}");
                        ImGui.EndDragDropSource();
                    }

                    if(ImGui.BeginDragDropTarget()){
                        unsafe{
                            ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ASSET_KEY");
                            if(Payload.NativePtr != null){
                                string DroppedKey = Marshal.PtrToStringAnsi(Payload.Data)!;

                                bool IsValidType = WE.Asset.GetKeysForType(AssetTargetType).Contains(DroppedKey);

                                if(IsValidType){
                                    if(ImGui.IsMouseReleased(ImGuiMouseButton.Left)){
                                        ConstructorInfo? Constructor = FieldType.GetConstructor([typeof(string)]);
                                        Field.SetValue(Component, Constructor!.Invoke([DroppedKey]));
                                    }
                                    
                                    ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(new Vector4(0.2f, 1, 0.2f, 1)), 4.0f);
                                }else{
                                    ImGui.SetTooltip($"Недопустимый тип! Ожидается: {AssetTargetType.Name}");
                                    ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(new Vector4(1, 0.2f, 0.2f, 1)), 4);
                                }
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                    
                    ImGui.SameLine();
                    if(ImGui.Button("...", new Vector2(ButtonWidth, 0))){
                        ImGui.OpenPopup("AssetPicker");
                    }
                    
                    ImGui.SameLine();
                    ImGui.Text(Label);
                    
                ImGui.EndGroup();

                if(ImGui.IsItemHovered()){ ImGui.SetTooltip($"Тип: {AssetTargetType.Name}"); }

                if(ImGui.BeginPopup("AssetPicker")){
                    foreach(string Key in WE.Asset.GetKeysForType(AssetTargetType).OrderBy(K => K)){
                        if(ImGui.Selectable(Key, Key == CurrentKey)){
                            ConstructorInfo? Constructor = FieldType.GetConstructor([typeof(string)]);
                            Field.SetValue(Component, Constructor!.Invoke([Key]));
                        }
                    }
                    ImGui.EndPopup();
                }
            }else if(FieldType == typeof(float)){
                float V = (float)Value;
                if(ImGui.DragFloat(Label, ref V, 0.1f)){ Field.SetValue(Component, V); }
            }else if(FieldType == typeof(int)){
                int V = (int)Value;
                if(ImGui.DragInt(Label, ref V)){ Field.SetValue(Component, V); }
            }else if(FieldType == typeof(bool)){
                bool V = (bool)Value;
                if(ImGui.Checkbox(Label, ref V)){ Field.SetValue(Component, V); }
            }else if(FieldType == typeof(string)){
                string V = (string)Value ?? "";

                WEEMultilineString? MultilineAttribute = Field.GetCustomAttribute<WEEMultilineString>();

                if(MultilineAttribute != null){
                    ImGui.Text(Label);
                    if(ImGui.InputTextMultiline($"##{Label}", ref V, 5000, new Vector2(-1, MultilineAttribute.Height))){
                        Field.SetValue(Component, V);
                    }
                }else{
                    if(ImGui.InputText(Label, ref V, 200)){
                        Field.SetValue(Component, V);
                    }
                }
            }else if(FieldType == typeof(Vector3F)){
                Vector3F V = (Vector3F)Value;
                System.Numerics.Vector3 SysV = new Vector3(V.X, V.Y, V.Z);
                if(ImGui.DragFloat3(Label, ref SysV, 0.1f)){
                    Field.SetValue(Component, new Vector3F(SysV.X, SysV.Y, SysV.Z));
                }
            }else if(FieldType == typeof(Color4B)){
                Color4B V = (Color4B)Value;
                System.Numerics.Vector4 SysV = new Vector4(V.R / 255f, V.G / 255f, V.B / 255f, V.A / 255f);
                if(ImGui.ColorEdit4(Label, ref SysV)){
                    Field.SetValue(Component, new Color4B((byte)(SysV.X * 255), (byte)(SysV.Y * 255), (byte)(SysV.Z * 255), (byte)(SysV.W * 255)));
                }
            }
            
            ImGui.PopID();
        }
    }
    
    // ----------------------------------------------------------------------

    public static Entity? SelectedEntity = null!;
    
    private static Entity? __DraggedEntity = null;
    
    private static void Update_Hierarchy(){
        if(!__ShowHierarchy){ return; }

        if(ImGui.Begin("Иерархия###Hierarchy", ref __ShowHierarchy)){
            try{
                if(ActiveScene == null){
                    ImGui.Text("Нет активной сцены");
                }else{
                    List<Entity> AllEntities = ActiveScene.AllEntity.ToList();
                    ImGui.TextDisabled($"Всего: {AllEntities.Count}, Корней: {ActiveScene.Roots.Count()}");

                    if(ImGui.BeginChild("HierarchyList")){
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
                    } ImGui.EndChild();
                }
            }catch(Exception e){
                WL.Logger.Warn("TODO HIERARCHY " + e.Message);
            }
        } ImGui.End();
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

        if(ImGui.Begin("Ресурсы###Assets", ref __ShowAssets)){
            List<Type> AssetTypes = WE.Asset.RegisteredTypes.OrderBy(T => T.Name).ToList();

            if(AssetTypes.Count == 0){
                ImGui.TextDisabled("Нет зарегистрированных ресурсов");
            }else{
                foreach(Type Type in AssetTypes){
                    ImGui.PushID(Type.FullName); 
                    
                        ImGui.Spacing();
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0.8f, 0.4f, 1));
                        ImGui.Text(Type.Name.ToUpper());
                        ImGui.PopStyleColor();
                        ImGui.Separator();
                        ImGui.Spacing();

                        List<string> Keys = WE.Asset.GetKeysForType(Type).OrderBy(k => k).ToList();

                        const float IconSize = 128;
                        float Padding = ImGui.GetStyle().ItemSpacing.X;

                        float windowVisibleRightEdge = ImGui.GetCursorScreenPos().X + ImGui.GetContentRegionAvail().X;

                        for(int i = 0; i < Keys.Count; i++){
                            string Key = Keys[i];
                            int ID = WE.Asset.GetID(Key);

                            ImGui.PushID(Key);

                                ImGui.BeginGroup();
                                    Vector2 CursorPosition = ImGui.GetCursorScreenPos();
                                    ImGui.InvisibleButton("##btn", new Vector2(IconSize, IconSize));

                                    if(ImGui.BeginDragDropSource()){
                                        IntPtr Ptr = Marshal.StringToHGlobalAnsi(Key);
                                        ImGui.SetDragDropPayload("ASSET_KEY", Ptr, (uint)Key.Length + 1);
                                        Marshal.FreeHGlobal(Ptr);
                                        
                                        ImGui.Text($"Перетаскивание ассета: {Key}");
                                        ImGui.EndDragDropSource();
                                    }
                                    
                                    bool IsHovered = ImGui.IsItemHovered();
                                    bool IsActive = ImGui.IsItemActive();

                                    uint Column = ImGui.GetColorU32(IsActive ? ImGuiCol.HeaderActive : IsHovered ? ImGuiCol.HeaderHovered : ImGuiCol.FrameBg);

                                    ImGui.GetWindowDrawList().AddRectFilled(CursorPosition, new Vector2(CursorPosition.X + IconSize, CursorPosition.Y + IconSize), Column, 4.0f);

                                    RenderTextScrolling($"[{ID}] {Key}", IconSize, IsHovered);
                                ImGui.EndGroup();

                                float CurrentItemRightEdge = ImGui.GetItemRectMax().X;
                                float NextItemRightEdge = CurrentItemRightEdge + Padding + IconSize;

                                if(i + 1 < Keys.Count && NextItemRightEdge < windowVisibleRightEdge){
                                    ImGui.SameLine();
                                }

                            ImGui.PopID();
                        }

                        ImGui.Spacing();
                        
                    ImGui.PopID();
                }
            }
        } ImGui.End();
    }
    
    public static void RenderTextScrolling(string text, float maxWidth, bool isHovered){
        Vector2 pos = ImGui.GetCursorScreenPos();
        Vector2 textSize = ImGui.CalcTextSize(text);
        var drawList = ImGui.GetWindowDrawList();

        // Определяем границы отрисовки (ClipRect)
        Vector2 clipMin = pos;
        Vector2 clipMax = new Vector2(pos.X + maxWidth, pos.Y + ImGui.GetTextLineHeightWithSpacing());

        if (textSize.X <= maxWidth)
        {
            // Текст помещается — просто рисуем
            drawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), text);
        }
        else
        {
            if (isHovered)
            {
                // Бегущая строка (Sin анимация)
                float diff = textSize.X - maxWidth;
                float speed = 2.0f;
                float offset = (MathF.Sin((float)ImGui.GetTime() * speed) * 0.5f + 0.5f) * diff;

                drawList.PushClipRect(clipMin, clipMax, true);
                drawList.AddText(new Vector2(pos.X - offset, pos.Y), ImGui.GetColorU32(ImGuiCol.Text), text);
                drawList.PopClipRect();
            }
            else
            {
                // Текст слишком длинный — обрезаем с многоточием
                string truncated = text;
                while (truncated.Length > 1 && ImGui.CalcTextSize(truncated + "...").X > maxWidth)
                {
                    truncated = truncated.Substring(0, truncated.Length - 1);
                }
                drawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.TextDisabled), truncated + "...");
            }
        }

        // Важно: AddText не двигает курсор ImGui, поэтому добавляем невидимый элемент,
        // чтобы следующий объект ImGui не наложился на этот текст.
        ImGui.Dummy(new Vector2(maxWidth, ImGui.GetTextLineHeightWithSpacing()));
    }

    // ----------------------------------------------------------------------
    
    private static string __FullLogBuffer  = "";
    private static bool   __ScrollToBottom = false;
    
    private static void Start_Console(){
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
    
    private static void Update_Console(){
        if(!__ShowConsole){ return; }

        if(ImGui.Begin("Консоль###Console", ref __ShowConsole)){

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
    
    // ----------------------------------------------------------------------

    private static void Update_Config(){
        if(!__ShowConfig || Config == null){ return; }
        
        ImGui.SetNextWindowSize(new Vector2(450, 250), ImGuiCond.FirstUseEver);
        if(ImGui.Begin("Конфиг###Config", ref __ShowConfig)){
            ImGui.TextDisabled($"Путь к конфигу: {__ConfigPath}");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.InputText("Название проекта", ref Config.Name, 64);

            ImGui.Text("Путь к Game DLL");
            float __ButtonW = 35;
            float __Spacing = ImGui.GetStyle().ItemSpacing.X;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - __ButtonW - __Spacing);
            ImGui.InputText("##GameDLLPath", ref Config.GameDLLPath, 256);
            ImGui.SameLine();
            if(ImGui.Button("...###SelectGameDLL")){
                DialogResult? Result = Dialog.FileOpen("dll");
                if(Result.IsOk){
                    Config.GameDLLPath = Result.Path;

                    try{
                        Assembly GameAssembly = Assembly.LoadFrom(Config.GameDLLPath);
                        WEE.Registry.ResetAndReload(GameAssembly);
                    }catch(Exception e){
                        WL.Logger.Error("ошибка при загрузке dll, todo");
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            if(ImGui.Button("Сохранить", new Vector2(-1, 30))){
                Config.Save(__ConfigPath);
                WL.Logger.Info("Конфиг сохранён!");
            }
        } ImGui.End();
    }
    
    // ----------------------------------------------------------------------
    
    private static bool __FirstFrame = true;
    
    public static void Update(){
        ImGUI.FrameStart(WEE.Cycle.Render_DT, WEE.Window.MainWindow.Size);

            if(!__IsProjectLoaded){
                Update_Launcher();   
            }else{
                Update_Menu();

                uint DockSpaceID = ImGui.GetID("MainDockSpace");
                ImGui.DockSpaceOverViewport(DockSpaceID, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                if(__FirstFrame){
                    __FirstFrame = false;

                    Assembly? GameAssembly = null;
                    try{
                        if(Config != null && !string.IsNullOrEmpty(Config.GameDLLPath)){ GameAssembly = Assembly.LoadFrom(Config.GameDLLPath); }
                    }catch(Exception e){
                        WL.Logger.Error("ошибка при загрузке dll, todo 2");
                    }
                    WEE.Registry.ResetAndReload(GameAssembly);

                    ImGuiDockBuilder.igDockBuilderRemoveNode(DockSpaceID); 
                    ImGuiDockBuilder.igDockBuilderAddNode(DockSpaceID, ImGuiDockNodeFlags.None);
                    ImGuiDockBuilder.igDockBuilderSetNodeSize(DockSpaceID, ImGui.GetMainViewport().Size);

                    ImGuiDockBuilder.igDockBuilderSplitNode(DockSpaceID, ImGuiDir.Right, 0.25f, out uint dockid_right, out uint dockid_left);

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
                
                Update_Config();

                if(__ShowImGUIDemo){ ImGui.ShowDemoWindow(ref __ShowImGUIDemo); }
            }
        
        ImGUI.FrameEnd();
    }

    public static void Render() => ImGUI.Render();
}