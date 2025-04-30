using System.Collections.Generic;
using Enums;
using Managers;
using Models;
using Models.Nodes;
using Presenters;
using UnityEngine;
using Zenject;

namespace Factories
{
    public class NodePresenterFactory : PlaceholderFactory<Vector2, NodeType, BaseNodePresenter>
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

        public override BaseNodePresenter Create(Vector2 position, NodeType nodeType)
        {
            GameObject prefabToInstantiate;
            switch (nodeType)
            {
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
                // Action node tipleri için prefab seçimi
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
                case NodeType.RobotAnimationAction:
                    prefabToInstantiate = _config.robotAnimationActionNode;
                    break;
                default:
                    Debug.LogError("Bilinmeyen NodeType: " + nodeType);
                    return null;
            }

            var go = _container.InstantiatePrefab(prefabToInstantiate,
                _graphManager.contentTransform);
            var nodePresenter = go.GetComponent<BaseNodePresenter>();
            List<Port> ports = new List<Port>();
            foreach (var port in nodePresenter.GetComponentsInChildren<PortPresenter>())
            {
                if (!(port is EventPortPresenter)) // Event port'ları hariç tut
                {
                    ports.Add(port.Model);
                }
            }

            BaseNode node = null;
            switch (nodeType)
            {
                case NodeType.Start:
                    node = new StartNode(LTGUtility.GenerateSID(), "Start Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.Finish:
                    node = new FinishNode(LTGUtility.GenerateSID(), "Finish Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.TouchNode:
                    node = new TouchNode(LTGUtility.GenerateSID(), "Touch Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.GrabNode:
                    node = new GrabNode(LTGUtility.GenerateSID(), "Grab Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.WaitForNextNode:
                    node = new GrabNode(LTGUtility.GenerateSID(), "Wait For Next Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.LookNode:
                    node = new LookNode(LTGUtility.GenerateSID(), "Look Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.LogicalOR:
                    node = new LogicNode(LTGUtility.GenerateSID(), "Logical OR Node", _config.defaultNodeColor, true, ports);
                    break;
                case NodeType.LogicalAND:
                    node = new LogicNode(LTGUtility.GenerateSID(), "Logical AND Node", _config.defaultNodeColor, true, ports);
                    break;
                // Action node tiplerini oluştur
                case NodeType.PlaySoundAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Ses Çal", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.PlaySound,
                    };
                    break;
                case NodeType.ChangeMaterialAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Materyal Değiştir", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.ChangeMaterial 
                    };
                    break;
                case NodeType.ChangePositionAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Nesne Hareket Ettir", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.ChangePosition 
                    };
                    break;

                case NodeType.RobotAnimationAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Nesne Hareket Ettir", _config.defaultNodeColor, true, ports)
                    {
                        Type = ActionNode.ActionType.RobotAnimationAction
                    };
                    break;
                case NodeType.ChangeRotationAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Nesne rotasyonu ayarla.", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.ChangeRotation 
                    };
                    break;
                case NodeType.ChangeScaleAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Nesne hacmini ayarla.", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.ChangeScale 
                    };
                    break;
                case NodeType.ToggleObjectAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Nesne Aç/Kapat", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.ToggleObject 
                    };
                    break;
                case NodeType.PlayAnimationAction:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Animasyon Oynat", _config.defaultNodeColor, true, ports) 
                    { 
                        Type = ActionNode.ActionType.PlayAnimation 
                    };
                    break;
                case NodeType.DescriptionActionNode:
                    node = new ActionNode(LTGUtility.GenerateSID(), "Yazı göster", _config.defaultNodeColor, true, ports)
                    {
                        Type = ActionNode.ActionType.DescriptionAction
                    };
                    break;
            }

            nodePresenter.Initialize(node);
            nodePresenter.GetComponent<RectTransform>().localPosition = Vector3.zero;
            
            return nodePresenter;
        }
    }
}