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

        // XML serialization for an empty constructor
        public AudioActionNode() { }

        public AudioActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports, List<string> dropdownItems = null)
            : base(id, title, color, enableSelect, ports)
        {
            this.DropdownItems = dropdownItems ?? new List<string>(); // Initialize with an empty list if null
        }
    }
}
