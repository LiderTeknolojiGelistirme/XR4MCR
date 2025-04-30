using Managers;
using Viroo.Interactions;
using Zenject;

namespace Actions
{
    public class ScaleUpAction : BroadcastObjectAction
    {
        [Inject] private GraphManager _graphManager;
        protected override void LocalExecuteImplementation(string data)
        {
            _graphManager.ScaleUpGraph();
        }
    }
}