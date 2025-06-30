using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class ToggleObjectActionNode : ActionNode
    {
        // Toggle action için özel property'ler
        public bool EnableObject { get; set; } = true;
        public bool ToggleMode { get; set; } = false; // true: toggle, false: set state
        
        // XML serialization için boş constructor
        public ToggleObjectActionNode() { }

        public ToggleObjectActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public ToggleObjectActionNode(BaseNode node) : base(node)
        {
        }
    }
} 