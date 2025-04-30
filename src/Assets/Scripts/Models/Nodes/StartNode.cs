using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class StartNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public StartNode() { }
        
        public StartNode(string id, string title, Color color, bool enableSelect,List<Port> ports) : base(id, title, color, enableSelect,ports)
        {
        }
    }
}