using System;
using System.Collections.Generic;
using Presenters;

namespace Models
{
    public class NodeGraph
    {
         private List<BaseNodePresenter> _nodes = new List<BaseNodePresenter>();
        private List<ConnectionPresenter> _connections = new List<ConnectionPresenter>();

        public event Action<BaseNodePresenter> OnNodeAdded;
        public event Action<BaseNodePresenter> OnNodeRemoved;
        public event Action<ConnectionPresenter> OnConnectionCreated;
        public event Action<ConnectionPresenter> OnConnectionRemoved;

        public void AddNode(BaseNodePresenter baseNode)
        {
            _nodes.Add(baseNode);
            OnNodeAdded?.Invoke(baseNode);
        }

        public void RemoveNode(BaseNodePresenter baseNode)
        {
            if (_nodes.Remove(baseNode))
            {
                OnNodeRemoved?.Invoke(baseNode);
            }
        }

        public void AddConnection(ConnectionPresenter connection)
        {
            _connections.Add(connection);
            OnConnectionCreated?.Invoke(connection);
        }

        public void RemoveConnection(ConnectionPresenter connection)
        {
            if (_connections.Remove(connection))
            {
                OnConnectionRemoved?.Invoke(connection);
            }
        }
    }
} 