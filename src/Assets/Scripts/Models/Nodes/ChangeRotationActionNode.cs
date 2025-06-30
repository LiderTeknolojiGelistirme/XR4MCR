using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class ChangeRotationActionNode : ActionNode
    {
        // Seçili nesne bilgileri
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }
        
        // Target rotation bilgileri (Euler angles)
        public float TargetRotX { get; set; }
        public float TargetRotY { get; set; }
        public float TargetRotZ { get; set; }
        public bool HasTargetRotation { get; set; }
        
        // Animasyon süresi
        public int Duration { get; set; } = 0;
        
        // Rotasyon ayarları
        public bool UseRelativeRotation { get; set; } = false;
        
        // XML serialization için boş constructor
        public ChangeRotationActionNode() { }

        public ChangeRotationActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public ChangeRotationActionNode(BaseNode node) : base(node)
        {
        }
    }
} 