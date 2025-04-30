using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Presenters;
using Models;
using Presenters.NodePresenters;
using NodeSystem;

namespace Managers
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private GraphManager graphManager;
        
        // Zenject injection için
        private void Awake()
        {
            if (graphManager == null)
            {
                graphManager = FindObjectOfType<GraphManager>();
                if (graphManager == null)
                {
                    Debug.LogError("GraphManager bulunamadı!");
                }
            }
        }
        
        // Bağlantı oluşturma kuralları
        public bool CanCreateConnection(PortPresenter sourcePort, PortPresenter targetPort)
        {
            // Aynı node'a bağlantı kurulamaz
            if (sourcePort.transform.parent == targetPort.transform.parent)
            {
                Debug.LogWarning("Aynı node üzerinde bağlantı kurulamaz!");
                return false;
            }
            
            // Event port kontrolü
            if (sourcePort is EventPortPresenter eventPort)
            {
                // Event port'lar sadece Action node'lara bağlanabilir
                bool isTargetActionNode = IsPortConnectedToActionNode(targetPort);
                bool isTargetInput = targetPort.Polarity == NodeSystem.PolarityType.Input;
                
                if (!isTargetInput || !isTargetActionNode)
                {
                    Debug.LogWarning("Event portları sadece Action node'ların input portlarına bağlanabilir!");
                    return false;
                }
                
                return true;
            }
            
            // Event port'lara diğer port'lar bağlanamaz
            if (targetPort is EventPortPresenter)
            {
                Debug.LogWarning("Event port'lar hedef olamaz!");
                return false;
            }
            
            // Normal port bağlantısı için polarite kontrolü
            // Input -> Output veya Output -> Input olmalı
            if (sourcePort.Polarity == targetPort.Polarity)
            {
                Debug.LogWarning("Aynı polariteye sahip portlar birbirine bağlanamaz!");
                return false;
            }
            
            // Halihazırda bağlantı var mı kontrolü
            foreach (var connection in sourcePort.ConnectionPresenters)
            {
                if (connection.Model.TargetPort == targetPort)
                {
                    Debug.LogWarning("Bu portlar zaten birbirine bağlı!");
                    return false;
                }
            }
            
            // Burada diğer özel kurallar eklenebilir
            
            return true;
        }
        
        // Bir port için mevcut bağlantıların sayısını al
        public int GetPortConnectionCount(PortPresenter port)
        {
            return port.ConnectionPresenters.Count;
        }
        
        // Bir portu tamamen temizle
        public void ClearPortConnections(PortPresenter port)
        {
            List<ConnectionPresenter> connectionsToRemove = new List<ConnectionPresenter>(port.ConnectionPresenters);
            
            foreach (var connection in connectionsToRemove)
            {
                graphManager.RemoveConnection(connection);
            }
        }
        
        // Port'un bir ActionNode'a bağlı olup olmadığını kontrol et
        private bool IsPortConnectedToActionNode(PortPresenter portPresenter)
        {
            if (portPresenter == null)
                return false;
                
            // Port'un bağlı olduğu node'u bul
            Transform currentTransform = portPresenter.transform;
            while (currentTransform != null)
            {
                // Node bir ActionNodePresenter mi?
                if (currentTransform.GetComponent<ActionNodePresenter>() != null)
                {
                    return true;
                }
                
                currentTransform = currentTransform.parent;
            }
            
            return false;
        }
    }
} 