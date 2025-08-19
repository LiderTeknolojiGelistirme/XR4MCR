using System.Collections.Generic;
using Commands;
using Enums;
using Managers;
using UnityEngine;
using Viroo.Interactions;
using Zenject;

namespace Actions
{
    public class CreateNodeAction : CreateObjectAction
    {
        private Dictionary<NodeType, string> canvasNodePrefabIds = new Dictionary<NodeType, string>
        {
            // Temel Node Tipleri
            { NodeType.Ghost, "ghost_node" },
            { NodeType.Start, "start_node" },
            { NodeType.Finish, "finish_node" },
            { NodeType.TouchNode, "touch_node" },
            { NodeType.GrabNode, "grab_node" },
            { NodeType.WaitForNextNode, "wait_for_next_node" },
            { NodeType.LookNode, "look_node" },
            { NodeType.LogicalOR, "logical_or_node" },
            { NodeType.LogicalAND, "logical_and_node" },
            { NodeType.ToolTouchNode, "tool_touch_node" },
            
            // Action Node Tipleri
            { NodeType.PlaySoundAction, "play_sound_action_node" },
            { NodeType.ChangeMaterialAction, "change_material_action_node" },
            { NodeType.ChangePositionAction, "change_position_action_node" },
            { NodeType.ChangeRotationAction, "change_rotation_action_node" },
            { NodeType.ChangeScaleAction, "change_scale_action_node" },
            { NodeType.ToggleObjectAction, "toggle_object_action_node" },
            { NodeType.PlayAnimationAction, "play_animation_action_node" },
            { NodeType.DescriptionActionNode, "description_action_node" },
            { NodeType.RobotAnimationAction, "robot_animation_action_node" },
            { NodeType.WorldDescriptionActionNode, "world_description_action_node" },
            { NodeType.VFXActionNode, "vfx_action_node" },
            { NodeType.HighlightObjectActionNode, "highlight_object_action_node" }
        };
        public NodeType nodeType;

        [Inject] private GraphManager _graphManager;
        [Inject] private XRInputManager _inputManager;

        protected override void LocalExecuteImplementation(string data)
        {
            base.LocalExecuteImplementation(data);
            if (_inputManager.GetCanvasPointerPosition(_graphManager).x > 680f)
            {
                UndoRedoManager.Execute(new CreateNodeCommand(nodeType, _graphManager));
            }
            else
            {
                UndoRedoManager.Execute(new CreateNodeCommand(nodeType, _graphManager,
                    _inputManager.GetCanvasPointerPosition(_graphManager)));
            }
        }
        
        public void CreateNode()
        {
            Vector2 position;
            
            if (_inputManager.GetCanvasPointerPosition(_graphManager).x > 680f)
            {
                position = Vector2.zero; // Default position
            }
            else
            {
                position = _inputManager.GetCanvasPointerPosition(_graphManager);
            }
            InstantiatePrefabId= canvasNodePrefabIds[nodeType];
            // todo yaratilma pozisyonu eklenecek
            Execute();
        }
    }
}