using System.Numerics;
using ImGuiNET;
using WEO;
using WLO;

namespace WEE_Interface;

public static class I_Hierarchy{
    private static Entity? __DraggedEntity;
    
    public static void Update(){
        if(!WEE.Interface.WindowHierarchyActive){ return; }

        if(ImGui.Begin("Иерархия###Hierarchy", ref WEE.Interface.WindowHierarchyActive)){
            try{
                if(WEE.Interface.CurrentScene == null){
                    ImGui.Text("Нет активной сцены");
                }else{
                    List<Entity> AllEntities = WEE.Interface.CurrentScene.AllEntity.ToList();
                    ImGui.TextDisabled($"Всего: {AllEntities.Count}, Корней: {WEE.Interface.CurrentScene.Roots.Count()}");

                    try{
                        if(ImGui.BeginChild("HierarchyList", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), ImGuiChildFlags.None, ImGuiWindowFlags.None)){
                            foreach(Entity Entity in WEE.Interface.CurrentScene.Roots.ToList()){
                                if(Entity.Node.Parent == null){
                                    DrawEntityNode(Entity);
                                }
                            }

                            Vector2 RemainingSpace = ImGui.GetContentRegionAvail();
                            ImGui.InvisibleButton("##EmptySpace", new Vector2(ImGui.GetWindowWidth(), Math.Max(RemainingSpace.Y, 50)));

                            if(ImGui.IsItemClicked(ImGuiMouseButton.Left)){
                                WEE.Interface.CurrentEntity = null;
                            }

                            if(ImGui.BeginPopupContextWindow("HierarchyContext")){
                                if(ImGui.MenuItem("Создать Entity")){
                                    Entity NewEntity = new Entity();
                                    WEE.Interface.CurrentScene.Add(NewEntity);
                                    WEE.Interface.CurrentEntity = NewEntity;
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

        if(WEE.Interface.CurrentEntity != null && !IsSelected){
            IsParentOfSelected = WEE.Interface.CurrentEntity.Node.IsDescendantOf(Entity.Node);
        }

        if(IsParentOfSelected){
            ImGui.SetNextItemOpen(true);
        }

        ImGuiTreeNodeFlags Flags = IsSelected || IsParentOfSelected ? ImGuiTreeNodeFlags.Selected : 0;
        Flags |= ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowOverlap;

        bool IsLeaf = Entity.Node.Children.Count == 0;
        if(IsLeaf){ Flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen; }
        
        int PushedColors = 0;
        if(IsSelected){
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.26f, 0.59f, 0.98f, 0.67f)); 
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.26f, 0.59f, 0.98f, 0.8f));
            PushedColors = 2;
        }else if(IsParentOfSelected){
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.4f, 0.4f, 0.4f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.5f, 0.5f, 0.5f, 0.4f));
            PushedColors = 2;
        }

        bool Opened = ImGui.TreeNodeEx($"[{Entity.ID}] {Entity.Name}###{Entity.GetHashCode()}", Flags);

        if(PushedColors > 0){ ImGui.PopStyleColor(PushedColors); }

        if(ImGui.IsItemClicked(ImGuiMouseButton.Left) || ImGui.IsItemClicked(ImGuiMouseButton.Right)){ WEE.Interface.CurrentEntity = Entity; }

        if(ImGui.BeginDragDropSource()){
            __DraggedEntity = Entity;
            ImGui.SetDragDropPayload("ENTITY_HIERARCHY", IntPtr.Zero, 0);
            ImGui.Text($"Перенос: {Entity.Name}");
            ImGui.EndDragDropSource();
        }

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
        
        if(ImGui.BeginDragDropTarget()){
            unsafe{
                ImGuiPayloadPtr Payload = ImGui.AcceptDragDropPayload("ENTITY_HIERARCHY", ImGuiDragDropFlags.AcceptNoDrawDefaultRect | ImGuiDragDropFlags.AcceptBeforeDelivery);
                if(Payload.NativePtr != null && __DraggedEntity != null && __DraggedEntity != Entity){
                    Vector2 ItemMin = ImGui.GetItemRectMin();
                    Vector2 ItemMax = ImGui.GetItemRectMax();
                    float MouseY = ImGui.GetMousePos().Y;
                    float ItemHeight = ItemMax.Y - ItemMin.Y;
                    float RelativeY = (MouseY - ItemMin.Y) / ItemHeight;

                    ImDrawListPtr DrawList = ImGui.GetWindowDrawList();
                    uint Color = ImGui.GetColorU32(ImGuiCol.DragDropTarget);
                    
                    if(!Entity.Node.IsDescendantOf(__DraggedEntity.Node)){
                        if(RelativeY < 0.25f){
                            DrawList.AddLine(ItemMin, new Vector2(ItemMax.X, ItemMin.Y), Color, 2);
                        }else if(RelativeY > 0.75f){
                            DrawList.AddLine(new Vector2(ItemMin.X, ItemMax.Y), ItemMax, Color, 2);
                        }else{
                            DrawList.AddRect(ItemMin, ItemMax, Color, 0, ImDrawFlags.None, 2);
                        }

                        if(Payload.IsDelivery()){
                            if(RelativeY < 0.25f){
                                MoveEntityRelative(__DraggedEntity, Entity, 0);
                            }else if(RelativeY > 0.75f){
                                MoveEntityRelative(__DraggedEntity, Entity, 1);
                            }else{
                                __DraggedEntity.Node.SetParent(Entity.Node);
                            }
                        }
                    }
                }
            }
            ImGui.EndDragDropTarget();
        }

        if(ImGui.BeginPopupContextItem()){
            WEE.Interface.CurrentEntity = Entity;
            
            if(ImGui.MenuItem("Создать Entity")){
                Entity NewEntity = new Entity();
                NewEntity.Node.SetParent(Entity.Node);
                WEE.Interface.CurrentEntity = NewEntity;
            }

            if(ImGui.MenuItem("Дублировать")){
                Entity Dupe = Entity.Duplicate();
                if(Entity.Node.Parent != null){
                    Dupe.Node.SetParent(Entity.Node.Parent);
                }else{
                    WEE.Interface.CurrentScene?.Add(Dupe);
                }
                WEE.Interface.CurrentEntity = Dupe;
            }
            
            ImGui.Separator();
            
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
}