using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class LookNode : BaseNode
    {
        // Nesne seçimi (TouchNode'daki sistem)
        public string SelectedObjectName { get; set; } = "";
        public string SelectedObjectID { get; set; } = "";  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public bool IsChildObjectEnabled { get; set; } = false;
        
        // Look özel özellikleri
        public float LookDistance { get; set; } = 10f;
        public float LookDuration { get; set; } = 3f;
        
        // XML serialization için boş constructor
        public LookNode() { }
        
        public LookNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }
    }
}