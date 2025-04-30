using Managers;
using Viroo.Interactions;
using Zenject;

namespace Actions
{
    public class ScaleDownAction : BroadcastObjectAction
    {
        [Inject] private GraphManager _graphManager;
        protected override void LocalExecuteImplementation(string data)
        {
            _graphManager.ScaleDownGraph();
        }
    }
}