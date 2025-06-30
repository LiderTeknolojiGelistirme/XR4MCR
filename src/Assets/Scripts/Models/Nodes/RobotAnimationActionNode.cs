using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class RobotAnimationActionNode : ActionNode
    {
        // Seçili robot nesnesi bilgileri
        public string SelectedObjectName { get; set; }
        public string SelectedObjectID { get; set; }
        
        // Target pozisyon bilgileri
        public float TargetPosX { get; set; }
        public float TargetPosY { get; set; }
        public float TargetPosZ { get; set; }
        public bool HasTargetPosition { get; set; }
        
        // Animasyon süresi
        public int Duration { get; set; } = 0;
        
        // Robot animasyon ayarları
        public string RobotAnimationType { get; set; }
        public bool WaitForCompletion { get; set; } = true;
        
        [XmlArray("AnimationParameters")]
        [XmlArrayItem("Parameter")]
        public List<string> AnimationParameters { get; set; } = new List<string>();
        
        // XML serialization için boş constructor
        public RobotAnimationActionNode() { }

        public RobotAnimationActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public RobotAnimationActionNode(BaseNode node) : base(node)
        {
        }
    }
} 