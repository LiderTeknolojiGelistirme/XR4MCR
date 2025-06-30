using Models.Nodes;
using UnityEngine;
using UnityEngine.Serialization;
using Models.Nodes;

namespace Presenters.NodePresenters
{
    public class GetKeyDownNodePresenter : BaseNodePresenter
    {
        public KeyCode keyCode;

        void Start()
        {
            // Description'ı sadece boşsa set et (Load'dan gelen değeri korumak için)
            if (string.IsNullOrEmpty(Model.Description))
            {
                Model.Description = "Wait for a key to be pressed";
            }
        }

        public override void Play()
        {
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log("Pressed: " + keyCode);
                CompleteNode();
            }
        }

        public override void ActivateNode()
        {
            Debug.Log("TestNode Activated");
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("TestNode Started");
            base.StartNode();
        }

        public override void CompleteNode()
        {
            Debug.Log("TestNode Completed");
            base.CompleteNode();
        }
    }
}