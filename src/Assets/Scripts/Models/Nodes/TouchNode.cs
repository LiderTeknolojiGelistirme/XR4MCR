using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class TouchNode : BaseNode
    {
        // Seçili nesne bilgileri (ToolTouchNode pattern'dan adapte edildi)
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }  // Parent nesnenin ID'si (her zaman)
        public string SelectedChildName { get; set; }  // Child nesnesinin ismi (name-based)
        public bool IsChildObjectEnabled { get; set; }
        
        // XML serializasyon için boş constructor
        public TouchNode() { }
        
        public TouchNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
            Description = "Touch the selected object";
        }

        public TouchNode(BaseNode node) : base(node)
        {
            Description = "Touch the selected object";
        }
    }
}