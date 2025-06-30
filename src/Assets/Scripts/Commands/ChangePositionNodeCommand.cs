using Enums;
using Interfaces;
using Managers;
using Presenters;
using UnityEngine;

namespace Commands
{
    public class ChangePositionNodeCommand : ICommand
    {
        private BaseNodePresenter _nodePresenter;
        private GraphManager _graphManager;
        Vector2 _initialPosition, _endPosition;
        private string id;

        public ChangePositionNodeCommand(GraphManager graphManager, BaseNodePresenter nodePresenter,
            Vector2 initialPosition, Vector2 endPosition)
        {
            _graphManager = graphManager;
            _initialPosition = initialPosition;
            _endPosition = endPosition;
            _nodePresenter = nodePresenter;
            id = nodePresenter.Model.ID;
        }

        public void Execute()
        {
            if (_nodePresenter == null)
            {
                foreach (BaseNodePresenter nodePresenter in _graphManager.NodePresenters)
                {
                    if (nodePresenter.Model.ID == id)
                    {
                        _nodePresenter = nodePresenter;
                        break;
                    }
                }
            }

            _nodePresenter.GetComponent<RectTransform>().anchoredPosition = _endPosition;
        }

        public void Undo()
        {
            if (_nodePresenter == null)
            {
                foreach (BaseNodePresenter nodePresenter in _graphManager.NodePresenters)
                {
                    if (nodePresenter.Model.ID == id)
                    {
                        _nodePresenter = nodePresenter;
                        break;
                    }
                }
            }
            _nodePresenter.GetComponent<RectTransform>().anchoredPosition = _initialPosition;
        }

        public void Redo()
        {
            if (_nodePresenter == null)
            {
                foreach (BaseNodePresenter nodePresenter in _graphManager.NodePresenters)
                {
                    if (nodePresenter.Model.ID == id)
                    {
                        _nodePresenter = nodePresenter;
                        break;
                    }
                }
            }
            _nodePresenter.GetComponent<RectTransform>().anchoredPosition = _endPosition;
        }
    }
}