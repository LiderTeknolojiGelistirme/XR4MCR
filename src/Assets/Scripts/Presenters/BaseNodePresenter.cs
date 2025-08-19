using System;
using Managers;
using Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IClickable = Interfaces.IClickable;
using IDraggable = Interfaces.IDraggable;
using IHover = Interfaces.IHover;
using ISelectable = Interfaces.ISelectable;
using TMPro;
using Zenject;
using CustomGraphics;
using UnityEditor;
using System.Linq;
using Commands;
using Interfaces;
using Models.Nodes;
using NodeSystem.Events;
using NodeSystem;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.Serialization;
using IGraphElement = Interfaces.IGraphElement;
using static Presenters.PortPresenter;
using Presenters.NodePresenters;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using RectTransform = UnityEngine.RectTransform;

namespace Presenters
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseNodePresenter : MonoBehaviour, IGraphElement, ISelectable, IDraggable,
        IClickable, IHover
    {

        [SerializeField] private List<PortPresenter> ports = new List<PortPresenter>();
        [SerializeField] private List<EventPortPresenter> eventPorts = new List<EventPortPresenter>();
        [SerializeField] private XRKeyboardDisplay keyboardDisplay;
        protected ScenarioManager ScenarioManager;
        protected BaseNode _model;
        private DiContainer _container;
        private Outline _outline;
        private Outline _headerOutline;
        private Vector3 _distanceFromPointer;
        private RectTransform _rectTransform;
        private GraphManager _graphManager;
        protected SystemManager SystemManager;
        private NodeConfig _config;
        private Vector2 _mouseDownPosition;
        private Vector2 _nodeStartPosition;
        private Vector3 _dragOffset;
        protected XRInputManager XRInputManager;
        private NotifierCanvas _achievementNotifier;
        private XRKeyboard XRKeyboard;

        private Vector2 initialPosition, endPosition;

       
        public void UpdateNodeDescription(string newText)
        {
            //LogManager.Log($"BaseNodePresenter.UpdateNodeDescription STARTED - NewText: '{newText}' for node: {Model?.Title}", Color.cyan);
            Model.Description = newText;
            //LogManager.Log($"BaseNodePresenter.UpdateNodeDescription COMPLETED", Color.green);
        }

        /// <summary>
        /// Model'deki ortak özellikleri UI'ya aktarır (Load sonrası).
        /// Child sınıflar bu metodu override ederek kendi özelliklerini ekleyebilir.
        /// </summary>
        public virtual void SyncModelToUI()
        {
            //LogManager.Log($"BaseNodePresenter.SyncModelToUI STARTED - Node: {Model?.Title}", Color.cyan);
            if (Model == null) return;

            // Description alanını sync et - keyboardDisplay.inputField aslında description input field'ı
            // UpdateNodeDescription metodu da keyboardDisplay.onTextSubmitted'e bağlı
            if (keyboardDisplay != null && keyboardDisplay.inputField != null)
            {
                // Model'deki description'ı input field'a aktar (boş olsa bile)
                keyboardDisplay.inputField.text = Model.Description ?? "";
                //LogManager.LogSuccess($"Description synced: '{Model.Description}' for node: {Model.Title}");
            }
            else
            {
                LogManager.LogWarning($"No description input field found for node: {Model.Title}");
            }

            // Title, Color ve diğer ortak özellikler buraya eklenebilir
            // Örnek: Node'un title'ını güncelleme, renk ayarları vs.

            LogManager.LogSuccess($"Base UI synced for node: {Model.Title} - Type: {this.GetType().Name}");
            //LogManager.Log($"BaseNodePresenter.SyncModelToUI COMPLETED", Color.green);
        }

        [Inject]
        public void Construct(GraphManager graphManager, SystemManager systemManager, ScenarioManager scenarioManager,
            NodeConfig config,
            DiContainer container, XRInputManager inputManager, XRKeyboard keyboard)
        {
            //LogManager.Log("BaseNodePresenter.Construct STARTED", Color.cyan);
            //Debug.Log("ENTER: NodePresenter Construct");
            ScenarioManager = scenarioManager;
            _graphManager = graphManager;
            SystemManager = systemManager;
            _config = config;
            _container = container;
            XRInputManager = inputManager;
            XRKeyboard = keyboard;

            // XRKeyboardDisplay'i ayarlama örneği
            if (keyboardDisplay != null)
            {
                keyboardDisplay.updateOnKeyPress = true;
                keyboardDisplay.onTextSubmitted.AddListener(UpdateNodeDescription);
            }
            //LogManager.Log("BaseNodePresenter.Construct COMPLETED", Color.green);
        }


        public IReadOnlyList<PortPresenter> Ports => ports;

        public IReadOnlyList<EventPortPresenter> EventPorts => eventPorts;

        public BaseNode Model
        {
            get => _model;
            set => _model = value;
        }

        protected virtual void Update()
        {
            if (Model.IsStarted && !Model.IsCompleted && Model.IsActive)
            {
                Play();
            }
        }


        public void Initialize(BaseNode model)
        {
            //LogManager.Log($"BaseNodePresenter.Initialize STARTED - Model: {model?.Title}", Color.cyan);
            _rectTransform = GetComponent<RectTransform>();
            if(keyboardDisplay != null)
            {
                keyboardDisplay.keyboard = XRKeyboard;

            }            

            _model = model;
            _outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            _outline.effectColor = _config.outlineColor;
            _outline.enabled = false;

            // Header GameObject'ini bul
            Transform headerTransform = transform.Find("Header");
            if (headerTransform != null)
            {
                // Header'a outline ekle
                _headerOutline = headerTransform.GetComponent<Outline>() ?? headerTransform.gameObject.AddComponent<Outline>();
                // #79E0EE renk kodunu RGB'ye çevirme (47%, 88%, 93%)
                _headerOutline.effectColor = new Color(0.47f, 0.88f, 0.93f);
                _headerOutline.effectDistance = new Vector2(3, -3);
                _headerOutline.enabled = false;
            }

            // Normal portları başlat
            ports = GetComponentsInChildren<PortPresenter>()
                .Where(p => !(p is EventPortPresenter))
                .ToList();

            // Her port için model oluştur ve initialize et
            foreach (var portPresenter in ports)
            {
                // Port tipine göre model oluştur
                PolarityType portType = portPresenter.Polarity;
                var portModel = new Port(portType, $"Port_{ports.IndexOf(portPresenter)}", this);

                // Port presenter'ı initialize et
                portPresenter.Initialize(portModel);
            }

            // Event portlarını başlat
            eventPorts = GetComponentsInChildren<EventPortPresenter>().ToList();
            for (int i = 0; i < eventPorts.Count; i++)
            {
                var eventPort = eventPorts[i];
                // Event tipi için benzersiz bir isim oluştur (i index kullanarak)
                string portName = $"EventPort_{eventPort.EventType}_{i}";

                // Event portu için özel EventPort model oluştur
                var portModel = new Models.EventPort(
                    PolarityType.Output,
                    portName,
                    this,
                    eventPort.EventType); // EventPortPresenter'da tanımlanan EventType değerini kullan

                // Event port presenter'ı initialize et
                eventPort.Initialize(portModel);
            }

            //LogManager.Log($"BaseNodePresenter.Initialize COMPLETED - Ports: {ports.Count}, EventPorts: {eventPorts.Count}", Color.green);
            LogManager.Log($"{Model?.Title} initialized.");
        }

        private void SetupUI()
        {
            //LogManager.Log("BaseNodePresenter.SetupUI STARTED", Color.cyan);
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = _config.size;
            var headerGO = new GameObject("Header", typeof(RectTransform), typeof(RoundedRectangle));
            headerGO.transform.SetParent(transform, false);
            var headerRect = headerGO.GetComponent<RectTransform>();
            var headerImage = headerGO.GetComponent<RoundedRectangle>();
            headerRect.anchorMin = new Vector2(0, 0.7f);
            headerRect.anchorMax = Vector2.one;
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            headerImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(headerGO.transform, false);
            var titleRect = titleGO.GetComponent<RectTransform>();
            var titleText = titleGO.GetComponent<TextMeshProUGUI>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(5, 0);
            titleRect.offsetMax = new Vector2(-5, 0);
            titleText.text = Model.Title;
            titleText.color = Color.white;
            titleText.fontSize = 16;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = false;
            //Debug.Log($"Creating node with title: {Model.Title}");
            //LogManager.Log("BaseNodePresenter.SetupUI COMPLETED", Color.green);
        }

        private void CreatePorts()
        {
            //LogManager.Log("BaseNodePresenter.CreatePorts STARTED", Color.cyan);
            CreatePort(PolarityType.Input, "Input");

            CreatePort(PolarityType.Output, "Output");
            //LogManager.Log("BaseNodePresenter.CreatePorts COMPLETED", Color.green);
        }

        public void CreatePort(PolarityType type, string name)
        {
            //LogManager.Log($"BaseNodePresenter.CreatePort STARTED - Type: {type}, Name: {name}", Color.cyan);
            // create port gameobject
            var portGameObject = new GameObject($"Port_{ports.Count}");
            portGameObject.transform.SetParent(transform);

            // create port presenter    
            // var portPresenter = portGameObject.AddComponent<PortPresenter>();
            var portPresenter = _container.InstantiateComponent<PortPresenter>(portGameObject);

            // create port model
            var portModel = new Port(type, name, this);

            // initialize port presenter
            portPresenter.Initialize(portModel);

            // set port size and position
            var rectTransform = portGameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = _config.portSize;
            rectTransform.anchoredPosition = new Vector2(-_config.portOffset, 0);

            // add port to ports list   
            ports.Add(portPresenter);

            // create port label
            var labelGO = new GameObject($"Label_{portModel.Name}", typeof(RectTransform),
                typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(transform);
            var label = labelGO.GetComponent<TextMeshProUGUI>();
            var labelRect = labelGO.GetComponent<RectTransform>();
            label.text = portModel.Name;
            label.fontSize = 12;
            label.color = portModel.ElementColor;
            if (type == PolarityType.Input)
            {
                label.alignment = TextAlignmentOptions.Left;
                labelRect.anchorMin = new Vector2(0, 0.5f);
                labelRect.anchorMax = new Vector2(0, 0.5f);
                labelRect.sizeDelta = new Vector2(60, 20);
                labelRect.anchoredPosition = new Vector2(_config.portOffset * 0.5f, 0);
            }
            else
            {
                label.alignment = TextAlignmentOptions.Right;
                labelRect.anchorMin = new Vector2(1, 0.5f);
                labelRect.anchorMax = new Vector2(1, 0.5f);
                labelRect.sizeDelta = new Vector2(60, 20);
                labelRect.anchoredPosition = new Vector2(-_config.portOffset * 0.5f, 0);
            }
            //LogManager.Log($"BaseNodePresenter.CreatePort COMPLETED - Port count: {ports.Count}", Color.green);
        }

        public bool EnableSelect { get; set; } = true;

        public void Select()
        {
            //LogManager.Log($"BaseNodePresenter.Select STARTED - Node: {Model?.Title}", Color.cyan);
            //Debug.Log($"BaseNodePresenter.Select STARTED - Node: {Model?.Title}");
            
            _graphManager.scrollRect.horizontal = false;
            _graphManager.scrollRect.vertical = false;
            
            if (!_model.EnableSelect) 
            {
                //Debug.Log($"Node selection disabled for: {Model?.Title}");
                LogManager.LogInput($"Node selection disabled for: {Model?.Title}");
                return;
            }
            
            _outline.effectColor = _config.selectedColor;
            _outline.enabled = true;
            
            if (!SystemManager.selectedElements.Contains(this))
            {
                SystemManager.selectedElements.Add(this);
                //Debug.Log($"Node added to selectedElements: {Model?.Title}. Total selected: {SystemManager.selectedElements.Count}");
                LogManager.LogInput($"Node added to selectedElements: {Model?.Title}. Total selected: {SystemManager.selectedElements.Count}");
                SystemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementSelected, this);
            }
            else
            {
                //Debug.Log($"Node already in selectedElements: {Model?.Title}");
                LogManager.LogInput($"Node already in selectedElements: {Model?.Title}");
            }
            
            //LogManager.Log($"BaseNodePresenter.Select COMPLETED", Color.green);
        }

        public void Unselect()
        {
            //LogManager.Log($"BaseNodePresenter.Unselect STARTED - Node: {Model?.Title}", Color.cyan);
            _graphManager.scrollRect.horizontal = true;
            _graphManager.scrollRect.vertical = true;
            if (!_model.EnableSelect) return;
            _outline.enabled = false;
            if (SystemManager.selectedElements.Contains(this))
            {
                SystemManager.selectedElements.Remove(this);
                SystemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementUnselected, this);
                LogManager.Log($"{Model?.Title} unselected.");
            }
            //LogManager.Log($"BaseNodePresenter.Unselect COMPLETED", Color.green);
        }

        private void OnDestroy()
        {
            //LogManager.Log($"BaseNodePresenter.OnDestroy STARTED - Node: {Model?.Title}", Color.cyan);
            Unselect();
            if (SystemManager.clickedElement == this as IElement)
                SystemManager.clickedElement = null;
            //LogManager.Log($"BaseNodePresenter.OnDestroy COMPLETED", Color.green);
        }

        public bool DisableClick { get; }

        public void OnPointerDown()
        {
            //LogManager.Log($"BaseNodePresenter.OnPointerDown STARTED - Node: {Model?.Title}", Color.cyan);
            
            // ✅ PERSISTENT BUTTON CHECK - Button interaction mode aktifse drag yapma
            bool isButtonDirectlyClicked = CheckIfButtonClickedDirectly();
            bool isPersistentButtonMode = _graphManager?.Pointer?.IsButtonInteractionMode == true;
            
            if (isPersistentButtonMode || isButtonDirectlyClicked || _graphManager?.Pointer?.IsButtonClicked == true)
            {
                // Button interaction active - prevent node drag
                return; // Erken çık, drag başlatma
            }
            
            initialPosition = _rectTransform.anchoredPosition;
            // ScrollRect'i sürükleme süresince devre dışı bırak
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = false;
            }

            if (!SystemManager.selectedElements.Contains(this))
            {
                //Debug.Log("tiklandi");
                Select();
                transform.SetAsLastSibling();

                Vector2 localPointerPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _graphManager.CanvasRectTransform,
                    XRInputManager.ScreenPointerPosition,
                    Camera.main,
                    out localPointerPosition);
                
                // COORDINATE SPACE COMPENSATION - Drag offset hesaplaması için scale kompensasyonu
                // localPointerPosition Canvas space'inde, ama transform.localPosition Content space'inde
                Vector3 compensatedPointerPosition = localPointerPosition;
                if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
                {
                    Vector3 contentScale = _graphManager.contentTransform.localScale;
                    
                    // Canvas space'den Content space'e dönüşüm
                    compensatedPointerPosition.x /= contentScale.x;
                    compensatedPointerPosition.y /= contentScale.y;
                }
                
                _mouseDownPosition = Input.mousePosition;
                _nodeStartPosition = transform.localPosition;
                _dragOffset = compensatedPointerPosition - transform.localPosition;
            }
            else
            {
                Unselect();
            }
            //LogManager.Log($"BaseNodePresenter.OnPointerDown COMPLETED", Color.green);
        }

        public void OnPointerUp()
        {
            //LogManager.Log($"BaseNodePresenter.OnPointerUp STARTED - Node: {Model?.Title}", Color.cyan);
            // ScrollRect'i tekrar etkinleştir
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }

            endPosition = _rectTransform.anchoredPosition;
            if (initialPosition != endPosition)
            {
                UndoRedoManager.Insert(new ChangePositionNodeCommand(_graphManager,this,initialPosition,endPosition));
            }
            //LogManager.Log($"BaseNodePresenter.OnPointerUp COMPLETED", Color.green);
        }

        public bool EnableDrag { get; set; } = true;

        public void OnBeginDrag()
        {
            //LogManager.Log($"BaseNodePresenter.OnBeginDrag STARTED - Node: {Model?.Title}", Color.cyan);
            
            // ✅ PERSISTENT BUTTON CHECK - Begin drag için persistent mode kontrolü
            bool isButtonDirectlyClicked = CheckIfButtonClickedDirectly();
            bool isPersistentButtonMode = _graphManager?.Pointer?.IsButtonInteractionMode == true;
            
            if (isPersistentButtonMode || isButtonDirectlyClicked || _graphManager?.Pointer?.IsButtonClicked == true)
            {
                // Button interaction active - prevent begin drag
                return; // Erken çık, drag başlatma
            }
            
            // ScrollRect'i sürükleme süresince devre dışı bırak
            if (EnableDrag && _graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = false;
            }

            if (EnableDrag)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphManager.CanvasRectTransform,
                    Input.mousePosition, null, out var mousePos);

                Select();
            }
                LogManager.Log($"BaseNodePresenter.OnBeginDrag COMPLETED", Color.green);
        }

        public void OnDrag(Vector2 position)
        {
            //LogManager.Log($"BaseNodePresenter.OnDrag STARTED - Position: {position}", Color.cyan);
            
            // ✅ PERSISTENT BUTTON CHECK DURING DRAG - Persistent mode aktifse drag'i durdur
            bool isPersistentButtonMode = _graphManager?.Pointer?.IsButtonInteractionMode == true;
            if (isPersistentButtonMode)
            {
                return; // Drag'i durdur
            }
            
            if (EnableDrag)
            {
                transform.localPosition = position - (Vector2)_dragOffset;
            }
            //LogManager.Log($"BaseNodePresenter.OnDrag COMPLETED", Color.green);
        }

        public void OnEndDrag()
        {
            //LogManager.Log($"BaseNodePresenter.OnEndDrag STARTED - Node: {Model?.Title}", Color.cyan);
            // ScrollRect'i tekrar etkinleştir
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }
            //LogManager.Log($"BaseNodePresenter.OnEndDrag COMPLETED", Color.green);
        }

        public bool EnableHover { get; set; } = true;

        public void OnPointerHoverEnter()
        {
            //LogManager.Log($"BaseNodePresenter.OnPointerHoverEnter STARTED - Node: {Model?.Title}", Color.cyan);
            if (EnableHover)
            {
                _outline.effectColor = _config.hoverColor;
                _outline.enabled = true;
            }
            //LogManager.Log($"BaseNodePresenter.OnPointerHoverEnter COMPLETED", Color.green);
        }

        public void OnPointerHoverExit()
        {
            //LogManager.Log($"BaseNodePresenter.OnPointerHoverExit STARTED - Node: {Model?.Title}", Color.cyan);
            if (EnableHover)
            {
                if (SystemManager.selectedElements.Contains(this))
                {
                    _outline.effectColor = _config.selectedColor;
                }
                else
                {
                    _outline.enabled = false;
                }
            }
            //LogManager.Log($"BaseNodePresenter.OnPointerHoverExit COMPLETED", Color.green);
        }

        public void OnClickInputField()
        {
            //LogManager.Log($"BaseNodePresenter.OnClickInputField STARTED - Node: {Model?.Title}", Color.cyan);
            keyboardDisplay.keyboard.Open();
            //LogManager.Log($"BaseNodePresenter.OnClickInputField COMPLETED", Color.green);
        }

        public string ID { get; set; }

        public int Priority { get; }

        public void Remove()
        {
            //LogManager.Log($"BaseNodePresenter.Remove STARTED - Node: {Model?.Title}", Color.cyan);
            if (_graphManager.NodePresenters.Contains(this))
            {
                _graphManager.NodePresenters.Remove(this);

                // Ghost node'lar için log almıyoruz
                if (!(this is NodePresenters.GhostNodePresenter))
                {
                    //LogManager.LogInteraction("Node removed: " + Model.Title);
                }
            }
            Destroy(gameObject);
            
            // Ghost node'lar için log almıyoruz
            if (!(this is NodePresenters.GhostNodePresenter))
            {
                //LogManager.LogInteraction("Gameobject destroyed: " + Model.Title);
            }
            LogManager.Log($"{Model?.Title} Removed", Color.green);
        }

        public PortPresenter GetPortPresenterByModel(Port port)
        {
            //LogManager.Log($"BaseNodePresenter.GetPortPresenterByModel STARTED - Port: {port?.ID}", Color.cyan);
            foreach (var portPresenter in ports)
            {
                if (portPresenter.Model == port)
                {
                    //LogManager.Log($"BaseNodePresenter.GetPortPresenterByModel COMPLETED - Found port: {port?.ID}", Color.green);
                    return portPresenter;
                }
            }

            //LogManager.Log($"BaseNodePresenter.GetPortPresenterByModel COMPLETED - Port not found: {port?.ID}", Color.green);
            return null;
        }

        public PortPresenter GetPortPresenterByModel(string portId)
        {
            //LogManager.Log($"BaseNodePresenter.GetPortPresenterByModel(string) STARTED - PortId: {portId}", Color.cyan);
            var result = ports.FirstOrDefault(p => p.Model.ID == portId);
            //LogManager.Log($"BaseNodePresenter.GetPortPresenterByModel(string) COMPLETED - Found: {result != null}", Color.green);
            return result;
        }

        private Vector2 GetLocalMousePosition()
        {
            //LogManager.Log($"BaseNodePresenter.GetLocalMousePosition STARTED", Color.cyan);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphManager.CanvasRectTransform,
                Input.mousePosition, null, out var mousePos);
            //LogManager.Log($"BaseNodePresenter.GetLocalMousePosition COMPLETED - Position: {mousePos}", Color.green);
            return mousePos;
        }

        /// <summary>
        /// Direct button check to prevent timing issues with Pointer flag
        /// </summary>
        private bool CheckIfButtonClickedDirectly()
        {
            try
            {
                if (_graphManager?.Pointer == null || XRInputManager == null)
                {
                    return false;
                }

                // Unity UI EventSystem kullanarak UI raycast yap
                var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = XRInputManager.ScreenPointerPosition
                };

                var raycastResults = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerEventData, raycastResults);
                
                foreach (var result in raycastResults)
                {
                    var buttonComponent = result.gameObject.GetComponent<UnityEngine.UI.Button>() ?? result.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
                    if (buttonComponent != null && buttonComponent.interactable)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (System.Exception e)
            {
                // Silent error handling for button check
                return false;
            }
        }

        #region ScenarioMembers

        public UnityEvent
            onStarted,
            onCompleted,
            onSkip;

        public virtual void Play()
        {
            //LogManager.Log($"BaseNodePresenter.Play STARTED - Node: {Model?.Title}", Color.cyan);
            //Debug.Log("This is base");
            //LogManager.Log($"BaseNodePresenter.Play COMPLETED", Color.green);
        }

        public virtual void ActivateNode()
        {
            //LogManager.Log($"BaseNodePresenter.ActivateNode STARTED - Node: {Model?.Title}", Color.cyan);
            Model.IsActive = true;


            ScenarioManager.ActiveNodePresenter = this;

            // Header outline'ı göster
            if (_headerOutline != null)
            {
                _headerOutline.enabled = true;
            }
            LogManager.Log($"{Model?.Title} activated.", Color.green);
        }

        public virtual void DeactivateNode()
        {
            //LogManager.Log($"BaseNodePresenter.DeactivateNode STARTED - Node: {Model?.Title}", Color.cyan);
            Model.IsActive = false;

            // Active node presenter'ı temizle
            if (ScenarioManager.ActiveNodePresenter == this)
            {
                ScenarioManager.ActiveNodePresenter = null;
            }

            // Header outline'ı gizle
            if (_headerOutline != null)
            {
                _headerOutline.enabled = false;
            }
            LogManager.Log($"{Model?.Title} deactivated.", Color.yellow);
        }

        public virtual void StartNode()
        {
            //LogManager.Log($"BaseNodePresenter.StartNode STARTED - Node: {Model?.Title}", Color.cyan);
            ActivateNode();
            Model.IsStarted = true;
            Model.IsCompleted = false;
            onStarted.Invoke();

            // Event portlarını tetikle
            TriggerEventPorts(NodeSystem.EventTypeEnum.OnStarted);
            LogManager.Log($"{Model?.Title} started.", Color.green);
        }

        public virtual void CompleteNode()
        {
            //LogManager.Log($"BaseNodePresenter.CompleteNode STARTED - Node: {Model?.Title}", Color.cyan);
            
            Model.IsCompleted = true;
            onCompleted.Invoke();
            DeactivateNode();
            // Event portlarını tetikle
            TriggerEventPorts(NodeSystem.EventTypeEnum.OnCompleted);

            // Header outline'ı gizle
            if (_headerOutline != null)
            {
                _headerOutline.enabled = false;
            }

            if (this is not StartNodePresenter && this is not FinishNodePresenter && this is not ActionNodePresenter)
            {
                _achievementNotifier = ScenarioManager.achievementCanvas.GetComponent<NotifierCanvas>();
                _achievementNotifier.GetComponent<NotifierCanvas>().ApplyToAchievementNotification();
            }

            if (TryToGoNextNode() || this is ActionNodePresenter) return;
            OnLastNodeComplete();
            LogManager.Log($"{Model?.Title} completed.", Color.green);
        }

        public virtual void OnSkipNode()
        {
            //LogManager.Log($"BaseNodePresenter.OnSkipNode STARTED - Node: {Model?.Title}", Color.cyan);
            onSkip.Invoke();
            DeactivateNode();

            // Event portlarını tetikle
            TriggerEventPorts(NodeSystem.EventTypeEnum.OnSkip);

            // Header outline'ı gizle
            if (_headerOutline != null)
            {
                _headerOutline.enabled = false;
            }

            if (this is not StartNodePresenter && this is not FinishNodePresenter && this is not ActionNodePresenter)
            {
                _achievementNotifier = ScenarioManager.achievementCanvas.GetComponent<NotifierCanvas>();
                _achievementNotifier.GetComponent<NotifierCanvas>().ApplyToAchievementNotification();
            }

            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
            LogManager.Log($"{Model?.Title} skipped.", Color.green);
        }

        private void OnLastNodeComplete()
        {
            //LogManager.Log($"BaseNodePresenter.OnLastNodeComplete STARTED - Node: {Model?.Title}", Color.cyan);
            ScenarioManager.FinishScenario();
            //LogManager.Log($"BaseNodePresenter.OnLastNodeComplete COMPLETED", Color.green);
        }

        private bool TryToGoNextNode()
        {
            //LogManager.Log($"BaseNodePresenter.TryToGoNextNode STARTED - Node: {Model?.Title}", Color.cyan);
            foreach (PortPresenter portPresenter in ports)
            {
                if (portPresenter.Polarity == PolarityType.Output)
                {
                    if (portPresenter.ConnectionPresenters.Count > 0)
                    {
                        foreach (ConnectionPresenter connectionPresenter in portPresenter.ConnectionPresenters)
                        {
                            if (connectionPresenter.Model.TargetPort != null)
                            {

                                // Sonra hedef node'u başlat (bu ScenarioManager.ActiveNodePresenter'ı günceller)
                                connectionPresenter.Model.TargetPort.Model.baseNode.StartNode();
                                ScenarioManager.ActiveNodePresenter =
                                    connectionPresenter.Model.TargetPort.Model.baseNode;
                                // UI'ı güncelle
                                ScenarioManager.UpdateNodeInfoDisplay();
                            }
                        }

                        LogManager.Log($"{Model?.Title} goes next node.", Color.green);
                        return true;
                    }
                    LogManager.LogError($"{Model?.Title} could not go to next node.");
                    return false;
                }
            }

            LogManager.LogError($"{Model?.Title} could not go to next node end of the loop.");
            return false;
        }

        private bool TryToGoPreviousNode()
        {
            //LogManager.Log($"BaseNodePresenter.TryToGoPreviousNode STARTED - Node: {Model?.Title}", Color.cyan);
            foreach (PortPresenter portPresenter in ports)
            {
                if (portPresenter.Polarity == PolarityType.Input)
                {
                    if (portPresenter.ConnectionPresenters.Count > 0)
                    {
                        if (portPresenter.ConnectionPresenters[0].Model.SourcePort != null)
                        {

                            // Sonra kaynak node'u başlat (bu ScenarioManager.ActiveNodePresenter'ı günceller)
                            portPresenter.ConnectionPresenters[0].Model.SourcePort.Model.baseNode.StartNode();
                            ScenarioManager.ActiveNodePresenter =
                                portPresenter.ConnectionPresenters[0].Model.SourcePort.Model.baseNode;
                            // UI'ı güncelle
                            ScenarioManager.UpdateNodeInfoDisplay();

                            LogManager.Log($"{Model?.Title} goes previous node.", Color.green);
                            return true;
                        }
                    }
                }
            }

            LogManager.LogError($"{Model?.Title} could not go to previous node.");
            return false;
        }


        public virtual void GoToNextNode()
        {
            //LogManager.Log($"BaseNodePresenter.GoToNextNode STARTED - Node: {Model?.Title}", Color.cyan);
            CompleteNode();
            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
            //LogManager.Log($"BaseNodePresenter.GoToNextNode COMPLETED", Color.green);
        }

        public virtual void GoToPreviousNode()
        {
            //LogManager.Log($"BaseNodePresenter.GoToPreviousNode STARTED - Node: {Model?.Title}", Color.cyan);
            if (TryToGoPreviousNode()) return;
            LogManager.LogWarning("You are at the first node. You can't go to previous node.");
            //LogManager.Log($"BaseNodePresenter.GoToPreviousNode COMPLETED", Color.green);
        }


        // Event portlarını tetikleme metodu
        private void TriggerEventPorts(NodeSystem.EventTypeEnum eventType)
        {
            //LogManager.Log($"BaseNodePresenter.TriggerEventPorts STARTED - EventType: {eventType}, Node: {Model?.Title}", Color.cyan);
            foreach (var eventPort in eventPorts)
            {
                if (eventPort.EventType == eventType)
                {
                    eventPort.TriggerEvent();
                }
            }
            //LogManager.Log($"BaseNodePresenter.TriggerEventPorts COMPLETED", Color.green);
        }

        #endregion

        #region Edit Mode Functions

        /// <summary>
        /// Düzenleme modunu açar. Tüm düzenleme UI elemanlarını gösterir.
        /// Child sınıflar bu metodu override ederek kendi düzenleme elemanlarını yönetebilir.
        /// </summary>
        public virtual void EditModeOn()
        {
            //LogManager.Log($"BaseNodePresenter.EditModeOn STARTED - Node: {Model?.Title}", Color.cyan);
            //LogManager.Log($"BaseNodePresenter.EditModeOn COMPLETED", Color.green);
            LogManager.Log($"{Model?.Title} is in edit mode.");
        }

        /// <summary>
        /// Düzenleme modunu kapatır. Tüm düzenleme UI elemanlarını gizler.
        /// Child sınıflar bu metodu override ederek kendi düzenleme elemanlarını yönetebilir.
        /// </summary>
        public virtual void EditModeOff()
        {
            //LogManager.Log($"BaseNodePresenter.EditModeOff STARTED - Node: {Model?.Title}", Color.cyan);
            //LogManager.Log($"BaseNodePresenter.EditModeOff COMPLETED", Color.green);
            LogManager.Log($"{Model?.Title} is not in edit mode.");
        }

        #endregion
    }
}