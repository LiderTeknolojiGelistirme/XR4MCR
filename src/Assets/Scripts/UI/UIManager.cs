using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Senaryo İlerleme Bilgileri")]
        [SerializeField] private TextMeshProUGUI _nodeIndexText; // Örn: "2/5" veya "Katman: 2/5"
        [SerializeField] private TextMeshProUGUI _nodeTitleText; // Node başlığı
        [SerializeField] private TextMeshProUGUI _nodeDescriptionText; // Node açıklaması
        
        private void Start()
        {
            // Başlangıçta bilgileri temizle
            ClearScenarioInfo();
        }
        
        // Senaryo ilerlemesini günceller
        public void UpdateScenarioProgress(string nodeTitle, int currentLayer, int totalLayers, string description)
        {
            Debug.Log($"UIManager: Node bilgileri güncelleniyor - Title: {nodeTitle}, Layer: {currentLayer}/{totalLayers}");
            
            if (_nodeIndexText != null)
            {
                _nodeIndexText.text = $"Katman: {currentLayer}/{totalLayers}";
            }
            
            if (_nodeTitleText != null)
            {
                _nodeTitleText.text = nodeTitle;
            }
            
            if (_nodeDescriptionText != null)
            {
                _nodeDescriptionText.text = description;
            }
        }
        
        // Senaryo bilgilerini temizler
        public void ClearScenarioInfo()
        {
            Debug.Log("UIManager: Senaryo bilgileri temizleniyor");
            
            if (_nodeIndexText != null) _nodeIndexText.text = "Katman: -/-";
            if (_nodeTitleText != null) _nodeTitleText.text = "---";
            if (_nodeDescriptionText != null) _nodeDescriptionText.text = "---";
        }
    }
} 