using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Models.Nodes
{
    
    [System.Serializable]
    public class FinishNode : BaseNode
    {
        // XML serializasyon için boş constructor
        public FinishNode() { }
        
        public FinishNode(string id, string title, Color color, bool enableSelect,List<Port> ports) : base(id, title, color, enableSelect,ports)
        {
        }
    }
}