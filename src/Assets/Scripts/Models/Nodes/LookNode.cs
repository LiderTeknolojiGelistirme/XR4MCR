using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class LookNode : BaseNode
    {
        public LookNode() { }
        public LookNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}