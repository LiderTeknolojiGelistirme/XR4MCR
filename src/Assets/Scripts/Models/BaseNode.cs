using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Models.Nodes;
using Presenters;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Enums;

namespace Models
{
    [XmlInclude(typeof(FinishNode))]
    [XmlInclude(typeof(StartNode))]
    [XmlInclude(typeof(TouchNode))]
    [XmlInclude(typeof(GrabNode))]
    [XmlInclude(typeof(GetKeyDownNode))]
    [XmlInclude(typeof(LookNode))]
    [XmlInclude(typeof(LogicNode))]
    [XmlInclude(typeof(ActionNode))]
    [XmlInclude(typeof(VFXActionNode))]
    [XmlInclude(typeof(HighlightObjectActionNode))]
    [XmlInclude(typeof(AudioActionNode))]
    [XmlInclude(typeof(ChangeMaterialActionNode))]
    [XmlInclude(typeof(ChangePositionActionNode))]
    [XmlInclude(typeof(ChangeRotationActionNode))]
    [XmlInclude(typeof(ChangeScaleActionNode))]
    [XmlInclude(typeof(ToggleObjectActionNode))]
    [XmlInclude(typeof(PlayAnimationActionNode))]
    [XmlInclude(typeof(RobotAnimationActionNode))]
    [XmlInclude(typeof(DescriptionActionNode))]
    [XmlInclude(typeof(WorldDescriptionActionNode))]
    [XmlInclude(typeof(ToolTouchNode))]
    [XmlInclude(typeof(WaitForNextNode))]
    [XmlInclude(typeof(Models.EventPort))]
    [Serializable]
    public abstract class BaseNode
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NodeType Type { get; set; }
        
        public float PosX { get; set; }
        public float PosY { get; set; }
        
        public float ColorR { get; set; }
        public float ColorG { get; set; }
        public float ColorB { get; set; }
        public float ColorA { get; set; }
        
        [XmlIgnore]
        public Color Color
        {
            get => new Color(ColorR, ColorG, ColorB, ColorA);
            set
            {
                ColorR = value.r;
                ColorG = value.g;
                ColorB = value.b;
                ColorA = value.a;
            }
        }
        
        [XmlIgnore]
        public Color HeaderColor { get; set; }
        
        [XmlIgnore]
        public Color DefaultColor { get; set; }
        
        public bool EnableSelect { get; set; }
        
        public bool EnableSelfConnection = true;

        #region Scenario Members
        
        public bool IsActive, IsStarted, IsCompleted;
        
        public List<Port> Ports { get; set; } = new List<Port>();
        
        
        #endregion
        
        public BaseNode() { }

        public BaseNode(string id, string title, Color color, bool enableSelect, List<Port> ports)
        {
            ID = id;
            Title = title;
            Color = color;
            DefaultColor = color;
            HeaderColor = new Color(0.2f, 0.2f, 0.2f); // Koyu gri header
            EnableSelect = enableSelect;
            if (ports != null)
                Ports.AddRange(ports);
        }

        public BaseNode(BaseNode node)
        {
            ID = node.ID;
            Title = node.Title;
            Color = node.Color;
            DefaultColor = node.DefaultColor;
            HeaderColor = node.HeaderColor;
            EnableSelect = node.EnableSelect;
            if (node.Ports != null)
            {
                Ports.AddRange(node.Ports);
            }
        }
        
    }
}