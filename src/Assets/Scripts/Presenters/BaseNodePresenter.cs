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
using Interfaces;
using Models.Nodes;
using NodeSystem.Events;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.Serialization;
using IGraphElement = Interfaces.IGraphElement;
using static Presenters.PortPresenter;

namespace Presenters
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseNodePresenter : MonoBehaviour, IGraphElement, ISelectable, IDraggable,
        IClickable, IHover
    {
        [SerializeField] private List<PortPresenter> ports = new List<PortPresenter>();
        protected ScenarioManager ScenarioManager;
        private BaseNode _model;
        private DiContainer _container;
        private Outline _outline;
        private Vector3 _distanceFromPointer;
        private RectTransform _rectTransform;
        private GraphManager _graphManager;
        private SystemManager _systemManager;
        private NodeConfig _config;
        private Vector2 _mouseDownPosition;
        private Vector2 _nodeStartPosition;
        private Vector3 _dragOffset;
        private XRInputManager _XRInputManager;


        [Inject]
        public void Construct(GraphManager graphManager, SystemManager systemManager, ScenarioManager scenarioManager,
            NodeConfig config,
            DiContainer container, XRInputManager inputManager)
        {
            Debug.Log("ENTER: NodePresenter Construct");
            ScenarioManager = scenarioManager;
            _graphManager = graphManager;
            _systemManager = systemManager;
            _config = config;
            _container = container;
            _XRInputManager = inputManager;
        }

        public IReadOnlyList<PortPresenter> Ports => ports;

        public BaseNode Model
        {
            get => _model;
            private set => _model = value;
        }

        private void Update()
        {
            if (Model.IsActive && Model.IsStarted && !Model.IsCompleted)
            {
                Play();
            }
        }


        public void Initialize(BaseNode model)
        {
            _model = model;
            _outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            _outline.effectColor = _config.outlineColor;
            _outline.enabled = false;
            //SetupUI();
            //CreatePorts();
            // Port presenter'ları bul ve initialize et
            ports = GetComponentsInChildren<PortPresenter>().ToList();

            // Her port için model oluştur ve initialize et
            foreach (var portPresenter in ports)
            {
                // Port tipine göre model oluştur
                PolarityType portType = portPresenter.Polarity;
                var portModel = new Port(portType, $"Port_{ports.IndexOf(portPresenter)}", this);

                // Port presenter'ı initialize et
                portPresenter.Initialize(portModel);
            }

            // UI elementlerini ayarla
            //SetupUI();
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
            if (!_model.EnableSelect) return;
            _outline.effectColor = _config.selectedColor;
            _outline.enabled = true;
            if (!_systemManager.selectedElements.Contains(this))
            {
                _systemManager.selectedElements.Add(this);
                _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementSelected, this);
            }
        }

        public void Unselect()
        {
            if (!_model.EnableSelect) return;
            _outline.enabled = false;
            if (_systemManager.selectedElements.Contains(this))
            {
                _systemManager.selectedElements.Remove(this);
                _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnElementUnselected, this);
            }
        }

        private void OnDestroy()
        {
            Unselect();
            if (_systemManager.clickedElement == this as IElement)
                _systemManager.clickedElement = null;
        }

        public bool DisableClick { get; }

        public void OnPointerDown()
        {
            if (!_systemManager.selectedElements.Contains(this))
            {
                Debug.Log("tiklandi");
                Select();
                transform.SetAsLastSibling();
                
                Vector2 localPointerPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _graphManager.CanvasRectTransform, 
                    _XRInputManager.ScreenPointerPosition, 
                    Camera.main, 
                    out localPointerPosition);
                _mouseDownPosition = Input.mousePosition;
                _nodeStartPosition = transform.localPosition;
                _dragOffset = (Vector3)localPointerPosition - transform.localPosition;
            }
            else
            {
                Unselect();
            }
        }

        public void OnPointerUp()
        {
        }

        public bool EnableDrag { get; set; } = true;

        public void OnBeginDrag()
        {
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
            //if (EnableDrag) Unselect();
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
                if (_systemManager.selectedElements.Contains(this))
                {
                    _outline.effectColor = _config.selectedColor;
                }
                else
                {
                    _outline.enabled = false;
                }
            }
        }

        public string ID { get; set; }

        public int Priority { get; }

        public void Remove() => Destroy(gameObject);

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

        public UnityEvent onActivated,
            onStarted,
            onCompleted,
            onDeactivated;

        public virtual void Play()
        {
            Debug.Log("This is base");
        }

        public virtual void ActivateNode()
        {
            Model.IsActive = true;
            onActivated.Invoke();
            ScenarioManager.ActiveNodePresenter = this;
        }

        public virtual void StartNode()
        {
            ActivateNode();
            Model.IsStarted = true;
            onStarted.Invoke();
        }

        public virtual void CompleteNode()
        {
            Model.IsCompleted = true;
            onCompleted.Invoke();
            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
        }

        private void OnLastNodeComplete()
        {
            DeactivateNode();
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
                        if (portPresenter.ConnectionPresenters[0].Model.TargetPort != null)
                        {
                            DeactivateNode();
                            portPresenter.ConnectionPresenters[0].Model.TargetPort.Model.baseNode.StartNode();
                            return true;
                        }
                    }
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
                            DeactivateNode();
                            portPresenter.ConnectionPresenters[0].Model.SourcePort.Model.baseNode.StartNode();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual void DeactivateNode()
        {
            Model.IsActive = false;
            Model.IsStarted = false;
            onDeactivated.Invoke();
        }

        public virtual void GoToNextNode()
        {
            if (TryToGoNextNode()) return;
            OnLastNodeComplete();
        }

        public virtual void GoToPreviousNode()
        {
            if (TryToGoPreviousNode()) return;
            Debug.LogWarning("En bastaki node'dasin!");
        }

        public virtual void OnComplete()
        {
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnSkip()
        {
        }

        #endregion
    }
}