using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using Models;
using UnityEngine.UI;
using Presenters.NodePresenters;
using NodeSystem;
using Models.Nodes;

namespace Presenters
{
    public class EventPortPresenter : PortPresenter
    {
        // EventTypeEnum artık ayrı bir dosyada
        
        // Editor'da seçilebilmesi için SerializeField
        [SerializeField] private NodeSystem.EventTypeEnum _eventType = NodeSystem.EventTypeEnum.OnActivated;
        
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
            
            // Kontrol noktasının açısını doğru bir şekilde ayarla
            // Bu, bağlantının doğrudan sağa doğru çıkmasını sağlar
            //SetControlPointDistanceAngle(50, 0);
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


                    //if (targetPortPresenter.CompareTag("StopInput") && targetNodePresenter != null)
                    //{
                    //    ActionNodePresenter actionNodePresenter = targetNodePresenter as ActionNodePresenter;
                    //    if (actionNodePresenter is AudioActionNodePresenter)
                    //    {
                    //        //StopLoop burada çağıralacak
                    //    }
                    //}
                    //// Hedef portun node'unu bulalım

                    


                    if (targetNodePresenter != null)
                    {
                        Debug.Log($"Event tetiklendi: {EventType} -> {targetNodePresenter.Model.Title}");



                        if (targetPortPresenter.CompareTag("RemoveInput") && targetNodePresenter != null)
                        {
                            DescriptionActionNodePresenter actionNodePresenter = targetNodePresenter as DescriptionActionNodePresenter;

                            actionNodePresenter.PerformRemove();
                           
                        }

                        if (targetPortPresenter.CompareTag("StopInput") && targetNodePresenter != null)
                        {
                            AudioActionNodePresenter actionNodePresenter = targetNodePresenter as AudioActionNodePresenter;

                            actionNodePresenter.PerformStop();

                        }

                        //// Hedef portun node'unu bulalım
                        else
                        {
                            // Node'u çalıştır
                            targetNodePresenter.Play();
                        }
                        
                    }
                }
            }
        }
        
        // Port'un bağlı olduğu node'u bul
        private BaseNodePresenter FindParentNodePresenter(PortPresenter portPresenter)
        {
            if (portPresenter == null) 
                return null;
                
            // Portu içeren transform'dan başlayarak yukarı doğru node arıyoruz
            Transform current = portPresenter.transform;
            
            // Yukarı doğru giderek node'u bulalım
            while (current != null)
            {
                // Bu GameObject bir BaseNodePresenter içeriyor mu?
                BaseNodePresenter nodePresenter = current.GetComponent<BaseNodePresenter>();
                if (nodePresenter != null)
                {
                    return nodePresenter;
                }
                
                // Bir üst transform'a geç
                current = current.parent;
            }
            
            return null;
        }
    }
} 