using UnityEngine;

namespace Presenters.NodePresenters
{
    public class FinishNodePresenter : BaseNodePresenter
    {
        public override void Play()
        {
            Debug.Log("This is finish");
            CompleteNode();
        }

        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            base.StartNode();
        }

        public override void CompleteNode()
        {
            base.CompleteNode();
        }

        public override void DeactivateNode()
        {
            base.DeactivateNode();
        }
    }
}