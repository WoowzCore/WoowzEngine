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

        ImGUI GUI = WEE.Interface.ImGUI;

        GUI.Window("Инспектор###Inspector", ref WEE.Interface.WindowInspectorActive, () => {
            if(WEE.Interface.CurrentEntity == null){
                ImGui.TextDisabled("Выберите объект в иерархии...");
            }else{
                string Prefix = $"[{WEE.Interface.CurrentEntity.ID}]";
                ImGui.AlignTextToFramePadding();
                ImGui.TextDisabled(Prefix);
                
                ImGui.SameLine();
                
                string Name = WEE.Interface.CurrentEntity.Name;
                ImGui.SetNextItemWidth(-(ImGui.CalcTextSize("Название").X + ImGui.GetStyle().ItemSpacing.X));
                if(ImGui.InputText("Название", ref Name, 100)){
                    WEE.Interface.CurrentEntity.Name = Name;
                }

                ImGui.Separator();

                if(ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen)){
                    Vector3 Position = new Vector3(WEE.Interface.CurrentEntity.Transform.Position.X, WEE.Interface.CurrentEntity.Transform.Position.Y, WEE.Interface.CurrentEntity.Transform.Position.Z);

                    if(ImGui.DragFloat3("Позиция", ref Position, 0.1f, 0, 0, "%g")){
                        WEE.Interface.CurrentEntity.Transform.Position = new Vector3F(Position.X, Position.Y, Position.Z);
                        WEE.Interface.CurrentEntity.SetTransformDirty();
                    }

                    const float RadToDeg = 180f / System.MathF.PI;
                    const float DegToRad = System.MathF.PI / 180;
                    
                    Vector3 Rotation = new Vector3(
                        WEE.Interface.CurrentEntity.Transform.Rotation.X * RadToDeg,
                        WEE.Interface.CurrentEntity.Transform.Rotation.Y * RadToDeg,
                        WEE.Interface.CurrentEntity.Transform.Rotation.Z * RadToDeg
                    );
                    
                    if(ImGui.DragFloat3("Поворот", ref Rotation, 0.1f, 0, 0, "%g")){
                        WEE.Interface.CurrentEntity.Transform.Rotation = new Vector3F(
                            Rotation.X * DegToRad,
                            Rotation.Y * DegToRad,
                            Rotation.Z * DegToRad
                        );
                        WEE.Interface.CurrentEntity.SetTransformDirty();
                    }
                    
                    Vector3 Scale = new Vector3(WEE.Interface.CurrentEntity.Transform.Scale.X, WEE.Interface.CurrentEntity.Transform.Scale.Y, WEE.Interface.CurrentEntity.Transform.Scale.Z);

                    if(ImGui.DragFloat3("Размер", ref Scale, 0.1f, 0, 0, "%g")){
                        WEE.Interface.CurrentEntity.Transform.Scale = new Vector3F(Scale.X, Scale.Y, Scale.Z);
                        WEE.Interface.CurrentEntity.SetTransformDirty();
                    }
                }

                ImGui.Separator();

                foreach(Component Component in WEE.Interface.CurrentEntity.GetAllComponents().ToList()){
                    GUI.CustomID(Component.GetHashCode(), () => {
                        bool ComponentOpen = ImGui.CollapsingHeader(Component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen);

                        GUI.PopupContextItem("ComponentSettings", () => {
                            if(ImGui.MenuItem("Удалить компонент")){
                                WEE.Interface.CurrentEntity.RemoveComponent(Component);
                            }
                        });

                        if(ComponentOpen){
                            DrawComponentFields(Component);
                        }
                    });
                }

                ImGui.Separator();

                if(ImGui.Button("Добавить компонент", new Vector2(-1, 0))){
                    ImGui.OpenPopup("AddComponentPopup");
                }

                GUI.Popup("AddComponentPopup", () => {
                    foreach(Type ComponentType in WEE.Registry.AvailableComponents){
                        if(ImGui.MenuItem(ComponentType.Name)){
                            MethodInfo Method = typeof(Entity).GetMethod("AddComponent")!;
                            MethodInfo Generic = Method.MakeGenericMethod(ComponentType);
                            Generic.Invoke(WEE.Interface.CurrentEntity, null);
                        }
                    }
                });
            }
        });
    }
    
    private static void DrawComponentFields(WEI.Component Component){
        Type Type = Component.GetType();
        
        foreach(FieldInfo Field in Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Field.GetCustomAttribute<WE_Save>() == null){ continue; }
            HandleMember(
                Component,
                Field.Name,
                Field.FieldType,
                () => Field.GetValue(Component),
                Value => Field.SetValue(Component, Value),
                Field
            );
        }

        foreach(PropertyInfo Property in Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
            if(Property.GetCustomAttribute<WE_Save>() == null || !Property.CanWrite || !Property.CanRead){ continue; }
            HandleMember(
                Component,
                Property.Name,
                Property.PropertyType,
                () => Property.GetValue(Component),
                Value => Property.SetValue(Component, Value),
                Property
            );
        }
    }
    
    private static void HandleMember(object Component, string Label, Type MemberType, Func<object?> Getter, Action<object?> Setter, MemberInfo Info){
        ImGUI GUI = WEE.Interface.ImGUI;
        
        GUI.CustomID(Label, () => {
            foreach(WEEI_InspectorDecorator Decorator in Info.GetCustomAttributes<WEEI_InspectorDecorator>()){
                Decorator.Draw(Label, Component, Info);
            }

            WEEI_InspectorProperty? PropertyAttribute = Info.GetCustomAttribute<WEEI_InspectorProperty>();
            if(PropertyAttribute == null){
                PropertyAttribute = WE.Editor.GetDefault(MemberType);
            }

            if(PropertyAttribute != null){
                PropertyAttribute.Draw(Label, Component, Info, Getter, Setter);
            }else{
                ImGui.TextDisabled($"{Label}: {MemberType.Name}");   
            }
        });
    }
}