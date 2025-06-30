using System;
using System.Collections.Generic;
using Enums;
using Managers;
using Models;
using Models.Nodes;
using Presenters;
using Presenters.NodePresenters;
using UnityEngine;
using Zenject;

namespace Factories
{
    public class NodePresenterFactory : PlaceholderFactory<Vector2, NodeType, BaseNode, BaseNodePresenter>
    {
        private readonly DiContainer _container;
        private readonly NodeConfig _config;
        private readonly GraphManager _graphManager;

        public NodePresenterFactory(DiContainer container, NodeConfig config, GraphManager graphManager)
        {
            _container = container;
            _config = config;
            _graphManager = graphManager;
        }

        public override BaseNodePresenter Create(Vector2 position, NodeType nodeType, BaseNode baseNode = null)
        {
            GameObject prefabToInstantiate;
            switch (nodeType)
            {
                case NodeType.Ghost:
                    prefabToInstantiate = _config.GhostNode;
                    break;
                case NodeType.Start:
                    prefabToInstantiate = _config.startNode;
                    break;
                case NodeType.Finish:
                    prefabToInstantiate = _config.finishNode;
                    break;
                case NodeType.TouchNode:
                    prefabToInstantiate = _config.touchNode;
                    break;
                case NodeType.GrabNode:
                    prefabToInstantiate = _config.grabNode;
                    break;
                case NodeType.WaitForNextNode:
                    prefabToInstantiate = _config.waitForNextNode;
                    break;
                case NodeType.LookNode:
                    prefabToInstantiate = _config.lookNode;
                    break;
                case NodeType.LogicalOR:
                    prefabToInstantiate = _config.LogicalOR;
                    break;
                case NodeType.LogicalAND:
                    prefabToInstantiate = _config.LogicalAND;
                    break;
                // Prefab selection for action node types
                case NodeType.PlaySoundAction:
                    prefabToInstantiate = _config.playSoundActionNode;
                    break;
                case NodeType.ChangeMaterialAction:
                    prefabToInstantiate = _config.materialChangeNodePresenter;
                    break;
                case NodeType.ChangePositionAction:
                    prefabToInstantiate = _config.changePositionActionNode;
                    break;
                case NodeType.ChangeRotationAction:
                    prefabToInstantiate = _config.changeRotationActionNode;
                    break;
                case NodeType.ChangeScaleAction:
                    prefabToInstantiate = _config.changeScaleActionNode;
                    break;
                case NodeType.ToggleObjectAction:
                    prefabToInstantiate = _config.toggleObjectActionNode;
                    break;
                case NodeType.PlayAnimationAction:
                    prefabToInstantiate = _config.playAnimationActionNode;
                    break;
                case NodeType.DescriptionActionNode:
                    prefabToInstantiate = _config.descriptionActionNode;
                    break;
                case NodeType.WorldDescriptionActionNode:
                    prefabToInstantiate = _config.worldDescriptionActionNode;
                    break;
                case NodeType.RobotAnimationAction:
                    prefabToInstantiate = _config.robotAnimationActionNode;
                    break;
                case NodeType.VFXActionNode:
                    prefabToInstantiate = _config.vfxActionNode;
                    break;
                case NodeType.HighlightObjectActionNode:
                    prefabToInstantiate = _config.highlightObjectActionNode;
                    break;
                case NodeType.ToolTouchNode:
                    prefabToInstantiate = _config.toolTouchNode;
                    break;

                default:
                    Debug.LogError("Unknown NodeType: " + nodeType);
                    return null;
            }

            var go = _container.InstantiatePrefab(prefabToInstantiate,
                _graphManager.contentTransform);
            var nodePresenter = go.GetComponent<BaseNodePresenter>();
            List<Port> ports = new List<Port>();
            foreach (var port in nodePresenter.GetComponentsInChildren<PortPresenter>())
            {
                if (!(port is EventPortPresenter)) // Exclude event ports
                {
                    ports.Add(port.Model);
                }
            }

            BaseNode node = null;

            if (baseNode == null)
            {
                switch (nodeType)
                {
                    case NodeType.Ghost:
                        node = new GhostNode(LTGUtility.GenerateSID(), "Ghost", _config.defaultNodeColor, true, ports)
                        {
                            Type = NodeType.Ghost
                        };
                        break;
                    case NodeType.Start:
                        node = new StartNode(LTGUtility.GenerateSID(), "Start Node", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.Start
                        };
                        break;
                    case NodeType.Finish:
                        node = new FinishNode(LTGUtility.GenerateSID(), "Finish Node", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.Finish
                        };
                        break;
                    case NodeType.TouchNode:
                        node = new TouchNode(LTGUtility.GenerateSID(), "Touch Node", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.TouchNode
                        };
                        break;
                    case NodeType.GrabNode:
                        node = new GrabNode(LTGUtility.GenerateSID(), "Grab Node", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.GrabNode
                        };
                        break;
                    case NodeType.WaitForNextNode:
                        node = new Models.Nodes.WaitForNextNode(LTGUtility.GenerateSID(), "Wait For Next Node", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.WaitForNextNode
                        };
                        break;
                    case NodeType.LookNode:
                        node = new LookNode(LTGUtility.GenerateSID(), "Look Node", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.LookNode
                        };
                        break;
                    case NodeType.LogicalOR:
                        node = new LogicNode(LTGUtility.GenerateSID(), "Logical OR Node", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.LogicalOR
                        };
                        break;
                    case NodeType.LogicalAND:
                        node = new LogicNode(LTGUtility.GenerateSID(), "Logical AND Node", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.LogicalAND
                        };
                        break;
                    // Create action node types
                    case NodeType.PlaySoundAction:
                        node = new AudioActionNode(LTGUtility.GenerateSID(), "Play Sound", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.PlaySoundAction
                        };
                        break;
                    case NodeType.ChangeMaterialAction:
                        node = new ChangeMaterialActionNode(LTGUtility.GenerateSID(), "Change Material", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.ChangeMaterialAction
                        };
                        break;
                    case NodeType.ChangePositionAction:
                        node = new ChangePositionActionNode(LTGUtility.GenerateSID(), "Move Object", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.ChangePositionAction
                        };
                        break;

                    case NodeType.RobotAnimationAction:
                        node = new RobotAnimationActionNode(LTGUtility.GenerateSID(), "Robot Animation", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.RobotAnimationAction
                        };
                        break;
                    case NodeType.ChangeRotationAction:
                        node = new ChangeRotationActionNode(LTGUtility.GenerateSID(), "Set Object Rotation",
                            _config.defaultNodeColor, true, ports)
                        {
                            Type = NodeType.ChangeRotationAction
                        };
                        break;
                    case NodeType.ChangeScaleAction:
                        node = new ChangeScaleActionNode(LTGUtility.GenerateSID(), "Set Object Scale",
                            _config.defaultNodeColor, true, ports)
                        {
                            Type = NodeType.ChangeScaleAction
                        };
                        break;
                    case NodeType.ToggleObjectAction:
                        node = new ToggleObjectActionNode(LTGUtility.GenerateSID(), "Toggle Object", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.ToggleObjectAction
                        };
                        break;
                    case NodeType.PlayAnimationAction:
                        node = new PlayAnimationActionNode(LTGUtility.GenerateSID(), "Play Animation", _config.defaultNodeColor,
                            true, ports)
                        {
                            Type = NodeType.PlayAnimationAction
                        };
                        break;
                    case NodeType.DescriptionActionNode:
                        node = new DescriptionActionNode(LTGUtility.GenerateSID(), "Show Text", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.DescriptionActionNode
                        };
                        break;
                    case NodeType.WorldDescriptionActionNode:
                        node = new WorldDescriptionActionNode(LTGUtility.GenerateSID(), "Show World Text", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.WorldDescriptionActionNode
                        };
                        break;
                    case NodeType.VFXActionNode:
                        node = new VFXActionNode(LTGUtility.GenerateSID(), "VFX Effect", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.VFXActionNode
                        };
                        break;
                    case NodeType.HighlightObjectActionNode:
                        node = new HighlightObjectActionNode(LTGUtility.GenerateSID(), "Highlight Effect", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.HighlightObjectActionNode
                        };
                        break;
                    case NodeType.ToolTouchNode:
                        node = new ToolTouchNode(LTGUtility.GenerateSID(), "Tool Touch", _config.defaultNodeColor, true,
                            ports)
                        {
                            Type = NodeType.ToolTouchNode
                        };
                        break;
                }
            }
            else
            {
                switch (nodeType)
                {
                    case NodeType.Ghost:
                        node = baseNode as GhostNode;
                        if (node != null) node.Type = NodeType.Ghost;
                        break;
                    case NodeType.Start:
                        node = baseNode as StartNode;
                        if (node != null) node.Type = NodeType.Start;
                        break;
                    case NodeType.Finish:
                        node = baseNode as FinishNode;
                        if (node != null) node.Type = NodeType.Finish;
                        break;
                    case NodeType.TouchNode:
                        node = baseNode as TouchNode;
                        if (node != null) node.Type = NodeType.TouchNode;
                        break;
                    case NodeType.GrabNode:
                        node = baseNode as GrabNode;
                        if (node != null) node.Type = NodeType.GrabNode;
                        break;
                    case NodeType.WaitForNextNode:
                        node = baseNode as Models.Nodes.WaitForNextNode;
                        if (node != null) node.Type = NodeType.WaitForNextNode;
                        break;
                    case NodeType.LookNode:
                        node = baseNode as LookNode;
                        if (node != null) node.Type = NodeType.LookNode;
                        break;
                    case NodeType.LogicalOR:
                        node = baseNode as LogicNode;
                        if (node != null) node.Type = NodeType.LogicalOR;
                        break;
                    case NodeType.LogicalAND:
                        node = baseNode as LogicNode;
                        if (node != null) node.Type = NodeType.LogicalAND;
                        break;
                    // Create action node types
                    case NodeType.PlaySoundAction:
                        node = new AudioActionNode(baseNode)
                        {
                            Type = NodeType.PlaySoundAction,
                        };
                        break;
                    case NodeType.ChangeMaterialAction:
                        node = new ChangeMaterialActionNode(baseNode)
                        {
                            Type = NodeType.ChangeMaterialAction
                        };
                        break;
                    case NodeType.ChangePositionAction:
                        node = new ChangePositionActionNode(baseNode)
                        {
                            Type = NodeType.ChangePositionAction
                        };
                        break;

                    case NodeType.RobotAnimationAction:
                        node = new RobotAnimationActionNode(baseNode)
                        {
                            Type = NodeType.RobotAnimationAction
                        };
                        break;
                    case NodeType.ChangeRotationAction:
                        node = new ChangeRotationActionNode(baseNode)
                        {
                            Type = NodeType.ChangeRotationAction
                        };
                        break;
                    case NodeType.ChangeScaleAction:
                        node = new ChangeScaleActionNode(baseNode)
                        {
                            Type = NodeType.ChangeScaleAction
                        };
                        break;
                    case NodeType.ToggleObjectAction:
                        node = new ToggleObjectActionNode(baseNode)
                        {
                            Type = NodeType.ToggleObjectAction
                        };
                        break;
                    case NodeType.PlayAnimationAction:
                        node = new PlayAnimationActionNode(baseNode)
                        {
                            Type = NodeType.PlayAnimationAction
                        };
                        break;
                    case NodeType.DescriptionActionNode:
                        node = new DescriptionActionNode(baseNode)
                        {
                            Type = NodeType.DescriptionActionNode
                        };
                        break;
                    case NodeType.WorldDescriptionActionNode:
                        node = new WorldDescriptionActionNode(baseNode)
                        {
                            Type = NodeType.WorldDescriptionActionNode
                        };
                        break;
                    case NodeType.VFXActionNode:
                        node = new VFXActionNode(baseNode)
                        {
                            Type = NodeType.VFXActionNode
                        };
                        break;
                    case NodeType.HighlightObjectActionNode:
                        node = new HighlightObjectActionNode(baseNode)
                        {
                            Type = NodeType.HighlightObjectActionNode
                        };
                        break;
                    case NodeType.ToolTouchNode:
                        node = new ToolTouchNode(baseNode)
                        {
                            Type = NodeType.ToolTouchNode
                        };
                        break;
                }
            }

            nodePresenter.Initialize(node);

            nodePresenter.GetComponent<RectTransform>().localPosition = Vector3.zero;

            return nodePresenter;
        }
    }
}