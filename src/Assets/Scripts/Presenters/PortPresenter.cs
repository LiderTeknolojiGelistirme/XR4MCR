using Models;
using UnityEngine;
using UnityEngine.UI;
using Interfaces;
using System.Collections.Generic;
using Zenject;
using Managers;
using System;
using System.Linq;
using CustomGraphics;
using NodeSystem.Events;
using NodeSystem;

namespace Presenters
{
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class PortPresenter : MonoBehaviour, IGraphElement, IDraggable, IClickable, IHover
    {
        #region Fields

        private GraphManager _graphManager;
        private Port _model;
        private NodeConfig _config;
        private SystemManager _systemManager;
        private XRInputManager _inputManager;
        private Camera _cachedCamera;

        private Color color;
        private Color _defaultColor;
        private Color _hoverColor;
        private Color _selectedColor;
        private bool _isHovered;
        private bool _isSelected;
        private bool _isDragging = false;

        [SerializeField] private Image _portImage;
        private RectTransform _rectTransform;

        [SerializeField] private NodeSystem.PolarityType _polarity = NodeSystem.PolarityType.Bidirectional;

        private PortPresenter _closestFoundPort;
        private PortPresenter _lastFoundPort;

        #endregion

        #region Properties

        public Port Model
        {
            get => _model;
            private set => _model = value;
        }

        public int Priority => 2;

        public NodeSystem.PolarityType Polarity
        {
            get => _polarity;
            set
            {
                _polarity = value;
                if (_model != null)
                {
                    /* Model güncellemesi yapılabilir */
                }
            }
        }

        public bool EnableSelect { get; set; } = true;
        public bool EnableHover { get; set; } = true;
        public bool DisableClick { get; set; } = false;
        public string ID { get; set; } = "Port";

        public bool EnableDrag
        {
            get => _model.EnableDrag;
            set => _model.EnableDrag = value;
        }

        public int ConnectionsCount
        {
            get
            {
                int count = 0;
                foreach (ConnectionPresenter connectionPresenter in _graphManager.ConnectionPresenters)
                {
                    if (connectionPresenter.Model.SourcePort.Model == this.Model ||
                        (connectionPresenter.Model.TargetPort.Model == this.Model))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public List<ConnectionPresenter> ConnectionPresenters
        {
            get
            {
                List<ConnectionPresenter> connectionPresenters = new List<ConnectionPresenter>();
                foreach (ConnectionPresenter connectionPresenter in _graphManager.ConnectionPresenters)
                {
                    if (connectionPresenter.Model.SourcePort == this || connectionPresenter.Model.TargetPort == this)
                    {
                        connectionPresenters.Add(connectionPresenter);
                    }
                }

                return connectionPresenters;
            }
        }

        #endregion

        #region Unity Lifecycle

        [Inject]
        public void Construct(NodeConfig config, SystemManager systemManager, GraphManager graphManager,
            XRInputManager inputManager)
        {
            Debug.Log("ENTER: PortPresenter Construct");
            _config = config;
            _systemManager = systemManager;
            _graphManager = graphManager;
            _inputManager = inputManager;
        }


        protected virtual void Awake()
        {
            _cachedCamera = Camera.main;
        }

        void OnEnable()
        {
            _inputManager.e_OnPointerDown.AddListener(OnXRPointerDown);
            _inputManager.e_OnPointerUp.AddListener(OnXRPointerUp);
            _inputManager.e_OnDrag.AddListener(OnXRDrag);
        }

        void OnDisable()
        {
            _inputManager.e_OnPointerDown.RemoveListener(OnXRPointerDown);
            _inputManager.e_OnPointerUp.RemoveListener(OnXRPointerUp);
            _inputManager.e_OnDrag.RemoveListener(OnXRDrag);
        }
        private void SetupModel()
        {
            // Eğer Model henüz set edilmediyse, hata verin:
            if (Model == null)
            {
                Debug.LogError("PortPresenter modeli initialize edilmemiş!");
            }

            // Control point'in oluşturulduğundan emin olun:
            if (Model != null && Model.ControlPoint == null)
            {
                SetupControlPoint();
            }

            _defaultColor = Model?.Polarity == NodeSystem.PolarityType.Input ? _config.inputPortColor : _config.outputPortColor;
            _hoverColor = _config.hoverColor;
            _selectedColor = _config.selectedColor;

            color = _defaultColor;
            _rectTransform = GetComponent<RectTransform>();
            _portImage = GetComponent<Image>();
            SetupVisuals();
        }

        #endregion

        #region Public Methods

        public void Initialize(Port model)
        {
            Model = model;
            SetupImage();
            SetupControlPoint();
            SetupModel();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            RemoveAllConnections();
        }

        public void RemoveAllConnections()
        {
            List<ConnectionPresenter> connectionPresenters = ConnectionPresenters;
            for (int i = connectionPresenters.Count - 1; i >= 0; i--)
            {
                connectionPresenters[i].Remove();
            }

            UpdateIcon();
        }

        public void UpdateIcon()
        {
            if (Model.image)
            {
                if (ConnectionsCount > 0)
                {
                    Model.image.sprite = Model.iconConnected;
                }
                else
                {
                    Model.image.sprite = Model.iconUnconnected;
                    Model.image.color = Model.iconColorDefault;
                }
            }
        }

        public void SetControlPointDistanceAngle(float distance, float angle)
        {
            var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            var newPosition = transform.localPosition;
            newPosition.x = x;
            newPosition.y = y;
            Model.ControlPoint.LocalPosition = new Vector3(newPosition.x, newPosition.y, 0);

            _graphManager.UpdateConnectionsLine();
        }

        #endregion

        #region Drag Interface Implementation

        public void OnDrag(Vector2 position)
        {
            if (EnableDrag)
            {
                // Başlangıç portunun canvas'a göre local pozisyonunu Vector2 olarak al:
                Vector2 startPortLocalPosition = _graphManager.CanvasRectTransform.InverseTransformPoint(Model.image.transform.position);
        
                // XR controller'dan gelen pointer pozisyonunu canvas local olarak al (Vector3 → Vector2 dönüşümü yap):
                Vector2 pointerLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);

                // Line için noktaları hazırla (başlangıç portu → VR pointer pozisyonu):
                Vector2[] linePoints = new Vector2[] {
                    startPortLocalPosition,
                    pointerLocalPosition
                };

                // Ghost connection çizgisini güncelle:
                _graphManager.ghostConnectionLine.SetPoints(linePoints);
            }
        }


        public void OnBeginDrag()
        {
            throw new System.NotImplementedException();
        }

        public void OnEndDrag()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Click Interface Implementation

        void DrawOnDragConnectionLine()
        {
            _graphManager.ghostConnectionLine.Draw(_graphManager.LineRenderer);
        }

        public void OnPointerDown()
        {
            if (DisableClick) return;

            // ScrollRect'i sürükleme süresince devre dışı bırak
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = false;
            }

            _isSelected = true;
            color = _selectedColor;
            UpdateVisuals();
            _graphManager.LineRenderer.OnPopulateMeshAddListener(DrawOnDragConnectionLine);
            _systemManager.LTGEvents.TriggerEvent(LTGEventType.OnPointerDown, this);
            
            // Log: Porta tıklandı
            LogManager.Log($"Port clicked: {ID} ({Polarity})", Color.cyan);
        }

        public void OnPointerUp()
        {
            if (DisableClick) return;

            // ScrollRect'i tekrar etkinleştir
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }

            Debug.Log($"PortPresenter.OnPointerUp çağrıldı: {ID}");

            _isSelected = false;
            color = _isHovered ? _hoverColor : _defaultColor;
            _graphManager.ghostConnectionLine.points.Clear();
            _graphManager.LineRenderer.OnPopulateMeshRemoveListener(DrawOnDragConnectionLine);

            if (_systemManager.clickedElement is PortPresenter sourcePort && sourcePort != this)
            {
                Debug.Log($"Bağlantı oluşturuluyor: {sourcePort.ID} → {this.ID}");
                
                // Log: Bağlantı oluşturma girişimi
                LogManager.LogInteraction($"Attempting connection: {sourcePort.ID} ({sourcePort.Polarity}) → {ID} ({Polarity})");
                
                sourcePort.ConnectTo(this);
                _graphManager.UpdateConnectionsLine();
            }
            else
            {
                // Log: Port serbest bırakıldı, bağlantı oluşturulmadı
                LogManager.Log($"Port released: {ID} (no connection made)", new Color(0.7f, 0.7f, 0.7f));
            }
        }
        
        void OnXRPointerDown()
        {
            if (IsXRRayHittingThisPort())
            {
                // ScrollRect'i sürükleme süresince devre dışı bırak (XR için)
                if (_graphManager != null && _graphManager.scrollRect != null)
                {
                    _graphManager.scrollRect.enabled = false;
                }

                Debug.Log("PortPresenter XR Pointer Down!");
                OnPointerDown();
                _isDragging = true;
            }
        }

        bool IsXRRayHittingThisPort()
        {
            if (_inputManager.xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Vector3 hitPosCanvasLocal = _graphManager.CanvasRectTransform.InverseTransformPoint(hit.point);
                Vector2 portLocalPos = _graphManager.CanvasRectTransform.InverseTransformPoint(transform.position);
                hitPosCanvasLocal.z = 0f;

                float distance = Vector2.Distance(hitPosCanvasLocal, portLocalPos);

                // Buradaki mesafe eşik değerini ayarla (örneğin 30f)
                if (distance <= 30f)
                {
                    Debug.Log($"Port vuruldu! Mesafe: {distance}");
                    return true;
                }
            }
            return false;
        }

        void OnXRDrag(Vector3 pos)
        {
            if (_isDragging)
            {
                OnDrag(pos);
            }
        }

        void OnXRPointerUp()
        {
            // ScrollRect'i tekrar etkinleştir (XR için)
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }

            if (_isDragging)
            {
                OnPointerUp();
                _isDragging = false;
            }
        }

        private ConnectionPresenter ConnectTo(PortPresenter closestFoundPort)
        {
            Debug.Log("ConnectTo metodu çağrıldı mı?");

            var connection = _graphManager.CreateConnection(this, closestFoundPort);

            if (connection != null)
            {
                Debug.Log($"Bağlantı yaratıldı: {this.ID} -> {closestFoundPort.ID}");
                _graphManager.UpdateConnectionsLine();
                
                // Log: Bağlantı başarıyla oluşturuldu
                LogManager.LogSuccess($"Connection successful: {this.ID} -> {closestFoundPort.ID}");
            }
            else
            {
                Debug.LogError("GraphManager.CreateConnection null döndürdü!");
                
                // Log: Bağlantı oluşturulamadı
                LogManager.LogError($"Connection failed between: {this.ID} and {closestFoundPort.ID}");
            }

            return connection;
        }

        #endregion

        #region Hover Interface Implementation

        public void OnPointerHoverEnter()
        {
            if (!EnableHover) return;
            _isHovered = true;
            color = _hoverColor;
            UpdateVisuals();
        }

        public void OnPointerHoverExit()
        {
            if (!EnableHover) return;
            _isHovered = false;
            color = _isSelected ? _selectedColor : _defaultColor;
            UpdateVisuals();
        }

        #endregion

        #region Private Methods

        private void SetupVisuals()
        {
            if (_portImage != null)
            {
                _portImage.type = Image.Type.Simple;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (Model?.image != null)
            {
                Model.image.color = color;
            }
        }

        private void SetupImage()
        {
            var image = transform.GetComponentInChildren<Image>();
            if (!image)
            {
                image = new GameObject("Image", typeof(RectTransform)).AddComponent<Image>();
                image.transform.SetParent(transform);
                image.transform.localPosition = Vector3.zero;
                ((RectTransform)image.transform).sizeDelta = new Vector2(20, 20);
                image.raycastTarget = false;
            }

            Model.image = image;
        }

        private void SetupControlPoint()
        {
            var controlPoint = GetComponentInChildren<PortControlPoint>();
            if (!controlPoint)
            {
                controlPoint = new GameObject("Control Point", typeof(RectTransform)).AddComponent<PortControlPoint>();
                controlPoint.transform.SetParent(transform);
                ((RectTransform)controlPoint.transform).sizeDelta = Vector2.zero;
                Model.ControlPoint = controlPoint;
                SetControlPointDistanceAngle(50, 0);
                Debug.Log("Yeni control point oluşturuldu: " + controlPoint.transform.position + " for port: " +
                          transform.name);
            }
            else
            {
                Model.ControlPoint = controlPoint;
                Debug.Log("Varolan control point bulundu: " + controlPoint.transform.position + " for port: " +
                          transform.name);
            }
        }

        #endregion

        #region Types

        // PolarityType enum'u artık ayrı bir dosyada tanımlandı
        // Buradaki enum tanımını kaldırıyoruz
        
        #endregion
    }
}