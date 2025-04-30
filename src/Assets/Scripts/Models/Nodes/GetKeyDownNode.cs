using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class GetKeyDownNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public GetKeyDownNode() { }
        
        public GetKeyDownNode(string id, string title, Color color, bool enableSelect,List<Port> ports) : base(id, title, color, enableSelect,ports)
        {
        }
    }
}