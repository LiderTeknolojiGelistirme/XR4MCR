using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class LogicNode : BaseNode
    {
        public LogicNode()
        {
        }
        
        public LogicNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}