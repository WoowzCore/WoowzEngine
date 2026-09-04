using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using WEI;
using WEI_Attribute;
using WEO;
using WLO.Math;

namespace WEE_Interface;

public static class I_Inspector{
    public static void Update(){
        if(!WEE.Interface.WindowInspectorActive){ return; }

        if(ImGui.Begin("Инспектор###Inspector", ref WEE.Interface.WindowInspectorActive)){
            try{
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
                        ImGui.PushID(Component.GetHashCode());
                        
                        bool ComponentOpen = ImGui.CollapsingHeader(Component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen);

                        if(ImGui.BeginPopupContextItem("ComponentSettings")){
                            if(ImGui.MenuItem("Удалить компонент")){
                                WEE.Interface.CurrentEntity.RemoveComponent(Component);
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
                                Generic.Invoke(WEE.Interface.CurrentEntity, null);
                            }
                        }
                        
                        ImGui.EndPopup();
                    }
                }
            }catch(Exception e){
                WL.Logger.Warn("TODO INSPECTOR", e);
            }
        } ImGui.End();
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
        ImGui.PushID(Label);
            
            object? Value = Getter();

            if(Value == null && MemberType == typeof(string)){ Value = ""; }

            if(MemberType.IsGenericType && MemberType.GetGenericTypeDefinition() == typeof(WEO.Asset<>)){
                Type AssetTargetType = MemberType.GetGenericArguments()[0];

                string CurrentKey = (string)(MemberType.GetField("Key")!.GetValue(Value) ?? "");
                bool   UseCache   = (bool)MemberType.GetField("UseCache")!.GetValue(Value)!;
                
                ImGui.BeginGroup();

                    float AvailableWidth = ImGui.GetContentRegionAvail().X;
                    float LabelWidth = ImGui.CalcTextSize(Label).X + 20;
                    const float ButtonWidth = 35;
                    
                    ImGui.SetNextItemWidth(AvailableWidth - LabelWidth - ButtonWidth - ImGui.GetStyle().ItemSpacing.X * 2);

                    string TempKey = CurrentKey;
                    if(ImGui.InputText($"##in_{Label}", ref TempKey, 256)){
                        ConstructorInfo? Constructor = MemberType.GetConstructor([typeof(string)]);
                        Setter(Constructor!.Invoke([TempKey]));
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
                                        ConstructorInfo? Constructor = MemberType.GetConstructor([typeof(string)]);
                                        Setter(Constructor!.Invoke([DroppedKey]));
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
                            ConstructorInfo? Constructor = MemberType.GetConstructor([typeof(string)]);
                            Setter(Constructor!.Invoke([Key]));
                        }
                    }
                    ImGui.EndPopup();
                }
            }else if(MemberType == typeof(float)){
                float V = (float)Value!;
                if(ImGui.DragFloat(Label, ref V, 0.1f)){ Setter(V); }
            }else if(MemberType == typeof(int)){
                int V = (int)Value!;
                if(ImGui.DragInt(Label, ref V)){ Setter(V); }
            }else if(MemberType == typeof(bool)){
                bool V = (bool)Value!;
                if(ImGui.Checkbox(Label, ref V)){ Setter(V); }
            }else if(MemberType == typeof(string)){
                string V = (string)Value!;

                WEEI_MultilineString? MultilineAttribute = Info.GetCustomAttribute<WEEI_MultilineString>();

                if(MultilineAttribute != null){
                    ImGui.Text(Label);
                    if(ImGui.InputTextMultiline($"##{Label}", ref V, 5000, new Vector2(-1, MultilineAttribute.Height))){
                        Setter(V);
                    }
                }else{
                    if(ImGui.InputText(Label, ref V, 200)){
                        Setter(V);
                    }
                }
            }else if(MemberType == typeof(Vector3F)){
                Vector3F V = (Vector3F)Value!;
                Vector3 SysV = new Vector3(V.X, V.Y, V.Z);
                if(ImGui.DragFloat3(Label, ref SysV, 0.1f, 0, 0, "%g")){
                    Setter(new Vector3F(SysV.X, SysV.Y, SysV.Z));
                }
            }else if(MemberType == typeof(Vector2F)){
                Vector2F V = (Vector2F)Value!;
                Vector2 SysV = new Vector2(V.X, V.Y);
                if(ImGui.DragFloat2(Label, ref SysV, 0.1f, 0, 0, "%g")){
                    Setter(new Vector2F(SysV.X, SysV.Y));
                }
            }else if(MemberType == typeof(Color4B)){
                Color4B V = (Color4B)Value!;
                Vector4 SysV = new Vector4(V.R / 255f, V.G / 255f, V.B / 255f, V.A / 255f);
                if(ImGui.ColorEdit4(Label, ref SysV)){
                    Setter(new Color4B((byte)(SysV.X * 255), (byte)(SysV.Y * 255), (byte)(SysV.Z * 255), (byte)(SysV.W * 255)));
                }
            }else if(MemberType.IsEnum){
                string[] Names = Enum.GetNames(MemberType);
                string CurrentName = Value!.ToString()!;

                if(ImGui.BeginCombo(Label, CurrentName)){
                    foreach(string Name in Names){
                        bool IsSelected = CurrentName == Name;
                        if(ImGui.Selectable(Name, IsSelected)){
                            Setter(Enum.Parse(MemberType, Name));
                        }

                        if(IsSelected){
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    
                    ImGui.EndCombo();
                }
            }
            
        ImGui.PopID();
    }
}