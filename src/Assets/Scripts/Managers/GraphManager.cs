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
        public Camera MainCamera;
        private LTGLineRenderer _lineRenderer;
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
            }
            else
            {
                Debug.LogWarning("Connection creation failed - Factory returned null");
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

        private bool IsValidConnection(PortReference source, PortReference target)
        {
            // Temel validasyon kontrolleri
            // - Port'lar mevcut mu?
            // - Aynı node'a mı bağlanmaya çalışıyor?
            // - Port tipleri uyumlu mu? vs.
            return true; // şimdilik
        }

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
            Pointer pointer, LTGLineRenderer lineRenderer)
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
            Vector2 center = Vector2.zero;
            CreateNodeAtPosition(center, NodeType.Start);
        }

        public void CreateFinishNode()
        {
            Vector2 center = Vector2.zero;
            CreateNodeAtPosition(center, NodeType.Finish);
        }

        public void Clear()
        {
            foreach (var item in ConnectionPresenters)
            {
                Object.Destroy(item.gameObject);
            }

            _connectionPresenters.Clear();

            // if (_previewConnection != null)
            // {
            //     Object.Destroy(_previewConnection.gameObject);
            //     _previewConnection = null;
            // }
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
            NodeGraphData graphData = new NodeGraphData
            {
                Nodes = new List<NodeData>(),
                Connections = new List<ConnectionData>()
            };

            foreach (var nodePresenter in _nodePresenters)
            {
                NodeData nodeData = new NodeData
                {
                    ID = nodePresenter.Model.ID,
                    Type = nodePresenter.GetType().Name,
                    Position = nodePresenter.transform.localPosition,
                    Ports = nodePresenter.Ports.Select(p => new PortData { ID = p.ID, Type = p.Polarity.ToString() }).ToList()
                };
                graphData.Nodes.Add(nodeData);
            }

            foreach (var conn in ConnectionPresenters)
            {
                graphData.Connections.Add(new ConnectionData
                {
                    SourcePortID = conn.Model.SourcePort.Model.ID,
                    TargetPortID = conn.Model.TargetPort.Model.ID
                });
            }

            XmlSerializer serializer = new XmlSerializer(typeof(NodeGraphData));
            using (FileStream fs = new FileStream(_filePath, FileMode.Create))
            {
                serializer.Serialize(fs, graphData);
            }
            Debug.Log("Kaydedilen dosya yolu: " + _filePath);
        }
        
        public void LoadGraph()
        {
            if (!File.Exists(_filePath))
                return;

            NodeGraphData graphData;
            XmlSerializer serializer = new XmlSerializer(typeof(NodeGraphData));
            using (FileStream fs = new FileStream(_filePath, FileMode.Open))
            {
                graphData = (NodeGraphData)serializer.Deserialize(fs);
            }

            Clear();

            // Node'ları tekrar oluştur
            foreach (var nodeData in graphData.Nodes)
            {
                NodeType nodeType = (NodeType)Enum.Parse(typeof(NodeType), nodeData.Type);
                BaseNodePresenter nodePresenter = CreateNodeAtPosition(nodeData.Position, nodeType);
                nodePresenter.ID = nodeData.ID;

                // Portları ayarla
                foreach(var portData in nodeData.Ports)
                {
                    var portPresenter = nodePresenter.Ports.FirstOrDefault(p => p.ID == portData.ID);
                    if (portPresenter != null)
                    {
                        portPresenter.Polarity = (PortPresenter.PolarityType)Enum.Parse(typeof(PortPresenter.PolarityType), portData.Type);
                    }
                }
            }

            // Bağlantıları oluştur
            foreach (var connData in graphData.Connections)
            {
                var sourcePort = FindPortPresenterByID(connData.SourcePortID);
                var targetPort = FindPortPresenterByID(connData.TargetPortID);
                if(sourcePort != null && targetPort != null)
                    CreateConnection(sourcePort, targetPort);
            }

            UpdateConnectionsLine();
        }
        
        PortPresenter FindPortPresenterByID(string portID)
        {
            foreach(var node in NodePresenters)
            {
                var port = node.Ports.FirstOrDefault(p => p.ID == portID);
                if(port != null) return port;
            }
            return null;
        }
    }
}