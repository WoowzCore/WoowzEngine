using ImGuiNET;
using WEO;
using WLO;

namespace WEE_Interface;

public static class I_Hierarchy{
    private static Entity? __DraggedEntity = null;
    
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
                        if(ImGui.BeginChild("HierarchyList")){
                            foreach(Entity Entity in WEE.Interface.CurrentScene.Roots.ToList()){
                                if(Entity.Node.Parent == null){
                                    DrawEntityNode(Entity);
                                }
                            }

                            if(ImGui.IsMouseDown(0) && ImGui.IsWindowHovered()){
                                WEE.Interface.CurrentEntity = null;
                            }

                            if(ImGui.BeginPopupContextWindow("HierarchyContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems)){
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
                WL.Logger.Warn("TODO HIERARCHY " + e.Message + "\n" + e.StackTrace);
            }
        } ImGui.End();
    }
    
     private static void DrawEntityNode(Entity Entity){
        bool IsLeaf = Entity.Node.Children.Count == 0;
        
        ImGuiTreeNodeFlags Flags = (WEE.Interface.CurrentEntity == Entity ? ImGuiTreeNodeFlags.Selected : 0);
        Flags |= ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        if(IsLeaf){
            Flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        bool Opened = ImGui.TreeNodeEx($@"{Entity.Name}###{Entity.GetHashCode()}", Flags);

        if(ImGui.IsItemClicked()){ WEE.Interface.CurrentEntity = Entity; }

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
                WEE.Interface.CurrentEntity = NewEntity;
            }

            if(ImGui.MenuItem("Дублировать")){
                Entity NewEntity = Entity.Duplicate();
                if(Entity.Node.Parent != null){
                    NewEntity.Node.SetParent(Entity.Node.Parent);
                }else{
                    WEE.Interface.CurrentScene?.Add(NewEntity);
                }
                WEE.Interface.CurrentEntity = NewEntity;
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
}