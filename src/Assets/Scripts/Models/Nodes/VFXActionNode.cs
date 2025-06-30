using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class VFXActionNode : ActionNode
    {
        // Seçili VFX effect bilgileri - SADECE BUNLAR XML'e kaydedilir
        public string SelectedEffect { get; set; }
        public int SelectedVFXIndex { get; set; } = 0;
        
        // Duration to show the VFX
        public float Duration { get; set; } = 2.0f;
        
        // Whether to use duration or show VFX indefinitely
        public bool UseDuration { get; set; } = true;

        // Target position properties (MVP pattern)
        public float TargetPositionX { get; set; } = 0f;
        public float TargetPositionY { get; set; } = 0f;
        public float TargetPositionZ { get; set; } = 0f;
        public bool HasTargetPosition { get; set; } = false;

        // UI wrapper property (XmlIgnore)
        [XmlIgnore]
        public Vector3 TargetPosition
        {
            get { return new Vector3(TargetPositionX, TargetPositionY, TargetPositionZ); }
            set 
            { 
                TargetPositionX = value.x;
                TargetPositionY = value.y; 
                TargetPositionZ = value.z;
                HasTargetPosition = true;
            }
        }

        // XML serialization for an empty constructor
        public VFXActionNode() { }

        public VFXActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
        }

        public VFXActionNode(BaseNode node) : base(node)
        {
        }
    }
}