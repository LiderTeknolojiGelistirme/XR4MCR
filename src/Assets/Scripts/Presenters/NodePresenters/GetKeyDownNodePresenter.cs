using UnityEngine;
using UnityEngine.Serialization;

namespace Presenters.NodePresenters
{
    public class GetKeyDownNodePresenter : BaseNodePresenter
    {
        public KeyCode keyCode;
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

        public override void DeactivateNode()
        {
            Debug.Log("TestNode Deactivated");
            base.DeactivateNode();
        }
    }
}