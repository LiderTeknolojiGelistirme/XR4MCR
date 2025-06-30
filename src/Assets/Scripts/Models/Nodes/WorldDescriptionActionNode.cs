using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System.Xml.Serialization;

namespace Models.Nodes
{
    [Serializable]
    public class WorldDescriptionActionNode : ActionNode
    {
        // Text içeriği
        public string WorldMessageText { get; set; } = "";
        
        // Gösterim süresi
        public int DisplayDuration { get; set; } = 3;
        
        // Text boyutu
        public float WorldTextSize { get; set; } = 1.0f;
        
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
        
        // Canvas Transform bilgileri (serileştirme için)
        public float CanvasPositionX { get; set; } = 0f;
        public float CanvasPositionY { get; set; } = 0f;
        public float CanvasPositionZ { get; set; } = 0f;
        public float CanvasRotationX { get; set; } = 0f;
        public float CanvasRotationY { get; set; } = 0f;
        public float CanvasRotationZ { get; set; } = 0f;
        public float CanvasRotationW { get; set; } = 1f;
        public float CanvasScaleX { get; set; } = 1f;
        public float CanvasScaleY { get; set; } = 1f;
        public float CanvasScaleZ { get; set; } = 1f;
        
        // Canvas parent bilgileri
        public string CanvasParentName { get; set; } = "";
        public bool IsCanvasPlaced { get; set; } = false;
        
        // Pozisyon ayarları (eski WorldPosition için)
        public float WorldPositionX { get; set; } = 0f;
        public float WorldPositionY { get; set; } = 0f;
        public float WorldPositionZ { get; set; } = 0f;
        public bool UseCustomPosition { get; set; } = false;
        public bool FollowCamera { get; set; } = true;
        public bool AutoClose { get; set; } = true;
        
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
        
        [XmlIgnore]
        public Vector3 WorldPosition
        {
            get => new Vector3(WorldPositionX, WorldPositionY, WorldPositionZ);
            set
            {
                WorldPositionX = value.x;
                WorldPositionY = value.y;
                WorldPositionZ = value.z;
            }
        }
        
        // Canvas Transform property'leri (UI ile kolay etkileşim için)
        [XmlIgnore]
        public Vector3 CanvasPosition
        {
            get => new Vector3(CanvasPositionX, CanvasPositionY, CanvasPositionZ);
            set
            {
                CanvasPositionX = value.x;
                CanvasPositionY = value.y;
                CanvasPositionZ = value.z;
            }
        }
        
        [XmlIgnore]
        public Quaternion CanvasRotation
        {
            get => new Quaternion(CanvasRotationX, CanvasRotationY, CanvasRotationZ, CanvasRotationW);
            set
            {
                CanvasRotationX = value.x;
                CanvasRotationY = value.y;
                CanvasRotationZ = value.z;
                CanvasRotationW = value.w;
            }
        }
        
        [XmlIgnore]
        public Vector3 CanvasScale
        {
            get => new Vector3(CanvasScaleX, CanvasScaleY, CanvasScaleZ);
            set
            {
                CanvasScaleX = value.x;
                CanvasScaleY = value.y;
                CanvasScaleZ = value.z;
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
        public string TextColorString { get; set; } = "White";
        
        // XML serialization için boş constructor
        public WorldDescriptionActionNode() { }

        public WorldDescriptionActionNode(string id, string title, Color color, bool enableSelect, List<Port> ports) : base(id, title, color, enableSelect, ports)
        {
           
        }

        public WorldDescriptionActionNode(BaseNode node) : base(node)
        {
        }
    }
} 