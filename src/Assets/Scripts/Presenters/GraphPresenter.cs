using UnityEngine;
using Models;
using System.Collections.Generic;
using Enums;
using Factories;
using Managers;
using Zenject;
using UnityEditor;

namespace Presenters
{
    public class GraphPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _connectionPrefab;
        
        private NodeGraph _model;
        private readonly Dictionary<string, BaseNodePresenter> _nodePresenters = new Dictionary<string, BaseNodePresenter>();
        private readonly Dictionary<string, ConnectionPresenter> _connectionPresenters = new Dictionary<string, ConnectionPresenter>();
        private NodePresenterFactory _nodePresenterFactory;
        private GraphManager _graphManager;

        [Inject]
        public void Construct(NodePresenterFactory nodePresenterFactory, GraphManager graphManager)
        {
            Debug.Log("ENTER: GraphPresenter Construct");
            _nodePresenterFactory = nodePresenterFactory;
            _graphManager = graphManager;
        }

        private void Awake()
        {
            _model = new NodeGraph();
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            _model.OnNodeAdded += HandleNodeAdded;
            _model.OnNodeRemoved += HandleNodeRemoved;
            _model.OnConnectionCreated += HandleConnectionCreated;
            _model.OnConnectionRemoved += HandleConnectionRemoved;
        }

        public BaseNodePresenter CreateNodePresenter(Vector2 position, NodeType nodeType)
        {
            var nodePresenter = _nodePresenterFactory.Create(position, nodeType);
            _nodePresenters.Add(nodePresenter.Model.ID, nodePresenter);
            return nodePresenter;
        }

        public ConnectionPresenter CreateConnectionPresenter(PortPresenter outputPort, PortPresenter inputPort)
        {
            var connectionGO = Instantiate(_connectionPrefab, transform);
            var connectionPresenter = connectionGO.GetComponent<ConnectionPresenter>();
            var connection = new Connection(inputPort, outputPort);
            connectionPresenter.Initialize(connection);
            _connectionPresenters.Add(connection.ID, connectionPresenter);
            return connectionPresenter;
        }

        #region Event Handlers
        private void HandleNodeAdded(BaseNodePresenter baseNode)
        {
        }

        private void HandleNodeRemoved(BaseNodePresenter baseNode)
        {
            if (_nodePresenters.TryGetValue(baseNode.ID, out var presenter))
            {
                Destroy(presenter.gameObject);
                _nodePresenters.Remove(baseNode.ID);
            }
        }

        private void HandleConnectionCreated(ConnectionPresenter connection)
        {
        }

        private void HandleConnectionRemoved(ConnectionPresenter connection)
        {
            if (_connectionPresenters.TryGetValue(connection.ID, out var presenter))
            {
                Destroy(presenter.gameObject);
                _connectionPresenters.Remove(connection.ID);
            }
        }
        #endregion

        public void Clear()
        {
            foreach (var presenter in _nodePresenters.Values)
            {
                Destroy(presenter.gameObject);
            }
            foreach (var presenter in _connectionPresenters.Values)
            {
                Destroy(presenter.gameObject);
            }
            _nodePresenters.Clear();
            _connectionPresenters.Clear();
        }
    }
}
