using System;
using System.Collections.Generic;
using Actions;
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
        [Inject] private InitializeNodeAction _initializeNodeAction;

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
                // Debug.Log($"[AddNodeButtonHelper] Canvas container'dan node oluşturuluyor: {nodeType}");
                // await CreateNodeFromCanvasContainer(nodeType, position);
                
                _initializeNodeAction.InitializeNode(nodeType, position);
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
                    
                    // InitializeNodeAction ile initialize et (Local + Network sync)
                    Debug.Log($"[AddNodeButtonHelper] InitializeNodeAction çağrılıyor: {createdObject.name}");
                    //_initializeNodeAction.InitializeNode(createdObject, currentNodeType);
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
                
                // Dynamic content expansion kontrolü
                if (_graphManager.ShouldExpandContentForNode(targetPosition))
                {
                    _graphManager.ExpandContentForNode(targetPosition);
                }
                
                Debug.Log($"[AddNodeButtonHelper] Canvas node konfigüre edildi: Pos={targetPosition}, Z={rectTransform?.localPosition.z}");
                
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddNodeButtonHelper] Canvas node konfigüre edilirken hata: {e.Message}");
            }
        }
    }
}