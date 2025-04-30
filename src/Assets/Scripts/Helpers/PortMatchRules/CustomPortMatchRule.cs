using Models;

namespace Helpers.PortMatchRules
{
    public class CustomPortMatchRule : PortMatchRule
    {
        public override bool ExecuteRule(Port draggedPort, Port foundPort)
        {
            return draggedPort.baseNode != foundPort.baseNode ? true: false;
        }
    }
}