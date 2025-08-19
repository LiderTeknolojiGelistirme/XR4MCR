using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class GrabNode : BaseNode
    {
        // Seçili nesne bilgileri (TouchNode sistemine benzer)
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public string SelectedChildName { get; set; }  // Child nesnesinin ismi (name-based, TouchNode ile uyumlu)
        public bool IsChildObjectEnabled { get; set; }
        
        // Target pozisyon bilgileri (grab node'a özel)
        public float TargetPosX { get; set; }
        public float TargetPosY { get; set; }
        public float TargetPosZ { get; set; }
        public bool HasTargetPosition { get; set; }  // Target pozisyonunun set edilip edilmediği
        
        // Target rotation bilgileri (Euler angles)
        public float TargetRotX { get; set; }
        public float TargetRotY { get; set; }
        public float TargetRotZ { get; set; }
        public bool HasTargetRotation { get; set; }  // Target rotasyonunun set edilip edilmediği
        
        // XML serializasyon için boş constructor
        public GrabNode() { }
        
        public GrabNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}       