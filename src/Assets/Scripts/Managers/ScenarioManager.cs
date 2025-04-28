using System.Collections.Generic;
using Presenters;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class ScenarioManager : MonoBehaviour
    {
        // Aktif node'u bilmeli -  butun node'lari bilmeli - Skip, Next gibi metodlar burada olmali - 
        [Inject] private GraphManager _graphManager;

        public BaseNodePresenter ActiveNodePresenter { get; set; }
        public List<BaseNodePresenter> NodePresenters { get; set; }
        
        public void StartScenario()
        {
            Debug.Log("Scenario Started!");
            _graphManager.StartNode.StartNode();
        }

        public void FinishScenario()
        {
            Debug.Log("Scenario Finished!");
        }

        public void GoNextNode()
        {
            ActiveNodePresenter.GoToNextNode();
        }

        public void GoPreviousNode()
        {
            ActiveNodePresenter.GoToPreviousNode();
        }

    }
}