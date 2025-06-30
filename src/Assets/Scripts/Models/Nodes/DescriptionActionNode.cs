using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class DescriptionActionNode : ActionNode
    {
        // Text içeriği
        public string MessageText { get; set; } = "";
        
        // Gösterim süresi
        public int DisplayDuration { get; set; } = 3;
        
        // Text rengi için RGBA bileşenleri
        public float TextColorR { get; set; } = 1f; // Varsayılan beyaz
        public float TextColorG { get; set; } = 1f;
        public float TextColorB { get; set; } = 1f;
        public float TextColorA { get; set; } = 1f;
        
        // Background rengi için RGBA bileşenleri  
        public float BackgroundColorR { get; set; } = 0f; // Varsayılan siyah
        public float BackgroundColorG { get; set; } = 0f;
        public float BackgroundColorB { get; set; } = 0f;
        public float BackgroundColorA { get; set; } = 0.8f;
        
        // XML'e kayıt edilmeyen Color property'leri (UI ile kolay etkileşim için)
        [XmlIgnore]
        public Color TextColor
        {
            get => new Color(TextColorR, TextColorG, TextColorB, TextColorA);
            set
            {
                TextColorR = value.r;
                TextColorG = value.g;
                TextColorB = value.b;
                TextColorA = value.a;
            }
        }
        
        [XmlIgnore]
        public Color BackgroundColor
        {
            get => new Color(BackgroundColorR, BackgroundColorG, BackgroundColorB, BackgroundColorA);
            set
            {
                BackgroundColorR = value.r;
                BackgroundColorG = value.g;
                BackgroundColorB = value.b;
                BackgroundColorA = value.a;
            }
        }
        
        // Geriye dönük uyumluluk için eski property'ler (deprecated)
        [XmlIgnore]
        public float DisplayDurationFloat
        {
            get => DisplayDuration;
            set => DisplayDuration = Mathf.RoundToInt(value);
        }
        
        [XmlIgnore]
        public string FontSize { get; set; } = "Medium";
        
        [XmlIgnore]
        public string MessageColor { get; set; } = "White";
        
        [XmlIgnore]
        public bool AutoClose { get; set; } = true;
        
        // XML serialization için boş constructor
        public DescriptionActionNode() { }

        public DescriptionActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public DescriptionActionNode(BaseNode node) : base(node)
        {
        }
    }
} 