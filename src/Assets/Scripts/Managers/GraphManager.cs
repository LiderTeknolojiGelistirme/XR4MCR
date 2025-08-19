using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Presenters;
using CustomGraphics;
using Enums;
using Factories;
using Models;
using Presenters.NodePresenters;
using Unity.XR.CoreUtils;
using Zenject;
using LTGLineRenderer = CustomGraphics.LTGLineRenderer;
using Object = UnityEngine.Object;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.SceneManagement;
using Virtualware.Networking.Client.SessionManagement;
using UI;
using Actions;
using Viroo.Interactions;

namespace Managers
{
    [DefaultExecutionOrder(-10)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class GraphManager : MonoBehaviour
    {
        private float maxScale = 3f;
        private float minScale = .2f;

        public TMP_InputField scaleInput;
        public Slider scaleSlider;
        private float scaleValue { get; set; }
        public RectTransform contentTransform;
        public ScrollRect scrollRect;
        public Camera MainCamera;
        private LTGLineRenderer _lineRenderer;
        public Image gridImage;
        public LTGLineRenderer LineRenderer => _lineRenderer;
        public Line ghostConnectionLine;
        public Pointer Pointer { get; private set; }

        [SerializeField] private Canvas _canvas;

        public Canvas Canvas =>
            _canvas; // Direkt serialize edilmiş field'ı döndür // Pointer prefabını inspector'dan atamak için

        private GameObject _pointer;
        private RectTransform _canvasRectTransform;
        public RectTransform CanvasRectTransform => _canvasRectTransform ??= Canvas.transform as RectTransform;

        private List<BaseNodePresenter> _nodePresenters = new List<BaseNodePresenter>();
        public List<BaseNodePresenter> NodePresenters => _nodePresenters;

        public StartNodePresenter StartNode { get; set; }
        public FinishNodePresenter FinishNode { get; set; }

        private List<Connection> localConnections = new List<Connection>();
        public RenderMode CanvasRenderMode => Canvas.renderMode;

        private NodeGraph _model;

        private NodeConfig _config;
        private SystemManager _systemManager;

        private NodePresenterFactory _nodePresenterFactory;
        private XRInputManager _inputManager;
        private bool _isInitialized = false;

        private ObjectFactory _objectFactory;

        private ScenarioFileManager _scenarioFileManager;

        [SerializeField] private float _connectionDetectionDistance = 10f;

        public float ConnectionDetectionDistance
        {
            get => _connectionDetectionDistance;
            set => _connectionDetectionDistance = value;
        }

        [Header("Dynamic Content System")] [SerializeField]
        private float _contentMultiplier = 2f; // Content viewport'un kaç katı olsun

        [SerializeField] private Vector2 _expansionOffset = new Vector2(1000f, 500f); // Genişletme offset'i

        private INetworkObjectsService _networkObjectsService;
        private INetworkScenesService _networkScenesService;
        private ISessionClientsProvider _sessionClientsProvider;

        [SerializeField] private PrefabInstantiableContainer _prefabContainer;

        #region Connection Management

        private ConnectionPresenterFactory _connectionPresenterFactory;
        //private ConnectionSyncAction _connectionSyncAction;

        // Connection'lar için dictionary (ID -> Presenter)
        private List<ConnectionPresenter> _connectionPresenters = new();
        public List<ConnectionPresenter> ConnectionPresenters => _connectionPresenters;
        


        public ConnectionPresenter CreateConnection(PortPresenter sourcePort, PortPresenter targetPort)
        {
            try
            {
                // Önce VIROO Actions sistemini dene
                if (_connectionCreateAction != null)
                {
                    _connectionCreateAction.CreateConnection(sourcePort, targetPort);
                    var connectionPresenter = _connectionCreateAction._createdConnectionPresenter;
                    
                    if (connectionPresenter != null)
                    {
                        return connectionPresenter;
                    }
                    else
                    {
                        LogManager.LogWarning("[CreateConnection] VIROO Action returned null connection - factory fallback'a geçiliyor");
                    }
                }
                else
                {
                    LogManager.LogWarning("[CreateConnection] ConnectionCreateAction is null - factory fallback kullanılacak");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"[CreateConnection] VIROO Action connection hatası: {e.Message} - Stack trace: {e.StackTrace}");
                LogManager.LogError($"[CreateConnection] Factory fallback'a geçiliyor...");
            }
            
            // VIROO Actions başarısız olursa Factory fallback kullan
            var result = CreateLocalConnection(sourcePort, targetPort);
            
            return result;
        }

        private async Task<ConnectionPresenter> CreateConnectionFromCanvasContainer(PortPresenter sourcePort,
            PortPresenter targetPort)
        {
            try
            {
                if (_networkObjectsService == null)
                {
                    LogManager.LogError("[GraphManager Connection Create] NetworkObjectsService henüz inject edilmedi!");
                    return null;
                }


                // Canvas container'ı bularak o container'dan oluştur
                var canvasContainer = contentTransform.GetComponent<PrefabInstantiableContainer>();
                if (canvasContainer == null)
                {
                    LogManager.LogError(
                        "[GraphManager Connection Create] Canvas Content'inde PrefabInstantiableContainer bulunamadı!");
                    return null;
                }


                // VIROO ile Canvas'da oluştur
                LogManager.LogError("var createResponse = await _networkObjectsService.CreateDynamicObject(\n                    \"connection\",\n                    Vector3.zero,\n                    Quaternion.identity,\n                    requestAuthority: true,\n                    isPersistent: true,\n                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name\n                );");
                var createResponse = await _networkObjectsService.CreateDynamicObject(
                    "connection",
                    Vector3.zero,
                    Quaternion.identity,
                    requestAuthority: true,
                    isPersistent: true,
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );

                if (createResponse.Success)
                {
                    GameObject createdObject = createResponse.InstantiatedObject.GameObject;


                    // Canvas ZenjectInjector ile inject et ve initialize et
                    TryInjectConnection(createdObject, sourcePort, targetPort);

                    return createdObject.GetComponent<ConnectionPresenter>();
                }
                else
                {
                    LogManager.LogError($"[ConnectionCreate GraphManager] Canvas connection oluşturulamadı!");
                    return null;
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"[ConnectionCreate GraphManager] Canvas connection oluşturulurken hata: {e.Message}");
                return null;
            }
        }

        private void TryInjectConnection(GameObject createdConnection, PortPresenter sourcePort,
            PortPresenter targetPort)
        {
            try
            {
                var canvasInjector = contentTransform.GetComponent<ZenjectInjector>();
                if (canvasInjector != null)
                {
                    canvasInjector.InjectObject(createdConnection);
                }
                else
                {
                    LogManager.LogWarning(
                        "[GraphManager Connection Create] Canvas Content'inde ZenjectInjector bulunamadı!");
                }

                var connectionPresenter = createdConnection.GetComponent<ConnectionPresenter>();
                if (connectionPresenter != null)
                {
                    // NodePresenterFactory'nin yaptığı gibi Model oluştur ve initialize et
                    ManuallyInitializeConnectionPresenter(connectionPresenter, sourcePort, targetPort);

                    // GraphManager'a ekle
                    ConnectionPresenters.Add(connectionPresenter);

                    // Host'ta oluşturulan connection'ı client'lara senkronize et
                    SyncConnectionToClients(sourcePort, targetPort, connectionPresenter);
                }
                else
                {
                    LogManager.LogError($"[GraphManager Connection Create] {createdConnection.name} nesnesinde connectionpresenter bulunamadı!");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"[GraphManager Connection Create] connection injection/initialization hatası: {e.Message}");
            }
        }
        
        private void ManuallyInitializeConnectionPresenter(ConnectionPresenter presenter, PortPresenter sourcePort, PortPresenter targetPort)
        {
            try
            {
                // NodePresenterFactory'nin CreateModel metodunun yaptığını burada yapalım
                Connection model = new Connection(sourcePort, targetPort);
                
                // Model'i presenter'a ata
                
                
                // Initialize metodunu çağır
                presenter.Initialize(model);
            }
            catch (Exception e)
            {
                LogManager.LogError($"[GraphManager Connection Create] connection model initialization hatası: {e.Message}");
            }
        }

        public ConnectionPresenter CreatePreviewConnection(PortPresenter startPort)
        {
            return _connectionPresenterFactory.CreatePreviewConnection(startPort);
        }

        public void RemoveConnection(ConnectionPresenter connectionPresenter)
        {
            if (_connectionPresenters.Contains(connectionPresenter))
            {
                Destroy(connectionPresenter.gameObject);
                _connectionPresenters.Remove(connectionPresenter);
            }
        }

        /// <summary>
        /// Client tarafında local connection oluştur (network object olmadan)
        /// </summary>
        public ConnectionPresenter CreateLocalConnection(PortPresenter sourcePort, PortPresenter targetPort)
        {
            try
            {
                // Factory kullanarak connection oluştur
                var connectionPresenter = _connectionPresenterFactory.CreateConnection(sourcePort, targetPort);
                
                if (connectionPresenter != null)
                {
                    // GraphManager'a ekle
                    ConnectionPresenters.Add(connectionPresenter);
                    
                    // Line'ları güncelle
                    UpdateConnectionsLine();
                    
                    return connectionPresenter;
                }
                else
                {
                    LogManager.LogError("[CreateLocalConnection] Connection factory NULL döndü!");
                    return null;
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"[CreateLocalConnection] Local connection oluşturma hatası: {e.Message}");
                LogManager.LogError($"[CreateLocalConnection] Stack trace: {e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Host'ta oluşturulan connection'ı Viroo Actions ile client'lara senkronize eder
        /// </summary>
        private void SyncConnectionToClients(PortPresenter sourcePort, PortPresenter targetPort, ConnectionPresenter connectionPresenter)
        {
            // Geçici olarak disabled - ConnectionSyncAction inject edilemiyor
            /*
            LogManager.Log("[GraphManager] SyncConnectionToClients STARTED", Color.cyan);
            
            try
            {
                if (_connectionSyncAction == null)
                {
                    LogManager.LogError("[GraphManager] ConnectionSyncAction is not injected!");
                    return;
                }

                // Port ve Node ID'lerini al
                string sourcePortId = sourcePort.Model?.ID;
                string targetPortId = targetPort.Model?.ID;
                string sourceNodeId = sourcePort.Model?.baseNode?.Model?.ID;
                string targetNodeId = targetPort.Model?.baseNode?.Model?.ID;

                if (string.IsNullOrEmpty(sourcePortId) || string.IsNullOrEmpty(targetPortId) ||
                    string.IsNullOrEmpty(sourceNodeId) || string.IsNullOrEmpty(targetNodeId))
                {
                    LogManager.LogError($"[GraphManager] Missing IDs - SourcePort: {sourcePortId}, TargetPort: {targetPortId}, SourceNode: {sourceNodeId}, TargetNode: {targetNodeId}");
                    return;
                }

                // Viroo Actions ile broadcast et
                _connectionSyncAction.BroadcastConnectionCreated(sourcePortId, targetPortId, sourceNodeId, targetNodeId);
                
                LogManager.LogSuccess($"[GraphManager] Connection sync broadcast completed: {sourcePortId} -> {targetPortId}");
            }
            catch (Exception e)
            {
                LogManager.LogError($"[GraphManager] Connection sync error: {e.Message}");
            }
            */
        }

        // Connection modellerine erişmek için extension
        public IEnumerable<Connection> ConnectionModels => _connectionPresenters.Select(p => p.Model);


        public IEnumerable<Connection> GetPortConnections(PortPresenter portRef)
        {
            return ConnectionModels.Where(c =>
                c.SourcePort.Equals(portRef) || c.TargetPort.Equals(portRef));
        }

        #endregion

        #region Dynamic Content System

        /// <summary>
        /// Viewport size'ına göre varsayılan content size'ı ayarlar
        /// </summary>
        private void InitializeContentSize()
        {
            if (scrollRect == null || scrollRect.viewport == null || contentTransform == null) return;

            // Viewport size'ı al
            Vector2 viewportSize = scrollRect.viewport.rect.size;

            // Default content size = viewport * multiplier
            Vector2 defaultContentSize = viewportSize * _contentMultiplier;
            contentTransform.sizeDelta = defaultContentSize;

            // Content'in merkezini viewport'un merkezinde göstermek için pozisyonu ayarla
            // Content'in merkezi (0,0) viewport'un merkezinde görünmeli
            Vector2 centerOffset = defaultContentSize * 0.5f - viewportSize * 0.5f;
            contentTransform.anchoredPosition = centerOffset;
        }

        /// <summary>
        /// Mevcut görünür alanın bounds'larını hesaplar
        /// </summary>
        private Bounds GetCurrentVisibleBounds()
        {
            if (scrollRect == null || scrollRect.viewport == null)
                return new Bounds(Vector3.zero, Vector3.one * 1000f);

            Vector2 viewportSize = scrollRect.viewport.rect.size;
            Vector2 contentPosition = scrollRect.content.anchoredPosition;

            // Görünür alanın merkezi (content space'inde)
            Vector2 visibleCenter = -contentPosition;

            // Görünür alan bounds'ı
            Bounds visibleBounds = new Bounds(visibleCenter, viewportSize);

            return visibleBounds;
        }

        /// <summary>
        /// Node pozisyonunun görünür alan dışında olup olmadığını kontrol eder
        /// </summary>
        /// <param name="nodePosition">Node pozisyonu</param>
        /// <returns>True eğer content genişletilmesi gerekiyorsa</returns>
        public bool ShouldExpandContentForNode(Vector2 nodePosition)
        {
            Bounds visibleBounds = GetCurrentVisibleBounds();

            // Node görünür alan içinde mi?
            if (visibleBounds.Contains(nodePosition))
            {
                return false; // Genişletme gerekmiyor
            }

            return true; // Genişletme gerekiyor
        }

        /// <summary>
        /// Content'i node pozisyonuna göre dinamik olarak genişletir
        /// </summary>
        /// <param name="nodePosition">Yeni node pozisyonu</param>
        public void ExpandContentForNode(Vector2 nodePosition)
        {
            if (contentTransform == null) return;

            Vector2 currentContentSize = contentTransform.sizeDelta;
            Vector2 newContentSize = currentContentSize;

            // X ekseni kontrolü
            float nodeAbsX = Mathf.Abs(nodePosition.x);
            float requiredXSize = nodeAbsX + _expansionOffset.x / 2f;
            if (requiredXSize > currentContentSize.x / 2f)
            {
                newContentSize.x = requiredXSize * 2f; // Merkezi korumak için 2 ile çarp
            }

            // Y ekseni kontrolü
            float nodeAbsY = Mathf.Abs(nodePosition.y);
            float requiredYSize = nodeAbsY + _expansionOffset.y / 2f;
            if (requiredYSize > currentContentSize.y / 2f)
            {
                newContentSize.y = requiredYSize * 2f; // Merkezi korumak için 2 ile çarp
            }

            // Content size'ı güncelle (sadece gerekiyorsa)
            if (newContentSize != currentContentSize)
            {
                contentTransform.sizeDelta = newContentSize;

                // Viewport merkezi content'in merkezinde görünmesi için pozisyonu sıfırla
                // Bu, content'in merkez noktasını viewport'un merkez noktasına hizalar
                contentTransform.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Content pozisyonunu viewport merkezine sıfırlar (hatalı pozisyonu düzeltmek için)
        /// </summary>
        public void ResetContentPosition()
        {
            if (contentTransform != null)
            {
                contentTransform.anchoredPosition = Vector2.zero;
            }
        }

        #endregion

        private void OnEnable()
        {
            Debug.Log($"GraphManager OnEnable: IsInitialized={_isInitialized}");
            LineRenderer?.OnPopulateMeshAddListener(DrawConnections);
            scaleValue = contentTransform.localScale.x;

            // ScenarioFileManager event'lerini bağla
            SubscribeToFileManagerEvents();

            // Content pozisyonunu otomatik düzelt (dinamik büyüyen content için)
            ResetContentPosition();
        }

        private void DrawConnections()
        {
            foreach (ConnectionPresenter connection in ConnectionPresenters)
            {
                connection.Model.line.Draw(LineRenderer);
            }
        }

        private void Awake()
        {
            // Sadece temel setup
            InitializeCanvas();
            this.QueueForInject();
        }
        
        protected void Inject(INetworkObjectsService networkObjectsService)
        {
            this._networkObjectsService = networkObjectsService;
        }


        //, INetworkObjectsService networkObjectsService, INetworkScenesService networkScenesService, ISessionClientsProvider sessionClientsProvider

        private ConnectionCreateAction _connectionCreateAction;
        
        [Inject]
        public void Construct(NodeConfig config, SystemManager systemManager,
            ConnectionPresenterFactory connectionPresenterFactory, NodePresenterFactory nodePresenterFactory,
            XRInputManager inputManager,
            Pointer pointer, LTGLineRenderer lineRenderer, ObjectFactory objectFactory, ConnectionCreateAction connectionCreateAction)
        {
            // Eğer zaten construct edilmişse çık
            if (_isInitialized)
            {
                LogManager.LogWarning("GraphManager already initialized!");
                return;
            }

            _config = config;
            _systemManager = systemManager;
            _connectionPresenterFactory = connectionPresenterFactory;
            _nodePresenterFactory = nodePresenterFactory;
            _inputManager = inputManager;
            _pointer = pointer.gameObject;
            _lineRenderer = lineRenderer;
            _objectFactory = objectFactory;
            _connectionCreateAction = connectionCreateAction;
            //_connectionSyncAction = connectionSyncAction;

            if (_lineRenderer == null)
            {
                Debug.LogError("LTGLineRenderer null!");
            }
            else
            {
                Debug.Log("LTGLineRenderer initialize edildi: " + _lineRenderer.gameObject.name);
            }

            Initialize();
            _isInitialized = true;
            Debug.Log("GraphManager initialized");
            CreateStartNode();
            CreateFinishNode();

            // Node'lar oluşturulduktan sonra content pozisyonunu düzelt
            ResetContentPosition();
        }

        private void Initialize()
        {
            if (!_isInitialized) // Eğer henüz initialize edilmemişse
            {
                _model = new NodeGraph();
                InitializeEvents();
                InitializePointer();
            }
        }


        private void InitializePointer()
        {
            Debug.Log("Initializing Pointer...");

            var pointerComponent = _pointer.GetComponent<Pointer>();

            // Config'den ikonları ve ayarları al
            pointerComponent.Initialize(
                _config.pointerColor,
                _config.defaultPointerSprite,
                _config.dragPointerSprite,
                _config.pointerSize
            );

            // Pointer sınıfına referansı kaydet
            Pointer = pointerComponent;

            Debug.Log("Pointer initialized successfully");
        }

        private void InitializeCanvas()
        {
            if (!MainCamera)
                MainCamera = Camera.main;
        }

        private void InitializeEvents()
        {
            if (_systemManager == null)
            {
                Debug.LogError("SystemManager is null! Waiting for injection...");
                return;
            }
        }

        private void OnDestroy()
        {
            // Pointer'ı temizle
            if (Pointer != null)
            {
                var pointerGO = Pointer.GetPointerImage()?.gameObject;
                if (pointerGO != null)
                {
                    DestroyImmediate(pointerGO);
                }
            }

            // Event'ları temizle
            UnsubscribeFromFileManagerEvents();
        }

        public BaseNodePresenter InstantiateNode(BaseNodePresenter baseNodeTemplate, Vector3 position)
        {
            BaseNodePresenter newBaseNodePresenter = Instantiate(baseNodeTemplate, Canvas.transform);
            newBaseNodePresenter.transform.position = position;
            return newBaseNodePresenter;
        }

        private void AddNode(BaseNodePresenter baseNode)
        {
            if (!_nodePresenters.Contains(baseNode))
            {
                _nodePresenters.Add(baseNode);
            }
        }

        private void RemoveNode(BaseNodePresenter baseNode)
        {
            if (_nodePresenters.Contains(baseNode))
            {
                _nodePresenters.Remove(baseNode);
            }
        }

        private void OnNodeAdded(object nodeObj)
        {
            if (nodeObj is BaseNodePresenter node && !_nodePresenters.Contains(node))
            {
                _nodePresenters.Add(node);
            }
        }

        private void OnNodeRemoved(object nodeObj)
        {
            if (nodeObj is BaseNodePresenter node && _nodePresenters.Contains(node))
            {
                _nodePresenters.Remove(node);
            }
        }

        public void RemoveSelectedNodes()
        {
            var nodesToRemove = _nodePresenters.ToList();
            foreach (var node in nodesToRemove)
            {
                if (node != null)
                {
                    RemoveNode(node);
                }
            }
        }

        public void RemoveSelectedObjects()
        {
            _systemManager.Selected3DObject.GetComponent<ObjectPresenter>().Remove();
        }


        private void OnValidate()
        {
            InitializeCanvas();
        }

        public BaseNodePresenter CreateNodeAtPosition(Vector2 position, NodeType nodeType, BaseNode baseNode = null)
        {
            var nodePresenter = CreateNodePresenter(position, nodeType, baseNode);

            //Pozisyonu açıkça ayarla (factory'nin doğru ayarlamadığı durumlara karşı)
            RectTransform rectTransform = nodePresenter.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }

            // Dinamik content genişletme kontrolü
            if (ShouldExpandContentForNode(position))
            {
                ExpandContentForNode(position);
            }

            switch (nodeType)
            {
                case NodeType.Start:
                    StartNode = nodePresenter as StartNodePresenter;
                    _model.AddNode(nodePresenter);
                    return nodePresenter;

                case NodeType.Finish:
                    FinishNode = nodePresenter as FinishNodePresenter;

                    _model.AddNode(nodePresenter);
                    return nodePresenter;

                default:
                    _model.AddNode(nodePresenter);
                    return nodePresenter;
            }
        }


        private BaseNodePresenter CreateNodePresenter(Vector2 position, NodeType nodeType, BaseNode baseNode)
        {
            var go = _nodePresenterFactory.Create(position, nodeType, baseNode);
            _nodePresenters.Add(go);
            return go;
        }

        public void CreateTestNode(NodeType nodeType)
        {
            Vector2 center = Vector2.zero;
            CreateNodeAtPosition(center, nodeType);
        }

        public void CreateStartNode()
        {
            // Start node'u sol tarafa yerleştir (sabit değer kullanarak)
            Vector2 leftPosition = new Vector2(-800f, 0);
            CreateNodeAtPosition(leftPosition, NodeType.Start);
        }

        public void CreateFinishNode()
        {
            // Finish node'u sağ tarafa yerleştir (sabit değer kullanarak)
            Vector2 rightPosition = new Vector2(500f, 0);
            CreateNodeAtPosition(rightPosition, NodeType.Finish);
        }

        public void Clear()
        {
            // VIROO network state'ini temizle (ASIL SORUNUN ÇÖZÜMÜ)
            ClearVIROONetworkState();

            // Bağlantıları temizle
            foreach (var connection in _connectionPresenters.ToList())
            {
                if (connection != null && connection.gameObject != null)
                    Destroy(connection.gameObject);
            }

            _connectionPresenters.Clear();

            // Node'ları temizle
            foreach (var node in _nodePresenters.ToList())
            {
                if (node != null && node.gameObject != null)
                    Destroy(node.gameObject);
            }

            _nodePresenters.Clear();

            // Başlangıç ve bitiş node referanslarını sıfırla
            StartNode = null;
            FinishNode = null;

            // LineRenderer mesh'ini temizle
            if (LineRenderer != null)
            {
                LineRenderer.SetVerticesDirty();
            }
        }

        /// <summary>
        /// VIROO network state'ini temiz hale getirir (connection loading problemi için kritik)
        /// </summary>
        private void ClearVIROONetworkState()
        {
            try
            {
                // ConnectionCreateAction state'ini reset et
                if (_connectionCreateAction != null)
                {
                    // Created connection presenter reference'ını temizle
                    _connectionCreateAction._createdConnectionPresenter = null;
                }

                // Canvas'taki orphan VIROO network objects'leri temizle
                ClearOrphanVIROOConnections();
            }
            catch (System.Exception ex)
            {
                LogManager.LogError($"[ClearVIROONetworkState] Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Canvas'ta kalmış orphan VIROO connection objects'lerini temizler
        /// </summary>
        private void ClearOrphanVIROOConnections()
        {
            try
            {
                if (contentTransform == null) return;

                // Canvas content altındaki tüm connection objects'leri bul
                var allConnections = contentTransform.GetComponentsInChildren<ConnectionPresenter>(true);

                foreach (var connection in allConnections)
                {
                    // Eğer bu connection GraphManager'ın listesinde yoksa orphan'dır
                    if (connection != null && !_connectionPresenters.Contains(connection))
                    {
                        if (connection.gameObject != null)
                        {
                            Destroy(connection.gameObject);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.LogError($"[ClearOrphanVIROOConnections] Hata: {ex.Message}");
            }
        }

        public PortPresenter FindPortPresenter(PortPresenter portRef)
        {
            // Önce node'u bul
            if (_nodePresenters.Contains(portRef.Model.baseNode))
            {
                return portRef;
            }

            Debug.LogWarning($"Node not found for port reference: {portRef}");
            return null;
        }

        internal void UpdateConnectionsLine()
        {
            int updatedCount = 0;
            int errorCount = 0;
            int totalConnections = ConnectionPresenters.Count;
            
            // Sadece critical durumlar log'lanıyor (performans için)
            
            foreach (ConnectionPresenter item in ConnectionPresenters)
            {
                try
                {
                    if (item == null)
                    {
                        LogManager.LogError($"[UpdateConnectionsLine] NULL connection presenter!");
                        errorCount++;
                        continue;
                    }
                    
                    if (item.Model == null)
                    {
                        LogManager.LogError($"[UpdateConnectionsLine] Connection model NULL! Connection: {item.gameObject.name}");
                        errorCount++;
                        continue;
                    }
                    
                    item.UpdateLine();
                    updatedCount++;
                }
                catch (System.Exception ex)
                {
                    LogManager.LogError($"[UpdateConnectionsLine] Line güncelleme hatası - Connection: {item?.gameObject?.name ?? "NULL"}, Hata: {ex.Message}");
                    errorCount++;
                }
            }
            
            // Sadece hata varsa veya critical durumlarda log
            if (errorCount > 0 || totalConnections == 0)
            {
                LogManager.Log($"[UpdateConnectionsLine] 📊 Line rendering özeti: {updatedCount} başarılı, {errorCount} hata, {totalConnections} toplam", 
                    errorCount > 0 ? Color.yellow : Color.green);
            }
                
            // LineRenderer'ı yenile
            if (LineRenderer != null)
            {
                LineRenderer.SetVerticesDirty();
            }
            else
            {
                LogManager.LogError("[UpdateConnectionsLine] ❌ LineRenderer NULL!");
            }
        }

        public void UnselectAllElements()
        {
            if (!_inputManager.Aux0KeyPress)
            {
                for (int i = _systemManager.selectedElements.Count - 1; i >= 0; i--)
                {
                    _systemManager.selectedElements[i].Unselect();
                }
            }
        }

        #region ScenarioFileManager Integration

        private void SubscribeToFileManagerEvents()
        {
            ScenarioFileManager.OnSaveRequested += SaveGraphToFile;
            ScenarioFileManager.OnLoadRequested += (filePath) => LoadGraphFromFile(filePath);
        }

        private void UnsubscribeFromFileManagerEvents()
        {
            ScenarioFileManager.OnSaveRequested -= SaveGraphToFile;
            // OnLoadRequested lambda ile subscribe edildiği için unsubscribe etmek zor
            // ScenarioFileManager.OnLoadRequested -= LoadGraphFromFile;
        }

        /// <summary>
        /// Save Scenario butonuna basıldığında çağrılır - Popup'ı açar
        /// </summary>
        public void ShowSaveDialog()
        {
            if (_scenarioFileManager == null)
            {
                _scenarioFileManager = FindObjectOfType<ScenarioFileManager>();
            }

            if (_scenarioFileManager != null)
            {
                _scenarioFileManager.ShowSavePopup();
            }
            else
            {
                Debug.LogError("ScenarioFileManager bulunamadı! Lütfen sahneye ekleyin.");
            }
        }

        /// <summary>
        /// Load Scenario butonuna basıldığında çağrılır - Popup'ı açar
        /// </summary>
        public void ShowLoadDialog()
        {
            if (_scenarioFileManager == null)
            {
                _scenarioFileManager = FindObjectOfType<ScenarioFileManager>();
            }

            if (_scenarioFileManager != null)
            {
                _scenarioFileManager.ShowLoadPopup();
            }
            else
            {
                Debug.LogError("ScenarioFileManager bulunamadı! Lütfen sahneye ekleyin.");
            }
        }


        /// <summary>
        /// Belirtilen dosya yoluna senaryoyu kaydeder
        /// </summary>
        private void SaveGraphToFile(string filePath)
        {
            try
            {
                SaveFile saveFile = CreateSaveFile();

                XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fs, saveFile);
                }

                Debug.Log($"Senaryo başarıyla kaydedildi: {filePath}");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Senaryo kaydetme hatası: {ex.Message}");
                Debug.LogError($"Senaryo kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirtilen dosya yolundan senaryoyu yükler
        /// </summary>
        private async Task LoadGraphFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    LogManager.LogError($"Dosya bulunamadı: {Path.GetFileName(filePath)}");
                    return;
                }

                SaveFile saveFile;
                XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    saveFile = (SaveFile)serializer.Deserialize(fs);
                }

                await LoadSaveFile(saveFile);

                Debug.Log($"Senaryo başarıyla yüklendi: {filePath}");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Senaryo yükleme hatası: {ex.Message}");
                Debug.LogError($"Senaryo yükleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Yeni senaryo oluşturur - tüm nodeları ve sahnedeki nesneleri temizler
        /// </summary>
        private void CreateNewScenario()
        {
            try
            {
                // Sahneyi temizle (geçici nesneleri sil)
                ResetScene();

                // Tüm node'ları ve connection'ları temizle
                Clear();

                // Start ve Finish node'larını yeniden oluştur
                CreateStartNode();
                CreateFinishNode();

                Debug.Log("New scenario created - all nodes and objects cleared");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"New scenario creation error: {ex.Message}");
                Debug.LogError($"New scenario creation error: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Eski API uyumluluğu için - Yeni sistemde ShowSaveDialog() kullanın
        /// </summary>
        [System.Obsolete("Use ShowSaveDialog() instead for better user experience")]
        public void SaveGraph()
        {
            ShowSaveDialog();
        }

        /// <summary>
        /// SaveFile objesi oluşturur (dahili kullanım için)
        /// </summary>
        private SaveFile CreateSaveFile()
        {
            SaveFile saveFile = new SaveFile
            {
                Nodes = new List<BaseNode>(),
                Objects = new List<ObjectModel>(),
                Connections = new List<ConnectionInfo>(),
            };

            // Node'ları kaydet
            foreach (var nodePresenter in _nodePresenters)
            {
                var model = nodePresenter.Model;
                // Pozisyon bilgilerini güncelle
                var rectTransform = nodePresenter.GetComponent<RectTransform>();
                model.PosX = rectTransform.anchoredPosition.x;
                model.PosY = rectTransform.anchoredPosition.y;

                // Renk bilgilerini güncelle
                model.Color = model.Color; // Setter ile bileşenleri günceller

                // Port bilgilerini güncelle
                model.Ports.Clear();
                foreach (var port in nodePresenter.Ports)
                {
                    // Port bilgilerini güncelle
                    port.Model.NodeID = model.ID;
                    port.Model.PolarityTypeString = port.Polarity.ToString();

                    // Port'u modelin portlarına ekle
                    model.Ports.Add(port.Model);
                }

                // Event Port bilgilerini güncelle
                foreach (var eventPort in nodePresenter.EventPorts)
                {
                    // Event Port bilgilerini güncelle 
                    eventPort.Model.NodeID = model.ID;
                    eventPort.Model.PolarityTypeString = eventPort.Polarity.ToString();

                    // Event Port'u modelin portlarına ekle
                    model.Ports.Add(eventPort.Model);
                }

                // GrabNodePresenter için target ghost pozisyonunu güncelle (save zamanında)
                if (nodePresenter is GrabNodePresenter grabNodePresenter)
                {
                    grabNodePresenter.UpdateTargetPositionForSave();
                }

                saveFile.Nodes.Add(model);
            }

            // Connection'ları kaydet
            foreach (var connectionPresenter in _connectionPresenters)
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(
                    connectionPresenter.Model.ID,
                    connectionPresenter.Model.SourcePort.Model.ID,
                    connectionPresenter.Model.TargetPort.Model.ID
                );
                saveFile.Connections.Add(connectionInfo);
            }

            SaveSceneObjects(saveFile);
            return saveFile;
        }

        private void SaveSceneObjects(SaveFile saveFile)
        {
            // VIROO_PrefabContainer altındaki nesneleri bul
            Transform virooContainer = GameObject.Find("VIROO_PrefabContainer")?.transform;

            if (virooContainer == null)
            {
                Debug.LogWarning("VIROO_PrefabContainer bulunamadı!");
                return;
            }

            foreach (Transform child in virooContainer)
            {
                // ObjectPresenter componenti olan nesneleri kontrol et
                var objectPresenter = child.GetComponent<ObjectPresenter>();
                if (objectPresenter == null)
                {
                    Debug.LogWarning($"ObjectPresenter componenti bulunamadı: {child.name}");
                    continue;
                }

                // Model bilgilerini güncelle
                objectPresenter.TransformToModel();

                var model = objectPresenter.Model;

                if (model == null)
                {
                    Debug.LogWarning($"ObjectModel null: {child.name}");
                    continue;
                }

                // ObjectType artık ObjectPresenter'da serialized field ile ayarlanıyor
                if (model.ObjectType == ObjectType.Unknown)
                {
                    Debug.LogWarning($"ObjectType Unknown: {child.name} - Prefab'da ObjectType ayarlanmamış olabilir!");
                }

                // Model'i kaydet listesine ekle
                saveFile.Objects.Add(model);

                Debug.Log($"VIROO nesnesi kaydedildi: {model.Name}, ID: {model.ID}, Tip: {model.ObjectType}");
            }

            Debug.Log($"Toplam {saveFile.Objects.Count} VIROO nesnesi kaydedildi.");
        }

        // DetermineObjectType metodu artık kullanılmıyor
        // ObjectType'lar artık ObjectPresenter'da serialized field ile ayarlanıyor

        /// <summary>
        /// Eski API uyumluluğu için - Yeni sistemde ShowLoadDialog() kullanın
        /// </summary>
        [System.Obsolete("Use ShowLoadDialog() instead for better user experience")]
        public void LoadGraph()
        {
            ShowLoadDialog();
        }

        /// <summary>
        /// SaveFile'dan sahneyi yükler (dahili kullanım için)
        /// </summary>
        private async Task LoadSaveFile(SaveFile saveFile)
        {
            // Sahneyi temizle (leakage önleme)
            ResetScene();

            // Tüm mevcut node'ları ve bağlantıları temizle
            Clear();

            // Node'ları asenkron olarak oluştur ve initialize edilmelerini bekle
            var nodeCreationTasks = new List<Task<BaseNodePresenter>>();
            
            foreach (var nodeModel in saveFile.Nodes)
            {
                var task = CreateAndInitializeNodeAsync(nodeModel);
                nodeCreationTasks.Add(task);
            }

            // Tüm node'ların oluşturulması ve initialize edilmesi tamamlanana kadar bekle
            var createdNodes = await Task.WhenAll(nodeCreationTasks);

            // VIROO Action sisteminin hazır olmasını bekle (connection'lar için kritik)
            await WaitForVIROOActionToBeReady();

            // Bağlantıları oluştur
            int successfulConnections = 0;
            int failedConnections = 0;
            
            foreach (var connInfo in saveFile.Connections)
            {
                var sourcePort = FindPortPresenterByID(connInfo.SourcePortID);
                var targetPort = FindPortPresenterByID(connInfo.TargetPortID);
                
                if (sourcePort != null && targetPort != null)
                {
                    var connection = CreateConnection(sourcePort, targetPort);
                    
                    if (connection != null)
                    {
                        successfulConnections++;
                    }
                    else
                    {
                        LogManager.LogError($"[LoadSaveFile] CreateConnection NULL döndü!");
                        failedConnections++;
                    }
                    
                    // VIROO Action sonucunu kontrol et
                    if (_connectionCreateAction != null && _connectionCreateAction._createdConnectionPresenter != null)
                    {
                        // Success
                    }
                    else
                    {
                        LogManager.LogWarning($"[LoadSaveFile] VIROO Action connection presenter NULL! Action: {_connectionCreateAction != null}, Presenter: {_connectionCreateAction?._createdConnectionPresenter != null}");
                    }
                }
                else
                {
                    LogManager.LogError($"[LoadSaveFile] Port bulunamadı!");
                    LogManager.LogError($"  - SourcePort ({connInfo.SourcePortID}): {sourcePort?.gameObject.name ?? "NULL"}");
                    LogManager.LogError($"  - TargetPort ({connInfo.TargetPortID}): {targetPort?.gameObject.name ?? "NULL"}");
                    
                    // Mevcut tüm port ID'lerini debug için listele
                    LogManager.LogError($"[LoadSaveFile] Mevcut port ID'leri listeleniyor...");
                    foreach (var node in NodePresenters)
                    {
                        foreach (var port in node.Ports)
                        {
                            LogManager.LogError($"  Normal Port - Node: {node.gameObject.name}, Port: {port.gameObject.name}, ID: {port.Model.ID}");
                        }
                        foreach (var eventPort in node.EventPorts)
                        {
                            LogManager.LogError($"  Event Port - Node: {node.gameObject.name}, Port: {eventPort.gameObject.name}, ID: {eventPort.Model.ID}");
                        }
                    }
                    
                    failedConnections++;
                }
            }

            // Sadece hata varsa connection sonuçlarını raporla
            if (failedConnections > 0)
            {
                LogManager.LogWarning($"[LoadSaveFile] Connection özeti: {successfulConnections} başarılı, {failedConnections} başarısız");
            }

            LoadSceneObjects(saveFile);
            UpdateConnectionsLine();
        }

        private NodeType DetermineNodeType(string nodeTypeName)
        {
            switch (nodeTypeName)
            {
                case "StartNode": return NodeType.Start;
                case "FinishNode": return NodeType.Finish;
                case "TouchNode": return NodeType.TouchNode;
                case "GrabNode": return NodeType.GrabNode;
                case "WaitForNextNode": return NodeType.WaitForNextNode;
                case "GetKeyDownNode": return NodeType.LookNode;
                case "LookNode": return NodeType.LookNode; // LookNode case'ini ekledim
                case "LogicNode": return NodeType.LogicalOR; // LogicNode için Type property'sine bakmak gerekecek
                case "ActionNode": return NodeType.ChangeMaterialAction; // Generic ActionNode için varsayılan

                // Özel action node sınıfları
                case "AudioActionNode": return NodeType.PlaySoundAction;
                case "VFXActionNode": return NodeType.VFXActionNode;
                case "HighlightObjectActionNode": return NodeType.HighlightObjectActionNode;
                case "ChangeMaterialActionNode": return NodeType.ChangeMaterialAction;
                case "ChangePositionActionNode": return NodeType.ChangePositionAction;
                case "ChangeRotationActionNode": return NodeType.ChangeRotationAction;
                case "ChangeScaleActionNode": return NodeType.ChangeScaleAction;
                case "ToggleObjectActionNode": return NodeType.ToggleObjectAction;
                case "PlayAnimationActionNode": return NodeType.PlayAnimationAction;
                case "RobotAnimationActionNode": return NodeType.RobotAnimationAction;
                case "DescriptionActionNode": return NodeType.DescriptionActionNode;
                case "WorldDescriptionActionNode": return NodeType.WorldDescriptionActionNode;
                case "ToolTouchNode": return NodeType.ToolTouchNode;

                // Eski isimler (geriye dönük uyumluluk)
                case "PlaySoundAction": return NodeType.PlaySoundAction;
                case "MoveObjectAction": return NodeType.ChangePositionAction;

                default: throw new ArgumentException($"Bilinmeyen node tipi: {nodeTypeName}");
            }
        }

        public PortPresenter FindPortPresenterByID(string portID)
        {
            int nodeIndex = 0;
            foreach (var node in NodePresenters)
            {
                nodeIndex++;
                
                // Normal portları kontrol et
                int portIndex = 0;
                foreach (var port in node.Ports)
                {
                    portIndex++;
                    
                    if (port.Model.ID == portID)
                    {
                        return port;
                    }
                }

                // Event portlarını kontrol et
                int eventPortIndex = 0;
                foreach (var eventPort in node.EventPorts)
                {
                    eventPortIndex++;
                    
                    if (eventPort.Model.ID == portID)
                    {
                        return eventPort;
                    }
                }
            }

            LogManager.LogError($"[FindPortPresenterByID] Port bulunamadı: {portID} - Tüm {NodePresenters.Count} node kontrol edildi");
            return null;
        }

        public void ScaleUpGraph()
        {
            Debug.Log("Scale Up - Mevcut değer: " + scaleValue);

            // %50 artış yap
            float newScale = scaleValue * 1.5f;

            // Maksimum sınırı kontrol et
            if (newScale > maxScale)
            {
                newScale = maxScale;
                Debug.Log("Maksimum scale değerine ulaşıldı: " + maxScale);
                return;
            }

            // Yeni scale değerini uygula
            Vector3 newScaleVector = Vector3.one * newScale;
            contentTransform.localScale = newScaleVector;

            // Grid görünümünü ters orantılı olarak ayarla
            float gridMultiplier = 1f / newScale;
            if (gridMultiplier >= 0.1f && gridMultiplier <= 10f)
                gridImage.pixelsPerUnitMultiplier = gridMultiplier;

            scaleValue = newScale;
            scaleInput.text = scaleValue.ToString("F2");
            scaleSlider.value = (scaleValue - minScale) / (maxScale - minScale);

            Debug.Log("Yeni scale değeri: " + scaleValue);
        }

        public void ScaleDownGraph()
        {
            Debug.Log("Scale Down - Mevcut değer: " + scaleValue);

            // %33 azalış yap (1/1.5 = 0.67 yaklaşık)
            float newScale = scaleValue * 0.67f;

            // Minimum sınırı kontrol et - sıfırın altına düşmemeli
            if (newScale < 0.1f) // 0.1f minimum güvenli değer
            {
                newScale = 0.1f;
                Debug.Log("Minimum scale değerine ulaşıldı: " + newScale);
            }

            // MinScale kontrolü de yap
            if (newScale < minScale)
            {
                newScale = minScale;
                Debug.Log("MinScale sınırına ulaşıldı: " + minScale);
            }

            // Yeni scale değerini uygula
            Vector3 newScaleVector = Vector3.one * newScale;
            contentTransform.localScale = newScaleVector;

            // Grid görünümünü ters orantılı olarak ayarla
            float gridMultiplier = 1f / newScale;
            if (gridMultiplier >= 0.1f && gridMultiplier <= 10f)
                gridImage.pixelsPerUnitMultiplier = gridMultiplier;

            scaleValue = newScale;
            scaleInput.text = scaleValue.ToString("F2");
            scaleSlider.value = (scaleValue - minScale) / (maxScale - minScale);

            Debug.Log("Yeni scale değeri: " + scaleValue);
        }

        private void LoadSceneObjects(SaveFile saveFile)
        {
            if (saveFile.Objects == null || saveFile.Objects.Count == 0)
            {
                Debug.Log("Yüklenecek VIROO nesnesi yok.");
                return;
            }

            // VIROO_PrefabContainer'ı bul
            Transform virooContainer = GameObject.Find("VIROO_PrefabContainer")?.transform;

            if (virooContainer == null)
            {
                Debug.LogWarning("VIROO_PrefabContainer bulunamadı!");
                return;
            }

            // Mevcut VIROO nesnelerini temizle
            // ClearVIROOObjects();

            Debug.Log($"VIROO buton tetikleme sistemi ile {saveFile.Objects.Count} nesne yükleniyor...");

            // Her model için ilgili butonu tetikle
            foreach (var model in saveFile.Objects)
            {
                TriggerObjectButtonByType(model.ObjectType);
            }

            // Coroutine başlat - oluşturulan nesneleri bekle ve model uygula
            StartCoroutine(WaitAndApplyModels(saveFile.Objects));
        }

        private void TriggerObjectButtonByType(ObjectType objectType)
        {
            // ObjectType'a göre buton ismini belirle
            string buttonName = GetButtonNameByObjectType(objectType);

            if (string.IsNullOrEmpty(buttonName))
            {
                Debug.LogWarning($"Buton adı bulunamadı: {objectType}");
                return;
            }

            // Canvas hierarchy'sinde butonu bul: CanvasObjects > ObjectCanvas > Objects > Scroll View > Viewport > Content > GridHolder
            var canvasObjects = GameObject.Find("CanvasObjects");
            if (canvasObjects == null)
            {
                Debug.LogWarning("CanvasObjects bulunamadı!");
                return;
            }

            var objectCanvas = canvasObjects.transform.Find("ObjectCanvas");
            if (objectCanvas == null)
            {
                Debug.LogWarning("ObjectCanvas bulunamadı!");
                return;
            }

            // Butonu GridHolder altında ara
            var gridHolder = objectCanvas.Find("Objects/Scroll View/Viewport/Content/GridHolder");
            if (gridHolder == null)
            {
                Debug.LogWarning("GridHolder bulunamadı!");
                return;
            }

            // Butonu bul ve tetikle
            var button = FindButtonInHierarchy(gridHolder, buttonName);
            if (button != null)
            {
                Debug.Log($"Buton tetikleniyor: {buttonName} -> {objectType}");
                button.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning($"Buton bulunamadı: {buttonName}");
            }
        }

        private string GetButtonNameByObjectType(ObjectType objectType)
        {
            // ObjectType enum değerini gerçek buton isimlerine çevir
            // Bu isimler Unity hierarchy'sindeki GridHolder altındaki buton isimlerine karşılık gelir
            switch (objectType)
            {
                case ObjectType.Robot: return "robot";
                case ObjectType.Barrier: return "barrier";
                case ObjectType.BrownDesk: return "brown_desk";
                case ObjectType.EmergencyButton: return "button";
                case ObjectType.Chair: return "chair";
                case ObjectType.Chassis: return "chassis";
                case ObjectType.Glasses: return "glasses";
                case ObjectType.Gloves: return "gloves";
                case ObjectType.Helmet: return "helmet";
                case ObjectType.Kabinet: return "kabinet";
                case ObjectType.Kawasaki: return "kawasakai-rc005L";
                case ObjectType.WhiteDesk: return "white-desk";
                case ObjectType.NightStand: return "nightstand";
                case ObjectType.YellowLine: return "yellow-line";
                case ObjectType.Capsule: return "capsule";
                case ObjectType.Cube: return "cube";
                case ObjectType.Cylinder: return "cylinder";
                case ObjectType.Sphere: return "sphere";
                case ObjectType.AllenWrench: return "allen-wrench";
                case ObjectType.Multimeter: return "multimeter";
                case ObjectType.Nipers: return "nipers";
                case ObjectType.Pincers: return "pincers";
                case ObjectType.Screwdriver: return "screwdriver";
                case ObjectType.Wrench: return "wrench";
                case ObjectType.ControlBox: return "controlbox";
                case ObjectType.ComputerFan: return "computerfan";
                case ObjectType.ur10seperated: return "ur10seperated";
                
                // Tire kaldırıldı - UI'daki gerçek adı
                default:
                    Debug.LogWarning($"Desteklenmeyen ObjectType: {objectType}");
                    return null;
            }
        }

        private UnityEngine.UI.Button FindButtonInHierarchy(Transform parent, string buttonName)
        {
            // Kendisini kontrol et
            if (parent.name == buttonName)
            {
                var button = parent.GetComponent<UnityEngine.UI.Button>();
                if (button != null) return button;
            }

            // Alt nesnelerde ara
            foreach (Transform child in parent)
            {
                var result = FindButtonInHierarchy(child, buttonName);
                if (result != null) return result;
            }

            return null;
        }

        private System.Collections.IEnumerator WaitAndApplyModels(List<ObjectModel> models)
        {
            // 2 saniye bekle - kullanıcının nesneleri oluşturması için
            yield return new WaitForSeconds(2f);

            Transform virooContainer = GameObject.Find("VIROO_PrefabContainer")?.transform;
            if (virooContainer == null) yield break;

            // Modelleri tipe göre grupla
            var modelsByType = models.GroupBy(m => m.ObjectType).ToDictionary(g => g.Key, g => g.ToList());

            foreach (Transform child in virooContainer)
            {
                var presenter = child.GetComponent<ObjectPresenter>();
                if (presenter == null) continue;

                // Bu nesnenin tipini presenter'dan al (artık prefab'da ayarlı)
                ObjectType childType = presenter.Model.ObjectType;

                // Bu tipte bekleyen model var mı?
                if (modelsByType.ContainsKey(childType) && modelsByType[childType].Count > 0)
                {
                    // İlk modeli al ve uygula
                    var model = modelsByType[childType][0];
                    modelsByType[childType].RemoveAt(0);

                    // XML'den okunan ID'yi koru (TouchNode için gerekli)
                    string xmlID = model.ID;

                    // Model'i presenter'a ata
                    presenter.Model = model;

                    // XML'den okunan ID'yi tekrar ata (NetworkObject ID'sini override et)
                    presenter.Model.ID = xmlID;

                    presenter.ModelToTransform();

                    // İsmi güncelle
                    child.gameObject.name = model.Name;

                    Debug.Log($"VIROO nesnesi geri yüklendi: {model.Name}, XML ID: {xmlID}, Tip: {model.ObjectType}");
                }
            }

            Debug.Log("VIROO nesnelerinin model bilgileri uygulandı!");

            // VIROO nesneleri oluştuktan sonra tüm node'ların UI'larını tekrar senkronize et
            foreach (var nodePresenter in _nodePresenters)
            {
                SyncPresenterModelToUI(nodePresenter);
            }

            Debug.Log("Node UI'ları VIROO nesneleri ile senkronize edildi!");
        }

        public void CreateCube()
        {
            // Geçici bir GameObject oluşturup sahneye koymadan transformunu kullan
            var dummyGO = new GameObject("TempCubeSpawn");
            dummyGO.transform.position = new Vector3(1, 1, 1);
            dummyGO.transform.rotation = Quaternion.identity;

            CreateObjectManually("cube", dummyGO.transform);

            // Eğer sahnede bırakmak istemiyorsan hemen sil
            Destroy(dummyGO);
        }


        public void CreateObjectManually(string prefabId, Transform spawnTransform)
        {
            // Bu metod artık sadece buton tetikleme için kullanılabilir
            // Direkt VIROO sistemini kullanmak için CreateCube gibi özel metodlar kullanın

            Debug.LogWarning(
                "CreateObjectManually: Network servisleri mevcut değil. Alternatif olarak CreateCube() gibi metodları kullanın.");
        }

        /// <summary>
        /// Model verilerini presenter'a kopyalar (load işlemi sırasında)
        /// </summary>
        private void CopyModelDataToPresenter(BaseNode model, BaseNodePresenter presenter)
        {
            try
            {
                // Presenter'ın model'ini değiştir
                presenter.Model = model;
                Debug.Log($"Model data kopyalandı: {presenter.GetType().Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Model data kopyalanırken hata oluştu {presenter.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Presenter'ın SyncModelToUI metodunu reflection ile çağırır
        /// </summary>
        private void SyncPresenterModelToUI(BaseNodePresenter presenter)
        {
            try
            {
                // Reflection ile SyncModelToUI metodunu bul ve çağır
                var syncMethod = presenter.GetType().GetMethod("SyncModelToUI");
                if (syncMethod != null)
                {
                    syncMethod.Invoke(presenter, null);
                    Debug.Log($"SyncModelToUI çağrıldı: {presenter.GetType().Name}");
                }
                else
                {
                    Debug.Log($"SyncModelToUI metodu bulunamadı: {presenter.GetType().Name}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SyncModelToUI çağrılırken hata oluştu {presenter.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Sahnedeki geçici nesneleri temizler (Load öncesi leakage önleme)
        /// </summary>
        private void ResetScene()
        {
            // WorldNotifierCanvas'ları temizle
            ClearWorldNotifierCanvases();

            // VIROO nesnelerini temizle
            ClearVIROOObjects();

            // Gelecekte başka geçici nesneler buraya eklenebilir:
            // ClearTemporaryEffects();
            // ClearDynamicAudio();
            // vs.
        }

        /// <summary>
        /// VIROO_PrefabContainer altındaki ObjectPresenter'lı nesneleri temizler
        /// </summary>
        private void ClearVIROOObjects()
        {
            // VIROO_PrefabContainer'ı bul
            Transform virooContainer = GameObject.Find("VIROO_PrefabContainer")?.transform;

            if (virooContainer == null)
            {
                Debug.LogWarning("VIROO_PrefabContainer bulunamadı! ObjectPresenter nesneleri temizlenemedi.");
                return;
            }

            // Mevcut dinamik nesneleri temizle (VIROO_PrefabContainer altındaki)
            List<Transform> toDestroy = new List<Transform>();
            foreach (Transform child in virooContainer)
            {
                // ObjectPresenter componenti olan nesneleri işaretle
                if (child.GetComponent<ObjectPresenter>() != null)
                {
                    toDestroy.Add(child);
                }
            }

            foreach (var child in toDestroy)
            {
                if (child != null && child.gameObject != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            if (toDestroy.Count > 0)
            {
                Debug.Log($"✅ VIROO nesneleri temizlendi: {toDestroy.Count} nesne silindi");
            }
            else
            {
                Debug.Log("VIROO_PrefabContainer'da silinecek ObjectPresenter nesnesi bulunamadı");
            }
        }

        /// <summary>
        /// Sahnedeki tüm WorldNotifierCanvas clone'larını temizler
        /// </summary>
        private void ClearWorldNotifierCanvases()
        {
            // "WorldNotifierCanvas(Clone)" isimli nesneleri bul ve temizle
            var worldCanvases = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.Contains("WorldNotifierCanvas") && go.name.Contains("Clone"))
                .ToArray();

            foreach (var canvas in worldCanvases)
            {
                if (canvas != null)
                {
                    DestroyImmediate(canvas);
                }
            }
        }

        /// <summary>
        /// Asenkron olarak node oluşturur ve port'ların initialize edilmesini bekler
        /// </summary>
        private async Task<BaseNodePresenter> CreateAndInitializeNodeAsync(BaseNode nodeModel)
        {
            try
            {
                NodeType nodeType = DetermineNodeType(nodeModel.GetType().Name);
                Vector2 position = new Vector2(nodeModel.PosX, nodeModel.PosY);
                
                // Node'u oluştur
                BaseNodePresenter nodePresenter = CreateNodeAtPosition(position, nodeType);

                // Node özelliklerini ayarla
                nodePresenter.ID = nodeModel.ID;
                nodePresenter.Model.ID = nodeModel.ID;
                nodePresenter.Model.Title = nodeModel.Title;
                nodePresenter.Model.Description = nodeModel.Description;
                nodePresenter.Model.Color =
                    new Color(nodeModel.ColorR, nodeModel.ColorG, nodeModel.ColorB, nodeModel.ColorA);

                // Node pozisyonunu ayarla (önemli)
                RectTransform rectTransform = nodePresenter.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = position;
                }

                // Model verilerini presenter'a kopyala
                CopyModelDataToPresenter(nodeModel, nodePresenter);

                // Port'ların initialize olmasını bekle (VIROO network'te async olan kısım)
                await WaitForPortsToInitialize(nodePresenter);

                // Portları ayarla - ID'ye göre eşleştir
                if (nodeModel.Ports != null && nodeModel.Ports.Count > 0)
                {
                    foreach (var portModel in nodeModel.Ports)
                    {
                        // Önce normal portlarda ara
                        var portPresenter = nodePresenter.Ports.FirstOrDefault(
                            p => p.Model.Name == portModel.Name &&
                                 p.Polarity.ToString() == portModel.PolarityTypeString);

                        if (portPresenter != null)
                        {
                            // Port ID'yi ayarla - bu kritik önemde!
                            string oldId = portPresenter.Model.ID;
                            portPresenter.Model.ID = portModel.ID;
                        }
                        else
                        {
                            // Event port olabilir, event portlarda ara
                            var eventPortModel = portModel as Models.EventPort;
                            if (eventPortModel != null)
                            {
                                // EventType'a göre eşleştirme yaparak ara
                                var eventPortPresenter = nodePresenter.EventPorts.FirstOrDefault(
                                    p => p.EventType.ToString() == eventPortModel.EventType.ToString());

                                if (eventPortPresenter != null)
                                {
                                    // Event Port ID'yi ayarla
                                    string oldId = eventPortPresenter.Model.ID;
                                    eventPortPresenter.Model.ID = portModel.ID;
                                    Debug.Log(
                                        $"EventPort eşleştirildi: {portModel.Name}, EventType: {eventPortModel.EventType}");
                                }
                                else
                                {
                                    LogManager.LogError($"[CreateAndInitializeNodeAsync] EventPort bulunamadı! EventType: {eventPortModel.EventType}, Name: {portModel.Name}");
                                    Debug.LogWarning($"EventPort bulunamadı! EventType: {eventPortModel.EventType}");
                                }
                            }
                            else
                            {
                                LogManager.LogError($"[CreateAndInitializeNodeAsync] Normal Port bulunamadı - Name: {portModel.Name}, Polarity: {portModel.PolarityTypeString}");
                                Debug.LogWarning($"Port bulunamadı: {portModel.Name}");
                            }
                        }
                    }
                }

                // Model verilerini UI'ya senkronize et (eğer presenter bu metodları destekliyorsa)
                SyncPresenterModelToUI(nodePresenter);

                // Node başarıyla oluşturuldu
                return nodePresenter;
            }
            catch (Exception e)
            {
                LogManager.LogError($"[CreateAndInitializeNodeAsync] Node oluşturma hatası: {e.Message}");
                LogManager.LogError($"[CreateAndInitializeNodeAsync] StackTrace: {e.StackTrace}");
                throw; // Exception'ı yeniden fırlat ki Task.WhenAll hatayı yakalasın
            }
        }

        /// <summary>
        /// Node'un portlarının initialize olmasını bekler (VIROO network için kritik)
        /// </summary>
        private async Task WaitForPortsToInitialize(BaseNodePresenter nodePresenter)
        {
            const int maxRetries = 50; // 5 saniye (100ms * 50)
            const int delayMs = 100;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                // Port'lar initialize oldu mu kontrol et
                bool allPortsReady = true;

                // Normal portları kontrol et
                foreach (var port in nodePresenter.Ports)
                {
                    if (port.Model == null)
                    {
                        allPortsReady = false;
                        break;
                    }
                }

                // Event portları kontrol et
                if (allPortsReady)
                {
                    foreach (var eventPort in nodePresenter.EventPorts)
                    {
                        if (eventPort.Model == null)
                        {
                            allPortsReady = false;
                            break;
                        }
                    }
                }

                if (allPortsReady)
                {
                    return; // Port'lar hazır, başarılı!
                }

                // Kısa bir süre bekle
                await Task.Delay(delayMs);
            }

            // Timeout oldu, warning ver ama devam et
            LogManager.LogWarning($"[WaitForPortsToInitialize] ⚠️ Port initialization timeout ({maxRetries * delayMs}ms): {nodePresenter.gameObject.name}");
        }

        /// <summary>
        /// VIROO Action sisteminin hazır olmasını bekler (connection creation için kritik)
        /// </summary>
        private async Task WaitForVIROOActionToBeReady()
        {
            const int maxRetries = 30; // 3 saniye (100ms * 30)
            const int delayMs = 100;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                // VIROO Action hazır mı kontrol et
                if (_connectionCreateAction != null)
                {
                    return; // VIROO Action hazır, başarılı!
                }

                // Kısa bir süre bekle
                await Task.Delay(delayMs);
            }

            // Timeout oldu, warning ver ama devam et
            LogManager.LogWarning($"[WaitForVIROOActionToBeReady] VIROO Action timeout ({maxRetries * delayMs}ms) - Factory fallback kullanılacak");
        }
    }
}