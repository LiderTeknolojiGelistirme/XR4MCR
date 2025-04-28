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
                case NodeType.GetKeyDownL:
                    prefabToInstantiate = _config.getKeyDownNodeL;
                    break;
                case NodeType.GetKeyDownT:
                    prefabToInstantiate = _config.getKeyDownNodeT;
                    break;
                case NodeType.GetKeyDownG:
                    prefabToInstantiate = _config.getKeyDownNodeG;
                    break;
                default:
                    Debug.LogError("Bilinmeyen NodeType: " + nodeType);
                    return null;
            }

            var go = _container.InstantiatePrefab(prefabToInstantiate,
                _graphManager.transform);
            var nodePresenter = go.GetComponent<BaseNodePresenter>();

            BaseNode node = null;
            switch (nodeType)
            {
                case NodeType.Start:
                    node = new StartNode(LTGUtility.GenerateSID(), "Start Node", _config.defaultNodeColor, true);
                    break;
                case NodeType.Finish:
                    node = new FinishNode(LTGUtility.GenerateSID(), "Finish Node", _config.defaultNodeColor, true);
                    break;
                case NodeType.GetKeyDownL:
                    node = new GetKeyDownNode(LTGUtility.GenerateSID(), "GetKeyDown Node", _config.defaultNodeColor, true);
                    break;
                case NodeType.GetKeyDownT:
                    node = new GetKeyDownNode(LTGUtility.GenerateSID(), "GetKeyDown Node", _config.defaultNodeColor, true);
                    break;
                case NodeType.GetKeyDownG:
                    node = new GetKeyDownNode(LTGUtility.GenerateSID(), "GetKeyDown Node", _config.defaultNodeColor, true);
                    break;
            }

            nodePresenter.Initialize(node);


            foreach (var port in nodePresenter.GetComponentsInChildren<PortPresenter>())
            {
                _container.Inject(port);
            }

            nodePresenter.GetComponent<RectTransform>().localPosition = Vector3.zero;
            
            return nodePresenter;
        }
    }
}