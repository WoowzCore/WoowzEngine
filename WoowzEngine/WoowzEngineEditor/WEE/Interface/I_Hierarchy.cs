using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using WEO;
using WLO;

namespace WEE_Interface;

public static class I_Hierarchy{
    private static Entity? __DraggedEntity;

    private static Entity? __LastSelectedEntity;
    private static bool    __NeedToScroll;
    
    public static void Update(){
        if(!WEE.Interface.WindowHierarchyActive){ return; }

        if(ImGui.Begin("Иерархия###Hierarchy", ref WEE.Interface.WindowHierarchyActive)){
            try{
                if(WEE.Interface.CurrentScene == null){
                    ImGui.Text("Нет активной сцены");
                }else{
                    if(WEE.Interface.CurrentEntity != __LastSelectedEntity){
                        __LastSelectedEntity = WEE.Interface.CurrentEntity;
                        if(__LastSelectedEntity != null){ __NeedToScroll = true; }
                    }
                    
                    List<Entity> AllEntities = WEE.Interface.CurrentScene.AllEntity.ToList();
                    ImGui.TextDisabled($"Всего: {AllEntities.Count}, Корней: {WEE.Interface.CurrentScene.Roots.Count()}");

                    try{
                        if(ImGui.BeginChild("HierarchyList", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None)){
                            foreach(Entity Entity in WEE.Interface.CurrentScene.Roots.ToList()){
                                if(Entity.Node.Parent == null){
                                    DrawEntityNode(Entity);
                                }
                            }

                            Vector2 RemainingSpace = ImGui.GetContentRegionAvail();
                            RemainingSpace.Y = Math.Max(RemainingSpace.Y, 25);
                            ImGui.Dummy(RemainingSpace);
                            if(ImGui.IsItemClicked(ImGuiMouseButton.Left)){
                                WEE.Interface.CurrentEntity = null;
                            }
                            
                            __HandleHierarchyDragDrop(null);

                            if(ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems)){
                                if(ImGui.MenuItem("Создать Entity")){
                                    Entity NewEntity = new Entity();
                                    WEE.Interface.CurrentScene.Add(NewEntity);
                                    WEE.Interface.CurrentEntity = NewEntity;
                                }
                                ImGui.EndPopup();
                            }
                        }
                    }finally{
                        ImGui.EndChild();
                    }
                }
            }catch(Exception e){
                WL.Logger.Warn("TODO HIERARCHY", e);
            }
        } ImGui.End();
    }
    
     private static void DrawEntityNode(Entity Entity){
        bool IsSelected = WEE.Interface.CurrentEntity == Entity;
        bool IsParentOfSelected = false;
        bool IsPrefab = Entity.IsPartOfPrefab;
        bool CanDrag = !IsPrefab || Entity == Entity.PrefabRoot;

        if(WEE.Interface.CurrentEntity != null && !IsSelected){
            IsParentOfSelected = WEE.Interface.CurrentEntity.Node.IsDescendantOf(Entity.Node);
        }

        if(IsParentOfSelected){
            ImGui.SetNextItemOpen(true);
        }

        if(IsSelected && __NeedToScroll){
            ImGui.SetScrollHereY(0.5f);
            __NeedToScroll = false;
        }

        ImGuiTreeNodeFlags Flags = IsSelected || IsParentOfSelected || IsPrefab ? ImGuiTreeNodeFlags.Selected : 0;
        Flags |= ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowOverlap;

        bool IsLeaf = Entity.Node.Children.Count == 0;
        if(IsLeaf){ Flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen; }
        
        int PushedColors = 0;
        if(IsSelected){
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(1, 0.8f, 0, 0.6f)); 
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1, 0.85f, 0.2f, 0.75f));
            PushedColors = 2;
        }else if(IsParentOfSelected){
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.5f, 0.45f, 0, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.6f, 0.55f, 0.1f, 0.4f));
            PushedColors = 2;
        }
        
        if(IsPrefab && !IsSelected){
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.65f, 0.85f, 1));
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.2f, 0.3f, 0.4f, 0.4f));
            PushedColors += 2;
        }

        bool Opened = ImGui.TreeNodeEx($"[{Entity.ID}] {Entity.Name}###{Entity.GetHashCode()}", Flags);

        if(PushedColors > 0){ ImGui.PopStyleColor(PushedColors); }

        if(ImGui.IsItemClicked(ImGuiMouseButton.Left) || ImGui.IsItemClicked(ImGuiMouseButton.Right)){ WEE.Interface.CurrentEntity = Entity; }

        if(CanDrag && ImGui.BeginDragDropSource()){
            __DraggedEntity = Entity;
            ImGui.SetDragDropPayload("ENTITY_HIERARCHY", IntPtr.Zero, 0);
            ImGui.Text($"Перенос: {Entity.Name}");
            ImGui.EndDragDropSource();
        }
        
        __HandleHierarchyDragDrop(Entity);

        if(ImGui.BeginPopupContextItem()){
            WEE.Interface.CurrentEntity = Entity;

            if(!Entity.IsPartOfPrefab){
                if(ImGui.MenuItem("Создать Entity")){
                    Entity NewEntity = new Entity();
                    NewEntity.Node.SetParent(Entity.Node);
                    WEE.Interface.CurrentEntity = NewEntity;
                }

                if(ImGui.MenuItem("Превратить в Prefab")){
                    I_Menu.SaveEntityAsPrefab(Entity);
                }
            }else{
                if(ImGui.MenuItem("Разобрать Prefab")){
                    Entity.SourcePrefab = null;
                }
            }
            
            if(ImGui.MenuItem("Дублировать")){
                Entity Dupe = Entity.Duplicate();
                
                if(Entity.Node.Parent != null){
                    Dupe.Node.SetParent(Entity.Node.Parent);
                    int Index = Entity.Node.Parent.Children.IndexOf(Entity.Node);
                    Entity.Node.Parent.MoveChild(Dupe.Node, Index + 1);
                }else{
                    WEE.Interface.CurrentScene!.Add(Dupe);
                    int Index = WEE.Interface.CurrentScene!.Roots.ToList().IndexOf(Entity);
                    WEE.Interface.CurrentScene!.MoveRoot(Dupe, Index + 1);
                }
                
                WEE.Interface.CurrentEntity = Dupe;
            }
            
            ImGui.Separator();
            
            if(ImGui.MenuItem("Удалить")){ Entity.Destroy(); WEE.Interface.CurrentEntity = null; }
            
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

    private static void __HandleHierarchyDragDrop(Entity? TargetEntity){
        if(!ImGui.BeginDragDropTarget()){ return; }

        void MoveEntityRelative(Entity Dragged, Entity Target, int Direction){
            HierarchyNode<Entity>? ParentNode = Target.Node.Parent;
            Dragged.Node.SetParent(ParentNode);

            if(ParentNode != null){
                int TargetIndex = ParentNode.Children.IndexOf(Target.Node);
                if(TargetIndex != -1){
                    ParentNode.MoveChild(Dragged.Node, Math.Clamp(TargetIndex + Direction, 0, ParentNode.Children.Count));
                }
            }else{
                Scene? Scene = WEE.Interface.CurrentScene;
                if(Scene != null){
                    List<Entity> Roots = Scene.Roots.ToList();
                    int TargetIndex = Roots.IndexOf(Target);
                    if(TargetIndex != -1){
                        Scene.MoveRoot(Dragged, TargetIndex + Direction);
                    }
                }
            }
        }
        
        unsafe{
            const ImGuiDragDropFlags Flags = ImGuiDragDropFlags.AcceptNoDrawDefaultRect | ImGuiDragDropFlags.AcceptBeforeDelivery;

            ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ENTITY_HIERARCHY", Flags);
            bool IsAsset = false;

            if(Payload.NativePtr == null){
                Payload = ImGui.AcceptDragDropPayload("ASSET_KEY", Flags);
                IsAsset = Payload.NativePtr != null;
            }

            if(Payload.NativePtr != null){
                Vector2 ItemMin = ImGui.GetItemRectMin();
                Vector2 ItemMax = ImGui.GetItemRectMax();
                float MouseY = ImGui.GetMousePos().Y;
                float RelativeY = (MouseY - ItemMin.Y) / (ItemMax.Y - ItemMin.Y);

                if(!IsAsset && __DraggedEntity != null && TargetEntity != null){
                    if(__DraggedEntity == TargetEntity || TargetEntity.Node.IsDescendantOf(__DraggedEntity.Node)){
                        ImGui.EndDragDropTarget();
                        return;
                    }
                }

                ImDrawListPtr DrawList = ImGui.GetWindowDrawList();
                uint Color = ImGui.GetColorU32(ImGuiCol.DragDropTarget);

                int DropMode = 1;

                if(TargetEntity != null){
                    if(RelativeY < 0.25f){
                        DrawList.AddLine(ItemMin, new Vector2(ItemMax.X, ItemMin.Y), Color, 2);
                        DropMode = 0;
                    }
                    else if(RelativeY > 0.75f){
                        DrawList.AddLine(new Vector2(ItemMin.X, ItemMax.Y), ItemMax, Color, 2);
                        DropMode = 2;
                    }
                    else{
                        DrawList.AddRect(ItemMin, ItemMax, Color, 0, ImDrawFlags.None, 2);
                        DropMode = 1;
                    }
                }

                if(Payload.IsDelivery()){
                    Entity? Subject = null;

                    if(!IsAsset){
                        Subject = __DraggedEntity;
                    }else{
                        string Key = Marshal.PtrToStringAnsi(Payload.Data)!;
                        object? Asset = WE.Asset.Resolve<object>(WE.Asset.GetID(Key));

                        if(Asset is Prefab){
                            Subject = Entity.FromPrefab(new Asset<Prefab>(Key));
                            WEE.Interface.CurrentScene?.Add(Subject);
                        }
                    }

                    if(Subject != null){
                        if(TargetEntity == null){
                            Subject.Node.SetParent(null);
                        }else{
                            if(TargetEntity.IsPartOfPrefab){
                                ImGui.EndDragDropTarget();
                                return;
                            }
                            
                            if(DropMode == 0){
                                MoveEntityRelative(Subject, TargetEntity, 0);
                            }
                            else if(DropMode == 2){
                                MoveEntityRelative(Subject, TargetEntity, 1);
                            }
                            else{
                                Subject.Node.SetParent(TargetEntity.Node);
                            }
                        }

                        WEE.Interface.CurrentEntity = Subject;
                    }
                }
            }
        }
        ImGui.EndDragDropTarget();
    }
}