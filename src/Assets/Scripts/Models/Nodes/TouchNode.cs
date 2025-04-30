using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class TouchNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public TouchNode() { }
        
        public TouchNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}