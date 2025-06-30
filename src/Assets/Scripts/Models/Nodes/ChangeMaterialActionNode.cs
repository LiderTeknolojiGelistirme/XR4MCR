using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class ChangeMaterialActionNode : ActionNode
    {
        // Seçili nesne bilgileri (TouchNode pattern)
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }  // Parent nesnenin ID'si (her zaman)
        public int SelectedChildIndex { get; set; } = -1;  // -1 = parent seçili, 0+ = child index
        public bool IsChildObjectEnabled { get; set; }
        
        // Dropdown bilgileri (AudioActionNode pattern)
        [XmlArray("DropdownItems")]
        [XmlArrayItem("Item")]
        public List<string> DropdownItems { get; set; } // List of dropdown options
        
        // Seçili material bilgileri
        public string SelectedMaterialName { get; set; }
        public int SelectedMaterialIndex { get; set; } = 0;
        
        // Material değiştirme için özel property'ler
        public string MaterialName { get; set; }
        public string MaterialPath { get; set; }
        
        // XML serialization için boş constructor
        public ChangeMaterialActionNode() { }

        public ChangeMaterialActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
            this.DropdownItems = new List<string>(); // Initialize with an empty list
        }

        public ChangeMaterialActionNode(BaseNode node) : base(node)
        {
            this.DropdownItems = new List<string>();
        }
    }
} 