using Enums;
using Interfaces;
using Managers;
using Models;
using Presenters;
using UnityEngine;
using Zenject;
using Managers;
using Cysharp.Threading.Tasks.Triggers;

namespace Commands
{
    public class CreateNodeCommand : ICommand
    {
        GraphManager _graphManager;
        private BaseNodePresenter _nodePresenter;
        private NodeType _nodeType;
        private Vector2 _position;
        private BaseNode _model;
    
        public CreateNodeCommand(NodeType nodeType, GraphManager graphManager)
        {
            _nodeType = nodeType;
            _graphManager = graphManager;
            _position = Vector2.zero;
        }

        public CreateNodeCommand(NodeType nodeType, GraphManager graphManager, Vector2 position)
        {
            _nodeType = nodeType;
            _graphManager = graphManager;
            _position = position;
        }
        public void Execute()
        {
            //_nodePresenter = _graphManager.CreateNodeAtPosition(new Vector2(_inputManager.GetCanvasPointerPosition(_graphManager).x, _inputManager.GetCanvasPointerPosition(_graphManager).y), _nodeType);
            _nodePresenter = _graphManager.CreateNodeAtPosition(_position, _nodeType);
            _model = _nodePresenter.Model;
            
            // Ghost node'lar için log almıyoruz
            if (_nodeType != NodeType.Ghost)
            {
                LogManager.LogInteraction(_nodePresenter.Model.Title + " is created ");
            }
        }
        
        public void Redo()
        {
            _nodePresenter = _graphManager.CreateNodeAtPosition(_position, _nodeType,_model);
            
            // Ghost node'lar için log almıyoruz
            if (_nodeType != NodeType.Ghost)
            {
                LogManager.LogInteraction("Redo");
            }
        }

        public void Undo()
        {
            _nodePresenter.Remove();
            
            // Ghost node'lar için log almıyoruz
            if (_nodeType != NodeType.Ghost)
            {
                LogManager.LogInteraction("Undo");
            }
        }
        
    }
}