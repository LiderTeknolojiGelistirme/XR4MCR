using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
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


namespace Managers
{
    [DefaultExecutionOrder(-10)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class GraphManager : MonoBehaviour
    {
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

        private string _filePath;

        [SerializeField] private float _connectionDetectionDistance = 10f;
        public float ConnectionDetectionDistance => _connectionDetectionDistance;

        #region Connection Management

        private ConnectionPresenterFactory _connectionPresenterFactory;

        // Connection'lar için dictionary (ID -> Presenter)
        private List<ConnectionPresenter> _connectionPresenters = new();
        public List<ConnectionPresenter> ConnectionPresenters => _connectionPresenters;

        public ConnectionPresenter CreateConnection(PortPresenter sourcePort, PortPresenter targetPort)
        {
            var connectionPresenter = _connectionPresenterFactory.CreateConnection(sourcePort, targetPort);
            if (connectionPresenter != null)
            {
                _connectionPresenters.Add(connectionPresenter);
                
                // Bilgi paneline bağlantı oluşturuldu logu ekle
                LogManager.LogInteraction($"Connection created: {sourcePort.gameObject.name} -> {targetPort.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("Connection creation failed - Factory returned null");
                
                // Bilgi paneline bağlantı başarısız logu ekle
                LogManager.LogWarning($"Connection failed: {sourcePort.gameObject.name} -> {targetPort.gameObject.name}");
            }

            return connectionPresenter;
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

        // Connection modellerine erişmek için extension
        public IEnumerable<Connection> ConnectionModels => _connectionPresenters.Select(p => p.Model);

        

        public IEnumerable<Connection> GetPortConnections(PortPresenter portRef)
        {
            return ConnectionModels.Where(c =>
                c.SourcePort.Equals(portRef) || c.TargetPort.Equals(portRef));
        }

        #endregion

        private void OnEnable()
        {
            Debug.Log($"GraphManager OnEnable: IsInitialized={_isInitialized}");
            _filePath = Path.Combine(Application.persistentDataPath, "MyNodeGraph.xml");
            LineRenderer?.OnPopulateMeshAddListener(DrawConnections);
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
            Debug.Log("GraphManager Awake");
            // Sadece temel setup
            InitializeCanvas();
            
        }



        [Inject]
        public void Construct(NodeConfig config, SystemManager systemManager,
            ConnectionPresenterFactory connectionPresenterFactory, NodePresenterFactory nodePresenterFactory,
            XRInputManager inputManager,
            Pointer pointer, LTGLineRenderer lineRenderer, ObjectFactory objectFactory)
        {
            Debug.Log("ENTER: GraphManager Construct");
            // Eğer zaten construct edilmişse çık
            if (_isInitialized)
            {
                Debug.LogWarning("GraphManager already initialized!");
                return;
            }

            Debug.Log(
                $"GraphManager Construct called with: config={config != null}, systemManager={systemManager != null}");

            _config = config;
            _systemManager = systemManager;
            _connectionPresenterFactory = connectionPresenterFactory;
            _nodePresenterFactory = nodePresenterFactory;
            _inputManager = inputManager;
            _pointer = pointer.gameObject;
            _lineRenderer = lineRenderer;
            _objectFactory = objectFactory;
            
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

        private void Update()
        {
        }

        private void InitializePointer()
        {
            Debug.Log("Initializing Pointer...");

            // if (_pointer != null)
            // {
            //     Debug.LogWarning("Pointer already exists!");
            //     return;
            // }

            // var pointerGO = new GameObject("Pointer");
            // pointerGO.transform.SetParent(transform);
            // var pointerComponent = pointerGO.AddComponent<Pointer>();
            // _pointer = pointerGO;
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


        private void OnValidate()
        {
            InitializeCanvas();
        }

        public BaseNodePresenter CreateNodeAtPosition(Vector2 position, NodeType nodeType)
        {
            var nodePresenter = CreateNodePresenter(position, nodeType);
            
            // Pozisyonu açıkça ayarla (factory'nin doğru ayarlamadığı durumlara karşı)
            RectTransform rectTransform = nodePresenter.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
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

        private BaseNodePresenter CreateNodePresenter(Vector2 position, NodeType nodeType)
        {
            var go = _nodePresenterFactory.Create(position, nodeType);
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
            foreach (ConnectionPresenter item in ConnectionPresenters)
            {
                item.UpdateLine();
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
        public void SaveGraph()
        {
            SaveFile saveFile = new SaveFile
            {
                Nodes = new List<BaseNode>(),
                Connections = new List<ConnectionInfo>(),
                SceneObjects = new List<SceneObjectInfo>()
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

            XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));
            using (FileStream fs = new FileStream(_filePath, FileMode.Create))
            {
                serializer.Serialize(fs, saveFile);
            }
            Debug.Log("Kaydedilen dosya yolu: " + _filePath);
        }
        
        public void LoadGraph()
        {
            if (!File.Exists(_filePath))
                return;

            SaveFile saveFile;
            XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));
            using (FileStream fs = new FileStream(_filePath, FileMode.Open))
            {
                saveFile = (SaveFile)serializer.Deserialize(fs);
            }

            // Tüm mevcut node'ları ve bağlantıları temizle
            Clear();

            // Node'ları tekrar oluştur
            foreach (var nodeModel in saveFile.Nodes)
            {
                NodeType nodeType = DetermineNodeType(nodeModel.GetType().Name);
                Vector2 position = new Vector2(nodeModel.PosX, nodeModel.PosY);
                BaseNodePresenter nodePresenter = CreateNodeAtPosition(position, nodeType);
                
                // Node özelliklerini ayarla
                nodePresenter.ID = nodeModel.ID;
                nodePresenter.Model.ID = nodeModel.ID;
                nodePresenter.Model.Title = nodeModel.Title;
                nodePresenter.Model.Description = nodeModel.Description;
                nodePresenter.Model.Color = new Color(nodeModel.ColorR, nodeModel.ColorG, nodeModel.ColorB, nodeModel.ColorA);
                
                // Node pozisyonunu ayarla (önemli)
                RectTransform rectTransform = nodePresenter.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = position;
                }

                // Portları ayarla - ID'ye göre eşleştir
                if (nodeModel.Ports != null && nodeModel.Ports.Count > 0)
                {
                    foreach(var portModel in nodeModel.Ports)
                    {
                        // Önce normal portlarda ara
                        var portPresenter = nodePresenter.Ports.FirstOrDefault(
                            p => p.Model.Name == portModel.Name && 
                            p.Polarity.ToString() == portModel.PolarityTypeString);
                        
                        if (portPresenter != null)
                        {
                            // Port ID'yi ayarla - bu kritik önemde!
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
                                    eventPortPresenter.Model.ID = portModel.ID;
                                    Debug.Log($"EventPort eşleştirildi: {portModel.Name}, EventType: {eventPortModel.EventType}");
                                }
                                else
                                {
                                    Debug.LogWarning($"EventPort bulunamadı! EventType: {eventPortModel.EventType}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Port bulunamadı: {portModel.Name}");
                            }
                        }
                    }
                }
            }

            // Bağlantıları oluştur
            foreach (var connInfo in saveFile.Connections)
            {
                var sourcePort = FindPortPresenterByID(connInfo.SourcePortID);
                var targetPort = FindPortPresenterByID(connInfo.TargetPortID);
                if(sourcePort != null && targetPort != null)
                    CreateConnection(sourcePort, targetPort);
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
                case "ActionNode": 
                    // ActionNode.Type özelliğine bakarak gerçek NodeType'ı belirlemek gerekecek
                    // Varsayılan olarak ChangeMaterialAction döndürelim
                    return NodeType.ChangeMaterialAction;
                case "PlaySoundAction": return NodeType.PlaySoundAction;
                case "ChangeMaterialAction": return NodeType.ChangeMaterialAction;
                case "MoveObjectAction": return NodeType.ChangePositionAction;
                case "ToggleObjectAction": return NodeType.ToggleObjectAction;
                case "PlayAnimationAction": return NodeType.PlayAnimationAction;
                default: throw new ArgumentException($"Bilinmeyen node tipi: {nodeTypeName}");
            }
        }
        
        public PortPresenter FindPortPresenterByID(string portID)
        {
            foreach(var node in NodePresenters)
            {
                // Normal portları kontrol et
                var port = node.Ports.FirstOrDefault(p => p.Model.ID == portID);
                if(port != null) return port;
                
                // Event portlarını kontrol et
                var eventPort = node.EventPorts.FirstOrDefault(p => p.Model.ID == portID);
                if(eventPort != null) return eventPort;
            }
            return null;
        }

        public void ScaleUpGraph()
        {
            Debug.Log("up");
            foreach (BaseNodePresenter nodePresenter in _nodePresenters)
            {
                contentTransform.localScale += Vector3.one * .1f;
            }
            gridImage.pixelsPerUnitMultiplier -= .1f;


            
        }
        public void ScaleDownGraph()
        {
            Debug.Log("Down");
            foreach (BaseNodePresenter nodePresenter in _nodePresenters)
            {
                contentTransform.localScale -= Vector3.one * .1f;
            }
            gridImage.pixelsPerUnitMultiplier += .1f;

            
        }

       

        private void SaveSceneObjects(SaveFile saveFile)
        {
            Transform scenarioArea = GameObject.Find("ScenarioArea")?.transform;
            
            if (scenarioArea == null)
            {
                Debug.LogWarning("ScenarioArea bulunamadı!");
                return;
            }
            
            foreach (Transform child in scenarioArea)
            {
                // Cube ve ObjectSpawnPosition nesnelerini hariç tut
                if (child.name.StartsWith("Cube") || child.name == "ObjectSpawnPosition")
                    continue;
                
                // Nesnenin tipini belirle
                ObjectType objectType = DetermineObjectType(child.gameObject);
                
                SceneObjectInfo objInfo = new SceneObjectInfo
                {
                    ID = System.Guid.NewGuid().ToString(),
                    Name = child.name,
                    ObjectType = objectType,
                    
                    // Transform bilgileri
                    PosX = child.position.x,
                    PosY = child.position.y,
                    PosZ = child.position.z,
                    
                    RotX = child.rotation.eulerAngles.x,
                    RotY = child.rotation.eulerAngles.y,
                    RotZ = child.rotation.eulerAngles.z,
                    
                    ScaleX = child.localScale.x,
                    ScaleY = child.localScale.y,
                    ScaleZ = child.localScale.z
                };
                
                saveFile.SceneObjects.Add(objInfo);
            }
        }
        
        private ObjectType DetermineObjectType(GameObject obj)
        {
            // Nesne adını küçük harfe çevir
            string objectName = obj.name.ToLower();
            
            // Tüm enum değerlerini otomatik olarak kontrol et
            foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
            {
                // Unknown değerini atla
                if (type == ObjectType.Unknown)
                    continue;
                
                // Enum adını al ve küçük harfe çevir
                string enumName = type.ToString().ToLower();
                
                // Özel durum kontrolü - BrownDesk ve WhiteDesk için
                if (type == ObjectType.BrownDesk && objectName.Contains("brown") && objectName.Contains("desk"))
                    return type;
                else if (type == ObjectType.WhiteDesk && objectName.Contains("white") && objectName.Contains("desk")) 
                    return type;
                // Normal kontrol - enum adı nesne adında geçiyor mu?
                else if (objectName.Contains(enumName))
                    return type;
            }
            
            // Eşleşme bulunamadı, uyarı ver
            Debug.LogWarning($"Bilinmeyen nesne tipi: {obj.name} - Hiçbir ObjectType ile eşleşmiyor.");
            
            // Bilinmeyen nesneler için Unknown kullan
            return ObjectType.Unknown;
        }
        
        private void LoadSceneObjects(SaveFile saveFile)
        {
            if (saveFile.SceneObjects == null || saveFile.SceneObjects.Count == 0)
            {
                Debug.Log("Yüklenecek 3D nesne yok.");
                return;
            }
                
            // ScenarioArea'yı bul
            Transform scenarioArea = GameObject.Find("ScenarioArea")?.transform;
            
            if (scenarioArea == null)
            {
                Debug.LogWarning("ScenarioArea bulunamadı!");
                return;
            }
            
            // Mevcut dinamik nesneleri temizle (Cube ve ObjectSpawnPosition hariç)
            List<Transform> toDestroy = new List<Transform>();
            foreach (Transform child in scenarioArea)
            {
                if (!child.name.StartsWith("Cube") && child.name != "ObjectSpawnPosition")
                    toDestroy.Add(child);
            }
            
            foreach (var child in toDestroy)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Kaydedilen nesneleri yükle
            foreach (var objInfo in saveFile.SceneObjects)
            {
                // Unknown tipindeki nesneleri atla
                if (objInfo.ObjectType == ObjectType.Unknown)
                {
                    Debug.LogWarning($"Unknown tipindeki nesne yüklenmedi: {objInfo.Name}");
                    continue;
                }
                
                // ObjectFactory kullanarak nesneyi oluştur
                if (_objectFactory == null)
                {
                    Debug.LogError("ObjectFactory null! Nesne oluşturulamadı.");
                    continue;
                }
                
                GameObject newObj = _objectFactory.Create(objInfo.ObjectType);
                
                if (newObj == null)
                {
                    Debug.LogError($"Nesne oluşturulamadı: {objInfo.Name}, Tip: {objInfo.ObjectType}");
                    continue;
                }
                
                // İsmi ayarla
                newObj.name = objInfo.Name;
                
                // Transform ayarla
                newObj.transform.position = new Vector3(objInfo.PosX, objInfo.PosY, objInfo.PosZ);
                newObj.transform.rotation = Quaternion.Euler(objInfo.RotX, objInfo.RotY, objInfo.RotZ);
                newObj.transform.localScale = new Vector3(objInfo.ScaleX, objInfo.ScaleY, objInfo.ScaleZ);
                
                // ScenarioArea'ya parent olarak ayarla
                newObj.transform.SetParent(scenarioArea);
                
                Debug.Log($"3D Nesne yüklendi: {objInfo.Name}, Tip: {objInfo.ObjectType}");
            }
        }
    }
}