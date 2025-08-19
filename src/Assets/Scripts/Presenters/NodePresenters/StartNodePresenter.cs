using System;
using UnityEngine;
using Managers;

namespace Presenters.NodePresenters
{
    public class StartNodePresenter : BaseNodePresenter
    {
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
        

        public override void Play()
        {
            //LogManager.LogScenario("Start node executed");
            //Debug.Log("This is start");
            CompleteNode();
        }
    }
}