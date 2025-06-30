using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class PlayAnimationActionNode : ActionNode
    {
        // Animasyon oynatma için özel property'ler
        public string AnimationName { get; set; }
        public float PlaybackSpeed { get; set; } = 1.0f;
        public bool LoopAnimation { get; set; } = false;
        public bool WaitForCompletion { get; set; } = true;
        
        // XML serialization için boş constructor
        public PlayAnimationActionNode() { }

        public PlayAnimationActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public PlayAnimationActionNode(BaseNode node) : base(node)
        {
        }
    }
} 