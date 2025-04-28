using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Presenters;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using static Presenters.PortPresenter;

namespace Models
{
    [Serializable]
    public class Port
    {
        // Unique identifier for the port
        public string ID { get; }
              
        // Reference to parent node
        public BaseNodePresenter ParentBaseNode { get => baseNode; private set => baseNode = value; }
        
        // Maximum allowed connections (0 indicates no limit)
        public int MaxConnections { get; set; } = 0;

        public Port(PolarityType type, string name, BaseNodePresenter baseNodeModel)
        {
            Polarity = type;
            ID = Guid.NewGuid().ToString();
            Name = name;
            baseNode = baseNodeModel;
        }

        // Parent node'u ayarlamak için metod
        public void SetParentNode(BaseNodePresenter baseNode)
        {
            ParentBaseNode = baseNode;
        }

        // PortReference oluşturmak için yardımcı metod
        

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
        [HideInInspector] public RectTransform rectTransform;

        // Reference to the GraphManager instance
        public GraphManager graphManager;
        // Associated Node instance
        [FormerlySerializedAs("node")] [HideInInspector] public BaseNodePresenter baseNode;


        // Polarity setting for this port
        [SerializeField] private PolarityType _polarity = PolarityType.Bidirectional;
        public PolarityType Polarity
        {
            get => _polarity;
            set => _polarity = value;
        }

        // Sprites for port representation based on connection state
        public Sprite iconUnconnected;
        public Sprite iconConnected;

        // Colors for various port states
        public Color iconColorDefault = new Color(0.98f, 0.94f, 0.84f);
        public Color iconColorHover = new Color(1f, 0.81f, 0.3f);
        public Color iconColorSelected = new Color(1f, 0.58f, 0.04f);
        public Color iconColorConnected = new Color(0.98f, 0.94f, 0.84f);

        // UI Image component used for displaying the port
        public Image image;
        // Control point for handling port connections
        [HideInInspector] private PortControlPoint controlPoint;
        public PortControlPoint ControlPoint { get => controlPoint; set => controlPoint = value; }

        // Gets or sets the color of the port's image element
        public Color ElementColor
        {
            get => image != null ? image.color : Color.clear;
            set { if (image != null) image.color = value; }
        }

        // Fixed priority value
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
        [HideInInspector] public Port lastFoundPort;
        // Private field for the closest found port (currently unused)
        private Port closestFoundPort;

        // Indicates whether the port can accept additional connections
        public bool HasSpots => MaxConnections == 0 || ConnectionsCount < MaxConnections;

        // Counts the number of connections currently associated with this port
        public int ConnectionsCount
        {
            get
            {
                
                return 0;
            }
        }

        public string Name { get; }

        // Yeni özellikler ekle
        public bool IsConnected => ConnectionsCount > 0;
        public bool CanConnect => !IsConnected || MaxConnections > ConnectionsCount;
        
        // Validation için
        public bool CanConnectTo(Port other)
        {
            return Polarity != other.Polarity && 
                   CanConnect && 
                   other.CanConnect && 
                   ParentBaseNode?.ID != other.ParentBaseNode?.ID;
        }
    }
}
