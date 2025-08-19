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
        // Seçili nesne bilgileri (GrabNode sistemine benzer)
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public string SelectedChildName { get; set; }  // Child nesnesinin ismi (name-based, GrabNode ile uyumlu)
        public bool IsChildObjectEnabled { get; set; }
        
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