using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class WaitForNextNode : BaseNode
    {
        // Bekleme süresi (saniye cinsinden)
        public float WaitTimeInSeconds { get; set; } = 5f;
        
        // Initial bekleme süresi (reset için)
        public float InitialTimeInSeconds { get; set; } = 5f;
        
        // Timer durumu
        public bool IsTimerRunning { get; set; } = false;
        
        // XML serializasyon için boş constructor
        public WaitForNextNode() { }
        
        public WaitForNextNode(string id, string title, Color color, bool enableSelect, List<Port> ports) 
            : base(id, title, color, enableSelect, ports)
        {
            WaitTimeInSeconds = 5f;
            InitialTimeInSeconds = 5f;
            IsTimerRunning = false;
        }
    }
} 