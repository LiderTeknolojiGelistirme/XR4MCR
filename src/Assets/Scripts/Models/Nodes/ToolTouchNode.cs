using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class ToolTouchNode : BaseNode
    {
        // Tool seçimi bilgileri (TouchNode pattern)
        public string ToolObjectName { get; set; }
        public string ToolObjectID { get; set; }  // Tool parent nesnenin ID'si
        public bool IsToolChildEnabled { get; set; }
        
        // Target seçimi bilgileri (TouchNode pattern)
        public string TargetObjectName { get; set; }
        public string TargetObjectID { get; set; }  // Target parent nesnenin ID'si
        public string TargetChildName { get; set; }  // Child nesnesinin ismi
        public bool IsTargetChildEnabled { get; set; }

        // XML serializasyon için boş constructor
        public ToolTouchNode() { }
        
        public ToolTouchNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
            Description = "Touch the target object with the selected tool";
        }

        public ToolTouchNode(BaseNode node) : base(node)
        {
            Description = "Touch the target object with the selected tool";
        }
    }
}