using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class GhostNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public GhostNode() { }
        
        public GhostNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}