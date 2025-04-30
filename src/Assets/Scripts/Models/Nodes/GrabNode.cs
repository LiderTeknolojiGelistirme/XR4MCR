using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class GrabNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public GrabNode() { }
        
        public GrabNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}