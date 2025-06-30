using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Models.Nodes
{
    [System.Serializable]
    public class AudioActionNode : ActionNode
    {
        // Property to hold a list of items from the dropdown menu
        [XmlArray("DropdownItems")]
        [XmlArrayItem("Item")]
        public List<string> DropdownItems { get; set; } // List of dropdown options

        // Seçili audio clip bilgileri
        public string SelectedAudioName { get; set; }
        public int SelectedAudioIndex { get; set; } = 0;
        
        // Audio ayarları
        public bool IsLooping { get; set; } = false;
        public float Volume { get; set; } = 1.0f;

        // XML serialization for an empty constructor
        public AudioActionNode() { }

        public AudioActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports, List<string> dropdownItems = null)
            : base(id, title, color, enableSelect, ports)
        {
            this.DropdownItems = dropdownItems ?? new List<string>(); // Initialize with an empty list if null
        }

        public AudioActionNode(BaseNode node) : base(node)
        {
            this.DropdownItems = new List<string>();
        }
    }
}
