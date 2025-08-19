using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System.Collections;
using System.IO;

namespace Managers
{
    public class LogManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _outputText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _maxLogCount = 500; // 100'den 500'e çıkarıldı - daha çok log tutabilmek için
        [SerializeField] private bool _autoScroll = true;

        private List<string> _logMessages = new List<string>();
        private bool _needsScroll = false;
        
        // Auto scroll kontrolü için
        private bool _userHasScrolledUp = false;
        private float _scrollThreshold = 0.05f; // %5 tolerans - kullanıcı en alttan biraz uzaklaşınca auto scroll dursun
        
        // Txt dosyasına yazma kontrol
        public static bool enableFileLogging = false; // Default olarak kapalı
        
        // Log dosya adı
        private static readonly string LOG_FILE_NAME = "LTG_debug.log";
        
        // VIROO Debug için dosya yolu
        private static string _debugLogFilePath;
        private static bool _fileLoggingInitialized = false;

        // Singleton pattern for static access
        private static LogManager _instance;

        [Inject]
        private void Construct()
        {
            _instance = this;
            Debug.Log("LogManager initialized");
            
            // ScrollRect setup'ı kontrol et ve düzelt
            SetupScrollRect();
            
            // ScrollRect listener ekle
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }
            
            // Dosya sistemi hazırla ama yazmaya başlama
            InitializeFileSystem();
        }

        /// <summary>
        /// ScrollRect'in doğru kurulumunu kontrol eder ve gerekiyorsa düzeltir
        /// </summary>
        private void SetupScrollRect()
        {
            if (_scrollRect == null)
            {
                Debug.LogError("[LogManager] ScrollRect atanmamış!");
                return;
            }

            if (_outputText == null)
            {
                Debug.LogError("[LogManager] OutputText atanmamış!");
                return;
            }

            // Content objesini bul (genelde _outputText'in parent'ı olmalı)
            Transform contentTransform = _outputText.transform.parent;
            if (contentTransform == null)
            {
                Debug.LogError("[LogManager] OutputText'in parent'ı (Content) bulunamadı!");
                return;
            }

            // ContentSizeFitter kontrol et
            ContentSizeFitter sizeFitter = contentTransform.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
            }

            // Vertical boyutu text'e göre ayarla
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // ScrollRect'i content ile bağla (genelde otomatik olmalı)
            if (_scrollRect.content == null)
            {
                _scrollRect.content = contentTransform.GetComponent<RectTransform>();
            }
        }
        
        private void InitializeFileSystem()
        {
            try
            {
                // %appdata%/LocalLow/CompanyName/ProductName klasörüne log dosyası yolu hazırla
                string dataPath = Application.persistentDataPath;
                _debugLogFilePath = Path.Combine(dataPath, LOG_FILE_NAME);
                
                // Uygulama her açıldığında log dosyasını temizle
                if (File.Exists(_debugLogFilePath))
                {
                    File.Delete(_debugLogFilePath);
                    Debug.Log("LogManager: Previous log file cleared on startup");
                }
                
                _fileLoggingInitialized = true;
                
                Debug.Log($"File logging system ready. Path: {_debugLogFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"File logging system initialization failed: {e.Message}");
                _fileLoggingInitialized = false;
            }
        }
        
        private static void WriteToFile(string category, string message)
        {
            // Boolean kapalıysa hiçbir şey yapma
            if (!enableFileLogging || !_fileLoggingInitialized || string.IsNullOrEmpty(_debugLogFilePath)) 
                return;

            try
            {
                // İlk yazma ise dosyayı başlat
                if (!File.Exists(_debugLogFilePath))
                {
                    File.WriteAllText(_debugLogFilePath, $"=== LTG Debug Log Started: {DateTime.Now} ===\n");
                }
                
                string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] [{category}] {message}\n";
                File.AppendAllText(_debugLogFilePath, logEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"File logging write failed: {e.Message}");
            }
        }

        #region File Logging Control

        /// <summary>
        /// Txt dosyasına yazmayı etkinleştir
        /// </summary>
        public static void EnableFileLogging()
        {
            enableFileLogging = true;
            WriteToFile("SYSTEM", "File logging enabled");
            Debug.Log("LogManager: File logging enabled");
        }

        /// <summary>
        /// Txt dosyasına yazmayı devre dışı bırak
        /// </summary>
        public static void DisableFileLogging()
        {
            if (enableFileLogging)
                WriteToFile("SYSTEM", "File logging disabled");
            enableFileLogging = false;
            Debug.Log("LogManager: File logging disabled");
        }

        /// <summary>
        /// Txt dosyasını temizle
        /// </summary>
        public static void ClearLogFile()
        {
            if (!_fileLoggingInitialized || string.IsNullOrEmpty(_debugLogFilePath)) return;
            
            try
            {
                if (File.Exists(_debugLogFilePath))
                {
                    File.Delete(_debugLogFilePath);
                    Debug.Log("LogManager: Log file cleared");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Log file clear failed: {e.Message}");
            }
        }

        /// <summary>
        /// Log dosyasının yolunu döndür
        /// </summary>
        public static string GetLogFilePath() => _debugLogFilePath;
        
        /// <summary>
        /// Log dosyasının var olup olmadığını kontrol et
        /// </summary>
        public static bool LogFileExists() => !string.IsNullOrEmpty(_debugLogFilePath) && File.Exists(_debugLogFilePath);

        #endregion

        #region Statik Loglama Metodları

        /// <summary>
        /// Standart log mesajı
        /// </summary>
        /// <param name="message">Gösterilecek mesaj</param>
        public static void Log(string message)
        {
            // Hem Unity konsoluna hem bilgi paneline yazdır
            Debug.Log(message);
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("LOG", message);

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
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("LOG", message);

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
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("ERROR", message);

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
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("WARNING", message);

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
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("SUCCESS", message);

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
            Debug.Log("[Scenario] " + message);
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("SCENARIO", message);

            if (_instance != null)
            {
                _instance.AddLog("[Scenario] " + message, new Color(0.5f, 0.7f, 1f)); // Açık mavi
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
            Debug.Log("[Interaction] " + message);
            
            // Dosyaya da yaz (eğer enabled ise)
            WriteToFile("INTERACTION", message);

            if (_instance != null)
            {
                _instance.AddLog("[Interaction] " + message, new Color(0.8f, 0.5f, 1f)); // Mor
            }
            else
            {
                Debug.LogWarning("LogManager is not available. Interaction message: " + message);
            }
        }

        // DebugLogger uyumluluğu için ek metodlar
        public static void LogPointer(string message) => WriteToFile("POINTER", message);
        public static void LogRaycast(string message) => WriteToFile("RAYCAST", message);
        public static void LogInput(string message) => WriteToFile("INPUT", message);
        public static void LogGizmo(string message) => WriteToFile("GIZMO", message);
        public static void LogSystem(string message) => WriteToFile("SYSTEM", message);

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
            if (_outputText == null) return;

            // ❌ ESKİ KOD: Sadece 28 log gösteriyordu
            // int visibleCount = Mathf.Min(28, _logMessages.Count);
            // var visibleLogs = _logMessages.GetRange(_logMessages.Count - visibleCount, visibleCount);
            
            // ✅ YENİ KOD: Tüm logları göster
            // Tüm logları göster - ScrollRect bu şekilde düzgün çalışır
            _outputText.text = string.Join("\n", _logMessages);

            // ContentSizeFitter ile text boyutu otomatik ayarlanır
            // Bu sayede ScrollRect düzgün scroll yapabilir

            // Auto scroll sadece kullanıcı yukarı scroll yapmamışsa çalışsın
            if (_autoScroll && _scrollRect != null && !_userHasScrolledUp)
            {
                StartCoroutine(AutoScrollToBottom());
            }
        }

        private IEnumerator AutoScrollToBottom()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();

            if (_scrollRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                _scrollRect.verticalNormalizedPosition = 0f;

                if (_scrollRect.verticalScrollbar != null)
                {
                    _scrollRect.verticalScrollbar.value = 0f;
                }
            }
        }

        /// <summary>
        /// Kullanıcının manuel olarak en son loglara dönmesini sağlar
        /// </summary>
        public void ScrollToBottom()
        {
            if (_scrollRect != null)
            {
                _userHasScrolledUp = false; // Auto scroll'u yeniden aktifleştir
                StartCoroutine(AutoScrollToBottom());
            }
        }

        /// <summary>
        /// Static erişim için ScrollToBottom
        /// </summary>
        public static void ForceScrollToBottom()
        {
            if (_instance != null)
            {
                _instance.ScrollToBottom();
            }
        }

        private void OnDestroy()
        {
            // Scroll listener'ını temizle
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }

        // Kullanıcının manuel scroll yaptığını algılayan metod
        private void OnScrollValueChanged(Vector2 value)
        {
            // ScrollRect'te: 0f = en alt (yeni loglar), 1f = en üst (eski loglar)
            // Kullanıcı en alttan (0f) biraz uzaklaşmışsa auto scroll'u durdur
            if (value.y > _scrollThreshold)
            {
                if (!_userHasScrolledUp)
                {
                    _userHasScrolledUp = true;
                }
            }
            else
            {
                if (_userHasScrolledUp)
                {
                    _userHasScrolledUp = false;
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