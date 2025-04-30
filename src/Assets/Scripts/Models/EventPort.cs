using System;
using UnityEngine;
using Presenters;
using NodeSystem;
using System.Xml.Serialization;

namespace Models
{
    [Serializable]
    public class EventPort : Port
    {
        // XML Serileştirme için EventType'ın string versiyonu
        private string _eventTypeString;
        
        [XmlIgnore] // Enum değeri XML'de doğrudan serileştirilmeyecek
        public NodeSystem.EventTypeEnum EventType 
        { 
            get
            {
                // String değerinden enum'a dönüştür
                if (!string.IsNullOrEmpty(_eventTypeString) && 
                    Enum.TryParse<NodeSystem.EventTypeEnum>(_eventTypeString, out var result))
                {
                    return result;
                }
                return NodeSystem.EventTypeEnum.OnActivated; // Varsayılan değer
            }
            set
            {
                // Enum değerini string olarak sakla
                _eventTypeString = value.ToString();
            }
        }
        
        // XML serileştirme için string property
        public string EventTypeString
        {
            get => _eventTypeString;
            set => _eventTypeString = value;
        }
        
        // XML serializasyon için boş constructor
        public EventPort() : base() { }
        
        public EventPort(NodeSystem.PolarityType type, string name, BaseNodePresenter baseNodeModel, NodeSystem.EventTypeEnum eventType) 
            : base(type, name, baseNodeModel)
        {
            EventType = eventType;
        }
    }
} 