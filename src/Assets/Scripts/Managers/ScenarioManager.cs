using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Presenters;
using Presenters.NodePresenters;
using Models;
using System.Linq;
using TMPro;
using Zenject;
using NodeSystem;

namespace Managers
{
    public class ScenarioManager : MonoBehaviour
    {
        // Aktif node'u bilmeli -  butun node'lari bilmeli - Skip, Next gibi metodlar burada olmali - 
        [Inject] private GraphManager _graphManager;
        [Inject] private UIManager _uiManager;
        [Inject] private NodeConfig _nodeConfig;
        [Inject] private DiContainer _container;

        public GameObject achievementCanvas {  get; private set; }

        public BaseNodePresenter ActiveNodePresenter { get; set; }
        public List<BaseNodePresenter> NodePresenters { get; set; }
        
        // Node seviyelerini takip etme
        private Dictionary<BaseNodePresenter, int> _nodeLayers = new Dictionary<BaseNodePresenter, int>();

        [Inject]
        public void Construct(GraphManager graphManager, UIManager uiManager, NodeConfig nodeConfig, DiContainer container)
        {
            _graphManager = graphManager;
            _uiManager = uiManager;
            _nodeConfig = nodeConfig;
            _container = container;

            Debug.Log("ScenarioManager: Constructor çağrıldı!");
            Debug.Log($"ScenarioManager: GraphManager inject edildi: {_graphManager != null}");
            Debug.Log($"ScenarioManager: UIManager inject edildi: {_uiManager != null}");
            Debug.Log($"ScenarioManager: NodeConfig inject edildi: {_nodeConfig != null}");

            if (_nodeConfig != null)
            {
                Debug.Log($"ScenarioManager: NodeConfig özellikleri - DefaultNodeColor: {_nodeConfig.defaultNodeColor}, ConnectionColor: {_nodeConfig.connectionColor}");
            }
            else
            {
                Debug.LogError("ScenarioManager: NodeConfig NULL! Inject edilemedi!");
            }

            achievementCanvas = _container.InstantiatePrefab(_nodeConfig.achievementCanvas);
        }

        private void Start()
        {
            Debug.Log("ScenarioManager: Start fonksiyonu çağrıldı");
            // Node seviyelerini hesapla
            CalculateNodeLayers();
        }

        //[Inject]
        //public void Construct(NodeConfig config)
        //{
        //    _nodeConfig = config;
        //    achievementCanvas = _container.InstantiatePrefab(_nodeConfig.achievementCanvas);
        //}

        public void StartScenario()
        {
            Debug.Log("ScenarioManager: Senaryo başlatıldı!");
            
            // Tüm nodeların durumunu sıfırla
            ResetAllNodeStates();
            
            // Node seviyelerini hesapla
            CalculateNodeLayers();
            
            // Başlangıç node'unu aktif yap
            _graphManager.StartNode.StartNode();
            ActiveNodePresenter = _graphManager.StartNode;
            Debug.Log($"ScenarioManager: Başlangıç node'u aktif yapıldı: {ActiveNodePresenter.Model.Title}");
            
            // Node bilgilerini göster
            UpdateNodeInfoDisplay();
        }
        
        /// <summary>
        /// Tüm nodeların durumunu temizler ve senaryoyu yeniden başlatmaya hazır hale getirir
        /// </summary>
        public void ResetAllNodeStates()
        {
            Debug.Log("ScenarioManager: Tüm node durumları sıfırlanıyor...");
            
            // GraphManager'dan tüm nodeları al
            var allNodes = _graphManager.NodePresenters;
            
            foreach (var node in allNodes)
            {
                if (node != null && node.Model != null)
                {
                    // Node'un durumunu temizle
                    node.Model.IsCompleted = false;
                    node.Model.IsActive = false;
                    node.Model.IsStarted = false;
                    
                    // Node'u deaktive et (event'leri tetikler)
                    node.DeactivateNode();
                }
            }
            
            Debug.Log($"ScenarioManager: Toplam {allNodes.Count} node sıfırlandı.");
        }

        public void FinishScenario()
        {
            Debug.Log("ScenarioManager: Senaryo tamamlandı!");
            
            // UI bilgilerini temizle
            _uiManager.ClearScenarioInfo();
        }

        public void GoNextNode()
        {
            string currentNodeTitle = ActiveNodePresenter?.Model.Title ?? "Bilinmeyen";
            Debug.Log($"ScenarioManager: Bir sonraki node'a geçilecek. Şu anki node: {currentNodeTitle}");
            
            ActiveNodePresenter.GoToNextNode();
            
            string nextNodeTitle = ActiveNodePresenter?.Model.Title ?? "Bilinmeyen"; 
            Debug.Log($"ScenarioManager: Bir sonraki node'a geçildi: {nextNodeTitle}");
            
            UpdateNodeInfoDisplay();

        }

        public void GoPreviousNode()
        {
            string currentNodeTitle = ActiveNodePresenter?.Model.Title ?? "Bilinmeyen";
            Debug.Log($"ScenarioManager: Bir önceki node'a geçilecek. Şu anki node: {currentNodeTitle}");
            
            ActiveNodePresenter.GoToPreviousNode();
            
            string prevNodeTitle = ActiveNodePresenter?.Model.Title ?? "Bilinmeyen";
            Debug.Log($"ScenarioManager: Bir önceki node'a geçildi: {prevNodeTitle}");
            
            UpdateNodeInfoDisplay();
        }
        
        // Node seviyelerini hesaplama
        public void CalculateNodeLayers()
        {
            Debug.Log("ScenarioManager: Node katmanları hesaplanmaya başlandı");
            _nodeLayers.Clear();
            
            // Başlangıç noktası olarak ilk node'u bul (Start ve Finish node'lar hariç)
            BaseNodePresenter firstNode = null;
            
            // Start node'dan çıkan bağlantıları bul
            if (_graphManager.StartNode != null)
            {
                Debug.Log($"ScenarioManager: Başlangıç node'u katman hesaplamasına dahil edilmeyecek: {_graphManager.StartNode.Model.Title}");
                
                // Start node'un çıkış portlarını bul
                var outputPorts = _graphManager.StartNode.Ports.Where(p => p.Polarity == NodeSystem.PolarityType.Output).ToList();
                
                // İlk bağlantı noktasını bul
                foreach (var outputPort in outputPorts)
                {
                    var connections = _graphManager.ConnectionPresenters.Where(c => c.Model.SourcePort == outputPort).ToList();
                    if (connections.Count > 0 && connections[0].Model.TargetPort != null)
                    {
                        firstNode = connections[0].Model.TargetPort.Model.baseNode;
                        break;
                    }
                }
            }
            
            if (_graphManager.FinishNode != null)
            {
                Debug.Log($"ScenarioManager: Bitiş node'u katman hesaplamasına dahil edilmeyecek: {_graphManager.FinishNode.Model.Title}");
            }
            
            // Eğer başlangıç node'undan sonraki node bulunamazsa, tüm node'ları katman 0 olarak işaretle
            if (firstNode == null)
            {
                Debug.LogWarning("ScenarioManager: Başlangıç node'undan sonraki node bulunamadı. Tüm node'lar katman 0 olarak işaretlenecek.");
                foreach (var node in _graphManager.NodePresenters)
                {
                    // Start ve Finish node'ları hariç
                    if (node != _graphManager.StartNode && node != _graphManager.FinishNode)
                    {
                        _nodeLayers[node] = 0;
                    }
                }
                return;
            }
            
            // İlk node 1. katmanda başlasın
            _nodeLayers[firstNode] = 1;
            Debug.Log($"ScenarioManager: İlk node (Katman 1): {firstNode.Model.Title}");
            
            // BFS algoritması ile node seviyeleri hesaplanır
            Queue<BaseNodePresenter> queue = new Queue<BaseNodePresenter>();
            queue.Enqueue(firstNode);
            
            while (queue.Count > 0)
            {
                BaseNodePresenter currentNode = queue.Dequeue();
                int currentLayer = _nodeLayers[currentNode];
                
                // Çıkış portlarını bul
                var outputPorts = currentNode.Ports.Where(p => p.Polarity == NodeSystem.PolarityType.Output).ToList();
                Debug.Log($"ScenarioManager: '{currentNode.Model.Title}' node'unun {outputPorts.Count} çıkış portu bulundu");
                
                foreach (var outputPort in outputPorts)
                {
                    // Bu porttan çıkan bağlantıları bul
                    var connections = _graphManager.ConnectionPresenters.Where(c => c.Model.SourcePort == outputPort).ToList();
                    
                    foreach (var connection in connections)
                    {
                        // Bağlantının hedef node'unu bul
                        var targetPort = connection.Model.TargetPort;
                        if (targetPort != null)
                        {
                            var targetNode = targetPort.Model.baseNode;
                            
                            // Finish node'u atla
                            if (targetNode == _graphManager.FinishNode)
                            {
                                continue;
                            }
                            
                            // Hedef node daha önce işlenmemişse veya daha yüksek bir seviyeye sahipse güncelle
                            if (!_nodeLayers.ContainsKey(targetNode) || _nodeLayers[targetNode] > currentLayer + 1)
                            {
                                _nodeLayers[targetNode] = currentLayer + 1;
                                queue.Enqueue(targetNode);
                                Debug.Log($"ScenarioManager: '{targetNode.Model.Title}' node'u {currentLayer + 1}. katmana yerleştirildi");
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"ScenarioManager: Toplam {_nodeLayers.Count} node katmanlandırıldı.");
            
            // Tüm node'ların katman bilgilerini yazdır (detaylı debug için)
            foreach (var nodeLayer in _nodeLayers)
            {
                Debug.Log($"ScenarioManager: Node: '{nodeLayer.Key.Model.Title}' - Katman: {nodeLayer.Value}");
            }
        }
        
        // Node bilgilerini güncelleme
        public void UpdateNodeInfoDisplay()
        {
            if (ActiveNodePresenter != null)
            {
                int nodeLayer = 0;
                
                // Start ve Finish node'lar için özel durum
                if (ActiveNodePresenter == _graphManager.StartNode)
                {
                    nodeLayer = 0; // Başlangıç node'u için katman 0 göster
                    Debug.Log($"ScenarioManager: Başlangıç node'u için katman 0 gösteriliyor");
                }
                else if (ActiveNodePresenter == _graphManager.FinishNode)
                {
                    // Finish node için, maksimum katman + 1 değerini göster (son katman)
                    int maxLayer = _nodeLayers.Count > 0 ? _nodeLayers.Values.Max() : 0;
                    nodeLayer = maxLayer + 1;
                    Debug.Log($"ScenarioManager: Bitiş node'u için katman {nodeLayer} gösteriliyor");
                }
                else if (_nodeLayers.ContainsKey(ActiveNodePresenter))
                {
                    nodeLayer = _nodeLayers[ActiveNodePresenter];
                }
                else
                {
                    Debug.LogWarning($"ScenarioManager: '{ActiveNodePresenter.Model.Title}' node'u için katman bilgisi bulunamadı!");
                }
                
                string nodeTitle = ActiveNodePresenter.Model.Title;
                string nodeDescription = ActiveNodePresenter.Model.Description;
                
                Debug.Log($"ScenarioManager: Node bilgileri güncelleniyor - Node: '{nodeTitle}', Katman: {nodeLayer}");
                
                // UIManager üzerinden bilgileri güncelle
                int totalNodes = _nodeLayers.Count;
                _uiManager.UpdateScenarioProgress(nodeTitle, nodeLayer, totalNodes, nodeDescription);
            }
            else
            {
                Debug.LogWarning("ScenarioManager: Node bilgileri güncellenemedi - aktif node yok");
                _uiManager.ClearScenarioInfo();
            }
        }
        
        // Aktif node bilgisini alma
        public (string title, string description, int layer) GetCurrentNodeInfo()
        {
            if (ActiveNodePresenter != null)
            {
                int layer = -1;
                
                // Start ve Finish node'lar için özel durum
                if (ActiveNodePresenter == _graphManager.StartNode)
                {
                    layer = 0;
                }
                else if (ActiveNodePresenter == _graphManager.FinishNode)
                {
                    int maxLayer = _nodeLayers.Count > 0 ? _nodeLayers.Values.Max() : 0;
                    layer = maxLayer + 1;
                }
                else if (_nodeLayers.ContainsKey(ActiveNodePresenter))
                {
                    layer = _nodeLayers[ActiveNodePresenter];
                }
                
                Debug.Log($"ScenarioManager: GetCurrentNodeInfo çağrıldı - Node: '{ActiveNodePresenter.Model.Title}', Katman: {layer}");
                return (ActiveNodePresenter.Model.Title, ActiveNodePresenter.Model.Description, layer);
            }
            
            Debug.LogWarning("ScenarioManager: GetCurrentNodeInfo çağrıldı ancak aktif node yok!");
            return ("", "", -1);
        }
    }
}