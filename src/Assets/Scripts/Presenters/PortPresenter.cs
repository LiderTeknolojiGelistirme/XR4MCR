using Models;
using UnityEngine;
using UnityEngine.UI;
using Interfaces;
using System.Collections.Generic;
using Zenject;
using Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Commands;
using CustomGraphics;
using MeadowGames.UINodeConnect4.UICContextMenu;
using NodeSystem.Events;
using NodeSystem;
using Serilog;

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
                //LogManager.Log($"PortPresenter.Polarity.set STARTED - New value: {value}", Color.cyan);
                _polarity = value;
                if (_model != null)
                {
                    /* Model güncellemesi yapılabilir */
                }
                //LogManager.Log($"PortPresenter.Polarity.set COMPLETED", Color.green);
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
                //LogManager.Log($"PortPresenter.ConnectionsCount.get STARTED - Port: {ID}", Color.cyan);
                int count = 0;
                foreach (ConnectionPresenter connectionPresenter in _graphManager.ConnectionPresenters)
                {
                    if (connectionPresenter.Model.SourcePort.Model == this.Model ||
                        (connectionPresenter.Model.TargetPort.Model == this.Model))
                    {
                        count++;
                    }
                }
                //LogManager.Log($"PortPresenter.ConnectionsCount.get COMPLETED - Count: {count}", Color.green);
                return count;
            }
        }

        public List<ConnectionPresenter> ConnectionPresenters
        {
            get
            {
                //LogManager.Log($"PortPresenter.ConnectionPresenters.get STARTED - Port: {ID}", Color.cyan);
                List<ConnectionPresenter> connectionPresenters = new List<ConnectionPresenter>();
                foreach (ConnectionPresenter connectionPresenter in _graphManager.ConnectionPresenters)
                {
                    if (connectionPresenter.Model.SourcePort == this || connectionPresenter.Model.TargetPort == this)
                    {
                        connectionPresenters.Add(connectionPresenter);
                    }
                }
                //LogManager.Log($"PortPresenter.ConnectionPresenters.get COMPLETED - Found {connectionPresenters.Count} connections", Color.green);
                return connectionPresenters;
            }
        }

        #endregion

        #region Unity Lifecycle

        [Inject]
        public void Construct(NodeConfig config, SystemManager systemManager, GraphManager graphManager,
            XRInputManager inputManager)
        {
            //LogManager.Log("PortPresenter.Construct STARTED", Color.cyan);
            //Debug.Log("ENTER: PortPresenter Construct");
            _config = config;
            _systemManager = systemManager;
            _graphManager = graphManager;
            _inputManager = inputManager;
            //LogManager.Log("PortPresenter.Construct COMPLETED", Color.green);
        }


        protected virtual void Awake()
        {
            //LogManager.Log("PortPresenter.Awake STARTED", Color.cyan);
            _cachedCamera = Camera.main;
            //LogManager.Log("PortPresenter.Awake COMPLETED", Color.green);
        }

        void OnEnable()
        {
            //LogManager.Log("PortPresenter.OnEnable STARTED", Color.cyan);
            _inputManager.e_OnPointerDown.AddListener(OnXRPointerDown);
            _inputManager.e_OnPointerUp.AddListener(OnXRPointerUp);
            _inputManager.e_OnDrag.AddListener(OnXRDrag);
            //LogManager.Log("PortPresenter.OnEnable COMPLETED", Color.green);
        }

        void OnDisable()
        {
            //LogManager.Log("PortPresenter.OnDisable STARTED", Color.cyan);
            _inputManager.e_OnPointerDown.RemoveListener(OnXRPointerDown);
            _inputManager.e_OnPointerUp.RemoveListener(OnXRPointerUp);
            _inputManager.e_OnDrag.RemoveListener(OnXRDrag);
            //LogManager.Log("PortPresenter.OnDisable COMPLETED", Color.green);
        }
        
        private void SetupModel()
        {
            //LogManager.Log("PortPresenter.SetupModel STARTED", Color.cyan);
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
            //LogManager.Log("PortPresenter.SetupModel COMPLETED", Color.green);
        }

        #endregion

        #region Public Methods

        public void Initialize(Port model)
        {
            //LogManager.Log($"PortPresenter.Initialize STARTED - Port: {model?.ID}", Color.cyan);
            Model = model;
            SetupImage();
            SetupControlPoint();
            SetupModel();
            //LogManager.Log($"PortPresenter.Initialize COMPLETED", Color.green);
        }

        public void Remove()
        {
            //LogManager.Log($"PortPresenter.Remove STARTED - Port: {ID}", Color.cyan);
            Destroy(gameObject);
            //LogManager.Log($"PortPresenter.Remove COMPLETED", Color.green);
        }

        private void OnDestroy()
        {
            //LogManager.Log($"PortPresenter.OnDestroy STARTED - Port: {ID}", Color.cyan);
            RemoveAllConnections();
            //LogManager.Log($"PortPresenter.OnDestroy COMPLETED", Color.green);
        }

        public void RemoveAllConnections()
        {
            //LogManager.Log($"PortPresenter.RemoveAllConnections STARTED - Port: {ID}", Color.cyan);
            List<ConnectionPresenter> connectionPresenters = ConnectionPresenters;
            for (int i = connectionPresenters.Count - 1; i >= 0; i--)
            {
                connectionPresenters[i].Remove();
            }

            UpdateIcon();
            //LogManager.Log($"PortPresenter.RemoveAllConnections COMPLETED - Removed {connectionPresenters.Count} connections", Color.green);
        }

        public void UpdateIcon()
        {
            //LogManager.Log($"PortPresenter.UpdateIcon STARTED - Port: {ID}", Color.cyan);
            if (Model.image)
            {
                    Model.image.color = Model.iconColorDefault;
            }
            //LogManager.Log($"PortPresenter.UpdateIcon COMPLETED", Color.green);
        }

        public void SetControlPointDistanceAngle(float distance, float angle)
        {
            //LogManager.Log($"PortPresenter.SetControlPointDistanceAngle STARTED - Distance: {distance}, Angle: {angle}", Color.cyan);
            var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            var newPosition = transform.localPosition;
            newPosition.x = x;
            newPosition.y = y;
            Model.ControlPoint.LocalPosition = new Vector3(newPosition.x, newPosition.y, 0);

            _graphManager.UpdateConnectionsLine();
            //LogManager.Log($"PortPresenter.SetControlPointDistanceAngle COMPLETED", Color.green);
        }

        #endregion

        #region Drag Interface Implementation

        public void OnDrag(Vector2 position)
        {
            //LogManager.Log($"PortPresenter.OnDrag STARTED - Position: {position}", Color.cyan);
            if (EnableDrag)
            {
                // Başlangıç portunun canvas'a göre local pozisyonunu Vector2 olarak al:
                Vector2 startPortLocalPosition = _graphManager.CanvasRectTransform.InverseTransformPoint(Model.image.transform.position);
        
                // XR controller'dan gelen pointer pozisyonunu canvas local olarak al (Vector3 → Vector2 dönüşümü yap):
                Vector3 pointerLocalPosition = _inputManager.GetCanvasPointerPosition(_graphManager);
                
                // COORDINATE SPACE COMPENSATION - Scale ve Scroll offset'lerini kompanse et
                // Bu, Pointer sınıfındaki aynı kompensasyon sistemi
                if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
                {
                    Vector2 contentPosition = _graphManager.scrollRect.content.anchoredPosition;
                    Vector3 contentScale = _graphManager.contentTransform.localScale;
                    
                    // Scale ile pozisyonu çarp (content ne kadar scale'lendiyse ghost line da o kadar çarpılmalı)
                    pointerLocalPosition.x *= contentScale.x;
                    pointerLocalPosition.y *= contentScale.y;
                    
                    // Content'in scroll offset'ini ekle
                    pointerLocalPosition.x += contentPosition.x;
                    pointerLocalPosition.y += contentPosition.y;
                }

                // Line için noktaları hazırla (başlangıç portu → kompanse edilmiş pointer pozisyonu):
                Vector2[] linePoints = new Vector2[] {
                    startPortLocalPosition,
                    pointerLocalPosition
                };

                // Ghost connection çizgisini güncelle:
                _graphManager.ghostConnectionLine.SetPoints(linePoints);
            }
            //LogManager.Log($"PortPresenter.OnDrag COMPLETED", Color.green);
        }


        public void OnBeginDrag()
        {
            //LogManager.Log($"PortPresenter.OnBeginDrag STARTED", Color.cyan);
            //throw new System.NotImplementedException();
            //LogManager.Log($"PortPresenter.OnBeginDrag COMPLETED", Color.green);
        }

        public void OnEndDrag()
        {
            //LogManager.Log($"PortPresenter.OnEndDrag STARTED", Color.cyan);
            //throw new System.NotImplementedException();
            //LogManager.Log($"PortPresenter.OnEndDrag COMPLETED", Color.green);
        }

        #endregion

        #region Click Interface Implementation

        void DrawOnDragConnectionLine()
        {
            //LogManager.Log($"PortPresenter.DrawOnDragConnectionLine STARTED", Color.cyan);
            _graphManager.ghostConnectionLine.Draw(_graphManager.LineRenderer);
            //LogManager.Log($"PortPresenter.DrawOnDragConnectionLine COMPLETED", Color.green);
        }

        public void OnPointerDown()
        {
            //LogManager.Log($"PortPresenter.OnPointerDown STARTED - Port: {ID}", Color.cyan);
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
            //LogManager.Log($"PortPresenter.OnPointerDown COMPLETED", Color.green);
        }

        public void OnPointerUp()
        {
            //LogManager.Log($"PortPresenter.OnPointerUp STARTED - Port: {ID}", Color.cyan);
            if (DisableClick) return;

            // ScrollRect'i tekrar etkinleştir
            if (_graphManager != null && _graphManager.scrollRect != null)
            {
                _graphManager.scrollRect.enabled = true;
            }

            //Debug.Log($"PortPresenter.OnPointerUp çağrıldı: {ID}");

            _isSelected = false;
            color = _isHovered ? _hoverColor : _defaultColor;
            _graphManager.ghostConnectionLine.points.Clear();
            _graphManager.LineRenderer.OnPopulateMeshRemoveListener(DrawOnDragConnectionLine);

            if (_systemManager.clickedElement is PortPresenter sourcePort && sourcePort != this)
            {
                //Debug.Log($"Bağlantı oluşturuluyor: {sourcePort.ID} → {this.ID}");
                
                // Log: Bağlantı oluşturma girişimi
                LogManager.LogInteraction($"Attempting connection: {sourcePort.ID} ({sourcePort.Polarity}) → {ID} ({Polarity})");
                
                
                var cp = sourcePort.ConnectTo(this);
                _graphManager.UpdateConnectionsLine();
                UndoRedoManager.Insert(new CreateConnectionCommand(sourcePort,this,cp,_graphManager));
            }
            else
            {
                // Log: Port serbest bırakıldı, bağlantı oluşturulmadı
                LogManager.LogWarning($"Port released: {ID} (no connection made) {_model.Polarity}");
            }
            //LogManager.Log($"PortPresenter.OnPointerUp COMPLETED", Color.green);
        }
        
        void OnXRPointerDown()
        {
            //LogManager.Log($"PortPresenter.OnXRPointerDown STARTED", Color.cyan);
            if (IsXRRayHittingThisPort())
            {
                // ScrollRect'i sürükleme süresince devre dışı bırak (XR için)
                if (_graphManager != null && _graphManager.scrollRect != null)
                {
                    _graphManager.scrollRect.enabled = false;
                }

                //Debug.Log("PortPresenter XR Pointer Down!");
                OnPointerDown();
                _isDragging = true;
            }
            //LogManager.Log($"PortPresenter.OnXRPointerDown COMPLETED", Color.green);
        }

        bool IsXRRayHittingThisPort()
        {
            //LogManager.Log($"PortPresenter.IsXRRayHittingThisPort STARTED - Port: {ID}", Color.cyan);
            if (_inputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                Vector3 hitPosCanvasLocal = _graphManager.CanvasRectTransform.InverseTransformPoint(hit.point);
                Vector2 portLocalPos = _graphManager.CanvasRectTransform.InverseTransformPoint(transform.position);
                hitPosCanvasLocal.z = 0f;

                // COORDINATE SPACE COMPENSATION - Scale offset'ini kompanse et
                // Port detection için scale faktörünü hesaba kat
                if (_graphManager.scrollRect != null && _graphManager.scrollRect.content != null)
                {
                    Vector3 contentScale = _graphManager.contentTransform.localScale;
                    
                    // Hit pozisyonunu scale'e göre ayarla
                    hitPosCanvasLocal.x /= contentScale.x;
                    hitPosCanvasLocal.y /= contentScale.y;
                }

                float distance = Vector2.Distance(hitPosCanvasLocal, portLocalPos);

                // Scale'e göre mesafe eşiğini ayarla
                float scaleAdjustedThreshold = 30f;
                if (_graphManager.contentTransform != null)
                {
                    float avgScale = (_graphManager.contentTransform.localScale.x + _graphManager.contentTransform.localScale.y) / 2f;
                    scaleAdjustedThreshold = 30f / avgScale; // Scale küçüldükçe threshold büyür
                }

                if (distance <= scaleAdjustedThreshold)
                {
                    //Debug.Log($"Port vuruldu! Mesafe: {distance}, Threshold: {scaleAdjustedThreshold}");
                    //LogManager.Log($"PortPresenter.IsXRRayHittingThisPort COMPLETED - Hit detected, distance: {distance}", Color.green);
                    return true;
                }
            }
            //LogManager.Log($"PortPresenter.IsXRRayHittingThisPort COMPLETED - No hit", Color.green);
            return false;
        }

        void OnXRDrag(Vector3 pos)
        {
            //LogManager.Log($"PortPresenter.OnXRDrag STARTED - Position: {pos}", Color.cyan);
            if (_isDragging)
            {
                OnDrag(pos);
            }
            //LogManager.Log($"PortPresenter.OnXRDrag COMPLETED", Color.green);
        }

        void OnXRPointerUp()
        {
            //LogManager.Log($"PortPresenter.OnXRPointerUp STARTED", Color.cyan);
            
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
            //LogManager.Log($"PortPresenter.OnXRPointerUp COMPLETED", Color.green);
        }

        public ConnectionPresenter ConnectTo(PortPresenter closestFoundPort)
        {
            //LogManager.Log($"PortPresenter.ConnectTo STARTED - From: {ID} To: {closestFoundPort?.ID}", Color.cyan);
            //Debug.Log("ConnectTo metodu çağrıldı mı?");

            ConnectionPresenter connection = _graphManager.CreateConnection(this, closestFoundPort);

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

            //LogManager.Log($"PortPresenter.ConnectTo COMPLETED - Connection: {connection != null}", Color.green);
            return connection;
        }

        #endregion

        #region Hover Interface Implementation

        public void OnPointerHoverEnter()
        {
            //LogManager.Log($"PortPresenter.OnPointerHoverEnter STARTED - Port: {ID}", Color.cyan);
            if (!EnableHover) return;
            _isHovered = true;
            color = _hoverColor;
            UpdateVisuals();
            //LogManager.Log($"PortPresenter.OnPointerHoverEnter COMPLETED", Color.green);
        }

        public void OnPointerHoverExit()
        {
            //LogManager.Log($"PortPresenter.OnPointerHoverExit STARTED - Port: {ID}", Color.cyan);
            if (!EnableHover) return;
            _isHovered = false;
            color = _isSelected ? _selectedColor : _defaultColor;
            UpdateVisuals();
            //LogManager.Log($"PortPresenter.OnPointerHoverExit COMPLETED", Color.green);
        }

        #endregion

        #region Private Methods

        private void SetupVisuals()
        {
            //LogManager.Log($"PortPresenter.SetupVisuals STARTED", Color.cyan);
            if (_portImage != null)
            {
                _portImage.type = Image.Type.Simple;
                UpdateVisuals();
            }
            //LogManager.Log($"PortPresenter.SetupVisuals COMPLETED", Color.green);
        }

        private void UpdateVisuals()
        {
            //LogManager.Log($"PortPresenter.UpdateVisuals STARTED", Color.cyan);
            if (Model?.image != null)
            {
                Model.image.color = color;
            }
            //LogManager.Log($"PortPresenter.UpdateVisuals COMPLETED", Color.green);
        }

        private void SetupImage()
        {
            //LogManager.Log($"PortPresenter.SetupImage STARTED", Color.cyan);
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
            //LogManager.Log($"PortPresenter.SetupImage COMPLETED", Color.green);
        }

        private void SetupControlPoint()
        {
            //LogManager.Log($"PortPresenter.SetupControlPoint STARTED", Color.cyan);
            var controlPoint = GetComponentInChildren<PortControlPoint>();
            if (!controlPoint)
            {
                controlPoint = new GameObject("Control Point", typeof(RectTransform)).AddComponent<PortControlPoint>();
                controlPoint.transform.SetParent(transform);
                ((RectTransform)controlPoint.transform).sizeDelta = Vector2.zero;
                Model.ControlPoint = controlPoint;
                SetControlPointDistanceAngle(50, 0);
                //Debug.Log("Yeni control point oluşturuldu: " + controlPoint.transform.position + " for port: " +
                //          transform.name);
            }
            else
            {
                Model.ControlPoint = controlPoint;
                //Debug.Log("Varolan control point bulundu: " + controlPoint.transform.position + " for port: " +
                //          transform.name);
            }
            //LogManager.Log($"PortPresenter.SetupControlPoint COMPLETED", Color.green);
        }

        #endregion

        #region Types

        // PolarityType enum'u artık ayrı bir dosyada tanımlandı
        // Buradaki enum tanımını kaldırıyoruz
        
        #endregion
    }
}