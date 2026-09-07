using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using WEI;
using WEI_Attribute;
using WEO;
using WLO.Interface;
using WLO.Math;

namespace WEE_Interface;

public static class I_Inspector{
    public static void Update(){
        if(!WEE.Interface.WindowInspectorActive){ return; }

        ImGUI GUI = WEE.D.ImGUI;

        GUI.Window("Инспектор###Inspector", ref WEE.Interface.WindowInspectorActive, () => {
            if(WEE.D.Selected.Entity == null){
                ImGui.TextDisabled("Выберите объект в иерархии...");
            }else{
                Entity Entity = WEE.D.Selected.Entity;
                
                string Prefix = $"[{Entity.ID}]";
                ImGui.AlignTextToFramePadding();
                ImGui.TextDisabled(Prefix);
                
                ImGui.SameLine();
                
                string Name = Entity.Name;
                ImGui.SetNextItemWidth(-(ImGui.CalcTextSize("Название").X + ImGui.GetStyle().ItemSpacing.X));
                if(ImGui.InputText("Название", ref Name, 100)){
                    Entity.Name = Name;
                }
                
                ImGui.Separator();

                if(Entity.IsPartOfPrefab){
                    Entity Root = Entity.PrefabRoot!;
                    
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.4f, 0.6f, 1, 1));
                    ImGui.Text("Prefab:");
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    ImGui.Text(Root.SourcePrefab!.Value.Key);

                    if (ImGui.Button("Разобрать")) {
                        Root.SourcePrefab = null;
                    }
                }else{
                    if(ImGui.Button("Превратить в Prefab")){
                        I_Menu.SaveEntityAsPrefab(Entity);
                    }
                }
                
                ImGui.Separator();
                
                if(ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen)){
                    Vector3 Position = new Vector3(Entity.Transform.Position.X, Entity.Transform.Position.Y, Entity.Transform.Position.Z);

                    if(ImGui.DragFloat3("Позиция", ref Position, 0.1f, 0, 0, "%g")){
                        Entity.Transform.Position = new Vector3F(Position.X, Position.Y, Position.Z);
                        Entity.SetTransformDirty();
                    }

                    const float RadToDeg = 180f / System.MathF.PI;
                    const float DegToRad = System.MathF.PI / 180;
                    
                    Vector3 Rotation = new Vector3(
                        Entity.Transform.Rotation.X * RadToDeg,
                        Entity.Transform.Rotation.Y * RadToDeg,
                        Entity.Transform.Rotation.Z * RadToDeg
                    );
                    
                    if(ImGui.DragFloat3("Поворот", ref Rotation, 0.1f, 0, 0, "%g")){
                        Entity.Transform.Rotation = new Vector3F(
                            Rotation.X * DegToRad,
                            Rotation.Y * DegToRad,
                            Rotation.Z * DegToRad
                        );
                        Entity.SetTransformDirty();
                    }
                    
                    Vector3 Scale = new Vector3(Entity.Transform.Scale.X, Entity.Transform.Scale.Y, Entity.Transform.Scale.Z);

                    if(ImGui.DragFloat3("Размер", ref Scale, 0.1f, 0, 0, "%g")){
                        Entity.Transform.Scale = new Vector3F(Scale.X, Scale.Y, Scale.Z);
                        Entity.SetTransformDirty();
                    }
                    
                    ImGui.TextDisabled("Наследование:");

                    ImGui.SameLine(); if(ImGui.Checkbox("Поз.", ref Entity.Transform.InheritPosition)){ Entity.SetTransformDirty(); }
                    ImGui.SameLine(); if(ImGui.Checkbox("Пов.", ref Entity.Transform.InheritRotation)){ Entity.SetTransformDirty(); }
                    ImGui.SameLine(); if(ImGui.Checkbox("Раз.", ref Entity.Transform.InheritScale   )){ Entity.SetTransformDirty(); }
                }

                ImGui.Separator();

                foreach(Component Component in Entity.GetAllComponents().ToList()){
                    GUI.CustomID(Component.GetHashCode(), () => {
                        bool ComponentOpen = ImGui.CollapsingHeader(Component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen);

                        if(!Entity.IsPartOfPrefab){
                            GUI.PopupContextItem("ComponentSettings", () => {
                                if(ImGui.MenuItem("Удалить компонент")){
                                    Entity.RemoveComponent(Component);
                                }
                            });   
                        }

                        if(ComponentOpen){
                            if(Entity.IsPartOfPrefab){ ImGui.BeginDisabled(); }
                            
                            DrawComponentFields(Component, GUI);
                            
                            if(Entity.IsPartOfPrefab){ ImGui.EndDisabled(); }
                        }
                    });
                }

                if(!Entity.IsPartOfPrefab){
                    ImGui.Separator();

                    if(ImGui.Button("Добавить компонент", new Vector2(-1, 0))){
                        ImGui.OpenPopup("AddComponentPopup");
                    }

                    GUI.Popup("AddComponentPopup", () => {
                        foreach(Type ComponentType in WEE.Registry.AvailableComponents){
                            if(ImGui.MenuItem(ComponentType.Name)){
                                MethodInfo Method = typeof(Entity).GetMethod("AddComponent")!;
                                MethodInfo Generic = Method.MakeGenericMethod(ComponentType);
                                Generic.Invoke(WEE.D.Selected.Entity, null);
                            }
                        }
                    });
                }
            }
        });
    }
    
    private static void DrawComponentFields(WEI.Component Component, ImGUI GUI){
        Type Type = Component.GetType();

        bool CanDraw(MemberInfo Info){
            return Info.GetCustomAttribute<WE_Save>() != null && Info.GetCustomAttribute<WEEI_Hide>() == null;
        }
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(!CanDraw(Field)){ continue; }
            HandleMember(
                Component,
                Field.Name,
                Field.FieldType,
                () => Field.GetValue(Component),
                Value => Field.SetValue(Component, Value),
                Field,
                GUI
            );
        }

        foreach(PropertyInfo Property in Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(!CanDraw(Property) || !Property.CanWrite || !Property.CanRead){ continue; }
            HandleMember(
                Component,
                Property.Name,
                Property.PropertyType,
                () => Property.GetValue(Component),
                Value => Property.SetValue(Component, Value),
                Property,
                GUI
            );
        }
    }
    
    private static void HandleMember(object Component, string Label, Type MemberType, Func<object?> Getter, Action<object?> Setter, MemberInfo Info, ImGUI GUI){
        WEI.Component? Component__ = Component as WEI.Component;

        bool IsOverriden = Component__ != null && Component__.OverridesValues.Contains(Info.Name);
        
        object Template = WEI.Component.Template.GetDefault(Component.GetType());
        object? DefaultValue = Info is FieldInfo F ? F.GetValue(Template) : ((PropertyInfo)Info).GetValue(Template);
        
        GUI.CustomID(Label, () => {
            foreach(WEEI_InspectorDecorator Decorator in Info.GetCustomAttributes<WEEI_InspectorDecorator>()){
                Decorator.Draw(Label, Component, Info, GUI);
            }
            
            if(!IsOverriden){
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }

            WEEI_InspectorProperty? PropertyAttribute = Info.GetCustomAttribute<WEEI_InspectorProperty>();
            if(PropertyAttribute == null){
                PropertyAttribute = WE.Editor.GetDefault(MemberType);
            }

            if(PropertyAttribute != null){
                PropertyAttribute.Draw(Label, Component, Info, Getter, (NewValue) => {
                    Setter(NewValue);
                    Component__!.OverridesValues.Add(Info.Name);
                }, GUI);
            }else{
                ImGui.TextDisabled($"{Label}: {MemberType.Name}");   
            }

            if(!IsOverriden){
                ImGui.PopStyleVar();
            }

            GUI.PopupContextItem($"Context_{Label}", () => {
                if(IsOverriden){
                    if(ImGui.MenuItem("Сбросить по умолчанию")){
                        Component__!.OverridesValues.Remove(Info.Name);
                        Setter(DefaultValue);
                    }
                }else{
                    ImGui.TextDisabled("Это значение по умолчанию");   
                }
            });
        });
    }
}