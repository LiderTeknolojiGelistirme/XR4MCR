using System;
using System.Collections.Generic;
using UnityEngine;
using Models;

namespace Models.Nodes
{
    [Serializable]
    public class ActionNode : BaseNode
    {
        public string TargetObjectID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        
        // XML serializasyon için boş constructor
        public ActionNode() { }
        
        public ActionNode(string id, string title,  Color color, bool enableSelect, List<Port> ports) 
            : base(id, title, color, enableSelect, ports)
        {
        }
        public ActionNode(BaseNode node) 
            : base(node)
        {
        }
    }
} 