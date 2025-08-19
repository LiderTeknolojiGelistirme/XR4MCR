using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using Models;
using UnityEngine.UI;
using Presenters.NodePresenters;
using NodeSystem;
using Models.Nodes;
using Managers; // LogManager için eklendi

namespace Presenters
{
    public class EventPortPresenter : PortPresenter
    {
        // EventTypeEnum artık ayrı bir dosyada
        
        // Editor'da seçilebilmesi için SerializeField
        [SerializeField] private NodeSystem.EventTypeEnum _eventType;
        
        // EventType özelliğini modele taşıdık, burada getter üzerinden erişiyoruz
        public NodeSystem.EventTypeEnum EventType => (Model as Models.EventPort)?.EventType ?? _eventType;
        
        [SerializeField] private Color _eventPortColor = new Color(1f, 0.5f, 0.1f); // Turuncu
        private Image _eventPortImage;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Image bileşenini al
            _eventPortImage = GetComponent<Image>();
            
            // Event portları için özel renk ayarla
            if (_eventPortImage != null)
            {
                _eventPortImage.color = _eventPortColor;
            }
            else
            {
                LogManager.LogWarning("[EventPortPresenter] Event port image komponenti bulunamadı!");
            }
        }
        
        // Override ederek EventPort model tipini kullanacağız
        public void Initialize(Models.EventPort model)
        {
            // Önce model'e seçilen EventType'ı ata
            model.EventType = _eventType;
            
            // Sonra base'in Initialize metodunu çağır
            base.Initialize(model);
            
            // Initialize'dan sonra kontrol noktasını ayarla
            SetControlPointDistanceAngle(50, 0);
        }
        
        // Event tetiklendiğinde bağlı action node'ları çalıştır
        public void TriggerEvent()
        {
            // Tüm bağlantıları dolaşalım
            foreach (var connection in ConnectionPresenters)
            {
                // Hedef portun presenter'ını bulalım
                PortPresenter targetPortPresenter = connection.Model.TargetPort;
                
                if (targetPortPresenter != null)
                {
                    BaseNodePresenter targetNodePresenter = FindParentNodePresenter(targetPortPresenter);

                    if (targetNodePresenter != null)
                    {
                        if (targetPortPresenter.CompareTag("RemoveInput") && targetNodePresenter != null)
                        {
                            DescriptionActionNodePresenter actionNodePresenter = targetNodePresenter as DescriptionActionNodePresenter;

                            if (actionNodePresenter != null)
                            {
                                actionNodePresenter.PerformRemove();
                            }
                            else
                            {
                                LogManager.LogError("[EventPortPresenter] DescriptionActionNodePresenter cast edilemedi!");
                            }
                        }
                        else if (targetPortPresenter.CompareTag("StopInput") && targetNodePresenter != null)
                        {
                            ActionNodePresenter actionNodePresenter = targetNodePresenter as ActionNodePresenter;

                            if (actionNodePresenter != null)
                            {
                                actionNodePresenter.StopAction();
                            }
                            else
                            {
                                LogManager.LogError("[EventPortPresenter] ActionNodePresenter cast edilemedi!");
                            }
                        }
                        else
                        {
                            // Node'u çalıştır
                            targetNodePresenter.Play();
                        }
                    }
                    else
                    {
                        LogManager.LogError("[EventPortPresenter] Hedef node presenter bulunamadı!");
                    }
                }
                else
                {
                    LogManager.LogError("[EventPortPresenter] Hedef port presenter null!");
                }
            }
        }
        
        // Port'un bağlı olduğu node'u bul
        private BaseNodePresenter FindParentNodePresenter(PortPresenter portPresenter)
        {
            if (portPresenter == null) 
            {
                LogManager.LogWarning("[EventPortPresenter] FindParentNodePresenter: PortPresenter null!");
                return null;
            }
                
            // Portu içeren transform'dan başlayarak yukarı doğru node arıyoruz
            Transform current = portPresenter.transform;
            int searchDepth = 0;
            
            // Yukarı doğru giderek node'u bulalım
            while (current != null && searchDepth < 10) // Sonsuz döngüyü önlemek için limit
            {
                // Bu GameObject bir BaseNodePresenter içeriyor mu?
                BaseNodePresenter nodePresenter = current.GetComponent<BaseNodePresenter>();
                if (nodePresenter != null)
                {
                    return nodePresenter;
                }
                
                // Bir üst transform'a geç
                current = current.parent;
                searchDepth++;
            }
            
            LogManager.LogError($"[EventPortPresenter] BaseNodePresenter bulunamadı! Arama derinliği: {searchDepth}");
            return null;
        }
    }
} 