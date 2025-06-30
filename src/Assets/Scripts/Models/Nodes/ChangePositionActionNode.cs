using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class ChangePositionActionNode : ActionNode
    {
        // Seçili nesne bilgileri (GrabNode sistemine benzer)
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public string SelectedChildName { get; set; }  // Child nesnesinin ismi (name-based, GrabNode ile uyumlu)
        public bool IsChildObjectEnabled { get; set; }
        
        // Target pozisyon bilgileri
        public float TargetPosX { get; set; }
        public float TargetPosY { get; set; }
        public float TargetPosZ { get; set; }
        public bool HasTargetPosition { get; set; }
        
        // Animasyon süresi
        public int Duration { get; set; } = 0;
        
        // Pozisyon değiştirme ayarları
        public bool UseRelativePosition { get; set; } = false;
        
        // XML serialization için boş constructor
        public ChangePositionActionNode() { }

        public ChangePositionActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public ChangePositionActionNode(BaseNode node) : base(node)
        {
        }
    }
} 