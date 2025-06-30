using System;
using System.Collections.Generic;
using Commands;
using Enums;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Interfaces;
using Virtualware.Networking.Client;
using Presenters;
using Models;
using Models.Nodes;

namespace Helpers
{
    public class AddNodeButtonHelper : MonoBehaviour
    {
        public NodeType nodeType;

        [Inject] private GraphManager _graphManager;
        [Inject] private XRInputManager _inputManager;

        private Button _button;
        
        // Canvas Container için VIROO Network Service'i
        private INetworkObjectsService networkObjectsService;
        
        // Node Type -> Canvas Prefab ID mapping
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
        
        // Şu anki nodeType'ı geçici olarak saklamak için
        private NodeType currentNodeType;

        private void Awake()
        {
            _button = GetComponent<Button>();
            //_button.onClick.AddListener(AddNode);
            
            // VIROO injection sistemini başlat (Canvas container için)
            this.QueueForInject();
        }
        
        // VIROO injection method
        protected void Inject(INetworkObjectsService networkObjectsService)
        {
            this.networkObjectsService = networkObjectsService;
        }

        public async void AddNode()
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

            // Canvas container'da bu node tipi var mı kontrol et
            if (canvasNodePrefabIds.ContainsKey(nodeType))
            {
                Debug.Log($"[AddNodeButtonHelper] Canvas container'dan node oluşturuluyor: {nodeType}");
                await CreateNodeFromCanvasContainer(nodeType, position);
            }
            else
            {
                Debug.Log($"[AddNodeButtonHelper] Eski sistemden node oluşturuluyor: {nodeType}");
                // Eski sistem kullan
                if (position == Vector2.zero)
                {
                    UndoRedoManager.Execute(new CreateNodeCommand(nodeType, _graphManager));
                }
                else
                {
                    UndoRedoManager.Execute(new CreateNodeCommand(nodeType, _graphManager, position));
                }
            }
        }
        
        private async System.Threading.Tasks.Task CreateNodeFromCanvasContainer(NodeType nodeType, Vector2 position)
        {
            try
            {
                if (networkObjectsService == null)
                {
                    Debug.LogError("[AddNodeButtonHelper] NetworkObjectsService henüz inject edilmedi!");
                    return;
                }

                // Current nodeType'ı sakla
                currentNodeType = nodeType;
                string prefabId = canvasNodePrefabIds[nodeType];
                
                Debug.Log($"[AddNodeButtonHelper] Canvas container'dan VIROO node oluşturuluyor: {nodeType} (ID: {prefabId})");
                
                // Canvas container'ı bularak o container'dan oluştur
                var canvasContainer = _graphManager.contentTransform.GetComponent<PrefabInstantiableContainer>();
                if (canvasContainer == null)
                {
                    Debug.LogError("[AddNodeButtonHelper] Canvas Content'inde PrefabInstantiableContainer bulunamadı!");
                    return;
                }
                
                // VIROO ile Canvas'da oluştur
                var createResponse = await networkObjectsService.CreateDynamicObject(
                    prefabId,
                    new Vector3(position.x, position.y, 0),
                    Quaternion.identity,
                    requestAuthority: true,
                    isPersistent: true,
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );

                if (createResponse.Success)
                {
                    Debug.Log($"[AddNodeButtonHelper] Canvas node {nodeType} başarıyla oluşturuldu!");
                    
                    GameObject createdObject = createResponse.InstantiatedObject.GameObject;
                    
                    // Canvas'da oluştu, pozisyonu düzelt
                    ConfigureCanvasNode(createdObject, position);
                    
                    // Canvas ZenjectInjector ile inject et ve initialize et
                    TryInjectCanvasNode(createdObject, nodeType);
                }
                else
                {
                    Debug.LogError($"[AddNodeButtonHelper] Canvas node {nodeType} oluşturulamadı!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddNodeButtonHelper] Canvas node oluşturulurken hata: {e.Message}");
            }
        }
        
        private void ConfigureCanvasNode(GameObject nodeObject, Vector2 targetPosition)
        {
            try
            {
                // RectTransform pozisyonunu ayarla
                RectTransform rectTransform = nodeObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = targetPosition;
                    rectTransform.localScale = Vector3.one;
                    
                    // Z pozisyonunu 0'a eşitle (Canvas layer'ında olması için)
                    Vector3 localPos = rectTransform.localPosition;
                    rectTransform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                }
                
                // GraphManager'a ekle
                var nodePresenter = nodeObject.GetComponent<BaseNodePresenter>();
                if (nodePresenter != null)
                {
                    _graphManager.NodePresenters.Add(nodePresenter);
                    
                    // Dynamic content expansion kontrolü
                    if (_graphManager.ShouldExpandContentForNode(targetPosition))
                    {
                        _graphManager.ExpandContentForNode(targetPosition);
                    }
                }
                
                Debug.Log($"[AddNodeButtonHelper] Canvas node konfigüre edildi: Pos={targetPosition}, Z={rectTransform?.localPosition.z}");
                
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddNodeButtonHelper] Canvas node konfigüre edilirken hata: {e.Message}");
            }
        }
        
        private void TryInjectCanvasNode(GameObject nodeObject, NodeType nodeType)
        {
            try
            {
                // 1. Canvas Content'deki ZenjectInjector'ı kullan
                var canvasInjector = _graphManager.contentTransform.GetComponent<ZenjectInjector>();
                if (canvasInjector != null)
                {
                    canvasInjector.InjectObject(nodeObject);
                    Debug.Log($"[AddNodeButtonHelper] {nodeObject.name} nesnesine Canvas ZenjectInjector ile injection uygulandı");
                }
                else
                {
                    Debug.LogWarning("[AddNodeButtonHelper] Canvas Content'inde ZenjectInjector bulunamadı!");
                }

                // 2. NodePresenter'ı al ve manuel initialization yap
                var nodePresenter = nodeObject.GetComponent<BaseNodePresenter>();
                if (nodePresenter != null)
                {
                    // NodePresenterFactory'nin yaptığı gibi Model oluştur ve initialize et
                    ManuallyInitializeNodePresenter(nodePresenter, nodeType);
                    
                    // GraphManager'a ekle
                    _graphManager.NodePresenters.Add(nodePresenter);
                    
                    Debug.Log($"[AddNodeButtonHelper] {nodeObject.name} başarıyla initialize edildi");
                }
                else
                {
                    Debug.LogError($"[AddNodeButtonHelper] {nodeObject.name} nesnesinde BaseNodePresenter bulunamadı!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddNodeButtonHelper] Node injection/initialization hatası: {e.Message}");
            }
        }

        private void ManuallyInitializeNodePresenter(BaseNodePresenter presenter, NodeType nodeType)
        {
            try
            {
                // NodePresenterFactory'nin CreateModel metodunun yaptığını burada yapalım
                BaseNode model = CreateModelForNodeType(nodeType);
                
                // Model'i presenter'a ata
                presenter.Model = model;
                
                // Initialize metodunu çağır
                presenter.Initialize(model);
                
                Debug.Log($"[AddNodeButtonHelper] {nodeType} node'u için model oluşturuldu ve initialize edildi");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddNodeButtonHelper] Node model initialization hatası: {e.Message}");
            }
        }

        private BaseNode CreateModelForNodeType(NodeType nodeType)
        {
            // LTGUtility.GenerateSID() metodunu kullanarak benzersiz ID oluştur
            string nodeId = System.Guid.NewGuid().ToString();
            List<Models.Port> ports = new List<Models.Port>();
            
            switch (nodeType)
            {
                case NodeType.Ghost:
                    return new Models.Nodes.GhostNode(nodeId, "Ghost Node", Color.gray, true, ports)
                    {
                        Type = NodeType.Ghost
                    };
                    
                case NodeType.Start:
                    return new Models.Nodes.StartNode(nodeId, "Start Node", Color.green, true, ports)
                    {
                        Type = NodeType.Start
                    };
                    
                case NodeType.Finish:
                    return new Models.Nodes.FinishNode(nodeId, "Finish Node", Color.red, true, ports)
                    {
                        Type = NodeType.Finish
                    };
                    
                case NodeType.TouchNode:
                    return new Models.Nodes.TouchNode(nodeId, "Touch Node", Color.blue, true, ports)
                    {
                        Type = NodeType.TouchNode
                    };
                    
                case NodeType.GrabNode:
                    return new Models.Nodes.GrabNode(nodeId, "Grab Node", Color.yellow, true, ports)
                    {
                        Type = NodeType.GrabNode
                    };
                    
                case NodeType.WaitForNextNode:
                    return new Models.Nodes.WaitForNextNode(nodeId, "Wait For Next Node", Color.cyan, true, ports)
                    {
                        Type = NodeType.WaitForNextNode
                    };
                    
                case NodeType.LookNode:
                    return new Models.Nodes.LookNode(nodeId, "Look Node", Color.magenta, true, ports)
                    {
                        Type = NodeType.LookNode
                    };
                    
                case NodeType.LogicalOR:
                    return new Models.Nodes.LogicNode(nodeId, "Logical OR Node", new Color(1f, 0.5f, 0f), true, ports)
                    {
                        Type = NodeType.LogicalOR
                    };
                    
                case NodeType.LogicalAND:
                    return new Models.Nodes.LogicNode(nodeId, "Logical AND Node", new Color(1f, 0.5f, 0f), true, ports)
                    {
                        Type = NodeType.LogicalAND
                    };
                    
                case NodeType.ToolTouchNode:
                    return new Models.Nodes.ToolTouchNode(nodeId, "Tool Touch Node", Color.blue, true, ports)
                    {
                        Type = NodeType.ToolTouchNode
                    };
                    
                // Action Node Tipleri
                case NodeType.PlaySoundAction:
                    return new Models.Nodes.AudioActionNode(nodeId, "Play Sound", Color.white, true, ports)
                    {
                        Type = NodeType.PlaySoundAction
                    };
                    
                case NodeType.ChangeMaterialAction:
                    return new Models.Nodes.ChangeMaterialActionNode(nodeId, "Change Material", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeMaterialAction
                    };
                    
                case NodeType.ChangePositionAction:
                    return new Models.Nodes.ChangePositionActionNode(nodeId, "Move Object", Color.white, true, ports)
                    {
                        Type = NodeType.ChangePositionAction
                    };
                    
                case NodeType.ChangeRotationAction:
                    return new Models.Nodes.ChangeRotationActionNode(nodeId, "Set Object Rotation", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeRotationAction
                    };
                    
                case NodeType.ChangeScaleAction:
                    return new Models.Nodes.ChangeScaleActionNode(nodeId, "Set Object Scale", Color.white, true, ports)
                    {
                        Type = NodeType.ChangeScaleAction
                    };
                    
                case NodeType.ToggleObjectAction:
                    return new Models.Nodes.ToggleObjectActionNode(nodeId, "Toggle Object", Color.white, true, ports)
                    {
                        Type = NodeType.ToggleObjectAction
                    };
                    
                case NodeType.PlayAnimationAction:
                    return new Models.Nodes.PlayAnimationActionNode(nodeId, "Play Animation", Color.white, true, ports)
                    {
                        Type = NodeType.PlayAnimationAction
                    };
                    
                case NodeType.DescriptionActionNode:
                    return new Models.Nodes.DescriptionActionNode(nodeId, "Show Text", Color.white, true, ports)
                    {
                        Type = NodeType.DescriptionActionNode
                    };
                    
                case NodeType.RobotAnimationAction:
                    return new Models.Nodes.RobotAnimationActionNode(nodeId, "Robot Animation", Color.white, true, ports)
                    {
                        Type = NodeType.RobotAnimationAction
                    };
                    
                case NodeType.WorldDescriptionActionNode:
                    return new Models.Nodes.WorldDescriptionActionNode(nodeId, "Show World Text", Color.white, true, ports)
                    {
                        Type = NodeType.WorldDescriptionActionNode
                    };
                    
                case NodeType.VFXActionNode:
                    return new Models.Nodes.VFXActionNode(nodeId, "VFX Effect", Color.white, true, ports)
                    {
                        Type = NodeType.VFXActionNode
                    };
                    
                case NodeType.HighlightObjectActionNode:
                    return new Models.Nodes.HighlightObjectActionNode(nodeId, "Highlight Effect", Color.white, true, ports)
                    {
                        Type = NodeType.HighlightObjectActionNode
                    };
                    
                default:
                    Debug.LogWarning($"[AddNodeButtonHelper] Desteklenmeyen node tipi: {nodeType}");
                    return new Models.Nodes.TouchNode(nodeId, "Unknown Node", Color.gray, true, ports)
                    {
                        Type = nodeType
                    };
            }
        }

        
    }
}