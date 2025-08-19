using System;
using System.Collections.Generic;
using Enums;
using Managers;
using Models;
using Presenters;
using UnityEngine;
using Viroo.Interactions;
using Zenject;

namespace Actions
{
    public class InitializeNodeAction : BroadcastObjectAction
    {
        [Inject]private GraphManager _graphManager;
        private GameObject _nodeObject;
        private NodeType _nodeType;

        // Broadcast için veri yapısı
        [System.Serializable]
        public class NodeInitData
        {
            public NodeType nodeType;
            public Vector2 position;
        }

        protected override void LocalExecuteImplementation(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                // Local çağrı - direkt node'u initialize et
                if (_nodeObject == null)
                {
                    LogManager.LogError("[InitializeNodeAction] ❌ Local initialization için _nodeObject null!");
                    return;
                }
                
                TryInjectCanvasNode(_nodeObject, _nodeType);
            }
            else
            {
                // Network'ten gelen broadcast - data'dan node'u bul ve initialize et
                try
                {
                    NodeInitData initData = JsonUtility.FromJson<NodeInitData>(data);
                    
                    // Bu client'ta aynı node'u bul (NodeId, name veya position ile)
                    var np =_graphManager.CreateNodeAtPosition(initData.position, initData.nodeType);
                    /*GameObject targetNode = FindNodeByNameOrPosition(np.gameObject.name, np.transform.position, np.ID);
                    
                    if (targetNode != null)
                    {
                        TryInjectCanvasNode(targetNode, initData.nodeType);
                    }
                    else
                    {
                        LogManager.LogWarning($"[InitializeNodeAction] ⚠️ Broadcast için target node BULUNAMADI!");
                        LogManager.LogWarning($"[InitializeNodeAction]   🔍 GraphManager NodePresenters count: {_graphManager?.NodePresenters?.Count ?? -1}");
                    }*/
                }
                catch (Exception e)
                {
                    LogManager.LogError($"[InitializeNodeAction] ❌ Broadcast data parse HATASI: {e.Message}");
                    LogManager.LogError($"[InitializeNodeAction] 📊 Hatalı data: {data}");
                    LogManager.LogError($"[InitializeNodeAction] 🔧 StackTrace: {e.StackTrace}");
                }
            }
        }

        public void InitializeNode(NodeType nodeType)
        {
            InitializeNode(nodeType, Vector2.zero);
        }
        
        public void InitializeNode(NodeType nodeType, Vector2 position)
        {
            _nodeType = nodeType;
            
            // Network broadcast için data hazırla
            string newNodeId = System.Guid.NewGuid().ToString();
            NodeInitData broadcastData = new NodeInitData
            {
                nodeType = nodeType,
                position = position
            };
            
            string jsonData = JsonUtility.ToJson(broadcastData);
            
            // Broadcast et
            ExecuteWithData(jsonData);
        }
        
        // Data ile execute etmek için yeni metod
        private void ExecuteWithData(string jsonData)
        {
            try
            {
                // BroadcastObjectAction'ın Execute metodunu data ile çağır
                Execute(jsonData); // Base implementation'ı tetikle
            }
            catch (Exception e)
            {
                LogManager.LogError($"[InitializeNodeAction] ❌ Broadcast gönderim HATASI: {e.Message}");
                LogManager.LogError($"[InitializeNodeAction] 📊 Hatalı JSON data: {jsonData}");
                LogManager.LogError($"[InitializeNodeAction] 🔧 StackTrace: {e.StackTrace}");
            }
        }
        
        private GameObject FindNodeByNameOrPosition(string nodeName, Vector3 position, string nodeId = null)
        {
            int totalChecked = 0;
            int validPresenters = 0;
            
            // ÖNCE GraphManager'daki node'lar arasında Node ID ile ara (En güvenilir yöntem)
            if (_graphManager != null && _graphManager.NodePresenters != null)
            {
                // ÖNCE NodeId ile arama (En güvenilir)
                if (!string.IsNullOrEmpty(nodeId))
                {
                    foreach (var nodePresenter in _graphManager.NodePresenters)
                    {
                        if (nodePresenter != null && nodePresenter.Model != null && nodePresenter.Model.ID == nodeId)
                        {
                            return nodePresenter.gameObject;
                        }
                    }
                    LogManager.LogWarning($"[InitializeNodeAction] ⚠️ NodeId ile bulunamadı: {nodeId}");
                }
                
                foreach (var nodePresenter in _graphManager.NodePresenters)
                {
                    totalChecked++;
                    
                    if (nodePresenter != null && nodePresenter.gameObject != null && nodePresenter.Model != null)
                    {
                        validPresenters++;
                        GameObject currentNode = nodePresenter.gameObject;
                        float distance = Vector3.Distance(currentNode.transform.position, position);
                        
                        // Pozisyon + NodeType kontrolü (En güvenilir kombinasyon)
                        if (distance < 0.1f && nodePresenter.Model.Type == _nodeType)
                        {
                            return currentNode;
                        }
                        
                        // Fallback: Sadece pozisyon kontrolü
                        if (distance < 0.05f) // Daha hassas tolerance
                        {
                            LogManager.LogWarning($"[InitializeNodeAction] ⚠️ NodeType eşleşmedi: {nodePresenter.Model.Type} != {_nodeType}");
                            return currentNode;
                        }
                    }
                    else
                    {
                        LogManager.LogWarning($"[InitializeNodeAction] ⚠️ Invalid presenter at index {totalChecked}: presenter={nodePresenter != null}, gameObject={nodePresenter?.gameObject != null}, model={nodePresenter?.Model != null}");
                    }
                }
            }
            else
            {
                LogManager.LogWarning($"[InitializeNodeAction] ⚠️ GraphManager durumu: _graphManager={_graphManager != null}, NodePresenters={_graphManager?.NodePresenters != null}");
            }
            
            // Bulunamadıysa scene'de ara
            GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
            
            int sceneChecked = 0;
            int sceneWithPresenter = 0;
            int sceneNodeIdChecked = 0;
            int scenePositionChecked = 0;
            
            // ÖNCE NodeId ile scene'de ara
            if (!string.IsNullOrEmpty(nodeId))
            {
                foreach (GameObject go in allGameObjects)
                {
                    sceneChecked++;
                    var nodePresenter = go.GetComponent<BaseNodePresenter>();
                    if (nodePresenter != null && nodePresenter.Model != null)
                    {
                        sceneNodeIdChecked++;
                        if (nodePresenter.Model.ID == nodeId)
                        {
                            return go;
                        }
                    }
                }
            }
            
            // Sonra Position + NodeType ile ara
            sceneChecked = 0; // Reset counter
            
            foreach (GameObject go in allGameObjects)
            {
                sceneChecked++;
                var nodePresenter = go.GetComponent<BaseNodePresenter>();
                
                if (nodePresenter != null && nodePresenter.Model != null)
                {
                    sceneWithPresenter++;
                    float distance = Vector3.Distance(go.transform.position, position);
                    
                    // Position + NodeType kontrolü
                    if (distance < 0.1f && nodePresenter.Model.Type == _nodeType)
                    {
                        scenePositionChecked++;
                        return go;
                    }
                }
            }
            
            LogManager.LogWarning($"[InitializeNodeAction] ❌ Node BULUNAMADI!");
            LogManager.LogWarning($"[InitializeNodeAction] 📊 Kapsamlı arama özeti:");
            LogManager.LogWarning($"[InitializeNodeAction]   📛 Aranan NodeName: '{nodeName}'");
            LogManager.LogWarning($"[InitializeNodeAction]   🆔 Aranan NodeId: '{nodeId ?? "NULL"}'");
            LogManager.LogWarning($"[InitializeNodeAction]   📍 Aranan Position: {position}");
            LogManager.LogWarning($"[InitializeNodeAction]   📌 Aranan NodeType: {_nodeType}");
            LogManager.LogWarning($"[InitializeNodeAction] 📊 GraphManager arama:");
            LogManager.LogWarning($"[InitializeNodeAction]   🏢 Total checked: {totalChecked}, Valid presenters: {validPresenters}");
            LogManager.LogWarning($"[InitializeNodeAction] 📊 Scene arama:");
            LogManager.LogWarning($"[InitializeNodeAction]   🌍 Total objects: {sceneChecked}, With presenters: {sceneWithPresenter}");
            LogManager.LogWarning($"[InitializeNodeAction]   🆔 NodeId checked: {sceneNodeIdChecked}, Position checked: {scenePositionChecked}");
            
            return null;
        }

        private void TryInjectCanvasNode(GameObject nodeObject, NodeType nodeType)
        {
            try
            {
                // ÖNCE TÜM NULL CHECK'LERİ YAP
                if (_graphManager == null)
                {
                    LogManager.LogError("[InitializeNodeAction] GraphManager is null");
                    return;
                }

                if (nodeObject == null)
                {
                    LogManager.LogError("[InitializeNodeAction] NodeObject is null!");
                    return;
                }

                if (_graphManager.contentTransform == null)
                {
                    LogManager.LogError("[InitializeNodeAction] GraphManager.contentTransform is null");
                    return;
                }

                // 1. Canvas Content'deki ZenjectInjector'ı kullan
                var canvasInjector = _graphManager.contentTransform.GetComponent<ZenjectInjector>();
                
                if (canvasInjector != null)
                {
                    canvasInjector.InjectObject(nodeObject);
                }
                else
                {
                    LogManager.LogWarning("[InitializeNodeAction] Canvas Content'inde ZenjectInjector bulunamadı!");
                }

                // 2. NodePresenter'ı al ve manuel initialization yap
                var nodePresenter = nodeObject.GetComponent<BaseNodePresenter>();
                
                if (nodePresenter != null)
                {
                    ManuallyInitializeNodePresenter(nodePresenter, nodeType, nodeObject);
                    
                    // GraphManager'a ekle (eğer zaten eklenmemişse)
                    if (!_graphManager.NodePresenters.Contains(nodePresenter))
                    {
                        _graphManager.NodePresenters.Add(nodePresenter);
                    }
                }
                else
                {
                    LogManager.LogError($"[InitializeNodeAction] {nodeObject.name} nesnesinde BaseNodePresenter bulunamadı!");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"[InitializeNodeAction] Node injection/initialization hatası: {e.Message}\nStackTrace: {e.StackTrace}");
            }
        }
        
        private void ManuallyInitializeNodePresenter(BaseNodePresenter presenter, NodeType nodeType, GameObject nodeObject)
        {
            try
            {
                if (presenter == null)
                {
                    LogManager.LogError("[InitializeNodeAction] ❌ Presenter is null!");
                    return;
                }

                if (nodeObject == null)
                {
                    LogManager.LogError("[InitializeNodeAction] ❌ NodeObject is null!");
                    return;
                }

                BaseNode model = CreateModelForNodeType(nodeType, nodeObject);
                
                if (model == null)
                {
                    LogManager.LogError("[InitializeNodeAction] ❌ Model creation FAILED!");
                    return;
                }
                
                presenter.Model = model;
                presenter.Initialize(model);
            }
            catch (Exception e)
            {
                LogManager.LogError($"[InitializeNodeAction] ❌ Node model initialization HATASI: {e.Message}");
                LogManager.LogError($"[InitializeNodeAction] 🔧 StackTrace: {e.StackTrace}");
                LogManager.LogError($"[InitializeNodeAction] 📊 Context:");
                LogManager.LogError($"[InitializeNodeAction]   📌 NodeType: {nodeType}");
                LogManager.LogError($"[InitializeNodeAction]   🎮 NodeObject: {nodeObject?.name ?? "NULL"}");
                LogManager.LogError($"[InitializeNodeAction]   🎭 Presenter: {presenter?.GetType().Name ?? "NULL"}");
            }
        }
        
        private BaseNode CreateModelForNodeType(NodeType nodeType, GameObject nodeObject)
        {
            string nodeId = LTGUtility.GenerateSID();
            List<Port> ports = new List<Port>();
            
            // NULL CHECK EKLE - Parametre olarak gelen nodeObject'i kullan
            if (nodeObject != null)
            {
                var nodePresenter = nodeObject.GetComponent<BaseNodePresenter>();
                
                if (nodePresenter != null)
                {
                    var portPresenters = nodePresenter.GetComponentsInChildren<PortPresenter>();
                    
                    foreach (var port in portPresenters)
                    {
                        if (port is EventPortPresenter)
                        {
                            // Skip event ports
                        }
                        else if (port.Model == null)
                        {
                            LogManager.LogWarning($"[InitializeNodeAction] ⚠️ Port.Model is NULL - SKIPPED");
                        }
                        else
                        {
                            ports.Add(port.Model);
                        }
                    }
                }
                else
                {
                    LogManager.LogWarning("[InitializeNodeAction] ⚠️ BaseNodePresenter BULUNAMADI!");
                    LogManager.LogWarning("[InitializeNodeAction] 📊 Available components:");
                    Component[] components = nodeObject.GetComponents<Component>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        LogManager.LogWarning($"[InitializeNodeAction]   [{i}] {components[i]?.GetType().Name ?? "NULL"}");
                    }
                    LogManager.LogWarning("[InitializeNodeAction] ⚠️ Model portlar olmadan oluşturulacak");
                }
            }
            else
            {
                LogManager.LogWarning("[InitializeNodeAction] ⚠️ nodeObject NULL - Model portlar olmadan oluşturulacak");
            }
            
            BaseNode createdModel = null;
            
            switch (nodeType)
            {
                case NodeType.Ghost:
                    createdModel = new Models.Nodes.GhostNode(nodeId, "Ghost Node", Color.gray, true, ports)
                    {
                        Type = NodeType.Ghost
                    };
                    break;
                    
                case NodeType.Start:
                    createdModel = new Models.Nodes.StartNode(nodeId, "Start Node", Color.green, true, ports)
                    {
                        Type = NodeType.Start
                    };
                    break;
                    
                case NodeType.Finish:
                    createdModel = new Models.Nodes.FinishNode(nodeId, "Finish Node", Color.red, true, ports)
                    {
                        Type = NodeType.Finish
                    };
                    break;
                    
                case NodeType.TouchNode:
                    createdModel = new Models.Nodes.TouchNode(nodeId, "Touch Node", Color.blue, true, ports)
                    {
                        Type = NodeType.TouchNode
                    };
                    break;
                    
                case NodeType.GrabNode:
                    createdModel = new Models.Nodes.GrabNode(nodeId, "Grab Node", Color.yellow, true, ports)
                    {
                        Type = NodeType.GrabNode
                    };
                    break;
                    
                case NodeType.WaitForNextNode:
                    createdModel = new Models.Nodes.WaitForNextNode(nodeId, "Wait For Next Node", Color.cyan, true, ports)
                    {
                        Type = NodeType.WaitForNextNode
                    };
                    break;
                    
                case NodeType.LookNode:
                    createdModel = new Models.Nodes.LookNode(nodeId, "Look Node", Color.magenta, true, ports)
                    {
                        Type = NodeType.LookNode
                    };
                    break;
                    
                case NodeType.LogicalOR:
                    createdModel = new Models.Nodes.LogicNode(nodeId, "Logical OR Node", new Color(1f, 0.5f, 0f), true, ports)
                    {
                        Type = NodeType.LogicalOR
                    };
                    break;
                    
                case NodeType.LogicalAND:
                    createdModel = new Models.Nodes.LogicNode(nodeId, "Logical AND Node", new Color(1f, 0.5f, 0f), true, ports)
                    {
                        Type = NodeType.LogicalAND
                    };
                    break;
                    
                case NodeType.ToolTouchNode:
                    createdModel = new Models.Nodes.ToolTouchNode(nodeId, "Tool Touch Node", Color.blue, true, ports)
                    {
                        Type = NodeType.ToolTouchNode
                    };
                    break;
                    
                // Action Node Tipleri
                case NodeType.PlaySoundAction:
                    createdModel = new Models.Nodes.AudioActionNode(nodeId, "Play Sound", Color.white, true, ports)
                    {
                        Type = NodeType.PlaySoundAction
                    };
                    break;
                    
                case NodeType.ChangeMaterialAction:
                    createdModel = new Models.Nodes.ChangeMaterialActionNode(nodeId, "Change Material", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeMaterialAction
                    };
                    break;
                    
                case NodeType.ChangePositionAction:
                    createdModel = new Models.Nodes.ChangePositionActionNode(nodeId, "Move Object", Color.white, true, ports)
                    {
                        Type = NodeType.ChangePositionAction
                    };
                    break;
                    
                case NodeType.ChangeRotationAction:
                    createdModel = new Models.Nodes.ChangeRotationActionNode(nodeId, "Set Object Rotation", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeRotationAction
                    };
                    break;
                    
                case NodeType.ChangeScaleAction:
                    createdModel = new Models.Nodes.ChangeScaleActionNode(nodeId, "Set Object Scale", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeScaleAction
                    };
                    break;
                    
                case NodeType.ToggleObjectAction:
                    createdModel = new Models.Nodes.ToggleObjectActionNode(nodeId, "Toggle Object", Color.white, true, ports)
                    {
                        Type = NodeType.ToggleObjectAction
                    };
                    break;
                    
                case NodeType.PlayAnimationAction:
                    createdModel = new Models.Nodes.PlayAnimationActionNode(nodeId, "Play Animation", Color.white, true, ports)
                    {
                        Type = NodeType.PlayAnimationAction
                    };
                    break;
                    
                case NodeType.DescriptionActionNode:
                    createdModel = new Models.Nodes.DescriptionActionNode(nodeId, "Show Text", Color.white, true, ports)
                    {
                        Type = NodeType.DescriptionActionNode
                    };
                    break;
                    
                case NodeType.RobotAnimationAction:
                    createdModel = new Models.Nodes.RobotAnimationActionNode(nodeId, "Robot Animation", Color.white, true, ports)
                    {
                        Type = NodeType.RobotAnimationAction
                    };
                    break;
                    
                case NodeType.WorldDescriptionActionNode:
                    createdModel = new Models.Nodes.WorldDescriptionActionNode(nodeId, "Show World Text", Color.white, true, ports)
                    {
                        Type = NodeType.WorldDescriptionActionNode
                    };
                    break;
                    
                case NodeType.VFXActionNode:
                    createdModel = new Models.Nodes.VFXActionNode(nodeId, "VFX Effect", Color.white, true, ports)
                    {
                        Type = NodeType.VFXActionNode
                    };
                    break;
                    
                case NodeType.HighlightObjectActionNode:
                    createdModel = new Models.Nodes.HighlightObjectActionNode(nodeId, "Highlight Effect", Color.white, true, ports)
                    {
                        Type = NodeType.HighlightObjectActionNode
                    };
                    break;
                    
                default:
                    LogManager.LogWarning($"[InitializeNodeAction] ⚠️ Desteklenmeyen node tipi: {nodeType}");
                    createdModel = new Models.Nodes.TouchNode(nodeId, "Unknown Node", Color.gray, true, ports)
                    {
                        Type = nodeType
                    };
                    break;
            }
            
            // Final model validation ve return
            if (createdModel != null)
            {
                return createdModel;
            }
            else
            {
                LogManager.LogError($"[InitializeNodeAction] ❌ Model oluşturulamadı! createdModel is NULL");
                LogManager.LogError($"[InitializeNodeAction] 📌 Failed for NodeType: {nodeType}");
                return null;
            }
        }
    }
}