using System;
using Enums;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Helpers
{
    public class AddNodeButtonHelper : MonoBehaviour
    {
        public NodeType nodeType;

        [Inject] private GraphManager _graphManager;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(AddNode);
        }

        public void AddNode()
        {
            _graphManager.CreateTestNode(nodeType);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(AddNode);
        }
    }
}