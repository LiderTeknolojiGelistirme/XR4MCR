using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Presenters;
using UnityEngine.UI;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine.Serialization;
using NodeSystem;
using static Presenters.PortPresenter;

namespace Models
{
    
    [Serializable]
    public class Port
    {
        // Unique identifier for the port
        public string ID { get; set; }
        
        // Node ID for serialization
        public string NodeID { get; set; }
        
        // Name property serialization için gerekli
        public string Name { get; set; }
        
        // Maximum allowed connections (0 indicates no limit)
        public int MaxConnections { get; set; } = 0;
        
        // Polarity type as string for serialization
        public string PolarityTypeString { get; set; }
        
        // XML serializasyon için boş constructor
        public Port() { }

        public Port(NodeSystem.PolarityType type, string name, BaseNodePresenter baseNodeModel)
        {
            Polarity = type;
            ID = Guid.NewGuid().ToString();
            Name = name;
            baseNode = baseNodeModel;
            NodeID = baseNodeModel.Model.ID;
            PolarityTypeString = type.ToString();
        }
       
        // Secondary identifier, lazily generated if empty
        [HideInInspector, SerializeField] private string _sID = "";
        public string SID
        {
            get
            {
                if (string.IsNullOrEmpty(_sID))
                {
                    _sID = LTGUtility.GenerateSID();
                }
                return _sID;
            }
            set => _sID = value;
        }

        // RectTransform reference for UI positioning
        [XmlIgnore]
        [HideInInspector] public RectTransform rectTransform;

        // Reference to the GraphManager instance
        [XmlIgnore]
        public GraphManager graphManager;
        
        // Associated Node instance
        [XmlIgnore]
        [FormerlySerializedAs("node")] [HideInInspector] public BaseNodePresenter baseNode;

        // Polarity setting for this port
        [SerializeField] private NodeSystem.PolarityType _polarity = NodeSystem.PolarityType.Bidirectional;
        
        [XmlIgnore]
        public NodeSystem.PolarityType Polarity
        {
            get => string.IsNullOrEmpty(PolarityTypeString) ? 
                NodeSystem.PolarityType.Bidirectional : 
                (NodeSystem.PolarityType)Enum.Parse(typeof(NodeSystem.PolarityType), PolarityTypeString);
            set 
            { 
                _polarity = value;
                PolarityTypeString = value.ToString();
            }
        }

        // Sprites for port representation based on connection state
        [XmlIgnore]
        public Sprite iconUnconnected;
        [XmlIgnore]
        public Sprite iconConnected;

        // Colors for various port states
        public Color iconColorDefault = new Color(0.98f, 0.94f, 0.84f);
        public Color iconColorHover = new Color(1f, 0.81f, 0.3f);
        public Color iconColorSelected = new Color(1f, 0.58f, 0.04f);
        public Color iconColorConnected = new Color(0.98f, 0.94f, 0.84f);

        // UI Image component used for displaying the port
        [XmlIgnore]
        public Image image;
        // Control point for handling port connections
        [XmlIgnore]
        [HideInInspector] private PortControlPoint controlPoint;
        [XmlIgnore]
        public PortControlPoint ControlPoint { get => controlPoint; set => controlPoint = value; }

        // Gets or sets the color of the port's image element
        [XmlIgnore]
        public Color ElementColor
        {
            get => image != null ? image.color : Color.clear;
            set { if (image != null) image.color = value; }
        }

        // Fixed priority value
        [XmlIgnore]
        public int Priority => 2;

        // Flag to enable or disable drag behavior
        [SerializeField] private bool _enableDrag = true;
        public bool EnableDrag
        {
            get => _enableDrag;
            set => _enableDrag = value;
        }

        // Flag to enable or disable hover behavior
        [SerializeField] private bool _enableHover = true;
        public bool EnableHover
        {
            get => _enableHover;
            set => _enableHover = value;
        }

        // Flag to enable or disable click behavior
        [SerializeField] private bool _disableClick = false;
        public bool DisableClick
        {
            get => _disableClick;
            set => _disableClick = value;
        }

        // Reference to the last found port during connection checks
        [XmlIgnore]
        [HideInInspector] public Port lastFoundPort;
        // Private field for the closest found port (currently unused)
        [XmlIgnore]
        private Port closestFoundPort;

        // Indicates whether the port can accept additional connections
        [XmlIgnore]
        public bool HasSpots => MaxConnections == 0 || ConnectionsCount < MaxConnections;

        // Counts the number of connections currently associated with this port
        [XmlIgnore]
        public int ConnectionsCount
        {
            get
            {
                
                return 0;
            }
        }

        // Yeni özellikler ekle
        [XmlIgnore]
        public bool IsConnected => ConnectionsCount > 0;
        [XmlIgnore]
        public bool CanConnect => !IsConnected || MaxConnections > ConnectionsCount;
        
        // Validation için
    }
}
