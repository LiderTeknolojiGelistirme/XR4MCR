using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class HighlightObjectActionNode : ActionNode
    {
        // Nesne seçimi (TouchNode'daki sistem)
        public string SelectedObjectName { get; set; } = "";
        public string SelectedObjectID { get; set; } = "";  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public string SelectedChildName { get; set; }  // Child nesnesinin ismi (GrabNode ile uyumlu)
        public bool IsChildObjectEnabled { get; set; } = false;
        
        // Süre bilgileri
        public float Duration { get; set; } = 2.0f;
        public bool UseDuration { get; set; } = true; // Toggle duration kontrolü
        
        // Eski alanlar (geriye dönük uyumluluk için - deprecated)
        [XmlIgnore]
        public List<string> DropdownItems { get; set; } = new List<string>();
        
        [XmlIgnore]
        public string SelectedEffect { get; set; } = "";

        // XML serialization için boş constructor
        public HighlightObjectActionNode() { }

        public HighlightObjectActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public HighlightObjectActionNode(BaseNode node) : base(node)
        {
        }
    }
}