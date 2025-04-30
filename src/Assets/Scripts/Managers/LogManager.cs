using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace Managers
{
    public class LogManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _outputText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _maxLogCount = 100;
        [SerializeField] private bool _autoScroll = true;
        
        private List<string> _logMessages = new List<string>();
        
        // Singleton pattern for static access
        private static LogManager _instance;
        
        [Inject]
        private void Construct()
        {
            _instance = this;
            Debug.Log("LogManager initialized");
        }
        
        #region Statik Loglama Metodları
        
        /// <summary>
        /// Standart log mesajı
        /// </summary>
        /// <param name="message">Gösterilecek mesaj</param>
        public static void Log(string message)
        {
            // Hem Unity konsoluna hem bilgi paneline yazdır
            Debug.Log(message);
            
            if (_instance != null)
            {
                _instance.AddLog(message);
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Message: " + message);
            }
        }
        
        /// <summary>
        /// Renkli log mesajı
        /// </summary>
        /// <param name="message">Gösterilecek mesaj</param>
        /// <param name="color">Mesaj rengi</param>
        public static void Log(string message, Color color)
        {
            Debug.Log(message);
            
            if (_instance != null)
            {
                _instance.AddLog(message, color);
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Message: " + message);
            }
        }
        
        /// <summary>
        /// Hata mesajı
        /// </summary>
        /// <param name="message">Gösterilecek hata mesajı</param>
        public static void LogError(string message)
        {
            Debug.LogError(message);
            
            if (_instance != null)
            {
                _instance.AddLog(message, Color.red);
            }
            else
            {
                Debug.LogError("LogManager is not available. Error: " + message);
            }
        }
        
        /// <summary>
        /// Uyarı mesajı
        /// </summary>
        /// <param name="message">Gösterilecek uyarı mesajı</param>
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
            
            if (_instance != null)
            {
                _instance.AddLog(message, new Color(1f, 0.7f, 0f)); // Turuncu
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Warning: " + message);
            }
        }
        
        /// <summary>
        /// Başarı mesajı
        /// </summary>
        /// <param name="message">Gösterilecek başarı mesajı</param>
        public static void LogSuccess(string message)
        {
            Debug.Log(message);
            
            if (_instance != null)
            {
                _instance.AddLog(message, Color.green);
            }
            else
            {
                Debug.Log("LogManager is not available. Success: " + message);
            }
        }
        
        /// <summary>
        /// Senaryo olayı log mesajı - mavi renkle gösterilir
        /// </summary>
        /// <param name="message">Gösterilecek senaryo mesajı</param>
        public static void LogScenario(string message)
        {
            Debug.Log("[Senaryo] " + message);
            
            if (_instance != null)
            {
                _instance.AddLog("[Senaryo] " + message, new Color(0.5f, 0.7f, 1f)); // Açık mavi
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Scenario message: " + message);
            }
        }
        
        /// <summary>
        /// Etkileşim log mesajı - mor renkle gösterilir
        /// </summary>
        /// <param name="message">Gösterilecek etkileşim mesajı</param>
        public static void LogInteraction(string message)
        {
            Debug.Log("[Etkileşim] " + message);
            
            if (_instance != null)
            {
                _instance.AddLog("[Etkileşim] " + message, new Color(0.8f, 0.5f, 1f)); // Mor
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Interaction message: " + message);
            }
        }
        
        #endregion
        
        #region İç Loglama Metodları
        
        // Instance method to add a log entry
        private void AddLog(string message)
        {
            AddLog(message, Color.white);
        }
        
        // Instance method to add a colored log entry
        private void AddLog(string message, Color color)
        {
            // Zaman damgası ekle
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = $"[{timestamp}] {message}";
            
            // Log listesine ekle
            _logMessages.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{formattedMessage}</color>");
            
            // Log sayısı maximumu aşarsa eskisini kaldır
            if (_logMessages.Count > _maxLogCount)
            {
                _logMessages.RemoveAt(0);
            }
            
            // Metni güncelle
            UpdateLogText();
        }
        
        // Update the displayed text
        private void UpdateLogText()
        {
            if (_outputText != null)
            {
                _outputText.text = string.Join("\n", _logMessages);
                
                // Otomatik kaydırma
                if (_autoScroll && _scrollRect != null)
                {
                    // Bir frame sonra kaydırma yap (UI güncellemesi için)
                    Canvas.ForceUpdateCanvases();
                    _scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        // Clear all logs
        public void ClearLogs()
        {
            _logMessages.Clear();
            UpdateLogText();
        }
        
        #endregion
    }
} 