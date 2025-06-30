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
            Model.Description = newText;
        }

        /// <summary>
        /// Model'deki ortak özellikleri UI'ya aktarır (Load sonrası).
        /// Child sınıflar bu metodu override ederek kendi özelliklerini ekleyebilir.
        /// </summary>
        public virtual void SyncModelToUI()
        {
            if (Model == null) return;

            // Description alanını sync et - keyboardDisplay.inputField aslında description input field'ı
            // UpdateNodeDescription metodu da keyboardDisplay.onTextSubmitted'e bağlı
            if (keyboardDisplay != null && keyboardDisplay.inputField != null)
            {
                // Model'deki description'ı input field'a aktar (boş olsa bile)
                keyboardDisplay.inputField.text = Model.Description ?? "";
                LogManager.LogSuccess($"Description synced: '{Model.Description}' for node: {Model.Title}");
            }
            else
            {
                LogManager.LogWarning($"No description input field found for node: {Model.Title}");
            }

            // Title, Color ve diğer ortak özellikler buraya eklenebilir
            // Örnek: Node'un title'ını güncelleme, renk ayarları vs.

            LogManager.LogSuccess($"Base UI synced for node: {Model.Title} - Type: {this.GetType().Name}");
        }

        [Inject]
        public void Construct(GraphManager graphManager, SystemManager systemManager, ScenarioManager scenarioManager,
            NodeConfig config,
            DiContainer container, XRInputManager inputManager, XRKeyboard keyboard)
        {
            Debug.Log("ENTER: NodePresenter Construct");
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
            if (Model.IsStarted && !Model.IsCompleted)
            {
                Play();
            }
        }


        public void Initialize(BaseNode model)
        {
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


        }

        private void SetupUI()
        {
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
            Debug.Log($"Creating node with title: {Model.Title}");
        }

        private void CreatePorts()
        {
            CreatePort(PolarityType.Input, "Input");

            CreatePort(PolarityType.Output, "Output");
        }

        public void CreatePort(PolarityType type, string name)
        {
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
        }

        public bool EnableSelect { get; set; } = true;

        public void Select()
        {
            _graphManager.scrollRect.horizontal = false;
            _graphManager.scrollRect.vertical = false;
            if (!_model.EnableSelect) return;
            _outline.effectColor = _config.selectedColor;
            _outline.enabled = true;
            if (!SystemManager.selectedElements.Contains(this))
            {
                SystemManager.selectedElements.Add(this);
                SystemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementSelected, this);
            }
        }

        public void Unselect()
        {
            _graphManager.scrollRect.horizontal = true;
            _graphManager.scrollRect.vertical = true;
            if (!_model.EnableSelect) return;
            _outline.enabled = false;
            if (SystemManager.selectedElements.Contains(this))
            {
                SystemManager.selectedElements.Remove(this);
                SystemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementUnselected, this);
            }
        }

        private void OnDestroy()
        {
            Unselect();
            if (SystemManager.clickedElement == this as IElement)
                SystemManager.clickedElement = null;
        }

        public bool DisableClick { get; }

        public void OnPointerDown()
        {
            initialPosition = _rectTransform.anchoredPosition;
            // ScrollRect'i sürükleme süresince devre dışı bırak
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = false;
            }

            if (!SystemManager.selectedElements.Contains(this))
            {
                Debug.Log("tiklandi");
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
        }

        public void OnPointerUp()
        {
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
        }

        public bool EnableDrag { get; set; } = true;

        public void OnBeginDrag()
        {
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
        }

        public void OnDrag(Vector2 position)
        {

            if (EnableDrag)
            {
                transform.localPosition = position - (Vector2)_dragOffset;
            }
        }

        public void OnEndDrag()
        {
            // ScrollRect'i tekrar etkinleştir
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }
        }

        public bool EnableHover { get; set; } = true;

        public void OnPointerHoverEnter()
        {
            if (EnableHover)
            {
                _outline.effectColor = _config.hoverColor;
                _outline.enabled = true;
            }
        }

        public void OnPointerHoverExit()
        {
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
        }

        public void OnClickInputField()
        {
            keyboardDisplay.keyboard.Open();
        }

        public string ID { get; set; }

        public int Priority { get; }

        public void Remove()
        {
            if (_graphManager.NodePresenters.Contains(this))
            {
                _graphManager.NodePresenters.Remove(this);

                // Ghost node'lar için log almıyoruz
                if (!(this is NodePresenters.GhostNodePresenter))
                {
                    LogManager.LogInteraction("Node removed: " + Model.Title);
                }
            }
            Destroy(gameObject);
            
            // Ghost node'lar için log almıyoruz
            if (!(this is NodePresenters.GhostNodePresenter))
            {
                LogManager.LogInteraction("Gameobject destroyed: " + Model.Title);
            }
        }

        public PortPresenter GetPortPresenterByModel(Port port)
        {
            foreach (var portPresenter in ports)
            {
                if (portPresenter.Model == port) return portPresenter;
            }

            return null;
        }

        public PortPresenter GetPortPresenterByModel(string portId)
        {
            return ports.FirstOrDefault(p => p.Model.ID == portId);
        }

        private Vector2 GetLocalMousePosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_graphManager.CanvasRectTransform,
                Input.mousePosition, null, out var mousePos);
            return mousePos;
        }

        #region ScenarioMembers

        public UnityEvent
            onStarted,
            onCompleted,
            onSkip;

        public virtual void Play()
        {
            //Debug.Log("This is base");
        }

        public virtual void ActivateNode()
        {
            Model.IsActive = true;


            ScenarioManager.ActiveNodePresenter = this;

            // Header outline'ı göster
            if (_headerOutline != null)
            {
                _headerOutline.enabled = true;
            }
        }

        public virtual void StartNode()
        {
            ActivateNode();
            Model.IsStarted = true;
            Model.IsCompleted = false;
            onStarted.Invoke();

            // Event portlarını tetikle
            TriggerEventPorts(NodeSystem.EventTypeEnum.OnStarted);
        }

        public virtual void CompleteNode()
        {
            Model.IsCompleted = true;
            onCompleted.Invoke();

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

            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
        }

        public virtual void OnSkipNode()
        {
            onSkip.Invoke();

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
        }

        private void OnLastNodeComplete()
        {
            ScenarioManager.FinishScenario();
        }

        private bool TryToGoNextNode()
        {
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

                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        private bool TryToGoPreviousNode()
        {
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

                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public virtual void GoToNextNode()
        {
            CompleteNode();
            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
        }

        public virtual void GoToPreviousNode()
        {
            if (TryToGoPreviousNode()) return;
            Debug.LogWarning("En bastaki node'dasin!");
        }


        // Event portlarını tetikleme metodu
        private void TriggerEventPorts(NodeSystem.EventTypeEnum eventType)
        {
            foreach (var eventPort in eventPorts)
            {
                if (eventPort.EventType == eventType)
                {
                    eventPort.TriggerEvent();
                }
            }
        }

        #endregion

        #region Edit Mode Functions

        /// <summary>
        /// Düzenleme modunu açar. Tüm düzenleme UI elemanlarını gösterir.
        /// Child sınıflar bu metodu override ederek kendi düzenleme elemanlarını yönetebilir.
        /// </summary>
        public virtual void EditModeOn()
        {
           
        }

        /// <summary>
        /// Düzenleme modunu kapatır. Tüm düzenleme UI elemanlarını gizler.
        /// Child sınıflar bu metodu override ederek kendi düzenleme elemanlarını yönetebilir.
        /// </summary>
        public virtual void EditModeOff()
        {
            
        }

        #endregion
    }
}