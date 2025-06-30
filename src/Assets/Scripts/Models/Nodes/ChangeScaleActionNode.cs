using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class ChangeScaleActionNode : ActionNode
    {
        // Seçili nesne bilgileri
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }
        
        // Target scale bilgileri
        public float TargetScaleX { get; set; } = 1.0f;
        public float TargetScaleY { get; set; } = 1.0f;
        public float TargetScaleZ { get; set; } = 1.0f;
        public bool HasTargetScale { get; set; }
        public bool UniformScale { get; set; } = true;
        
        // Animasyon süresi
        public int Duration { get; set; } = 0;
        
        // XML serialization için boş constructor
        public ChangeScaleActionNode() { }

        public ChangeScaleActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public ChangeScaleActionNode(BaseNode node) : base(node)
        {
        }
    }
} 