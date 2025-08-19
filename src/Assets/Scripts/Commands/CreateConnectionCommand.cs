using Interfaces;
using Managers;
using Presenters;
using UnityEngine;

namespace Commands
{
    public class CreateConnectionCommand : ICommand
    {
        GraphManager _graphManager;
        private PortPresenter _sourcePortPresenter, _targetPortPresenter;
        
        private ConnectionPresenter _connectionPresenter;
        private string _sourcePortId,_targetPortId;

        public CreateConnectionCommand(PortPresenter sourcePortPresenter, PortPresenter targetPortPresenter, ConnectionPresenter connectionPresenter, GraphManager graphManager)
        {
            _sourcePortPresenter = sourcePortPresenter;
            _targetPortPresenter = targetPortPresenter;
            _connectionPresenter = connectionPresenter;
            _graphManager = graphManager;
            _sourcePortId = sourcePortPresenter.Model.ID;
            _targetPortId = targetPortPresenter.Model.ID;
        }
        public async void Execute()
        {
            _connectionPresenter =  _sourcePortPresenter.ConnectTo(_targetPortPresenter);
            _graphManager.UpdateConnectionsLine();
        }

        public void Undo()
        {
            _connectionPresenter.Remove();
            _graphManager.UpdateConnectionsLine();
        }

        public async void Redo()
        {
            if (_sourcePortPresenter == null)
            {
                foreach (BaseNodePresenter nodePresenter in _graphManager.NodePresenters)
                {
                    foreach(PortPresenter portPresenter in nodePresenter.Ports)
                    {
                        if(portPresenter.Model.ID == _sourcePortId)
                        {
                            _sourcePortPresenter = portPresenter;
                        }
                    }
                    foreach (PortPresenter portPresenter in nodePresenter.EventPorts)
                    {
                        if (portPresenter.Model.ID == _sourcePortId)
                        {
                            _sourcePortPresenter = portPresenter;
                        }
                    }
                }
            }
            if (_targetPortPresenter == null)
            {
                foreach (BaseNodePresenter nodePresenter in _graphManager.NodePresenters)
                {
                    foreach (PortPresenter portPresenter in nodePresenter.Ports)
                    {
                        if (portPresenter.Model.ID == _targetPortId)
                        {
                            _targetPortPresenter = portPresenter;
                        }
                    }
                    foreach (PortPresenter portPresenter in nodePresenter.EventPorts)
                    {
                        if (portPresenter.Model.ID == _targetPortId)
                        {
                            _targetPortPresenter = portPresenter;
                        }
                    }
                }
            }
            _connectionPresenter =  _sourcePortPresenter.ConnectTo(_targetPortPresenter);
            _graphManager.UpdateConnectionsLine();
        }
    }
}